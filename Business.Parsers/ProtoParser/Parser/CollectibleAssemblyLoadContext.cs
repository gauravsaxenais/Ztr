namespace Business.Parsers.ProtoParser.Parser
{
    using System;
    using System.Reflection;
    using System.Runtime.Loader;

    public class CollectibleAssemblyLoadContext : AssemblyLoadContext, IDisposable
    {
        public CollectibleAssemblyLoadContext() : base(true)
        { }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }

        public void Dispose()
        {
            Unload();
        }
    }
}
