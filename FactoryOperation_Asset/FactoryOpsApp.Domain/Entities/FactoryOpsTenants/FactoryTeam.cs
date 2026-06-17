using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class FactoryTeam
    {
        [Key]
        public int TeamId { get; set; }

        [Required]
        public int TenantId { get; set; }
       
        public int? ManagerId { get; set; }

        [ForeignKey("ManagerId")]
        public FactoryUsers? Manager { get; set; }  

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public int? Site { get; set; }
        [ForeignKey("Site")]
        public Location? Location { get; set; }
        public string Department { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
    }
}
