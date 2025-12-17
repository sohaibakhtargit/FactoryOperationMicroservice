using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeviceCategoryEnum
    {
        Environmental,
        Mechanical,
        Hydraulic,
        Fluid,
        Electrical,
        Safety,
        QualityControl
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeviceStatusEnum
    {
        Online,
        Offline,
        Error,
        Maintenance,
        ConfigurationUpdated,
        Unknown
    }

    public class FactoryDevice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeviceId { get; set; }
        [Required]
        public int TenantId { get; set; }
        [Required]
        public string DeviceName { get; set; } = null!;
        [Required]
        public string DeviceCode { get; set; } = null!;
        public DeviceCategoryEnum? Category { get; set; }
        [Required]
        public DeviceStatusEnum Status { get; set; } = DeviceStatusEnum.Offline;
        public DateTime? LastSeen { get; set; }
        public int? LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Location? Location { get; set; }
        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public FactoryGroup? Group { get; set; }
        public string DataFormat { get; set; } = "JSON";
        public string? PhysicalLocation { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public ICollection<FactoryDeviceTopic>? DeviceTopics { get; set; }
    }
}
