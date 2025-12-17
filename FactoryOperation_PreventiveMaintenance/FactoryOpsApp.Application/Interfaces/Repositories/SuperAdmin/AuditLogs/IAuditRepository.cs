using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.AuditLogs
{
    public interface IAuditRepository
    {
        public GetAllRecord<GetAllAuditsDto> GetAuditRecords();
        public GetAllRecord<GetAllAuditsDto> GetTenantAuditRecords(int TenantId);
    }
}
