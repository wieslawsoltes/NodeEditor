# Wire zip ties

Zip ties gather independent wires into parallel lanes between movable collars.
Wires retain their endpoints, identity, color, connection rules, and individual
routing configuration. Dissolving a bundle restores ordinary wire routing.

## Implementation scope

- Optional model capabilities for bundle membership and editing, preserving
  existing custom `IConnector` and `IDrawingNode` implementations.
- Two end collars and optional intermediate collars, stable lane ordering,
  configurable spacing, visibility, and locking.
- Shared geometry for rendering, selection, labels, arrows, crossings, and
  spatial hit testing, including Bezier, straight, and orthogonal wires.
- Pointer and keyboard editing through Avalonia behaviors, with undo batching.
- Serialization, cloning, copy/paste, and cleanup when wires or nodes are deleted.
- An executable Avalonia WebAssembly demo and automated regression coverage.

The implementation follows the supplied zip-tie interaction recording. This
document will include complete integration and interaction details as validation
finishes.
