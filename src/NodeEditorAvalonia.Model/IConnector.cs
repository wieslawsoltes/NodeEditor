namespace NodeEditor.Model
{
    public interface IConnector
    {
        IDrawingNode? Parent { get; set; }
        ConnectorOrientation Orientation { get; set; }
        IPin? Start { get; set; }
        IPin? End { get; set; }
        double Offset { get; set; }
    }
}
