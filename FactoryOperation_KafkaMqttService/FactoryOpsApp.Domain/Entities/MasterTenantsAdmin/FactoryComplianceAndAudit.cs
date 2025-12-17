using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class FactoryComplianceAndAudit
    {
        [Key]
        public int Id { get; set; }

        public int TenantId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ComplianceType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        public DateTime? LastReviewedOn { get; set; }

        public string? Status { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }
    }
}
