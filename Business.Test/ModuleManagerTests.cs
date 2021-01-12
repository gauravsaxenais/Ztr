namespace Business.Test
{
    using Business.GitRepositoryWrappers.Interfaces;
    using Moq;
    using Xunit;

    // naming convention - [MethodWeTest_StateUnderTest_ExpectedBehavior]

    public class ModuleManagerTests
    {
        private readonly IModuleServiceManager _moduleServiceManager;
        [Fact]
        public void GetAllModulesAsync_AllModules_ReturnsAllModules()
        {
            // Arrange
            var mockServiceManager = new Mock<IModuleServiceManager>();
            //mockServiceManager.Setup(x => x.GetAllModulesAsync("1.0.21", "m7"))
            //    .Returns((Task<List<ModuleReadModel>>) => new List<ModuleReadModel>());

        }
    }
}
