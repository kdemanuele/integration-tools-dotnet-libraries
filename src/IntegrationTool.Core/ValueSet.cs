using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntegrationTool.Core.ValueSetCompareRule;

namespace IntegrationTool.Core
{
    public class ValueSet
    {
        private readonly DataSet dataSet;

        public ValueSet()
        {
            dataSet = new DataSet();
        }

        /// <summary>
        /// Adds a specific label-value combination to the specified set
        /// </summary>
        /// <param name="setName">Set name</param>
        /// <param name="label">The text used as a label for the label-value combination</param>
        /// <param name="value">The text used as a value for the label-value combination</param>
        public void AddValueToSet(string setName, string label, string value)
        {
            if (!dataSet.Tables.Contains(setName))
            {
                DataTable dataTable = new(setName);
                dataTable.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Label", Type.GetType("System.String")),
                    new DataColumn("Value", Type.GetType("System.String"))
                });
                dataSet.Tables.Add(dataTable);
            }

            dataSet.Tables[setName].Rows.Add(label, value);
        }

        /// <summary>
        /// Gets the Value Set definition
        /// </summary>
        /// <param name="setName">Set to return</param>
        /// <returns>A table representation of the set information</returns>
        public DataTable GetValueSet(string setName)
        {
            if (dataSet.Tables.Contains(setName))
            {
                return dataSet.Tables[setName];
            }

            throw new TransformationRuleException($"A reference to the Value Set collection with name '{setName}' is not defined");
        }

        /// <summary>
        /// Returns the first match for the given source within the specified set
        /// </summary>
        /// <param name="setName">Set to use for value</param>
        /// <param name="source">The text used for the lookup</param>
        /// <param name="matchOn">Determines the field to match on</param>
        /// <param name="caseSensitive">By Default, it is case sensitive match</param>
        /// <returns>A tuple with Label (Item1) and Value (Item2) of the matching set record</returns>
        public (string, string) GetLabelAndValue(string setName, string source, MatchOn matchOn, bool caseSensitive)
        {
            var table = GetValueSet(setName);
            var comparison = caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

            var row = table.AsEnumerable().FirstOrDefault(row => row.Field<string>(matchOn.ToString()).Equals(source, comparison));

            if (row is null)
            {
                return (null, null);
            }

            return (row.Field<string>("Label"), row.Field<string>("Value"));
        }
    }
}
