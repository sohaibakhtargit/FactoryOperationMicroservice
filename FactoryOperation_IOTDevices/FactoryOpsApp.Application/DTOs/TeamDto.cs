using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AddTeamDto
    {
    public int TenantId { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? Site { get; set; }
    public string Department { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public List<int?> MemberIds { get; set; } = new(); 
    }

    public class GetTeamDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public int? SiteId { get; set; }
        public string? Site { get; set; }
        public string Department { get; set; } = string.Empty;
        public int? ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public List<TeamMemberMapDto> Members { get; set; } = new();
    }

    public class TeamMemberMapDto
    {
        public int? UserId { get; set; }
        public string MemberName { get; set; } = string.Empty;
    }


}

