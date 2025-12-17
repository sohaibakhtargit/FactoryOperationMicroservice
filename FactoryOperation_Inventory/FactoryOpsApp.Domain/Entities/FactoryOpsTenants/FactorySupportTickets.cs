using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class FactorySupportTickets
    {

        [Key]
        public int TicketId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Subject { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Open";

        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = "Low";

        [MaxLength(255)]
        public string Module { get; set; } = string.Empty;
        public int? AssignedTo { get; set; }

        [ForeignKey("AssignedTo")]
        public FactoryTeam? AssignedTeam { get; set; }
        public string? ResolutionNotes { get; set; }
        public int? SatisfactionRating { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
