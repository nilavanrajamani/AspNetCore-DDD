// Capacity Pricing Manager for US002: Set Event Capacity and Pricing Tiers
class CapacityPricingManager {
    constructor(options = {}) {
        this.eventId = options.eventId;
        this.eventDate = options.eventDate;
        this.existingTiers = options.existingTiers || [];
        this.tierCount = this.existingTiers.length;
        
        this.initializeEventHandlers();
        this.updateCapacitySummary();
        this.updateRevenueProjection();
    }

    initializeEventHandlers() {
        // Total capacity change handler
        $('#TotalCapacity').on('input', () => {
            this.updateCapacitySummary();
            this.validateCapacityDistribution();
        });

        // Add tier button handler
        $('#addPricingTier').on('click', () => {
            this.addPricingTier();
        });

        // Form submission handler
        $('#capacityPricingForm').on('submit', (e) => {
            if (!this.validateForm()) {
                e.preventDefault();
                return false;
            }
            this.showLoadingOverlay();
        });

        // Preview button handler
        $('#previewCapacity').on('click', () => {
            this.updatePreviewModal();
        });

        // Save from preview handler
        $('#saveFromPreview').on('click', () => {
            $('#capacityPreviewModal').modal('hide');
            $('#capacityPricingForm').submit();
        });

        // Attach event handlers to existing tier forms
        this.attachTierEventHandlers();
    }

    attachTierEventHandlers() {
        // Capacity change handlers
        $(document).on('input', '.tier-capacity, .tier-price', () => {
            this.updateCapacitySummary();
            this.updateRevenueProjection();
            this.validateCapacityDistribution();
        });

        // Name change handler
        $(document).on('input', '.tier-name', (e) => {
            const index = $(e.target).closest('.pricing-tier-form').data('tier-index');
            this.updateTierNameDisplay(index, e.target.value);
        });
    }

    addPricingTier() {
        this.tierCount++;
        const tierIndex = this.tierCount - 1;
        
        const tierHtml = this.generateTierForm(tierIndex);
        $('#pricingTiersContainer').append(tierHtml);
        $('#emptyPricingTiers').hide();
        
        // Smooth scroll to new tier
        const newTier = $(`[data-tier-index="${tierIndex}"]`);
        if (newTier.length) {
            newTier[0].scrollIntoView({
                behavior: 'smooth',
                block: 'center'
            });
        }

        this.updateCapacitySummary();
        this.updateRevenueProjection();
    }

    removePricingTier(tierIndex) {
        if (confirm('Are you sure you want to remove this pricing tier?')) {
            const tierElement = $(`[data-tier-index="${tierIndex}"]`);
            
            tierElement.fadeOut(300, () => {
                tierElement.remove();
                this.reindexTiers();
                this.updateCapacitySummary();
                this.updateRevenueProjection();
                this.validateCapacityDistribution();
                
                // Show empty state if no tiers
                if ($('.pricing-tier-form').length === 0) {
                    $('#emptyPricingTiers').show();
                }
            });
        }
    }

    toggleTierForm(tierIndex) {
        const tierElement = $(`[data-tier-index="${tierIndex}"]`);
        const content = tierElement.find('.tier-form-content');
        const icon = tierElement.find('.btn-outline-secondary i');
        
        content.slideToggle(300);
        icon.toggleClass('fa-chevron-up fa-chevron-down');
    }

    updateTierNameDisplay(tierIndex, name) {
        const displayElement = $(`[data-tier-index="${tierIndex}"] .tier-name-display`);
        displayElement.text(name || 'New Pricing Tier');
    }

