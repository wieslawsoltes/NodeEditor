# Ink and Annotations

The editor includes a lightweight ink layer for sketching or annotating diagrams.

## Ink data model

`IDrawingNode` exposes `InkStrokes`, a list of `InkStroke` objects. Each stroke contains:

- `Color`, `Thickness`, `Opacity`, `Name`
- `Points` (`InkPoint` with coordinates and timestamps)

## Drawing ink

`InkLayer` is a control that listens for pointer input when ink mode is active:

- `EnableInk` enables the feature.
- `IsInkMode` toggles between edit and ink input.

The MVVM `DrawingNodeViewModel` exposes commands to:

- Toggle ink mode (`DrawInkCommand`)
- Add pens (`AddPenCommand`)
- Clear ink (`ClearInkCommand`)
- Convert ink to nodes (`ConvertInkCommand`)

## Converting ink to nodes

`DrawingNodeViewModel.ConvertInkToNodes` converts each stroke into a new node with an `InkShape` content object. The node bounds are computed from the stroke extents.

## Pens

`DrawingNodeSettingsViewModel` maintains a pen list and active pen. The default palette includes multiple colors and a basic thickness.
