using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs
{
    public interface IAuditService
    {
        public GetAllRecord<GetAllAuditsDto> GetAuditRecords();
        public GetAllRecord<GetAllAuditsDto> GetTenantAuditRecords(int TenandId);
    }
}
