using System.Collections.Generic;
using System.Windows.Input;

namespace NodeEditor.Model
{
    public interface IDrawingNode : INode
    {
        IList<INode>? Nodes { get; set; }
        IList<IConnector>? Connectors { get; set; }
        ISet<INode>? SelectedNodes { get; set; }
        ISet<IConnector>? SelectedConnectors { get; set; }
        INodeSerializer? Serializer { get; set; }
        void DrawingLeftPressed(double x, double y);
        void DrawingRightPressed(double x, double y);
        void ConnectorLeftPressed(IPin pin);
        void ConnectorMove(double x, double y);
        void CutNodes();
        void CopyNodes();
        void PasteNodes();
        void DuplicateNodes();
        void DeleteNodes();
        void SelectAllNodes();
        void DeselectAllNodes();
        ICommand CutCommand { get; }
        ICommand CopyCommand { get; }
        ICommand PasteCommand { get; }
        ICommand DuplicateCommand { get; }
        ICommand SelectAllCommand { get; }
        ICommand DeselectAllCommand { get; }
        ICommand DeleteCommand { get; }
    }
}
