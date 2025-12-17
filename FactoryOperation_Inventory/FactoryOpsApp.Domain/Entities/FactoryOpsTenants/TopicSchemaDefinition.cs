using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("TopicSchemaDefinitions")]
    public class TopicSchemaDefinition
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SchemaDefinitionId { get; set; }

        [Required]
        public int TopicId { get; set; }
       

        [Required]
        [ForeignKey("TopicId")]
        [JsonIgnore]
        public virtual FactoryMqttTopic Topic { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string KeyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public DataTypeEnum DataType { get; set; } = DataTypeEnum.JSON;

        [Required]
        public int TenantId { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int? DeletedBy { get; set; }
    }
}