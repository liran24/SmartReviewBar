/**
 * Smart Sticky Reviewer - Admin Widget
 * Configuration dashboard for managing review display settings
 */

// API Configuration
// By default, assume the admin dashboard is served by the same host as the API,
// and the API is available at `${origin}/api`.
//
// You can override this for local dev or separate hosting by adding `?apiBaseUrl=...`
// e.g. `/widgets/admin-widget/index.html?apiBaseUrl=https://your-domain.com/api`
const API_BASE_URL = (() => {
    try {
        const params = new URLSearchParams(window.location.search);
        const fromQuery = params.get('apiBaseUrl');
        if (fromQuery) return fromQuery.replace(/\/+$/, '');

        // Optional global override
        const fromGlobal = window.SmartStickyReviewerAdminConfig?.apiBaseUrl;
        if (fromGlobal) return String(fromGlobal).replace(/\/+$/, '');

        return `${window.location.origin}/api`;
    } catch {
        return 'http://localhost:5000/api';
    }
})();

// DOM Elements
const elements = {
    // Site Configuration
    siteId: document.getElementById('siteId'),
    plan: document.getElementById('plan'),
    planFeatures: document.getElementById('planFeatures'),
    primaryProvider: document.getElementById('primaryProvider'),
    isEnabled: document.getElementById('isEnabled'),

    // Fallback Settings
    useManualFallback: document.getElementById('useManualFallback'),
    manualRating: document.getElementById('manualRating'),
    manualRatingGroup: document.getElementById('manualRatingGroup'),
    manualReviewCount: document.getElementById('manualReviewCount'),
    manualCountGroup: document.getElementById('manualCountGroup'),
    fallbackText: document.getElementById('fallbackText'),
    fallbackTextGroup: document.getElementById('fallbackTextGroup'),
    notifyOnFailure: document.getElementById('notifyOnFailure'),
    notifyGroup: document.getElementById('notifyGroup'),
    notificationEmail: document.getElementById('notificationEmail'),
    emailGroup: document.getElementById('emailGroup'),

    // Styling
    bgColor: document.getElementById('bgColor'),
    textColor: document.getElementById('textColor'),
    starColor: document.getElementById('starColor'),
    position: document.getElementById('position'),
    fontSize: document.getElementById('fontSize'),
    showReviewCount: document.getElementById('showReviewCount'),
    showStars: document.getElementById('showStars'),
    advancedStyling: document.getElementById('advancedStyling'),

    // Manual Reviews
    productId: document.getElementById('productId'),
    productRating: document.getElementById('productRating'),
    productReviewCount: document.getElementById('productReviewCount'),
    productDisplayText: document.getElementById('productDisplayText'),
    saveManualReview: document.getElementById('saveManualReview'),

    // Preview
    widgetPreview: document.getElementById('widgetPreview'),

    // Actions
    loadConfig: document.getElementById('loadConfig'),
    saveConfig: document.getElementById('saveConfig'),
    statusMessage: document.getElementById('statusMessage')
};

// Feature Configuration
const featuresByPlan = {
    0: [], // Free
    1: ['MultipleReviewProviders', 'ManualFallbackText'], // Pro
    2: ['MultipleReviewProviders', 'ManualFallbackText', 'EmailNotificationOnFailure', 'AdvancedStyling'] // Premium
};

const allFeatures = [
    { name: 'MultipleReviewProviders', label: 'Multiple Review Providers', minPlan: 'Pro' },
    { name: 'ManualFallbackText', label: 'Manual Fallback Text', minPlan: 'Pro' },
    { name: 'EmailNotificationOnFailure', label: 'Email Notifications', minPlan: 'Premium' },
    { name: 'AdvancedStyling', label: 'Advanced Styling', minPlan: 'Premium' }
];

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    setupEventListeners();
    updatePlanFeatures();
    updatePreview();
});

/**
 * Setup Event Listeners
 */
function setupEventListeners() {
    // Plan change
    elements.plan.addEventListener('change', () => {
        updatePlanFeatures();
        updateFeatureAccess();
    });

    // Manual fallback toggle
    elements.useManualFallback.addEventListener('change', toggleManualFallbackFields);

    // Notification toggle
    elements.notifyOnFailure.addEventListener('change', toggleNotificationFields);

    // Preview updates
    ['bgColor', 'textColor', 'starColor', 'fontSize', 'position', 'showReviewCount', 'showStars'].forEach(id => {
        elements[id].addEventListener('change', updatePreview);
    });

    // Action buttons
    elements.loadConfig.addEventListener('click', loadConfiguration);
    elements.saveConfig.addEventListener('click', saveConfiguration);
    elements.saveManualReview.addEventListener('click', saveManualReview);
}

