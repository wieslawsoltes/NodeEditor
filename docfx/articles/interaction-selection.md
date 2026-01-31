# Interaction and Selection

Selection and interaction are implemented in the UI layer with behaviors attached to the editor control.

## Selection modes

Selection supports three modes:

- **Replace**: select only the current hit set.
- **Add**: add to the selection (`Shift`).
- **Toggle**: toggle selection state (`Ctrl` / `Cmd`).

The selection logic uses `HitTestHelper` and updates selection state on the drawing (`IDrawingNode`).

## Lasso selection

Dragging on empty space creates a selection rectangle. Nodes and connector segments intersecting the rectangle are selected.

## Selected adorners

Selection is visualized using:

- `SelectionAdorner` for the lasso rectangle.
- `SelectedAdorner` for the bounding rectangle of selected nodes.
- `ConnectorSelectedAdorner` for selected connector highlighting.

## Connector selection

Connector selection uses a spatial index of connector segments, so large graphs remain responsive even with many connectors.

## Connection rejection

If a connection is rejected (direction, bus width, or custom validation), the drawing raises `ConnectionRejected`. The UI shows feedback using `ConnectionRejectedBrush` resources in the default theme.

## Keyboard nudge

Arrow keys move selected nodes. Snap settings (`SnapX`, `SnapY`) and nudge settings (`NudgeStep`, `NudgeMultiplier`) control the move delta.
