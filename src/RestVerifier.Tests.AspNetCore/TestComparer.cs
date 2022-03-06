using FluentAssertions;
using RestVerifier.FluentAssertions;

namespace RestVerifier.Tests.AspNetCore;

public class TestComparer: FluentAssertionComparer
{
    public override void Compare(object obj1, object obj2, string message = null)
    {
        if (obj1 is Stream stream1 && obj2 is Stream stream2)
        {
            stream1.Should().HaveLength(stream2.Length);
            return;
        }
        base.Compare(obj1, obj2, message);
    }
}