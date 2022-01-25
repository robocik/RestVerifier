using System;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.AutoFixture;

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

    protected virtual void Configure(Fixture fixture)
    {
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
    public virtual object? Create(Type type)
    {
        var obj = Fixture.Create(new SeededRequest(type, null), _context);
        return obj;
    }
}