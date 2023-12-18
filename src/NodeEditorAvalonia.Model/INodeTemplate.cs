namespace NodeEditor.Model;

public interface INodeTemplate
{
    string? Title { get; set; }
    INode? Template { get; set; }
    INode? Preview { get; set; }
}