/**
 * Update Plan Features Display
 */
function updatePlanFeatures() {
    const plan = parseInt(elements.plan.value);
    const enabledFeatures = featuresByPlan[plan] || [];

    let html = '<div class="feature-list-items">';
    
    allFeatures.forEach(feature => {
        const isEnabled = enabledFeatures.includes(feature.name);
        const className = isEnabled ? 'enabled' : 'disabled';
        const icon = isEnabled ? '✓' : '✗';
        
        html += `
            <div class="feature-item ${className}">
                <span>${icon}</span>
                <span>${feature.label}</span>
                ${!isEnabled ? `<span class="badge">${feature.minPlan}</span>` : ''}
            </div>
        `;
    });

    html += '</div>';
    elements.planFeatures.innerHTML = html;
}

/**
 * Update Feature Access Based on Plan
 */
function updateFeatureAccess() {
    const plan = parseInt(elements.plan.value);
    const enabledFeatures = featuresByPlan[plan] || [];

    // Fallback Text (Pro+)
    const hasFallbackText = enabledFeatures.includes('ManualFallbackText');
    elements.fallbackText.disabled = !hasFallbackText;
    elements.fallbackTextGroup.classList.toggle('disabled-feature', !hasFallbackText);

    // Email Notification (Premium)
    const hasNotification = enabledFeatures.includes('EmailNotificationOnFailure');
    elements.notifyOnFailure.disabled = !hasNotification;
    elements.notifyGroup.classList.toggle('disabled-feature', !hasNotification);

    // Advanced Styling (Premium)
    const hasAdvancedStyling = enabledFeatures.includes('AdvancedStyling');
    elements.advancedStyling.disabled = !hasAdvancedStyling;
}

/**
 * Toggle Manual Fallback Fields
 */
function toggleManualFallbackFields() {
    const show = elements.useManualFallback.checked;
    elements.manualRatingGroup.style.display = show ? 'block' : 'none';
    elements.manualCountGroup.style.display = show ? 'block' : 'none';
}

/**
 * Toggle Notification Fields
 */
function toggleNotificationFields() {
    const show = elements.notifyOnFailure.checked;
    elements.emailGroup.style.display = show ? 'block' : 'none';
}

/**
 * Update Widget Preview
 */
function updatePreview() {
    const previewBar = elements.widgetPreview.querySelector('.preview-bar');
    
    previewBar.style.backgroundColor = elements.bgColor.value;
    previewBar.style.color = elements.textColor.value;
    previewBar.style.fontSize = `${elements.fontSize.value}px`;

    const stars = previewBar.querySelector('.stars');
    stars.style.color = elements.starColor.value;
    stars.style.display = elements.showStars.checked ? 'inline' : 'none';

    const count = previewBar.querySelector('.count');
    count.style.display = elements.showReviewCount.checked ? 'inline' : 'none';
}

/**
 * Load Configuration from Server
 */
async function loadConfiguration() {
    const siteId = elements.siteId.value.trim();
    
    if (!siteId) {
        showStatus('Please enter a Site ID', 'error');
        return;
    }

    try {
        showStatus('Loading configuration...', 'info');
        
        const response = await fetch(`${API_BASE_URL}/configuration/${siteId}`);
        const data = await response.json();

        if (!data.found) {
            showStatus('No configuration found for this site. Create a new one!', 'info');
            return;
        }

        // Populate form
        elements.plan.value = data.plan || 0;
        elements.primaryProvider.value = data.primaryProvider || 0;
        elements.isEnabled.checked = data.isEnabled !== false;

        // Fallback config
        if (data.fallbackConfig) {
            elements.useManualFallback.checked = data.fallbackConfig.useManualRatingFallback || false;
            elements.manualRating.value = data.fallbackConfig.manualRating || 4.5;
            elements.manualReviewCount.value = data.fallbackConfig.manualReviewCount || 100;
            elements.fallbackText.value = data.fallbackConfig.fallbackText || '';
            elements.notifyOnFailure.checked = data.fallbackConfig.notifyOnFailure || false;
            elements.notificationEmail.value = data.fallbackConfig.notificationEmail || '';
        }

        // Style config
        if (data.style) {
            elements.bgColor.value = data.style.backgroundColor || '#ffffff';
            elements.textColor.value = data.style.textColor || '#333333';
            elements.starColor.value = data.style.starColor || '#ffc107';
            elements.position.value = data.style.position || 'bottom';
            elements.fontSize.value = data.style.fontSize || 14;
            elements.showReviewCount.checked = data.style.showReviewCount !== false;
            elements.showStars.checked = data.style.showStars !== false;
        }

        // Update UI
        toggleManualFallbackFields();
        toggleNotificationFields();
        updatePlanFeatures();
        updateFeatureAccess();
        updatePreview();

        showStatus('Configuration loaded successfully!', 'success');
    } catch (error) {
        console.error('Error loading configuration:', error);
        showStatus('Failed to load configuration. Check your connection.', 'error');
    }
}

