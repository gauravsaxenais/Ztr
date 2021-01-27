namespace ZTR.Framework.Service
{
    using System;
    using EnsureThat;
    using Newtonsoft.Json;
    using Business;

    /// <summary>
    /// ApiResponse
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApiResponse"/> is success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public ErrorMessage<ErrorType> Error { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse"/> class.
        /// </summary>
        public ApiResponse() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse"/> class.
        /// </summary>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <param name="data">The data.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="exception">The exception.</param>
        public ApiResponse(bool status, object data, ErrorType errorCode, Exception exception)
        {
            EnsureArg.IsNotNull(data, nameof(data));

            Success = status;
            Data = data;
            Error = new ErrorMessage<ErrorType>(errorCode, exception);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="exception">The exception.</param>
        public ApiResponse(ErrorType errorCode, Exception exception) : this (false, exception, errorCode, exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse"/> class.
        /// </summary>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <param name="data">The data.</param>
        public ApiResponse(bool status, object data)
        {
            EnsureArg.IsNotNull(data, nameof(data));

            Success = status;
            Data = data;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
