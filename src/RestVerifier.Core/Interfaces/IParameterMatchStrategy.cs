using System.Collections.Generic;

namespace RestVerifier.Core.Interfaces;

public interface IParameterMatchStrategy
{
    void Match(IObjectsComparer comparer, IDictionary<string, object?> contextActionArguments, IList<ParameterValue> values);
}