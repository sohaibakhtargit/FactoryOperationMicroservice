using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Handlers;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using MQTTnet.Client;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices;
    /// <summary>
    /// MQTT Communication API
    /// Manages MQTT messaging, IoT device communication, and data simulation
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MqttController : ControllerBase
    {
        private readonly IMqttService _mqttService;
        private readonly IIoTDataSimulator _simulator;
        private readonly ILogger<MqttController> _logger;
        private readonly IMqttMessageHandler _mqttMessageHandler;

        public MqttController(IMqttService mqttService, IIoTDataSimulator simulator, ILogger<MqttController> logger, IMqttMessageHandler mqttMessageHandler)
        {
            _mqttService = mqttService;
            _simulator = simulator;
            _logger = logger;
            _mqttMessageHandler = mqttMessageHandler;
        }

        /// <summary>
        /// Publish MQTT message
        /// Publishes message to specified MQTT topic
        /// </summary>
        /// <param name="request">MQTT publish request data</param>
        /// <returns>Publish operation result</returns>
        /// <response code="200">Message successfully published</response>
        /// <response code="500">MQTT publish error occurred</response>
        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] MqttPublishRequest request)
        {
            try
            {
                await _mqttService.PublishAsync(request.Topic, request.Message, request.Retain);
                return Ok(new { message = "Message published successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing MQTT message");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Subscribe to MQTT topic
        /// Subscribes to specified MQTT topic for message reception
        /// </summary>
        /// <param name="request">MQTT subscribe request data</param>
        /// <returns>Subscribe operation result</returns>
        /// <response code="200">Successfully subscribed to topic</response>
        /// <response code="500">MQTT subscribe error occurred</response>
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] MqttSubscribeRequest request)
        {
            try
            {
                await _mqttService.SubscribeAsync(request.Topic);
                return Ok(new { message = $"Subscribed to topic: {request.Topic}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to MQTT topic");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Start IoT simulation
        /// Starts IoT data simulation for testing and development
        /// </summary>
        /// <returns>Simulation start result</returns>
        /// <response code="200">IoT simulation successfully started</response>
        /// <response code="500">Simulation start error occurred</response>
        [HttpPost("simulation/start")]
        public async Task<IActionResult> StartSimulation()
        {
            try
            {
                await _simulator.StartSimulationAsync();
                return Ok(new { message = "IoT simulation started" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting simulation");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Stop IoT simulation
        /// Stops running IoT data simulation
        /// </summary>
        /// <returns>Simulation stop result</returns>
        /// <response code="200">IoT simulation successfully stopped</response>
        /// <response code="500">Simulation stop error occurred</response>
        [HttpPost("simulation/stop")]
        public async Task<IActionResult> StopSimulation()
        {
            try
            {
                await _simulator.StopSimulationAsync();
                return Ok(new { message = "IoT simulation stopped" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping simulation");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get MQTT status
        /// Retrieves current MQTT connection and simulation status
        /// </summary>
        /// <returns>MQTT status information</returns>
        /// <response code="200">Successfully retrieved MQTT status</response>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var simulationRunning = _simulator is IoTDataSimulator simulator
                ? simulator.IsSimulationRunning
                : false;

            return Ok(new
            {
                connected = _mqttService.IsConnected,
                simulationRunning
            });
        }

        /// <summary>
        /// Get debug configuration
        /// Retrieves MQTT configuration details for debugging
        /// </summary>
        /// <param name="config">Configuration service</param>
        /// <returns>MQTT configuration details</returns>
        /// <response code="200">Successfully retrieved configuration</response>
        [HttpGet("debug/config")]
        public IActionResult GetConfig([FromServices] IConfiguration config)
        {
            return Ok(new
            {
                brokerUrl = config["Mqtt:BrokerUrl"],
                port = config["Mqtt:Port"],
                clientId = config["Mqtt:ClientId"]
            });
        }

        /// <summary>
        /// Test MQTT connection
        /// Tests connection to external MQTT broker for diagnostics
        /// </summary>
        /// <returns>Connection test result</returns>
        /// <response code="200">Connection test completed</response>
        /// <response code="500">Connection test failed</response>
        [HttpGet("debug/test-connect")]
        public async Task<IActionResult> TestConnect()
        {
            try
            {
                // Create a temporary client to test connection
                var factory = new MqttFactory();
                using var client = factory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("test.mosquitto.org", 1883)
                    .WithClientId("test-client")
                    .WithCleanSession()
                    .Build();

                var result = await client.ConnectAsync(options);
                return Ok(new
                {
                    success = result.ResultCode == MqttClientConnectResultCode.Success,
                    resultCode = result.ResultCode.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test message handler
        /// Manually invokes MQTT message handler for testing purposes
        /// </summary>
        /// <param name="payload">Test message payload</param>
        /// <returns>Handler invocation result</returns>
        /// <response code="200">Handler successfully invoked</response>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("test-handler")]
        public async Task<IActionResult> TestHandler([FromBody] string payload)
        {
            await _mqttMessageHandler.HandleSensorDataMessageAsync("factory/test", payload);
            return Ok("Handler invoked manually");
        }
    }
