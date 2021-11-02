using IntegrationTool.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IntegrationTool.Transform
{
    class TypeConverter
    {
        internal static object ConvertToIsoUtcDateTime(dynamic source, TransformationRule rule, IIntegrationLogger logger)
        {
            DateTime dateTime = DateTime.MinValue;

            if (source is null || !DateTime.TryParse(source.ToString(), out dateTime))
            {
                logger.LogWarning($"The source field {rule.SourceField} has a null value or a non valid Date Time: {source}");
                return null;
            }

            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffff%K");
        }

        internal static object ConvertToIsoDateTime(dynamic source, TransformationRule rule, IIntegrationLogger logger)
        {
            DateTime dateTime = DateTime.MinValue;

            if (source is null || !DateTime.TryParse(source.ToString(), out dateTime))
            {
                logger.LogWarning($"The source field {rule.SourceField} has a null value or a non valid Date Time: {source}");
                return null;
            }

            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffff%K");
        }

        internal static double? ConvertToDouble(dynamic source, TransformationRule rule, IIntegrationLogger logger)
        {
            double numeric = default;

            if (source is null || !double.TryParse(source.ToString(), out numeric))
            {
                logger.LogWarning($"The source field {rule.SourceField} has a null value or a non valid numeric value: {source}");
                return null;
            }

            return numeric;
        }

        internal static long? ConvertToLong(dynamic source, TransformationRule rule, IIntegrationLogger logger)
        {
            long numeric = default;

            if (source is null || !long.TryParse(source.ToString(), out numeric))
            {
                logger.LogWarning($"The source field {rule.SourceField} has a null value or a non valid numeric value: {source}");
                return null;
            }

            return numeric;
        }

        internal static string SerializeJson(dynamic source, TransformationRule rule, IIntegrationLogger logger)
        {
            try
            {
                return JsonConvert.SerializeObject(source);
            }
            catch (Exception ex)
            {
                logger.LogError($"The value in the field {rule.SourceField} is not JSON serializable.", ex);
            }

            return null;
        }
    }
}
