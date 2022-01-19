using FluentAssertions;
using RestVerifier.Interfaces;

namespace RestVerifier;

public class FluentAssertionComparer:IObjectsComparer
{
    public void Compare(object? obj1, object? obj2)
    {
        obj1.Should().BeEquivalentTo(obj2, h => h.IgnoringCyclicReferences());
    }
}