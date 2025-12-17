using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class WorkOrderSubTaskDto
    {
        public int SubTaskId { get; set; }
        public int WorkOrderId { get; set; }
        public int? ParentTaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public PriorityLevel Priority { get; set; }
        public SubTaskStatus Status { get; set; }
        public int EstimatedMinutes { get; set; }
        public int? ActualMinutes { get; set; }
        public int? AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; }
        public int? AssignedToTeamId { get; set; }
        public string AssignedToTeamName { get; set; }
        public int Sequence { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CreateWorkOrderSubTaskDto
    {
        public int WorkOrderId { get; set; }
        public int? ParentTaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public PriorityLevel Priority { get; set; }
        public SubTaskStatus Status { get; set; }
        public int? ActualMinutes { get; set; }
        public int EstimatedMinutes { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public int Sequence { get; set; }
        public int TenantId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UpdateWorkOrderSubTaskDto
    {
        public int TenantId { get; set; }   
        public int SubTaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public PriorityLevel Priority { get; set; }
        public SubTaskStatus Status { get; set; }
        public int EstimatedMinutes { get; set; }
        public int? ActualMinutes { get; set; }
        public int? AssignedToUserId { get; set; }
        public int? AssignedToTeamId { get; set; }
        public int Sequence { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
    }
    public class WorkOrderWithSubTasksDto
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }   // adjust property name as per WorkOrder entity
        public string WorkOrderTitle { get; set; }    // adjust property name as per WorkOrder entity
        public string WorkOrderStatus { get; set; }
        public string WorkOrderPriority { get; set; }
        public int? EstimatedDurationInMinutes { get; set; }
        public int? AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; }
        public List<WorkOrderSubTaskDto> SubTasks { get; set; } = new List<WorkOrderSubTaskDto>();
    }


}
