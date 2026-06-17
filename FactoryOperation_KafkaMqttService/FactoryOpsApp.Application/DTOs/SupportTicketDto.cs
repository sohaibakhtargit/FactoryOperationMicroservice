using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AddSupportTicketDto
    {
        public int TenantId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Open"; 
        public string Priority { get; set; } = "Low";
        public string Module { get; set; } = string.Empty;
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string CreatedBy { get; set; }
        public int? AssignedTo { get; set; }
        public string? ResolutionNotes { get; set; }
        public int? SatisfactionRating { get; set; }
    }

    public class GetSupportTicketDto
    {
        public int TicketId { get; set; }
        public int TenantId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Description { get; set; }

        public int? SatisfactionRating { get; set; }
        public string? ResolutionNotes { get; set; }

        public int? AssignedTo { get; set; }
        public string? AssignedName { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

