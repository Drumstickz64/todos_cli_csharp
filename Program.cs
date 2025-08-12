using TodoList.Database;
using TodoList.CommandLine;

namespace TodoList;

internal class Program
{
    private const string DEFAULT_SQLITE_DB_PATH = "todos.db";
    private const string DEFAULT_JSON_DB_PATH = "db.json";

    private static int Main(string[] args)
    {
        string? dbPath = Environment.GetEnvironmentVariable("TODOS_CLI_DB_PATH");
        string? dbType = Environment.GetEnvironmentVariable("TODOS_CLI_DB_TYPE");

        using IDatabase db = CreateDBFromEnvVars(dbType, dbPath);

        return CLI.Run(db, args);
    }

    private static IDatabase CreateDBFromEnvVars(string? dbType, string? dbPath)
    {
        switch (dbType)
        {
            case null or "sqlite": return new SQLiteDatabase(dbPath ?? DEFAULT_SQLITE_DB_PATH);
            case "json": return new JSONDatabase(dbPath ?? DEFAULT_JSON_DB_PATH);
            default:
                {
                    Console.WriteLine($"Unknown database type '{dbType}', defaulting to sqlite");
                    return new SQLiteDatabase(DEFAULT_SQLITE_DB_PATH);
                }
        }
    }
}
