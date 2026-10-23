using WriteAheadLog;

var separator = "-----------";

Console.WriteLine($"WAL consumer: beginning\n{separator}");

Console.WriteLine("WAL consumer: loading processes...");
await using var walAL = new WALAccessLayer(Path.Combine(AppContext.BaseDirectory, "logs.bin"));
Console.WriteLine("WAL consumer: finished loading\n---------");

while (true)
{
    Console.WriteLine(
        $"WAL consumer: select operation\n- Press '1' for appending log\n- Input '2' for reading a specific log line\n- Input 'e' to exit proces\n{separator}"
    );
    Console.Write("waiting for input: ");
    var input = Console.ReadLine();

    Console.WriteLine($"selected: {input}");

    if (input == "1")
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
    else if (input == "2")
    {
        Console.WriteLine("insert position of log line to read: ");
        var logLine = Console.ReadLine()!;

        Console.WriteLine(
            $"Log line at #{logLine}: {await walAL.Read(Convert.ToInt32(logLine) - 1)}"
        );
    }
    else if (input == "e")
    {
        return;
    }
}
