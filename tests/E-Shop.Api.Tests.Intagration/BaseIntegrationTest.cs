

namespace E_Shop.Api.Tests.Intagration
{
    [Collection("IntegrationApiCollection")]
    public class BaseIntegrationTest : IAsyncLifetime
    {
        protected readonly ApplicationApiFactory Factory;
        protected readonly HttpClient Client;
        public BaseIntegrationTest(ApplicationApiFactory factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
        }
        public async ValueTask InitializeAsync()
        {
            await Factory.ResetDatabaseAsync();
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
