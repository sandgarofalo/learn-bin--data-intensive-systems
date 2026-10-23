using LearnBin.DataIntensiveSystems.WriteAheadLog;

var terminalSeparatorGlyph = "-----------";
var insertLogLineInputKey = "1";
var readLogLineInputKey = "2";
var exitProcessInputKey = "e";

Console.WriteLine($"WAL consumer: beginning\n{terminalSeparatorGlyph}");

Console.WriteLine("WAL consumer: loading processes...");
await using var walAL = new WALAccessLayer(Path.Combine(AppContext.BaseDirectory, "logs.bin"));
Console.WriteLine("WAL consumer: finished loading\n---------");

while (true)
{
    Console.WriteLine(
        string.Join(
            '\n',
            [
                "WAL consumer: select operation",
                $"- Input '{insertLogLineInputKey}' for appending log",
                $"- Input '{readLogLineInputKey}' for reading a specific log line",
                $"- Input '{exitProcessInputKey}' to exit proces",
                terminalSeparatorGlyph,
            ]
        )
    );

    Console.Write("waiting for input: ");
    var input = Console.ReadLine();
    Console.WriteLine($"selected: {input}");

    if (input == insertLogLineInputKey)
    {
        string logText;
        var valid = true;

        do
        {
            Console.WriteLine("insert log line to store: ");
            logText = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(logText))
            {
                valid = false;
            }
        } while (!valid);

        await walAL.AppendAsync(logText!);
    }
    else if (input == readLogLineInputKey)
    {
        Console.WriteLine("insert position of log line to read: ");
        var logLine = Console.ReadLine()!;

        Console.WriteLine(
            $"Log line at #{logLine}: {await walAL.Read(Convert.ToInt32(logLine) - 1)}"
        );
    }
    else if (input == exitProcessInputKey)
    {
        return;
    }
}
