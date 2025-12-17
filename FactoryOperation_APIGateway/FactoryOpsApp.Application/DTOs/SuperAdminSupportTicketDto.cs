using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace FactoryOpsApp.Application.DTOs
{
    public class SuperAdminSupportTicketDto
    {
        public int TicketId { get; set; }
        public int TenantId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string TenantName { get; set; }
        public string? AssignedName { get; set; }
        public string Module { get; set; } = string.Empty;
        public string CreatedBy { get; set; }
        public int? AssignedTo { get; set; }
        public DateTime? CreatedAt { get; set; }


    }
    public class UpdateSupportTicketDto
    {
        public int TicketId { get; set; }
        public int TenantId { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public int? AssignedTo { get; set; }
        public string? ResolutionNotes { get; set; }
    }
}

