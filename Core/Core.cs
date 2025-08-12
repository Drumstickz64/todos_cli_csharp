namespace TodoList.Core;

record class Item(int ID, string Title, bool Done)
{
    public override string ToString()
    {
        return $"{ID:D4}: {(Done ? "[X]" : "[ ]")} {Title}";
    }
}
