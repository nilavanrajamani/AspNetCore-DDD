// Notification system for real-time updates
class NotificationManager {
    constructor() {
        this.connection = null;
        this.initializeSignalR();
    }

    async initializeSignalR() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/eventNotification")
            .withAutomaticReconnect()
            .build();

        // Set up event handlers
        this.connection.on("EventCreated", (eventData) => {
            this.showNotification("success", "New Event Created", `Event "${eventData.title}" has been created`);
            this.refreshEventList();
        });

        this.connection.on("EventUpdated", (eventData) => {
            this.showNotification("info", "Event Updated", `Event "${eventData.title}" has been updated`);
            this.refreshEventList();
        });

        this.connection.on("EventDeleted", (eventData) => {
            this.showNotification("warning", "Event Deleted", `Event "${eventData.title}" has been deleted`);
            this.refreshEventList();
        });

        this.connection.on("EventStatusChanged", (eventData) => {
            this.showNotification("info", "Event Status Changed", 
                `Event "${eventData.title}" status changed to ${eventData.status}`);
            this.refreshEventList();
        });

        try {
            await this.connection.start();
            console.log("SignalR Connected");
        } catch (err) {
            console.error("SignalR Connection Error:", err);
            setTimeout(() => this.initializeSignalR(), 5000);
        }
    }

    showNotification(type, title, message, duration = 5000) {
        const toastContainer = document.getElementById('toastContainer');
        if (!toastContainer) return;

        const toastId = 'toast-' + Date.now();
        const iconClass = this.getIconClass(type);
        const bgClass = this.getBgClass(type);

        const toastHtml = `
            <div class="toast align-items-center text-white ${bgClass} border-0" role="alert" 
                 aria-live="assertive" aria-atomic="true" id="${toastId}">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="${iconClass} me-2"></i>
                        <strong>${title}</strong><br>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" 
                            data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { delay: duration });
        toast.show();

        // Remove toast element after it's hidden
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    }

    getIconClass(type) {
        switch (type) {
            case 'success': return 'fas fa-check-circle';
            case 'error': 
            case 'danger': return 'fas fa-exclamation-circle';
            case 'warning': return 'fas fa-exclamation-triangle';
            case 'info': return 'fas fa-info-circle';
            default: return 'fas fa-bell';
        }
    }

    getBgClass(type) {
        switch (type) {
            case 'success': return 'bg-success';
            case 'error': 
            case 'danger': return 'bg-danger';
            case 'warning': return 'bg-warning';
            case 'info': return 'bg-info';
            default: return 'bg-primary';
        }
    }

    refreshEventList() {
        // Refresh event list if we're on the events page
        if (window.location.pathname.includes('/Events/List') && typeof refreshEvents === 'function') {
            refreshEvents();
        }
    }

    async joinEventGroup(eventId) {
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke("JoinEventGroup", eventId);
        }
    }

    async leaveEventGroup(eventId) {
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke("LeaveEventGroup", eventId);
        }
    }

    async joinOrganizerGroup(organizerId) {
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke("JoinOrganizerGroup", organizerId);
        }
    }
}

// Global notification manager instance
window.notificationManager = new NotificationManager();

// Utility functions for UI feedback
let loadingTimeout;

window.showLoading = function(timeoutMs = 10000) {
    const spinner = document.getElementById('loadingSpinner');
    if (spinner) {
        spinner.style.display = 'flex';
        
        // Clear any existing timeout
        if (loadingTimeout) {
            clearTimeout(loadingTimeout);
        }
        
        // Set a timeout to auto-hide the spinner if it's left showing
        loadingTimeout = setTimeout(() => {
            console.warn('Loading spinner auto-hidden after timeout');
            window.hideLoading();
        }, timeoutMs);
    }
};

window.hideLoading = function() {
    const spinner = document.getElementById('loadingSpinner');
    if (spinner) {
        spinner.style.display = 'none';
        
        // Clear the timeout since we're manually hiding
        if (loadingTimeout) {
            clearTimeout(loadingTimeout);
            loadingTimeout = null;
        }
    }
};

window.showSuccess = function(title, message) {
    window.notificationManager.showNotification('success', title, message);
};

window.showError = function(title, message) {
    window.notificationManager.showNotification('error', title, message);
};

window.showWarning = function(title, message) {
    window.notificationManager.showNotification('warning', title, message);
};

window.showInfo = function(title, message) {
    window.notificationManager.showNotification('info', title, message);
};

// Initialize loading spinner state on page load
document.addEventListener('DOMContentLoaded', function() {
    // Ensure loading spinner is hidden by default
    window.hideLoading();
});