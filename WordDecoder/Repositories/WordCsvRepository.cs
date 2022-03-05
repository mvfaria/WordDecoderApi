namespace WordDecoderApi.Repositories;

public class WordCsvRepository : IWordRepository
{
    private readonly string[] _words;

    public WordCsvRepository(IWebHostEnvironment env)
    {
        var filePath = Path.Combine(env.ContentRootPath, "Data", "wordList.csv");
        _words = File.ReadLines(filePath).Select(x => x.Split(',')[0]).ToArray();
    }

    public bool Contains(string word) => _words.Contains(word.ToLower());

    public string GetRandomly() => _words[Random.Shared.Next(_words.Length)].ToLower();
}
