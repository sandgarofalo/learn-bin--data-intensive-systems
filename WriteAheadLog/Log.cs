using System.Text;

public class Log
{
    private static readonly string _dbPath = Path.Join(
        AppContext.BaseDirectory,
        "toy-database.txt"
    );

    static Log()
    {
        File.CreateText(_dbPath);
    }

    public static void Append(string text)
    {
        File.AppendAllTextAsync(_dbPath, $"{text}\n", Encoding.UTF8);
    }

    public static IEnumerable<string> ReadAll()
    {
        return File.ReadLines(_dbPath);
    }

    public static void Reset()
    {
        // File.Delete(_dbPath);
    }
}
