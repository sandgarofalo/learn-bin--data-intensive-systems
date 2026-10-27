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
appendCommand.SetAction(async parseResult =>
{
    await walAL.AppendAsync(parseResult.GetValue(appendCommandTextArg)!);
});
root.Subcommands.Add(appendCommand);

return await root.Parse(args).InvokeAsync();
