# US002: Set Event Capacity and Pricing Tiers - UI Implementation Requirements

**Date Created:** September 25, 2025  
**Epic:** Epic 1 - Event Management  
**User Story:** US002 - Set Event Capacity and Pricing Tiers  
**Branch:** feature/event_management_system_ddd  
**Backend Status:** ✅ COMPLETED - All APIs implemented and tested

## Overview

This document outlines the comprehensive UI implementation requirements for US002: Set Event Capacity and Pricing Tiers. Building upon the successfully implemented backend APIs, this UI will provide event organizers with an intuitive interface to configure event capacity and create multiple pricing tiers with flexible pricing strategies.

**Business Context:**  
Event organizers need the ability to set total event capacity and create multiple pricing tiers (Early Bird, Regular, VIP, etc.) with different prices, capacities, and sale periods. This enables sophisticated pricing strategies and revenue optimization while maintaining inventory control.

## User Story Details

### Primary User Story
```
As an event organizer
I want to set event capacity and multiple pricing tiers
So that I can control attendance and implement pricing strategies
```

### Acceptance Criteria
```gherkin
Given I have a draft event
When I access the capacity and pricing configuration
Then I should be able to set total event capacity
And create multiple pricing tiers with individual settings
And each tier should have name, price, currency, capacity, and sale period
And the sum of tier capacities must equal total capacity
And I should receive validation for business rules
And changes should be saved and reflected in the event

Scenario: Set basic capacity and pricing
Given I have event "Tech Conference 2025" in draft status
When I set total capacity to 500
And I add pricing tier "Early Bird" with price $50, capacity 150, sale period Jan 1-31
And I add pricing tier "Regular" with price $75, capacity 250, sale period Feb 1-Nov 30
And I add pricing tier "VIP" with price $150, capacity 100, sale period Jan 1-Nov 30
Then the event should have total capacity 500
And 3 pricing tiers should be configured
And capacity validation should pass (150 + 250 + 100 = 500)
And the configuration should be saved to the event

Scenario: Validation for capacity mismatch
Given I have an event with total capacity 500
When I try to create pricing tiers totaling 600 capacity
Then I should receive validation error "Pricing tier capacities must sum to total capacity"
And the configuration should not be saved

Scenario: Edit existing pricing configuration
Given I have an event with existing capacity and pricing
When I modify the total capacity or pricing tiers
Then I should see current values pre-filled
And I should be able to update individual tiers
And validation should apply to the updated configuration
```

## UI Design Requirements

### Page Structure and Navigation

#### 1. Capacity & Pricing Management Page
**URL:** `/Events/{eventId}/Capacity`  
**Access:** Event organizers only, Draft events only  
**Layout:** Full-page form with progressive disclosure

#### 2. Integration Points
- **From Event Details:** "Set Capacity & Pricing" button
- **From Event List:** "Configure Pricing" quick action  
- **Navigation Breadcrumb:** Events > [Event Name] > Capacity & Pricing

### Visual Design Specifications

#### 1. Page Header
```html
<div class="page-header bg-gradient-primary text-white">
    <div class="container">
        <nav aria-label="breadcrumb">
            <ol class="breadcrumb bg-transparent mb-2">
                <li class="breadcrumb-item"><a href="/Events" class="text-white-50">Events</a></li>
                <li class="breadcrumb-item"><a href="/Events/Details/{id}" class="text-white-50">{EventTitle}</a></li>
                <li class="breadcrumb-item active text-white">Capacity & Pricing</li>
            </ol>
        </nav>
        <h1 class="h3 mb-1">
            <i class="fas fa-users me-2"></i>
            Event Capacity & Pricing Configuration
        </h1>
        <p class="mb-0 text-white-75">Configure total capacity and pricing tiers for your event</p>
    </div>
</div>
```

#### 2. Main Content Layout
**Container:** Bootstrap container with card-based sections  
**Responsive Breakpoints:** Mobile-first design with collapsible sections  
**Color Scheme:** Primary blue theme with status-based accent colors

### Form Components and Interactions

