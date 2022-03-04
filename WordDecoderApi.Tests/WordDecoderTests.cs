using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WordDecoderApi.Repositories;
using Xunit;

namespace WordDecoderApi.Tests;

public class WordDecoderTests
{
    [Fact]
    public async Task StartANewGameBeforeGuessingAsync()
    {
        var application = new WordDecoderApplication();

        var client = application.CreateClient();
        var resp = await client.PostAsJsonAsync("/guess", new Guess());
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("Please start a new game.", gameResp.Message);
    }

    [Fact]
    public async Task StartNewGameAsync()
    {
        var application = new WordDecoderApplication();

        var client = application.CreateClient();
        var gameResp = await client.GetFromJsonAsync<GameResponse>("/startNewGame");

        Assert.Equal("XXXXX, A new game has started. You have 6 attempts.", gameResp.Message);
    }

    [Fact]
    public async Task CorrectGuessEndsTheGameAsync()
    {
        var application = new WordDecoderApplication();

        using (var scope = application.Services.CreateScope())
        {
            var provider = scope.ServiceProvider;
            using (var context = provider.GetRequiredService<WordDecoderDb>())
            {
                await context.Database.EnsureCreatedAsync();
                await context.GameStates.AddAsync(new GameState { Word = "HOUSE" });
                await context.SaveChangesAsync();
            }
        }

        var client = application.CreateClient();
        var resp = await client.PostAsJsonAsync("/guess", new Guess { Word="HOUSE"});
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("YYYYY, Congrats! You won the game.", gameResp.Message);
    }

    [Fact]
    public async Task LetterIsNotInWordAsync()
    {
        var application = new WordDecoderApplication();

        using (var scope = application.Services.CreateScope())
        {
            var provider = scope.ServiceProvider;
            using (var context = provider.GetRequiredService<WordDecoderDb>())
            {
                await context.Database.EnsureCreatedAsync();
                await context.GameStates.AddAsync(new GameState { Attempts = 3, Word = "HOUSE" });
                await context.SaveChangesAsync();
            }
        }

        var client = application.CreateClient();
        var resp = await client.PostAsJsonAsync("/guess", new Guess { Word = "BALMY" });
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal($"XXXXX, Try again. You have 2 attempts left.", gameResp.Message);
    }

    [Fact]
    public async Task LetterIsInWordButIncorrectSpotAsync()
    {
        var application = new WordDecoderApplication();

        using (var scope = application.Services.CreateScope())
        {
            var provider = scope.ServiceProvider;
            using (var context = provider.GetRequiredService<WordDecoderDb>())
            {
                await context.Database.EnsureCreatedAsync();
                await context.GameStates.AddAsync(new GameState { Attempts = 3, Word = "HOUSE" });
                await context.SaveChangesAsync();
            }
        }

        var client = application.CreateClient();
        var resp = await client.PostAsJsonAsync("/guess", new Guess { Word = "CHILL" });
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal($"XHXXX, Try again. You have 2 attempts left.", gameResp.Message);
    }

    [Fact]
    public async Task LetterIsInWordInCorrectSpotAsync()
    {
        var application = new WordDecoderApplication();

        using (var scope = application.Services.CreateScope())
        {
            var provider = scope.ServiceProvider;
            using (var context = provider.GetRequiredService<WordDecoderDb>())
            {
                await context.Database.EnsureCreatedAsync();
                await context.GameStates.AddAsync(new GameState { Attempts = 3, Word = "HOUSE" });
                await context.SaveChangesAsync();
            }
        }

        var client = application.CreateClient();
        var resp = await client.PostAsJsonAsync("/guess", new Guess { Word = "HOTEL" });
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal($"YYXHX, Try again. You have 2 attempts left.", gameResp.Message);
    }

    [Fact]
    public async Task TheGameEndsWhenThereIsNoAttemptsLeftAsync()
    {
        var application = new WordDecoderApplication();

        using (var scope = application.Services.CreateScope())
        {
            var provider = scope.ServiceProvider;
            using (var context = provider.GetRequiredService<WordDecoderDb>())
            {
                await context.Database.EnsureCreatedAsync();
                await context.GameStates.AddAsync(new GameState { Attempts = 1, Word = "HOUSE" });
                await context.SaveChangesAsync();
            }
        }

        var client = application.CreateClient();
        var resp = await client.PostAsJsonAsync("/guess", new Guess { Word = "BALMY" });
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("XXXXX, You have no more attempts left, please start a new game.", gameResp.Message);

        var secondResp = await client.PostAsJsonAsync("/guess", new Guess());
        var secondGameResp = await secondResp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, secondResp.StatusCode);
        Assert.Equal("Please start a new game.", secondGameResp.Message);
    }
}

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
}
