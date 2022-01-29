using System.Collections.Generic;
using System.Linq;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Core.Matchers;

public class PositionMatchStrategy:IParameterMatchStrategy
{
    public void Match(IObjectsComparer comparer,IDictionary<string, object?> contextActionArguments, IList<ParameterValue> values)
    {
        var newValue = contextActionArguments.Values.ToList();
        for (var index = 0; index < values.Count; index++)
        {
            var originalValue = values[index];
            var value = newValue[index];
            comparer.Compare(value, originalValue.ValueToCompare);
        }
    }
}