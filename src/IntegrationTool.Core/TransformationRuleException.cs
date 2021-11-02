using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace IntegrationTool.Core
{
    public class TransformationRuleException : ArgumentException
    {
        public TransformationRuleException(string? message, string? paramName) : base(message, paramName)
        { }

        public TransformationRuleException(string? message) : base(message)
        { }
    }
}
