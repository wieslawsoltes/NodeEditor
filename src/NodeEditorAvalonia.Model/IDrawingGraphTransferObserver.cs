using System.Collections.Generic;

namespace NodeEditor.Model;

/// <summary>
/// Optional lifecycle hook for a drawing that adopts a deserialized graph, such
/// as a clipboard paste. Called before nodes and wires receive their new parent.
/// </summary>
public interface IDrawingGraphTransferObserver
{
    /// <summary>
    /// Releases subscriptions owned by temporary source parents before their
    /// deserialized nodes and wires are adopted by the destination drawing.
    /// Implementations must preserve graph data and connection identity.
    /// </summary>
    void PrepareGraphTransfer(IEnumerable<INode>? nodes, IEnumerable<IConnector>? connectors);
}
