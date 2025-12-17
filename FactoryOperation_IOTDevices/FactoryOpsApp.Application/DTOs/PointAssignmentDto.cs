using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.ComponentModel.DataAnnotations;

public class PointAssignmentDto
{
    public int PointAssignmentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string TaskName { get; set; } = string.Empty;

    [Required]
    public int AssignedToUserId { get; set; }

    public int? TeamId { get; set; }

    [MaxLength(20)]
    public string Complexity { get; set; } = "Medium";

    [MaxLength(20)]
    public string Urgency { get; set; } = "Medium";

    [MaxLength(500)]
    public string? Description { get; set; }

    public int BasePoints { get; set; } = 0;
    public int BonusPoints { get; set; } = 0;
    public int TotalPoints { get; set; } = 0;

    // Change to enum
    public PointAssignmentStatus Status { get; set; } = PointAssignmentStatus.Pending;

    public DateTime? CompletionDate { get; set; }

    [Required]
    public int TenantId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GetPointAssignmentDto : PointAssignmentDto
{
    public string TeamName { get; set; } = string.Empty;
    public string AssignedToUserName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}