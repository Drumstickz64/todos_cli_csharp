namespace TodoList.CommandLine;

using TodoList.Core;
using TodoList.Database;

static class CLI
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

                int id = db.Add(title);

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

    public static int Run(IDatabase db, string[] args)
    {
        if (args.Length <= 0)
        {
            Console.WriteLine("Expected a command name, but got none");
            DisplayHelpMessage();
            return 1;
        }

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

class CLIException(string message, Exception? innerException = null) : Exception(message, innerException);

record Cmd
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string UsageText { get; init; }
    public required Action<IDatabase, string[]> Handler { get; init; }
}