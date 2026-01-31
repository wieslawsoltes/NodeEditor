# Core Model Interfaces

This section maps the model layer (`NodeEditorAvalonia.Model`) which defines all core concepts and behaviors.

## IEditor

```csharp
public interface IEditor
{
    IList<INodeTemplate>? Templates { get; set; }
    IDrawingNode? Drawing { get; set; }
}
```

`IEditor` is a lightweight host contract used by the MVVM layer and UI. It pairs a drawing with a set of templates for the toolbox.

## IDrawingNode

`IDrawingNode` is the root of a graph. It extends `INode` and adds:

- Node and connector collections (`Nodes`, `Connectors`).
- Ink strokes (`InkStrokes`).
- Settings (`IDrawingNodeSettings`).
- Selection state and events.
- Editor commands for clipboard, alignment, ordering, visibility, and more.

The drawing is also responsible for selection notifications and connection rejection events.

## INode

A node represents a visual container positioned on the canvas.

Key properties:

- Position, size, rotation: `X`, `Y`, `Width`, `Height`, `Rotation`.
- Content object: `Content` (typically a view model).
- Pins: `IList<IPin>? Pins`.
- Visibility and locking: `IsVisible`, `IsLocked`.

Key behaviors:

- `CanSelect/CanRemove/CanMove/CanResize` gate interaction.
- `Move` and `Resize` implement geometry updates.
- Events: Created, Removed, Moved, Selected, Deselected, Resized.

## IPin and IConnectablePin

`IPin` is a connection point on a node.

Core properties:

- Position and size: `X`, `Y`, `Width`, `Height`.
- Alignment: `PinAlignment` (Left, Right, Top, Bottom, None).
- Connection capability: `CanConnect`, `CanDisconnect`.

`IConnectablePin` extends `IPin` with connection semantics:

- `Direction`: Input, Output, or Bidirectional.
- `BusWidth`: integer width for bus signals.

## IConnector

A connector defines a link between two pins:

- Endpoints: `Start`, `End`.
- Style: `ConnectorStyle` (Bezier, Straight, Orthogonal).
- Routing mode: `ConnectorRoutingMode` (Auto, Manual).
- Orientation: `ConnectorOrientation` (Auto, Horizontal, Vertical).
- Arrow styles: `ConnectorArrowStyle` (None, Arrow, Circle, Diamond).
- `Waypoints` for manual routing.
- Visibility and locking: `IsVisible`, `IsLocked`.

Like nodes and pins, connectors raise Created/Removed/Selected events.

## Templates and factories

- `INodeTemplate` represents a toolbox template with a title, a template node, and a preview node.
- `INodeTemplatesHost` exposes a template list on a host view model.
- `INodeFactory` creates templates and drawings.
- `IDrawingNodeFactory` creates pins, connectors, and lists used by the editor implementation.

## Serialization and undo hosting

- `INodeSerializer` provides `Serialize` / `Deserialize` for graph persistence and clipboard support.
- `IUndoRedoHost` exposes undo/redo state and batching APIs used by the UI behaviors.

## Enumerations

Key enums defined in the model layer:

- `PinAlignment`, `PinDirection`.
- `ConnectorStyle`, `ConnectorRoutingMode`, `ConnectorRoutingAlgorithm`, `ConnectorOrientation`, `ConnectorArrowStyle`.
- `NodeAlignment`, `NodeDistribution`, `NodeOrder`.
- `NodeResizeDirection`.

These are used across UI, MVVM, and logic layers to keep interaction consistent.
