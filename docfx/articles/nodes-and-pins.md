# Nodes and Pins

Nodes and pins are the core building blocks of a graph. The MVVM layer provides concrete implementations that you can extend or replace.

## NodeViewModel

`NodeViewModel` implements `INode` and provides:

- Basic geometry and layout (`X`, `Y`, `Width`, `Height`, `Rotation`).
- Visibility and lock state.
- `Pins` list and `Content` object.
- Event hooks for created, moved, resized, and selected states.

### Resize behavior

`NodeViewModel.Resize` updates geometry based on `NodeResizeDirection`. It also updates pin positions so pins stay aligned relative to the node when the node is resized.

### Move and lock

`Move` and `Resize` respect `CanMove` / `CanResize`. Locking a node (`IsLocked`) disables movement and resizing.

## PinViewModel

`PinViewModel` implements `IPin` and `IConnectablePin`:

- Position and size relative to the node.
- Alignment (`Left`, `Right`, `Top`, `Bottom`, `None`).
- Direction (`Input`, `Output`, `Bidirectional`).
- Bus width (`BusWidth`) for multi-bit connectors.

### Bus width

Bus widths are clamped to at least 1. If you need to represent bus signals, set `BusWidth > 1` and update the connector validation rules accordingly.

## Adding pins

Use the extension method from `NodeViewModelExtensions`:

```csharp
var node = new NodeViewModel
{
    Name = "Example",
    X = 100,
    Y = 100,
    Width = 180,
    Height = 100
};

node.AddPin(x: 0, y: 30, width: 10, height: 10, alignment: PinAlignment.Left, name: "In");
node.AddPin(x: 180, y: 30, width: 10, height: 10, alignment: PinAlignment.Right, name: "Out");
```

## Alignment effects

Pin alignment is used for:

- Calculating connector control points.
- Auto-routing orientation.
- Pin repositioning when nodes are resized.

## Node content

`INode.Content` is an arbitrary object. In MVVM scenarios, the content is typically a view model bound by a view locator or data template.
