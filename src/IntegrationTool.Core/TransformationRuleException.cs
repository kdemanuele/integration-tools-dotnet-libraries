using System;

#nullable enable

namespace IntegrationTool.Core;

public class TransformationRuleException : ArgumentException
{
    public TransformationRuleException(string? message, string? paramName) : base(message, paramName)
    { }

    public TransformationRuleException(string? message) : base(message)
    { }
}
