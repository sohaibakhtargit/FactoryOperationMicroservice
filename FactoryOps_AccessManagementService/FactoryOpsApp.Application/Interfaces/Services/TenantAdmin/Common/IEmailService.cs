using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common
{
    public interface IEmailService
    {
        public Task<EmailResponse> SendEmailAsync(EmailDTO email);

    }
}
