using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.IsolationControl;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.IsolationControl;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.IsolationControl
{
    public class TenantIsolationService : ITenantIsolationService
    {
        private readonly ITenantIsolationRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly IFileStorageService _fileStorageService;

        public TenantIsolationService(ITenantIsolationRepository repository, IConfiguration configuration, IFileStorageService fileStorageService)
        {
            _repository = repository;
            _configuration = configuration;
            _fileStorageService = fileStorageService;
            _fileStorageService = fileStorageService;
        }

        public async Task<CommonResponseModel> AddComplianceAuditAsync(AddComplianceAuditDto dto)
        {
            var entity = new FactoryComplianceAndAudit
            {
                ComplianceType = dto.ComplianceType,
                Description = dto.Description,
                //   LastReviewedOn = dto.LastReviewedOn,
                Status = dto.Status,
                //  IsActive = dto.IsActive,
                IsDeleted = false
            };

            var result = await _repository.AddComplianceAuditAsync(entity);

            return new CommonResponseModel
            {
                StatusCode = result ? "200" : "500",
                StatusMessage = result ? "Compliance audit added successfully." : "Failed to add compliance audit"
            };
        }


        public async Task<CommonResponseModel> AddOrUpdateIsolationAsync(AddTenantIsolationDto dto)
        {
            string? relativePath = null;
            //byte[]? imageBytes = null;
            if (dto.Logo == null)
            {
                throw new ArgumentNullException(nameof(dto.Logo), "Branding Logo is required");
            }

            relativePath = await _fileStorageService.SaveFileAsync(dto.Logo, "Logo");
            //imageBytes = await File.ReadAllBytesAsync(Path.Combine("wwwroot", relativePath));
            var model = new TenantIsolation
            {
                TenantId = dto.TenantId,
                TenantName = dto.TenantName,
                DataEncryption = dto.DataEncryption,
                EncryptionKeyId = dto.EncryptionKeyId,
                CustomBranding = dto.CustomBranding,
                LogoUrl = relativePath,
                ColorScheme = dto.ColorScheme,
                DataPartitionId = dto.DataPartitionId
            };

            var result = await _repository.AddOrUpdateAsync(model);

            return new GetSpecificRecord<object>
            {
                StatusCode = "200",
                StatusMessage = "Success",
                Data = null
            };

        }

        public Task<CommonResponseModel> DeleteComplianceAuditAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<GetAllRecord<GetComplianceAuditDto>> GetAllComplianceAuditsAsync(int tenantId)
        {
            throw new NotImplementedException();
        }

        public Task<GetComplianceAuditDto?> GetComplianceAuditByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<GetTenantIsolationDto?> GetIsolationByTenantIdAsync(int tenantId)
        {
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8107";

            var entity = await _repository.GetByTenantIdAsync(tenantId);
            if (entity == null) return null;

            return new GetTenantIsolationDto
            {
                TenantId = entity.TenantId,
                TenantName = entity.TenantName,
                DataEncryption = entity.DataEncryption,
                EncryptionKeyId = entity.EncryptionKeyId,
                CustomBranding = entity.CustomBranding,
                //LogoUrl = entity.LogoUrl,
                LogoUrl = entity.LogoUrl != null
                            ? $"{baseUrl}/{entity.LogoUrl.Replace("\\", "/")}"
                            : null,
                ColorScheme = entity.ColorScheme,
                DataPartitionId = entity.DataPartitionId
            };
        }
        public async Task<CommonResponseModel> UpdateComplianceAuditAsync(UpdateComplianceAuditDto dto)
        {
            var audit = new FactoryComplianceAndAudit
            {
                Id = dto.Id,
                ComplianceType = dto.ComplianceType,
                Description = dto.Description,
                LastReviewedOn = dto.LastReviewedOn,
                Status = dto.Status,
                IsActive = dto.IsActive
            };

            var result = await _repository.UpdateComplianceAuditAsync(audit);

            return new CommonResponseModel
            {
                StatusCode = "200",
                StatusMessage = result ? "Compliance audit updated successfully." : "Update failed."
            };
        }

        public async Task<CommonResponseModel> UpsertAuditComplianceMetricAsync(UpdateAuditComplianceMetricsDto dto)
        {
            return await _repository.UpsertAuditComplianceMetricAsync(dto);
        }
        public GetAllRecord<GetAuditComplianceMetricsDto?> GetAuditComplianceMetricsAsync()
        {
            return _repository.GetAuditComplianceMetricsAsync();
        }

    }
}

