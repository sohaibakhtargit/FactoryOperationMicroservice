using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.AuditLogs;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.SuperAdmin.AuditLogs
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
