# US006: Set Event Visibility - UI Implementation Requirements

**Date Created:** January 31, 2025  
**Epic:** Epic 1 - Event Management  
**User Story:** US006 - Set Event Visibility with User Invitation System  
**Branch:** feature/event_management_system_ddd  
**Backend Status:** ✅ COMPLETED - All APIs implemented and tested

## Overview

This document outlines the comprehensive UI implementation requirements for US006: Set Event Visibility. Building upon the successfully implemented backend APIs, this UI will provide event organizers with an intuitive interface to control event access through visibility settings (Public, Private, InviteOnly) and manage user invitations with role-based permissions.

**Business Context:**  
Event organizers need granular control over who can see and access their events. This includes public events for maximum reach, private events for internal use, and invite-only events for exclusive gatherings. The invitation system supports role-based access (Attendee, Speaker, Sponsor, VIP) to enable sophisticated event access management.

## User Story Details

### Primary User Story
```
As an event organizer
I want to set event visibility (public/private/invite-only)
So that I can control who can see and access my events
```

### Acceptance Criteria
```gherkin
Given I am creating or updating an event
When I set the event visibility
Then the event should only be accessible based on visibility rules
And appropriate access controls should be enforced

Scenario: Set event to public
Given I have an event in draft status
When I set visibility to "Public"
Then the event should be visible to all users
And appear in public event listings
And no invitations should be required

Scenario: Set event to private
Given I have an event
When I set visibility to "Private"
Then only I as the organizer should see the event
And it should not appear in public listings
And no other users should have access

Scenario: Set event to invite-only
Given I have an event
When I set visibility to "InviteOnly"
Then I should be able to invite specific users
And only invited users and I should see the event
And each invitation should include a role assignment

Scenario: Manage user invitations
Given I have an invite-only event
When I invite users with different roles
Then each user should receive appropriate access
And I should be able to view and manage all invitations
And I should be able to remove invitations

Scenario: Validate access permissions
Given I have events with different visibility settings
When users try to access events
Then access should be granted based on visibility rules and invitations
And appropriate error messages should be shown for unauthorized access
```

## UI Design Requirements

### Page Structure and Navigation

#### 1. Event Visibility Management Section
**Location:** Integrated into Event Details/Edit page as a dedicated section  
**Access:** Event organizers only  
**Layout:** Card-based section with expandable invitation management

#### 2. Integration Points
- **From Event Details:** "Visibility Settings" section  
- **From Event Creation:** Visibility selection during event creation  
- **From Event List:** Visibility indicator badges

### Visual Design Specifications

#### 1. Visibility Settings Section Header
```html
<div class="card border-0 shadow-sm mb-4">
    <div class="card-header bg-light border-bottom-0">
        <h5 class="card-title mb-0">
            <i class="fas fa-eye text-primary me-2"></i>
            Event Visibility & Access Control
        </h5>
        <p class="card-text text-muted mb-0">Control who can see and access your event</p>
    </div>
```

