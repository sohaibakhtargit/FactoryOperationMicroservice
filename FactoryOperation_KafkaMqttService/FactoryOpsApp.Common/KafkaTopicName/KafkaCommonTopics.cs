using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Common.KafkaTopicName
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
        public static string BuildTopicName(object tenantId, string actionType)
        {
            if (tenantId == null)
                throw new ArgumentNullException(nameof(tenantId));
            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));

            // Use StackTrace to get calling method and class name
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1);
            var method = frame?.GetMethod();

            string apiName = method?.DeclaringType?.Name ?? "UnknownController";

            apiName = CleanAsyncMethodName(apiName);

            // Format: controller/api/tenant/action
            return $"{apiName.ToLowerInvariant()}-{tenantId}-{actionType.ToLowerInvariant()}";
        }
        private static string CleanAsyncMethodName(string apiName)
        {
            // Match pattern like <MethodName>...
            var match = Regex.Match(apiName, @"<(.+?)>");
            return match.Success ? match.Groups[1].Value : apiName;
        }
    }
}
