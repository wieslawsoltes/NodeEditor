using System.Collections.Generic;

namespace NodeEditor.Model;

public interface IDrawingNodeFactory
{
    IPin CreatePin();
    IConnector CreateConnector();
    public IList<T> CreateList<T>();
}
