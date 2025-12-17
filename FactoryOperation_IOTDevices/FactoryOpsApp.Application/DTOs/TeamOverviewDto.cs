using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class TeamOverviewDto
    {
        public int TenantId { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int TotalPoints { get; set; }
        //public int TaskId { get; set; }
        public string TaskName { get; set; }
        public int AssignedtoUserId { get; set; }
        public string UserName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        //public int DepartmentId { get; set; }
        public string Department { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public List<TeamMemberMapDto> Members { get; set; } = new();
    }
}
