namespace NodeEditor.Model;

public readonly struct ConnectionValidationContext
{
    public IDrawingNode Drawing { get; }
    public IPin Start { get; }
    public IPin End { get; }

    public ConnectionValidationContext(IDrawingNode drawing, IPin start, IPin end)
    {
        Drawing = drawing;
        Start = start;
        End = end;
    }
}

public delegate bool ConnectionValidationHandler(ConnectionValidationContext context);
