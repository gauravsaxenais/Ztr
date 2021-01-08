namespace ZTR.Framework.Test
{
    public interface IIntegrationFixture<TFixture> : ITestFixture<TFixture>
        where TFixture : class
    {
    }
}
