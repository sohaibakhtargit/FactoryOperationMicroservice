using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("PointAssignments")]
    public class PointAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PointAssignmentId { get; set; }

        [Required, MaxLength(200)]
        public string TaskName { get; set; }

        [Required]
        public int AssignedToUserId { get; set; }

        public int? TeamId { get; set; }

        [MaxLength(20)]
        public string Complexity { get; set; } = "Medium";

        [MaxLength(20)]
        public string Urgency { get; set; } = "Medium";

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
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TeamId))]
        public FactoryTeam? Team { get; set; }
    }

    public enum PointAssignmentStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }
}