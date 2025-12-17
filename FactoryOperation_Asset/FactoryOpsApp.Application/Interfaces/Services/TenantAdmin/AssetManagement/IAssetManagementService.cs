using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement
{
    public interface IAssetManagementService
    {
        Task<CommonResponseModel> AddAsset(AssetRegistryDto dto);

        Task<CommonResponseModel> UpdateAsset(AssetRegistryDto dto);

        GetAllRecord<GetAssetRegistryDto> GetAllAssets(int TenantId);

        Task<CommonResponseModel> DeleteAsset(int id, int TenantId);

    }
}
