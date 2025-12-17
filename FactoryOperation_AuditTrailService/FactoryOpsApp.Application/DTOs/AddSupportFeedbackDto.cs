using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AddSupportFeedbackDto
    {
        public int FeedbackId { get; set; }
        public int TicketId { get; set; }
        public int TenantId { get; set; }
        public int Rating { get; set; }  
        public string Comments { get; set; }
        public int SubmittedBy { get; set; } 
    }

    public class UpdateSupportFeedbackDto
    {
        public int TenantId { get; set; }
        public int FeedbackId { get; set; }

        // Optional fields to update
        public int? Rating { get; set; }
        public string? Comments { get; set; }
        public bool? IsActive { get; set; }
    }

    public class AcknowledgeFeedbackDto
    {
        public int TenantId { get; set; }
        public int FeedbackId { get; set; }
        public int? AcknowledgedBy { get; set; } 
    }

    public class GetSupportFeedbackDto
    {
        public int FeedbackId { get; set; }
        public int TicketId { get; set; }
        public string TicketSubject { get; set; }
        public string Comments { get; set; }
        public int Rating { get; set; }
        public string SubmittedByName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool Acknowledged { get; set; }
        public string AcknowledgedByName { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
    }

    public class FeedbackMetricDto
    {
        public int TenantId { get; set; }
        public decimal AvgRating { get; set; }
        public decimal SatisfactionRate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
    public class DeleteSupportFeedbackDto
    {
        public int TenantId { get; set; }
        public int FeedbackId { get; set; }

       // public int? DeletedBy { get; set; }
    }



}
