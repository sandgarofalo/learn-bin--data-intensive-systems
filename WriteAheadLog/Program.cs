Console.WriteLine("booted up");
Log.Append("wrote");
Log.Append("wrote again");

Console.WriteLine($"what i got from reading back: {string.Join(", ", Log.ReadAll())}");

Log.Reset();
Console.WriteLine("done");
