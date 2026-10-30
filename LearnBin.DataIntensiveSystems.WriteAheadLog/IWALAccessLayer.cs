namespace LearnBin.DataIntensiveSystems.WriteAheadLog;

public interface IWALAccessLayer
{
    Task AppendAsync(string logText);
    Task<string?> Read(int index);
}
