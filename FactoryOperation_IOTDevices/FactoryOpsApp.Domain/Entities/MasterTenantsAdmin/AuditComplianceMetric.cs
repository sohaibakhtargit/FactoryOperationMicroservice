using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Domain.Entities.MasterTenantsAdmin
{
    public class AuditComplianceMetric
    {
        [Key]
        public int Id { get; set; }
        public int TenantId { get; set; }

        public string IsolationLevel { get; set; }
        public string DataRegion { get; set; }
        public string ComplianceLevel { get; set; }
        public DateTime? LastAuditDate { get; set; }
        public decimal? MonthlyMaintenanceCost { get; set; }

        public string ValidationIsolationLevel { get; set; }
        public string ValidationSchemaIsolation { get; set; }
        public string ValidationFileStorage { get; set; }
        public string ValidationApiAccessControl { get; set; }
        public DateTime? ValidationTimestamp { get; set; }

        public decimal? MetricDataIntegrityScore { get; set; }
        public decimal? MetricAccessControl { get; set; }
        public decimal? MetricSchemaIsolation { get; set; }
        public decimal? MetricNetworkSegmentation { get; set; }
        public DateTime? MetricsTimestamp { get; set; }

        public int? AccessAttempts24h { get; set; }
        public int? BlockedRequests24h { get; set; }
        public int? PolicyViolations24h { get; set; }
        public int? DataQueries24h { get; set; }
        public DateTime? SecurityStatsTimestamp { get; set; }

        // Audit
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
