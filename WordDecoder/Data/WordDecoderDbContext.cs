using Microsoft.EntityFrameworkCore;
using WordDecoderApi.Model;

namespace WordDecoderApi.Data;

public class WordDecoderDb : DbContext
{
    public WordDecoderDb(DbContextOptions<WordDecoderDb> options) : base(options) { }

    public DbSet<GameState> GameStates => Set<GameState>();
}
