# Performance and Hit Testing

Large graphs require efficient hit testing and selection. NodeEditor uses spatial indexing to keep interactions fast.

## HitTestIndex

`HitTestIndex` builds two spatial indexes:

- An R-tree over node bounds.
- An R-tree over connector segments.

The index is maintained incrementally by listening to:

- Node and connector collection changes.
- Property changes on nodes, connectors, and waypoint points.
- Routing settings changes.

## RTree

`RTree<T>` is an internal, generic R-tree implementation used for spatial queries. It supports insert, update, remove, and search.

## Connector segments

Connector paths are flattened into line segments (including Bezier flattening). Each segment is indexed for fast intersection and proximity tests.

## Practical impact

- Lasso selection remains responsive for large graphs.
- Selecting connectors by proximity is precise and efficient.
- Auto-routing updates can be limited to affected connectors.
