using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class GetUsersListDto
    {
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int RoleId { get; set; }
        public string Role { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool MFAEnabled { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool Status { get; set; }
        public bool Suspend { get; set; }
        public bool ForceLogout { get; set; }
    }
}
