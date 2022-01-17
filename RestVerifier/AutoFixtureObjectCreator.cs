using System;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;

namespace RestVerifier;

public class AutoFixtureObjectCreator:ITestObjectCreator
{
    private SpecimenContext _context;
    public Fixture Fixture { get; }

    public AutoFixtureObjectCreator()
    {
        Fixture = new Fixture();
        _context = new SpecimenContext(Fixture);
        Configure(Fixture);
    }

    private  void Configure(Fixture fixture)
    {
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
    public object? Create(Type type)
    {
        var obj = Fixture.Create(new SeededRequest(type, null), _context);
        return obj;
    }
}