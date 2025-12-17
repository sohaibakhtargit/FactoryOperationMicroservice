using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AssetTrackingStatusEnum
    {
        Active,
        Idle,
        Maintenance,
        Retired,
        Down
    }

    public class AssetTracking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TrackingId { get; set; }

        public int TenantId { get; set; }

        public int AssetId { get; set; }
        [ForeignKey(nameof(AssetId))]
        public AssetRegistry Asset { get; set; }

        public int CurrentLocation { get; set; }
        [ForeignKey(nameof(CurrentLocation))]
        public Location Location { get; set; }

        [Required]
        [MaxLength(30)]
        public AssetTrackingStatusEnum Status { get; set; }

        [Column("AssignedTo")]
        public int? AssignedTo { get; set; }

        [ForeignKey(nameof(AssignedTo))]
        public FactoryUsers? AssignedUser { get; set; }

        public DateTime? LastMovedOn { get; set; }
        public string? GpsCoordinates { get; set; }
        public DateTime StatusUpdatedOn { get; set; } = DateTime.UtcNow;
        public string? Remarks { get; set; }

        public DateTime? DownStartTime { get; set; }
        public DateTime? DownEndTime { get; set; }
        public double? TotalDownMinutes { get; set; }
        public double? TotalDownAccumulatedMinutes { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
