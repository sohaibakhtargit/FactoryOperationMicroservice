namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs
{
    public class TenantDashboardDto
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string AdminEmail { get; set; }
        public string Plan { get; set; }

        public int ActiveUsers { get; set; }
        public int MaxUsers { get; set; }

        public int TotalAssets { get; set; }
        public int MaxAssets { get; set; }

        public double UsedStorageGB { get; set; }
        public double MaxStorageGB { get; set; }

        public string? ContactNumber { get; set; }
        public DateTime? LastActiveDate { get; set; }
    }
}
