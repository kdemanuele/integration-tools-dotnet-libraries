using IntegrationTool.Core;
using log4net;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.Core
{
    public interface ITransform<TTransformationRule> where TTransformationRule: ITransformationRule
    {
        public ValueSet ValueSet { get; init; }
        public IIntegrationLogger Logger { get; init; }
        public RuleSetCollection<TTransformationRule> RuleSets { get; init; }

        public dynamic PerformTransformation(dynamic source, string rulesetName);
    }
}
