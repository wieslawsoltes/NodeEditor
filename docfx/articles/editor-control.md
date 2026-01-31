# Editor Control and Visual Tree

The Avalonia UI layer defines a set of controls that render the drawing and handle input. These controls bind directly to the model interfaces.

## Editor

`NodeEditor.Controls.Editor` is the main control. It exposes:

- `DrawingSource`: the `IDrawingNode` to render.
- `InputSource`: control that receives input (usually the editor itself).
- `AdornerCanvas`: a canvas used for selection and guide overlays.
- `ZoomControl`: a `NodeZoomBorder` used for zoom and pan.

The control template defines two named parts:

- `PART_ZoomBorder`
- `PART_AdornerCanvas`

### Basic XAML

```xml
<editor:Editor DrawingSource="{Binding Editor.Drawing}" />
```

## DrawingNode / Nodes / Connectors

The rendering pipeline is composed of nested templated controls:

- `DrawingNode`: hosts the full drawing.
- `Nodes`: renders the node list.
- `Connectors`: renders connector list.
- `Pins`: renders pins inside each node.

Each control has a `DrawingSource` or `NodeSource` property to bind to model data.

## NodeZoomBorder

`NodeZoomBorder` is a specialized `ZoomBorder` with commands:

- `ZoomIn`, `ZoomOut`
- `ResetZoom`
- `FitCanvas`, `FitToFill`, `Fill`

This matches the key bindings in the samples and provides a clean API for toolbar buttons.

## ExportRoot

`ExportRoot` is a simple decorator used as a top-level container for exporting the editor content. It ensures a stable visual root for export operations.

## EditableTextBlock

`EditableTextBlock` is used in the default templates to allow inline editing of node labels.

## Control templates

Default templates are provided in `NodeEditorAvalonia/Themes/Controls/*.axaml`. You can replace any of these with custom templates by overriding them in your app styles.
