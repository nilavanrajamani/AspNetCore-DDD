using System.ComponentModel.DataAnnotations;

namespace DDD.Infra.CrossCutting.Identity.Models.AccountViewModels;

public class AssignPermissionRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}