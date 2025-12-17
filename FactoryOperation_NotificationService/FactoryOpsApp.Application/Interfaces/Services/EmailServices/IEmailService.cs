using FactoryOperation_NotificationService.FactoryOpsApp.Application.Common;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services
{
    public interface IEmailService
    {
        public Task<EmailResponse> SendEmailAsync(EmailDTO email);

    }
}
