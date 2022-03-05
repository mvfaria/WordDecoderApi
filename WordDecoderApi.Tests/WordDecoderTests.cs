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
    public async Task GuessIsNotAValidWordAsync()
    {
        var application = new WordDecoderApplication();

        var guessWord = "ZZZZZ";
        var client = application.CreateClient();
        
        await client.GetFromJsonAsync<GameResponse>("/startNewGame");
        
        var resp = await client.PostAsJsonAsync("/guess", new Guess { Word = guessWord });
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        Assert.Equal($"The word {guessWord} is not valid.", gameResp.Message);
    }

    [Fact]
    public async Task TheGameEndsWhenGuessIsCorrectAsync()
    {
        var application = new WordDecoderApplication();
        await application.AddGameStateAsync(new GameState { Word = "about" });

        var client = application.CreateClient();
        var resp = await client.PostAsJsonAsync("/guess", new Guess { Word= "about" });
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("YYYYY, Congrats! You won the game.", gameResp.Message);
    }

    [Fact]
    public async Task LetterIsNotInWordAsync()
    {
        var application = new WordDecoderApplication();
        await application.AddGameStateAsync(new GameState { Attempts = 3, Word = "house" });

        var client = application.CreateClient();
        var resp = await client.PostAsJsonAsync("/guess", new Guess { Word = "balmy" });
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal($"XXXXX, Try again. You have 2 attempts left.", gameResp.Message);
    }

    [Fact]
    public async Task LetterIsInWordButIncorrectSpotAsync()
    {
        var application = new WordDecoderApplication();
        await application.AddGameStateAsync(new GameState { Attempts = 3, Word = "house" });

        var client = application.CreateClient();
        var resp = await client.PostAsJsonAsync("/guess", new Guess { Word = "chill" });
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal($"XHXXX, Try again. You have 2 attempts left.", gameResp.Message);
    }

    [Fact]
    public async Task LetterIsInWordInCorrectSpotAsync()
    {
        var application = new WordDecoderApplication();
        await application.AddGameStateAsync(new GameState { Attempts = 3, Word = "house" });

        var client = application.CreateClient();
        var resp = await client.PostAsJsonAsync("/guess", new Guess { Word = "hotel" });
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal($"YYXHX, Try again. You have 2 attempts left.", gameResp.Message);
    }

    [Fact]
    public async Task TheGameEndsWhenThereIsNoAttemptsLeftAsync()
    {
        var application = new WordDecoderApplication();
        await application.AddGameStateAsync(new GameState { Attempts = 1, Word = "house" });

        var client = application.CreateClient();
        var resp = await client.PostAsJsonAsync("/guess", new Guess { Word = "balmy" });
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
