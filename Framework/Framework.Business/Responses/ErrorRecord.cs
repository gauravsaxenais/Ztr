namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using EnsureThat;

    /// <summary>
    /// ErrorRecord class.
    /// </summary>
    /// <typeparam name="TErrorCode">The type of the error code.</typeparam>
    public class ErrorRecord<TErrorCode>
        where TErrorCode : struct, Enum
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecord{TErrorCode}"/> class.
        /// </summary>
        /// <param name="ordinalPosition">The ordinal position.</param>
        /// <param name="errorMessages">The error messages.</param>
        public ErrorRecord(int ordinalPosition, ErrorMessages<TErrorCode> errorMessages)
        {
            EnsureArg.IsGte(ordinalPosition, 0, nameof(ordinalPosition));
            EnsureArg.IsNotNull(errorMessages, nameof(errorMessages));

            SetValues(null, ordinalPosition, errorMessages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecord{TErrorCode}"/> class.
        /// </summary>
        /// <param name="ordinalPosition">The ordinal position.</param>
        /// <param name="errorMessage">The error message.</param>
        public ErrorRecord(int ordinalPosition, ErrorMessage<TErrorCode> errorMessage)
        {
            EnsureArg.IsGte(ordinalPosition, 0, nameof(ordinalPosition));
            EnsureArg.IsNotNull(errorMessage, nameof(errorMessage));

            var errorMessages = new ErrorMessages<TErrorCode>() { errorMessage };
            SetValues(null, ordinalPosition, errorMessages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecord{TErrorCode}"/> class.
        /// </summary>
        /// <param name="ordinalPosition">The ordinal position.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        public ErrorRecord(int ordinalPosition, TErrorCode errorCode, string message)
        {
            EnsureArg.IsGte(ordinalPosition, 0, nameof(ordinalPosition));
            EnsureArg.IsNotNull<TErrorCode>(errorCode, nameof(errorCode));
            EnsureArg.IsNotEmptyOrWhiteSpace(message, nameof(message));

            var errorMessages = new ErrorMessages<TErrorCode>
            {
                new ErrorMessage<TErrorCode>(errorCode, message)
            };

            SetValues(null, ordinalPosition, errorMessages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecord{TErrorCode}"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ordinalPosition">The ordinal position.</param>
        /// <param name="errorMessages">The error messages.</param>
        public ErrorRecord(long id, int ordinalPosition, ErrorMessages<TErrorCode> errorMessages)
        {
            EnsureArg.IsGte(ordinalPosition, 0, nameof(ordinalPosition));
            EnsureArg.IsNotNull(errorMessages, nameof(errorMessages));

            SetValues(id, ordinalPosition, errorMessages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecord{TErrorCode}"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ordinalPosition">The ordinal position.</param>
        /// <param name="errorMessage">The error message.</param>
        public ErrorRecord(long id, int ordinalPosition, ErrorMessage<TErrorCode> errorMessage)
        {
            EnsureArg.IsGte(ordinalPosition, 0, nameof(ordinalPosition));
            EnsureArg.IsNotNull(errorMessage, nameof(errorMessage));

            var errorMessages = new ErrorMessages<TErrorCode>() { errorMessage };
            SetValues(id, ordinalPosition, errorMessages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecord{TErrorCode}"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="ordinalPosition">The ordinal position.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        public ErrorRecord(long id, int ordinalPosition, TErrorCode errorCode, string message)
        {
            EnsureArg.IsGte(ordinalPosition, 0, nameof(ordinalPosition));
            EnsureArg.IsNotNull<TErrorCode>(errorCode, nameof(errorCode));
            EnsureArg.IsNotEmptyOrWhiteSpace(message, nameof(message));

            var errorMessages = new ErrorMessages<TErrorCode>
            {
                new ErrorMessage<TErrorCode>(errorCode, message)
            };

            SetValues(id, ordinalPosition, errorMessages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecord{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="exception">The exception.</param>
        public ErrorRecord(TErrorCode errorCode, Exception exception)
        {
            EnsureArg.IsNotNull<TErrorCode>(errorCode, nameof(errorCode));
            EnsureArg.IsNotNull(exception, nameof(exception));

            var errorMessages = new ErrorMessages<TErrorCode>
            {
                new ErrorMessage<TErrorCode>(errorCode, exception)
            };

            SetValues(null, 0, errorMessages);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecord{TErrorCode}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        public ErrorRecord(TErrorCode errorCode, string message)
        {
            EnsureArg.IsNotNull<TErrorCode>(errorCode, nameof(errorCode));
            EnsureArg.IsNotEmptyOrWhiteSpace(message, nameof(message));

            var errorMessages = new ErrorMessages<TErrorCode>
            {
                new ErrorMessage<TErrorCode>(errorCode, message)
            };

            SetValues(null, 0, errorMessages);
        }

        internal ErrorRecord(long? id, int ordinalPosition, IEnumerable<ErrorMessage<TErrorCode>> errorMessages)
        {
            EnsureArg.IsNotNull(errorMessages, nameof(errorMessages));

            SetValues(id, ordinalPosition, new ErrorMessages<TErrorCode>(errorMessages));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRecord{TErrorCode}"/> class.
        /// </summary>
        protected ErrorRecord()
        {
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public long? Id { get; set; }

        /// <summary>
        /// Gets the ordinal position.
        /// </summary>
        /// <value>
        /// The ordinal position.
        /// </value>
        public int OrdinalPosition { get; set; }

        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public ErrorMessages<TErrorCode> Errors { get; set; }

        internal string ToFormattedString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Id: '{Id}'");
            stringBuilder.AppendLine($"OrdinalPosition: '{OrdinalPosition}'");
            stringBuilder.AppendLine($"Errors: ");

            foreach (var error in Errors)
            {
                stringBuilder.AppendLine($"        {error.ToFormattedString()}");
            }

            return stringBuilder.ToString();
        }

        private void SetValues(long? id, int ordinalPosition, ErrorMessages<TErrorCode> errors)
        {
            Id = id;
            OrdinalPosition = ordinalPosition;
            Errors = errors;
        }
    }
}