#### 2. Visibility Options Interface
```html
<div class="card-body">
    <div class="row">
        <div class="col-12 mb-4">
            <label class="form-label fw-semibold">
                Event Visibility <span class="text-danger">*</span>
            </label>
            
            <!-- Public Option -->
            <div class="form-check form-check-card mb-3">
                <input class="form-check-input" type="radio" name="visibility" id="visibilityPublic" value="Public">
                <label class="form-check-label w-100" for="visibilityPublic">
                    <div class="card h-100">
                        <div class="card-body p-3">
                            <div class="d-flex align-items-center mb-2">
                                <i class="fas fa-globe text-success me-2"></i>
                                <h6 class="mb-0 fw-bold">Public Event</h6>
                            </div>
                            <p class="text-muted mb-0 small">
                                Visible to everyone. Appears in public listings and search results.
                            </p>
                        </div>
                    </div>
                </label>
            </div>
            
            <!-- Private Option -->
            <div class="form-check form-check-card mb-3">
                <input class="form-check-input" type="radio" name="visibility" id="visibilityPrivate" value="Private">
                <label class="form-check-label w-100" for="visibilityPrivate">
                    <div class="card h-100">
                        <div class="card-body p-3">
                            <div class="d-flex align-items-center mb-2">
                                <i class="fas fa-lock text-warning me-2"></i>
                                <h6 class="mb-0 fw-bold">Private Event</h6>
                            </div>
                            <p class="text-muted mb-0 small">
                                Only visible to you. Hidden from public listings and other users.
                            </p>
                        </div>
                    </div>
                </label>
            </div>
            
            <!-- Invite-Only Option -->
            <div class="form-check form-check-card mb-3">
                <input class="form-check-input" type="radio" name="visibility" id="visibilityInviteOnly" value="InviteOnly">
                <label class="form-check-label w-100" for="visibilityInviteOnly">
                    <div class="card h-100">
                        <div class="card-body p-3">
                            <div class="d-flex align-items-center mb-2">
                                <i class="fas fa-user-friends text-info me-2"></i>
                                <h6 class="mb-0 fw-bold">Invite-Only Event</h6>
                            </div>
                            <p class="text-muted mb-0 small">
                                Visible only to invited users. Manage invitations with role-based access.
                            </p>
                        </div>
                    </div>
                </label>
            </div>
        </div>
    </div>
</div>
```

### Invitation Management Interface (InviteOnly Events)

#### 1. Invitation Management Panel
```html
<div id="invitationPanel" class="mt-4" style="display: none;">
    <div class="card border-0 shadow-sm">
        <div class="card-header bg-gradient-info text-white">
            <h6 class="mb-0">
                <i class="fas fa-envelope me-2"></i>
                Manage Event Invitations
            </h6>
        </div>
        <div class="card-body">
            <!-- Add New Invitation -->
            <div class="row mb-4">
                <div class="col-md-8">
                    <label for="inviteUserEmail" class="form-label fw-semibold">
                        Invite User by Email
                    </label>
                    <div class="input-group">
                        <span class="input-group-text">
                            <i class="fas fa-at text-muted"></i>
                        </span>
                        <input type="email" 
                               class="form-control" 
                               id="inviteUserEmail" 
                               placeholder="user@example.com">
                    </div>
                </div>
                <div class="col-md-4">
                    <label for="inviteUserRole" class="form-label fw-semibold">
                        Role
                    </label>
                    <select class="form-select" id="inviteUserRole">
                        <option value="Attendee">Attendee</option>
                        <option value="Speaker">Speaker</option>
                        <option value="Sponsor">Sponsor</option>
                        <option value="VIP">VIP</option>
                    </select>
                </div>
            </div>
            
            <div class="row mb-4">
                <div class="col-12">
                    <button type="button" class="btn btn-primary" id="sendInvitationBtn">
                        <i class="fas fa-paper-plane me-2"></i>
                        Send Invitation
                    </button>
                </div>
            </div>
            
            <!-- Current Invitations List -->
            <div class="row">
                <div class="col-12">
                    <h6 class="fw-semibold mb-3">Current Invitations</h6>
                    <div id="invitationsList" class="table-responsive">
                        <table class="table table-hover">
                            <thead class="table-light">
                                <tr>
                                    <th><i class="fas fa-user me-1"></i>User</th>
                                    <th><i class="fas fa-tag me-1"></i>Role</th>
                                    <th><i class="fas fa-calendar me-1"></i>Invited</th>
                                    <th><i class="fas fa-check-circle me-1"></i>Status</th>
                                    <th width="100">Actions</th>
                                </tr>
                            </thead>
                            <tbody id="invitationsTableBody">
                                <!-- Dynamic content will be populated here -->
                            </tbody>
                        </table>
                        
                        <!-- Empty State -->
                        <div id="noInvitationsMessage" class="text-center py-4">
                            <i class="fas fa-inbox text-muted mb-3" style="font-size: 3rem;"></i>
                            <h6 class="text-muted">No invitations sent yet</h6>
                            <p class="text-muted mb-0">Start by inviting users above</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

#### 2. Invitation Row Template
```html
<tr data-invitation-id="{invitationId}">
    <td>
        <div class="d-flex align-items-center">
            <div class="avatar-sm me-2">
                <span class="avatar-initial rounded-circle bg-primary-subtle text-primary">
                    {userInitials}
                </span>
            </div>
            <div>
                <h6 class="mb-0">{userName}</h6>
                <small class="text-muted">{userEmail}</small>
            </div>
        </div>
    </td>
    <td>
        <span class="badge bg-{roleColor}-subtle text-{roleColor}">
            <i class="fas fa-{roleIcon} me-1"></i>
            {roleName}
        </span>
    </td>
    <td class="text-muted">
        {invitedDate}
    </td>
    <td>
        <span class="badge bg-{statusColor}">
            <i class="fas fa-{statusIcon} me-1"></i>
            {statusText}
        </span>
    </td>
    <td>
        <div class="btn-group btn-group-sm">
            <button type="button" class="btn btn-outline-danger" onclick="removeInvitation('{invitationId}')">
                <i class="fas fa-times"></i>
            </button>
        </div>
    </td>
