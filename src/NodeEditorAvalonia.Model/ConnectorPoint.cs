using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NodeEditor.Model;

public sealed class ConnectorPoint : INotifyPropertyChanged
{
    private double _x;
    private double _y;

    public event PropertyChangedEventHandler? PropertyChanged;

    public double X
    {
        get => _x;
        set
        {
            if (value.Equals(_x))
            {
                return;
            }

            _x = value;
            OnPropertyChanged();
        }
    }

    public double Y
    {
        get => _y;
        set
        {
            if (value.Equals(_y))
            {
                return;
            }

            _y = value;
            OnPropertyChanged();
        }
    }

    public ConnectorPoint()
    {
    }

    public ConnectorPoint(double x, double y)
    {
        _x = x;
        _y = y;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
