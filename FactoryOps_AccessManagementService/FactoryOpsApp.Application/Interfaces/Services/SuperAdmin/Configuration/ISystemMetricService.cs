using FactoryOpsApp.Application.DTOs;
namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration
{
    public interface ISystemMetricService
    {
        //Task<CommonResponseModel> CreateMetricAsync();
        Task<GetSystemMetricDto?> GetLatestMetricAsync();
        Task<List<GetSystemMetricDto>> GetAllMetricsAsync();
    }
}
