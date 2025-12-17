/*using FactoryOperation_NotificationService.FactoryOpsApp.Application.DTOs;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EmailServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.EmailService
{
    public sealed class NotificationEmailSender : INotificationEmailSender
    {
        private readonly ILogger<NotificationEmailSender> _logger;
        private readonly SmtpSettings _settings;
        private readonly IEmailService _emailService;

        public NotificationEmailSender(
            ILogger<NotificationEmailSender> logger,
            IOptions<SmtpSettings> settings,
            IEmailService emailService)
        {
            _logger = logger;
            _settings = settings.Value;
            _emailService = emailService;
        }

        public async Task SendWorkOrderEmailAsync(WorkOrderEventDto evt)
        {
            try
            {
                // If no user email, skip
                *//* if (string.IsNullOrWhiteSpace(evt.AssignedToUserId.ToString()))
                 {
                     _logger.LogWarning("Skipping email — AssignedToUserEmail is empty for WO={WO}", evt.WorkOrderId);
                     return;
                 }*//*

                var emailDto = new EmailDTO
                {
                    From = _settings.From ?? "shoaibmaliklenovo@gmail.com",    // From SMTP Settings
                    //To = evt.AdminEmail,
                    To = "factory.operation@yopmail.com",  // Target User Email
                    Subject = $"Work Order {evt.EventType}: {evt.WorkOrderNumber}",

                    Body = $@"
                        <html>
                        <body>
                            <h2>Work Order Notification</h2>

                            <p><b>Event Type:</b> {evt.EventType}</p>
                            <p><b>Work Order Number:</b> {evt.WorkOrderNumber}</p>
                            <p><b>Title:</b> {evt.Title}</p>
                            <p><b>Status:</b> {evt.Status}</p>
                            <p><b>Priority:</b> {evt.Priority}</p>
                            <p><b>Event Time:</b> {evt.EventTime}</p>

                            <hr />

                            <p>This is an automated notification from FactoryOps.</p>
                        </body>
                        </html>"
                };

                var response = await _emailService.SendEmailAsync(emailDto);

                if (response.Success)
                    _logger.LogInformation("Email sent for WorkOrder={WO} to {Email}", evt.WorkOrderId, evt.AssignedToUserId.ToString());
                else
                    _logger.LogWarning("Email FAILED for WorkOrder={WO}: {Msg}", evt.WorkOrderId, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email sending failed for WO={WO}", evt.WorkOrderId);
            }
        }
        public async Task SendInventoryLowStockEmailAsync(LowStockEventDto evt)
        {
            try
            {
                // If no user email, skip
                *//* if (string.IsNullOrWhiteSpace(evt.TargetUserId.ToString()))
                 {
                     _logger.LogWarning("Skipping email — Store Keeper Email is empty for WO={WO}", evt.ItemName);
                     return;
                 }*//*

                // Determine the target email (for now hardcoded like your WorkOrder email)
                var targetEmail = "factory.operation@yopmail.com";

                var emailDto = new EmailDTO
                {
                    From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                    To = targetEmail,
                    Subject = $"LOW STOCK ALERT: {evt.ItemName}",

                    Body = $@"
                <html>
                <body>
                    <h2 style='color:#d9534f;'>Low Stock Alert</h2>

                    <p><b>Item:</b> {evt.ItemName}</p>
                    <p><b>Item ID:</b> {evt.ItemId}</p>

                    <p><b>Available Quantity:</b> {evt.QuantityAvailable}</p>
                    <p><b>Reorder Level:</b> {evt.ReorderLevel}</p>

                    <p><b>Event Type:</b> {evt.EventType}</p>
                    <p><b>Event Time:</b> {evt.EventTime}</p>

                    <hr />

                    <p>Please restock this item as soon as possible to avoid workflow interruptions.</p>
                    <p>This is an automated notification from Factory Team.</p>
                </body>
                </html>"
                };

                var response = await _emailService.SendEmailAsync(emailDto);

                if (response.Success)
                    _logger.LogInformation("Low stock email sent for Item={ItemId} to {Email}", evt.ItemId, targetEmail);
                else
                    _logger.LogWarning("Low stock email FAILED for Item={ItemId}: {Msg}", evt.ItemId, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Low stock email sending failed for Item={ItemId}", evt.ItemId);
            }
        }



        public async Task SendWorkOrderProgressEmailAsync(WorkOrderProgressUpdatedEventDto evt)
        {
            var dto = new EmailDTO
            {
                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = "factory.operation@yopmail.com",
                Subject = $"Work Order Progress {evt.Action}: {evt.WorkOrderNumber}",
                Body = $@"
            <h2>Work Order Progress Updated</h2>
            <p><b>Work Order:</b> {evt.WorkOrderNumber}</p>
            <p><b>Status:</b> {evt.NewStatus}</p>
            <p><b>Progress:</b> {evt.ProgressPercentage}%</p>
         
            <p><b>Updated At:</b> {evt.UpdatedAt}</p>"
            };

            await _emailService.SendEmailAsync(dto);
        }

        public async Task SendWorkOrderAssignedEmailAsync(WorkOrderAssignedEventDto evt)
        {
            var dto = new EmailDTO
            {
                From = "shoaibmaliklenovo@gmail.com",
                To = "factory.operation@yopmail.com",
                Subject = $"Work Order Assigned: {evt.WorkOrderNumber}",
                Body = $@"
    <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
        
        <h2 style='color: #2a5298; margin-bottom: 10px;'>Work Order Assigned</h2>

        <p style='font-size: 15px;'>
            A work order has been assigned to you. Below are the details:
        </p>

        <table style='border-collapse: collapse; width: 100%; margin-top: 15px;'>
            <tr>
                <td style='padding: 8px; font-weight: bold; width: 180px;'>Work Order Number:</td>
                <td style='padding: 8px;'>{evt.WorkOrderNumber}</td>
            </tr>

            <tr>
                <td style='padding: 8px; font-weight: bold;'>Title:</td>
                <td style='padding: 8px;'>{evt.Title}</td>
            </tr>

            <tr>
                <td style='padding: 8px; font-weight: bold;'>Assigned To (User ID):</td>
                <td style='padding: 8px;'>{evt.AssignedToUserId}</td>
            </tr>

            <tr>
                <td style='padding: 8px; font-weight: bold;'>Assigned At:</td>
                <td style='padding: 8px;'>{evt.EventTime}</td>
            </tr>
        </table>

        <br />

        <a href='#' style='display: inline-block; padding: 10px 20px; background-color: #2a5298; color: white; 
            text-decoration: none; border-radius: 5px;'>
            View Work Order
        </a>

        <p style='margin-top: 25px; font-size: 13px; color: #777;'>
            This is an automated notification from FactoryOps.
        </p>

    </div>"
            };

            await _emailService.SendEmailAsync(dto);
        }

    }
}
*/





