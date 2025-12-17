using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AssetTrackingDto
    {
        public int TenantId { get; set; }
        public long TrackingId { get; set; }
        public int AssetId { get; set; }
        public string? AssetName { get; set; }
        public int CurrentLocationId { get; set; }
        public string? CurrentLocationName { get; set; }
        public AssetTrackingStatusEnum Status { get; set; }
        public int? AssignedToId { get; set; }
        public string? AssignedToName { get; set; }
        public DateTime? LastMovedOn { get; set; }
        public string? GpsCoordinates { get; set; }
        public DateTime StatusUpdatedOn { get; set; }
        public string? Remarks { get; set; }

        public DateTime? DownStartTime { get; set; }
        public DateTime? DownEndTime { get; set; }
        public double? TotalDownMinutes { get; set; }
        public double? TotalDownAccumulatedMinutes { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AssetTrackingCreateDto
    {
        public int TenantId { get; set; }
        public int AssetId { get; set; }
        public int CurrentLocation { get; set; }
        public AssetTrackingStatusEnum Status { get; set; }
        public int? AssignedTo { get; set; }
        public DateTime? LastMovedOn { get; set; }
        public string? GpsCoordinates { get; set; }
        public string? Remarks { get; set; }
        public int? CreatedBy { get; set; }
    }

    public class AssetTrackingUpdateDto
    {
        public int TenantId { get; set; }
        public int? AssetId { get; set; }
        public long TrackingId { get; set; }
        public int? CurrentLocation { get; set; }
        public AssetTrackingStatusEnum Status { get; set; }
        public int? AssignedTo { get; set; }
        public DateTime? LastMovedOn { get; set; }
        public string? GpsCoordinates { get; set; }
        public string? Remarks { get; set; }

        public DateTime? DownStartTime { get; set; }
        public DateTime? DownEndTime { get; set; }
        public double? TotalDownMinutes { get; set; }
        public double? TotalDownAccumulatedMinutes { get; set; }

        public int? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
