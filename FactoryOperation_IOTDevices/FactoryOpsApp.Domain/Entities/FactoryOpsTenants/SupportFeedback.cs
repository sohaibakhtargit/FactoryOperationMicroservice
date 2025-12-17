using System;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants; 

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class SupportFeedback
    {
        public int FeedbackId { get; set; }
        public int TicketId { get; set; }
        public int TenantId { get; set; }

        public int Rating { get; set; }
        public string? Comments { get; set; }

        public int SubmittedBy { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public bool Acknowledged { get; set; } = false;
        public int? AcknowledgedBy { get; set; }
        public DateTime AcknowledgedAt { get; set; } 

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

       
    }
}