</tr>
```

### Visual Indicators and Status Elements

#### 1. Event Visibility Badges (for Event Lists)
```html
<!-- Public Event -->
<span class="badge bg-success-subtle text-success">
    <i class="fas fa-globe me-1"></i>Public
</span>

<!-- Private Event -->
<span class="badge bg-warning-subtle text-warning">
    <i class="fas fa-lock me-1"></i>Private
</span>

<!-- Invite-Only Event -->
<span class="badge bg-info-subtle text-info">
    <i class="fas fa-user-friends me-1"></i>Invite-Only
</span>
```

#### 2. Access Control Summary Widget
```html
<div class="card border-0 bg-light">
    <div class="card-body p-3">
        <h6 class="card-title mb-2">Access Summary</h6>
        <div class="d-flex justify-content-between small">
            <span>Visibility:</span>
            <span class="fw-bold text-primary" id="currentVisibility">Not Set</span>
        </div>
        <div class="d-flex justify-content-between small" id="invitationsSummary" style="display: none;">
            <span>Total Invitations:</span>
            <span class="fw-bold text-info" id="totalInvitations">0</span>
        </div>
        <div class="d-flex justify-content-between small" id="pendingInvitationsSummary" style="display: none;">
            <span>Pending:</span>
            <span class="fw-bold text-warning" id="pendingInvitations">0</span>
        </div>
    </div>
</div>
```

### JavaScript Functionality Requirements

#### 1. Visibility Selection Handler
```javascript
// Handle visibility selection changes
document.querySelectorAll('input[name="visibility"]').forEach(radio => {
    radio.addEventListener('change', function() {
        const selectedVisibility = this.value;
        updateVisibilityUI(selectedVisibility);
        
        // Show/hide invitation panel based on selection
        const invitationPanel = document.getElementById('invitationPanel');
        if (selectedVisibility === 'InviteOnly') {
            invitationPanel.style.display = 'block';
            loadCurrentInvitations();
        } else {
            invitationPanel.style.display = 'none';
        }
        
        // Auto-save visibility change
        saveVisibilityChange(selectedVisibility);
    });
});