using FactoryOperation_NotificationService.FactoryOpsApp.Application.DTOs;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EmailServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.EmailService
{
    public sealed class NotificationEmailSender : INotificationEmailSender
    {
        private readonly ILogger<NotificationEmailSender> _logger;
        private readonly SmtpSettings _settings;
        private readonly IEmailService _emailService;

        public NotificationEmailSender(
            ILogger<NotificationEmailSender> logger,
            IOptions<SmtpSettings> settings,
            IEmailService emailService)
        {
            _logger = logger;
            _settings = settings.Value;
            _emailService = emailService;
        }

        public async Task SendWorkOrderEmailAsync(WorkOrderEventDto evt)
        {
            try
            {
                var emailDto = new EmailDTO
                {
                    From = _settings.From ?? "no-reply@factoryops.com",
                    To = "factory.operation@yopmail.com",
                    Subject = $"Work Order {evt.EventType}: {evt.WorkOrderNumber}",

                    Body = BuildEmailTemplate(
                        title: $"Work Order {evt.EventType}",
                        headerColor: "#2A5298",
                        content: $@"
                            <p><strong>Work Order Number:</strong> {evt.WorkOrderNumber}</p>
                            <p><strong>Title:</strong> {evt.Title}</p>
                            <p><strong>Status:</strong> {evt.Status}</p>
                            <p><strong>Priority:</strong> {evt.Priority}</p>
                            <p><strong>Event Time:</strong> {evt.EventTime}</p>"
                    )
                };

                await _emailService.SendEmailAsync(emailDto);
                _logger.LogInformation("Work order email sent for WO={WO}", evt.WorkOrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed sending work order email WO={WO}", evt.WorkOrderId);
            }
        }


        public async Task SendInventoryLowStockEmailAsync(LowStockEventDto evt)
        {
            try
            {
                var emailDto = new EmailDTO
                {
                    From = _settings.From ?? "no-reply@factoryops.com",
                    To = "factory.operation@yopmail.com",
                    Subject = $"LOW STOCK ALERT: {evt.ItemName}",

                    Body = BuildEmailTemplate(
                        title: $"Low Stock Alert: {evt.ItemName}",
                        headerColor: "#D9534F",
                        content: $@"
                            <p><strong>Item:</strong> {evt.ItemName}</p>
                            <p><strong>Item ID:</strong> {evt.ItemId}</p>
                            <p><strong>Available Quantity:</strong> {evt.QuantityAvailable}</p>
                            <p><strong>Reorder Level:</strong> {evt.ReorderLevel}</p>
                            <p><strong>Event Time:</strong> {evt.EventTime}</p>

                            <p style='color:#b30000; font-weight:bold;'>
                               ⚠ Immediate restocking is recommended to avoid workflow delays.
                            </p>"
                    )
                };

                await _emailService.SendEmailAsync(emailDto);
                _logger.LogInformation("Low stock email sent for Item={ID}", evt.ItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed sending low stock email Item={ID}", evt.ItemId);
            }
        }

        public async Task SendWorkOrderProgressEmailAsync(WorkOrderProgressUpdatedEventDto evt)
        {
            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "no-reply@factoryops.com",
                To = "factory.operation@yopmail.com",
                Subject = $"Work Order Progress {evt.Action}: {evt.WorkOrderNumber}",

                Body = BuildEmailTemplate(
                    title: "Work Order Progress Updated",
                    headerColor: "#17A2B8",
                    content: $@"
                        <p><strong>Work Order Number:</strong> {evt.WorkOrderNumber}</p>
                        <p><strong>Status:</strong> {evt.NewStatus}</p>
                        <p><strong>Progress:</strong> {evt.ProgressPercentage}%</p>
                        <p><strong>Updated At:</strong> {evt.UpdatedAt}</p>"
                )
            };

            await _emailService.SendEmailAsync(emailDto);
        }


        public async Task SendWorkOrderAssignedEmailAsync(WorkOrderAssignedEventDto evt)
        {
            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "no-reply@factoryops.com",
                To = "factory.operation@yopmail.com",
                Subject = $"Work Order Assigned: {evt.WorkOrderNumber}",

                Body = BuildEmailTemplate(
                    title: "Work Order Assigned to You",
                    headerColor: "#28A745",
                    content: $@"
                        <p><strong>Work Order Number:</strong> {evt.WorkOrderNumber}</p>
                        <p><strong>Title:</strong> {evt.Title}</p>
                        <p><strong>Assigned To User ID:</strong> {evt.AssignedToUserId}</p>
                        <p><strong>Assigned At:</strong> {evt.EventTime}</p>

                        <a href='#' style='display:inline-block;padding:10px 20px;background:#28A745;color:white;
                            border-radius:5px;text-decoration:none;margin-top:15px;'>
                            View Work Order
                        </a>"
                )
            };

            await _emailService.SendEmailAsync(emailDto);
        }
        public async Task SendPurchaseRequestEmailAsync(InventoryEventDto evt)
        {
            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = "factory.operation@yopmail.com",
                Subject = $"Purchase Request Created: {evt.InventoryName}",
                Body = BuildEmailTemplate(
                    title: "New Purchase Request Created",
                    headerColor: "#6F42C1",
                    content: $@"
                        <p><strong>Purchase Requisition ID:</strong> {evt.PurchaseRequisitionId}</p>
                        <p><strong>Inventory Name:</strong> {evt.InventoryName}</p>
                        <p><strong>Event Type:</strong> {evt.EventTime}</p>
                        <p><strong>Tenant ID:</strong> {evt.TenantId}</p>"
                )

            };
        }
        private string BuildEmailTemplate(string title, string headerColor, string content)
        {
            return $@"
                <html>
                <body style='font-family:Arial,Helvetica,sans-serif;background:#f4f4f4;padding:20px;'>

                    <div style='max-width:600px;margin:0 auto;background:white;border-radius:8px;
                        box-shadow:0 2px 6px rgba(0,0,0,0.15);'>

                        <div style='background:{headerColor};color:white;padding:16px 20px;
                            border-radius:8px 8px 0 0;'>
                            <h2 style='margin:0;font-weight:600;'>{title}</h2>
                        </div>

                        <div style='padding:20px;font-size:15px;color:#333;line-height:1.6;'>
                            {content}
                            <br/><br/>
                            <p style='font-size:12px;color:#777;'>This is an automated message from FactoryOps.</p>
                        </div>

                    </div>

                </body>
                </html>";
        }

    }
}
