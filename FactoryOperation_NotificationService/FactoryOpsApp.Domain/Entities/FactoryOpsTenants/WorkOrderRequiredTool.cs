using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOpsApp.Domain.Entities
{
    [Table("WorkOrderRequiredTools")]
    public class WorkOrderRequiredTool
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int WorkOrderId { get; set; }

        [ForeignKey("WorkOrderId")]
        public WorkOrder WorkOrder { get; set; }

        // Nullable, if user doesn’t select any tool
        public int? ToolId { get; set; }

        [ForeignKey("ToolId")]
        public Inventory? Tool { get; set; }
        public int? QuantityRequired { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
