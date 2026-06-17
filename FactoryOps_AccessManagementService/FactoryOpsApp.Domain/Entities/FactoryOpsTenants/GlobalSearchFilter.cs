using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class GlobalSearchFilters
    {
        [Key] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PId { get; set; }
        public int? TenantId { get; set; }
        public int? RoleId { get; set; }
        public string FilterVariable { get; set; }
        public string ModuleName { get; set; }
        public string? SubModuleName { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
