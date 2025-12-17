using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.IOTDevices
{
    /// <summary>
    /// MQTT Topic Management API
    /// Manages MQTT topics, topic configurations, and topic subscriptions
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MqttTopicController : ControllerBase
    {
        private readonly IFactoryMqttTopicService _mqttTopicService;

        public MqttTopicController(IFactoryMqttTopicService mqttTopicService)
        {
            _mqttTopicService = mqttTopicService;
        }

        /// <summary>
        /// Get all MQTT topics
        /// Retrieves complete list of all MQTT topics for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of MQTT topics</returns>
        /// <response code="200">Successfully retrieved all MQTT topics</response>
        [HttpGet("Get-AllMqttTopic")]
        public async Task<IActionResult> GetAllMqttTopic(int tenantId)
        {
            var result = await _mqttTopicService.GetAllMqttTopicAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get MQTT topic by ID
        /// Retrieves specific MQTT topic details and configuration
        /// </summary>
        /// <param name="topicId">Topic identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>MQTT topic details</returns>
        /// <response code="200">Successfully retrieved MQTT topic</response>
        [HttpGet("Get-MqttTopic-ById")]
        public async Task<IActionResult> GetMqttTopicById(int topicId, int tenantId)
        {
            var result = await _mqttTopicService.GetMqttTopicByIdAsync(topicId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Add MQTT topic
        /// Creates new MQTT topic configuration for device communication
        /// </summary>
        /// <param name="dto">MQTT topic data</param>
        /// <returns>Topic creation result</returns>
        /// <response code="200">MQTT topic successfully added</response>
        /// <response code="400">Invalid topic data provided</response>
        [HttpPost("Add-MqttTopic")]
        public async Task<IActionResult> AddMqttTopic([FromBody] MqttTopicDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mqttTopicService.AddMqttTopicAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update MQTT topic
        /// Modifies existing MQTT topic configuration and settings
        /// </summary>
        /// <param name="dto">Updated topic data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">MQTT topic successfully updated</response>
        /// <response code="400">Invalid topic data provided</response>
        [HttpPost("Update-MqttTopic")]
        public async Task<IActionResult> UpdateMqttTopic([FromBody] MqttTopicDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mqttTopicService.UpdateMqttTopicAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete MQTT topic
        /// Removes MQTT topic configuration from system
        /// </summary>
        /// <param name="topicId">Topic identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">MQTT topic successfully deleted</response>
        [HttpPost("Delete-MqttTopic")]
        public async Task<IActionResult> DeleteMqttTopic(int topicId, int tenantId)
        {
            var result = await _mqttTopicService.DeleteMqttTopicAsync(topicId, tenantId);
            return Ok(result);
        }
    }
}