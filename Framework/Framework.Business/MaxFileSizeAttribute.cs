namespace ZTR.Framework.Business
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// MaxFileSizeAttribute
    /// </summary>
    /// <seealso cref="ValidationAttribute" />
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxFileSizeAttribute"/> class.
        /// </summary>
        /// <param name="maxFileSize">Maximum size of the file.</param>
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns></returns>
        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <returns></returns>
        public string GetErrorMessage()
        {
            return $"Maximum allowed file size is {_maxFileSize} bytes.";
        }
    }
}
