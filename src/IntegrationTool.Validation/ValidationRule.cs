using IntegrationTool.Core;
using System;

namespace IntegrationTool.Validation;

public class ValidationRule : IValidationRule
{
    public string Field { get; set; }
    public bool IsRequired { get; set; } = false;
    public Func<dynamic, IIntegrationLogger, bool> Expression { get; set; }
    public string Pattern { get; set; }
}
