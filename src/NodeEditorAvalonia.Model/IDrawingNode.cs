using System.Collections.Generic;
using System.Windows.Input;

namespace NodeEditor.Model
{
    public interface IDrawingNode : INode
    {
        IList<INode>? Nodes { get; set; }
        ISet<INode>? SelectedNodes { get; set; }
        IList<IConnector>? Connectors { get; set; }
        INodeSerializer? Serializer { get; set; }
        void DrawingLeftPressed(double x, double y);
        void DrawingRightPressed(double x, double y);
        void ConnectorLeftPressed(IPin pin);
        void ConnectorMove(double x, double y);
        void CutNodes();
        void CopyNodes();
        void PasteNodes();
        void DeleteNodes();
        ICommand CutCommand { get; }
        ICommand CopyCommand { get; }
        ICommand PasteCommand { get; }
        ICommand DeleteCommand { get; }
    }
}
