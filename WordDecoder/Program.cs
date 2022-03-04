using WordDecoderApi;
using WordDecoderApi.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<WordDecoderDb>(opt => opt.UseInMemoryDatabase("WordDecoder"));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


var words = new[]
{
    "HOUSE", "GRACE", "CHASE", "CHILL", "HORSE", "CLEAN", "BALMY", "HOTEL", "CHEER", "HAPPY"
};

app.MapGet("/startNewGame", async (WordDecoderDb db) =>
{
    db.GameStates.RemoveRange(await db.GameStates.ToListAsync());

    var gameState = new GameState()
    {
        Attempts = 6,
        Word = words[Random.Shared.Next(words.Length)],
    };

    await db.GameStates.AddAsync(gameState);
    await db.SaveChangesAsync();

    return Results.Ok(new GameResponse($"XXXXX, A new game has started. You have {gameState.Attempts} attempts."));
});



app.MapPost("/guess", async (WordDecoderDb db, Guess guess) =>
{
    var gameState = await db.GameStates.FirstOrDefaultAsync();
    if (gameState is null)
    {
        return Results.Ok(new GameResponse("Please start a new game."));
    }

    var guessWord = guess.Word.ToUpper();
    if (!words.Contains(guessWord))
    {
        return Results.BadRequest($"The word {guess.Word} is not valid.");
    }

    var secretWord = gameState.Word.ToUpper();
    if (guessWord == secretWord)
    {
        db.GameStates.Remove(gameState);
        await db.SaveChangesAsync();

        return Results.Ok(new GameResponse("YYYYY, Congrats! You won the game."));
    }

    var clue = "";

    for (int i = 0; i < guessWord.Length; i++)
    {
        clue +=
            guessWord[i] == secretWord[i] ? "Y"
                : secretWord.Contains(guessWord[i]) ? "H" : "X";
    }

    gameState.Attempts--;

    var msg = $"{clue}, ";
    if (gameState.Attempts == 0)
    {
        db.GameStates.Remove(gameState);
        msg += "You have no more attempts left, please start a new game.";
    }
    else
    {
        db.Update(gameState);
        msg += $"Try again. You have {gameState.Attempts} attempts left.";
    }

    await db.SaveChangesAsync();

    return Results.Ok(new GameResponse(msg));
});



//app.MapGet("/guess", async (WordDecoderDb db, Guess guess) =>
//{
//    var gameState = await db.GameStates.FirstOrDefaultAsync();
//    if (gameState is null)
//    {
//        return Results.Ok(new GameResponse("Please start a new game."));
//    }

//    var guessWord = guess.Word.ToUpper();
//    if (!words.Contains(guessWord))
//    {
//        return Results.BadRequest($"The word {guess.Word} is not valid.");
//    }

//    var secretWord = gameState.Word.ToUpper();
//    if (guessWord == secretWord)
//    {
//        db.GameStates.Remove(gameState);
//        await db.SaveChangesAsync();

//        return Results.Ok(new GameResponse("YYYYY, Congrats! You won the game."));
//    }

//    var clue = "";

//    for (int i = 0; i < guessWord.Length; i++)
//    {
//        clue +=
//            guessWord[i] == secretWord[i] ? "Y"
//                : secretWord.Contains(guessWord[i]) ? "H" : "X";
//    }

//    gameState.Attempts--;

//    var msg = $"{clue}, ";
//    if (gameState.Attempts == 0)
//    {
//        db.GameStates.Remove(gameState);
//        msg += "You have no more attempts left, please start a new game.";
//    }
//    else
//    {
//        db.Update(gameState);
//        msg += $"Try again. you have {gameState.Attempts} attempts left.";
//    }

//    await db.SaveChangesAsync();

//    return Results.Ok(new GameResponse(msg));
//});


app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}