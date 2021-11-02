using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.Core.UnitTest.Mocks
{
    public class MockTransformationRule : ITransformationRule
    {
        public string SourceField { get; set; }
        public string TargetField { get; set; }
        public ITransformationRule.As AsType { get; set; }
        public ValueSetCompareRule? ValueSetMatch { get; set; }

        public ITransformationRule.TransformType Operation => throw new NotImplementedException();

        public Func<dynamic, IIntegrationLogger, dynamic> Expression { get; set; }
    }
}
