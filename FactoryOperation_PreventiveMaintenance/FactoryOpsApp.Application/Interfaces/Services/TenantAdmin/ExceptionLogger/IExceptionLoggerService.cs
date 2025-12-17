using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger
{
    public interface IExceptionLoggerService
    {
        Task LogExceptionAsync(Exception ex, string sourceModule, string apiName, int? tenantId = null, int? userId = null);
    }
}
