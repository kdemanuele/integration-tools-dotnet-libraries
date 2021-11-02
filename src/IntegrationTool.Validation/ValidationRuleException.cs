using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace IntegrationTool.Validation
{
    public class ValidationRuleException : ArgumentException
    {
        public ValidationRuleException(string? message, string? paramName) : base(message, paramName)
        { }

        public ValidationRuleException(string? message) : base(message)
        { }
    }
}