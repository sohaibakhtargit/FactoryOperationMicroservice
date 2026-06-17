namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.AI
{
    public interface IAiQueryService
    {
        Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, int tenantId);
    }
}
