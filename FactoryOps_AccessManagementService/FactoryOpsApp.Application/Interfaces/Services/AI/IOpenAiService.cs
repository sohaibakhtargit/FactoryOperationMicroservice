namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.AI
{
    public interface IOpenAiService
    {
        Task<string> GenerateSqlAsync(string prompt, int tenantId, int userId);
    }
}
