using System;

#nullable enable

namespace IntegrationTool.Validation;

public class ValidationRuleException : ArgumentException
{
    public ValidationRuleException(string? message, string? paramName) : base(message, paramName)
    { }

    public ValidationRuleException(string? message) : base(message)
    { }
}
