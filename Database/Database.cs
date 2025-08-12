namespace TodoList.Database;

using TodoList.Core;

interface IDatabase : IDisposable
{
    public List<Item> GetAll();

    public Item Get(int id);

    public int Add(string title);

    public bool Remove(int id);

    public bool Toggle(int id);
}

public class DatabaseException : Exception
{
    public DatabaseException() : base() { }

    public DatabaseException(string message) : base(message) { }

    public DatabaseException(string message, Exception innerException)
        : base(message, innerException) { }
}