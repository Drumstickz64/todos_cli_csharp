using System.Text.Json;

using TodoList.Core;
namespace TodoList.Database;

sealed class JSONDatabase : IDatabase
{
    private readonly List<Item> _items = [];
    private readonly string _path;

    public JSONDatabase(string path)
    {
        _path = path;

        try
        {
            string json = File.ReadAllText(path);
            _items = JsonSerializer.Deserialize<List<Item>>(json)!;
        }
        catch (FileNotFoundException) { }
    }

    public void Dispose()
    {
        Save();
    }

    public void Save()
    {
        string json = JsonSerializer.Serialize(_items);
        File.WriteAllText(_path, json);
    }
    public List<Item> GetAll()
    {
        return [.. _items];
    }

    public Item Get(int id)
    {
        if (id >= _items.Count) throw new DatabaseException($"No todo item with the id {id} was found");
        return _items[id];
    }

    public int Add(string title)
    {
        int id = _items.Count;
        _items.Add(new Item()
        {
            ID = id,
            Title = title,
            Done = false
        });
        return id;
    }

    public bool Remove(int id)
    {
        if (id >= _items.Count) return false;

        _items.RemoveAt(id);
        return true;
    }

    public bool Toggle(int id)
    {
        if (id >= _items.Count) return false;

        _items[id].Done = !_items[id].Done;
        return true;
    }
}