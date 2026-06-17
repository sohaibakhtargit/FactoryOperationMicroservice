using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.TenantManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.TenantManagement
{
    public class CompanyBrandingInfoRepository : ICompanyBrandingInfoRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        private readonly IConfiguration _configuration;
        private readonly IFileStorageService _fileStorageService;

        public CompanyBrandingInfoRepository(
            MasterFactoryOpsDbContext masterDbcontext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger,
            IConfiguration configuration,
            IFileStorageService fileStorageService)
        {
            _masterDbcontext = masterDbcontext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
            _configuration = configuration;
            _fileStorageService = fileStorageService;
        }
        public async Task<CommonResponseModel> CreateCompanyBrandingAsync(CreateCompanyBrandingDto dto)
        {
            var response = new CommonResponseModel();

            using var transaction = await _masterDbcontext.Database.BeginTransactionAsync();

            try
            {
                if (await _masterDbcontext.CompanyBrandingInfo.AnyAsync(c =>
                    c.CompanyName.ToLower() == dto.CompanyName.ToLower() && !c.IsDeleted))
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = CompanyBrandingInfoStatusMessage.BadRequest;
                    return response;
                }

                string? companyImagePath = null;
                string? companyNameLogoPath = null;
                byte[]? imageBytes = null;

                
                if (dto.CompanyImageFile != null)
                {
                    companyImagePath = await _fileStorageService.SaveFileAsync(dto.CompanyImageFile, "CompanyImages");
                    imageBytes = await File.ReadAllBytesAsync(Path.Combine("wwwroot", companyImagePath));
                }

               
                if (dto.CompanyNameLogo != null)
                {
                    companyNameLogoPath = await _fileStorageService.SaveFileAsync(dto.CompanyNameLogo, "CompanyNameLogos");
                }

                var newCompanyBranding = new CompanyBrandingInfo
                {
                    CompanyName = dto.CompanyName,

                    // ✅ Correct: store saved file path (not FileName)
                    CompanyImage = companyImagePath,

                    // (Leaving as-is since already exists in your model)
                    CompanyLogo = companyImagePath,

                    CompanyNameLogo = companyNameLogoPath,

                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy
                };

                _masterDbcontext.CompanyBrandingInfo.Add(newCompanyBranding);
                await _masterDbcontext.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    "Create",
                    $"Company branding '{dto.CompanyName}' created with {(companyImagePath != null ? "Image" : "No Image")} and {(companyNameLogoPath != null ? "Text Logo" : "No Text Logo")}",
                    null,
                    null,
                    "CompanyBrandingModule"
                );

                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = CompanyBrandingInfoStatusMessage.AddCompanyBranding;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "CompanyBrandingModule",
                    "CreateCompanyBranding",
                    null,
                    null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{CompanyBrandingInfoStatusMessage.AddCompanyBrandingFailed}: {ex.Message}";
            }

            return response;
        }

        public async Task<CommonResponseModel> UpdateCompanyBrandingAsync(UpdateCompanyBrandingDto dto)
        {
            var response = new CommonResponseModel();

            using var transaction = await _masterDbcontext.Database.BeginTransactionAsync();

            try
            {
                var existingCompany = await _masterDbcontext.CompanyBrandingInfo
                    .FirstOrDefaultAsync(c => c.Id == dto.Id && !c.IsDeleted);

                if (existingCompany == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = CompanyBrandingInfoStatusMessage.FetchFailed;
                    return response;
                }

                if (await _masterDbcontext.CompanyBrandingInfo.AnyAsync(c =>
                    c.Id != dto.Id &&
                    c.CompanyName.ToLower() == dto.CompanyName.ToLower() &&
                    !c.IsDeleted))
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = CompanyBrandingInfoStatusMessage.BadRequest;
                    return response;
                }

                
                if (dto.CompanyImageFile != null)
                {
                    var imagePath = await _fileStorageService.SaveFileAsync(dto.CompanyImageFile, "CompanyImages");
                    existingCompany.CompanyImage = imagePath;
                    existingCompany.CompanyLogo = imagePath;
                }

                
                if (dto.CompanyNameLogo != null)
                {
                    var nameLogoPath = await _fileStorageService.SaveFileAsync(dto.CompanyNameLogo, "CompanyNameLogos");
                    existingCompany.CompanyNameLogo = nameLogoPath;
                }

                existingCompany.CompanyName = dto.CompanyName;
                existingCompany.UpdatedAt = DateTime.UtcNow;
                existingCompany.UpdatedBy = dto.UpdatedBy;

                await _auditLogger.LogAuditAsync(
                    "Update",
                    $"Company branding '{dto.CompanyName}' updated",
                    null,
                    "",
                    "CompanyBrandingModule"
                );

                await _masterDbcontext.SaveChangesAsync();
                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = CompanyBrandingInfoStatusMessage.CompanyBrandingUpdated;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "CompanyBrandingModule",
                    "UpdateCompanyBranding",
                    null,
                    null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{CompanyBrandingInfoStatusMessage.UpdatedFailed}: {ex.Message}";
            }

            return response;
        }

        public GetAllRecord<CompanyBrandingResponseDto> GetAllCompanyBrandings()
        {
            var response = new GetAllRecord<CompanyBrandingResponseDto>();
            try
            {
                string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8128";
                var companyBrandings = _masterDbcontext.CompanyBrandingInfo
                    .Where(c => !c.IsDeleted)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CompanyBrandingResponseDto
                    {
                        Id = c.Id,
                        CompanyName = c.CompanyName,
                        CompanyNameLogo = c.CompanyNameLogo != null
                       ? $"{baseUrl}/{c.CompanyNameLogo.Replace("\\", "/")}"
                         : null,
                        CompanyImage = c.CompanyImage != null ? $"/api/companybranding/image/{c.Id}" : null,
                        CompanyLogo = c.CompanyLogo != null
                        ? $"{baseUrl}/{c.CompanyLogo.Replace("\\", "/")}"
                            : null,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = CompanyBrandingInfoStatusMessage.DataFetched;
                response.GetAllData = companyBrandings;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    "CompanyBrandingModule",
                    "GetAllCompanyBrandings",
                    null,
                    null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{CompanyBrandingInfoStatusMessage.Error}: {ex.Message}";
            }

            return response;
        }

    }
}
