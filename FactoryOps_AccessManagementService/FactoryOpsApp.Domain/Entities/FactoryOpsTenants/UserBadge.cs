using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("UserBadges")]
    public class UserBadge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(BadgeId))]
        public Badge Badge { get; set; }
    }

}
