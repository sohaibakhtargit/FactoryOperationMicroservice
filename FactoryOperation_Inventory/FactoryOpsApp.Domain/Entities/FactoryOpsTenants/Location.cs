using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LocationTypeEnum
    {
        Campus,
        Building,
        Floor,
        Room,
        Area,
        Section
    }

    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LocationId { get; set; }
        public int TenantId { get; set; }
        public string LocationName { get; set; } = null!;
        public LocationTypeEnum LocationType { get; set; }
        public int? ParentLocationId { get; set; }
        [ForeignKey("ParentLocationId")]
        public Location? ParentLocation { get; set; }
        public ICollection<Location>? ChildLocations { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }

}
