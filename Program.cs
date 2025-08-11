using TodoList.Database;
using TodoList.CommandLine;

namespace TodoList;

internal class Program
{
    private static int Main(string[] args)
    {
        using var db = new SqliteDatabase("TodoList.db");
        // using var db = new JsonDatabase("db.json");

        return CLI.Run(db, args);
    }
}
