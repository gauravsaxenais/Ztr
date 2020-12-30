namespace ZTR.Framework.Business
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using EnsureThat;

    public static class StringExtensions
    {
        private static readonly object _balanceLock = new object();

        public static string ToGuidValue(this string source)
        {
            byte[] hashedBytes;
            EnsureArg.IsNotNullOrWhiteSpace(source, nameof(source));

            lock (_balanceLock)
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

        public static Guid ToGuid(this string source)
        {
            return Guid.Parse(ToGuidValue(source));
        }

        public static string ToLowerFirstCharacter(this string input)
        {
            EnsureArg.IsNotNullOrWhiteSpace(input, nameof(input));

            var firstChar = input[0];
            if (firstChar >= 'A' && firstChar <= 'Z')
            {
                firstChar = (char)(firstChar + 32);
            }

            return firstChar + input[1..];
        }

        public static string RemoveNewline(this string input)
        {
            EnsureArg.IsNotNullOrWhiteSpace(input, nameof(input));
            input = input
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty);

            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            input = regex.Replace(input, " ");
            return input;
        }
    }
}
