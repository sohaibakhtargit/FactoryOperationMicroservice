using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    [Table("TrainingModules")]
    public class TrainingModule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TrainingModuleId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }

        public TrainingType? ModuleType { get; set; }

        public DifficultyLevel? Difficulty { get; set; }

        public int? DurationMinutes { get; set; }

        public TrainingCategory? Category { get; set; }

        public string? Prerequisites { get; set; }
        public string? LearningObjectives { get; set; }

        public int[]? EnrolledUserIds { get; set; }
        public int[]? CompletedUserIds { get; set; }

        public int? CompletionRate { get; set; } = 0;

        [Required]
        public int TenantId { get; set; }
        public bool IsActive { get; set; } = true;

        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    public enum DifficultyLevel
    {
        Beginner,
        Immediate,
        Advanced
    }
    public enum TrainingType
    {
        [Display(Name = "Video Training")]
        VideoTraining,

        [Display(Name = "Document/PDF")]
        DocumentPDF,

        [Display(Name = "Quiz/Assessment")]
        QuizAssessment,

        [Display(Name = "Interactive")]
        Interactive
    }
    public enum TrainingCategory
    {
        Safety,
        Technical,
        Quality,
        Compliance,
        Leadership
    }
}
