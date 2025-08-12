using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using TodoList.Core;

namespace TodoList.Database;

sealed class SQLiteDatabase(string path) : IDatabase
{
    private TodosContext _ctx = new(path);

    public void Dispose()
    {
        _ctx.Dispose();
    }

    public int Add(string title)
    {

        Item item = new() { Title = title };
        _ctx.Add(item);
        _ctx.SaveChanges();
        return item.ID;

    }

    public Item Get(int id)
    {
        return _ctx.Find<Item>(id) ?? throw new Exception($"No todo item with the id {id} was found");
    }

    public List<Item> GetAll()
    {
        return [.. _ctx.Items];
    }

    public bool Remove(int id)
    {
        Item? item = _ctx.Find<Item>(id);
        if (item is null) return false;

        _ctx.Remove(item);
        _ctx.SaveChanges();
        return true;
    }

    public bool Toggle(int id)
    {
        Item? item = _ctx.Find<Item>(id);
        if (item is null) return false;

        item.Done = !item.Done;
        _ctx.SaveChanges();
        return true;
    }

    public class TodosContext() : DbContext()
    {
        private readonly string _dbPath = "todos.db";

        public DbSet<Item> Items { get; set; }

        public string DbPath => _dbPath;

        internal TodosContext(string path) : this()
        {
            _dbPath = path;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data source=todos.db");
    }
}
