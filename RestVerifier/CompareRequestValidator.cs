using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RestVerifier.Configurator;

namespace RestVerifier;



public class CompareRequestValidator
{
    private IObjectsComparer _comparer;
    public ITestObjectCreator Creator { get; }
    private ValidationContext _context = new ();
    private Type? _returnType;
    private MethodConfiguration? _currentMethod;
    public CompareRequestValidator()
    {
        _comparer = new FluentAssertionComparer();
        Creator = new AutoFixtureObjectCreator();
    }

    public CompareRequestValidator(IObjectsComparer comparer, ITestObjectCreator creator)
    {
        _comparer = comparer;
        Creator = creator;
    }

    public IRemoteServiceContext Context => _context;

    public object? AddReturnType(Type? type)
    {
        _returnType ??= type;
        if (_returnType==null || _returnType == typeof(void) || _returnType == typeof(Task))
        {
            return null;
        }

        if (_returnType.IsGenericType && _returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            _returnType = _returnType.GetGenericArguments().First();
        }
        
        var returnObject = Creator.Create(_returnType);
        
        
        Context.AddReturnValue(returnObject);
        
        if (_currentMethod?.ReturnTransform != null)
        {
            returnObject = (object?)_currentMethod.ReturnTransform.DynamicInvoke(returnObject);
        }
        return returnObject;
    }

    public bool ValidateParams(IDictionary<string, object?> contextActionArguments)
    {
        var values = _context.ValuesToCompare.ToList();
        _comparer.Compare(contextActionArguments.Count, values.Count);

        if (contextActionArguments.Values.Count > 0)
        {
            var newValue = contextActionArguments.Values.ToList();
            for (var index = 0; index < newValue.Count; index++)
            {
                var originalValue = values[index];
                var value = newValue[index];
                _comparer.Compare(value,originalValue);
            }
        }
        
        return true;
    }

    public void ValidateReturnValue(object? returnValue)
    {
        _comparer.Compare(returnValue, _context.ReturnObject);
    }

    public void Reset(MethodConfiguration methodConfiguration)
    {
        _currentMethod = methodConfiguration;
        _context.Reset();
    }

    public void RegisterClientReturnType(Type type)
    {
        _returnType = type;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            _returnType = type.GetGenericArguments().First();
        }
    }
}