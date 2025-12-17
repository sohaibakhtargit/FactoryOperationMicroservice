using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class FactoryTeamMembers
    {
        [Key]
        public int TeamMemberId { get; set; }

        [Required]
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        public FactoryTeam Team { get; set; }
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public FactoryUsers? User { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
    }

}
