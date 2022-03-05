using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using WordDecoderApi.Data;
using WordDecoderApi.Model;

namespace WordDecoderApi.Tests;

class WordDecoderApplication : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var root = new InMemoryDatabaseRoot();

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<WordDecoderDb>));

            services.AddDbContext<WordDecoderDb>(options =>
                options.UseInMemoryDatabase("Testing", root));
        });

        return base.CreateHost(builder);
    }

    internal async Task AddGameStateAsync(GameState state)
    {
        using (var scope = Services.CreateScope())
        {
            var provider = scope.ServiceProvider;
            using (var context = provider.GetRequiredService<WordDecoderDb>())
            {
                await context.Database.EnsureCreatedAsync();
                await context.GameStates.AddAsync(state);
                await context.SaveChangesAsync();
            }
        }
    }
}
