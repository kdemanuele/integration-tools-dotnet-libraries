using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.Core
{
    public interface IValidate<TValidatationRule> where TValidatationRule: IValidationRule
    {
        public IIntegrationLogger Logger { get; init; }
        public RuleSetCollection<TValidatationRule> RuleSets { get; init; }

        public bool PerformValidation(dynamic source, string rulesetName);
    }
}
