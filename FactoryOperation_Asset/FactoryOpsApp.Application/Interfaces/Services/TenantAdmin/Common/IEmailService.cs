using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common
{
    public interface IEmailService
    {
        public Task<EmailResponse> SendEmailAsync(EmailDTO email);

    }
}
