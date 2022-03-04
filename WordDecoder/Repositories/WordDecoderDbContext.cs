using Microsoft.EntityFrameworkCore;

namespace WordDecoderApi.Repositories;

public class WordDecoderDb : DbContext
{
    public WordDecoderDb(DbContextOptions<WordDecoderDb> options) : base(options) { }

    public DbSet<GameState> GameStates => Set<GameState>();
}
