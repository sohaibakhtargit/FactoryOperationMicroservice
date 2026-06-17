using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.AssetManagement
{
    public class AssetDocumentRepository : IAssetDocumentRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly IConfiguration _configuration;

        private readonly IAuditLogService _auditLogger;
        private readonly IFileStorageService _fileStorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AssetDocumentRepository(IConfiguration configuration,
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
             MasterFactoryOpsDbContext masterDbcontext,
             IHttpContextAccessor httpContextAccessor,
            IAuditLogService auditLogger, IFileStorageService fileStorageService)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
            _masterDbcontext = masterDbcontext;
            _configuration = configuration;
            _fileStorageService = fileStorageService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CommonResponseModel> AddAssetDocumentAsync(CreateAssetDocumentDto dto)
        {
            CommonResponseModel response = new();
            int documentId = 0;

            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var tran = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                var existing = await tenantDb.AssetDocuments
                    .FirstOrDefaultAsync(d =>
                        d.AssetId == dto.AssetId &&
                        d.DocumentTitle == dto.DocumentTitle &&
                        d.IsDeleted == false);

                if (existing != null)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = AssetDocumentStatusMessage.DuplicateTitle;
                    return response;
                }

                string? relativePath = null;
                byte[]? fileBytes = null;
                string? originalFileName = null;
                string? fileExtension = null;
                long? fileSize = null;

                if (dto.DocumentFile == null)
                {
                    throw new ArgumentNullException(nameof(dto.DocumentFile), AssetDocumentStatusMessage.FileRequired);
                }
                
                    relativePath = await _fileStorageService.SaveFileAsync(dto.DocumentFile, "AssetDocuments");
                    originalFileName = dto.DocumentFile.FileName;
                    fileExtension = Path.GetExtension(dto.DocumentFile.FileName);
                    fileSize = dto.DocumentFile.Length;

                var assetDoc = new AssetDocuments
                {
                    TenantId = dto.TenantId,
                    AssetId = dto.AssetId,
                    DocumentTitle = dto.DocumentTitle,
                    DocumentType = dto.DocumentType,
                    Category = dto.Category,
                    Description = dto.Description,
                    ExpiryDate = dto.ExpiryDate,
                    ReminderDaysBeforeExpiry = dto.ReminderDaysBeforeExpiry,
                    ComplianceFlag = dto.ComplianceFlag,
                    FilePath = relativePath,
                    OriginalFileName = originalFileName ?? string.Empty,
                    FileExtension = fileExtension ?? string.Empty,
                    FileSize = fileSize,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy,
                    UploadedOn = DateTime.UtcNow,
                    Status = DocumentStatus.Active,
                    IsActive = true,
                    IsDeleted = false
                };

                await tenantDb.AssetDocuments.AddAsync(assetDoc);
                await tenantDb.SaveChangesAsync();
                documentId = (int)assetDoc.DocumentId;

                var ctx = _httpContextAccessor.HttpContext;
                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Create",
                    Details = $"Asset Document Added: {dto.DocumentTitle}",
                    EventType = "AssetDocument",
                    TenantId = dto.TenantId,
                    Email = "",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection.RemoteIpAddress?.ToString()
                };

                await tenantDb.SaveChangesAsync();
                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(auditData);
                await _masterDbcontext.SaveChangesAsync();

                await tran.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetDocumentStatusMessage.DocumentAdded;
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AssetDocumentModule",
                    apiName: "Add-Document",
                    tenantId: dto.TenantId,
                    userId: dto.CreatedBy
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetDocumentStatusMessage.AddFailed}: {ex.Message}";
            }

            return response;
        }
        public GetAllRecord<AssetDocumentResponseDto> GetAllAssetDocuments(int tenantId)
        {
            GetAllRecord<AssetDocumentResponseDto> response = new();
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8107";
            try
            {
                var tenant = _masterDbcontext.FactoryTenants
                    .FirstOrDefault(t => t.TenantId == tenantId && !t.IsDeleted);

                if (tenant == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetDocumentStatusMessage.TenantNotFound;
                    return response;
                }

                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var documents = tenantDb.AssetDocuments
                    .Where(d => !d.IsDeleted).Include(d => d.Asset)
                    .AsEnumerable()
                    .Select(d => new AssetDocumentResponseDto
                    {
                        DocumentId = d.DocumentId,
                        AssetId = d.AssetId,
                        AssetName = d.Asset != null ? d.Asset.AssetName : string.Empty,
                        TenantId = d.TenantId,
                        DocumentTitle = d.DocumentTitle,
                        DocumentType = d.DocumentType,
                        Category = d.Category,
                        Description = d.Description,
                        ExpiryDate = d.ExpiryDate,
                        ReminderDaysBeforeExpiry = d.ReminderDaysBeforeExpiry,
                        ComplianceFlag = d.ComplianceFlag,
                        FilePath = d.FilePath,
                        DocumentUrl = d.FilePath != null
                            ? $"{baseUrl}/{d.FilePath.Replace("\\", "/")}"
                            : null,
                        OriginalFileName = d.OriginalFileName,
                        FileExtension = d.FileExtension,
                        FileSize = d.FileSize,
                        UploadedOn = d.UploadedOn,
                        Status = d.Status,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt,
                    })
                    .ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetDocumentStatusMessage.DocumentsFetched;
                response.GetAllData = documents;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AssetDocumentModule",
                    apiName: "GetAllAssetDocuments",
                    tenantId: tenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetDocumentStatusMessage.FetchFailed}: {ex.Message}";
            }

            return response;
        }
        public GetAllRecord<AssetDocumentResponseDto> GetAssetDocumentsByAssetId(int tenantId, int assetId)
        {
            GetAllRecord<AssetDocumentResponseDto> response = new();
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8107";
            try
            {
                var tenant = _masterDbcontext.FactoryTenants
                    .FirstOrDefault(t => t.TenantId == tenantId && !t.IsDeleted);

                if (tenant == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetDocumentStatusMessage.TenantNotFound;
                    return response;
                }

                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var documents = tenantDb.AssetDocuments
                    .Where(d => d.AssetId == assetId && !d.IsDeleted).Include(d => d.Asset)
                    .AsEnumerable()
                    .Select(d => new AssetDocumentResponseDto
                    {
                        DocumentId = d.DocumentId,
                        AssetId = d.AssetId,
                        AssetName = d.Asset != null ? d.Asset.AssetName : string.Empty,
                        TenantId = d.TenantId,
                        DocumentTitle = d.DocumentTitle,
                        DocumentType = d.DocumentType,
                        Category = d.Category,
                        Description = d.Description,
                        ExpiryDate = d.ExpiryDate,
                        ReminderDaysBeforeExpiry = d.ReminderDaysBeforeExpiry,
                        ComplianceFlag = d.ComplianceFlag,
                        FilePath = d.FilePath,
                        DocumentUrl = d.FilePath != null
                            ? $"{baseUrl}/{d.FilePath.Replace("\\", "/")}"
                            : null,
                        OriginalFileName = d.OriginalFileName,
                        FileExtension = d.FileExtension,
                        FileSize = d.FileSize,
                        UploadedOn = d.UploadedOn,
                        Status = d.Status,
                        CreatedAt = d.CreatedAt,
                        //CreatedBy = d.CreatedBy,
                        UpdatedAt = d.UpdatedAt,
                        //UpdatedBy = d.UpdatedBy
                    })
                    .ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetDocumentStatusMessage.DocumentsFetched;
                response.GetAllData = documents;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AssetDocumentModule",
                    apiName: "GetAssetDocumentsByAssetId",
                    tenantId: tenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetDocumentStatusMessage.FetchFailed}:{ex.Message}";
            }

            return response;
        }
        public GetAllRecord<AssetDocumentResponseDto> GetAllAssetDocumentsCompliance(int tenantId)
        {
            GetAllRecord<AssetDocumentResponseDto> response = new();
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8107";
            try
            {
                var tenant = _masterDbcontext.FactoryTenants
                    .FirstOrDefault(t => t.TenantId == tenantId && !t.IsDeleted);

                if (tenant == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetDocumentStatusMessage.TenantNotFound;
                    return response;
                }

                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var documents = tenantDb.AssetDocuments
         .Where(d => !d.IsDeleted && d.Category == DocumentCategory.Compliance).Include(d => d.Asset )
         .OrderBy(d => d.ExpiryDate ?? DateTime.MaxValue)
         .Take(3)
         .Select(d => new AssetDocumentResponseDto
         {
             DocumentId = d.DocumentId,
             TenantId = d.TenantId,
             AssetId = d.AssetId,
             AssetName = d.Asset != null ? d.Asset.AssetName : string.Empty,
             DocumentTitle = d.DocumentTitle ?? string.Empty,
             DocumentType = d.DocumentType,
             Category = d.Category,
             Description = d.Description ?? string.Empty,
             FilePath = d.FilePath ?? string.Empty,
             DocumentUrl = d.FilePath != null
                            ? $"{baseUrl}/{d.FilePath.Replace("\\", "/")}"
                            : null,
             OriginalFileName = d.OriginalFileName ?? string.Empty,
             FileExtension = d.FileExtension ?? string.Empty,
             FileSize = d.FileSize,
             ExpiryDate = d.ExpiryDate,
             ReminderDaysBeforeExpiry = d.ReminderDaysBeforeExpiry,
             Status = d.Status,
             ComplianceFlag = d.ComplianceFlag,
             UploadedBy = d.UploadedBy ?? string.Empty,
             UploadedOn = d.UploadedOn,
             CreatedAt = d.CreatedAt,
             UpdatedAt = d.UpdatedAt
         })
         .ToList();


                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetDocumentStatusMessage.DocumentsFetched;
                response.GetAllData = documents;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AssetDocumentModule",
                    apiName: "GetAllAssetDocuments",
                    tenantId: tenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetDocumentStatusMessage.FetchFailed} {ex.Message}";
            }

            return response;
        }
        public GetAllRecord<AssetDocumentResponseDto> GetAssetDocumentsUrl(int tenantId)
        {
            GetAllRecord<AssetDocumentResponseDto> response = new();
            string baseUrl = _configuration["BaseUrl:Staging"] ?? "https://ms.stagingsdei.com:8107";
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var documents = tenantDb.AssetDocuments
                    .Where(d => !d.IsDeleted).Include(d => d.Asset)
                    .AsEnumerable()
                    .Select(d => new AssetDocumentResponseDto
                    {
                        DocumentId = d.DocumentId,
                        AssetId = d.AssetId,
                        AssetName = d.Asset != null ? d.Asset.AssetName : string.Empty,
                        TenantId = d.TenantId,
                        DocumentTitle = d.DocumentTitle,
                        DocumentType = d.DocumentType,
                        Category = d.Category,
                        Description = d.Description,
                        ExpiryDate = d.ExpiryDate,
                        ReminderDaysBeforeExpiry = d.ReminderDaysBeforeExpiry,
                        ComplianceFlag = d.ComplianceFlag,
                        FilePath = d.FilePath,
                        DocumentUrl = d.FilePath != null
                            ? $"{baseUrl}/{d.FilePath.Replace("\\", "/")}"
                            : null,
                        OriginalFileName = d.OriginalFileName,
                        FileExtension = d.FileExtension,
                        FileSize = d.FileSize,
                        UploadedOn = d.UploadedOn,
                        Status = d.Status,
                        CreatedAt = d.CreatedAt,
                        //CreatedBy = d.CreatedBy,
                        UpdatedAt = d.UpdatedAt,
                        //UpdatedBy = d.UpdatedBy
                    })
                    .ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetDocumentStatusMessage.Success;
                response.GetAllData = documents;
            }
            catch (Exception ex)
            {
                _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AssetDocumentModule",
                    apiName: "GetAllAssetDocuments",
                    tenantId: tenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetDocumentStatusMessage.FetchFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateAssetDocument(UpdateAssetDocumentDto dto)
        {
            CommonResponseModel response = new();

            using var transaction = await _masterDbcontext.Database.BeginTransactionAsync();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var document = await tenantDb.AssetDocuments
      .FirstOrDefaultAsync(d => d.DocumentId == dto.DocumentId && d.IsDeleted == false);


                if (document == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetDocumentStatusMessage.DocumentNotFound;
                    return response;
                }

                var exists = await tenantDb.AssetDocuments
                    .AnyAsync(d =>
                        d.DocumentId != dto.DocumentId &&
                        d.AssetId == dto.AssetId &&
                        d.DocumentTitle.ToLower() == dto.DocumentTitle.ToLower() &&
                        !d.IsDeleted);

                if (exists)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.StatusMessage = AssetDocumentStatusMessage.DuplicateTitle;
                    return response;
                }

                document.DocumentTitle = dto.DocumentTitle;
                document.DocumentType = dto.DocumentType;
                document.Category = dto.Category;
                document.Description = dto.Description;
                document.ExpiryDate = dto.ExpiryDate;
                document.ReminderDaysBeforeExpiry = dto.ReminderDaysBeforeExpiry;
                document.ComplianceFlag = dto.ComplianceFlag;

                if (dto.DocumentFile != null)
                {
                    var relativePath = await _fileStorageService.SaveFileAsync(dto.DocumentFile, "AssetDocuments");
                    var fileBytes = await File.ReadAllBytesAsync(Path.Combine("wwwroot", relativePath));

                    document.FilePath = relativePath;
                    document.FileSize = dto.DocumentFile.Length;
                    document.OriginalFileName = dto.DocumentFile.FileName;
                    document.FileExtension = Path.GetExtension(dto.DocumentFile.FileName);
                    //document.FileContent = fileBytes; // optional
                }

                document.UpdatedAt = DateTime.UtcNow;
                document.UpdatedBy = dto.UpdatedBy;

                var ctx = _httpContextAccessor.HttpContext;
                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Update",
                    Details = $"Updated Asset Document '{document.DocumentTitle}' (ID: {dto.DocumentId})",
                    EventType = "AssetDocumentUpdate",
                    TenantId = dto.TenantId,
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
                response.StatusMessage = AssetDocumentStatusMessage.DocumentUpdated;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AssetDocumentModule",
                    apiName: "UpdateAssetDocument",
                    tenantId: dto?.TenantId,
                    userId: dto?.UpdatedBy
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetDocumentStatusMessage.UpdateFailed}:{ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> DeleteAssetDocument(int id, int tenantId)
        {
            CommonResponseModel response = new();

            using var transaction = await _masterDbcontext.Database.BeginTransactionAsync();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var document = await tenantDb.AssetDocuments
                    .FirstOrDefaultAsync(d => d.DocumentId == id && !d.IsDeleted);

                if (document == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AssetDocumentStatusMessage.DocumentNotFound;
                    return response;
                }

                document.IsDeleted = true;
                document.IsActive = false;
                document.UpdatedAt = DateTime.UtcNow;
                document.UpdatedBy = tenantId; 

                var ctx = _httpContextAccessor.HttpContext;
                var auditData = new Audit_Log_MasterDb()
                {
                    Action = "Delete",
                    Details = $"Deleted asset document '{document.DocumentTitle}' (ID: {id})",
                    EventType = "AssetDocumentDelete",
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
                response.StatusMessage = AssetDocumentStatusMessage.DocumentDeleted;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AssetDocumentModule",
                    apiName: "DeleteAssetDocument",
                    tenantId: tenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetDocumentStatusMessage.DeleteFailed}: {ex.Message}";
            }

            return response;
        }

    }
}