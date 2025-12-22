/**
 * Smart Sticky Reviewer - Site Widget
 * Displays a sticky bar with product review information
 */

(function() {
    'use strict';

    // Configuration
    const CONFIG = {
        apiBaseUrl: 'http://localhost:5000/api',
        pollInterval: 0, // Set > 0 to enable polling
        animationDuration: 300,
        localStorageKey: 'smartStickyReviewer_closed'
    };

    // State
    let widgetState = {
        isVisible: false,
        isClosed: false,
        siteId: null,
        productId: null,
        style: null
    };

    // DOM Elements
    const widget = document.getElementById('smartStickyReviewer');
    const loader = document.getElementById('widgetLoader');
    const starsEl = widget.querySelector('.stars');
    const ratingValueEl = widget.querySelector('.rating-value');
    const ratingTextEl = widget.querySelector('.rating-text');
    const reviewCountEl = widget.querySelector('.review-count');

    /**
     * Initialize the widget
     */
    function init() {
        // Get configuration from page or data attributes
        widgetState.siteId = getConfigValue('siteId', 'demo-site-123');
        widgetState.productId = getConfigValue('productId', 'product-456');

        // Check if user closed the widget
        if (wasClosedByUser()) {
            return;
        }

        // Load review data
        loadReviewData();

        // Setup demo controls if present
        setupDemoControls();
    }

    /**
     * Get configuration value from various sources
     */
    function getConfigValue(key, defaultValue) {
        // Try data attribute on widget
        const dataValue = widget.dataset[key];
        if (dataValue) return dataValue;

        // Try global config object
        if (window.SmartStickyReviewerConfig && window.SmartStickyReviewerConfig[key]) {
            return window.SmartStickyReviewerConfig[key];
        }

        // Try URL parameters (for demo)
        const urlParams = new URLSearchParams(window.location.search);
        const urlValue = urlParams.get(key);
        if (urlValue) return urlValue;

        // Try demo input fields
        const inputEl = document.getElementById('demo' + key.charAt(0).toUpperCase() + key.slice(1));
        if (inputEl) return inputEl.value;

        return defaultValue;
    }

    /**
     * Check if widget was closed by user
     */
    function wasClosedByUser() {
        try {
            const closedData = localStorage.getItem(CONFIG.localStorageKey);
            if (!closedData) return false;

            const { productId, timestamp } = JSON.parse(closedData);
            
            // Reset after 24 hours
            const hoursSinceClosed = (Date.now() - timestamp) / (1000 * 60 * 60);
            if (hoursSinceClosed > 24) {
                localStorage.removeItem(CONFIG.localStorageKey);
                return false;
            }

            return productId === widgetState.productId;
        } catch {
            return false;
        }
    }

    /**
     * Load review data from API
     */
    async function loadReviewData() {
        showLoader();

        try {
            const url = `${CONFIG.apiBaseUrl}/reviews?siteId=${encodeURIComponent(widgetState.siteId)}&productId=${encodeURIComponent(widgetState.productId)}`;
            
            const response = await fetch(url);
            const data = await response.json();

            hideLoader();

            if (!data.isEnabled) {
                console.log('Smart Sticky Reviewer: Widget is disabled');
                return;
            }

            if (data.success) {
                displayReview(data);
            } else {
                console.warn('Smart Sticky Reviewer:', data.errorMessage);
                // Fail silently - don't show widget
            }
        } catch (error) {
            console.error('Smart Sticky Reviewer: Error loading data', error);
            hideLoader();
            // Fail silently - don't show widget
        }
    }

    /**
     * Display review data in widget
     */
    function displayReview(data) {
        // Apply styles
        applyStyles(data.style);

        // Set stars
        const fullStars = Math.floor(data.rating);
        const hasHalfStar = data.rating % 1 >= 0.5;
        let starsHtml = '';
        
        for (let i = 0; i < 5; i++) {
            if (i < fullStars) {
                starsHtml += '★';
            } else if (i === fullStars && hasHalfStar) {
                starsHtml += '★'; // Could use half-star character
            } else {
                starsHtml += '☆';
            }
        }
        starsEl.textContent = starsHtml;
        starsEl.style.display = data.style.showStars ? 'inline' : 'none';

        // Set rating value
        ratingValueEl.textContent = data.rating.toFixed(1);

        // Set rating text
        if (data.displayText) {
            ratingTextEl.textContent = data.displayText;
        } else {
            ratingTextEl.textContent = 'out of 5';
        }

        // Set review count
        if (data.style.showReviewCount && data.reviewCount > 0) {
            reviewCountEl.textContent = `(${formatNumber(data.reviewCount)} reviews)`;
            reviewCountEl.style.display = 'inline';
        } else {
            reviewCountEl.style.display = 'none';
        }

        // Add fallback indicator if applicable
        if (data.isFallback) {
            const indicator = document.createElement('span');
            indicator.className = 'fallback-indicator';
            indicator.textContent = 'fallback';
            ratingTextEl.appendChild(indicator);
        }

        // Show widget with animation
        showWidget(data.style.position);
    }

    /**
     * Apply custom styles from configuration
     */
    function applyStyles(style) {
        if (!style) return;

        widgetState.style = style;

        // Apply CSS custom properties
        document.documentElement.style.setProperty('--widget-bg', style.backgroundColor || '#ffffff');
        document.documentElement.style.setProperty('--widget-text', style.textColor || '#333333');
        document.documentElement.style.setProperty('--widget-star', style.starColor || '#ffc107');

        // Apply font size
        widget.querySelector('.reviewer-content').style.fontSize = `${style.fontSize || 14}px`;

        // Set position class
        widget.classList.remove('position-top', 'position-bottom');
        widget.classList.add(`position-${style.position || 'bottom'}`);
    }

    /**
     * Show the widget with animation
     */
    function showWidget(position) {
        widget.style.display = 'block';
        widget.classList.add('hidden');

        // Trigger reflow for animation
        widget.offsetHeight;

        // Remove hidden class to animate in
        requestAnimationFrame(() => {
            widget.classList.remove('hidden');
            widgetState.isVisible = true;
        });
    }

    /**
     * Hide the widget with animation
     */
    function hideWidget() {
        widget.classList.add('hidden');
        widgetState.isVisible = false;

        setTimeout(() => {
            widget.style.display = 'none';
        }, CONFIG.animationDuration);
    }

    /**
     * Show loading indicator
     */
    function showLoader() {
        loader.style.display = 'flex';
    }

    /**
     * Hide loading indicator
     */
    function hideLoader() {
        loader.style.display = 'none';
    }

    /**
     * Format number with K/M suffix
     */
    function formatNumber(num) {
        if (num >= 1000000) {
            return (num / 1000000).toFixed(1) + 'M';
        }
        if (num >= 1000) {
            return (num / 1000).toFixed(1) + 'K';
        }
        return num.toString();
    }

    /**
     * Setup demo controls
     */
    function setupDemoControls() {
        const refreshBtn = document.getElementById('refreshWidget');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => {
                // Update state from inputs
                const siteIdInput = document.getElementById('demoSiteId');
                const productIdInput = document.getElementById('demoProductId');

                if (siteIdInput) widgetState.siteId = siteIdInput.value;
                if (productIdInput) widgetState.productId = productIdInput.value;

                // Clear closed state
                localStorage.removeItem(CONFIG.localStorageKey);
                widgetState.isClosed = false;

                // Hide and reload
                hideWidget();
                setTimeout(loadReviewData, CONFIG.animationDuration);
            });
        }
    }

    /**
     * Close widget (called from close button)
     */
    window.closeWidget = function() {
        hideWidget();
        widgetState.isClosed = true;

        // Remember closure
        try {
            localStorage.setItem(CONFIG.localStorageKey, JSON.stringify({
                productId: widgetState.productId,
                timestamp: Date.now()
            }));
        } catch {
            // Ignore localStorage errors
        }
    };

    /**
     * Public API
     */
    window.SmartStickyReviewer = {
        init: init,
        refresh: loadReviewData,
        show: () => showWidget(widgetState.style?.position || 'bottom'),
        hide: hideWidget,
        setConfig: (config) => {
            if (config.siteId) widgetState.siteId = config.siteId;
            if (config.productId) widgetState.productId = config.productId;
        }
    };

    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
