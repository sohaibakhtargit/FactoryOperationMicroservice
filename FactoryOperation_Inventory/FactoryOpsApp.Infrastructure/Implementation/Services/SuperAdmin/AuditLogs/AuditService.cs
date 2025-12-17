using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.AuditLogs;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Infrastructure.Service.SuperAdmin.AuditLogs
{
    public class AuditService :IAuditService
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
