using System;
using System.Collections.Generic;

namespace NodeEditor.Model;

public class DrawingNodeFactory
{
    public Func<IPin> CreatePin { get; set; }

    public Func<IConnector> CreateConnector { get; set; }

    public Func<IList<IConnector>> CreateConnectorList { get; set; }
}
