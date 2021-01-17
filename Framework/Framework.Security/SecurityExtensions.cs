namespace ZTR.Framework.Security
{
    using System;

    public static class SecurityExtensions
    {
        public static string GuidToString(this Guid masterKey)
        {
            return masterKey.ToString().ToUpperInvariant();
        }
    }
}