async function saveVisibilityChange(visibility) {
    try {
        showLoadingSpinner();
        
        const response = await fetch(`/api/v1/event-management/${eventId}/visibility`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({ visibility: visibility })
        });
        
        if (response.ok) {
            showSuccessToast('Event visibility updated successfully');
            updateAccessSummary(visibility);
        } else {
            const error = await response.json();
            showErrorToast(error.message || 'Failed to update event visibility');
        }
    } catch (error) {
        showErrorToast('Error updating event visibility');
        console.error('Visibility update error:', error);
    } finally {
        hideLoadingSpinner();
    }
}
```

#### 2. Invitation Management Functions
```javascript
// Send new invitation
async function sendInvitation() {
    const email = document.getElementById('inviteUserEmail').value;
    const role = document.getElementById('inviteUserRole').value;
    
    if (!email || !validateEmail(email)) {
        showErrorToast('Please enter a valid email address');
        return;
    }
    
    try {
        showLoadingSpinner();
        
        // First, resolve email to userId (this would be a separate API call)
        const userResponse = await fetch(`/api/v1/users/lookup?email=${encodeURIComponent(email)}`);
        if (!userResponse.ok) {
            showErrorToast('User not found. Please ensure the email is registered in the system.');
            return;
        }
        
        const user = await userResponse.json();
        
        // Send invitation
        const response = await fetch(`/api/v1/event-management/${eventId}/invitations`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({
                userId: user.id,
                role: role
            })
        });
        
        if (response.ok) {
            showSuccessToast(`Invitation sent to ${email}`);
            document.getElementById('inviteUserEmail').value = '';
            document.getElementById('inviteUserRole').value = 'Attendee';
            await loadCurrentInvitations();
        } else {
            const error = await response.json();
            showErrorToast(error.message || 'Failed to send invitation');
        }
    } catch (error) {
        showErrorToast('Error sending invitation');
        console.error('Invitation error:', error);
    } finally {
        hideLoadingSpinner();
    }
}

