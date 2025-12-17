using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.IsolationControl
{
    public interface ITenantIsolationService
    {
        Task<CommonResponseModel> AddOrUpdateIsolationAsync(AddTenantIsolationDto dto);
        Task<GetTenantIsolationDto?> GetIsolationByTenantIdAsync(int tenantId);


        // Compliance & Audit methods 
        Task<CommonResponseModel> AddComplianceAuditAsync(AddComplianceAuditDto dto);
        Task<CommonResponseModel> UpdateComplianceAuditAsync(UpdateComplianceAuditDto dto);

        Task<CommonResponseModel> DeleteComplianceAuditAsync(int id);
        Task<GetAllRecord<GetComplianceAuditDto>> GetAllComplianceAuditsAsync(int tenantId);
        Task<GetComplianceAuditDto?> GetComplianceAuditByIdAsync(int id);

        //
        Task<CommonResponseModel> UpsertAuditComplianceMetricAsync(UpdateAuditComplianceMetricsDto dto);
        GetAllRecord<GetAuditComplianceMetricsDto?> GetAuditComplianceMetricsAsync();
    }
}