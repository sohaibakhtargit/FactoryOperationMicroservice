using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class TenantMasterMapping
    {

        [Key]
        public int TenantMapId { get; set; }
        [ForeignKey("FactoryTenants")]
        public int TenantId { get; set; }
        public FactoryTenants FactoryTenants { get; set; } = null!;
        public string TenantName { get; set; } = string.Empty;
        public string DbName { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }
}
