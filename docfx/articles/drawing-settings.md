# Drawing Settings

`IDrawingNodeSettings` controls interaction and routing behavior for a drawing. The MVVM layer provides `DrawingNodeSettingsViewModel` with sensible defaults.

## Connection settings

- `EnableConnections`: master toggle.
- `RequireDirectionalConnections`: enforce Output -> Input.
- `RequireMatchingBusWidth`: enforce bus width compatibility.
- `EnableMultiplePinConnections`: allow multiple connectors per pin.
- `AllowSelfConnections`: allow a node to connect to itself.
- `AllowDuplicateConnections`: allow duplicate links.
- `ConnectionValidator`: custom validation hook (`ConnectionValidationHandler`).

## Ink settings

- `EnableInk`, `IsInkMode`: enable ink layer and switch between selection and ink mode.
- `InkPens`, `ActivePen`: available pens and current pen.

## Snap + nudge

- `EnableSnap`: grid snapping for drag operations.
- `SnapX`, `SnapY`: snap step size.
- `NudgeStep`, `NudgeMultiplier`: keyboard nudge increments.

## Grid + guides

- `EnableGrid`: show or align to a grid.
- `GridCellWidth`, `GridCellHeight`: grid size.
- `EnableGuides`: show alignment guides.
- `GuideSnapTolerance`: snapping tolerance in pixels.

## Connector routing

- `EnableConnectorRouting`: enable auto routing.
- `RoutingGridSize`: grid size for routing.
- `RoutingObstaclePadding`: padding around nodes.
- `RoutingAlgorithm`: Auto, Orthogonal, or Octilinear.
- `RoutingBendPenalty`: cost for changing direction.
- `RoutingDiagonalCost`: cost for diagonal movement.
- `RoutingCornerRadius`: rounding for corners.
- `RoutingMaxCells`: cap on routing grid size.
- `DefaultConnectorStyle`: default connector style for new links.

## Example

```csharp
var settings = new DrawingNodeSettingsViewModel
{
    EnableSnap = true,
    SnapX = 10,
    SnapY = 10,
    EnableGuides = true,
    RequireDirectionalConnections = true,
    RequireMatchingBusWidth = true,
    DefaultConnectorStyle = ConnectorStyle.Orthogonal,
    RoutingAlgorithm = ConnectorRoutingAlgorithm.Octilinear
};
```
