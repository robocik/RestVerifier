using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Core;



public class CompareRequestValidator
{
    private readonly VerifierConfiguration _configuration;
    private IObjectsComparer _comparer;
    public ITestObjectCreator Creator { get; }
    private ValidationContext _context = new ();
    private MethodConfiguration? _currentMethod;
    
    public CompareRequestValidator(VerifierConfiguration configuration, IObjectsComparer comparer, ITestObjectCreator creator)
    {
        _configuration = configuration;
        _comparer = comparer;
        Creator = creator;
    }

    public IRemoteServiceContext Context => _context;
    public IObjectsComparer Comparer => _comparer;

    public object? AddReturnType(Type? type)
    {
        if (_currentMethod == null)
        {
            throw new ArgumentNullException("_currentMethod");
        }
        var builder = new ReturnValueBuilder(_configuration, this);
        return builder.AddReturnType(_currentMethod, type);
    }

    public bool ValidateParams(IDictionary<string, object?> contextActionArguments)
    {
        var values = _context.ValuesToCompare.ToList();
        Comparer.Compare(contextActionArguments.Count, values.Count);

        if (contextActionArguments.Values.Count > 0)
        {
            var newValue = contextActionArguments.Values.ToList();
            for (var index = 0; index < newValue.Count; index++)
            {
                var originalValue = values[index];
                var value = newValue[index];
                Comparer.Compare(value,originalValue);
            }
        }
        
        return true;
    }

    public void ValidateReturnValue(object? returnValue)
    {
        Comparer.Compare(returnValue, _context.ReturnObject);
    }

    public void Reset(MethodConfiguration methodConfiguration)
    {
        _currentMethod = methodConfiguration;
        _context.Reset();
    }
    
}