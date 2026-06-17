using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FactoryOperation_Asset.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.Common
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

            // Identify API or controller name
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1);
            var method = frame?.GetMethod();

            string apiName = method?.DeclaringType?.Name ?? "unknown";
            apiName = CleanAsyncMethodName(apiName);

            // Remove suffix like "Controller"
            apiName = apiName
                .Replace("Controller", "")
                .Replace("Api", "")
                .ToLowerInvariant();

            // Format: factoryops.<entity>.<tenant>.<event>
            return $"factoryops.{apiName}.{tenantId}.{actionType.ToLowerInvariant()}";
        }

        private static string CleanAsyncMethodName(string apiName)
        {
            var match = Regex.Match(apiName, @"<(.+?)>");
            return match.Success ? match.Groups[1].Value : apiName;
        }

    }
}
