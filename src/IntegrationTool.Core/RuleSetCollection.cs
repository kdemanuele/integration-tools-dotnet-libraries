using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTool.Core
{
    public class RuleSetCollection<TRule> where TRule: IRule
    {
        private readonly Dictionary<string, Dictionary<string, List<TRule>>> collection = new();

        /// <summary>
        /// This method checks if the rule set needs to be created or not. If it already exists it will simply skip any operation.
        /// 
        /// It is safe to be called each time a rule set is initialised
        /// </summary>
        /// <param name="rulesetName">Name of the Rule Set to be added</param>
        public RuleSetCollection<TRule> InitialiseRuleSet(string rulesetName)
        {
            return InitialiseRuleSet(rulesetName, null);
        }

        /// <summary>
        /// This method checks if the rule set needs to be created or not. If it already exists it will simply skip any operation.
        /// 
        /// It is safe to be called each time a rule set is initialised
        /// </summary>
        /// <param name="rulesetName">Name of the Rule Set to be added</param>
        /// <param name="targetNames">List of names with which each transformation will be identified in the output</param>
        public RuleSetCollection<TRule> InitialiseRuleSet(string rulesetName, params string[] targetNames)
        {
            if (rulesetName is null)
            {
                throw new ArgumentNullException(nameof(rulesetName), "Rule Set name cannot be null");
            }

            if (!collection.ContainsKey(rulesetName))
            {
                collection[rulesetName] = new();
            }

            if (targetNames is not null)
            {
                foreach (var target in targetNames)
                {
                    collection[rulesetName].Add(target, new());
                }
            }

            return this;
        }

        /// <summary>
        /// Determines if the Rule Set exists and has been properly initialised
        /// </summary>
        /// <param name="rulesetName">Name for the rule set</param>
        /// <returns>The state of initialisation for the rule set</returns>
        public RuleSetState GetRuleSetState(string rulesetName)
        {
            if (!collection.ContainsKey(rulesetName))
            {
                return RuleSetState.Undeclared;
            }

            if (collection[rulesetName].Keys.Count == 0)
            {
                return RuleSetState.No_Targets_Defined;
            }

            foreach (var rules in collection[rulesetName])
            {
                if (rules.Value.Count == 0)
                {
                    return RuleSetState.Target_Without_Rules_Defined;
                }    
            }

            return RuleSetState.Initialised;
        }

        public Dictionary<string, List<TRule>> this[string rulesetName] => collection[rulesetName];
    }

    public enum RuleSetState
    {
        Initialised,
        No_Targets_Defined,
        Target_Without_Rules_Defined,
        Undeclared
    }
}
