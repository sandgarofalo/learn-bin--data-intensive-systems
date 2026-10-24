using System.Text;

namespace LearnBin.DataIntensiveSystems.WriteAheadLog;

public class WALAccessLayer : IAsyncDisposable
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

        int logTextLength = 0;

        foreach (var b in Encoding.UTF8.GetBytes(logText))
        {
            _fileWriteStream.WriteByte(b);
            logTextLength++;
        }

        _fileWriteStream.WriteByte(Convert.ToByte('\n'));
        await _fileWriteStream.FlushAsync();

        foreach (var b in BitConverter.GetBytes(logTextLength))
        {
            _linesIndexerWriteStream.WriteByte(b);
        }
        await _linesIndexerWriteStream.FlushAsync();
    }

    public async Task<string?> Read(int index)
    {
        var totalBytesRead = 0;
        var totalBytesToPassForReadingOffset = index * 4; // final addition to account for newlines
        var offset = 0;
        List<byte> tempBytes = [];

        _linesIndexerReadStream.Seek(0, SeekOrigin.Begin);

        while (totalBytesRead < totalBytesToPassForReadingOffset)
        {
            tempBytes.Add((byte)_linesIndexerReadStream.ReadByte());
            totalBytesRead++;

            var remainder = totalBytesRead % 4;
            if (remainder == 0)
            {
                var localOffset = BitConverter.ToInt32(tempBytes.ToArray());
                offset += localOffset;
                tempBytes.Clear();
            }
        }

        List<byte> logTextBytes = [];
        _fileReadStream.Seek(offset + index, SeekOrigin.Begin);
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

        // 10 20 10 30
        // 50 60 30 10
        // 00 20 ab 20
        // 50 50 50 50
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
