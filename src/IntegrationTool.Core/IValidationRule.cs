using System;

namespace IntegrationTool.Core;

public interface IValidationRule : IRule
{
    public string Field { get; set; }

    public bool IsRequired { get; set; }

    public Func<dynamic, IIntegrationLogger, bool> Expression { get; set; }

    public string Pattern { get; set; }
}
