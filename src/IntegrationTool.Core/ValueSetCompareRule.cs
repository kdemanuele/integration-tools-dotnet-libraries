namespace IntegrationTool.Core;

public struct ValueSetCompareRule
{
    public ValueSetCompareRule(string setName, MatchOn matchOn, bool caseSensitive)
    {
        SetName = setName;
        MatchRule = matchOn;
        IsCaseSensitiveMatch = caseSensitive;
    }

    public string SetName { get; }

    public MatchOn MatchRule { get; }

    public bool IsCaseSensitiveMatch { get; }

    public enum MatchOn
    {
        Label,
        Value
    }
}
