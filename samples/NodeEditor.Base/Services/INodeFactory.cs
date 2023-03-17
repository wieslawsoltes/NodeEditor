using System.Collections.Generic;
using NodeEditor.Model;

namespace NodeEditorDemo.Services;

public interface INodeFactory
{
    IList<INodeTemplate> CreateTemplates();
    IDrawingNode CreateDrawing(string? name = null);
}
