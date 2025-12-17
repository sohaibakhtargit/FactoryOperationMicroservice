namespace FactoryOpsApp.Application.DTOs
{
    public class GetAllAuditsDto
    {
        public int AuditLogID { get; set; }
        public string Event { get; set; }
        public int UserId { get; set; }
        public string Tenant { get; set; }
        public string? Email { get; set; }
        //public string? Roles { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public string? UserName { get; set; }

        public string? Ipaddress { get; set; }
        public string? Action { get; set; }
        public string? Details { get; set; }

    }
}
