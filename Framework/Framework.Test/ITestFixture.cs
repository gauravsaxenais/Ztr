namespace ZTR.Framework.Test
{
    using System;
    using Xunit;

    public interface ITestFixture<TFixture> : IClassFixture<TFixture>, IDisposable
        where TFixture : class
    {
        IServiceProvider ServiceProvider { get; }
    }
}
