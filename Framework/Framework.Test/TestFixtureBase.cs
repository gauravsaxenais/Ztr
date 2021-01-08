namespace ZTR.Framework.Test
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public abstract class TestFixtureBase<TFixture> : IDisposable, ITestFixture<TFixture>
        where TFixture : class
    {
        private IHost _host;
        private ILogger _logger;

        protected TestFixtureBase()
        {
            Initialize();
        }

        ~TestFixtureBase()
        {
            Dispose(false);
        }

        public IServiceProvider ServiceProvider { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract IHostBuilder ConfigureHost();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _host?.Dispose();
            }
        }

        private void Initialize()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            var builder = ConfigureHost();

            _host = builder.Build();

            ServiceProvider = _host.Services;
            _logger = ServiceProvider.GetRequiredService<ILogger<TestFixtureBase<TFixture>>>();

            // run host
            _host.Start();
            _host.StopAsync();
        }
    }
}
