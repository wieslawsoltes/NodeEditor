# Serialization and Persistence

The MVVM layer provides `NodeSerializer`, a JSON serializer for drawings, nodes, and connectors.

## NodeSerializer

Key characteristics:

- Uses Newtonsoft.Json.
- `TypeNameHandling.Objects` to preserve runtime types.
- `PreserveReferencesHandling.Objects` to preserve shared references.
- Custom contract resolver to materialize `IList<T>` as a concrete list type.

## Basic usage

```csharp
var serializer = new NodeSerializer(typeof(System.Collections.ObjectModel.ObservableCollection<>));

var drawing = new DrawingNodeViewModel();

drawing.SetSerializer(serializer);

serializer.Save("drawing.json", drawing);

var loaded = serializer.Load<DrawingNodeViewModel>("drawing.json");
```

## Clipboard and undo

`DrawingNodeViewModel` uses the serializer to:

- Copy/paste nodes and connectors.
- Capture undo/redo snapshots.

If you provide a custom serializer, ensure it can round-trip the node, pin, and connector types you use.

## Custom serializers

You can implement `INodeSerializer` to use alternative formats or faster binary serialization.
