using System.Diagnostics.CodeAnalysis;

namespace OALY2000.Results;

public class Result<T> : UnsafeResult
{
    public T? Value { get => (T?)_value; private init => _value = value; }
    public Exception? Error { get => _error; private init => _error = value; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = null;
    }

    private Result(Exception error)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get => _isSuccess; private init => _isSuccess = value; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Fail(Exception error) => new(error);
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Exception error) => Fail(error);
}
