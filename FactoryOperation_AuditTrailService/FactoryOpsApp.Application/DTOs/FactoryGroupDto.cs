using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class FactoryGroupDto
    {
        public int GroupId { get; set; }
        public int TenantId { get; set; }
        public int? LocationId { get; set; }
        public string Name { get; set; } = null!;
        public GroupTypeEnum Type { get; set; }
        public string? Description { get; set; }
        public List<int>? UserIds { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
    }

    public class FactoryGroupUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
    }

    public class FactoryGroupGetDto
    {
        public int GroupId { get; set; }
        public int TenantId { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public GroupTypeEnum Type { get; set; }
        public string? Description { get; set; }
        public List<FactoryGroupUserDto>? Users { get; set; } 
    }

}
