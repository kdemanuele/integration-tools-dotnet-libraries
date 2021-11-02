using IntegrationTool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.Validation
{
    public class ValidationRule : IValidationRule
    {
        public string Field { get; set; }
        public bool IsRequired { get; set; } = false;
        public Func<dynamic, IIntegrationLogger, bool> Expression { get; set; }
        public string Pattern { get; set; }
    }
}
