using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.BulkImportFileSample;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_WorkOrder.Controllers.TenantAdminController.BulkImportFileSample
{
    [Route("api/[controller]")]
    [ApiController]
    public class BulkImportSampleController : ControllerBase
    {
        private readonly IBulkImportSampleService _bulkImportSampleService;

        public BulkImportSampleController(IBulkImportSampleService bulkImportSampleService)
        {
            _bulkImportSampleService = bulkImportSampleService;
        }

        [HttpPost("upload-sample")]
        public async Task<IActionResult> UploadSample([FromForm] BulkImportSampleDto bulkImportSampleDto)
        {
            await _bulkImportSampleService.UploadSampleAsync(bulkImportSampleDto);
            return Ok(new { Message = "Sample file uploaded successfully." });
        }

        [HttpGet("download-sample")]
        public async Task<IActionResult> DownloadSample([FromQuery] int tenantId, [FromQuery] string moduleName)
        {
            var (fileBytes, fileName) = await _bulkImportSampleService.DownloadSampleAsync(tenantId, moduleName);
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}
