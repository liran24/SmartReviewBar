/**
 * Smart Sticky Reviewer - Embeddable Widget Script
 * Include this script on any website to display the review widget
 * 
 * Usage:
 * <script>
 *   window.SmartStickyReviewerConfig = {
 *     siteId: 'your-site-id',
 *     productId: 'your-product-id',
 *     apiUrl: 'https://your-api-domain.com/api'
 *   };
 * </script>
 * <script src="https://your-cdn.com/smart-sticky-reviewer/embed.js" async></script>
 */

(function() {
    'use strict';

    // Default configuration
    const DEFAULT_CONFIG = {
        apiUrl: 'http://localhost:5000/api',
        siteId: '',
        productId: '',
        position: 'bottom',
        zIndex: 9999
    };

    // Merge with user configuration
    const config = Object.assign({}, DEFAULT_CONFIG, window.SmartStickyReviewerConfig || {});

    // Validate required config
    if (!config.siteId || !config.productId) {
        console.warn('Smart Sticky Reviewer: siteId and productId are required');
        return;
    }

    // Widget HTML template
    const widgetHTML = `
        <div id="ssr-widget" class="ssr-widget ssr-hidden ssr-position-${config.position}" style="z-index: ${config.zIndex};">
            <div class="ssr-content">
                <span class="ssr-stars"></span>
                <span class="ssr-rating"></span>
                <span class="ssr-text"></span>
                <span class="ssr-count"></span>
                <button class="ssr-close" aria-label="Close">&times;</button>
            </div>
        </div>
    `;

    // Widget CSS
    const widgetCSS = `
        .ssr-widget {
            position: fixed;
            left: 0;
            right: 0;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            transition: transform 0.3s ease-out, opacity 0.3s ease-out;
        }
        .ssr-widget.ssr-position-bottom {
            bottom: 0;
            box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.15);
        }
        .ssr-widget.ssr-position-top {
            top: 0;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.15);
        }
        .ssr-widget.ssr-hidden {
            opacity: 0;
            pointer-events: none;
        }
        .ssr-widget.ssr-hidden.ssr-position-bottom {
            transform: translateY(100%);
        }
        .ssr-widget.ssr-hidden.ssr-position-top {
            transform: translateY(-100%);
        }
        .ssr-content {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            padding: 12px 50px 12px 20px;
            max-width: 1200px;
            margin: 0 auto;
            position: relative;
        }
        .ssr-stars {
            font-size: 1.2em;
            letter-spacing: 2px;
        }
        .ssr-rating {
            font-weight: bold;
            font-size: 1.1em;
        }
        .ssr-text {
            font-size: 0.95em;
        }
        .ssr-count {
            opacity: 0.7;
            font-size: 0.9em;
        }
        .ssr-close {
            position: absolute;
            right: 15px;
            top: 50%;
            transform: translateY(-50%);
            background: none;
            border: none;
            font-size: 1.5rem;
            opacity: 0.5;
            cursor: pointer;
            padding: 5px 10px;
            color: inherit;
        }
        .ssr-close:hover {
            opacity: 1;
        }
        @media (max-width: 640px) {
            .ssr-content {
                flex-wrap: wrap;
                gap: 8px;
                padding: 10px 40px 10px 15px;
            }
            .ssr-text {
                display: none;
            }
        }
    `;

    // Local storage key for remembering closed state
    const STORAGE_KEY = 'ssr_closed_' + config.productId;

    /**
     * Check if widget was closed by user
     */
    function wasClosedByUser() {
        try {
            const data = localStorage.getItem(STORAGE_KEY);
            if (!data) return false;
            
            const { timestamp } = JSON.parse(data);
            const hoursSinceClosed = (Date.now() - timestamp) / (1000 * 60 * 60);
            
            if (hoursSinceClosed > 24) {
                localStorage.removeItem(STORAGE_KEY);
                return false;
            }
            return true;
        } catch {
            return false;
        }
    }

    /**
     * Remember that user closed widget
     */
    function rememberClosed() {
        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify({ timestamp: Date.now() }));
        } catch {
            // Ignore storage errors
        }
    }

    /**
     * Generate star display
     */
    function generateStars(rating) {
        const fullStars = Math.floor(rating);
        let stars = '';
        for (let i = 0; i < 5; i++) {
            stars += i < fullStars ? '★' : '☆';
        }
        return stars;
    }

    /**
     * Format number with K/M suffix
     */
    function formatNumber(num) {
        if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
        if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
        return num.toString();
    }

    /**
     * Apply styles from API response
     */
    function applyStyles(widget, style) {
        if (!style) return;
        
        widget.style.backgroundColor = style.backgroundColor || '#ffffff';
        widget.style.color = style.textColor || '#333333';
        
        const stars = widget.querySelector('.ssr-stars');
        if (stars) {
            stars.style.color = style.starColor || '#ffc107';
            stars.style.display = style.showStars !== false ? 'inline' : 'none';
        }
        
        const content = widget.querySelector('.ssr-content');
        if (content) {
            content.style.fontSize = (style.fontSize || 14) + 'px';
        }

        const count = widget.querySelector('.ssr-count');
        if (count) {
            count.style.display = style.showReviewCount !== false ? 'inline' : 'none';
        }
        
        // Update position if different from initial
        if (style.position && style.position !== config.position) {
            widget.classList.remove('ssr-position-' + config.position);
            widget.classList.add('ssr-position-' + style.position);
        }
    }

    /**
     * Initialize widget
     */
    async function init() {
        // Check if already closed
        if (wasClosedByUser()) {
            return;
        }

        // Inject CSS
        const styleEl = document.createElement('style');
        styleEl.textContent = widgetCSS;
        document.head.appendChild(styleEl);

        // Inject HTML
        const container = document.createElement('div');
        container.innerHTML = widgetHTML;
        document.body.appendChild(container.firstElementChild);

        const widget = document.getElementById('ssr-widget');
        
        // Setup close button
        widget.querySelector('.ssr-close').addEventListener('click', () => {
            widget.classList.add('ssr-hidden');
            rememberClosed();
        });

        // Fetch review data
        try {
            const url = `${config.apiUrl}/reviews?siteId=${encodeURIComponent(config.siteId)}&productId=${encodeURIComponent(config.productId)}`;
            const response = await fetch(url);
            const data = await response.json();

            if (!data.isEnabled || !data.success) {
                widget.remove();
                return;
            }

            // Apply styles
            applyStyles(widget, data.style);

            // Set content
            widget.querySelector('.ssr-stars').textContent = generateStars(data.rating);
            widget.querySelector('.ssr-rating').textContent = data.rating.toFixed(1);
            widget.querySelector('.ssr-text').textContent = data.displayText || 'out of 5';
            
            if (data.reviewCount > 0 && data.style.showReviewCount !== false) {
                widget.querySelector('.ssr-count').textContent = `(${formatNumber(data.reviewCount)} reviews)`;
            }

            // Show widget
            requestAnimationFrame(() => {
                widget.classList.remove('ssr-hidden');
            });

        } catch (error) {
            console.error('Smart Sticky Reviewer: Error loading data', error);
            widget.remove();
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
