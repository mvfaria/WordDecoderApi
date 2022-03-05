using System.ComponentModel.DataAnnotations;

namespace WordDecoderApi.Model;

public class GameState
{
    public int Id { get; set; }

    public int Attempts { get; set; }
    
    [Required]
    public string? Word { get; set; }
}
