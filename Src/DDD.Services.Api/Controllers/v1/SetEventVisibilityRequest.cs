using System.ComponentModel.DataAnnotations;

namespace DDD.Services.Api.Controllers.V1;

public class SetEventVisibilityRequest
{
    [Required]
    public string Visibility { get; set; }
}