using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;

namespace FactoryOpsApp.Application.DTOs
{
    public class DeviceDto
    {
        public int DeviceId { get; set; }
        public int TenantId { get; set; }
        public string DeviceName { get; set; } = null!;
        public string DeviceCode { get; set; } = null!;
        public DeviceCategoryEnum? Category { get; set; }
        public DeviceStatusEnum Status { get; set; }
        public DateTime? LastSeen { get; set; }
        public int? LocationId { get; set; }
        public int? GroupId { get; set; }
        public string DataFormat { get; set; } = "JSON";
        public string? PhysicalLocation { get; set; }
        public string? Description { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public List<int>? TopicIds { get; set; } = new List<int>();
    }

    public class GetDeviceDto
    {
        public int DeviceId { get; set; }
        public int TenantId { get; set; }
        public string DeviceName { get; set; } = null!;
        public string DeviceCode { get; set; } = null!;
        public DeviceCategoryEnum? Category { get; set; }
        public DeviceStatusEnum Status { get; set; }
        public DateTime? LastSeen { get; set; }
        public int? LocationId { get; set; }
        public string? Location { get; set; }
        public int? ParentLocationId { get; set; }
        public string? ParentLocationName { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string DataFormat { get; set; } = "JSON";
        public string? PhysicalLocation { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; }
        public List<TopicMap> Topics { get; set; } = new List<TopicMap>();
    }

    public class TopicMap
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; } = string.Empty;
    }

}
