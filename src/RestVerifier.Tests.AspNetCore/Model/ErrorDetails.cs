namespace RestVerifier.Tests.AspNetCore.Model;

public enum ServiceError
{
    Unknown,
    ObjectNotFoundException,
    UniqueException,
    StaleObjectStateException,
    UnauthorizedAccessException,
    ArgumentNullException,
    InvalidOperationException,
    ArgumentOutOfRangeException,
    ConstraintException,
    InvalidHouseTypeException
}
public class ErrorDetails
{
    public ServiceError ServiceError { get; set; }
    public string Message { get; set; }
}