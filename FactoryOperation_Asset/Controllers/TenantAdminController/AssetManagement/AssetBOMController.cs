using FactoryOperation_Asset.FactoryOpsApp.Application.DTOs;
using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_Asset.Controllers.TenantAdminController.AssetManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetBOMController : ControllerBase
    {
        private readonly IAssetBOMService _assetBOMService;
        public AssetBOMController(IAssetBOMService assetBOMService)
        {
                _assetBOMService = assetBOMService;
        }

        [HttpPost("AddAssetBomPart")]

        public async Task<IActionResult> AddAssetBomPart(AssetBOMDto dto)
        {
            var result = await _assetBOMService.AddBOMPart(dto);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet("GetBOMPart")]
        public async Task<IActionResult> GetBOMPart(int tenantId)
        {
            var result = await _assetBOMService.GetBOMPart(tenantId);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet("GetBomById")]
        public async Task<IActionResult> GetBomById(int bomPartId, int tenantId)
        {
            var result = await _assetBOMService.GetBOMById(bomPartId, tenantId);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpPost("UpdateAssetBomPart")]
        public async Task<IActionResult> UpdateAssetBomPart(UpdateAssetBomDto dto)
        {
            var result = await _assetBOMService.UpdateBOMPart(dto);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpPost("DeleteAssetBomPart")]
        public async Task<IActionResult> DeleteAssetBomPart(DeleteAssetBomDto dto)
        {
            var result = await _assetBOMService.DeleteBOMPart(dto);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest();
        }

    }
}
