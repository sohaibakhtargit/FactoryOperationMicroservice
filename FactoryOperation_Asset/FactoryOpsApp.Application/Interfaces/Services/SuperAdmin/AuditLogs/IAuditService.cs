using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs
{
    public interface IAuditService
    {
        public GetAllRecord<GetAllAuditsDto> GetAuditRecords();
        public GetAllRecord<GetAllAuditsDto> GetTenantAuditRecords(int TenandId);
    }
}
