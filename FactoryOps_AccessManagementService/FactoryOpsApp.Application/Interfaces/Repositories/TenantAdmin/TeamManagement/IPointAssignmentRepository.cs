using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement
{
    public interface IPointAssignmentRepository
    {
        Task<CommonResponseModel> AddPointAssignmentAsync(PointAssignmentDto dto);
        Task<CommonResponseModel> UpdatePointAssignmentAsync(PointAssignmentDto dto);
        Task<CommonResponseModel> DeletePointAssignmentAsync(int pointAssignmentId, int tenantId);
        Task<GetAllRecord<GetPointAssignmentDto>> GetAllPointAssignmentsAsync(int tenantId);
        Task<GetSpecificRecord<GetPointAssignmentDto>> GetPointAssignmentByIdAsync(int pointAssignmentId, int tenantId);
    }
}
