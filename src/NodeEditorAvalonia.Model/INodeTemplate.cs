using System;

namespace NodeEditor.Model;

public interface INodeTemplate
{
    string? Title { get; set; }
    Func<double, double, INode>? Build { get; set; }
    INode? Preview { get; set; }
}
