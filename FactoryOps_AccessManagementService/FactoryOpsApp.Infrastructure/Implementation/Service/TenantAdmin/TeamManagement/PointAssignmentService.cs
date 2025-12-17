using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TeamManagement
{
    public class PointAssignmentService : IPointAssignmentService
    {
        private readonly IPointAssignmentRepository _repository;

        public PointAssignmentService(IPointAssignmentRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddPointAssignmentAsync(PointAssignmentDto dto)
        {
            return _repository.AddPointAssignmentAsync(dto);
        }

        public Task<CommonResponseModel> UpdatePointAssignmentAsync(PointAssignmentDto dto)
        {
            return _repository.UpdatePointAssignmentAsync(dto);
        }

        public Task<CommonResponseModel> DeletePointAssignmentAsync(int pointAssignmentId, int tenantId)
        {
            return _repository.DeletePointAssignmentAsync(pointAssignmentId, tenantId);
        }

        public Task<GetAllRecord<GetPointAssignmentDto>> GetAllPointAssignmentsAsync(int tenantId)
        {
            return _repository.GetAllPointAssignmentsAsync(tenantId);
        }

        public Task<GetSpecificRecord<GetPointAssignmentDto>> GetPointAssignmentByIdAsync(int pointAssignmentId, int tenantId)
        {
            return _repository.GetPointAssignmentByIdAsync(pointAssignmentId, tenantId);
        }
    }
}