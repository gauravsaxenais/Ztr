using System;
using System.Diagnostics;
using System.Text;
using EnsureThat;
using FluentValidation.Results;
using Newtonsoft.Json;
using ZTR.Framework.Business.Content;
using ZTR.Framework.Business.Models;
using ZTR.Framework.Configuration;

namespace ZTR.Framework.Business
{
    /// <summary>
    /// ErrorMessage
    /// </summary>
    /// <typeparam name="TErrorCode">The type of the error code.</typeparam>
    public class ErrorMessage<TErrorCode>
        where TErrorCode : Enum
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage{TErrorCode}"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        /// <param name="attemptedValue">The attempted value.</param>
        /// <param name="exception">The exception.</param>
        public ErrorMessage(string propertyName, TErrorCode errorCode, string message, object attemptedValue, Exception exception)
        {
            EnsureArg.IsNotNull(propertyName, nameof(propertyName));
            EnsureArg.IsNotNullOrWhiteSpace(message, nameof(message));

            PropertyName = propertyName;
            ErrorCode = errorCode;
            Message = message;
            AttemptedValue = attemptedValue;

            ID = Guid.NewGuid().ToString("n");
            Message = message;

            if (exception != null)
            {
                Detail = GenerateMessageFromException(exception);
                Exception = exception.StackTrace.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public ErrorMessage(TErrorCode errorCode, string message, Exception exception) : this(string.Empty, errorCode, message, null, exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="exception">The exception.</param>
        public ErrorMessage(TErrorCode errorCode, Exception exception) :
            this(errorCode, exception is IApplicationException ? exception.Message : Resource.ExceptionMessage, exception)
        {         
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        public ErrorMessage(TErrorCode errorCode, string message) :
            this(errorCode, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage{TErrorCode}"/> class.
        /// </summary>
        /// <param name="validationFailure">The validation failure.</param>
        /// <exception cref="InvalidOperationException">Could not parse an error code enumeration of type {typeof(TErrorCode).Name} with a value for {validationFailure.ErrorCode}.</exception>
        public ErrorMessage(ValidationFailure validationFailure)
        {
            EnsureArg.IsNotNull(validationFailure, nameof(validationFailure));

            PropertyName = validationFailure.PropertyName;

            if (Enum.TryParse(typeof(TErrorCode), validationFailure.ErrorCode, out object errorCode))
            {
                ErrorCode = (TErrorCode)errorCode;
            }
            else
            {
                throw new InvalidOperationException($"Could not parse an error code enumeration of type {typeof(TErrorCode).Name} with a value for {validationFailure.ErrorCode}.");
            }

            AttemptedValue = validationFailure.AttemptedValue?.ToString();
            Message = validationFailure.ErrorMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessage{TErrorCode}"/> class.
        /// </summary>
        protected ErrorMessage()
        {
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public TErrorCode ErrorCode { get; private set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the exception detail.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Detail { get; private set; }

        /// <summary>
        /// Gets the Unique Identifier.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string ID { get; private set; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets or sets the attempted value.
        /// </summary>
        /// <value>
        /// The attempted value.
        /// </value>
        public object AttemptedValue { get; private set; }

        internal string ToFormattedString()
        {
            return $"{ErrorCode} - Property: '{PropertyName}' with value '{AttemptedValue}'. {Message}";
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"ID: {ID}, {ErrorCode} - Property: '{PropertyName}' with value '{AttemptedValue}'. {Message} and Detail: {Detail}";
        }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public string Exception { get; private set; }

        /// <summary>
        /// Generates the message from exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private static string GenerateMessageFromException(Exception exception)
        {
            var strBuilder = new StringBuilder();

            string message;
            if (ApplicationConfiguration.IsDevelopment)
            {
                message = exception.Demystify().ToString();
            }
            else
            {
                message = exception.Message;
            }

            var errorMessage = BuildErrorMessageFromException(exception);

            strBuilder.Append(message)
                      .Append(errorMessage);

            return strBuilder.ToString().Replace(Environment.NewLine, " ", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildErrorMessageFromException(Exception exception)
        {
            var sb = new StringBuilder();

            var resultProperty = exception.GetType().GetProperty("Result");
            if (resultProperty != null)
            {
                sb.Append("Result: ");
                var result = resultProperty.GetValue(exception);
                if (result != null)
                {
                    sb.Append(JsonConvert.SerializeObject(result, Formatting.Indented));
                }
            }

            return sb.ToString();
        }
    }
}
