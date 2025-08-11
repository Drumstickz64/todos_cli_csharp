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
