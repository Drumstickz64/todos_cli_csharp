namespace TodoList;

using System.Text.Json;

using TodoID = int;

internal class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var db = new DB();

            if (args.Length <= 0) throw new CLIException("Expected a command name, but got none");

            string cmd = args[0];

            switch (cmd)
            {
                case "add": HandleCmdAdd(db, args[1..]); break;
                case "remove": HandleCmdRemove(db, args[1..]); break;
                case "list": HandleCmdList(db, args[1..]); break;
                default: throw new CLIException($"Unknown command '{cmd}'");
            }

            db.Save();
            return 0;
        }
        catch (CLIException ex)
        {
            Console.WriteLine(ex.Message);
            return 1;
        }
    }

    private static void HandleCmdAdd(DB db, string[] args)
    {
        if (args.Length != 1) throw new CLIException($"Expected 1 argument, got {args.Length}\nUsage: todo add <title>");

        string title = args[0];

        TodoID id = db.Add(title);

        Console.WriteLine($"Todo added successfully. ID = {id}");
    }

    private static void HandleCmdRemove(DB db, string[] args)
    {
        if (args.Length != 1) throw new CLIException($"Expected 1 argument, got {args.Length}\nUsage: todo remove <id>");

        string idStr = args[0];
        if (!int.TryParse(idStr, out int id)) throw new CLIException($"ID '{id}' is not a valid ID number");

        bool wasFound = db.Remove(id);
        if (!wasFound) throw new CLIException($"No todo with ID '{id}' was found");

        Console.WriteLine("Todo removed successfully");
    }

    private static void HandleCmdList(DB db, string[] args)
    {
        if (args.Length > 0)
            throw new CLIException($"Expected 1 argument, got {args.Length}\nUsage: todo list");

        for (int i = 0; i < db.Items.Count; ++i)
        {
            Console.WriteLine($"  {i:D4}: {db.Items[i]}");
        }
    }
}

class CLIException(string message, Exception? innerException = null) : Exception(message, innerException);

class DB
{
    private readonly List<Item> _items = [];

    public IReadOnlyList<Item> Items => _items;

    public DB()
    {
        string json = File.ReadAllText("db.json");
        _items = JsonSerializer.Deserialize<List<Item>>(json)!;
    }

    public void Save()
    {
        string json = JsonSerializer.Serialize(_items);
        File.WriteAllText("db.json", json);
    }

    public TodoID Add(string title)
    {
        _items.Add(new Item
        {
            Title = title,
        });
        return _items.Count - 1;
    }

    public bool Remove(TodoID id)
    {
        if (id >= _items.Count) return false;

        _items.RemoveAt(id);
        return true;
    }
}

class Item
{
    public required string Title { get; set; }
    public bool Done { get; set; } = false;

    public override string ToString()
    {
        return $"{(Done ? "✅" : "❌")} {Title}";
    }
}
