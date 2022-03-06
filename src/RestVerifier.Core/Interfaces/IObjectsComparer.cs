namespace RestVerifier.Core.Interfaces;

public interface IObjectsComparer
{
    void Compare(object? obj1, object? obj2,string? message=null);
}