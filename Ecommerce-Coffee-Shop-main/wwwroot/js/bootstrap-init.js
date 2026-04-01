/**
 * Bootstrap Component Initialization Script
 * 
 * Purpose:
 * This script initializes all Bootstrap 5.3.8 interactive components after the DOM loads.
 * It ensures that tooltips, modals, dropdowns, and other components are properly set up
 * and provides event handling for component lifecycle events.
 * 
 * Bootstrap Version: 5.3.8
 * 
 * Components Initialized:
 * - Tooltips: Hover information displays
 * - Popovers: Click-triggered information displays
 * - Toasts: Notification messages
 * - Modals: Dialog overlays
 * - Dropdowns: Menu components
 * - Alerts: Auto-dismissing alert messages
 * - Form Validation: HTML5 validation with Bootstrap styling
 * 
 * Compatibility:
 * - Works with jQuery if present (for legacy ASP.NET validation)
 * - Pure JavaScript implementation (no jQuery required)
 * - Compatible with ASP.NET MVC Tag Helpers
 * 
 * Usage:
 * Include this script after bootstrap.bundle.min.js in layout files.
 * Components will be automatically initialized on page load.
 */

(function() {
    'use strict';
    
    // ============================================
    // Bootstrap Availability Check
    // ============================================
    
    /**
     * Verify that Bootstrap JavaScript is loaded before proceeding.
     * If Bootstrap is not available, log an error and exit early to prevent
     * JavaScript errors from attempting to use undefined Bootstrap objects.
     */
    if (typeof bootstrap === 'undefined') {
        console.error('Bootstrap JavaScript is not loaded. Please ensure bootstrap.bundle.min.js is included.');
        return;
    }
    
    // ============================================
    // jQuery Compatibility Check
    // ============================================
    
    /**
     * Check if jQuery is present on the page.
     * jQuery is used by ASP.NET MVC's unobtrusive validation library.
     * Bootstrap 5 does not require jQuery, but they can coexist peacefully.
     */
    if (typeof jQuery !== 'undefined') {
        console.log('jQuery detected - Bootstrap and jQuery are compatible');
    }
    
    // ============================================
    // DOM Ready Event Listener
    // ============================================
    
    /**
     * Wait for the DOM to be fully loaded before initializing components.
     * This ensures all HTML elements are available for component initialization.
     */
    document.addEventListener('DOMContentLoaded', function() {
        
        // ============================================
        // Tooltip Initialization
        // ============================================
        
        /**
         * Initialize all Bootstrap tooltips on the page.
         * 
         * Tooltips provide contextual information on hover.
         * They must be explicitly initialized because they use Popper.js positioning.
         * 
         * Usage in HTML:
         * <button data-bs-toggle="tooltip" title="Tooltip text">Hover me</button>
         * 
         * Bootstrap API: https://getbootstrap.com/docs/5.3/components/tooltips/
         */
        var tooltipTriggerList = [].slice.call(
            document.querySelectorAll('[data-bs-toggle="tooltip"]')
        );
        tooltipTriggerList.map(function(tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
        
        // ============================================
        // Popover Initialization
        // ============================================
        
        /**
         * Initialize all Bootstrap popovers on the page.
         * 
         * Popovers are similar to tooltips but support more content and are
         * triggered by click instead of hover (by default).
         * 
         * Usage in HTML:
         * <button data-bs-toggle="popover" title="Title" data-bs-content="Content">Click me</button>
         * 
         * Bootstrap API: https://getbootstrap.com/docs/5.3/components/popovers/
         */
        var popoverTriggerList = [].slice.call(
            document.querySelectorAll('[data-bs-toggle="popover"]')
        );
        popoverTriggerList.map(function(popoverTriggerEl) {
            return new bootstrap.Popover(popoverTriggerEl);
        });
        
        // ============================================
        // Toast Initialization
        // ============================================
        
        /**
         * Initialize all Bootstrap toasts on the page.
         * 
         * Toasts are lightweight notifications that appear temporarily.
         * They must be initialized before they can be shown.
         * 
         * Usage in HTML:
         * <div class="toast" role="alert">...</div>
         * 
         * To show: toast.show()
         * 
         * Bootstrap API: https://getbootstrap.com/docs/5.3/components/toasts/
         */
        var toastElList = [].slice.call(document.querySelectorAll('.toast'));
        toastElList.map(function(toastEl) {
            return new bootstrap.Toast(toastEl);
        });
        
        // ============================================
        // Modal Initialization
        // ============================================
        
        /**
         * Initialize all Bootstrap modals on the page.
         * 
         * Modals are dialog overlays for important content or actions.
         * Pre-initialization ensures they're ready when triggered.
         * 
         * Usage in HTML:
         * <div class="modal" id="myModal">...</div>
         * <button data-bs-toggle="modal" data-bs-target="#myModal">Open</button>
         * 
         * Bootstrap API: https://getbootstrap.com/docs/5.3/components/modal/
         */
        var modalElList = [].slice.call(document.querySelectorAll('.modal'));
        modalElList.map(function(modalEl) {
            return new bootstrap.Modal(modalEl);
        });
        
        // ============================================
        // Dropdown Initialization
        // ============================================
        
        /**
         * Initialize all Bootstrap dropdowns on the page.
         * 
         * Dropdowns provide contextual menus and actions.
         * They use Popper.js for intelligent positioning.
         * 
         * Usage in HTML:
         * <button data-bs-toggle="dropdown">Menu</button>
         * <ul class="dropdown-menu">...</ul>
         * 
         * Bootstrap API: https://getbootstrap.com/docs/5.3/components/dropdowns/
         */
        var dropdownElList = [].slice.call(document.querySelectorAll('[data-bs-toggle="dropdown"]'));
        dropdownElList.map(function(dropdownToggleEl) {
            return new bootstrap.Dropdown(dropdownToggleEl);
        });
        
        // ============================================
        // Auto-Dismiss Alerts
        // ============================================
        
        /**
         * Automatically dismiss alert messages after 5 seconds.
         * 
         * This provides a better UX by removing success/info messages automatically
         * without requiring user interaction. Only affects dismissible alerts.
         * 
         * Usage in HTML:
         * <div class="alert alert-dismissible">...</div>
         * 
         * Bootstrap API: https://getbootstrap.com/docs/5.3/components/alerts/
         */
        var alertList = document.querySelectorAll('.alert-dismissible');
        alertList.forEach(function(alert) {
            setTimeout(function() {
                var bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }, 5000);  // 5 second delay
        });
        
        // ============================================
        // Form Validation
        // ============================================
        
        /**
         * Enable Bootstrap form validation styling.
         * 
         * This adds Bootstrap's validation classes (.is-valid, .is-invalid) to forms
         * with the .needs-validation class when they are submitted.
         * Works with HTML5 validation attributes (required, pattern, etc.)
         * 
         * Usage in HTML:
         * <form class="needs-validation" novalidate>...</form>
         * 
         * The novalidate attribute disables browser default validation UI,
         * allowing Bootstrap's styled validation to take over.
         * 
         * Bootstrap API: https://getbootstrap.com/docs/5.3/forms/validation/
         */
        var forms = document.querySelectorAll('.needs-validation');
        Array.prototype.slice.call(forms).forEach(function(form) {
            form.addEventListener('submit', function(event) {
                if (!form.checkValidity()) {
                    event.preventDefault();      // Prevent form submission
                    event.stopPropagation();     // Stop event bubbling
                }
                form.classList.add('was-validated');  // Add Bootstrap validation classes
            }, false);
        });
        
        console.log('Bootstrap components initialized successfully');
    });
    
    // ============================================
    // Bootstrap Modal Event Handlers
    // ============================================
    
    /**
     * Modal lifecycle event listeners.
     * 
     * Bootstrap modals fire events at different stages of their lifecycle:
     * - show.bs.modal: Fires immediately when show() is called
     * - shown.bs.modal: Fires when modal is fully visible (after transitions)
     * - hide.bs.modal: Fires immediately when hide() is called
     * - hidden.bs.modal: Fires when modal is fully hidden (after transitions)
     * 
     * These event listeners log modal activity for debugging.
     * You can add custom logic here (e.g., load data when modal opens).
     * 
     * Usage:
     * document.addEventListener('shown.bs.modal', function(event) {
     *     // Custom logic when any modal is shown
     *     console.log('Modal shown:', event.target.id);
     * });
     * 
     * Bootstrap API: https://getbootstrap.com/docs/5.3/components/modal/#events
     */
    
    document.addEventListener('show.bs.modal', function(event) {
        console.log('Modal showing:', event.target.id);
    });
    
    document.addEventListener('shown.bs.modal', function(event) {
        console.log('Modal shown:', event.target.id);
    });
    
    document.addEventListener('hide.bs.modal', function(event) {
        console.log('Modal hiding:', event.target.id);
    });
    
    document.addEventListener('hidden.bs.modal', function(event) {
        console.log('Modal hidden:', event.target.id);
    });
    
    // ============================================
    // Bootstrap Dropdown Event Handlers
    // ============================================
    
    /**
     * Dropdown lifecycle event listeners.
     * 
     * Bootstrap dropdowns fire events at different stages:
     * - show.bs.dropdown: Fires immediately when dropdown is opening
     * - shown.bs.dropdown: Fires when dropdown is fully visible
     * - hide.bs.dropdown: Fires immediately when dropdown is closing
     * - hidden.bs.dropdown: Fires when dropdown is fully hidden
     * 
     * These event listeners log dropdown activity for debugging.
     * You can add custom logic here (e.g., load menu items dynamically).
     * 
     * Bootstrap API: https://getbootstrap.com/docs/5.3/components/dropdowns/#events
     */
    
    document.addEventListener('show.bs.dropdown', function(event) {
        console.log('Dropdown showing:', event.target.id);
    });
    
    document.addEventListener('shown.bs.dropdown', function(event) {
        console.log('Dropdown shown:', event.target.id);
    });
    
    document.addEventListener('hide.bs.dropdown', function(event) {
        console.log('Dropdown hiding:', event.target.id);
    });
    
    document.addEventListener('hidden.bs.dropdown', function(event) {
        console.log('Dropdown hidden:', event.target.id);
    });
    
    // ============================================
    // Bootstrap Offcanvas Event Handlers
    // ============================================
    
    /**
     * Offcanvas lifecycle event listeners.
     * 
     * Bootstrap offcanvas components (slide-in panels) fire events:
     * - show.bs.offcanvas: Fires immediately when offcanvas is opening
     * - shown.bs.offcanvas: Fires when offcanvas is fully visible
     * - hide.bs.offcanvas: Fires immediately when offcanvas is closing
     * - hidden.bs.offcanvas: Fires when offcanvas is fully hidden
     * 
     * These event listeners log offcanvas activity for debugging.
     * Useful for cart drawers, mobile menus, and side panels.
     * 
     * Usage in SaleStore:
     * - Cart drawer in POS interface
     * - Mobile navigation menus
     * 
     * Bootstrap API: https://getbootstrap.com/docs/5.3/components/offcanvas/#events
     */
    
    document.addEventListener('show.bs.offcanvas', function(event) {
        console.log('Offcanvas showing:', event.target.id);
    });
    
    document.addEventListener('shown.bs.offcanvas', function(event) {
        console.log('Offcanvas shown:', event.target.id);
    });
    
    document.addEventListener('hide.bs.offcanvas', function(event) {
        console.log('Offcanvas hiding:', event.target.id);
    });
    
    document.addEventListener('hidden.bs.offcanvas', function(event) {
        console.log('Offcanvas hidden:', event.target.id);
    });
    
})();

/**
 * Additional Notes for Developers:
 * 
 * 1. Component Initialization:
 *    - All components are initialized automatically on page load
 *    - For dynamically added content, manually initialize components:
 *      var tooltip = new bootstrap.Tooltip(element);
 * 
 * 2. Component Methods:
 *    - Get instance: var modal = bootstrap.Modal.getInstance(element);
 *    - Get or create: var modal = bootstrap.Modal.getOrCreateInstance(element);
 *    - Show: modal.show();
 *    - Hide: modal.hide();
 *    - Dispose: modal.dispose();
 * 
 * 3. Event Handling:
 *    - All Bootstrap events bubble up the DOM
 *    - Use event.target to get the component element
 *    - Use event.relatedTarget for additional context (e.g., button that triggered modal)
 * 
 * 4. jQuery Compatibility:
 *    - Bootstrap 5 does not require jQuery
 *    - If jQuery is present, Bootstrap components work with both APIs:
 *      $('#myModal').modal('show');  // jQuery API
 *      bootstrap.Modal.getInstance(element).show();  // JavaScript API
 * 
 * 5. ASP.NET MVC Integration:
 *    - Works seamlessly with ASP.NET Tag Helpers
 *    - Compatible with jQuery Unobtrusive Validation
 *    - Form validation integrates with ASP.NET ModelState errors
 * 
 * 6. Performance:
 *    - Components are initialized once on page load
 *    - Event listeners use event delegation for efficiency
 *    - Dispose of components when removing elements to prevent memory leaks
 * 
 * 7. Debugging:
 *    - Check browser console for initialization messages
 *    - Event logs show component lifecycle for debugging
 *    - Use Bootstrap.Debug = true for additional logging (if available)
 */
