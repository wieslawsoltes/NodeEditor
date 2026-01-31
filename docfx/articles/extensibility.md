# Extensibility

NodeEditor is designed to be customized at both the model and UI layers.

## Custom model implementations

You can implement the core interfaces yourself:

- `INode`, `IPin`, `IConnector`
- `IDrawingNode`, `IDrawingNodeSettings`
- `INodeTemplate`, `INodeFactory`

This is useful when you need custom persistence, performance tuning, or integration with other domain models.

## Connection validation

Use `IDrawingNodeSettings.ConnectionValidator` to enforce custom connection rules:

```csharp
settings.ConnectionValidator = context =>
{
    // Validate context.Start and context.End
    return true;
};
```

The editor uses this validator during connection gestures and when swapping connector direction.

## Custom routing

You can:

- Disable auto routing (`EnableConnectorRouting = false`).
- Force manual routing with waypoints (`ConnectorRoutingMode.Manual`).
- Implement custom routing by updating `Waypoints` or providing a custom connector style.

## Custom templates and toolbox

Replace the toolbox UI by supplying your own control bound to `INodeTemplate` items. You can also create your own drag-and-drop behaviors if you need a different workflow.

## Custom view models

The MVVM layer is optional. You can subclass the provided view models or replace them entirely. Key extension points:

- `DrawingNodeViewModel` for editor behavior and undo/redo.
- `NodeViewModel` and `PinViewModel` for custom properties.
- `ConnectorViewModel` for custom connection metadata.
