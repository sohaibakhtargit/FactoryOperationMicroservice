using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required.")]
        public string ContactNumber { get; set; }
        [Required(ErrorMessage = "Role is required.")]
        public int RoleId { get; set; }
        [Required(ErrorMessage = "Tenant ID is required.")]
        public int TenantId { get; set; }
        public bool MFAEnabled { get; set; } = false;
        //public List<int> RoleIds { get; set; } = new();
    }
    public class UserResponsetDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EventType { get; set; } // Created, Updated, Deleted
        public int TenantId { get; set; }
        public string RoleId { get; set; }
        public DateTime EventTime { get; set; } = DateTime.UtcNow;
    }
}
