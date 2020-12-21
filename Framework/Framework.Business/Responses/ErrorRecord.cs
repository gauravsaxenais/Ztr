namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using EnsureThat;

    public class ErrorRecord<TErrorCode>
        where TErrorCode : struct, Enum
    {
        public ErrorRecord(int ordinalPosition, ErrorMessages<TErrorCode> errorMessages)
        {
            EnsureArg.IsGte(ordinalPosition, 0, nameof(ordinalPosition));
            EnsureArg.IsNotNull(errorMessages, nameof(errorMessages));

            SetValues(null, ordinalPosition, errorMessages);
        }

        public ErrorRecord(int ordinalPosition, ErrorMessage<TErrorCode> errorMessage)
        {
            EnsureArg.IsGte(ordinalPosition, 0, nameof(ordinalPosition));
            EnsureArg.IsNotNull(errorMessage, nameof(errorMessage));

            var errorMessages = new ErrorMessages<TErrorCode>() { errorMessage };
            SetValues(null, ordinalPosition, errorMessages);
        }

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

        public ErrorRecord(long id, int ordinalPosition, ErrorMessages<TErrorCode> errorMessages)
        {
            EnsureArg.IsGte(ordinalPosition, 0, nameof(ordinalPosition));
            EnsureArg.IsNotNull(errorMessages, nameof(errorMessages));

            SetValues(id, ordinalPosition, errorMessages);
        }

        public ErrorRecord(long id, int ordinalPosition, ErrorMessage<TErrorCode> errorMessage)
        {
            EnsureArg.IsGte(ordinalPosition, 0, nameof(ordinalPosition));
            EnsureArg.IsNotNull(errorMessage, nameof(errorMessage));

            var errorMessages = new ErrorMessages<TErrorCode>() { errorMessage };
            SetValues(id, ordinalPosition, errorMessages);
        }

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
