using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WordDecoderApi.Model;
using Xunit;

namespace WordDecoderApi.Tests;

public class WordDecoderTests
{
    [Fact]
    public async Task StartANewGameBeforeGuessingAsync()
    {
        var application = new WordDecoderApplication();

        var client = application.CreateClient();
        var resp = await client.GetAsync("/guess/test");
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

        Assert.Equal("XXXXX", gameResp.Clue);
        Assert.Equal("A new game has started. You have 6 attempts.", gameResp.Message);
    }

    [Fact]
    public async Task GuessIsNotAValidWordAsync()
    {
        var application = new WordDecoderApplication();

        var guessWord = "ZZZZZ";
        var client = application.CreateClient();
        
        await client.GetFromJsonAsync<GameResponse>("/startNewGame");
        
        var resp = await client.GetAsync($"/guess/{guessWord}");
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
        var resp = await client.GetAsync("/guess/about");
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("YYYYY", gameResp.Clue);
        Assert.Equal("Congrats! You won the game.", gameResp.Message);
    }

    [Fact]
    public async Task LetterIsNotInWordAsync()
    {
        var application = new WordDecoderApplication();
        await application.AddGameStateAsync(new GameState { Attempts = 3, Word = "house" });

        var client = application.CreateClient();
        var resp = await client.GetAsync("/guess/balmy");
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("XXXXX", gameResp.Clue);
        Assert.Equal($"Try again. You have 2 attempts left.", gameResp.Message);
    }

    [Fact]
    public async Task LetterIsInWordButIncorrectSpotAsync()
    {
        var application = new WordDecoderApplication();
        await application.AddGameStateAsync(new GameState { Attempts = 3, Word = "house" });

        var client = application.CreateClient();
        var resp = await client.GetAsync("/guess/chill");
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("XHXXX", gameResp.Clue);
        Assert.Equal($"Try again. You have 2 attempts left.", gameResp.Message);
    }

    [Fact]
    public async Task LetterIsInWordInCorrectSpotAsync()
    {
        var application = new WordDecoderApplication();
        await application.AddGameStateAsync(new GameState { Attempts = 3, Word = "house" });

        var client = application.CreateClient();
        var resp = await client.GetAsync("/guess/hotel");
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("YYXHX", gameResp.Clue);
        Assert.Equal($"Try again. You have 2 attempts left.", gameResp.Message);
    }

    [Fact]
    public async Task TheGameEndsWhenThereIsNoAttemptsLeftAsync()
    {
        var application = new WordDecoderApplication();
        await application.AddGameStateAsync(new GameState { Attempts = 1, Word = "house" });

        var client = application.CreateClient();
        var resp = await client.GetAsync("/guess/balmy");
        var gameResp = await resp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("XXXXX", gameResp.Clue);
        Assert.Equal("You have no more attempts left, please start a new game.", gameResp.Message);

        var secondResp = await client.GetAsync("/guess/any");
        var secondGameResp = await secondResp.Content.ReadFromJsonAsync<GameResponse>();

        Assert.Equal(HttpStatusCode.OK, secondResp.StatusCode);
        Assert.Equal("Please start a new game.", secondGameResp.Message);
    }
}
