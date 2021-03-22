namespace ZTR.Framework.Business
{
    using System.IO;
    public static class GlobalMethods
    {
        public static string GetCurrentAppPath()
        {
            var currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return currentDirectory;
        }
    }
}
