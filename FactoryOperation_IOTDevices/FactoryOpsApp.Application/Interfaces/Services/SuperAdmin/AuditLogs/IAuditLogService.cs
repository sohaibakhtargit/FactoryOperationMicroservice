using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs
{
    public interface IAuditLogService
    {
        public Task LogAuditAsync(string action, string details, int? tenantId, string? email, string? eventType);

    }
}
