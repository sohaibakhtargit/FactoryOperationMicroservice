using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    [Table("ModuleMasterMapping")]
    public class ModuleMasterMapping
    {
        [Key]
        public int ModuleMapId { get; set; }

        [ForeignKey("FactoryTenant")]
        public int? TenantId { get; set; }

        [ForeignKey("ModuleMaster")]
        public int? ModuleId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public virtual FactoryTenants? FactoryTenant { get; set; }
        public virtual ModuleMaster? ModuleMaster { get; set; }
    }
}
