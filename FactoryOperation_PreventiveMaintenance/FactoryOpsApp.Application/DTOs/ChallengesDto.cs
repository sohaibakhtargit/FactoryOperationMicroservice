using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class ChallengesDto
    {
        public int TenantId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public ChallengeType ChallengeType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reward { get; set; }
        public string? Requirements { get; set; }
       // public List<int>? ParticipantIds { get; set; }
        public string? ProgressData { get; set; }
        public int? TeamId { get; set; }
        public int? MaxParticipants { get; set; }


        // public int Status { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class GetChallengesCardDetails
    {
        public int? ChallengeId { get; set; }
        public int? TenantId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ChallengeType { get; set; } 
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysLeft { get; set; } 
        public int? ParticipantsCount { get; set; } 
        public int? MaxParticipants { get; set; } 
        public string? Reward { get; set; }
        public List<string>? Requirements { get; set; } = new();
        public string? TeamName { get; set; } 
    }
    public class ChallengeBoardData
    {
        public List<GetChallengesCardDetails>? UpcomingChallenges { get; set; } = new();
        public List<GetChallengesCardDetails>? OngoingChallenges { get; set; } = new();
        public List<GetChallengesCardDetails>? CompletedChallenges { get; set; } = new();
    }
}
