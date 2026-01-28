namespace NodeEditor.Model;

public interface IConnectablePin
{
    PinDirection Direction { get; }
    int BusWidth { get; }
}
