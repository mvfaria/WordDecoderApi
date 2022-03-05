using WordDecoderApi.Repositories;
using Microsoft.EntityFrameworkCore;
using WordDecoderApi.Data;
using WordDecoderApi.Model;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IWordRepository, WordCsvRepository>();
builder.Services.AddDbContext<WordDecoderDb>(opt => opt.UseInMemoryDatabase("WordDecoder"));
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/startNewGame", async (WordDecoderDb db, IWordRepository wordRepo) =>
{
    db.GameStates.RemoveRange(await db.GameStates.ToListAsync());

    var gameState = new GameState()
    {
        Attempts = 6,
        Word = wordRepo.GetRandomly()
    };

    await db.GameStates.AddAsync(gameState);
    await db.SaveChangesAsync();

    return Results.Ok(new GameResponse($"A new game has started. You have {gameState.Attempts} attempts.", "XXXXX"));
});

app.MapGet("/guess/{word}", async (WordDecoderDb db, IWordRepository wordRepo, string word) =>
{
    var gameState = await db.GameStates.FirstOrDefaultAsync();
    if (gameState is null)
    {
        return Results.Ok(new GameResponse("Please start a new game."));
    }

    var guessWord = word.ToLower();
    if (!wordRepo.Contains(guessWord))
    {
        return Results.BadRequest(new GameResponse($"The word {word} is not valid."));
    }

    if (guessWord == gameState.Word)
    {
        db.GameStates.Remove(gameState);
        await db.SaveChangesAsync();

        return Results.Ok(new GameResponse("Congrats! You won the game.", "YYYYY"));
    }

    gameState.Attempts--;

    var msg = "";
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

    return Results.Ok(new GameResponse(msg, GetClue(guessWord, gameState.Word)));
});

app.Run();

string GetClue(string guessWord, string secretWord)
{
    var clue = "";

    for (int i = 0; i < guessWord.Length; i++)
    {
        clue +=
            guessWord[i] == secretWord[i] ? "Y"
                : secretWord.Contains(guessWord[i]) ? "H" : "X";
    }

    return clue;
}