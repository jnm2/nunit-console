using System;
using System.ComponentModel;
using System.Diagnostics;

namespace NUnit.Engine
{
    internal static class Result
    {
        public static Result<T> Success<T>(T value) => Result<T>.Success(value);

        public static ErrorResult Error(string message) => new ErrorResult(message);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public struct ErrorResult
        {
            public string Message { get; }

            public ErrorResult(string message)
            {
                if (string.IsNullOrEmpty(message))
                    throw new ArgumentException("Error message must be specified.", nameof(message));

                Message = message;
            }
        }
    }

    [DebuggerDisplay("{ToString(),nq}")]
    internal struct Result<T>
    {
        private readonly string _errorMessage;
        private readonly T _value;

        public T Value => _errorMessage is null
            ? Value
            : throw new InvalidOperationException("The result does not represent success.");

        private Result(T value, string errorMessage)
        {
            _value = value;
            _errorMessage = errorMessage;
        }

        internal static Result<T> Success(T value)
        {
            return new Result<T>(value, errorMessage: null);
        }

        public static implicit operator Result<T>(Result.ErrorResult error)
        {
            return new Result<T>(default(T), error.Message);
        }

        public bool IsSuccess(out T value)
        {
            value = _value;
            return _errorMessage is null;
        }

        public bool IsError()
        {
            return _errorMessage is object;
        }

        public bool IsError(out string message)
        {
            message = _errorMessage;
            return _errorMessage is object;
        }

        public override string ToString()
        {
            return _errorMessage is null
                ? $"Success({_value})"
                : $"Error({_errorMessage})";
        }
    }
}
