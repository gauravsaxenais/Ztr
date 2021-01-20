namespace ZTR.Framework.Business
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using EnsureThat;

    /// <summary>
    /// StringExtensions
    /// </summary>
    public static class StringExtensions
    {
        private static readonly object BalanceLock = new object();

        /// <summary>
        /// Converts to guid value.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static string ToGuidValue(this string source)
        {
            byte[] hashedBytes;
            EnsureArg.IsNotNullOrWhiteSpace(source, nameof(source));

            lock (BalanceLock)
            {
                using var hasher = new MD5CryptoServiceProvider();
                var inputBytes = Encoding.ASCII.GetBytes(source);
                hashedBytes = hasher.ComputeHash(inputBytes);
            }

            var sb = new StringBuilder();
            foreach (var byteInHash in hashedBytes)
            {
                sb.Append(byteInHash.ToString("X2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts to guid.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static Guid ToGuid(this string source)
        {
            return Guid.Parse(ToGuidValue(source));
        }

        /// <summary>
        /// Removes the newline.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string RemoveNewline(this string input)
        {
            EnsureArg.IsNotNullOrWhiteSpace(input, nameof(input));
            input = input
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty);

            var regex = new Regex("[ ]{2,}", RegexOptions.None);
            input = regex.Replace(input, " ");
            return input;
        }

        /// <summary>
        /// Determines whether this instance is number.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        ///   <c>true</c> if the specified input is number; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumber(this string input)
        {
            EnsureArg.IsNotNullOrWhiteSpace(input, nameof(input));
            return decimal.TryParse(input, out _);
        }

        /// <summary>
        /// Removes the quotes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string RemoveQuotes(this string input)
        {
            EnsureArg.IsNotNullOrWhiteSpace(input, nameof(input));
            return input.Replace(@"""", string.Empty);
        }

        /// <summary>
        /// Determines whether [is path URL].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if [is path URL] [the specified path]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPathUrl(this string path)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(path);

            if (System.IO.File.Exists(path))
            {
                return false;
            }

            try
            {
                var uri = new Uri(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the first from split.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns></returns>
        public static string GetFirstFromSplit(this string input, char delimiter)
        {
            var index = input.IndexOf(delimiter);

            return index == -1 ? input : input.Substring(0, index);
        }
    }
}
