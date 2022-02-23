using IntegrationTool.Core;
using System;
using static IntegrationTool.Core.ITransformationRule;

namespace IntegrationTool.Transform;

public class TransformationRule : ITransformationRule
{
    public string SourceField { get; set; }

    public string TargetField { get; set; }

    public As AsType { get; set; } = As.Unchanged;

    public ValueSetCompareRule? ValueSetMatch { get; set; }

    public TransformType Operation
    {
        get
        {
            var operation = TransformType.AS_IS;

            if (AsType is not As.Unchanged)
            {
                operation = TransformType.TYPE_CONVERSION;
            }

            if (Expression is not null)
            {
                operation |= TransformType.EXPRESSION;
            }

            if (ValueSetMatch is not null)
            {
                operation |= TransformType.VALUE_CONVERSION;
            }

            return operation;
        }
    }

    public Func<dynamic, IIntegrationLogger, dynamic> Expression { get; set; }
}
