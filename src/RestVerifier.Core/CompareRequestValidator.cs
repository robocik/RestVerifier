using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;
using RestVerifier.Core.Matchers;

namespace RestVerifier.Core;



public class CompareRequestValidator
{
    private readonly VerifierConfiguration _configuration;
    private IObjectsComparer _comparer;
    public ITestObjectCreator Creator { get; }
    private ValidationContext _context = new ();
    private MethodConfiguration? _currentMethod;
    private IParameterMatchStrategy _matcher;
    
    public CompareRequestValidator(VerifierConfiguration configuration, IObjectsComparer comparer, ITestObjectCreator creator, IParameterMatchStrategy matcher)
    {
        _configuration = configuration;
        _comparer = comparer;
        Creator = creator;
        _matcher = matcher;
    }

    public IRemoteServiceContext Context => _context;
    public IObjectsComparer Comparer => _comparer;
    public bool ShouldValidateReachEndpoint { get; set; }

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
            _matcher.Match(Comparer,contextActionArguments, values);
        }
        
        return true;
    }
    

    public void ValidateReturnValue(object? returnValue)
    {
        if (ValidationContext.NotSet == _context.ReturnObject && _context.ReturnObject!=returnValue)
        {
            throw new ArgumentOutOfRangeException("ReturnType","No return value. Probably your controller method is void but your client method has different return type");
        }
        Comparer.Compare(returnValue, _context.ReturnObject);
    }

    public void Reset(MethodConfiguration methodConfiguration)
    {
        _currentMethod = methodConfiguration;
        _context.Reset();
    }

    public void AddException(Exception exception)
    {
        _context.AddException(exception);
    }

    public void ThrowIfExceptions()
    {
        if (_context.Exceptions.Any())
        {
            throw new VerifierExecutionException(_currentMethod!.MethodInfo, $"During testing method {_currentMethod!.MethodInfo} exception has been thrown but no exception was raised in your client class. Please verify if you handle exceptions correctly. Source exception you can find in InnerException property", _context.Exceptions.First());
        }
    }

    public void ThrowIfNotReachEndpoint()
    {
        if (ShouldValidateReachEndpoint && !_context.ReachEndpoint)
        {
            throw new VerifierExecutionException(_currentMethod!.MethodInfo, $"During testing method {_currentMethod!.MethodInfo} didn't reach endpoint, which means that no controller action was invoked.");
        }
    }
    public void ReachEndpoint()
    {
        _context.MarkReachEndpoint();
    }



}