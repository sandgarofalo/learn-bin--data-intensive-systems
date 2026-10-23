using WriteAheadLog;

await using var walAL = new WALAccessLayer(Path.Combine(AppContext.BaseDirectory, "logs.bin"));

await walAL.AppendAsync("log text 1");
await walAL.AppendAsync("log text 2!!!");
await walAL.AppendAsync("log text 3!!!");

Console.WriteLine(await walAL.Read(2));
