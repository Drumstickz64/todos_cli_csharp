using System.Diagnostics;
using Microsoft.Data.Sqlite;
using TodoList.Core;

namespace TodoList.Database;

sealed class SqliteDatabase : IDatabase
{
    private readonly SqliteConnection conn;

    public SqliteDatabase(string path)
    {
        conn = new SqliteConnection($"Data Source={path}");
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

    public int Add(string title)
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

        int inDBId = reader.GetInt32(0);
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
            int id = reader.GetInt32(0);
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