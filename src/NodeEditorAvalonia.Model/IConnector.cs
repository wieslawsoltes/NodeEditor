using System.ComponentModel;

namespace NodeEditor.Model;

public interface IConnector
{
    string? Name { get; set; }
    IDrawingNode? Parent { get; set; }
    ConnectorOrientation Orientation { get; set; }
    IPin? Start { get; set; }
    IPin? End { get; set; }
    double Offset { get; set; }
    bool CanSelect();
    bool CanRemove();
}
