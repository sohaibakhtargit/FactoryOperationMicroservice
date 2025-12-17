using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class BadgeDto
    {
        public int BadgeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public BadgeType BadgeType { get; set; } = BadgeType.Achievement;
        public BadgeRarity Rarity { get; set; } = BadgeRarity.Common;
        public int PointsRequired { get; set; } = 0;
        public int TasksRequired { get; set; } = 0;
        public int DaysRequired { get; set; } = 0;

        [Required]
        public int TenantId { get; set; }
        public int? TeamId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class GetBadgeDto : BadgeDto
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public int TotalAwarded { get; set; }
        public string? TeamName { get; set; }
        public List<TeamMemberDto>? TeamMembers { get; set; }
    }

    public class TeamMemberDto
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
    }

    public class CreateBadgeDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string BadgeType { get; set; } = "Achievement";
        public string Rarity { get; set; } = "Common";
        public int PointsRequired { get; set; } = 0;
        public int TasksRequired { get; set; } = 0;
        public int DaysRequired { get; set; } = 0;

        [Required]
        public int TenantId { get; set; }
        public int? TeamId { get; set; }

        [Required]
        public int CreatedBy { get; set; }
    }

    public class UpdateBadgeDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public BadgeType BadgeType { get; set; } = BadgeType.Achievement;
        public BadgeRarity Rarity { get; set; } = BadgeRarity.Common;
        public int PointsRequired { get; set; } = 0;
        public int TasksRequired { get; set; } = 0;
        public int DaysRequired { get; set; } = 0;
        public int? TeamId { get; set; }
        public bool IsActive { get; set; } = true;
        public int UpdatedBy { get; set; }
    }
}