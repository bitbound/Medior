using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared
{
    [DataContract]
    public class Result
    {
        public static Result Fail(string error)
        {
            return new Result(error);
        }
        public static Result Fail(Exception ex)
        {
            return new Result(ex);
        }

        public static Result<T> Fail<T>(string errorMessage)
        {
            return new Result<T>(errorMessage);
        }

        public static Result<T> Fail<T>(Exception ex)
        {
            return new Result<T>(ex);
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

        private Result(string errorMessage)
        {
            IsSuccess = false;
            Error = errorMessage;
            Exception = new Exception(errorMessage);
        }

        private Result(Exception exception)
        {
            IsSuccess = false;
            Error = exception.Message;
            Exception = exception;
        }

        [DataMember]
        public bool IsSuccess { get; init; }

        [DataMember]
        public string? Error { get; init; }

        [DataMember]
        public Exception? Exception { get; init; }


    }

    [DataContract]
    public class Result<T>
    {
        public Result(T value)
        {
            Value = value;
            IsSuccess = true;
        }

        public Result(string errorMessage)
        {
            IsSuccess = false;
            Error = errorMessage;
            Exception = new Exception(errorMessage);
        }

        public Result(Exception exception)
        {
            IsSuccess = false;
            Error = exception.Message;
            Exception = exception;
        }


        [DataMember]
        public bool IsSuccess { get; init; }

        [DataMember]
        public string? Error { get; init; }

        [DataMember]
        public Exception? Exception { get; init; }

        [DataMember]
        public T? Value { get; init; }
    }
}
