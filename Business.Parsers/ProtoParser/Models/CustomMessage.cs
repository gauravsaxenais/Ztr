namespace Business.Parsers.ProtoParser.Models
{
    using System;
    using Google.Protobuf;

    public class CustomMessage : IDisposable
    {
        public string Name { get; set; }
        public IMessage Message { get; set; }

        private void ReleaseUnmanagedResources()
        {
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~CustomMessage()
        {
            ReleaseUnmanagedResources();
        }
    }
}
