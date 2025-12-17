using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using FactoryOpsApp.Infrastructure.Settings;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.Common
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<EmailResponse> SendEmailAsync(EmailDTO email)
        {
            var response = new EmailResponse();

            try
            {
                var message = new MimeMessage();

                // From
                message.From.Add(new MailboxAddress("", email.From));

                // To
                message.To.Add(new MailboxAddress("", email.To));

                // CC
                foreach (var cc in email.Cc)
                {
                    message.Cc.Add(new MailboxAddress("", cc));
                }

                // BCC
                foreach (var bcc in email.Bcc)
                {
                    message.Bcc.Add(new MailboxAddress("", bcc));
                }

                message.Subject = email.Subject;

                var builder = new BodyBuilder { HtmlBody = email.Body };

                // Attachments
                foreach (var attachment in email.Attachments)
                {
                    builder.Attachments.Add(attachment.FileName, attachment.Content);
                }

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(
                    _settings.Host,
                    _settings.Port,
                    SecureSocketOptions.StartTls
                    );

                await client.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password);

                var messageId = await client.SendAsync(message);
                await client.DisconnectAsync(true);

                response.Success = true;
                response.Message = "Email sent successfully";
                response.MessageId = messageId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                response.Success = false;
                response.Message = "Failed to send email";
            }

            return response;
        }
    }
}
