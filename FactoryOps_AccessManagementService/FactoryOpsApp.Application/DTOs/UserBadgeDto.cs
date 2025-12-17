using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;

namespace FactoryOpsApp.Application.DTOs
{
    public class UserBadgeDto
    {
        public int UserBadgeId { get; set; }

        [Required]
        public int BadgeId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime AwardedDate { get; set; } = DateTime.UtcNow;
        public int Progress { get; set; } = 0;
        public bool IsAwarded { get; set; } = false;

        [Required]
        public int TenantId { get; set; }
    }

    public class GetUserBadgeDto : UserBadgeDto
    {
        public string BadgeName { get; set; } = string.Empty;
        public string BadgeDescription { get; set; } = string.Empty;
        public int BadgeType { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AwardUserBadgeDto
    {
        [Required]
        public int BadgeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        public int AwardedBy { get; set; }
    }

    public class UpdateUserBadgeProgressDto
    {
        [Required]
        public int UserBadgeId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [Range(0, 100)]
        public int Progress { get; set; }

        public bool IsAwarded { get; set; } = false;
    }
}