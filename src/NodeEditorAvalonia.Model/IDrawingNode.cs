using System.Collections.Generic;

namespace NodeEditor.Model
{
    public interface IDrawingNode : INode
    {
        IList<INode>? Nodes { get; set; }
        IList<IConnector>? Connectors { get; set; }
        void DrawingPressed(double x, double y);
        void DrawingCancel();
        void ConnectorPressed(IPin pin);
        void ConnectorMove(double x, double y);
    }
}
