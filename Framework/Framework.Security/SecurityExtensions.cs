namespace ZTR.Framework.Security
{
    using System;

    public static class SecurityExtensions
    {
        /// <summary>
        /// Unique identifiers to string.
        /// </summary>
        /// <param name="masterKey">The master key.</param>
        /// <returns></returns>
        public static string GuidToString(this Guid masterKey)
        {
            return masterKey.ToString().ToUpperInvariant();
        }
    }
}
