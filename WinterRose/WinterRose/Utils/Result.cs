using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// A result of an operation that can either be a success or a failure
    /// </summary>
    /// <typeparam name="TValue">The type of object that will be given if the operation is a success</typeparam>
    /// <typeparam name="TError">The type of object that will be given if the operation is a faulure</typeparam>
    public readonly struct Result<TValue, TError>
    {
        /// <summary>
        /// The value of successful operation
        /// </summary>
        public TValue Value { get; }
        /// <summary>
        /// The error of a failed operation
        /// </summary>
        public TError Error { get; }
        /// <summary>
        /// Whether or not the operation was a success
        /// </summary>
        public bool IsSuccess { get; }
        /// <summary>
        /// Whether or not the operation was a failure
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Creates a new result as successful
        /// </summary>
        /// <param name="value"></param>
        public Result(TValue value)
        {
            Value = value;
            IsSuccess = true;
        }

        /// <summary>
        /// Creates a new result as a failure
        /// </summary>
        /// <param name="error"></param>
        public Result(TError error)
        {
            Error = error;
            IsSuccess = false;
        }

        /// <summary>
        /// Implicitly converts a value to a successful result
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Result<TValue, TError>(TValue value) => new Result<TValue, TError>(value);
        /// <summary>
        /// Implicitly converts an error to a failed result
        /// </summary>
        /// <param name="error"></param>
        public static implicit operator Result<TValue, TError>(TError error) => new Result<TValue, TError>(error);

        /// <summary>
        /// If the result is a success, invokes <paramref name="Success"/> with the value. Otherwise invokes <paramref name="Failure"/> with the value and the error
        /// </summary>
        /// <param name="Success"></param>
        /// <param name="Failure"></param>
        /// <returns>This same instance</returns>
        public readonly Result<TValue, TError> Evaluate(Action<TValue> Success, Action<TValue, TError> Failure)
        {
            if (IsSuccess)
                Success(Value);
            else
                Failure(Value, Error);
            return this;
        }

        public static unsafe implicit operator Result<TValue, TError>*(Result<TValue, TError> result) =>  (Result<TValue, TError>*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref result);
    }
}