#### 1. Total Capacity Configuration Section
```html
<div class="card border-0 shadow-sm mb-4">
    <div class="card-header bg-light border-bottom-0">
        <h5 class="card-title mb-0">
            <i class="fas fa-calculator text-primary me-2"></i>
            Total Event Capacity
        </h5>
    </div>
    <div class="card-body">
        <div class="row">
            <div class="col-md-6">
                <label for="TotalCapacity" class="form-label fw-semibold">
                    Total Capacity <span class="text-danger">*</span>
                </label>
                <div class="input-group">
                    <span class="input-group-text">
                        <i class="fas fa-hashtag text-muted"></i>
                    </span>
                    <input type="number" 
                           class="form-control form-control-lg" 
                           id="TotalCapacity" 
                           min="1" 
                           max="50000" 
                           placeholder="e.g., 500"
                           required>
                </div>
                <div class="form-text">
                    <i class="fas fa-info-circle text-info me-1"></i>
                    Maximum number of attendees for this event
                </div>
            </div>
            <div class="col-md-6">
                <div class="capacity-summary p-3 bg-light rounded mt-4 mt-md-0">
                    <h6 class="mb-2 text-muted">Capacity Summary</h6>
                    <div class="d-flex justify-content-between">
                        <span>Total Capacity:</span>
                        <span class="fw-bold text-primary" id="capacitySummaryTotal">0</span>
                    </div>
                    <div class="d-flex justify-content-between">
                        <span>Assigned to Tiers:</span>
                        <span class="fw-bold" id="capacitySummaryAssigned">0</span>
                    </div>
                    <div class="d-flex justify-content-between">
                        <span>Available:</span>
                        <span class="fw-bold" id="capacitySummaryAvailable">0</span>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

#### 2. Pricing Tiers Configuration Section
```html
<div class="card border-0 shadow-sm mb-4">
    <div class="card-header bg-light border-bottom-0">
        <div class="d-flex justify-content-between align-items-center">
            <h5 class="card-title mb-0">
                <i class="fas fa-tags text-primary me-2"></i>
                Pricing Tiers
            </h5>
            <button type="button" class="btn btn-outline-primary btn-sm" id="addPricingTier">
                <i class="fas fa-plus me-1"></i>
                Add Pricing Tier
            </button>
        </div>
    </div>
    <div class="card-body">
        <div id="pricingTiersContainer">
            <!-- Dynamic pricing tier forms will be inserted here -->
        </div>
        
        <!-- Empty state when no tiers exist -->
        <div id="emptyPricingTiers" class="text-center py-5 text-muted">
            <i class="fas fa-tag fa-3x mb-3 text-muted"></i>
            <h6>No Pricing Tiers Configured</h6>
            <p>Click "Add Pricing Tier" to create your first pricing tier</p>
        </div>
    </div>
