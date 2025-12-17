using FactoryOperation_NotificationService.FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_NotificationService.FactoryOpsApp.Application.Templates
{
    public static class EmailTemplateBuilder
    {
        public static EmailDTO BuildAssignmentEmail(dynamic data) => new EmailDTO
        {
            To = data.To,
            Subject = "New Work Order Assigned",
            Body =
                $"Hello {data.FirstName} {data.LastName},<br/><br/>" +
                $"A new work order has been assigned to you.<br/><br/>" +
                $"<b>Work Order:</b> {data.WorkOrderNumber}<br/>" +
                $"<b>Title:</b> {data.Title}<br/>" +
                $"<b>Priority:</b> {data.Priority}<br/>" +
                $"<b>Due Date:</b> {data.DueDate}<br/><br/>" +
                $"Regards,<br/>Maintenance Team"
        };

        public static EmailDTO BuildTenantEmail(dynamic data) => new EmailDTO
        {
            To = data.To,
            Subject = "New Work Order Created",
            Body =
                $"Hello {data.AdminName},<br/><br/>" +
                $"A new work order has been created.<br/><br/>" +
                $"<b>Work Order:</b> {data.WorkOrderNumber}<br/>" +
                $"<b>Title:</b> {data.Title}<br/>" +
                $"<b>Priority:</b> {data.Priority}<br/>" +
                $"<b>Status:</b> {data.Status}<br/><br/>" +
                $"Regards,<br/>Maintenance Team"
        };

        public static EmailDTO BuildSupervisorEmail(dynamic data) => new EmailDTO
        {
            To = data.To,
            Subject = "Work Order Assigned",
            Body =
                $"Hello {data.FirstName},<br/><br/>" +
                $"A new work order has been created.<br/><br/>" +
                $"<b>Work Order:</b> {data.WorkOrderNumber}<br/>" +
                $"<b>Title:</b> {data.Title}<br/><br/>" +
                $"Regards,<br/>Maintenance Team"
        };

        public static EmailDTO BuildCreatedByEmail(dynamic data) => new EmailDTO
        {
            To = data.To,
            Subject = "Your Work Order Has Been Created",
            Body =
                $"Hello {data.FirstName},<br/><br/>" +
                $"Your work order has been successfully created.<br/><br/>" +
                $"<b>Work Order:</b> {data.WorkOrderNumber}<br/>" +
                $"<b>Title:</b> {data.Title}<br/><br/>" +
                $"Regards,<br/>Maintenance Team"
        };
    }

}
