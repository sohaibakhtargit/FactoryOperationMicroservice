using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

public class FactoryUsers
{
    [Key]
    public int UserId { get; set; }
    [Required]
    public int TenantId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string ContactNumber { get; set; }
    public string? DepartmentOrTeam { get; set; }
    public bool Status { get; set; } = true;
    public DateTime? LastLogin { get; set; }
    public bool PasswordResetRequested { get; set; } = false;
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public bool MFAEnabled { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public bool Suspend { get; set; } = false;
    public bool ForceLogout { get; set; } = false;
    [Column("ProfileImage")]
    public string? ProfileImage { get; set; }

    [Column("ProfileLogoUrl")]
    public string? ProfileLogoUrl { get; set; }
    public string? Bio { get; set; }
    public string? OTPCode { get; set; }
    public DateTime? OTPExpiry { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public ICollection<FactoryUserRoles> FactoryUserRoles { get; set; } = new List<FactoryUserRoles>();
}
