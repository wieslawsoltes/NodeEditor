using System.Collections.Generic;

namespace NodeEditor.Model;

public interface IEditor
{
    IList<INodeTemplate>? Templates { get; set; }
    IDrawingNode? Drawing { get; set; }
}
