namespace IntegrationTool.Core;

public interface ITransform<TTransformationRule> where TTransformationRule : ITransformationRule
{
    public ValueSet ValueSet { get; init; }
    public IIntegrationLogger Logger { get; init; }
    public RuleSetCollection<TTransformationRule> RuleSets { get; init; }

    public dynamic PerformTransformation(dynamic source, string rulesetName);
}