</div>
```

#### 3. Individual Pricing Tier Form Component
```html
<div class="pricing-tier-form border rounded p-3 mb-3" data-tier-index="{index}">
    <div class="d-flex justify-content-between align-items-start mb-3">
        <h6 class="mb-0">
            <span class="badge bg-primary me-2">Tier {index}</span>
            <span class="tier-name-display">New Pricing Tier</span>
        </h6>
        <div class="btn-group btn-group-sm">
            <button type="button" class="btn btn-outline-secondary" title="Collapse/Expand">
                <i class="fas fa-chevron-up"></i>
            </button>
            <button type="button" class="btn btn-outline-danger" title="Remove Tier">
                <i class="fas fa-trash"></i>
            </button>
        </div>
    </div>
    
    <div class="tier-form-content">
        <div class="row">
            <div class="col-md-6">
                <label class="form-label fw-semibold">
                    Tier Name <span class="text-danger">*</span>
                </label>
                <input type="text" 
                       class="form-control tier-name" 
                       placeholder="e.g., Early Bird, Regular, VIP"
                       maxlength="50"
                       required>
                <div class="form-text">
                    Choose a descriptive name for this pricing tier
                </div>
            </div>
            <div class="col-md-3">
                <label class="form-label fw-semibold">
                    Price <span class="text-danger">*</span>
                </label>
                <div class="input-group">
                    <span class="input-group-text">$</span>
                    <input type="number" 
                           class="form-control tier-price" 
                           step="0.01" 
                           min="0" 
                           max="10000"
                           placeholder="0.00"
                           required>
                </div>
            </div>
            <div class="col-md-3">
                <label class="form-label fw-semibold">
                    Currency
                </label>
                <select class="form-select tier-currency">
                    <option value="USD" selected>USD ($)</option>
                    <option value="EUR">EUR (€)</option>
                    <option value="GBP">GBP (£)</option>
                    <option value="CAD">CAD ($)</option>
                </select>
            </div>
        </div>
        
        <div class="row mt-3">
            <div class="col-md-4">
                <label class="form-label fw-semibold">
                    Tier Capacity <span class="text-danger">*</span>
                </label>
                <input type="number" 
                       class="form-control tier-capacity" 
                       min="1" 
                       placeholder="e.g., 100"
                       required>
                <div class="form-text">
                    Number of tickets in this tier
                </div>
            </div>
            <div class="col-md-4">
                <label class="form-label fw-semibold">
                    Sale Start Date <span class="text-danger">*</span>
                </label>
                <input type="datetime-local" 
                       class="form-control tier-sale-start"
                       required>
            </div>
            <div class="col-md-4">
                <label class="form-label fw-semibold">
                    Sale End Date <span class="text-danger">*</span>
                </label>
                <input type="datetime-local" 
                       class="form-control tier-sale-end"
                       required>
            </div>
        </div>
    </div>
</div>
```

#### 4. Action Buttons Section
```html
<div class="card border-0 shadow-sm">
    <div class="card-body">
        <div class="d-flex justify-content-between align-items-center">
            <div>
                <button type="button" class="btn btn-outline-secondary me-2" onclick="history.back()">
                    <i class="fas fa-arrow-left me-1"></i>
                    Back to Event
                </button>
            </div>
            <div>
                <button type="button" class="btn btn-outline-primary me-2" id="previewCapacity">
                    <i class="fas fa-eye me-1"></i>
                    Preview
                </button>
                <button type="submit" class="btn btn-primary" id="saveCapacityPricing">
                    <i class="fas fa-save me-1"></i>
                    Save Configuration
                </button>
            </div>
        </div>
    </div>
</div>
```

### JavaScript Functionality Requirements

#### 1. Dynamic Pricing Tier Management
```javascript
class CapacityPricingManager {
    constructor() {
        this.tiers = [];
        this.totalCapacity = 0;
        this.initializeEventHandlers();
    }

    initializeEventHandlers() {
        // Total capacity change handler
        $('#TotalCapacity').on('input', (e) => {
            this.updateCapacitySummary();
            this.validateCapacityDistribution();
        });

        // Add tier button handler
        $('#addPricingTier').on('click', () => {
            this.addPricingTier();
        });

        // Form submission handler
        $('#capacityPricingForm').on('submit', (e) => {
            e.preventDefault();
            this.saveConfiguration();
        });
    }

    addPricingTier(tierData = null) {
        const tierIndex = this.tiers.length + 1;
        const tierHtml = this.generateTierForm(tierIndex, tierData);
        
        $('#pricingTiersContainer').append(tierHtml);
        $('#emptyPricingTiers').hide();
        
        // Attach event handlers to new tier
        this.attachTierEventHandlers(tierIndex);
        
        // Smooth scroll to new tier
        $(`[data-tier-index="${tierIndex}"]`)[0].scrollIntoView({
            behavior: 'smooth',
            block: 'center'
        });
    }

    removePricingTier(tierIndex) {
        const tierElement = $(`[data-tier-index="${tierIndex}"]`);
        
        // Confirmation dialog
        if (confirm('Are you sure you want to remove this pricing tier?')) {
            tierElement.fadeOut(300, () => {
                tierElement.remove();
                this.reindexTiers();
                this.updateCapacitySummary();
                this.validateCapacityDistribution();
                
                // Show empty state if no tiers
                if ($('.pricing-tier-form').length === 0) {
                    $('#emptyPricingTiers').show();
                }
            });
        }
    }

