using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace NodeEditor.Model;

/// <summary>
/// A spatial wire bundle. Connections keep their endpoints and electrical meaning;
/// the ordered members share parallel lanes between the tie anchors.
/// </summary>
public interface IZipTie : INotifyPropertyChanged
{
    /// <summary>Gets or sets the optional display name.</summary>
    string? Name { get; set; }

    /// <summary>Gets or sets the first tie anchor in drawing coordinates.</summary>
    ConnectorPoint Start { get; set; }

    /// <summary>Gets or sets the last tie anchor in drawing coordinates.</summary>
    ConnectorPoint End { get; set; }

    /// <summary>Gets or sets additional tie anchors, ordered from <see cref="Start"/> to <see cref="End"/>.</summary>
    IList<ConnectorPoint> Waypoints { get; set; }

    /// <summary>Gets or sets the member wires in stable lane order.</summary>
    IList<IConnector> Connectors { get; set; }

    /// <summary>Gets or sets the finite, positive distance between adjacent wire lanes.</summary>
    double Spacing { get; set; }

    /// <summary>Gets or sets whether the bundle participates in routing and rendering.</summary>
    bool IsVisible { get; set; }

    /// <summary>Gets or sets whether interactive edits to the bundle are disabled.</summary>
    bool IsLocked { get; set; }
}

/// <summary>
/// Optional drawing capability for editing wire bundles. Existing custom
/// <see cref="IDrawingNode"/> and <see cref="IConnector"/> implementations remain compatible.
/// </summary>
public interface IZipTieHost
{
    /// <summary>Gets or sets the drawing's bundles. Use an observable collection for live updates.</summary>
    IList<IZipTie> ZipTies { get; set; }

    /// <summary>Gets or sets the actively edited bundle; selection is not document data.</summary>
    IZipTie? SelectedZipTie { get; set; }

    /// <summary>Finds the bundle owning a wire, or returns <see langword="null"/>.</summary>
    IZipTie? FindZipTie(IConnector connector);

    /// <summary>Bundles at least two eligible wires and selects the result, or returns <see langword="null"/>.</summary>
    IZipTie? CreateZipTie(IEnumerable<IConnector> connectors);

    /// <summary>Dissolves an editable bundle without deleting its wires.</summary>
    void RemoveZipTie(IZipTie tie);

    /// <summary>Appends an eligible, unbundled wire to the stable lane order.</summary>
    bool AddToZipTie(IZipTie tie, IConnector connector);

    /// <summary>Detaches a wire, dissolving the bundle when fewer than two members remain.</summary>
    bool RemoveFromZipTie(IZipTie tie, IConnector connector);

    /// <summary>Reverses the lane order without swapping connection endpoints.</summary>
    void ReverseZipTie(IZipTie tie);

    /// <summary>Removes intermediate tie anchors while retaining both end anchors.</summary>
    void StraightenZipTie(IZipTie tie);
}

/// <summary>Optional constant-time lane lookup for renderers of large bundles.</summary>
public interface IZipTieLaneProvider
{
    /// <summary>Gets the wire's stable, zero-based lane index, or minus one when it is unbundled.</summary>
    int GetZipTieLane(IConnector connector);
}

/// <summary>Optional command surface for binding zip-tie editing tools.</summary>
public interface IZipTieCommands
{
    /// <summary>Bundles the selected wires.</summary>
    ICommand CreateZipTieCommand { get; }

    /// <summary>Dissolves the selected bundle.</summary>
    ICommand RemoveZipTieCommand { get; }

    /// <summary>Adds the selected wires to the active bundle.</summary>
    ICommand AddSelectedToZipTieCommand { get; }

    /// <summary>Removes the selected wires from their bundles.</summary>
    ICommand RemoveSelectedFromZipTieCommand { get; }

    /// <summary>Reverses the active bundle's lane order.</summary>
    ICommand ReverseZipTieCommand { get; }

    /// <summary>Removes the active bundle's intermediate anchors.</summary>
    ICommand StraightenZipTieCommand { get; }
}
