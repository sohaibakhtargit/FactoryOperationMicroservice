using CsvHelper;
using CsvHelper.Configuration;
using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_Asset.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOperation_Asset.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.Common;
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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;
using System.Text.Json;
using static FactoryOperation_Asset.FactoryOpsApp.Common.CommonConstantURLs;
using static FactoryOpsApp.Common.CommonConstant;
using static FactoryOpsApp.Domain.Entities.FactoryOpsTenants.AssetRegistry;

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

        private static DateTime? ToUtc(DateTime? date)
        {
            if (!date.HasValue)
                return null;

            return date.Value.Kind switch
            {
                DateTimeKind.Utc => date,
                DateTimeKind.Local => date.Value.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(date.Value, DateTimeKind.Utc),
                _ => date
            };
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
                    AcquisitionCost = dto.AcquisitionCost,
                    PurchaseDate = dto.PurchaseDate ?? DateTime.UtcNow,
                    WarrantyExpiry = dto.WarrantyExpiry ?? null,
                    InsurancePolicyNumber = dto.InsurancePolicyNumber ?? null,
                    BulkImportId = dto.BulkAssetId,
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
                    LastMovedOn = DateTime.UtcNow,
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


        private static TEnum? ParseEnumSafe<TEnum>(string? value)
        where TEnum : struct
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return Enum.TryParse<TEnum>(value, true, out var result)
                ? result
                : null;
        }
        private List<T> ReadCsvFile<T>(IFormFile file) where T : new()
        {
            using var reader = new StreamReader(file.OpenReadStream());

            using var csv = new CsvReader(
                reader,
                new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    BadDataFound = null,
                    TrimOptions = TrimOptions.Trim,
                    IgnoreBlankLines = true
                });

            return csv.GetRecords<T>().ToList();
        }
        private async Task<string?> ValidateAssetRow(AssetImportCsvDto row, BulkAssetImportRequest request)
        {

            using var tenantDb =
                _tenantDbContext.GetTenantDbContext(request.TenantId);


            if (string.IsNullOrWhiteSpace(row.AssetName))
                return "AssetName is required";

            if (string.IsNullOrWhiteSpace(row.SerialNumber))
                return "SerialNumber is required";

            if (row.AssignedTo.HasValue)
            {
                var userExists = await tenantDb.FactoryUsers
                    .AnyAsync(u =>
                        !u.IsDeleted &&
                        u.IsActive &&
                        u.UserId == row.AssignedTo.Value);

                if (!userExists)
                    return $"AssignedTo user id '{row.AssignedTo}' does not exist in this tenant";
            }
        
            var locationExists = await tenantDb.Locations
                .AnyAsync(l =>
                    !l.IsDeleted &&
                    l.IsActive &&
                    l.LocationId == row.LocationId.Value &&
                    l.TenantId == request.TenantId);

            if (!locationExists)
                return $"Location '{row.LocationId}' does not belong to this tenant";

            var duplicateExists = await tenantDb.AssetRegistry.AnyAsync(a =>
                !a.IsDeleted &&
                (
                    (!string.IsNullOrEmpty(a.SerialNumber) &&
                     !string.IsNullOrEmpty(row.SerialNumber) &&
                     a.SerialNumber.ToLower() == row.SerialNumber.ToLower())

                    ||

                    (!string.IsNullOrEmpty(a.AssetUniqueId) &&
                     !string.IsNullOrEmpty(row.AssetUniqueId) &&
                     a.AssetUniqueId.ToLower() == row.AssetUniqueId.ToLower())
                ));

            if (duplicateExists)
                return "Duplicate SerialNumber or AssetUniqueId exists";

            return null;
        } 
        public async Task<BulkAssetImportResult> ImportBulkAssetAsync(
        BulkAssetImportRequest request)
        {
            using var tenantDb =
                _tenantDbContext.GetTenantDbContext(request.TenantId);


            using var transaction =
                await tenantDb.Database.BeginTransactionAsync();

            var result = new BulkAssetImportResult();

            var rows = ReadCsvFile<AssetImportCsvDto>(request.File);

            result.TotalRecords = rows.Count;

           
            int rowNumber = 2;
            foreach (var row in rows)
            {
                var error = await ValidateAssetRow(row, request);

                if (error != null)
                {
                    result.FailureCount = 1;

                    result.Errors.Add(new BulkAssetError
                    {
                        RowNumber = rowNumber,
                        ErrorMessage = error
                    });

                    await transaction.RollbackAsync();
                    return result;
                }

                rowNumber++;
            }

            /*foreach (var row in rows)
            {
                var error = await ValidateAssetRow(
                    row); 

                if (error != null)
                {
                    result.FailureCount = 1;

                    result.Errors.Add(new BulkAssetError
                    {
                        RowNumber = rowNumber,
                        ErrorMessage = error
                    });

                    await transaction.RollbackAsync();
                    return result;
                }

                rowNumber++;
            }*/

            if (result.FailureCount > 0)
            {
                await transaction.RollbackAsync();
                return result;
            }

            var filePath =
                await _fileStorageService.SaveFileAsync(
                    request.File,
                    "Upload");

            var bulkImport = new AssetBulkImport
            {
                TenantId = request.TenantId,
                FileName = request.File.FileName,
                FilePath = filePath,
                UploadedBy = request.CreatedBy,
                UploadedAt = DateTime.UtcNow
            };

            tenantDb.AssetBulkImport.Add(bulkImport);
            await tenantDb.SaveChangesAsync();

            foreach (var row in rows)
            {
                var asset = new AssetRegistry
                {
                    BulkImportId = bulkImport.BulkImportId,
                    TenantId = request.TenantId,

                    AssetUniqueId = row.AssetUniqueId,
                    AssetName = row.AssetName.Trim(),
                    AssetTypeId = row.AssetTypeId,

                    Model = row.Model,
                    SerialNumber = row.SerialNumber,
                    CategoryHierarchy = row.CategoryHierarchy,
                    LocationId = row.LocationId!.Value,

                    Department = row.Department,
                    Vendor = row.Vendor,
                    Supplier = row.Supplier,
                    Manufacturer = row.Manufacturer,

                    ExpectedLifespan = row.ExpectedLifespan,
                    DepreciationRule = row.DepreciationRule,
                    Power = row.Power,

                    Criticality =
                        ParseEnumSafe<AssetRegistry.CriticalityLevel>(
                            row.Criticality),

                    DocumentUrl = row.DocumentUrl,
                    InsurancePolicyNumber = row.InsurancePolicyNumber,
                    WarrantyExpiry = ToUtc(row.WarrantyExpiry),

                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                tenantDb.AssetRegistry.Add(asset);

                await tenantDb.SaveChangesAsync();

                var tracking = new AssetTracking
                {
                    AssetId = asset.AssetId,
                    TenantId = request.TenantId,
                    CurrentLocation = row.LocationId.Value,
                    AssignedTo = row.AssignedTo,
                    Status =
                        ParseEnumSafe<AssetTrackingStatusEnum>(row.Status)
                        ?? AssetTrackingStatusEnum.Active,

                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                tenantDb.AssetTracking.Add(tracking);
            }

            await tenantDb.SaveChangesAsync();

            await transaction.CommitAsync();

            result.SuccessCount = result.TotalRecords;
            result.FailureCount = 0;

            return result;
        }

        /* private List<T> ReadCsvFile<T>(IFormFile file) where T : new()
         {
             using var reader = new StreamReader(file.OpenReadStream());
             using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
             {
                 HasHeaderRecord = true,
                 HeaderValidated = null,
                 MissingFieldFound = null,
                 BadDataFound = null,
                 TrimOptions = TrimOptions.Trim,
                 IgnoreBlankLines = true
             });

             return csv.GetRecords<T>().ToList();
         }


         public async Task<BulkAssetImportResult> ImportBulkAssetAsync(BulkAssetImportRequest request)
         {
             using var tenantDb = _tenantDbContext.GetTenantDbContext(request.TenantId);

             List<AssetImportCsvDto> rows;
             try
             {
                 rows = ReadCsvFile<AssetImportCsvDto>(request.File);
             }
             catch (Exception ex)
             {
                 throw new Exception("CSV parsing failed: " + ex.Message, ex);
             }
             var filePath = await _fileStorageService.SaveFileAsync(request.File,"Upload");

             var bulkImport = new AssetBulkImport
             {
                 TenantId = request.TenantId,
                 FileName = request.File.FileName,
                 FilePath = filePath,
                 UploadedBy = request.CreatedBy,
                 UploadedAt = DateTime.UtcNow
             };

             tenantDb.AssetBulkImport.Add(bulkImport);
             await tenantDb.SaveChangesAsync();

             var result = new BulkAssetImportResult
             {
                 TotalRecords = rows.Count
             };

             int rowNumber = 2;
             foreach (var row in rows)
             {
                 try
                 {
                     if (string.IsNullOrWhiteSpace(row.AssetName))
                         throw new Exception("AssetName is required");

                     if (!row.LocationId.HasValue || row.LocationId <= 0)
                         throw new Exception("Invalid LocationId");

                     var dto = new AssetRegistryDto
                     {
                         BulkAssetId = bulkImport.BulkImportId,
                         TenantId = request.TenantId,

                         AssetId = row.AssetId,
                         AssetUniqueId = row.AssetUniqueId,
                         AssetName = row.AssetName.Trim(),
                         AssetTypeId = row.AssetTypeId,

                         Model = row.Model,
                         SerialNumber = row.SerialNumber,
                         CategoryHierarchy = row.CategoryHierarchy,
                         LocationId = row.LocationId.Value,

                         Status = ParseEnumSafe<AssetTrackingStatusEnum>(row.Status)
                                     ?? AssetTrackingStatusEnum.Active,

                         AssignedTo = row.AssignedTo,
                         Department = row.Department,
                         Vendor = row.Vendor,
                         Supplier = row.Supplier,
                         Manufacturer = row.Manufacturer,

                         ExpectedLifespan = row.ExpectedLifespan,
                         DepreciationRule = row.DepreciationRule,
                         Power = row.Power,

                         Criticality = ParseEnumSafe<AssetRegistry.CriticalityLevel>(row.Criticality),

                         DocumentUrl = row.DocumentUrl,
                         InsurancePolicyNumber = row.InsurancePolicyNumber,
                         WarrantyExpiry = ToUtc(row.WarrantyExpiry),

                         CreatedBy = request.CreatedBy
                     };

                     var response = await AddAsset(dto);

                     if (response.StatusCode != StatusCode.Success)
                         throw new Exception(response.StatusMessage);

                     result.SuccessCount++;
                 }
                 catch (Exception ex)
                 {
                     result.FailureCount++;
                     result.Errors.Add(new BulkAssetError
                     {
                         RowNumber = rowNumber,
                         ErrorMessage = ex.Message
                     });
                 }

                 rowNumber++;
             }
             bulkImport.TotalRecords = result.TotalRecords;
             bulkImport.SuccessCount = result.SuccessCount;
             bulkImport.FailureCount = result.FailureCount;

             await tenantDb.SaveChangesAsync();

             return result;
         }*/
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

                bool LocationChange = asset.LocationId != dto.LocationId;

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
                asset.Supplier = dto.Supplier ?? null;
                asset.Manufacturer = dto.Manufacturer ?? null;
                asset.ExpectedLifespan = dto.ExpectedLifespan ?? null;
                asset.DepreciationRule = dto.DepreciationRule ?? null;
                asset.Power = dto.Power;
                asset.Criticality = dto.Criticality ?? null;
                asset.WarrantyExpiry = dto.WarrantyExpiry;
                asset.InsurancePolicyNumber = dto.InsurancePolicyNumber;
                asset.UpdatedAt = DateTime.UtcNow;
                asset.UpdatedBy = dto.UpdatedBy;

                var tracking = await tenantDb.AssetTracking
                    .FirstOrDefaultAsync(t => t.AssetId == dto.AssetId);

                bool StatusChange = tracking.Status != dto.Status;
                if (tracking != null)
                {
                    tracking.Status = dto.Status;
                    tracking.AssignedTo = dto.AssignedTo;
                    tracking.CurrentLocation = dto.LocationId;
                    tracking.UpdatedAt = DateTime.UtcNow;
                    tracking.UpdatedBy = dto.UpdatedBy;
                }

                await tenantDb.SaveChangesAsync();

                if (LocationChange || StatusChange)
                {

                    string eventType = StatusChange ? "AssetStatusChange" : "AssetLocationStatusChange";
                    {
                        var eventDto = new AssetEventDto
                        {
                            AssetId = asset.AssetId,
                            TenantId = asset.TenantId,
                            AssetName = asset.AssetName,
                            EventType = eventType,
                            EventTime = DateTime.UtcNow,
                            Status = dto.Status.ToString(),
                            LocationId = asset.LocationId,
                            LocationName = asset.Location != null ? asset.Location.LocationName : null,
                        };
                        await PublishKafkaEventAsyncc(asset.TenantId, asset.LocationId, eventDto);

                    }

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
        private async Task PublishKafkaEventAsyncc(int tenantId, int LocationId, AssetEventDto eventDto)
        {
            var correlationId = Guid.NewGuid().ToString();
            string topicName = KafkaCommonTopics.BuildTopicName(tenantId, eventDto.EventType);

            var kafkaRequest = new
            {
                Topic = topicName,
                Key = $"Asset-{LocationId}",
                Payload = eventDto,
                Source = "AssetService",
                Headers = new Dictionary<string, string>
                {
                    ["tenant-id"] = tenantId.ToString(),
                    ["correlation-id"] = correlationId
                }
            };

            using var httpClient = new HttpClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(kafkaRequest),
                Encoding.UTF8,
                "application/json"
            );

            var kafkaResponse = await httpClient.PostAsync(ConstantUrls.kafkaPublish, jsonContent);
            kafkaResponse.EnsureSuccessStatusCode();
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
                    .Include(a => a.AssetLifecycles)
                    .FirstOrDefaultAsync(a => a.AssetId == id && !a.IsDeleted);

                if (asset == null)
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = AssetManagementStatusMessage.AssetNotFound
                    };
                }

                asset.IsDeleted = true;
                asset.IsActive = false;
                asset.DeletedAt = DateTime.UtcNow;
                asset.DeletedBy = tenantId;

                var trackings = await tenantDb.AssetTracking
                    .Where(t => t.AssetId == id && !t.IsDeleted)
                    .ToListAsync();

                foreach (var t in trackings)
                {
                    t.IsDeleted = true;
                    t.IsActive = false;
                    t.DeletedAt = DateTime.UtcNow;
                    t.DeletedBy = tenantId;
                }

                var lifecycles = await tenantDb.AssetLifecycles
                    .Where(l => l.AssetId == id && !l.IsDeleted)
                    .ToListAsync();

                foreach (var lifecycle in lifecycles)
                {
                    lifecycle.IsDeleted = true;
                    lifecycle.IsActive = false;
                    lifecycle.DeletedAt = DateTime.UtcNow;
                    lifecycle.DeletedBy = tenantId;

                    var mappings = await tenantDb.AssetLifecycleMappings
                        .Where(m => m.LifecycleId == lifecycle.LifecycleId && !m.IsDeleted)
                        .ToListAsync();

                    foreach (var m in mappings)
                    {
                        m.IsDeleted = true;
                        m.IsActive = false;
                    }
                }

                var maintenance = await tenantDb.MaintenanceHistory
                    .Where(m => m.AssetId == id && !m.IsDeleted)
                    .ToListAsync();

                foreach (var m in maintenance)
                {
                    m.IsDeleted = true;
                    m.IsActive = false;
                    m.DeletedAt = DateTime.UtcNow;
                    m.DeletedBy = tenantId;
                }

                var documents = await tenantDb.AssetDocuments
                    .Where(d => d.AssetId == id && !d.IsDeleted)
                    .ToListAsync();

                foreach (var d in documents)
                {
                    d.IsDeleted = true;
                    d.IsActive = false;
                    d.DeletedAt = DateTime.UtcNow;
                    d.DeletedBy = tenantId;
                }

                var financials = await tenantDb.AssetFinancialAnalysis
                    .Where(f => f.AssetId == id && !f.IsDeleted)
                    .ToListAsync();

                foreach (var f in financials)
                {
                    f.IsDeleted = true;
                    f.IsActive = false;
                }

                var bomList = await tenantDb.AssetBillOfMaterials
                    .Where(b => b.AssetId == id && !b.IsDeleted)
                    .ToListAsync();

                foreach (var b in bomList)
                {
                    b.IsDeleted = true;
                    b.IsActive = false;
                    b.DeletedAt = DateTime.UtcNow;
                    b.DeletedBy = tenantId;
                }

                var ctx = _httpContextAccessor.HttpContext;
                await _masterDbcontext.Audit_Log_MasterDb.AddAsync(new Audit_Log_MasterDb
                {
                    Action = "Delete",
                    Details = $"Deleted asset '{asset.AssetName}' (ID: {id}) with all relations",
                    EventType = "AssetDelete",
                    TenantId = tenantId,
                    Email = ctx?.User?.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    UserName = Environment.UserName,
                    Ipaddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
                });

                await tenantDb.SaveChangesAsync();
                await _masterDbcontext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = AssetManagementStatusMessage.AssetDeleted
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "AssetModule",
                    "DeleteAsset",
                    tenantId,
                    null);

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{AssetManagementStatusMessage.DeleteFailed}: {ex.Message}"
                };
            }
        }

        public async Task<CommonResponseModel> BulkAssetDeleteAsync(int tenantId, List<int> assetIds)
        {
            CommonResponseModel response = new();

            using var masterTransaction = await _masterDbcontext.Database.BeginTransactionAsync();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var ctx = _httpContextAccessor.HttpContext;

                foreach (var assetId in assetIds)
                {

                    var asset = await tenantDb.AssetRegistry
                        .Include(a => a.AssetLifecycles)
                        .FirstOrDefaultAsync(a => a.AssetId == assetId && !a.IsDeleted);


                    if (asset == null)
                        continue;

                    asset.IsDeleted = true;
                    asset.IsActive = false;
                    asset.DeletedAt = DateTime.UtcNow;
                    asset.DeletedBy = tenantId;

            /*        var trackings = await tenantDb.AssetTracking
                        .Where(t => t.AssetId == assetId && !t.IsDeleted)
                        .ToListAsync();*/


                    var trackings = await tenantDb.AssetTracking
                        .Where(t => t.AssetId == assetId && !t.IsDeleted)
                        .Select(t => new AssetTracking
                        {
                            AssetId = t.AssetId,
                            IsDeleted = t.IsDeleted,
                            IsActive = t.IsActive
                        })
                        .ToListAsync();

                    foreach (var t in trackings)
                    {
                        t.IsDeleted = true;
                        t.IsActive = false;
                        t.DeletedAt = DateTime.UtcNow;
                        t.DeletedBy = tenantId;
                    }

                    var lifecycles = await tenantDb.AssetLifecycles
                        .Where(l =>
                        l.AssetId == assetId && !l.IsDeleted)
                        .ToListAsync();



                    foreach (var lifecycle in lifecycles)
                    {
                        lifecycle.IsDeleted = true;
                        lifecycle.IsActive = false;
                        lifecycle.DeletedAt = DateTime.UtcNow;
                        lifecycle.DeletedBy = tenantId;

                        var mappings = await tenantDb.AssetLifecycleMappings
                            .Where(m => m.LifecycleId == lifecycle.LifecycleId && !m.IsDeleted)
                            .ToListAsync();

                        foreach (var m in mappings)
                        {
                            m.IsDeleted = true;
                            m.IsActive = false;
                        }
                    }

                    var maintenance = await tenantDb.MaintenanceHistory
                        .Where(m => m.AssetId == assetId && !m.IsDeleted)
                        .Select(m => new MaintenanceHistory
                        {
                            AssetId = m.AssetId,
                            IsDeleted = m.IsDeleted,
                            IsActive = m.IsActive
                        }) 
                        .ToListAsync();

                    foreach (var m in maintenance)
                    {
                        m.IsDeleted = true;
                        m.IsActive = false;
                        m.DeletedAt = DateTime.UtcNow;
                        m.DeletedBy = tenantId;
                    }


                    var documents = await tenantDb.AssetDocuments
                        .Where(d => d.AssetId == assetId && !d.IsDeleted)
                        .ToListAsync();
   

                    foreach (var d in documents)
                    {
                        d.IsDeleted = true;
                        d.IsActive = false;
                        d.DeletedAt = DateTime.UtcNow;
                        d.DeletedBy = tenantId;
                    }

                    var financials = await tenantDb.AssetFinancialAnalysis
                        .Where(f => f.AssetId == assetId && !f.IsDeleted)
                        .ToListAsync();

                    foreach (var f in financials)
                    {
                        f.IsDeleted = true;
                        f.IsActive = false;
                    }

                  
                    var bomList = await tenantDb.AssetBillOfMaterials
                        .Where(b => b.AssetId == assetId && !b.IsDeleted)
                        .ToListAsync();

          

                    foreach (var b in bomList)
                    {
                        b.IsDeleted = true;
                        b.IsActive = false;
                        b.DeletedAt = DateTime.UtcNow;
                        b.DeletedBy = tenantId;
                    }

                  
                    await _masterDbcontext.Audit_Log_MasterDb.AddAsync(new Audit_Log_MasterDb
                    {
                        Action = "Delete",
                        Details = $"Deleted asset '{asset.AssetName}' (ID: {assetId}) with all relations",
                        EventType = "BulkAssetDelete",
                        TenantId = tenantId,
                        Email = ctx?.User?.Identity?.Name ?? "System",
                        Timestamp = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false,
                        UserName = Environment.UserName,
                        Ipaddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
                    });
                }

                await tenantDb.SaveChangesAsync();
                await _masterDbcontext.SaveChangesAsync();
                await masterTransaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetManagementStatusMessage.BulkAssetDeleted;
            }
            catch (Exception ex)
            {
                await masterTransaction.RollbackAsync();

                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    "AssetModule",
                    "BulkAssetDeleteAsync",
                    tenantId,
                    null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetManagementStatusMessage.BulkDeleteFailed}: {ex.Message}";
            }

            return response;
        }






        /*    public async Task<CommonResponseModel> BulkAssetDeleteAsync(int tenantId, List<int> assetIds)
            {
                CommonResponseModel response = new();

                using var masterTransaction = await _masterDbcontext.Database.BeginTransactionAsync();
                try
                {
                    using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                    var ctx = _httpContextAccessor.HttpContext;

                    // Step 1: Get only the AssetIds that exist in DB and are not deleted
                    var assetIdsToDelete = await tenantDb.AssetRegistry
                        .Where(a => !a.IsDeleted && assetIds.Contains(a.AssetId))
                        .Select(a => a.AssetId)
                        .ToListAsync();

                    foreach (var assetId in assetIdsToDelete)
                    {
                        var asset = await tenantDb.AssetRegistry
                            .Include(a => a.AssetLifecycles)
                            .FirstOrDefaultAsync(a => a.AssetId == assetId && !a.IsDeleted);

                        if (asset == null)
                            continue;

                        // ---- Asset ----
                        asset.IsDeleted = true;
                        asset.IsActive = false;
                        asset.DeletedAt = DateTime.UtcNow;
                        asset.DeletedBy = tenantId;

                        // ---- AssetTracking ----
                        var trackings = await tenantDb.AssetTracking
                            .Where(t => t.AssetId == assetId && !t.IsDeleted)
                            .ToListAsync();

                        foreach (var t in trackings)
                        {
                            t.IsDeleted = true;
                            t.IsActive = false;
                            t.DeletedAt = DateTime.UtcNow;
                            t.DeletedBy = tenantId;
                        }

                        // ---- Lifecycles + Mappings ----
                        var lifecycles = await tenantDb.AssetLifecycles
                            .Where(l => l.AssetId == assetId && !l.IsDeleted)
                            .ToListAsync();

                        foreach (var lifecycle in lifecycles)
                        {
                            lifecycle.IsDeleted = true;
                            lifecycle.IsActive = false;
                            lifecycle.DeletedAt = DateTime.UtcNow;
                            lifecycle.DeletedBy = tenantId;

                            var mappings = await tenantDb.AssetLifecycleMappings
                                .Where(m => m.LifecycleId == lifecycle.LifecycleId && !m.IsDeleted)
                                .ToListAsync();

                            foreach (var m in mappings)
                            {
                                m.IsDeleted = true;
                                m.IsActive = false;
                            }
                        }

                        // ---- Maintenance ----
                        var maintenance = await tenantDb.MaintenanceHistory
                             .Where(m => m.AssetId == assetId && !m.IsDeleted)
                             .ToListAsync();

                        foreach (var m in maintenance)
                        {
                            m.IsDeleted = true;
                            m.IsActive = false;
                            m.DeletedAt = DateTime.UtcNow;
                            m.DeletedBy = tenantId;
                        }

                        // ---- Documents ----
                        var documents = await tenantDb.AssetDocuments
                            .Where(d => d.AssetId == assetId && !d.IsDeleted)
                            .ToListAsync();

                        foreach (var d in documents)
                        {
                            d.IsDeleted = true;
                            d.IsActive = false;
                            d.DeletedAt = DateTime.UtcNow;
                            d.DeletedBy = tenantId;
                        }

                        // ---- Financials ----
                        var financials = await tenantDb.AssetFinancialAnalysis
                            .Where(f => f.AssetId == assetId && !f.IsDeleted)
                            .ToListAsync();

                        foreach (var f in financials)
                        {
                            f.IsDeleted = true;
                            f.IsActive = false;
                        }

                        // ---- BOM ----
                        var bomList = await tenantDb.AssetBillOfMaterials
                            .Where(b => b.AssetId == assetId && !b.IsDeleted)
                            .ToListAsync();

                        foreach (var b in bomList)
                        {
                            b.IsDeleted = true;
                            b.IsActive = false;
                            b.DeletedAt = DateTime.UtcNow;
                            b.DeletedBy = tenantId;
                        }

                        // ---- Audit Log (per asset) ----
                        await _masterDbcontext.Audit_Log_MasterDb.AddAsync(new Audit_Log_MasterDb
                        {
                            Action = "Delete",
                            Details = $"Deleted asset '{asset.AssetName}' (ID: {assetId}) with all relations",
                            EventType = "BulkAssetDelete",
                            TenantId = tenantId,
                            Email = ctx?.User?.Identity?.Name ?? "System",
                            Timestamp = DateTime.UtcNow,
                            IsActive = true,
                            IsDeleted = false,
                            UserName = Environment.UserName,
                            Ipaddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
                        });
                    }

                    await tenantDb.SaveChangesAsync();
                    await _masterDbcontext.SaveChangesAsync();
                    await masterTransaction.CommitAsync();

                    response.StatusCode = StatusCode.Success;
                    response.StatusMessage = AssetManagementStatusMessage.BulkAssetDeleted;
                }
                catch (Exception ex)
                {
                    await masterTransaction.RollbackAsync();

                    await _exceptionLogger.LogExceptionAsync(
                        ex,
                        "AssetModule",
                        "BulkAssetDeleteAsync",
                        tenantId,
                        null);

                    response.StatusCode = StatusCode.Error;
                    response.StatusMessage = $"{AssetManagementStatusMessage.BulkDeleteFailed}: {ex.Message}";
                }

                return response;
            }
    */



        public async Task<(byte[] fileBytes, string fileName)> GenerateAssetBillingPdf(int tenantId, int assetId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var asset = await tenantDb.AssetRegistry
                .Include(a => a.Location)
                .FirstOrDefaultAsync(a =>
                    a.AssetId == assetId &&
                    a.TenantId == tenantId &&
                    !a.IsDeleted);

            if (asset == null)
                throw new Exception("Asset not found");

            var tracking = await tenantDb.AssetTracking
                .Include(t => t.Location)
                .Include(t => t.AssignedUser)
                .Where(t =>
                    t.AssetId == assetId &&
                    t.TenantId == tenantId &&
                    !t.IsDeleted)
                .OrderByDescending(t => t.UpdatedAt)
                .FirstOrDefaultAsync();

            var baseAmount = asset.AcquisitionCost ?? 0;
            var taxAmount = Math.Round(baseAmount * 0.18m, 2);
            var totalAmount = baseAmount + taxAmount;

            var document = new AssetBillingPdfDocument(
                asset,
                tracking,
                baseAmount,
                taxAmount,
                totalAmount
            );

            var pdfBytes = document.GeneratePdf();
            var fileName = $"Asset-Bill-{asset.AssetId}.pdf";

            return (pdfBytes, fileName);
        }

        public class AssetBillingPdfDocument : IDocument
        {
            private readonly AssetRegistry _asset;
            private readonly AssetTracking? _tracking;
            private readonly decimal _baseAmount;
            private readonly decimal _taxAmount;
            private readonly decimal _totalAmount;

            public AssetBillingPdfDocument(
                AssetRegistry asset,
                AssetTracking? tracking,
                decimal baseAmount,
                decimal taxAmount,
                decimal totalAmount)
            {
                _asset = asset;
                _tracking = tracking;
                _baseAmount = baseAmount;
                _taxAmount = taxAmount;
                _totalAmount = totalAmount;
            }

            public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.UtcNow.ToString("dd MMM yyyy HH:mm"));
                    });
                });
            }

            private void ComposeHeader(IContainer container)
            {
                container.Text("ASSET BILLING SUMMARY")
                    .FontSize(20)
                    .Bold()
                    .FontColor(Colors.Blue.Medium);
            }

            private void ComposeContent(IContainer container)
            {
                container.Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Text("Asset Information").Bold().FontSize(14);
                    column.Item().Text($"Asset Name: {_asset.AssetName}");
                    column.Item().Text($"Serial Number: {_asset.SerialNumber}");
                    column.Item().Text($"Location: {_asset.Location?.LocationName}");
                    column.Item().Text($"Purchase Date: {_asset.PurchaseDate:dd MMM yyyy}");

                    column.Item().LineHorizontal(1);

                    if (_tracking != null)
                    {
                        var fullName = _tracking.AssignedUser != null
                            ? $"{_tracking.AssignedUser.FirstName} {_tracking.AssignedUser.LastName}"
                            : null;
                        column.Item().Text("Asset Status Information").Bold().FontSize(14);
                        column.Item().Text($"Current Status: {_tracking.Status}");
                        column.Item().Text($"Assigned To: {fullName ?? "Not Assigned"}");
                        column.Item().Text($"Last Moved On: {_tracking.LastMovedOn:dd MMM yyyy HH:mm}");
                        column.Item().Text($"Tracking Location: {_tracking.Location?.LocationName}");
                        column.Item().LineHorizontal(1);
                    }

                    column.Item().Text("Billing Summary").Bold().FontSize(14);
                    column.Item().Text($"Base Amount: ₹{_baseAmount}");
                    column.Item().Text($"Tax (18% GST): ₹{_taxAmount}");
                    column.Item().Text($"Total Amount: ₹{_totalAmount}")
                        .Bold()
                        .FontSize(15);
                });
            }
        }

    }

}
