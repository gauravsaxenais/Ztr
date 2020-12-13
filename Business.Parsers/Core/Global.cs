using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Business.Core
{   
    public class Global
    {
        public static string WebRoot => AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        
    }
}
