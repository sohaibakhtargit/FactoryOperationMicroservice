using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.WorkOrderManagement
{
    public static class KafkaCommonTopics
    {
        /// <summary>
        /// Builds a dynamic Kafka topic name in the pattern:
        /// ControllerName / ApiName / TenantId / ActionType
        /// Automatically detects the calling controller and method name.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="actionType">The action type (e.g., Created, Updated).</param>
        /// <returns>Formatted Kafka topic name.</returns>
        public static string BuildTopicName(int tenantId, string actionType)
        {
            if (tenantId <= 0)
                throw new ArgumentNullException(nameof(tenantId));
            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));

            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1);
            var method = frame?.GetMethod();

            string apiName = method?.DeclaringType?.Name ?? "unknown";
            apiName = CleanAsyncMethodName(apiName);

            apiName = apiName
                .Replace("Controller", "")
                .Replace("Api", "")
                .ToLowerInvariant();

            
            return $"factoryops.{apiName}.{tenantId}.{actionType.ToLowerInvariant()}";
        }

        private static string CleanAsyncMethodName(string apiName)
        {
            var match = Regex.Match(apiName, @"<(.+?)>");
            return match.Success ? match.Groups[1].Value : apiName;
        }

        public static string BuildTopicNameWorkOrderProgressTest(
    int tenantId,
    string apiName,
    string actionType)
        {
            if (tenantId <= 0)
                throw new ArgumentNullException(nameof(tenantId));

            if (string.IsNullOrWhiteSpace(apiName))
                throw new ArgumentException("Api name cannot be empty", nameof(apiName));

            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));

            apiName = apiName
                .Replace("Controller", "")
                .Replace("Api", "")
                .ToLowerInvariant();

            return $"factoryops.{apiName}.{tenantId}.{actionType.ToLowerInvariant()}";
        }

    }
}
