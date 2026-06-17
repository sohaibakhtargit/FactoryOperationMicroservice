using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GroupTypeEnum
    {
        Area,
        Production,
        QualityControl,
        Maintenance,
        Safety,
        ResearchAndDevelopment,
        Logistics,
        Department,
        Line,
        Building,
        Team,
        Section,
        Zone
    }
    public class FactoryGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupId { get; set; }
        [Required]
        public int TenantId { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public GroupTypeEnum Type { get; set; }
        public int? LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Location? Location { get; set; }
        public int? ParentGroupId { get; set; }
        [ForeignKey("ParentGroupId")]
        public FactoryGroup? ParentGroup { get; set; }
        public int DeviceCount { get; set; } = 0;
        public int UserCount { get; set; } = 0;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public ICollection<FactoryGroup>? ChildGroups { get; set; }
        public ICollection<FactoryDevice>? Devices { get; set; }
        public ICollection<FactoryGroupUser>? GroupUsers { get; set; }
    }
}
