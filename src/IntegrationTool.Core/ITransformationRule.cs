using System;

namespace IntegrationTool.Core;

public interface ITransformationRule : IRule
{
    public string SourceField { get; set; }

    public string TargetField { get; set; }

    public As AsType { get; set; }

    public ValueSetCompareRule? ValueSetMatch { get; set; }

    public TransformType Operation { get; }

    public Func<dynamic, IIntegrationLogger, dynamic> Expression { get; set; }

    [Flags]
    public enum TransformType
    {
        AS_IS,
        TYPE_CONVERSION,
        EXPRESSION,
        VALUE_CONVERSION = 4
    }

    public enum As
    {
        Unchanged,
        String,
        WholeNumber,
        Double,
        IsoDate,
        IsoUtcDate,
        SerializeJson
    }
}
