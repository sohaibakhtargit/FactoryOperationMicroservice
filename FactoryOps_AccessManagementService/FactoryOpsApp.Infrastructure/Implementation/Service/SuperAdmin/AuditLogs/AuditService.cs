using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.AuditLogs
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _IAuditRepo;
        public AuditService(IAuditRepository IAuditRepo)
        {
            _IAuditRepo = IAuditRepo;

        }
        public GetAllRecord<GetAllAuditsDto> GetAuditRecords()
        {
            return _IAuditRepo.GetAuditRecords();
        }

        public GetAllRecord<GetAllAuditsDto> GetTenantAuditRecords(int TenantId)
        {
            return _IAuditRepo.GetTenantAuditRecords(TenantId);
        }
    }
}
