# Move, Resize, and Rotate

The UI layer provides behaviors to transform nodes with interactive handles.

## Dragging

Dragging selected nodes moves them and optionally snaps to the grid. When guides are enabled, the selection snaps to alignment lines with nearby nodes.

## Resizing

`NodeResizeBehavior` is attached to resize handles in the node template. It supports:

- Eight directions (`NodeResizeDirection`).
- Snap and guide alignment.
- Undo batching when available.

Pins are repositioned based on their alignment so they stay attached to the correct side after resize.

## Rotation

`NodeRotateBehavior` supports:

- Drag-based rotation around the node center.
- Angle snapping (15-degree increments).
- A rotation readout badge using theme resources:
  - `RotationSnapReadoutBackgroundBrush`
  - `RotationSnapReadoutBorderBrush`
  - `RotationSnapReadoutForegroundBrush`

Rotation updates connector endpoints and selection bounds automatically.

## Zoom and Pan

`NodeZoomBorder` wraps the drawing and provides:

- Zoom in/out.
- Reset zoom.
- Fit or fill the drawing area.

The sample apps bind these commands to keyboard shortcuts.
