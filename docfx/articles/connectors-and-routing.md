# Connectors and Routing

Connectors link pins and define how wires are rendered and routed.

## ConnectorViewModel

`ConnectorViewModel` implements `IConnector` and tracks:

- Start and end pins.
- Style (`Bezier`, `Straight`, `Orthogonal`).
- Routing mode (`Auto` or `Manual`).
- Orientation (`Auto`, `Horizontal`, `Vertical`).
- Arrow styles (`None`, `Arrow`, `Circle`, `Diamond`).
- Waypoints (for manual routing).

The MVVM implementation observes pin and parent changes so connectors redraw when a node moves or rotates.

## Control points for Bezier connectors

`ConnectorExtensions.GetControlPoints` uses the pin alignment and connector orientation to compute Bezier control points. This ensures the curve flows out of a pin in the expected direction.

## Routing modes

- **Auto**: the routing engine computes a path based on obstacles.
- **Manual**: use `Waypoints` to define the path. If waypoints are empty, the connector falls back to a simple path.

## Auto routing algorithm

`OrthogonalRouter` builds a grid and runs a shortest path search with:

- Obstacle avoidance (nodes are treated as blocked cells).
- Bend penalties (`RoutingBendPenalty`).
- Optional diagonal movement (`ConnectorRoutingAlgorithm.Octilinear`).
- A maximum grid size (`RoutingMaxCells`).

If routing fails, the connector falls back to a simple orthogonal or straight path.

## Routing settings

Auto routing uses these `IDrawingNodeSettings` values:

- `EnableConnectorRouting`
- `RoutingGridSize`, `RoutingObstaclePadding`
- `RoutingAlgorithm`, `RoutingBendPenalty`, `RoutingDiagonalCost`
- `RoutingCornerRadius`

## Arrow styles

`Connector` renders optional arrows at either end. Arrow size scales with the connector stroke thickness.

## Manual waypoint example

```csharp
connector.RoutingMode = ConnectorRoutingMode.Manual;
connector.Waypoints = new List<ConnectorPoint>
{
    new ConnectorPoint { X = 200, Y = 120 },
    new ConnectorPoint { X = 220, Y = 160 }
};
```
