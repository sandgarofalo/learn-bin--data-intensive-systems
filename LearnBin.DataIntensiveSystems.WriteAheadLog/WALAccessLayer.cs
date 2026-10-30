using System.Text;

namespace LearnBin.DataIntensiveSystems.WriteAheadLog;

public class WALAccessLayer : IAsyncDisposable, IWALAccessLayer
{
    private readonly FileStream _fileReadStream;
    private readonly FileStream _fileWriteStream;

    private readonly FileStream _linesIndexerReadStream;
    private readonly FileStream _linesIndexerWriteStream;

    private readonly string _filePath;

    private string LinesIndexerPath
    {
        get
        {
            return Path.Combine(
                Path.GetDirectoryName(_filePath) ?? AppContext.BaseDirectory,
                $"{Path.GetFileNameWithoutExtension(_filePath)}.lines-index.bin"
            );
        }
    }

    public WALAccessLayer(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;

        _fileReadStream = new FileStream(
            _filePath,
            FileMode.OpenOrCreate,
            FileAccess.Read,
            FileShare.ReadWrite
        );
        _fileWriteStream = new FileStream(
            _filePath,
            FileMode.Append,
            FileAccess.Write,
            FileShare.ReadWrite
        );

        _linesIndexerReadStream = new FileStream(
            LinesIndexerPath,
            FileMode.OpenOrCreate,
            FileAccess.Read,
            FileShare.ReadWrite
        );
        _linesIndexerWriteStream = new FileStream(
            LinesIndexerPath,
            FileMode.Append,
            FileAccess.Write,
            FileShare.ReadWrite
        );
    }

    public async Task AppendAsync(string logText)
    {
        ArgumentException.ThrowIfNullOrEmpty(logText);
        if (logText.Contains('\n'))
            throw new ArgumentException(
                "Can't include newline character(s) in the log text",
                nameof(logText)
            );

        foreach (var b in Encoding.UTF8.GetBytes(logText))
        {
            _fileWriteStream.WriteByte(b);
        }
        _fileWriteStream.WriteByte((byte)'\n');
        await _fileWriteStream.FlushAsync();

        Console.WriteLine(
            $"Finished writing log text '{logText}': position of stream: '{_fileWriteStream.Position}'"
        );

        foreach (var b in BitConverter.GetBytes((int)_fileWriteStream.Position))
        {
            _linesIndexerWriteStream.WriteByte(b);
        }
        await _linesIndexerWriteStream.FlushAsync();
    }

    public async Task<string?> Read(int index)
    {
        List<byte> totalFileOffsetBytes = [];
        var offset = 0;

        if (index != 0)
        {
            _linesIndexerReadStream.Seek((index - 1) * 4, SeekOrigin.Begin);

            while (totalFileOffsetBytes.Count < 4)
            {
                totalFileOffsetBytes.Add((byte)_linesIndexerReadStream.ReadByte());
            }

            offset = BitConverter.ToInt32(totalFileOffsetBytes.ToArray());
        }

        List<byte> logTextBytes = [];
        _fileReadStream.Seek(offset, SeekOrigin.Begin);
        var foundEnd = false;

        while (!foundEnd)
        {
            var bInInt = _fileReadStream.ReadByte();

            if ((byte)bInInt == (byte)'\n' || bInInt == -1)
            {
                foundEnd = true;
                break;
            }

            logTextBytes.Add((byte)bInInt);
        }

        return logTextBytes.Count is 0
            ? null
            : new string(Encoding.UTF8.GetChars([.. logTextBytes]));
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _fileWriteStream.Dispose();
        _fileReadStream.Dispose();
        _linesIndexerReadStream.Dispose();
        _linesIndexerWriteStream.Dispose();
    }
}