    validateCapacityDistribution() {
        const totalCapacity = parseInt($('#TotalCapacity').val()) || 0;
        let assignedCapacity = 0;

        $('.tier-capacity').each((index, element) => {
            assignedCapacity += parseInt($(element).val()) || 0;
        });

        const isValid = assignedCapacity === totalCapacity;
        
        // Update validation UI
        this.updateValidationState(isValid, assignedCapacity, totalCapacity);
        
        return isValid;
    }

    updateValidationState(isValid, assigned, total) {
        const difference = assigned - total;
        
        if (difference === 0 && total > 0) {
            this.showValidationSuccess();
        } else if (difference > 0) {
            this.showValidationError(`Tier capacities exceed total by ${difference}`);
        } else if (difference < 0 && total > 0) {
            this.showValidationWarning(`${Math.abs(difference)} capacity remaining unassigned`);
        } else {
            this.clearValidation();
        }
    }
}
```

#### 2. Real-time Validation and User Feedback
```javascript
// Real-time capacity summary updates
updateCapacitySummary() {
    const totalCapacity = parseInt($('#TotalCapacity').val()) || 0;
    let assignedCapacity = 0;

    $('.tier-capacity').each((index, element) => {
        assignedCapacity += parseInt($(element).val()) || 0;
    });

    const availableCapacity = totalCapacity - assignedCapacity;

    $('#capacitySummaryTotal').text(totalCapacity.toLocaleString());
    $('#capacitySummaryAssigned').text(assignedCapacity.toLocaleString());
    $('#capacitySummaryAvailable').text(availableCapacity.toLocaleString());

    // Color coding based on availability
    const availableElement = $('#capacitySummaryAvailable');
    if (availableCapacity === 0 && totalCapacity > 0) {
        availableElement.removeClass('text-warning text-danger').addClass('text-success');
    } else if (availableCapacity < 0) {
        availableElement.removeClass('text-success text-warning').addClass('text-danger');
    } else if (availableCapacity > 0) {
        availableElement.removeClass('text-success text-danger').addClass('text-warning');
    }
}

// Form validation before submission
validateForm() {
    const errors = [];
    
    // Validate total capacity
    const totalCapacity = parseInt($('#TotalCapacity').val());
    if (!totalCapacity || totalCapacity <= 0) {
        errors.push('Total capacity must be a positive number');
    }

    // Validate pricing tiers exist
    if ($('.pricing-tier-form').length === 0) {
        errors.push('At least one pricing tier must be configured');
    }

    // Validate each pricing tier
    $('.pricing-tier-form').each((index, tierElement) => {
        const tierName = $(tierElement).find('.tier-name').val().trim();
        const tierPrice = parseFloat($(tierElement).find('.tier-price').val());
        const tierCapacity = parseInt($(tierElement).find('.tier-capacity').val());
        const saleStart = $(tierElement).find('.tier-sale-start').val();
        const saleEnd = $(tierElement).find('.tier-sale-end').val();

        if (!tierName) {
            errors.push(`Tier ${index + 1}: Name is required`);
        }
        if (!tierPrice || tierPrice < 0) {
            errors.push(`Tier ${index + 1}: Valid price is required`);
        }
        if (!tierCapacity || tierCapacity <= 0) {
            errors.push(`Tier ${index + 1}: Valid capacity is required`);
        }
        if (!saleStart || !saleEnd) {
            errors.push(`Tier ${index + 1}: Sale period dates are required`);
        }
        if (saleStart && saleEnd && new Date(saleStart) >= new Date(saleEnd)) {
            errors.push(`Tier ${index + 1}: Sale start date must be before end date`);
        }
    });

    // Validate capacity distribution
    if (!this.validateCapacityDistribution()) {
        errors.push('Total tier capacities must equal total event capacity');
    }

    return errors;
}
```

### API Integration Requirements

#### 1. Backend API Endpoints
```javascript
class CapacityPricingApiService {
    constructor() {
        this.baseUrl = '/api/v1/events';
    }

