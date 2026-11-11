

namespace E_Shop.Api.Tests.Intagration
{
    [Collection("IntegrationApiCollection")]
    public class BaseIntegrationTest : IAsyncLifetime
    {
        protected readonly ApplicationApiFactory _factory;
        protected readonly HttpClient _client;
        public BaseIntegrationTest(ApplicationApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
        public async ValueTask InitializeAsync()
        {
            await _factory.ResetDatabaseAsync();
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
