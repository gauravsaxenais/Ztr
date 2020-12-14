namespace Business.Core
{
    using System;

    public class Global
    {
        public static string WebRoot => AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        
    }
}
