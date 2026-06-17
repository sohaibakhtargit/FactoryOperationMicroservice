using Confluent.Kafka;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.DTOs;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EmailServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
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
                var validEmails = evt.TargetEmailAddresses?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(IsValidEmail)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

                if (validEmails == null || !validEmails.Any())
                {
                    _logger.LogWarning(
                        "No valid recipient emails found for WorkOrder={WorkOrderNumber}",
                        evt.WorkOrderNumber);

                    return;
                }

                var emailDto = new EmailDTO
                {
                    From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                    To = validEmails,
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
                var validEmails = evt.TargetEmailAddresses?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(IsValidEmail)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

                if (validEmails == null || !validEmails.Any())
                {
                    _logger.LogWarning(
                        "No valid recipient emails found for item={WorkOrderNumber}",
                        evt.ItemName);

                    return;
                }

                var emailDto = new EmailDTO
                {
                    From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                    To = validEmails,
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
            var validEmails = evt.TargetEmailAddresses?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Where(IsValidEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for WorkOrder={WorkOrderNumber}",
                    evt.WorkOrderNumber);

                return;
            }

            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = validEmails,
                Subject = $"Work Order Progress {evt.Action}: {evt.WorkOrderNumber}",

                Body = BuildEmailTemplate(
                    title: "Work Order Progress Updated",
                    headerColor: "#17A2B8",
                    content: $@"
                        <p><strong>Work Order Number:</strong> {evt.WorkOrderNumber}</p>
                        <p><strong>Status:</strong> {evt.Status}</p>
                        <p><strong>Progress:</strong> {evt.ProgressPercentage}%</p>
                        <p><strong>Updated At:</strong> {evt.UpdatedAt}</p>"
                )
            };

            await _emailService.SendEmailAsync(emailDto);
        }


        public async Task SendWorkOrderAssignedEmailAsync(WorkOrderAssignedEventDto evt)
        {
            var validEmails = evt.TargetEmailAddresses?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Where(IsValidEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for WorkOrder={WorkOrderNumber}",
                    evt.WorkOrderNumber);

                return;
            }

            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = validEmails,
                Subject = $"Work Order Assigned: {evt.WorkOrderNumber}",

                Body = BuildEmailTemplate(
                    title: "Work Order Assigned to You",
                     headerColor: "#17A2B8",
                    content: $@"
                        <p><strong>Work Order Number:</strong> {evt.WorkOrderNumber}</p>
                        <p><strong>Title:</strong> {evt.Title}</p>
                        <p><strong>Assigned To User ID:</strong> {evt.AssignedToUserId}</p>
                        <p><strong>Assigned At:</strong> {evt.EventTime}</p>

                        //<a href='#' style='display:inline-block;padding:10px 20px;background:#28A745;color:white;
                        //    border-radius:5px;text-decoration:none;margin-top:15px;'>
                        //    View Work Order
                        //</a>"
                )
            };

            await _emailService.SendEmailAsync(emailDto);
        }
        public async Task SendPurchaseRequestEmailAsync(InventoryEventDto evt)
        {
            var validEmails = evt.TargetEmailAddresses?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Where(IsValidEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for Inventory={Item}",
                    evt.InventoryName);

                return;
            }
            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = validEmails,
                Subject = $"Purchase Request Created: {evt.InventoryName}",
                Body = BuildEmailTemplate(
                    title: "New Purchase Request Created",
                    headerColor: "#17A2B8",
                    content: $@"
                        <p><strong>Purchase Requisition ID:</strong> {evt.PurchaseRequisitionId}</p>
                        <p><strong>Inventory Name:</strong> {evt.InventoryName}</p>
                         <p><strong>Inventory Quantity:</strong> {evt.Quantity}</p>
                        <p><strong>Inventory Estimated Cost:</strong>{evt.Cost}</p>
                        <p><strong>Event Type:</strong> {evt.EventTime}</p>
                        <p><strong>Tenant ID:</strong> {evt.TenantId}</p>"
                )

             };
            await _emailService.SendEmailAsync(emailDto);

        }

        public async Task SendEmailAssetLocationStatusAsync(AssetEventDto evt)
        {
            var validEmails = evt.TargetEmailAddresses?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Where(IsValidEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for Asset={Assetame}",
                    evt.AssetName);

                return;
            }
            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = validEmails,
                Subject = $"Asset Location/Status Changed: {evt.AssetName}",
                Body = BuildEmailTemplate(
                    title: "Asset Location/Status Change Notification",
                    headerColor: "#17A2B8",
                    
                    content: $@"
                        <p><strong>Message:</strong> {evt.Message}</p>
                        <p><strong>Asset Name:</strong> {evt.AssetName}</p>
                        <p><strong>Asset ID:</strong> {evt.AssetId}</p>
                        <p><strong>New Location:</strong> {evt.LocationName}</p>
                        <p><strong>New Status:</strong> {evt.Status}</p>
                        <p><strong>Event Type:</strong> {evt.EventTime}</p>
                        <p><strong>Tenant ID:</strong> {evt.TenantId}</p>"
                )
            };
            await _emailService.SendEmailAsync(emailDto);
        }

        public async Task SendUpdatePurchaseRequestEmailAsync(InventoryEventDto evt)
        {
            var validEmails = evt.TargetEmailAddresses?
           .Where(x => !string.IsNullOrWhiteSpace(x))
           .Select(x => x.Trim())
           .Where(IsValidEmail)
           .Distinct(StringComparer.OrdinalIgnoreCase)
           .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for Inventory={Item}",
                    evt.InventoryName);

                return;
            }

            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = validEmails,
                Subject = $" Order Recieved : {evt.InventoryName}", 
                Body = BuildEmailTemplate(
                    title: "Purchase Order Request",
                      headerColor: "#17A2B8",
                         content: $@"
                        <p><strong>Purchase Requisition ID:</strong> {evt.PurchaseRequisitionId}</p>
                        <p><strong>Inventory Name:</strong> {evt.InventoryName}</p>
                        <p><strong>Inventory Quantity:</strong> {evt. Quantity}</p>
                        <p><strong>Inventory Estimated Cost:</strong>{evt.Cost}</p>
                        <p><strong>Event Type:</strong> {evt.EventTime}</p>
                        <p><strong>Tenant ID:</strong> {evt.TenantId}</p>"
                    )
            };
            await _emailService.SendEmailAsync(emailDto);

        }
        public async Task SendWorkOrderDeletedEmailAsync(WorkOrderEventDto evt)
        {
            var validEmails = evt.TargetEmailAddresses?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(IsValidEmail)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for WorkOrder={WorkOrderNumber}",
                    evt.WorkOrderNumber);

                return;
            }
            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = validEmails,
                Subject = $"Work Order Deleted: {evt.WorkOrderNumber}",
                Body = BuildEmailTemplate(
                    title: "Work Order Deleted",
                    headerColor: "#D9534F",
                    content: $@"
                        <p><strong>WorkOrderNumber:</strong> {evt.WorkOrderNumber}</p>
                        <p><strong>Title:</strong> {evt.Title}</p>
                        <p><strong>Deleted At:</strong> {evt.EventTime}</p>"
                )
            };
            await _emailService.SendEmailAsync(emailDto);
        }

        public async Task SendServiceRequestEmailAsync(ServiceRequestEventDto evt)
        {
            var validEmails = evt.TargetEmailAddresses?
               .Where(x => !string.IsNullOrWhiteSpace(x))
               .Select(x => x.Trim())
               .Where(IsValidEmail)
               .Distinct(StringComparer.OrdinalIgnoreCase)
               .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for ServiceRequest={ServiceRequestNumber}",
                    evt.ServiceRequestNumber);

                return;
            }

            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = validEmails,

                Subject = $"Service Request Created: {evt.ServiceRequestNumber}",

                Body = BuildEmailTemplate(
                    title: "Service Request Created",
                    headerColor: "#5BC0DE", 
                    content: $@"
                <p><strong>Request Number:</strong> {evt.ServiceRequestNumber}</p>
                <p><strong>Title:</strong> {evt.Title}</p>
                <p><strong>Description:</strong> {evt.Description}</p>
                <p><strong>Priority:</strong> {evt.Priority}</p>
                <p><strong>Status:</strong> {evt.Status}</p>
                <p><strong>Asset Id:</strong> {evt.AssetId}</p>
                <p><strong>Created At:</strong> {evt.EventTime}</p>"
                )
            };

            await _emailService.SendEmailAsync(emailDto);
        }
        public async Task SendServiceRequestApprovedEmailAsync(ServiceRequestEventDto evt)
        {
            var validEmails = evt.TargetEmailAddresses?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Where(IsValidEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for ServiceRequest={ServiceRequestNumber}",
                    evt.ServiceRequestNumber);

                return;
            }
            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "no-reply@factoryops.com",
                To = validEmails,
                Subject = $"Service Request Approved: {evt.ServiceRequestNumber}",

                Body = BuildEmailTemplate(
                    title: "Service Request Approved",
                    headerColor: "#28A745",
                    content: $@"
                <p><strong>Request Number:</strong> {evt.ServiceRequestNumber}</p>
                <p><strong>Title:</strong> {evt.Title}</p>
                <p><strong>Status:</strong> Approved</p>
                <p>Your request has been successfully approved and converted into a Work Order.</p>"
                )
            };

            await _emailService.SendEmailAsync(emailDto);
        }

        public async Task SendServiceRequestRejectedEmailAsync(ServiceRequestEventDto evt)
        {
            var validEmails = evt.TargetEmailAddresses?
             .Where(x => !string.IsNullOrWhiteSpace(x))
             .Select(x => x.Trim())
             .Where(IsValidEmail)
             .Distinct(StringComparer.OrdinalIgnoreCase)
             .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for ServiceRequest={ServiceRequestNumber}",
                    evt.ServiceRequestNumber);

                return;
            }

            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = validEmails,
                Subject = $"Service Request Rejected: {evt.ServiceRequestNumber}",

                Body = BuildEmailTemplate(
                    title: "Service Request Rejected",
                    headerColor: "#D9534F", 
                    content: $@"
            <p><strong>Request Number:</strong> {evt.ServiceRequestNumber}</p>
            <p><strong>Title:</strong> {evt.Title}</p>
            <p><strong>Description:</strong> {evt.Description}</p>
            <p><strong>Status:</strong> {evt.Status}</p>
            <p><strong>Rejected By:</strong> {evt.RejectedBy}</p>
            <p><strong>Reason:</strong> {evt.Reason}</p>
            <p><strong>Rejected At:</strong> {evt.EventTime}</p>

            <br/>
            <p style='color:#D9534F;'><strong>Note:</strong> Please review the rejection reason and take necessary action.</p>
            "
                )
            };

            await _emailService.SendEmailAsync(emailDto);
        }

        public async Task SendServiceRequestWorkOrderAssignedEmailAsync(ServiceRequestEventDto evt)
        {
            var validEmails = evt.TargetEmailAddresses?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(IsValidEmail)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for ServiceRequest={ServiceRequestNumber}",
                    evt.ServiceRequestNumber);

                return;
            }
            var emailDto = new EmailDTO
            {

                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = validEmails,
                Subject = $"Service Request Assigned: {evt.ServiceRequestNumber}",

                Body = BuildEmailTemplate(
                    title: "Service Request WorkOrder Assigned",
                    headerColor: "#D9534F",
                    content: $@"
                <p><strong>Request Number:</strong> {evt.ServiceRequestNumber}</p> 
                <p><strong>Work Order Number:</strong> {evt.WorkOrderNumber}</p>
                <p><strong> WorkOrderId : </strong> {evt.WorkOrderId}</p>
                <p><strong>Priority:</strong> {evt.Priority}</p>
                <p><strong>Title:</strong> {evt.Title}</p>
                <p><strong>Description:</strong> {evt.Description}</p>
                <p><strong>Status:</strong> {evt.Status}</p>
                <p><strong>Assigned To:</strong> {evt.AssignedTo}</p>
                <p><strong>Assigned At:</strong> {evt.EventTime}</p>
                <p><strong>Note:</strong> A new Work Order has been created based on your service request and assigned to {evt.AssignedTo}. Please check your work order dashboard for details.</p>
                
               ")
            };
            await _emailService.SendEmailAsync(emailDto);
        }


        public async Task SendServiceRequestReopenedEmailAsync(ServiceRequestEventDto evt)
        {
            var validEmails = evt.TargetEmailAddresses?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(IsValidEmail)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for ServiceRequest={ServiceRequestNumber}",
                    evt.ServiceRequestNumber);

                return;
            }
            var emailDto = new EmailDTO
            {

                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = validEmails,
                Subject = $"Service Request Reopened: {evt.ServiceRequestNumber}",
                Body = BuildEmailTemplate(
                    "Service Request Reopened",
                    "#5cb85c",
                    $@"
            <p><strong>Request Number:</strong> {evt.ServiceRequestNumber}</p>
            <p><strong>Title:</strong> {evt.Title}</p>
            <p><strong>Status:</strong> {evt.Status}</p>
            <p>The request has been updated Again.</p>"
                )
            };

            await _emailService.SendEmailAsync(emailDto);
        }

        public async Task SendWorkOrderApproveRejectEmailAsync(WorkOrderEventDto evt)
        {
            var validEmails = evt.TargetEmailAddresses?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(IsValidEmail)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (validEmails == null || !validEmails.Any())
            {
                _logger.LogWarning(
                    "No valid recipient emails found for WorkOrder={WorkOrderNumber}",
                    evt.WorkOrderNumber);

                return;
            }
            var emailDto = new EmailDTO
            {
                From = _settings.From ?? "shoaibmaliklenovo@gmail.com",
                To = validEmails,
                Subject = $"Workorder Update: {evt.WorkOrderNumber}",
                Body = BuildEmailTemplate(
                    "Workorder Update",
                    "#5cb85c",
                    $@"
            <p><strong>Request Number:</strong> {evt.WorkOrderNumber}</p>
            <p><strong>Title:</strong> {evt.Title}</p>
            <p><strong>Status:</strong> {evt.Status}</p>
            <p>The WorkOrder have new  updated  Again.</p>"
                )
            };
            await _emailService.SendEmailAsync(emailDto);
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

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
