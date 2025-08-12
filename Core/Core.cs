namespace TodoList.Core;

class Item
{
    public required string Title { get; set; }
    public int ID { get; set; } = 0;
    public bool Done { get; set; } = false;

    public override string ToString()
    {
        return $"{ID:D4}: {(Done ? "[X]" : "[ ]")} {Title}";
    }
}
