namespace NodeEditor.Model
{
    public interface IPin
    {
        INode? Parent { get; set; }
        double X { get; set; }
        double Y { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        PinAlignment Alignment { get; set; }
    }
}
