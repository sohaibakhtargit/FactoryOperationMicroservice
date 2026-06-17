using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("ProcessedMessage")]
    public class ProcessedMessage
    {
        [Key]
        [Column("MessageId")]
        public string MessageId { get; set; } = default!;

        [Required]
        [Column("ProcessedAt")]
        public DateTime ProcessedAt { get; set; }
    }
}