    async setEventCapacityAndPricing(eventId, capacityData) {
        try {
            const response = await fetch(`${this.baseUrl}/${eventId}/capacity`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.getAuthToken()}`
                },
                body: JSON.stringify(capacityData)
            });

            if (response.ok) {
                return await response.json();
            } else {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Failed to save capacity configuration');
            }
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }

    async getEventCapacityAndPricing(eventId) {
        try {
            const response = await fetch(`${this.baseUrl}/${eventId}/capacity`);
            if (response.ok) {
                return await response.json();
            }
            return null; // No capacity configured yet
        } catch (error) {
            console.error('Error fetching capacity data:', error);
            return null;
        }
    }
}
```

#### 2. Request/Response Models
```javascript
// Request payload structure
const capacityConfigurationRequest = {
    eventId: "guid",
    totalCapacity: 500,
    pricingTiers: [
        {
            name: "Early Bird",
            price: 50.00,
            currency: "USD",
            capacity: 150,
            saleStartDate: "2025-01-01T00:00:00Z",
            saleEndDate: "2025-01-31T23:59:59Z"
        },
        {
            name: "Regular",
            price: 75.00,
            currency: "USD",
            capacity: 250,
            saleStartDate: "2025-02-01T00:00:00Z",
            saleEndDate: "2025-11-30T23:59:59Z"
        },
        {
            name: "VIP",
            price: 150.00,
            currency: "USD",
            capacity: 100,
            saleStartDate: "2025-01-01T00:00:00Z",
            saleEndDate: "2025-11-30T23:59:59Z"
        }
    ]
};
```

### Responsive Design Requirements

#### 1. Mobile Optimization (< 768px)
- Stack capacity and summary sections vertically
- Collapse pricing tier forms by default with expand/collapse buttons
- Simplified tier form layout with stacked inputs
- Touch-friendly button sizes (minimum 44px)
- Scrollable tier list with fixed header

#### 2. Tablet Layout (768px - 992px)
- Two-column layout for capacity configuration
- Collapsible pricing tiers with accordion behavior
- Optimized form field grouping

#### 3. Desktop Layout (> 992px)
- Full multi-column layout with sidebar summary
- Expanded pricing tier forms
- Advanced interactions and hover effects

### Accessibility Requirements

#### 1. WCAG 2.1 AA Compliance
- Proper heading hierarchy (h1 → h6)
- Semantic HTML structure with landmarks
- Form labels associated with inputs
- Error messages programmatically linked to fields
- Keyboard navigation support
- Screen reader announcements for dynamic content

#### 2. Keyboard Navigation
- Tab order follows logical flow
- Enter key to add pricing tiers
- Delete key to remove focused tier
- Escape key to cancel operations
- Arrow keys for tier navigation

#### 3. Screen Reader Support
```html
<!-- ARIA labels and descriptions -->
<div role="region" aria-labelledby="capacityHeading">
    <h2 id="capacityHeading">Event Capacity Configuration</h2>
    <label for="totalCapacity">
        Total Capacity
        <span aria-label="required field" class="text-danger">*</span>
    </label>
    <input id="totalCapacity" 
           type="number" 
           aria-describedby="capacityHelp"
           aria-invalid="false">
    <div id="capacityHelp" class="form-text">
        Maximum number of attendees for this event
    </div>
</div>

<!-- Live region for validation feedback -->
<div id="validationLiveRegion" 
     aria-live="polite" 
     aria-atomic="true" 
     class="sr-only">
</div>
```

### Error Handling and User Feedback

#### 1. Validation Error Display
```html
<div class="alert alert-danger" role="alert" id="validationSummary" style="display: none;">
    <h6 class="alert-heading">
        <i class="fas fa-exclamation-triangle me-2"></i>
        Please correct the following errors:
    </h6>
    <ul class="mb-0" id="validationErrorList">
        <!-- Dynamic error list -->
    </ul>
</div>
```

#### 2. Success Feedback
```html
<div class="alert alert-success" role="alert" id="successMessage" style="display: none;">
    <h6 class="alert-heading">
        <i class="fas fa-check-circle me-2"></i>
        Configuration Saved Successfully
    </h6>
    <p class="mb-0">Event capacity and pricing tiers have been updated.</p>
</div>
```

#### 3. Loading States
```html
<div class="loading-overlay" id="savingOverlay" style="display: none;">
    <div class="loading-content">
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Saving configuration...</span>
        </div>
        <p class="mt-2 mb-0">Saving capacity configuration...</p>
    </div>
</div>
```

### Preview and Summary Features

#### 1. Configuration Preview Modal
```html
<div class="modal fade" id="capacityPreviewModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">
                    <i class="fas fa-eye me-2"></i>
                    Capacity & Pricing Preview
                </h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <div class="row">
                    <div class="col-md-6">
                        <h6 class="text-muted mb-3">Event Overview</h6>
                        <table class="table table-sm">
                            <tr>
                                <td>Total Capacity:</td>
                                <td class="fw-bold" id="previewTotalCapacity">-</td>
                            </tr>
                            <tr>
                                <td>Pricing Tiers:</td>
                                <td class="fw-bold" id="previewTierCount">-</td>
                            </tr>
                            <tr>
                                <td>Price Range:</td>
                                <td class="fw-bold" id="previewPriceRange">-</td>
                            </tr>
                        </table>
                    </div>
                    <div class="col-md-6">
                        <h6 class="text-muted mb-3">Revenue Potential</h6>
                        <table class="table table-sm">
                            <tr>
                                <td>Minimum Revenue:</td>
                                <td class="fw-bold text-success" id="previewMinRevenue">-</td>
                            </tr>
                            <tr>
                                <td>Maximum Revenue:</td>
                                <td class="fw-bold text-success" id="previewMaxRevenue">-</td>
                            </tr>
                            <tr>
                                <td>Average Price:</td>
                                <td class="fw-bold" id="previewAvgPrice">-</td>
                            </tr>
                        </table>
                    </div>
                </div>
                
                <hr>
                
                <h6 class="text-muted mb-3">Pricing Tiers Breakdown</h6>
                <div id="previewTiersList">
                    <!-- Dynamic tier summary -->
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                    Close
                </button>
                <button type="button" class="btn btn-primary" id="saveFromPreview">
                    <i class="fas fa-save me-1"></i>
                    Save Configuration
                </button>
            </div>
        </div>
    </div>
</div>
```

### Integration with Existing Event Management

#### 1. Event Details Page Integration
Add capacity configuration button to event details page:
```html
<!-- In Events/Details/{id} page -->
<div class="event-actions">
    @if (Model.Event.Status == EventStatus.Draft)
    {
        <a href="/Events/@Model.Event.Id/Capacity" class="btn btn-outline-primary">
            <i class="fas fa-users me-1"></i>
            Set Capacity & Pricing
        </a>
    }
    else if (Model.Event.TotalCapacity.HasValue)
    {
        <a href="/Events/@Model.Event.Id/Capacity" class="btn btn-outline-secondary">
            <i class="fas fa-users me-1"></i>
            View Capacity & Pricing
        </a>
    }
</div>
```

#### 2. Event List Page Integration
Show capacity status in event cards:
```html
<!-- In Events/List page -->
<div class="event-capacity-info">
    @if (event.TotalCapacity.HasValue)
    {
        <span class="badge bg-success">
            <i class="fas fa-users me-1"></i>
            @event.TotalCapacity Capacity
        </span>
        <span class="badge bg-info">
            @event.PricingTiers?.Count Tiers
        </span>
    }
    else
    {
        <span class="badge bg-warning">
            <i class="fas fa-exclamation-triangle me-1"></i>
            Capacity Not Set
        </span>
    }
</div>
```

## Technical Architecture

### Page Model Structure
```csharp
public class CapacityPageModel : PageModel
{
    private readonly IEventAppService _eventAppService;
    
    [BindProperty]
    public SetEventCapacityViewModel CapacityConfiguration { get; set; }
    
    public EventViewModel Event { get; set; }
    
    public async Task<IActionResult> OnGetAsync(Guid eventId)
    {
        Event = await _eventAppService.GetByIdAsync(eventId);
        if (Event == null)
            return NotFound();
            
        // Load existing capacity configuration if available
        CapacityConfiguration = new SetEventCapacityViewModel
        {
            EventId = eventId,
            TotalCapacity = Event.TotalCapacity ?? 0,
            PricingTiers = Event.PricingTiers?.Select(pt => new PricingTierViewModel
            {
                Name = pt.Name,
                Price = pt.Price,
                Currency = pt.Currency,
                Capacity = pt.Capacity,
                SaleStartDate = pt.SaleStartDate,
                SaleEndDate = pt.SaleEndDate
            }).ToList() ?? new List<PricingTierViewModel>()
        };
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
            
        await _eventAppService.SetCapacityAndPricingAsync(CapacityConfiguration);
        
        TempData["SuccessMessage"] = "Event capacity and pricing have been configured successfully.";
        return RedirectToPage("/Events/Details", new { id = CapacityConfiguration.EventId });
    }
}
```

### Client-Side Architecture
```javascript
// Main application entry point
$(document).ready(() => {
    const capacityManager = new CapacityPricingManager();
    const apiService = new CapacityPricingApiService();
    
    // Initialize with existing data if available
    if (window.existingCapacityData) {
        capacityManager.loadExistingData(window.existingCapacityData);
    }
    
    // Initialize default tier if none exist
    if ($('.pricing-tier-form').length === 0) {
        capacityManager.addPricingTier({
            name: 'General Admission',
            price: 0,
            currency: 'USD',
            capacity: 0,
            saleStartDate: '',
            saleEndDate: ''
        });
    }
});
```

## User Experience Enhancements

### 1. Progressive Disclosure
- Collapsible pricing tier sections
- Show/hide advanced options
- Step-by-step configuration wizard option

### 2. Smart Defaults and Suggestions
- Suggest tier names based on common patterns
- Auto-calculate balanced capacity distribution
- Price suggestions based on event type

### 3. Contextual Help and Tips
- Inline help text for complex fields
- Tooltip explanations for business rules
- Examples and best practices

### 4. Real-time Calculations
- Revenue projections based on current configuration
- Capacity utilization visualization
- Pricing comparison across tiers

## Testing Requirements

### 1. Functional Testing
- Form validation scenarios
- API integration testing
- Error handling verification
- Accessibility compliance testing

### 2. User Experience Testing
- Mobile responsiveness
- Cross-browser compatibility
- Performance under various data loads
- Usability testing with target users

### 3. Integration Testing
- Backend API integration
- Database persistence verification
- Event workflow integration
- Real-time updates testing

## Success Metrics

### 1. Usability Metrics
- Time to complete capacity configuration
- Error rate during form completion
- User satisfaction scores
- Task completion rate

### 2. Technical Metrics
- Page load performance
- API response times
- Form validation accuracy
- Cross-browser compatibility

### 3. Business Metrics
- Adoption rate of pricing tier features
- Average number of tiers per event
- Configuration completion rate
- Support ticket reduction

## Future Enhancements

### 1. Advanced Pricing Features
- Dynamic pricing based on demand
- Early bird discounts automation
- Group pricing and bulk discounts
- Promotional codes integration

### 2. Analytics and Reporting
- Tier performance analytics
- Revenue optimization suggestions
- Capacity utilization reports
- Pricing strategy recommendations

### 3. Integration Capabilities
- External ticketing system integration
- Payment processor connections
- Marketing automation triggers
- CRM system synchronization

## Conclusion

This UI implementation for US002: Set Event Capacity and Pricing Tiers provides a comprehensive, user-friendly interface for event organizers to configure sophisticated pricing strategies. Built upon the successfully implemented backend APIs, it offers:

- **Intuitive Design:** Clean, professional interface following established patterns
- **Comprehensive Functionality:** Full capacity and pricing tier management
- **Real-time Validation:** Immediate feedback and error prevention
- **Responsive Experience:** Optimized for all device types
- **Accessibility Compliance:** WCAG 2.1 AA standards met
- **Integration Ready:** Seamlessly integrates with existing event management workflow

**Development Priority:** High - Core feature for event monetization  
**Estimated Effort:** 3-4 development days  
**Dependencies:** Backend APIs (✅ Completed)  
**Success Criteria:** 95% task completion rate, <2 minute configuration time

The implementation provides a solid foundation for advanced event capacity and pricing management while maintaining the high-quality user experience established in the existing event management system.