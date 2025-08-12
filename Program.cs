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

        using IDatabase db = dbType switch
        {
            null or "sqlite" => new SQLiteDatabase(dbPath ?? DEFAULT_SQLITE_DB_PATH),
            "json" => new JSONDatabase(dbPath ?? DEFAULT_JSON_DB_PATH),
            _ => HandleInvalidDBType(dbType),
        };

        static IDatabase HandleInvalidDBType(string dbType)
        {
            Console.WriteLine($"Unknown database type '{dbType}', defaulting to sqlite");
            return new SQLiteDatabase(DEFAULT_SQLITE_DB_PATH);
        }

        return CLI.Run(db, args);
    }
}
