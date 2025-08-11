namespace TodoList.Core;

record Item
{
    public required int ID { get; set; }
    public required string Title { get; set; }
    public bool Done { get; set; } = false;

    public override string ToString()
    {
        return $"{ID:D4}: {(Done ? "[X]" : "[ ]")} {Title}";
    }
}
