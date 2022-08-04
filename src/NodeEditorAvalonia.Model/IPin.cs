namespace NodeEditor.Model;

public interface IPin
{
    string? Name { get; set; }
    INode? Parent { get; set; }
    double X { get; set; }
    double Y { get; set; }
    double Width { get; set; }
    double Height { get; set; }
    PinAlignment Alignment { get; set; }
    bool CanConnect();
    bool CanDisconnect();
}
