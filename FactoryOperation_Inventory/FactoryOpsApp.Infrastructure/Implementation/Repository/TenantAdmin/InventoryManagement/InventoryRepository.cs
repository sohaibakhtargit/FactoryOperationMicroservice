using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static FactoryOperation_Inventory.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.InventoryManagement
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        private readonly IEmailService _iEmailService;
        private readonly INotificationService _notificationService;

        public InventoryRepository(
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger,
            IEmailService iEmailService,
            INotificationService notificationService)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
            _iEmailService = iEmailService;
            _notificationService = notificationService;
        }

        public async Task<CommonResponseModel> CreateAsync(InventoryDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            try
            {
                var entity = new Inventory
                {
                    TenantId = dto.TenantId,
                    ItemCode = dto.ItemCode,
                    ItemName = dto.ItemName,
                    Manufacturer = dto.Manufacturer,
                    LocationId = dto.LocationId,
                    Status = dto.Status,
                    Category = dto.Category,
                    QuantityAvailable = dto.QuantityAvailable,
                    ReorderLevel = dto.ReorderLevel,
                    MaxStockLevel = dto.MaxStockLevel, 
                    ReservedQuantity = 0,
                    UnitPrice = dto.UnitPrice,
                    MonthlyConsumption = dto.MonthlyConsumption,
                    IsActive = true,
                    CreatedBy = dto.TenantId,
                    CreatedAt = DateTime.UtcNow
                };

                tenantDb.Inventory.Add(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    action: "Create",
                    details: $"Created inventory item {dto.ItemName}",
                    tenantId: dto.TenantId,
                    email: "",
                    eventType: "Inventory"
                );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryStatusMessage.Created;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "InventoryModule", "Add-Inventory", dto.TenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryStatusMessage.CreateFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<GetAllRecord<InventoryDto>> GetAllAsync(int tenantId)
        {
            var response = new GetAllRecord<InventoryDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var items = await tenantDb.Inventory
                    .Include(x => x.StorageLocation)
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                    .OrderByDescending(x => x.ItemId)
                    .ToListAsync();

                response.GetAllData = items.Select(i => new InventoryDto
                {
                    ItemId = i.ItemId,
                    TenantId = i.TenantId,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    LocationId = i.LocationId,
                    Location = i.StorageLocation != null ? i.StorageLocation.LocationName : string.Empty,
                    Status = i.Status,
                    Manufacturer = i.Manufacturer,
                    Category = i.Category,
                    QuantityAvailable = i.QuantityAvailable,
                    ReorderLevel = i.ReorderLevel,
                    MaxStockLevel = i.MaxStockLevel,
                    ReservedQuantity = i.ReservedQuantity,
                    UnitPrice = i.UnitPrice,
                    MonthlyConsumption = i.MonthlyConsumption,
                    IsActive = i.IsActive,
                    IsDeleted = i.IsDeleted
                }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryStatusMessage.FetchSuccess;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "InventoryModule", "GetAll-Inventory", tenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryStatusMessage.FetchFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<GetSpecificRecord<InventoryDto>> GetByIdAsync(int id, int tenantId)
        {
            var response = new GetSpecificRecord<InventoryDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var item = await tenantDb.Inventory
                    .Include(x => x.StorageLocation)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ItemId == id && x.TenantId == tenantId && !x.IsDeleted);

                if (item == null)
                {
                    response.StatusCode = StatusCode.NotFound; ;
                    response.StatusMessage = InventoryStatusMessage.NotFound;
                    return response;
                }

                response.Data = new InventoryDto
                {
                    ItemId = item.ItemId,
                    TenantId = item.TenantId,
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    Category = item.Category,
                    LocationId = item.LocationId,
                    Location = item.StorageLocation != null ? item.StorageLocation.LocationName : string.Empty,
                    Status = item.Status,
                    Manufacturer = item.Manufacturer,
                    QuantityAvailable = item.QuantityAvailable,
                    ReorderLevel = item.ReorderLevel,
                    MaxStockLevel = item.MaxStockLevel,
                    ReservedQuantity = item.ReservedQuantity,
                    UnitPrice = item.UnitPrice,
                    MonthlyConsumption = item.MonthlyConsumption,
                    IsActive = item.IsActive,
                    IsDeleted = item.IsDeleted
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryStatusMessage.FetchSuccess;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "InventoryModule", "Get-Inventory-ById", tenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryStatusMessage.FetchFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateAsync(InventoryDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            try
            {
                var existing = await tenantDb.Inventory
                    .FirstOrDefaultAsync(x => x.ItemId == dto.ItemId && x.TenantId == dto.TenantId && !x.IsDeleted);

                if (existing == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = InventoryStatusMessage.NotFound;
                    return response;
                }

                existing.ItemCode = dto.ItemCode;
                existing.ItemName = dto.ItemName;
                existing.Category = dto.Category;
                existing.Manufacturer = dto.Manufacturer;
                existing.LocationId = dto.LocationId;
                existing.Status = dto.Status;
                existing.QuantityAvailable = dto.QuantityAvailable;
                existing.ReorderLevel = dto.ReorderLevel;
                existing.MaxStockLevel = dto.MaxStockLevel;
                existing.UnitPrice = dto.UnitPrice;
                existing.MonthlyConsumption = dto.MonthlyConsumption;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = dto.TenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    action: "Update",
                    details: $"Updated inventory item {dto.ItemName}",
                    tenantId: dto.TenantId,
                    email: "",
                    eventType: "Inventory"
                );

                if (existing.QuantityAvailable <= existing.ReorderLevel)
                {
                    var title = $"Low Stock Alert: {existing.ItemName}";
                    var message = $"Item '{existing.ItemName}' is low on stock ({existing.QuantityAvailable}/{existing.ReorderLevel}). Please reorder.";
               
                    var notificationEntity = new MasterNotification
                    {
                        TenantId = existing.TenantId,
                        Module = "Inventory",
                        EntityId = existing.ItemId,
                        Title = title,
                        Message = message,
                        NotificationType = "LowStock",
                        TargetUserId = dto.UpdatedBy,
                        CreatedByUserId = dto.UpdatedBy,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        AdditionalData = JsonDocument.Parse(JsonSerializer.Serialize(new
                        {
                            existing.ItemCode,
                            existing.ItemName,
                            existing.QuantityAvailable,
                            existing.ReorderLevel
                        }))
                    };

                    tenantDb.MasterNotifications.Add(notificationEntity);
                    await tenantDb.SaveChangesAsync();


                    _ = Task.Run(() => _notificationService.NotifyLowStockAsync(new InventoryNotificationDto
                    {
                        TenantId = existing.TenantId,
                        ItemId = existing.ItemId,
                        ItemName = existing.ItemName,
                        QuantityAvailable = existing.QuantityAvailable,
                        ReorderLevel = existing.ReorderLevel,
                        Title = title,
                        Message = message,
                        TargetUserId = dto.UpdatedBy,
                        EventTime = DateTime.UtcNow
                    }));
                    var emailDto = new EmailDTO
                    {
                        From = "shoaibmaliklenovo@gmail.com",
                        To = "factory.operation@yopmail.com",
                        Subject = $"Low Stock Alert: {existing.ItemName}",
                        Body = $@"
                            <html>
                            <body>
                                <h3>Inventory Alert for {existing.ItemName}</h3>
                                <p>Dear Shopkeeper,</p>
                                <p>The stock for <strong>{existing.ItemName}</strong> has dropped to <strong>{existing.QuantityAvailable}</strong>, 
                                which is below the reorder level of <strong>{existing.ReorderLevel}</strong>.</p>
                                <p>Please reorder soon to avoid shortages.</p>
                                <p>Regards,<br/>Factory Operations System</p>
                            </body>
                            </html>"
                    };

                    var emailResponse = await _iEmailService.SendEmailAsync(emailDto);
                    if (!emailResponse.Success)
                    {
                        response.StatusCode = StatusCode.MultiStatus;
                        response.StatusMessage = InventoryStatusMessage.EmailFailed;
                        return response;
                    }
                }

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryStatusMessage.Updated;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "InventoryModule", "Update-Inventory", dto.TenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryStatusMessage.UpdateFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> DeleteAsync(int id, int tenantId)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var existing = await tenantDb.Inventory
                    .FirstOrDefaultAsync(x => x.ItemId == id && x.TenantId == tenantId && !x.IsDeleted);

                if (existing == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = InventoryStatusMessage.NotFound;
                    return response;
                }

                existing.IsDeleted = true;
                existing.IsActive = false;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = tenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    action: "Delete",
                    details: $"Deleted inventory item {existing.ItemName}",
                    tenantId: tenantId,
                    email: "",
                    eventType: "Inventory"
                );

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryStatusMessage.Deleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "InventoryModule", "Delete-Inventory", tenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryStatusMessage.DeleteFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<GetSpecificRecord<StockTrackingSummaryDto>> GetStockTrackingAsync(int tenantId)
        {
            var response = new GetSpecificRecord<StockTrackingSummaryDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var items = await tenantDb.Inventory
                    .Include(x => x.StorageLocation)
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                    .ToListAsync();

                var stockTrackingItems = items.Select(i => new StockTrackingDto
                {
                    ItemId = i.ItemId,
                    TenantId = i.TenantId,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    Location = i.StorageLocation != null ? i.StorageLocation.LocationName : "Unknown Location",
                    CurrentStock = i.QuantityAvailable,
                    Available = i.QuantityAvailable - i.ReservedQuantity,
                    Reserved = i.ReservedQuantity,
                    MinThreshold = i.ReorderLevel,
                    MaxThreshold = i.MaxStockLevel, 
                    Status = i.Status.ToString(),
                    LastUpdated = i.UpdatedAt ?? i.CreatedAt
                }).ToList();

                var summary = new StockTrackingSummaryDto
                {
                    TotalItems = items.Count,
                    LowStockAlerts = items.Count(i => i.QuantityAvailable <= i.ReorderLevel),
                    OutOfStock = items.Count(i => i.QuantityAvailable == 0),
                    StockLevels = stockTrackingItems
                };

                response.Data = summary;
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryStatusMessage.StockTrackingFetched;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "InventoryModule", "GetStockTracking", tenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryStatusMessage.StockTrackingFetchFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<GetSpecificRecord<StockReservationSummaryDto>> GetStockReservationsAsync(int tenantId)
        {
            var response = new GetSpecificRecord<StockReservationSummaryDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var items = await tenantDb.Inventory
                    .Include(x => x.StorageLocation)
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.ReservedQuantity > 0)
                    .ToListAsync();

                var reservationItems = items.Select(i => new StockReservationDto
                {
                    ItemId = i.ItemId,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    Location = i.StorageLocation != null ? i.StorageLocation.LocationName : "Unknown Location",
                    ReservedQuantity = i.ReservedQuantity,
                    AvailableQuantity = i.QuantityAvailable - i.ReservedQuantity,
                    TotalQuantity = i.QuantityAvailable,
                    Status = i.Status.ToString(),
                    LastUpdated = i.UpdatedAt ?? i.CreatedAt
                }).ToList();

                var summary = new StockReservationSummaryDto
                {
                    TotalReservedItems = items.Count,
                    TotalReservedQuantity = items.Sum(i => i.ReservedQuantity),
                    TotalReservedValue = items.Sum(i => i.ReservedQuantity * i.UnitPrice),
                    ReservationItems = reservationItems
                };

                response.Data = summary;
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryStatusMessage.StockReservationsFetched;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "InventoryModule", "GetStockReservations", tenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryStatusMessage.StockReservationsFetchFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<GetSpecificRecord<SerialBatchSummaryDto>> GetSerialBatchTrackingAsync(int tenantId)
        {
            var response = new GetSpecificRecord<SerialBatchSummaryDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var items = await tenantDb.Inventory
                    .Include(x => x.StorageLocation)
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                    .ToListAsync();

                var serialBatchItems = items.Select(i => new SerialBatchTrackingDto
                {
                    ItemId = i.ItemId,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    Location = i.StorageLocation != null ? i.StorageLocation.LocationName : "Unknown Location",
                    CurrentStock = i.QuantityAvailable,
                    Available = i.QuantityAvailable - i.ReservedQuantity,
                    BatchInfo = "Batch tracking details will be displayed here",
                    SerialInfo = "Serial tracking details will be displayed here", 
                    Status = i.Status.ToString(),
                    LastUpdated = i.UpdatedAt ?? i.CreatedAt
                }).ToList();

                var summary = new SerialBatchSummaryDto
                {
                    TotalTrackedItems = items.Count,
                    ItemsWithBatchInfo = items.Count(i => i.QuantityAvailable > 0),
                    ItemsWithSerialInfo = items.Count(i => i.ReservedQuantity > 0),
                    TrackedItems = serialBatchItems
                };

                response.Data = summary;
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryStatusMessage.SerialBatchFetched;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "InventoryModule", "GetSerialBatchTracking", tenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryStatusMessage.SerialBatchFetchFailed}: {ex.Message}";
            }

            return response;
        }
        public async Task<GetSpecificRecord<StockTrackingResponseDto>> GetStockTrackingSummaryAsync(int tenantId, StockTrackingType type)
        {
            var response = new GetSpecificRecord<StockTrackingResponseDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            try
            {
                var dto = new StockTrackingResponseDto { Type = type };

                switch (type)
                {
                    case StockTrackingType.Overview:
                        {
                            var items = await tenantDb.Inventory
                                .Include(x => x.StorageLocation)
                                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                                .ToListAsync();

                            var stockTrackingItems = items.Select(i => new StockTrackingDto
                            {
                                ItemId = i.ItemId,
                                TenantId = i.TenantId,
                                ItemCode = i.ItemCode,
                                ItemName = i.ItemName,
                                Location = i.StorageLocation != null ? i.StorageLocation.LocationName : "Unknown Location",
                                CurrentStock = i.QuantityAvailable,
                                Available = i.QuantityAvailable - i.ReservedQuantity,
                                Reserved = i.ReservedQuantity,
                                MinThreshold = i.ReorderLevel,
                                MaxThreshold = i.MaxStockLevel,
                                Status = i.Status.ToString(),
                                LastUpdated = i.UpdatedAt ?? i.CreatedAt
                            }).ToList();

                            dto.Overview = new StockTrackingSummaryDto
                            {
                                TotalItems = items.Count,
                                LowStockAlerts = items.Count(i => i.QuantityAvailable <= i.ReorderLevel),
                                OutOfStock = items.Count(i => i.QuantityAvailable == 0),
                                StockLevels = stockTrackingItems
                            };
                        }
                        break;

                    case StockTrackingType.Reservations:
                        {
                            var items = await tenantDb.Inventory
                                .Include(x => x.StorageLocation)
                                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.ReservedQuantity > 0)
                                .ToListAsync();

                            var reservationItems = items.Select(i => new StockReservationDto
                            {
                                ItemId = i.ItemId,
                                ItemCode = i.ItemCode,
                                ItemName = i.ItemName,
                                Location = i.StorageLocation != null ? i.StorageLocation.LocationName : "Unknown Location",
                                ReservedQuantity = i.ReservedQuantity,
                                AvailableQuantity = i.QuantityAvailable - i.ReservedQuantity,
                                TotalQuantity = i.QuantityAvailable,
                                Status = i.Status.ToString(),
                                LastUpdated = i.UpdatedAt ?? i.CreatedAt
                            }).ToList();

                            dto.Reservations = new StockReservationSummaryDto
                            {
                                TotalReservedItems = items.Count,
                                TotalReservedQuantity = items.Sum(i => i.ReservedQuantity),
                                TotalReservedValue = items.Sum(i => i.ReservedQuantity * i.UnitPrice),
                                ReservationItems = reservationItems
                            };
                        }
                        break;

                    case StockTrackingType.SerialBatch:
                        {
                            var items = await tenantDb.Inventory
                                .Include(x => x.StorageLocation)
                                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                                .ToListAsync();

                            var serialBatchItems = items.Select(i => new SerialBatchTrackingDto
                            {
                                ItemId = i.ItemId,
                                ItemCode = i.ItemCode,
                                ItemName = i.ItemName,
                                Location = i.StorageLocation != null ? i.StorageLocation.LocationName : "Unknown Location",
                                CurrentStock = i.QuantityAvailable,
                                Available = i.QuantityAvailable - i.ReservedQuantity,
                                BatchInfo = "Batch tracking details will be displayed here",
                                SerialInfo = "Serial tracking details will be displayed here",
                                Status = i.Status.ToString(),
                                LastUpdated = i.UpdatedAt ?? i.CreatedAt
                            }).ToList();

                            dto.SerialBatch = new SerialBatchSummaryDto
                            {
                                TotalTrackedItems = items.Count,
                                ItemsWithBatchInfo = items.Count(i => i.QuantityAvailable > 0),
                                ItemsWithSerialInfo = items.Count(i => i.ReservedQuantity > 0),
                                TrackedItems = serialBatchItems
                            };
                        }
                        break;
                }

                response.Data = dto;
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = InventoryStatusMessage.StockTrackingFetched;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex, "InventoryModule", "GetStockTrackingSummary", tenantId, null);

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{InventoryStatusMessage.StockTrackingFetchFailed}: {ex.Message}";
            }

            return response;
        }

    }
}