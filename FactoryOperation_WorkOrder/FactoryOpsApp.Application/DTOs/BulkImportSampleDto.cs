namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs
{
    public class BulkImportSampleDto
    {
        public int TenantId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public IFormFile File { get; set; }
    }
}
