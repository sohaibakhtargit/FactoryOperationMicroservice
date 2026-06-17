using FactoryOperation_Asset.FactoryOpsApp.Application.DTOs;
using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOperation_Asset.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_Asset.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.AssetManagement
{
    public class AssetBOMRepository: IAssetBOMRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;

        public AssetBOMRepository(
            TenantDbContextFactory tenantDbContext, 
            IAuditLogService auditLogger
            )
        {
            _tenantDbContext= tenantDbContext;
            _auditLogger = auditLogger;
        }

        public async Task<CommonResponseModel> AddBOMPart(AssetBOMDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                var bomPartEntity = new AssetBillOfMaterials
                {
                    TenantId = dto.TenantId,
                    AssetId = dto.AssetId,
                    PartName = dto.PartName,
                    PartNumber = dto.PartNumber,
                    Description = dto.Description,
                    Category = dto.Category,
                    Quantity = dto.Quantity,
                    UnitCost = dto.UnitCost,
                    MinimumStockLevel = dto.MinimumStockLevel,
                    LeadTimeDays = dto.LeadTimeDays,
                    Supplier = dto.Supplier,
                    StorageLocation = dto.StorageLocation,
                    CompatibleModels = dto.CompatibleModels,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = dto.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                tenantDb.AssetBillOfMaterials.Add(bomPartEntity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    "AssetBOM",
                    "Create",
                    dto.CreatedBy,
                    dto.TenantId.ToString(),
                    bomPartEntity.BomPartId.ToString()
                );

                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = "BOM part created successfully.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

            return response;
        }

        public async Task<CommonResponseModel> UpdateBOMPart(UpdateAssetBomDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            using var transaction = await tenantDb.Database.BeginTransactionAsync();

            try
            {
                var existing = await tenantDb.AssetBillOfMaterials
                    .FirstOrDefaultAsync(x =>
                        x.BomPartId == dto.BomPartId &&
                        x.AssetId == dto.AssetId &&
                        !x.IsDeleted);

                if (existing == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = "BOM part not found.";
                    return response;
                }

                existing.PartName = dto.PartName;
                existing.PartNumber = dto.PartNumber;
                existing.Description = dto.Description;
                existing.Category = dto.Category;
                existing.Quantity = dto.Quantity;
                existing.UnitCost = dto.UnitCost;
                existing.MinimumStockLevel = dto.MinimumStockLevel;
                existing.LeadTimeDays = dto.LeadTimeDays;
                existing.Supplier = dto.Supplier;
                existing.StorageLocation = dto.StorageLocation;
                existing.CompatibleModels = dto.CompatibleModels;
                existing.UpdatedBy = dto.UpdatedBy;
                existing.UpdatedAt = DateTime.UtcNow;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    "AssetBOM",
                    "Update",
                    dto.UpdatedBy,
                    dto.TenantId.ToString(),
                    existing.BomPartId.ToString());

                await transaction.CommitAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = "BOM part updated successfully.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            return response;
        }


        public async Task<GetAllRecord<GetAssetBOMDto>> GetBOMPart(int tenantId)
        {
            var response = new GetAllRecord<GetAssetBOMDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var items = await tenantDb.AssetBillOfMaterials
              .Include(x => x.AssetRegistry) 
              .Where(x => !x.IsDeleted)
              .OrderByDescending(x => x.BomPartId)
              .Select(x => new GetAssetBOMDto
              {
                  BomPartId = x.BomPartId,
                  AssetId = x.AssetId,
                  AssetName = x.AssetRegistry.AssetName,
                  PartNumber = x.PartNumber,
                  PartName = x.PartName,
                  Category = x.Category,
                  Quantity = x.Quantity,
                  UnitCost = x.UnitCost,
                  MinimumStockLevel = x.MinimumStockLevel,
                  Supplier = x.Supplier,
                  IsActive = x.IsActive,
                  Description = x.Description
              })
              .ToListAsync();

            response.GetAllData = items;
            response.StatusCode = StatusCode.Success;
            response.StatusMessage = "BOM parts fetched successfully.";

            return response;
        }


        public async Task<GetSpecificRecord<GetAssetBOMDto>> GetBOMById(int bomPartId, int tenantId)
        {
            var response = new GetSpecificRecord<GetAssetBOMDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var item = await tenantDb.AssetBillOfMaterials
                .Include(x => x.AssetRegistry)
                .Where(x =>
                    x.BomPartId == bomPartId &&
                    !x.IsDeleted)
                .Select(x => new GetAssetBOMDto
                {
                    BomPartId = x.BomPartId,
                    AssetId = x.AssetId,
                    AssetName = x.AssetRegistry.AssetName,
                    PartNumber = x.PartNumber,
                    PartName = x.PartName,
                    Category = x.Category,
                    Quantity = x.Quantity,
                    UnitCost = x.UnitCost,
                    MinimumStockLevel = x.MinimumStockLevel,
                    Supplier = x.Supplier,
                    IsActive = x.IsActive,
                    Description = x.Description
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                response.StatusCode = StatusCode.NotFound;
                response.StatusMessage = "BOM part not found.";
                return response;
            }

            response.Data = item;
            response.StatusCode = StatusCode.Success;
            response.StatusMessage = "BOM part fetched successfully.";

            return response;
        }


        public async Task<CommonResponseModel> DeleteBOMPart(DeleteAssetBomDto dto)
        {
            var response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

            var existing = await tenantDb.AssetBillOfMaterials
                .FirstOrDefaultAsync(x =>
                    x.BomPartId == dto.BomPartId &&
                    !x.IsDeleted);

            if (existing == null)
            {
                response.StatusCode = StatusCode.NotFound;
                response.StatusMessage = "BOM part not found.";
                return response;
            }

            existing.IsDeleted = true;
            existing.IsActive = false;
            existing.DeletedBy = dto.DeletedBy;
            existing.DeletedAt = DateTime.UtcNow;

            await tenantDb.SaveChangesAsync();

            await _auditLogger.LogAuditAsync(
                "AssetBOM",
                "Delete",
                dto.DeletedBy,
                dto.TenantId.ToString(),
                existing.BomPartId.ToString());

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = "BOM part deleted successfully.";

            return response;
        }

    }
}
