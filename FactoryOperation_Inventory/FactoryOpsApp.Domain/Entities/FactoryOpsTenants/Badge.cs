using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

[Table("Badges")]
public class Badge
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BadgeId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    public BadgeType BadgeType { get; set; } = BadgeType.Achievement;

    public BadgeRarity Rarity { get; set; } = BadgeRarity.Common;

    public int PointsRequired { get; set; } = 0;
    public int TasksRequired { get; set; } = 0;
    public int DaysRequired { get; set; } = 0;

    [Required]
    public int TenantId { get; set; }

    public int? TeamId { get; set; }

    [ForeignKey("TeamId")]
    public FactoryTeam? FactoryTeam { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserBadge> UserBadges { get; set; }
}

public enum BadgeType
{
    Safety,
    Speed,
    Quality,
    Leadership,
    Achievement,
    Milestone
}

public enum BadgeRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}