/**
 * Save Configuration to Server
 */
async function saveConfiguration() {
    const siteId = elements.siteId.value.trim();
    
    if (!siteId) {
        showStatus('Please enter a Site ID', 'error');
        return;
    }

    const config = {
        siteId: siteId,
        plan: parseInt(elements.plan.value),
        primaryProvider: parseInt(elements.primaryProvider.value),
        isEnabled: elements.isEnabled.checked,
        fallbackConfig: {
            useManualRatingFallback: elements.useManualFallback.checked,
            manualRating: parseFloat(elements.manualRating.value) || null,
            manualReviewCount: parseInt(elements.manualReviewCount.value) || null,
            fallbackText: elements.fallbackText.value || null,
            notifyOnFailure: elements.notifyOnFailure.checked,
            notificationEmail: elements.notificationEmail.value || null
        },
        style: {
            backgroundColor: elements.bgColor.value,
            textColor: elements.textColor.value,
            starColor: elements.starColor.value,
            position: elements.position.value,
            fontSize: parseInt(elements.fontSize.value),
            showReviewCount: elements.showReviewCount.checked,
            showStars: elements.showStars.checked
        }
    };

    try {
        showStatus('Saving configuration...', 'info');

        const response = await fetch(`${API_BASE_URL}/configuration`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(config)
        });

        const data = await response.json();

        if (data.success) {
            const message = data.isNew 
                ? 'Configuration created successfully!' 
                : 'Configuration updated successfully!';
            showStatus(message, 'success');
        } else {
            showStatus(`Error: ${data.errorMessage}`, 'error');
        }
    } catch (error) {
        console.error('Error saving configuration:', error);
        showStatus('Failed to save configuration. Check your connection.', 'error');
    }
}

/**
 * Save Manual Review
 */
async function saveManualReview() {
    const siteId = elements.siteId.value.trim();
    const productId = elements.productId.value.trim();

    if (!siteId || !productId) {
        showStatus('Please enter both Site ID and Product ID', 'error');
        return;
    }

    const review = {
        siteId: siteId,
        productId: productId,
        rating: parseFloat(elements.productRating.value),
        reviewCount: parseInt(elements.productReviewCount.value),
        displayText: elements.productDisplayText.value
    };

    try {
        showStatus('Saving manual review...', 'info');

        const response = await fetch(`${API_BASE_URL}/reviews/manual`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(review)
        });

        const data = await response.json();

        if (data.success) {
            const message = data.isNew 
                ? 'Manual review created!' 
                : 'Manual review updated!';
            showStatus(message, 'success');
            
            // Clear fields
            elements.productId.value = '';
            elements.productDisplayText.value = '';
        } else {
            showStatus(`Error: ${data.errorMessage}`, 'error');
        }
    } catch (error) {
        console.error('Error saving manual review:', error);
        showStatus('Failed to save manual review. Check your connection.', 'error');
    }
}

/**
 * Show Status Message
 */
function showStatus(message, type) {
    elements.statusMessage.textContent = message;
    elements.statusMessage.className = `status-message ${type}`;
    elements.statusMessage.style.display = 'block';

    // Auto-hide after 5 seconds for success/info
    if (type !== 'error') {
        setTimeout(() => {
            elements.statusMessage.style.display = 'none';
        }, 5000);
    }
}
