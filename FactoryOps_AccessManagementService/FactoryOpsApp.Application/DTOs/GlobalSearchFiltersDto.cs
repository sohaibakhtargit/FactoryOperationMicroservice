namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs
{
    
    public class CreateFilterConfigurationDto
    {
        public int? TenantId { get; set; }
        public int? RoleId { get; set; }
        public string FilterVariable { get; set; }
        public string ModuleName { get; set; }
        public string? SubModuleName { get; set; }
        public int CreatedBy { get; set; }
    }
    public class GetFilterConfigurationDto
    {
        public int PId { get; set; }
        public string FilterVariable { get; set; }
    }
}
