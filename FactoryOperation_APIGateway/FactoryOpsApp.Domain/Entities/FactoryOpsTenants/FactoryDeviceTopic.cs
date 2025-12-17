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
    [Table("DeviceTopics")]
    public class FactoryDeviceTopic
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int TenantId { get; set; }
        [Required]
        public int DeviceId { get; set; }
        [ForeignKey("DeviceId")]
        public FactoryDevice Device { get; set; } = null!;
        [Required]
        public int TopicId { get; set; }
        [ForeignKey("TopicId")]
        public FactoryMqttTopic Topic { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
