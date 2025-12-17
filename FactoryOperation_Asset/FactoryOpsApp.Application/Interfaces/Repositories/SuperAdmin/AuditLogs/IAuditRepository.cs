using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.AuditLogs
{
    public interface IAuditRepository
    {
        public GetAllRecord<GetAllAuditsDto> GetAuditRecords();
        public GetAllRecord<GetAllAuditsDto> GetTenantAuditRecords(int TenantId);
    }
}
