using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Medior.Shared;

[DataContract]
public class Result
{
    public static Result Fail(string reason)
    {
        return new Result(reason);
    }
    public static Result Fail(Exception ex)
    {
        return new Result(ex);
    }

    public static Result<T> Fail<T>(string reason)
    {
        return new Result<T>(reason);
    }

    public static Result<T> Fail<T>(Exception exception)
    {
        return new Result<T>(exception);
    }

    public static Result Ok()
    {
        return new Result();
    }

    public static Result<T> Ok<T>(T value)
    {
        return new Result<T>(value);
    }

    private Result()
    {
        IsSuccess = true;
    }

    private Result(string reason)
    {
        IsSuccess = false;
        Reason = reason;
    }

    private Result(Exception exception)
    {
        IsSuccess = false;
        Reason = exception.Message;
        Exception = exception;
    }

    [DataMember]
    [MemberNotNullWhen(false, nameof(Reason))]
    public bool IsSuccess { get; init; }

    [DataMember]
    public string? Reason { get; init; }

    [DataMember]
    public Exception? Exception { get; init; }

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool HadException => Exception is not null;
}

[DataContract]
public class Result<T>
{
    public Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    public Result(string reason)
    {
        IsSuccess = false;
        Reason = reason;
    }

    public Result(Exception exception)
    {
        IsSuccess = false;
        Reason = exception.Message;
        Exception = exception;
    }


    [DataMember]
    [MemberNotNullWhen(false, nameof(Reason))]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; init; }

    [DataMember]
    public string? Reason { get; init; }

    [DataMember]
    public Exception? Exception { get; init; }

    [DataMember]
    public T? Value { get; init; }

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool HadException => Exception is not null;
}
