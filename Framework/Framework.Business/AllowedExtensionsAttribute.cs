using System.Collections;
using System.Linq;

namespace ZTR.Framework.Business
{
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// AllowedExtensionsAttribute
    /// </summary>
    /// <seealso cref="ValidationAttribute" />
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllowedExtensionsAttribute"/> class.
        /// </summary>
        /// <param name="extensions">The extensions.</param>
        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);

                if (!string.IsNullOrWhiteSpace(extension) && !_extensions.ToList().Contains(extension.ToLower()))
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
            return $"This extension is not allowed!";
        }
    }
}
