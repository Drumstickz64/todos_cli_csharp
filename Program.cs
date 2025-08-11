using System.Diagnostics;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace TodoList;

using TodoID = int;

internal class Program
{
    static readonly private Cmd[] _cmds = [
        new()
        {
            Name = "help",
            Description = "Display help message",
            UsageText = "todo help",
            Handler = static (_, args) => {
                switch (args.Length) {
                    case 0: DisplayHelpMessage(); break;
                    case 1: {
                        string cmdName = args[0];
                        Cmd? cmd = _cmds!.FirstOrDefault(cmd => cmd.Name == cmdName);
                        if (cmd == null) {
                            Console.WriteLine($"Unknown command '{cmdName}'");
                            Console.WriteLine();
                            DisplayCmdList();
                            return;
                        }

                        Console.WriteLine(cmd.UsageText);
                    }; break;

                    default: throw new CLIException($"Expected 0 or 1 arguments, got {args.Length}");
                }
            },
        },
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

                List<Item> items = db.GetAll();
                foreach (var item in items)
                {
                    Console.WriteLine($"  {item}");
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
            DisplayHelpMessage();
            return 1;
        }

        using var db = new SqliteDB();

        string cmdName = args[0];
        Cmd? cmd = _cmds.FirstOrDefault(it => it.Name == cmdName);
        if (cmd == null)
        {
            Console.WriteLine($"Unknown command '{cmdName}'");
            Console.WriteLine();
            DisplayCmdList();
            return 1;
        }

        try
        {
            cmd.Handler(db, args[1..]);

            return 0;
        }
        catch (CLIException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine($"Usage: {cmd.UsageText}");
            return 1;
        }
    }

    private static void DisplayHelpMessage()
    {
        Console.WriteLine("Usage: todos <command> [...args]");
        Console.WriteLine();
        DisplayCmdList();
    }

    private static void DisplayCmdList()
    {
        Console.WriteLine("Commands:");
        foreach (var cmd in _cmds)
        {
            Console.WriteLine($"  {cmd.Name,-18}{cmd.Description}");
        }
    }
}

internal class CLIException(string message, Exception? innerException = null) : Exception(message, innerException);

internal interface IDB : IDisposable
{
    public List<Item> GetAll();

    public Item Get(TodoID id);

    public TodoID Add(string title);

    public bool Remove(TodoID id);

    public bool Toggle(TodoID id);
}

internal class JsonDB : IDB
{
    private readonly List<Item> _items = [];

    public JsonDB()
    {
        string json = File.ReadAllText("db.json");
        _items = JsonSerializer.Deserialize<List<Item>>(json)!;
    }

    public void Dispose()
    {
        Save();
    }

    public void Save()
    {
        string json = JsonSerializer.Serialize(_items);
        File.WriteAllText("db.json", json);
    }
    public List<Item> GetAll()
    {
        return [.. _items];
    }

    public Item Get(TodoID id)
    {
        return _items[id];
    }

    public TodoID Add(string title)
    {
        int id = _items.Count - 1;
        _items.Add(new Item
        {
            ID = id,
            Title = title,
        });
        return id;
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

internal sealed class SqliteDB : IDB
{
    private readonly SqliteConnection conn;

    public SqliteDB()
    {
        conn = new SqliteConnection("Data Source=TodoList.db");
        conn.Open();

        var command = conn.CreateCommand();
        command.CommandText =
        @"
            CREATE TABLE IF NOT EXISTS todos (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                done INTEGER DEFAULT 0
            );
        ";

        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        conn.Dispose();
    }

    public TodoID Add(string title)
    {
        conn.Open();

        var command = conn.CreateCommand();
        command.CommandText =
        @"
            INSERT INTO todos (title) VALUES ($title);
        ";

        command.Parameters.AddWithValue("$title", title);

        command.ExecuteNonQuery();

        var idCommand = conn.CreateCommand();
        idCommand.CommandText = @"SELECT last_insert_rowid()";
        var id = (long)idCommand.ExecuteScalar()!;

        return (int)id;
    }

    public Item Get(int id)
    {
        conn.Open();

        var command = conn.CreateCommand();
        command.CommandText =
        @"
            SELECT * FROM todos WHERE id = $id;
        ";

        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();

        reader.Read();

        TodoID inDBId = reader.GetInt32(0);
        string title = reader.GetString(1);
        bool done = reader.GetBoolean(2);

        Debug.Assert(inDBId == id);

        var item = new Item
        {
            ID = inDBId,
            Title = title,
            Done = done,
        };

        return item;
    }

    public List<Item> GetAll()
    {
        conn.Open();

        var command = conn.CreateCommand();
        command.CommandText =
        @"
            SELECT * FROM todos;
        ";

        using var reader = command.ExecuteReader();

        var items = new List<Item>();
        while (reader.Read())
        {
            TodoID id = reader.GetInt32(0);
            string title = reader.GetString(1);
            bool done = reader.GetBoolean(2);

            items.Add(new Item
            {
                ID = id,
                Title = title,
                Done = done,
            });
        }

        return items;
    }

    public bool Remove(int id)
    {
        conn.Open();

        var command = conn.CreateCommand();
        command.CommandText =
        @"
            DELETE FROM todos WHERE id = $id;
        ";

        command.Parameters.AddWithValue("$id", id);

        int removedCount = command.ExecuteNonQuery();
        if (removedCount > 1)
            throw new Exception($"ASSERTION FAILED: expected remove command to always remove 0 or 1 todos, but it removed {removedCount} todos");

        return removedCount == 1;
    }

    public bool Toggle(int id)
    {
        conn.Open();

        var command = conn.CreateCommand();
        command.CommandText =
        @"
            UPDATE todos
            SET done = 1 - done
            WHERE id = $id;
        ";

        command.Parameters.AddWithValue("$id", id);

        int updatedCount = command.ExecuteNonQuery();
        if (updatedCount > 1)
            throw new Exception($"ASSERTION FAILED: expected toggle command to always update 0 or 1 todos, but it removed {updatedCount} todos");

        return updatedCount == 1;
    }
}

internal record Item
{
    public required int ID { get; set; }
    public required string Title { get; set; }
    public bool Done { get; set; } = false;

    public override string ToString()
    {
        return $"{ID:D4}: {(Done ? "[X]" : "[ ]")} {Title}";
    }
}

internal record Cmd
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string UsageText { get; init; }
    public required Action<IDB, string[]> Handler { get; init; }
}
