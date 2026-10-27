using System.CommandLine;
using LearnBin.DataIntensiveSystems.WriteAheadLog;

await using var walAL = new WALAccessLayer(Path.Combine(AppContext.BaseDirectory, "logs.bin"));

RootCommand root = new("CLI tool to CRUD the write-ahead log");

var readCommand = new Command("read", "Reads a specific log line");
var readCommandPositionArg = new Argument<int>("position") { Arity = ArgumentArity.ExactlyOne };
readCommand.Arguments.Add(readCommandPositionArg);
readCommand.SetAction(async parseResult =>
{
    Console.WriteLine(await walAL.Read(parseResult.GetValue(readCommandPositionArg) - 1));
});

root.Subcommands.Add(readCommand);

var appendCommand = new Command("append", "Appends a new log line");
var appendCommandTextArg = new Argument<string>("text") { Arity = ArgumentArity.ExactlyOne };
appendCommand.Arguments.Add(appendCommandTextArg);
appendCommand.Validators.Add(r =>
{
    var text = r.GetValue(appendCommandTextArg);

    if (string.IsNullOrWhiteSpace(text))
    {
        r.AddError("Cannot insert an empty log line");
        return;
    }

    if (text.Contains('\n'))
    {
        r.AddError("Cannot insert a log line containing a newline character in it");
    }
});
appendCommand.SetAction(async parseResult =>
{
    await walAL.AppendAsync(parseResult.GetValue(appendCommandTextArg)!);
});
root.Subcommands.Add(appendCommand);

return await root.Parse(args).InvokeAsync();
