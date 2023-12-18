using System.Collections.Generic;

namespace NodeEditor.Model;

public interface INodeFactory
{
    IList<INodeTemplate> CreateTemplates();
    IDrawingNode CreateDrawing(string? name = null);
}
