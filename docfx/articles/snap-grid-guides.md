# Snap, Grid, and Guides

The editor supports snapping, grid alignment, and guide lines to keep layouts tidy.

## Snap

When `EnableSnap` is true, drag operations are snapped to `SnapX` / `SnapY` increments. Snapping is applied to:

- Node dragging.
- Selection dragging.
- Node resizing (if snap is enabled on the drawing).

## Grid

Grid settings are defined in `IDrawingNodeSettings`:

- `EnableGrid`
- `GridCellWidth`
- `GridCellHeight`

The default theme can display a grid background in the editor template.

## Guides

Guides appear while moving or resizing nodes and help align edges and centers.

- `EnableGuides`: toggles guide usage.
- `GuideSnapTolerance`: alignment threshold in pixels.

Guides are shown via `GuidesAdorner` in the adorner layer.

## Nudge

Arrow keys move selected nodes by:

- `NudgeStep` (default 1)
- `NudgeMultiplier` (default 10 when Shift is held)

If snap is enabled, nudge uses the snap step instead of the base step.
