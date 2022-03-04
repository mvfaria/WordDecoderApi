namespace WordDecoderApi.Repositories
{
    public interface IWordRepository
    {
        string GetRandomly();
        bool Contains(string word);
    }
}
