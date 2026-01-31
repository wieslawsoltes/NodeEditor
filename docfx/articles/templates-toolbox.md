# Templates and Toolbox

Templates enable drag-and-drop node creation and are exposed via the toolbox control.

## INodeTemplate

A node template consists of:

- `Title`: display name.
- `Template`: the node instance to insert.
- `Preview`: a lightweight node used for UI preview.

`NodeTemplateViewModel` provides a default implementation.

## Template sources

Templates are typically created by an `INodeFactory`:

```csharp
public interface INodeFactory
{
    IList<INodeTemplate> CreateTemplates();
    IDrawingNode CreateDrawing(string? name = null);
}
```

## Toolbox control

`NodeEditor.Controls.Toolbox` binds to `IEnumerable<INodeTemplate>` via `TemplatesSource`.

### Drag and drop

`ToolboxDragBehavior` enables dragging templates into the editor. Templates are serialized into the drag payload under the "NodeTemplate" format so drop handlers can retrieve them.

### Double click insertion

`InsertTemplateOnDoubleTappedBehavior` can insert a template directly on double click.

## Host integration

The MVVM `EditorViewModel` implements `INodeTemplatesHost`, so a typical binding looks like:

```xml
<editor:Toolbox TemplatesSource="{Binding Editor.Templates}" />
```

## Using templates in a custom factory

Use templates to provide consistent node creation and previews:

```csharp
var template = new NodeTemplateViewModel
{
    Title = "My Node",
    Template = CreateNode(),
    Preview = CreatePreviewNode()
};
```
