namespace ZTR.Framework.Business
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using ZTR.Framework.Configuration;
    using EnsureThat;
    using FluentValidation.Results;
    using Newtonsoft.Json;

    public class ErrorMessage<TErrorCode>
        where TErrorCode : Enum
    {
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

        public ErrorMessage(TErrorCode errorCode, string message, Exception exception) : this(string.Empty, errorCode, message, null, exception)
        {
        }

        public ErrorMessage(TErrorCode errorCode, Exception exception) :
            this(errorCode, exception.Message, exception)
        {
        }

        public ErrorMessage(TErrorCode errorCode, string message) :
            this(errorCode, message, null)
        {
        }

        public ErrorMessage(ErrorType validationError, ValidationFailure validationFailure)
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

        protected ErrorMessage()
        {
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public TErrorCode ErrorCode { get; set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets the exception detail.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Detail { get; set; }

        /// <summary>
        /// Gets the Unique Identifier.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string ID { get; set; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the attempted value.
        /// </summary>
        /// <value>
        /// The attempted value.
        /// </value>
        public object AttemptedValue { get; set; }

        internal string ToFormattedString()
        {
            return $"{ErrorCode} - Property: '{PropertyName}' with value '{AttemptedValue}'. {Message}";
        }

        public override string ToString()
        {
            return $"ID: {ID}, {ErrorCode} - Property: '{PropertyName}' with value '{AttemptedValue}'. {Message} and Detail: {Detail}";
        }

        public string Exception { get; set; }
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
