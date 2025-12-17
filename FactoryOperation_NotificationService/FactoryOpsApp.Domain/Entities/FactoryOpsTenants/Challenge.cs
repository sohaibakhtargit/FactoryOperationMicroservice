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

    [Table("Challenges")]
    public class Challenge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChallengeId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))] 
        public ChallengeType ChallengeType { get; set; }
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required, MaxLength(500)]
        public string Reward { get; set; }

        public string Requirements { get; set; }
        //public int ParticipantIds { get; set; }
        public List<int>? ParticipantIds { get; set; }
        public string ProgressData { get; set; }
        //  public int Status { get; set; } = 1;
        public int? MaxParticipants { get; set; }

        [Required]
        public int TenantId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? TeamId {  get; set; }
        [ForeignKey("TeamId")]
        public FactoryUsers? Team { get; set; }

    }
    public enum ChallengeType
    {
        Individual,
        Team,
        OrganizationWide
    }

}
