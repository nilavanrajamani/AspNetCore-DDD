using System.ComponentModel.DataAnnotations;

namespace DDD.Services.Api.Controllers.V1;

public class UnpublishEventRequest
{
    [Required(ErrorMessage = "Reason is required")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Reason must be between 3 and 500 characters")]
    public string Reason { get; set; }
}