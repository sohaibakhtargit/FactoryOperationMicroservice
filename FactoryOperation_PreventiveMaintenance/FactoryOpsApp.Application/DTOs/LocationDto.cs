using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class LocationDto
    {
        public int LocationId { get; set; } 
        public int TenantId { get; set; }
        public string LocationName { get; set; } = null!;
        public LocationTypeEnum LocationType { get; set; }
        public int? ParentLocationId { get; set; }
        public string? ParentLocationName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
    }

    public class LocationSimpleDto
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;

        public int? ParentLocationId { get; set; }
        public string? ParentLocationName { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public List<LocationSimpleDto> ChildLocations { get; set; } = new();
    }

    public class LocationResponse
    {
        public string StatusCode { get; set; } = "200";
        public string StatusMessage { get; set; } = "Success";
        public List<LocationSimpleDto> ChildLocations { get; set; } = new();
    }
}
