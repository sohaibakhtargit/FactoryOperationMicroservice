using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.AssetManagement
{
    public class AssetManagementRepository : IAssetManagementRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        private readonly IFileStorageService _fileStorageService;
        private readonly IConfiguration _configuration;
        public AssetManagementRepository(TenantDbContextFactory tenantDbContext,
            MasterFactoryOpsDbContext masterDbcontext,
            IHttpContextAccessor httpContextAccessor,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger,
             IConfiguration configuration,
             IFileStorageService fileStorageService

            )
        {
            _tenantDbContext = tenantDbContext;
            _masterDbcontext = masterDbcontext;
            _httpContextAccessor = httpContextAccessor;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
            _configuration = configuration;
            _fileStorageService = fileStorageService;

        }
        public async Task<CommonResponseModel> AddAsset(AssetRegistryDto dto)
        {
            CommonResponseModel response = new();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                var exists = await tenantDb.AssetRegistry.AnyAsync(a =>
                   !a.IsDeleted &&
                   (
                       a.SerialNumber != null && dto.SerialNumber != null &&
                        a.SerialNumber.ToLower() == dto.SerialNumber.ToLower()
                       ||
                       a.AssetUniqueId != null && dto.AssetUniqueId != null &&
                        a.AssetUniqueId.ToLower() == dto.AssetUniqueId.ToLower()
                   ));

                if (exists)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = AssetManagementStatusMessage.DuplicateUniqueId;
                    return response;
                }
                string? relativePath = null;
                if (dto.DocumentFile != null)
                {
                    relativePath = await _fileStorageService.SaveFileAsync(dto.DocumentFile, "AssetDocuments");
                    var imageBytes = await File.ReadAllBytesAsync(Path.Combine("wwwroot", relativePath));

                }

                var asset = new AssetRegistry
                {
                    AssetName = dto.AssetName,
                    AssetTypeId = dto.AssetTypeId,
                    TenantId = dto.TenantId,
                    Model = dto.Model ?? null,
                    SerialNumber = dto.SerialNumber,
                    CategoryHierarchy = dto.CategoryHierarchy ?? null,
                    LocationId = dto.LocationId,
                    Department = dto.Department ?? null,
                    Vendor = dto.Vendor ?? null,
                    Supplier = dto.Supplier ?? null,
                    Manufacturer = dto.Manufacturer ?? null,
                    ExpectedLifespan = dto.ExpectedLifespan ?? null,
                    DepreciationRule = dto.DepreciationRule ?? null,
                    Power = dto.Power ?? null,
                    Criticality = dto.Criticality ?? null,
                    DocumentFile = dto.DocumentFile?.FileName ?? null,
                    DocumentUrl = relativePath,
                    WarrantyExpiry = dto.WarrantyExpiry ?? null,
                    InsurancePolicyNumber = dto.InsurancePolicyNumber ?? null,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy
                };

                await tenantDb.AssetRegistry.AddAsync(asset);
                await tenantDb.SaveChangesAsync();

                var tracking = new AssetTracking
                {
                    AssetId = asset.AssetId,
                    CurrentLocation = dto.LocationId,
                    TenantId = dto.TenantId,
                    Status = AssetTrackingStatusEnum.Active,
                    AssignedTo = dto.AssignedTo ?? null,
                    LastMovedOn =  DateTime.UtcNow,
                    GpsCoordinates = "",
                    Remarks = "",
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await tenantDb.AssetTracking.AddAsync(tracking);
                await tenantDb.SaveChangesAsync();

                await transaction.CommitAsync();

                var ctx = _httpContextAccessor.HttpContext;
                var auditData = new Audit_Log_MasterDb
                {
                    Action = "Create",
                    Details = $"Add-Asset ({dto.AssetName})",
                    EventType = "AssetCreated",
                    TenantId = dto.TenantId,
                    Email = "",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection.RemoteIpAddress?.ToString(),
                };

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);
                await _masterDbcontext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetManagementStatusMessage.AssetAdded;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); 

                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AssetModule",
                    apiName: "AddAsset",
                    tenantId: dto?.TenantId,
                    userId: dto?.CreatedBy
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetManagementStatusMessage.AddFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateAsset(AssetRegistryDto dto)
         {
             CommonResponseModel response = new();

             using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
             using var transaction = await tenantDb.Database.BeginTransactionAsync();

             try
             {
                 var asset = await tenantDb.AssetRegistry
                     .FirstOrDefaultAsync(a => a.AssetId == dto.AssetId);

                 if (asset == null)
                 {
                     response.StatusCode = StatusCode.NotFound;
                     response.StatusMessage = AssetManagementStatusMessage.AssetNotFound;
                     return response;
                 }

                 if (!string.IsNullOrEmpty(dto.SerialNumber))
                 {
                     var serialExists = await tenantDb.AssetRegistry
                         .AnyAsync(a =>
                             a.AssetId != dto.AssetId &&
                             !a.IsDeleted &&
                             a.SerialNumber != null &&
                             a.SerialNumber.ToLower() == dto.SerialNumber.ToLower());

                     if (serialExists)
                     {
                         response.StatusCode = StatusCode.BadRequest;
                         response.StatusMessage = AssetManagementStatusMessage.DuplicateSerialNumber;
                         return response;
                     }
                 }

                 if (!string.IsNullOrEmpty(dto.AssetUniqueId))
                 {
                     var uniqueIdExists = await tenantDb.AssetRegistry
                         .AnyAsync(a =>
                             a.AssetId != dto.AssetId &&
                             !a.IsDeleted &&
                             a.AssetUniqueId != null &&
                             a.AssetUniqueId.ToLower() == dto.AssetUniqueId.ToLower());

                    if (uniqueIdExists)
                    {
                        response.StatusCode = StatusCode.BadRequest;
                        response.StatusMessage = AssetManagementStatusMessage.DuplicateUniqueId;
                        return response;
                    }
                   
                }
                string? relativePath = null;
                byte[]? fileBytes = null;

                if (dto.DocumentFile != null)
                {
                    relativePath = await _fileStorageService.SaveFileAsync(dto.DocumentFile, "AssetDocuments");
                    var fullPath = Path.Combine("wwwroot", relativePath);


                    if (File.Exists(fullPath))
                        fileBytes = await File.ReadAllBytesAsync(fullPath);
                    asset.DocumentFile = dto.DocumentFile?.FileName ?? null;
                    asset.DocumentUrl = relativePath;

                    asset.DocumentFile = dto.DocumentFile.FileName;
                    asset.DocumentUrl = relativePath;               

                }
                
                asset.AssetName = dto.AssetName;
                asset.AssetTypeId = dto.AssetTypeId;
                asset.TenantId = dto.TenantId;
                asset.Model = dto.Model ?? null;
                asset.SerialNumber = dto.SerialNumber;
                asset.AssetUniqueId = dto.AssetUniqueId;
                asset.CategoryHierarchy = dto.CategoryHierarchy ?? null;
                asset.LocationId = dto.LocationId;
                asset.Department = dto.Department ?? null;
                asset.Vendor = dto.Vendor ?? null;
                asset.Supplier = dto.Supplier ?? null  ;
                asset.Manufacturer = dto.Manufacturer ?? null;
                asset.ExpectedLifespan = dto.ExpectedLifespan ?? null;
                asset.DepreciationRule = dto.DepreciationRule ?? null;
                asset.Power = dto.Power;
                asset.Criticality = dto.Criticality?? null;
                asset.WarrantyExpiry = dto.WarrantyExpiry;
                asset.InsurancePolicyNumber = dto.InsurancePolicyNumber;


                 asset.UpdatedAt = DateTime.UtcNow;
                 asset.UpdatedBy = dto.UpdatedBy;

                 var tracking = await tenantDb.AssetTracking
                     .FirstOrDefaultAsync(t => t.AssetId == dto.AssetId);

                 if (tracking != null)
                 {
                     tracking.AssignedTo = dto.AssignedTo;
                     tracking.CurrentLocation = dto.LocationId;
                     tracking.UpdatedAt = DateTime.UtcNow;
                     tracking.UpdatedBy = dto.UpdatedBy;
                 }

                 await tenantDb.SaveChangesAsync();
                 await transaction.CommitAsync();

                 var ctx = _httpContextAccessor.HttpContext;
                 var auditData = new Audit_Log_MasterDb
                 {
                     Action = "Update",
                     Details = $"Updated asset '{asset.AssetName}' (ID: {dto.AssetId})",
                     EventType = "AssetUpdate",
                     TenantId = dto.TenantId,
                     Email = ctx?.User?.Identity?.Name ?? "System",
                     Timestamp = DateTime.UtcNow,
                     IsActive = true,
                     IsDeleted = false,
                     UserName = Environment.UserName,
                     Ipaddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
                 };

                 await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);
                 await _masterDbcontext.SaveChangesAsync();

                 response.StatusCode = StatusCode.Success;
                 response.StatusMessage = AssetManagementStatusMessage.AssetUpdated;
             }
             catch (Exception ex)
             {
                 await transaction.RollbackAsync();

                 await _exceptionLogger.LogExceptionAsync(
                     ex,
                     sourceModule: "AssetModule",
                     apiName: "UpdateAsset",
                     tenantId: dto?.TenantId,
                     userId: dto?.UpdatedBy
                 );

                 response.StatusCode = StatusCode.Error;
                 response.StatusMessage = $"{AssetManagementStatusMessage.UpdateFailed}: {ex.Message}";
             }

             return response;
         }
        public GetAllRecord<GetAssetRegistryDto> GetAllAssets(int tenantId)
        {
            GetAllRecord<GetAssetRegistryDto> response = new();

            try
            {
                var tenant = _masterDbcontext.FactoryTenants
                    .FirstOrDefault(l => l.TenantId == tenantId && !l.IsDeleted);

                if (tenant == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetManagementStatusMessage.TenantNotFound;
                    return response;
                }

                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8107";

                var assets = tenantDb.AssetRegistry
                    .Include(a => a.FactoryAssetType)
                    .Include(a => a.Location)
                    .Where(a => !a.IsDeleted)
                    .AsEnumerable()
                    .Select(a => new GetAssetRegistryDto
                    {
                        AssetId = a.AssetId,
                        AssetName = a.AssetName,
                        AssetTypeId = a.AssetTypeId ?? 0,
                        TenantId = a.TenantId,
                        AssetTypeName = a.FactoryAssetType?.Type_Name,
                        Model = a.Model,
                        SerialNumber = a.SerialNumber,
                        AssetUniqueId = a.AssetUniqueId,
                        CategoryHierarchy = a.CategoryHierarchy,
                        LocationId = a.LocationId,
                        LocationName = a.Location?.LocationName,
                        Department = a.Department,
                        Vendor = a.Vendor,
                        Supplier = a.Supplier,
                        Manufacturer = a.Manufacturer,
                        PurchaseDate = a.PurchaseDate,
                        AcquisitionCost = a.AcquisitionCost,
                        WarrantyExpiry = a.WarrantyExpiry,
                        ExpectedLifespan = a.ExpectedLifespan,
                        DepreciationRule = a.DepreciationRule,
                        Power = a.Power,
                        Criticality = a.Criticality,
                        InsurancePolicyNumber = a.InsurancePolicyNumber,
                        DocumentUrl = a.DocumentUrl != null
                                        ? $"{baseUrl}/{a.DocumentUrl.Replace("\\", "/")}"
                                        : null,
     
                        IsActive = a.IsActive,
                        CreatedAt = a.CreatedAt,
                        CreatedBy = a.CreatedBy,
                        UpdatedAt = a.UpdatedAt,
                        UpdatedBy = a.UpdatedBy
                    })
                    .ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetManagementStatusMessage.AssetsFetched;
                response.GetAllData = assets;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AssetModule",
                    apiName: "GetAllAssets",
                    tenantId: tenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetManagementStatusMessage.FetchFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> DeleteAsset(int id, int tenantId)
        {
            CommonResponseModel response = new();

            using var transaction = await _masterDbcontext.Database.BeginTransactionAsync();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var asset = await tenantDb.AssetRegistry
                    .FirstOrDefaultAsync(a => a.AssetId == id && !a.IsDeleted);

                if (asset == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetManagementStatusMessage.AssetNotFound;
                    return response;
                }

                asset.IsDeleted = true;
                asset.IsActive = false;
                asset.DeletedAt = DateTime.UtcNow;
                asset.DeletedBy = tenantId;

                var trackings = await tenantDb.AssetTracking
                    .Where(t => t.AssetId == id && !t.IsDeleted)
                    .ToListAsync();

                foreach (var tracking in trackings)
                {
                    tracking.IsDeleted = true;
                    tracking.IsActive = false;
                }

                var ctx = _httpContextAccessor.HttpContext;
                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Delete",
                    Details = $"Deleted asset '{asset.AssetName}' (ID: {id})",
                    EventType = "AssetDelete",
                    TenantId = tenantId,
                    Email = ctx?.User?.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
                };

                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);

                await tenantDb.SaveChangesAsync();
                await _masterDbcontext.SaveChangesAsync();
                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetManagementStatusMessage.AssetDeleted;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(
                        ex,
                        sourceModule: "AssetModule",
                        apiName: "DeleteAsset",
                        tenantId: tenantId,
                        userId: null
                    );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetManagementStatusMessage.DeleteFailed}: {ex.Message}";
            }

            return response;
        }

    }

}
