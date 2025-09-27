using System;
using System.ComponentModel.DataAnnotations;

namespace DDD.Services.Api.Controllers.V1;

public class InviteUserToEventRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Role { get; set; }
}