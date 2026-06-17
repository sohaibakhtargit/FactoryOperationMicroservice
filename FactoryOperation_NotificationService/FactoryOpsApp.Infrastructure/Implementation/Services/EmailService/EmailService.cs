using FactoryOperation_NotificationService.FactoryOpsApp.Application.Common;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.DTOs;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.EmailService
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

                message.From.Add(new MailboxAddress("", email.From));

                // message.To.Add(new MailboxAddress("", email.To));
                foreach (var to in email.To)
                {
                    message.To.Add(new MailboxAddress("", to));
                }

                foreach (var cc in email.Cc)
                {
                    message.Cc.Add(new MailboxAddress("", cc));
                }

                foreach (var bcc in email.Bcc)
                {
                    message.Bcc.Add(new MailboxAddress("", bcc));
                }

                message.Subject = email.Subject;

                var builder = new BodyBuilder { HtmlBody = email.Body };

                foreach (var attachment in email.Attachments)
                {
                    builder.Attachments.Add(attachment.FileName, attachment.Content);
                }

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                //client.ServerCertificateValidationCallback = (s, c, h, e) => true;
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

