using System.Collections.Generic;

namespace NodeEditor.Model
{
    public interface IDrawingNode : INode
    {
        IList<INode>? Nodes { get; set; }
        ISet<INode>? SelectedNodes { get; set; }
        IList<IConnector>? Connectors { get; set; }
        INodeSerializer? Serializer { get; set; }
        void DrawingPressed(double x, double y);
        void DrawingCancel();
        void ConnectorPressed(IPin pin);
        void ConnectorMove(double x, double y);
        void CutNodes();
        void CopyNodes();
        void PasteNodes(double x = 0.0, double y = 0.0);
    }
}
