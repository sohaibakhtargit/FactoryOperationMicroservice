using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class TechnicianAssignment_DispatchDto
    {
        public int TenantId { get; set; }
        public int AvailableTechnicians { get; set; }
        public int TotalTechnicians { get; set; }
        public int UnassignedWorkOrders { get; set; }
        public int InProgressWorkOrders { get; set; }
        public int AsignedWorkOrders { get; set; }
        public double AvgResponseTimeMinutes { get; set; }
        public string ResponseStatus { get; set; }

    }
    public class WorkOrdersRequiringAssignmentDto
    {
        public int TenantId { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
        public string Title { get; set; }
        public string Priority { get; set; }
        public string? Location { get; set; }
        public int? LocationId { get; set; }
        public int? TechId { get; set; }
        public string? TechName { get; set; }
        public List<string> SkillsRequired { get; set; }
        public WorkOrderStatus? Status { get; set; }
    }
    public class TechnicianDto
    {
        public int TenantId { get; set; }
        public int UserId { get; set; }
        public string TeamName { get; set; }
        public string FullName { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
        public List<string> Skills { get; set; }
        public string Availability { get; set; }
        public int ActiveWorkOrders { get; set; }
        public double Efficiency { get; set; }
    }
    public class AssignmentHistoryDto
    {
        public int TenantId { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
        public string Title { get; set; }
        public string TechnicianName { get; set; }
        public string TeamName { get; set; }
        public DateTime AssignedDate { get; set; }
        public WorkOrderStatus? Status { get; set; }
    }
    public class AssignTechnicianUpdateWorkOrder
    {
        public int TenantId { get; set; }
        public int WorkOrderId { get; set; }
       // public string TeamName { get; set; }
        public int? UserId { get; set; }
        public int? TeamId { get; set; }
        public int UpdatedBy {  get; set; } 
        public WorkOrderStatus Status { get; set; }
    }
}
