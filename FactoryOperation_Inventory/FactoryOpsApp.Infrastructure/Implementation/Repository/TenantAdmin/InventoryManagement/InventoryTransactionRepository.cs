using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOperation_Inventory.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.InventoryManagement
{
    public class InventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public InventoryTransactionRepository(TenantDbContextFactory tenantDbContext,
                                           IAuditLogService auditLogger,
                                           IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> CreateTransactionAsync(InventoryTransactionDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

               
                var transactionId = GenerateTransactionId(dto.TransactionType);

                var entity = new InventoryTransaction
                {
                    TransactionId = transactionId,
                    TenantId = dto.TenantId,
                    TransactionType = dto.TransactionType,
                    PartId = dto.PartId,
                    Quantity = dto.Quantity,
                    FromLocationId = dto.FromLocationId,
                    ToLocationId = dto.ToLocationId,
                    ReferenceNumber = dto.ReferenceNumber,
                    Notes = dto.Notes,
                    PerformedById = dto.PerformedById,
                    Status = dto.Status,
                    TransactionDate = dto.TransactionDate,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.CreatedBy
                };

                await tenantDb.InventoryTransaction.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await UpdateInventoryStock(dto, tenantDb);

                await _auditLogger.LogAuditAsync("Create", $"Created {dto.TransactionType} Transaction: {transactionId}", dto.TenantId, "", "CreateTransactionAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = TransactionStatusMessage.Created };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryTransaction-Module", "CreateTransactionAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{TransactionStatusMessage.CreateFailed}: {ex.Message}" };
            }
        }
        private string GenerateTransactionId(TransactionType type)
        {
            var prefix = type switch
            {
                TransactionType.Receipt => "REC",
                TransactionType.Issue => "ISS",
                TransactionType.Transfer => "TRF",
                TransactionType.Return => "RET",
                TransactionType.Adjustment => "ADJ",
                TransactionType.Scrap => "SCR",
                _ => "TRN"
            };

            var datePart = DateTime.UtcNow.ToString("yyMMdd");
            var randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();

            return $"{prefix}{datePart}{randomSuffix}";
        }
        private async Task UpdateInventoryStock(InventoryTransactionDto dto, FactoryOpsDBContext tenantDb)
        {
            var inventoryItem = await tenantDb.Inventory
                .FirstOrDefaultAsync(i => i.ItemId == dto.PartId && !i.IsDeleted);

            if (inventoryItem != null)
            {
                switch (dto.TransactionType)
                {
                    case TransactionType.Receipt:
                    case TransactionType.Return:
                        inventoryItem.QuantityAvailable += dto.Quantity;
                        break;
                    case TransactionType.Issue:
                    case TransactionType.Scrap:
                        inventoryItem.QuantityAvailable -= dto.Quantity;
                        break;
                    case TransactionType.Transfer:
                        
                        break;
                    case TransactionType.Adjustment:
                        inventoryItem.QuantityAvailable = dto.Quantity;
                        break;
                }

                inventoryItem.UpdatedAt = DateTime.UtcNow;
                await tenantDb.SaveChangesAsync();
            }
        }
        public async Task<CommonResponseModel> UpdateTransactionAsync(InventoryTransactionDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.InventoryTransaction
                    .FirstOrDefaultAsync(t => t.Id == dto.Id &&
                                              t.TenantId == dto.TenantId &&
                                              !t.IsDeleted);

                if (entity == null)
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = TransactionStatusMessage.NotFound
                    };
                }

                entity.TransactionType = dto.TransactionType;
                entity.PartId = dto.PartId;
                entity.Quantity = dto.Quantity;
                entity.FromLocationId = dto.FromLocationId;
                entity.ToLocationId = dto.ToLocationId;
                entity.ReferenceNumber = dto.ReferenceNumber;
                entity.Notes = dto.Notes;
                entity.PerformedById = dto.PerformedById;
                entity.Status = dto.Status;
                entity.TransactionDate = dto.TransactionDate;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.UpdatedBy;

                await tenantDb.SaveChangesAsync();

                await UpdateInventoryStock(dto, tenantDb);

                await _auditLogger.LogAuditAsync("Update", $"Updated {dto.TransactionType} Transaction: {dto.Id}", dto.TenantId, "", "UpdateTransactionAsync");

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TransactionStatusMessage.Updated
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryTransaction-Module", "UpdateTransactionAsync", dto.TenantId, null);
                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{TransactionStatusMessage.UpdateFailed}: {ex.Message}"
                };
            }
        }
        public async Task<CommonResponseModel> DeleteTransactionAsync(int transactionId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.InventoryTransaction
                    .FirstOrDefaultAsync(t => t.Id == transactionId && !t.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = TransactionStatusMessage.NotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = tenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Delete", $"Deleted Transaction: {entity.TransactionId}", tenantId, "", "DeleteTransactionAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = TransactionStatusMessage.Deleted };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryTransaction-Module", "DeleteTransactionAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{TransactionStatusMessage.DeleteFailed}: {ex.Message}" };
            }
        }
        public async Task<GetAllRecord<GetInventoryTransactionDto>> GetAllTransactionsAsync(int tenantId, TransactionQueryDto? query = null)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var transactionsQuery = tenantDb.InventoryTransaction
                    .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                    .Include(t => t.Part)
                    .Include(t => t.FromLocation)
                    .Include(t => t.ToLocation)
                    .Include(t => t.PerformedBy)
                    .AsQueryable();

                if (query != null)
                {
                    if (query.PartId.HasValue)
                        transactionsQuery = transactionsQuery.Where(t => t.PartId == query.PartId.Value);

                    if (query.TransactionType.HasValue)
                        transactionsQuery = transactionsQuery.Where(t => t.TransactionType == query.TransactionType.Value);

                    if (query.FromDate.HasValue)
                        transactionsQuery = transactionsQuery.Where(t => t.TransactionDate >= query.FromDate.Value);

                    if (query.ToDate.HasValue)
                        transactionsQuery = transactionsQuery.Where(t => t.TransactionDate <= query.ToDate.Value);

                    if (query.LocationId.HasValue)
                        transactionsQuery = transactionsQuery.Where(t => t.FromLocationId == query.LocationId || t.ToLocationId == query.LocationId);
                }

                var entities = await transactionsQuery
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                var dtoList = entities.Select(t => new GetInventoryTransactionDto
                {
                    Id = t.Id,
                    TransactionId = t.TransactionId,
                    TenantId = t.TenantId,
                    TransactionType = t.TransactionType,
                    TransactionTypeDisplay = t.TransactionType.ToString(),
                    PartId = t.PartId,
                    PartCode = t.Part?.ItemCode ?? "N/A",
                    PartName = t.Part?.ItemName ?? "N/A",
                    Quantity = t.Quantity,
                    FromLocationId = t.FromLocationId,
                    FromLocationName = t.FromLocation?.LocationName,
                    ToLocationId = t.ToLocationId,
                    ToLocationName = t.ToLocation?.LocationName,
                    ReferenceNumber = t.ReferenceNumber,
                    Notes = t.Notes,
                    PerformedById = t.PerformedById,
                    PerformedByName = t.PerformedBy != null ? $"{t.PerformedBy.FirstName} {t.PerformedBy.LastName}" : "N/A",
                    Status = t.Status,
                    StatusDisplay = t.Status.ToString(),
                    TransactionDate = t.TransactionDate,
                    IsActive = t.IsActive,
                    TransactionDateFormatted = t.TransactionDate.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return new GetAllRecord<GetInventoryTransactionDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TransactionStatusMessage.FetchSuccess,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryTransaction-Module", "GetAllTransactionsAsync", tenantId, null);
                return new GetAllRecord<GetInventoryTransactionDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{TransactionStatusMessage.FetchFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetSpecificRecord<GetInventoryTransactionDto>> GetTransactionByIdAsync(int transactionId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.InventoryTransaction
                    .Include(t => t.Part)
                    .Include(t => t.FromLocation)
                    .Include(t => t.ToLocation)
                    .Include(t => t.PerformedBy)
                    .FirstOrDefaultAsync(t => t.Id == transactionId && !t.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<GetInventoryTransactionDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = TransactionStatusMessage.NotFound
                    };

                var dto = new GetInventoryTransactionDto
                {
                    Id = entity.Id,
                    TransactionId = entity.TransactionId,
                    TenantId = entity.TenantId,
                    TransactionType = entity.TransactionType,
                    TransactionTypeDisplay = entity.TransactionType.ToString(),
                    PartId = entity.PartId,
                    PartCode = entity.Part?.ItemCode ?? "N/A",
                    PartName = entity.Part?.ItemName ?? "N/A",
                    Quantity = entity.Quantity,
                    FromLocationId = entity.FromLocationId,
                    FromLocationName = entity.FromLocation?.LocationName,
                    ToLocationId = entity.ToLocationId,
                    ToLocationName = entity.ToLocation?.LocationName,
                    ReferenceNumber = entity.ReferenceNumber,
                    Notes = entity.Notes,
                    PerformedById = entity.PerformedById,
                    PerformedByName = entity.PerformedBy != null ? $"{entity.PerformedBy.FirstName} {entity.PerformedBy.LastName}" : "N/A",
                    Status = entity.Status,
                    StatusDisplay = entity.Status.ToString(),
                    TransactionDate = entity.TransactionDate,
                    IsActive = entity.IsActive,
                    TransactionDateFormatted = entity.TransactionDate.ToString("yyyy-MM-dd HH:mm")
                };

                return new GetSpecificRecord<GetInventoryTransactionDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TransactionStatusMessage.FetchSuccess,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryTransaction-Module", "GetTransactionByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetInventoryTransactionDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{TransactionStatusMessage.FetchFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<GetInventoryTransactionDto>> GetTransactionsByPartIdAsync(int partId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.InventoryTransaction
                    .Where(t => t.TenantId == tenantId && t.PartId == partId && !t.IsDeleted)
                    .Include(t => t.Part)
                    .Include(t => t.FromLocation)
                    .Include(t => t.ToLocation)
                    .Include(t => t.PerformedBy)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                var dtoList = entities.Select(t => new GetInventoryTransactionDto
                {
                    Id = t.Id,
                    TransactionId = t.TransactionId,
                    TenantId = t.TenantId,
                    TransactionType = t.TransactionType,
                    TransactionTypeDisplay = t.TransactionType.ToString(),
                    PartId = t.PartId,
                    PartCode = t.Part?.ItemCode ?? "N/A",
                    PartName = t.Part?.ItemName ?? "N/A",
                    Quantity = t.Quantity,
                    FromLocationId = t.FromLocationId,
                    FromLocationName = t.FromLocation?.LocationName,
                    ToLocationId = t.ToLocationId,
                    ToLocationName = t.ToLocation?.LocationName,
                    ReferenceNumber = t.ReferenceNumber,
                    Notes = t.Notes,
                    PerformedById = t.PerformedById,
                    PerformedByName = t.PerformedBy != null ? $"{t.PerformedBy.FirstName} {t.PerformedBy.LastName}" : "N/A",
                    Status = t.Status,
                    StatusDisplay = t.Status.ToString(),
                    TransactionDate = t.TransactionDate,
                    IsActive = t.IsActive,
                    TransactionDateFormatted = t.TransactionDate.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return new GetAllRecord<GetInventoryTransactionDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TransactionStatusMessage.PartTransactionsFetched,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryTransaction-Module", "GetTransactionsByPartIdAsync", tenantId, null);
                return new GetAllRecord<GetInventoryTransactionDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{TransactionStatusMessage.FetchFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<GetInventoryTransactionDto>> GetTransactionsByDateRangeAsync(int tenantId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.InventoryTransaction
                    .Where(t => t.TenantId == tenantId && !t.IsDeleted &&
                               t.TransactionDate >= fromDate && t.TransactionDate <= toDate)
                    .Include(t => t.Part)
                    .Include(t => t.FromLocation)
                    .Include(t => t.ToLocation)
                    .Include(t => t.PerformedBy)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                var dtoList = entities.Select(t => new GetInventoryTransactionDto
                {
                    Id = t.Id,
                    TransactionId = t.TransactionId,
                    TenantId = t.TenantId,
                    TransactionType = t.TransactionType,
                    TransactionTypeDisplay = t.TransactionType.ToString(),
                    PartId = t.PartId,
                    PartCode = t.Part?.ItemCode ?? "N/A",
                    PartName = t.Part?.ItemName ?? "N/A",
                    Quantity = t.Quantity,
                    FromLocationId = t.FromLocationId,
                    FromLocationName = t.FromLocation?.LocationName,
                    ToLocationId = t.ToLocationId,
                    ToLocationName = t.ToLocation?.LocationName,
                    ReferenceNumber = t.ReferenceNumber,
                    Notes = t.Notes,
                    PerformedById = t.PerformedById,
                    PerformedByName = t.PerformedBy != null ? $"{t.PerformedBy.FirstName} {t.PerformedBy.LastName}" : "N/A",
                    Status = t.Status,
                    StatusDisplay = t.Status.ToString(),
                    TransactionDate = t.TransactionDate,
                    IsActive = t.IsActive,
                    TransactionDateFormatted = t.TransactionDate.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return new GetAllRecord<GetInventoryTransactionDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TransactionStatusMessage.DateRangeTransactionsFetched   ,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryTransaction-Module", "GetTransactionsByDateRangeAsync", tenantId, null);
                return new GetAllRecord<GetInventoryTransactionDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{TransactionStatusMessage.FetchFailed}: {ex.Message}"
                };
            }
        }
        public async Task<CommonResponseModel> UpdateTransactionStatusAsync(int transactionId, int tenantId, int updatedBy)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.InventoryTransaction
                    .FirstOrDefaultAsync(t => t.Id == transactionId && !t.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = TransactionStatusMessage.NotFound };

                entity.Status = entity.Status == TransactionStatus.Pending ?
                    TransactionStatus.Completed : TransactionStatus.Pending;

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = updatedBy;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Update",
                    $"Updated Transaction Status: {entity.TransactionId} to {entity.Status}",
                    tenantId, "", "UpdateTransactionStatusAsync");

                return new CommonResponseModel
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = $"{TransactionStatusMessage.StatusUpdated} {entity.Status}"
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryTransaction-Module", "UpdateTransactionStatusAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{TransactionStatusMessage.StatusUpdateFailed}: {ex.Message}" };
            }
        }
        public async Task<GetAllRecord<GetInventoryTransactionDto>> GetAllTransactionsAsync(int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entities = await tenantDb.InventoryTransaction
                    .Where(t => t.TenantId == tenantId && !t.IsDeleted)
                    .Include(t => t.Part)
                    .Include(t => t.FromLocation)
                    .Include(t => t.ToLocation)
                    .Include(t => t.PerformedBy)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                var dtoList = entities.Select(t => new GetInventoryTransactionDto
                {
                    Id = t.Id,
                    TransactionId = t.TransactionId,
                    TenantId = t.TenantId,
                    TransactionType = t.TransactionType,
                    TransactionTypeDisplay = t.TransactionType.ToString(),
                    PartId = t.PartId,
                    PartCode = t.Part?.ItemCode ?? "N/A",
                    PartName = t.Part?.ItemName ?? "N/A",
                    Quantity = t.Quantity,
                    FromLocationId = t.FromLocationId,
                    FromLocationName = t.FromLocation?.LocationName,
                    ToLocationId = t.ToLocationId,
                    ToLocationName = t.ToLocation?.LocationName,
                    ReferenceNumber = t.ReferenceNumber,
                    Notes = t.Notes,
                    PerformedById = t.PerformedById,
                    PerformedByName = t.PerformedBy != null ? $"{t.PerformedBy.FirstName} {t.PerformedBy.LastName}" : "N/A",
                    Status = t.Status,
                    StatusDisplay = t.Status.ToString(),
                    TransactionDate = t.TransactionDate,
                    IsActive = t.IsActive,
                    TransactionDateFormatted = t.TransactionDate.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return new GetAllRecord<GetInventoryTransactionDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = TransactionStatusMessage.AllTransactionsFetched,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "InventoryTransaction-Module", "GetAllTransactionsAsync", tenantId, null);
                return new GetAllRecord<GetInventoryTransactionDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{TransactionStatusMessage.FetchFailed}: {ex.Message}"
                };
            }
        }
    }
}
