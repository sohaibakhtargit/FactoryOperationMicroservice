using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.FactoryOpsTenants
{
    public class AnalyticsTeamPerformance
    {
        public int AnalyticsId {  get; set; }
        public int TenantId {  get; set; }
        public int TeamId {  get; set; }
        public string TeamName {  get; set; }
        public string TeamMember {  get; set; }
        public int TeamMemberId { get;set; }
        public int ActiveTask {  get; set; }
        public int ActiveTaskId { get; set; }
        public int ActiveTaskCount { get; set; }
        public int OverdueTask { get; set; }
        public decimal AverageTaskTime { get; set; }
        public int EfficiencyScore {  get; set; }

    }
}
