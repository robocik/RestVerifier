using System;
using System.Collections.Generic;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Core.Matchers;

public class NameMatchStrategy:IParameterMatchStrategy
{
    public void Match(IObjectsComparer comparer,IDictionary<string, object?> contextActionArguments, IList<ParameterValue> values)
    {
        for (var index = 0; index < values.Count; index++)
        {
            var originalValue = values[index];
            if (string.IsNullOrEmpty(originalValue.Name))
            {
                throw new ArgumentNullException("Name","One or more parameters doesn't have a name but match parameters strategy is set to use names");
            }

            if (contextActionArguments.TryGetValue(originalValue.Name!, out var value))
            {
                comparer.Compare(value, originalValue.ValueToCompare);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Parameter {originalValue.Name} not found");
            }
        }
    }
}