using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class SuperAdminSupportFeedbackDto
    {
        public int FeedbackId { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }

        public int Rating { get; set; }
        public string Comments { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public bool Acknowledged { get; set; }
    }

}
