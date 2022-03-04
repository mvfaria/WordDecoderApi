using System.ComponentModel.DataAnnotations;

namespace WordDecoderApi
{
    public class GameState
    {
        public int Id { get; set; }

        public int Attempts { get; set; }
        
        [Required]
        public string? Word { get; set; }
    }
}