    generateTierForm(tierIndex) {
        const defaultStartDate = new Date().toISOString().slice(0, -8);
        const defaultEndDate = this.eventDate ? this.eventDate.toISOString().slice(0, -8) : defaultStartDate;

        return `
            <div class="pricing-tier-form border rounded p-3 mb-3" data-tier-index="${tierIndex}">
                <div class="d-flex justify-content-between align-items-start mb-3">
                    <h6 class="mb-0">
                        <span class="badge bg-primary me-2">Tier ${tierIndex + 1}</span>
                        <span class="tier-name-display">New Pricing Tier</span>
                    </h6>
                    <div class="btn-group btn-group-sm">
                        <button type="button" class="btn btn-outline-secondary" title="Collapse/Expand" onclick="window.capacityManager.toggleTierForm(${tierIndex})">
                            <i class="fas fa-chevron-up"></i>
                        </button>
                        <button type="button" class="btn btn-outline-danger" title="Remove Tier" onclick="window.capacityManager.removePricingTier(${tierIndex})">
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
                            <input name="CapacityConfiguration.PricingTiers[${tierIndex}].Name" 
                                   type="text" 
                                   class="form-control tier-name" 
                                   placeholder="e.g., Early Bird, Regular, VIP"
                                   maxlength="50"
                                   value="">
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
                                <input name="CapacityConfiguration.PricingTiers[${tierIndex}].Price" 
                                       type="number" 
                                       class="form-control tier-price" 
                                       step="0.01" 
                                       min="0" 
                                       max="10000"
                                       placeholder="0.00"
                                       value="0">
                            </div>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label fw-semibold">
                                Currency
                            </label>
                            <select name="CapacityConfiguration.PricingTiers[${tierIndex}].Currency" class="form-select tier-currency">
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
                            <input name="CapacityConfiguration.PricingTiers[${tierIndex}].Capacity" 
                                   type="number" 
                                   class="form-control tier-capacity" 
                                   min="1" 
                                   placeholder="e.g., 100"
                                   value="0">
                            <div class="form-text">
                                Number of tickets in this tier
                            </div>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label fw-semibold">
                                Sale Start Date <span class="text-danger">*</span>
                            </label>
                            <input name="CapacityConfiguration.PricingTiers[${tierIndex}].SaleStartDate" 
                                   type="datetime-local" 
                                   class="form-control tier-sale-start"
                                   value="${defaultStartDate}">
                        </div>
                        <div class="col-md-4">
                            <label class="form-label fw-semibold">
                                Sale End Date <span class="text-danger">*</span>
                            </label>
                            <input name="CapacityConfiguration.PricingTiers[${tierIndex}].SaleEndDate" 
                                   type="datetime-local" 
                                   class="form-control tier-sale-end"
                                   value="${defaultEndDate}">
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    reindexTiers() {
        $('.pricing-tier-form').each((index, element) => {
            const $tier = $(element);
            $tier.attr('data-tier-index', index);
            
            // Update badge number
            $tier.find('.badge').text(`Tier ${index + 1}`);
            
            // Update form field names
            $tier.find('input, select').each((i, input) => {
                const $input = $(input);
                const name = $input.attr('name');
                if (name && name.includes('PricingTiers[')) {
                    const newName = name.replace(/PricingTiers\[\d+\]/, `PricingTiers[${index}]`);
                    $input.attr('name', newName);
                }
            });
        });
        
        this.tierCount = $('.pricing-tier-form').length;
    }

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
        availableElement.removeClass('text-success text-warning text-danger');
        
        if (availableCapacity === 0 && totalCapacity > 0) {
            availableElement.addClass('text-success');
        } else if (availableCapacity < 0) {
            availableElement.addClass('text-danger');
        } else if (availableCapacity > 0) {
            availableElement.addClass('text-warning');
        }
    }

    updateRevenueProjection() {
        let maxRevenue = 0;
        let minPrice = Number.MAX_VALUE;
        let maxPrice = 0;
        let totalRevenue = 0;
        let totalCapacity = 0;

        $('.pricing-tier-form').each((index, element) => {
            const price = parseFloat($(element).find('.tier-price').val()) || 0;
            const capacity = parseInt($(element).find('.tier-capacity').val()) || 0;
            
            if (price > 0 && capacity > 0) {
                const tierRevenue = price * capacity;
                maxRevenue += tierRevenue;
                totalRevenue += tierRevenue;
                totalCapacity += capacity;
                
                if (price < minPrice) minPrice = price;
                if (price > maxPrice) maxPrice = price;
            }
        });

        const avgPrice = totalCapacity > 0 ? totalRevenue / totalCapacity : 0;
        
        // Reset min price if no valid prices found
        if (minPrice === Number.MAX_VALUE) minPrice = 0;

        $('#maxRevenue').text(`$${maxRevenue.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
        $('#avgPrice').text(`$${avgPrice.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
        $('#minPrice').text(`$${minPrice.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
        $('#maxPrice').text(`$${maxPrice.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
    }

    validateCapacityDistribution() {
        const totalCapacity = parseInt($('#TotalCapacity').val()) || 0;
        let assignedCapacity = 0;

        $('.tier-capacity').each((index, element) => {
            assignedCapacity += parseInt($(element).val()) || 0;
        });

        const isValid = assignedCapacity === totalCapacity && totalCapacity > 0;
        
        // Update validation state
        this.updateValidationState(isValid, assignedCapacity, totalCapacity);
        
        return isValid;
    }

    updateValidationState(isValid, assigned, total) {
        const difference = assigned - total;
        
        // Clear previous validation messages
        this.clearValidationMessages();
        
        if (total <= 0) {
            return; // Don't show validation for empty capacity
        }
        
        if (difference === 0) {
            this.showValidationSuccess();
        } else if (difference > 0) {
            this.showValidationError(`Tier capacities exceed total by ${difference.toLocaleString()}`);
        } else if (difference < 0) {
            this.showValidationWarning(`${Math.abs(difference).toLocaleString()} capacity remaining unassigned`);
        }
    }

    showValidationSuccess() {
        this.showValidationMessage('success', 'Capacity distribution is valid');
    }

    showValidationError(message) {
        this.showValidationMessage('danger', message);
    }

    showValidationWarning(message) {
        this.showValidationMessage('warning', message);
    }

    showValidationMessage(type, message) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show mt-2" role="alert" id="capacityValidationAlert">
                <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'warning' ? 'exclamation-triangle' : 'exclamation-circle'} me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        $('.capacity-summary').after(alertHtml);
    }

    clearValidationMessages() {
        $('#capacityValidationAlert').remove();
    }

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

        if (errors.length > 0) {
            this.showFormErrors(errors);
            return false;
        }

        return true;
    }

    showFormErrors(errors) {
        const errorHtml = `
            <div class="alert alert-danger" role="alert" id="formValidationErrors">
                <h6 class="alert-heading">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    Please correct the following errors:
                </h6>
                <ul class="mb-0">
                    ${errors.map(error => `<li>${error}</li>`).join('')}
                </ul>
            </div>
        `;
        
        $('#formValidationErrors').remove();
        $('.container').prepend(errorHtml);
        
        // Scroll to top
        $('html, body').animate({ scrollTop: 0 }, 300);
    }

    updatePreviewModal() {
        const totalCapacity = parseInt($('#TotalCapacity').val()) || 0;
        const tierCount = $('.pricing-tier-form').length;
        
        let minPrice = Number.MAX_VALUE;
        let maxPrice = 0;
        let totalRevenue = 0;
        let totalTierCapacity = 0;

        const tiersData = [];
        $('.pricing-tier-form').each((index, element) => {
            const name = $(element).find('.tier-name').val() || 'Unnamed Tier';
            const price = parseFloat($(element).find('.tier-price').val()) || 0;
            const capacity = parseInt($(element).find('.tier-capacity').val()) || 0;
            const currency = $(element).find('.tier-currency').val() || 'USD';
            
            tiersData.push({ name, price, capacity, currency });
            
            if (price > 0) {
                if (price < minPrice) minPrice = price;
                if (price > maxPrice) maxPrice = price;
                totalRevenue += price * capacity;
                totalTierCapacity += capacity;
            }
        });
        
        if (minPrice === Number.MAX_VALUE) minPrice = 0;
        const avgPrice = totalTierCapacity > 0 ? totalRevenue / totalTierCapacity : 0;

        // Update preview modal content
        $('#previewTotalCapacity').text(totalCapacity.toLocaleString());
        $('#previewTierCount').text(tierCount);
        $('#previewPriceRange').text(`$${minPrice.toFixed(2)} - $${maxPrice.toFixed(2)}`);
        $('#previewMaxRevenue').text(`$${totalRevenue.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
        $('#previewAvgPrice').text(`$${avgPrice.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`);
        
        // Update tiers list
        const tiersListHtml = tiersData.map((tier, index) => `
            <div class="tier-preview-item border rounded p-2 mb-2">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <strong>${tier.name}</strong>
                        <span class="badge bg-secondary ms-2">${tier.capacity} tickets</span>
                    </div>
                    <div class="text-end">
                        <div class="h6 mb-0">$${tier.price.toFixed(2)} ${tier.currency}</div>
                        <small class="text-muted">Max: $${(tier.price * tier.capacity).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</small>
                    </div>
                </div>
            </div>
        `).join('');
        
        $('#previewTiersList').html(tiersListHtml);
    }

    showLoadingOverlay() {
        $('#savingOverlay').show();
    }

    hideLoadingOverlay() {
        $('#savingOverlay').hide();
    }
}

// Global functions for onclick handlers
function removePricingTier(tierIndex) {
    if (window.capacityManager) {
        window.capacityManager.removePricingTier(tierIndex);
    }
}

function toggleTierForm(tierIndex) {
    if (window.capacityManager) {
        window.capacityManager.toggleTierForm(tierIndex);
    }
}

function updateTierNameDisplay(tierIndex, name) {
    if (window.capacityManager) {
        window.capacityManager.updateTierNameDisplay(tierIndex, name);
    }
}