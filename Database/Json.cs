using System.Text.Json;

using TodoList.Core;
namespace TodoList.Database;

sealed class JsonDatabase : IDatabase
{
    private readonly List<Item> _items = [];

    public JsonDatabase(string path)
    {
        string json = File.ReadAllText(path);
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

    public Item Get(int id)
    {
        return _items[id];
    }

    public int Add(string title)
    {
        int id = _items.Count - 1;
        _items.Add(new Item
        {
            ID = id,
            Title = title,
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