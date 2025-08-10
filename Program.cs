using System.Text.Json;

namespace TodoList;

using TodoID = int;

internal class Program
{
    static readonly private Cmd[] cmds = [
        new()
        {
            Name = "add",
            Description = "Add a new todo",
            UsageText = "todo add <title>",
            Handler = static (db, args) =>
            {
                if (args.Length != 1) throw new CLIException($"Expected 1 argument, got {args.Length}");

                string title = args[0];

                TodoID id = db.Add(title);

                Console.WriteLine($"Todo added successfully. ID = {id}");
            },
        },
        new()
        {
            Name = "remove",
            Description = "Remove a todo by ID",
            UsageText = "todo remove <id>",
            Handler = static (db, args) =>
            {
                if (args.Length != 1) throw new CLIException($"Expected 1 argument, got {args.Length}");

                string idStr = args[0];
                if (!int.TryParse(idStr, out int id)) throw new CLIException($"ID '{id}' is not a valid ID number");

                bool wasFound = db.Remove(id);
                if (!wasFound) throw new CLIException($"No todo with ID '{id}' was found");

                Console.WriteLine("Todo removed successfully");
            },
        },
        new()
        {
            Name = "list",
            Description = "List all todos",
            UsageText = "todo list",
            Handler = static (db, args) =>
            {
                if (args.Length > 0)
                    throw new CLIException($"Expected 0 arguments, got {args.Length}");

                for (int i = 0; i < db.Items.Count; ++i)
                {
                    Console.WriteLine($"  {i:D4}: {db.Items[i]}");
                }
            },
        },
        new()
        {
            Name = "toggle",
            Description = "Toggle the status of a todo by ID",
            UsageText = "todo toggle <id>",
            Handler = static (db, args) =>
            {
                if (args.Length != 1) throw new CLIException($"Expected 1 argument, got {args.Length}");

                string idStr = args[0];
                if (!int.TryParse(idStr, out int id)) throw new CLIException($"ID '{id}' is not a valid ID number");

                bool wasFound = db.Toggle(id);
                if (!wasFound) throw new CLIException($"No todo with ID '{id}' was found");


                Item todo = db.Get(id);
                Console.WriteLine($"Todo toggled successfully");
                Console.WriteLine(todo);
            },
        },
    ];

    private static int Main(string[] args)
    {
        if (args.Length <= 0)
        {
            Console.WriteLine("Expected a command name, but got none");
            return 1;
        }

        var db = new DB();

        string cmdName = args[0];
        Cmd? cmd = cmds.FirstOrDefault(it => it.Name == cmdName);
        if (cmd == null)
        {
            Console.WriteLine($"Unknown command '{cmdName}'");
            return 1;
        }

        try
        {
            cmd.Handler(db, args[1..]);

            db.Save();

            return 0;
        }
        catch (CLIException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine($"Usage: {cmd.UsageText}");
            return 1;
        }
    }


}

internal class CLIException(string message, Exception? innerException = null) : Exception(message, innerException);

internal class DB
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

    public Item Get(TodoID id)
    {
        return _items[id];
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

    public bool Toggle(TodoID id)
    {
        if (id >= _items.Count) return false;

        _items[id].Done = !_items[id].Done;
        return true;
    }
}

internal record Item
{
    public required string Title { get; set; }
    public bool Done { get; set; } = false;

    public override string ToString()
    {
        return $"{(Done ? "[X]" : "[ ]")} {Title}";
    }
}

internal record Cmd
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string UsageText { get; init; }
    public required Action<DB, string[]> Handler { get; init; }
}