// Remove invitation
async function removeInvitation(userId) {
    if (!confirm('Are you sure you want to remove this invitation?')) {
        return;
    }
    
    try {
        showLoadingSpinner();
        
        const response = await fetch(`/api/v1/event-management/${eventId}/invitations/${userId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });
        
        if (response.ok) {
            showSuccessToast('Invitation removed successfully');
            await loadCurrentInvitations();
        } else {
            const error = await response.json();
            showErrorToast(error.message || 'Failed to remove invitation');
        }
    } catch (error) {
        showErrorToast('Error removing invitation');
        console.error('Remove invitation error:', error);
    } finally {
        hideLoadingSpinner();
    }
}

// Load current invitations
async function loadCurrentInvitations() {
    try {
        const response = await fetch(`/api/v1/event-management/${eventId}/invitations`, {
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });
        
        if (response.ok) {
            const invitations = await response.json();
            renderInvitationsList(invitations);
            updateInvitationsSummary(invitations);
        } else {
            console.error('Failed to load invitations');
        }
    } catch (error) {
        console.error('Error loading invitations:', error);
    }
}

// Render invitations list
function renderInvitationsList(invitations) {
    const tbody = document.getElementById('invitationsTableBody');
    const noInvitationsMessage = document.getElementById('noInvitationsMessage');
    
    if (invitations.length === 0) {
        tbody.style.display = 'none';
        noInvitationsMessage.style.display = 'block';
        return;
    }
    
    tbody.style.display = 'table-row-group';
    noInvitationsMessage.style.display = 'none';
    
    tbody.innerHTML = invitations.map(invitation => `
        <tr data-invitation-id="${invitation.userId}">
            <td>
                <div class="d-flex align-items-center">
                    <div class="avatar-sm me-2">
                        <span class="avatar-initial rounded-circle bg-primary-subtle text-primary">
                            ${getInitials(invitation.userName)}
                        </span>
                    </div>
                    <div>
                        <h6 class="mb-0">${invitation.userName}</h6>
                        <small class="text-muted">${invitation.userEmail}</small>
                    </div>
                </div>
            </td>
            <td>
                <span class="badge bg-${getRoleColor(invitation.role)}-subtle text-${getRoleColor(invitation.role)}">
                    <i class="fas fa-${getRoleIcon(invitation.role)} me-1"></i>
                    ${invitation.role}
                </span>
            </td>
            <td class="text-muted">
                ${formatDate(invitation.invitedAt)}
            </td>
            <td>
                <span class="badge bg-${getStatusColor(invitation.status)}">
                    <i class="fas fa-${getStatusIcon(invitation.status)} me-1"></i>
                    ${invitation.status}
                </span>
            </td>
            <td>
                <button type="button" class="btn btn-outline-danger btn-sm" onclick="removeInvitation('${invitation.userId}')">
                    <i class="fas fa-times"></i>
                </button>
            </td>
        </tr>
    `).join('');
}
```

#### 3. Utility Functions
```javascript
// Role-based styling helpers
function getRoleColor(role) {
    const colors = {
        'Attendee': 'primary',
        'Speaker': 'success',
        'Sponsor': 'warning',
        'VIP': 'danger'
    };
    return colors[role] || 'secondary';
}

function getRoleIcon(role) {
    const icons = {
        'Attendee': 'user',
        'Speaker': 'microphone',
        'Sponsor': 'handshake',
        'VIP': 'crown'
    };
    return icons[role] || 'user';
}

function getStatusColor(status) {
    const colors = {
        'Pending': 'warning',
        'Accepted': 'success',
        'Declined': 'danger'
    };
    return colors[status] || 'secondary';
}

function getStatusIcon(status) {
    const icons = {
        'Pending': 'clock',
        'Accepted': 'check',
        'Declined': 'times'
    };
    return icons[status] || 'question';
}

function getInitials(name) {
    return name.split(' ').map(n => n[0]).join('').toUpperCase();
}

function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

// Update access summary widget
function updateAccessSummary(visibility) {
    document.getElementById('currentVisibility').textContent = visibility;
    
    const invitationsSummary = document.getElementById('invitationsSummary');
    const pendingInvitationsSummary = document.getElementById('pendingInvitationsSummary');
    
    if (visibility === 'InviteOnly') {
        invitationsSummary.style.display = 'flex';
        pendingInvitationsSummary.style.display = 'flex';
    } else {
        invitationsSummary.style.display = 'none';
        pendingInvitationsSummary.style.display = 'none';
    }
}

function updateInvitationsSummary(invitations) {
    document.getElementById('totalInvitations').textContent = invitations.length;
    const pendingCount = invitations.filter(i => i.status === 'Pending').length;
    document.getElementById('pendingInvitations').textContent = pendingCount;
}
```

### Form Validation and User Experience

#### 1. Real-time Validation
- **Email Format:** Validate email format before allowing invitation submission
- **Duplicate Prevention:** Check for existing invitations before sending new ones
- **Role Selection:** Ensure valid role is selected
- **Visibility Requirements:** Validate that InviteOnly events have appropriate invitation management

#### 2. User Feedback Systems
```html
<!-- Success Toast Template -->
<div class="toast align-items-center text-white bg-success border-0" role="alert">
    <div class="d-flex">
        <div class="toast-body">
            <i class="fas fa-check-circle me-2"></i>
            <span id="successMessage"></span>
        </div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto"></button>
    </div>
</div>

<!-- Error Toast Template -->
<div class="toast align-items-center text-white bg-danger border-0" role="alert">
    <div class="d-flex">
        <div class="toast-body">
            <i class="fas fa-exclamation-circle me-2"></i>
            <span id="errorMessage"></span>
        </div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto"></button>
    </div>
</div>

<!-- Loading Spinner -->
<div id="loadingSpinner" class="text-center" style="display: none;">
    <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
    </div>
    <p class="mt-2 text-muted">Processing request...</p>
</div>
```

### Responsive Design Requirements

#### 1. Mobile-First Approach
- **Card-based Design:** Stack visibility options vertically on mobile
- **Touch-Friendly:** Minimum 44px touch targets for buttons
- **Simplified Interface:** Collapsible sections for complex forms
- **Optimized Tables:** Horizontal scroll for invitation tables on mobile

#### 2. Tablet and Desktop Enhancements
- **Side-by-side Layout:** Show visibility options and invitation panel side by side
- **Enhanced Tables:** Full-width tables with sortable columns
- **Keyboard Navigation:** Full keyboard accessibility for all interactions

### CSS Styling Requirements

#### 1. Custom Styles for Visibility Cards
```css
.form-check-card .form-check-input {
    display: none;
}

.form-check-card .form-check-input:checked + .form-check-label .card {
    border-color: var(--bs-primary);
    box-shadow: 0 0 0 0.25rem rgba(var(--bs-primary-rgb), 0.25);
    background-color: rgba(var(--bs-primary-rgb), 0.05);
}

.form-check-card .card {
    transition: all 0.2s ease-in-out;
    cursor: pointer;
    border: 2px solid var(--bs-border-color);
}

.form-check-card .card:hover {
    border-color: var(--bs-primary);
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.avatar-sm {
    width: 32px;
    height: 32px;
}

.avatar-initial {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.75rem;
    font-weight: 600;
}
```

#### 2. Animation and Transition Effects
```css
.invitation-panel-enter {
    opacity: 0;
    transform: translateY(-20px);
    transition: all 0.3s ease-in-out;
}

.invitation-panel-enter-active {
    opacity: 1;
    transform: translateY(0);
}

.table tbody tr {
    transition: background-color 0.2s ease-in-out;
}

.btn-group-sm .btn {
    transition: all 0.2s ease-in-out;
}

.btn-group-sm .btn:hover {
    transform: scale(1.05);
}
```

### Integration with Backend APIs

#### 1. API Service Class
```javascript
class EventVisibilityApiService {
    constructor(baseUrl, authToken) {
        this.baseUrl = baseUrl;
        this.authToken = authToken;
    }
    
    async setEventVisibility(eventId, visibility) {
        const response = await fetch(`${this.baseUrl}/api/v1/event-management/${eventId}/visibility`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.authToken}`
            },
            body: JSON.stringify({ visibility })
        });
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return response.json();
    }
    
    async inviteUserToEvent(eventId, userId, role) {
        const response = await fetch(`${this.baseUrl}/api/v1/event-management/${eventId}/invitations`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.authToken}`
            },
            body: JSON.stringify({ userId, role })
        });
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return response.json();
    }
    
    async removeUserInvitation(eventId, userId) {
        const response = await fetch(`${this.baseUrl}/api/v1/event-management/${eventId}/invitations/${userId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${this.authToken}`
            }
        });
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return true;
    }
    
    async checkUserAccess(eventId, userId) {
        const response = await fetch(`${this.baseUrl}/api/v1/event-management/${eventId}/access/${userId}`, {
            headers: {
                'Authorization': `Bearer ${this.authToken}`
            }
        });
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        return response.json();
    }
}
```

### Testing Requirements

#### 1. Unit Testing (JavaScript)
- **Visibility Selection:** Test visibility option selection and UI updates
- **Invitation Management:** Test invitation sending and removal
- **Form Validation:** Test email validation and error handling
- **API Integration:** Test API service methods with mock responses

#### 2. Integration Testing
- **End-to-End Workflows:** Test complete visibility configuration workflows
- **Cross-browser Compatibility:** Ensure functionality across major browsers
- **Mobile Responsiveness:** Test touch interactions and responsive layouts
- **Error Scenarios:** Test error handling for network failures and invalid data

#### 3. User Acceptance Testing
- **Organizer Workflows:** Test complete event visibility management workflows
- **Invitation Workflows:** Test user invitation and management processes
- **Access Control:** Verify proper access control based on visibility settings
- **Performance:** Ensure smooth performance with large invitation lists

### Accessibility Requirements

#### 1. WCAG 2.1 AA Compliance
- **Keyboard Navigation:** Full keyboard accessibility for all interactive elements
- **Screen Reader Support:** Proper ARIA labels and semantic HTML
- **Color Contrast:** Minimum 4.5:1 contrast ratio for all text
- **Focus Management:** Clear visual focus indicators

#### 2. Semantic HTML Structure
```html
<!-- Proper heading hierarchy -->
<h1>Event Management</h1>
<h2>Visibility Settings</h2>
<h3>Invitation Management</h3>

<!-- ARIA labels for form controls -->
<input type="radio" 
       name="visibility" 
       id="visibilityPublic" 
       aria-describedby="publicDescription">
<div id="publicDescription" class="sr-only">
    Make this event visible to all users and include in public listings
</div>

<!-- Table accessibility -->
<table role="table" aria-label="Event invitations">
    <caption class="sr-only">List of users invited to this event</caption>
    <thead>
        <tr role="row">
            <th scope="col">User</th>
            <th scope="col">Role</th>
            <th scope="col">Date Invited</th>
            <th scope="col">Status</th>
            <th scope="col">Actions</th>
        </tr>
    </thead>
</table>
```

### Performance Optimization

#### 1. Loading Strategies
- **Lazy Loading:** Load invitation data only when InviteOnly is selected
- **Debounced Search:** Implement debounced email validation
- **Efficient Rendering:** Use virtual scrolling for large invitation lists
- **Caching:** Cache invitation data to avoid repeated API calls

#### 2. Bundle Optimization
- **Code Splitting:** Separate visibility management code into its own bundle
- **Tree Shaking:** Remove unused JavaScript code
- **CSS Optimization:** Minimize CSS bundle size
- **Image Optimization:** Use appropriate image formats and sizes

## Error Handling and Edge Cases

### 1. Network Error Scenarios
- **Connection Failure:** Display retry option with exponential backoff
- **Timeout Handling:** Show timeout message with manual retry option
- **Server Errors:** Display user-friendly error messages with support contact

### 2. Data Validation Edge Cases
- **Invalid Email Formats:** Real-time email validation with helpful error messages
- **Duplicate Invitations:** Prevent sending duplicate invitations to the same user
- **Permission Errors:** Handle authorization errors gracefully
- **State Synchronization:** Ensure UI state matches backend data

### 3. User Experience Edge Cases
- **Large Invitation Lists:** Implement pagination for performance
- **Concurrent Modifications:** Handle conflicts when multiple organizers edit simultaneously
- **Browser Compatibility:** Graceful degradation for older browsers
- **Offline Scenarios:** Queue actions for when connectivity is restored

## Deployment and Configuration

### 1. Environment Variables
```javascript
// Configuration object
const config = {
    apiBaseUrl: process.env.API_BASE_URL || 'https://api.eventmanagement.com',
    maxInvitationsPerEvent: process.env.MAX_INVITATIONS || 1000,
    invitationPageSize: process.env.INVITATION_PAGE_SIZE || 50,
    debounceDelay: process.env.DEBOUNCE_DELAY || 500
};
```

### 2. Feature Flags
- **Bulk Invitations:** Toggle for bulk invitation feature
- **Advanced Roles:** Toggle for additional invitation roles
- **Email Templates:** Toggle for custom invitation email templates
- **Analytics:** Toggle for invitation tracking and analytics

## Future Enhancements

### 1. Phase 2 Features
- **Bulk User Invitations:** CSV upload and bulk invitation management
- **Custom Invitation Messages:** Personalized invitation emails
- **Invitation Analytics:** Track invitation acceptance rates and engagement
- **Advanced Role Permissions:** Granular permissions for different roles

### 2. Phase 3 Features
- **Integration with External Systems:** LDAP, Active Directory integration
- **Social Media Integration:** Share invite-only events on social platforms
- **Calendar Integration:** Add events to invited users' calendars
- **Multi-language Support:** Localized invitation emails and UI

### 3. Advanced Functionality
- **Waiting Lists:** Manage waiting lists for exclusive events
- **Tiered Invitations:** Different invitation tiers with varying privileges
- **Geographic Restrictions:** Location-based invitation and access controls
- **Time-sensitive Invitations:** Auto-expiring invitations

## Conclusion

This UI implementation for US006: Set Event Visibility provides a comprehensive interface for event organizers to control access to their events through intuitive visibility settings and sophisticated invitation management. The design prioritizes user experience, accessibility, and performance while maintaining consistency with the existing Event Management System interface.

**Key Features Delivered:**
- ✅ Intuitive visibility selection with clear explanations
- ✅ Comprehensive invitation management for invite-only events
- ✅ Role-based invitation system with visual indicators
- ✅ Real-time validation and user feedback
- ✅ Mobile-responsive design with accessibility compliance
- ✅ Robust error handling and edge case management
- ✅ Performance optimization for large invitation lists

**Ready for:** Frontend development and integration with the completed backend APIs

**Next Steps:** Implement the UI components following this specification and conduct thorough testing across all supported browsers and devices.