namespace MeuSitePessoal.Application.Common.Models;

/// <summary>
/// Specifies the type of error that occurred during an operation.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Indicates no error occurred.
    /// </summary>
    None,

    /// <summary>
    /// Indicates a generic failure or validation error.
    /// </summary>
    Failure,

    /// <summary>
    /// Indicates a requested resource was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// Indicates a state conflict (e.g., entity already exists).
    /// </summary>
    Conflict
}

/// <summary>
/// Represents the outcome of an operation, wrapping a success/failure state along with an error message and type.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error message associated with a failed result.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// Gets the category/type of the error.
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    protected Result(bool isSuccess, string error, ErrorType type = ErrorType.None)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Successful result cannot have an error.");
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failed result must have an error.");

        IsSuccess = isSuccess;
        Error = string.IsNullOrEmpty(error) ? string.Empty : error;
        Type = type;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, string.Empty);

    /// <summary>
    /// Creates a failed result with the specified error message and error type.
    /// </summary>
    public static Result Failure(string error, ErrorType type = ErrorType.Failure) => new(false, error, type);

    /// <summary>
    /// Creates a successful result associated with a specific value.
    /// </summary>
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);

    /// <summary>
    /// Creates a failed result associated with a typed generic parameter.
    /// </summary>
    public static Result<T> Failure<T>(string error, ErrorType type = ErrorType.Failure) => new(default, false, error, type);
}

/// <summary>
/// Represents the outcome of an operation that produces a value.
/// </summary>
/// <typeparam name="T">The type of the underlying value.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value associated with a successful operation.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    protected internal Result(T? value, bool isSuccess, string error, ErrorType type = ErrorType.None)
        : base(isSuccess, error, type)
    {
        Value = value;
    }
}
