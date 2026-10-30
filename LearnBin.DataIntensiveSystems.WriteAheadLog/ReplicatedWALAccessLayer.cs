namespace LearnBin.DataIntensiveSystems.WriteAheadLog;

public class ReplicatedWALAccessLayer : IWALAccessLayer
{
    private readonly WALAccessLayer _leader;
    private readonly WALAccessLayer _follower1;
    private readonly WALAccessLayer _follower2;

    private readonly Random _random = new();

    public ReplicatedWALAccessLayer(string fileName)
    {
        var invalidFileNameChars = Path.GetInvalidFileNameChars();

        if (fileName.Any(c => invalidFileNameChars.Contains(c)))
        {
            throw new ArgumentException("Invalid characters in file name", nameof(fileName));
        }

        _leader = new(Path.Combine(AppContext.BaseDirectory, $"{fileName}{nameof(_leader)}.bin"));
        _follower1 = new(
            Path.Combine(AppContext.BaseDirectory, $"{fileName}{nameof(_follower1)}.bin")
        );
        _follower2 = new(
            Path.Combine(AppContext.BaseDirectory, $"{fileName}{nameof(_follower2)}.bin")
        );
    }

    public async Task AppendAsync(string logText)
    {
        await _leader.AppendAsync(logText);

        await Task.WhenAll(
            Task.Delay(500).ContinueWith(_ => _follower1.AppendAsync(logText)),
            Task.Delay(5000).ContinueWith(_ => _follower2.AppendAsync(logText))
        );
    }

    public async Task<string?> Read(int index)
    {
        var nextInt64 = _random.NextInt64(1, 3);

        return nextInt64 == 1 ? await _follower1.Read(index)
            : nextInt64 == 2 ? await _leader.Read(index)
            : await _follower2.Read(index);
    }
}
