using FluentAssertions;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.FluentAssertions;

public class FluentAssertionComparer:IObjectsComparer
{
    public virtual void Compare(object? obj1, object? obj2,string? message=null)
    {
        obj1.Should().BeEquivalentTo(obj2, h => h.IgnoringCyclicReferences(),message);
    }
}