using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.IsolationControl
{
    public interface ITenantIsolationRepository
    {
        Task<TenantIsolation?> GetByTenantIdAsync(int tenantId);
        Task<bool> AddOrUpdateAsync(TenantIsolation isolation);
        Task<List<FactoryComplianceAndAudit>> GetAllComplianceAuditsAsync(int tenantId);
        Task<FactoryComplianceAndAudit?> GetComplianceAuditByIdAsync(int id);
        Task<bool> AddComplianceAuditAsync(FactoryComplianceAndAudit audit);
        Task<bool> UpdateComplianceAuditAsync(FactoryComplianceAndAudit audit);
        Task<bool> DeleteComplianceAuditAsync(int id);

        //
        Task<CommonResponseModel> UpsertAuditComplianceMetricAsync(UpdateAuditComplianceMetricsDto dto);
        GetAllRecord<GetAuditComplianceMetricsDto?> GetAuditComplianceMetricsAsync();


    }
}
