using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FactoryOperation_Asset.FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class AssetLifecycleMappings

    {

        [Key]

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public long MappingId { get; set; }

        public long LifecycleId { get; set; }

        [ForeignKey(nameof(LifecycleId))]

        public virtual AssetLifecycle AssetLifecycles { get; set; }

        public int AssetId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]

        public AssetLifecycleStageEnum? AssetStage { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }

}
