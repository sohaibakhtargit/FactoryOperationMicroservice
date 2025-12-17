using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class InventoryCostIntegration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkOrderPartId { get; set; }

        public int TenantId { get; set; }

        [ForeignKey("WorkOrder")]
        public int WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; }

        [MaxLength(255)]
        public string WorkOrderName { get; set; }

        [ForeignKey("Inventory")]
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; }

        [MaxLength(255)]
        public string PartName { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "numeric(18,2)")]
        public decimal UnitCost { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }

        public DateTime? DeletedAt { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
    }
}
