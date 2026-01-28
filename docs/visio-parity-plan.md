# Visio-Level Editing Parity Plan (Core Node Editor)

## Scope and intent
This plan targets core Visio-like editing behaviors that belong in the NodeEditor control itself: selection, smart guides, alignment/distribution, ordering, snapping, and rendering feedback. It intentionally avoids host-app features (stencils, multi-page documents, data linking, printing pipelines) which are outside NodeEditor’s responsibilities.

## Parity targets (core)
- Smart guides with visual alignment lines and snapping to nearby shapes.
- Alignment and distribution commands for selected nodes.
- Z-order management for selected nodes.
- Consistent selection/dragging across nodes/connectors with guide updates.
- Settings UI to toggle guides and tune snap tolerance.

## Implementation plan
### Phase 1: Smart guides and snapping
- [x] Add guide settings to `IDrawingNodeSettings` and default settings view model.
- [x] Add `GuidesAdorner` + `GuideLine` to render alignment lines on drag.
- [x] Compute guide snap deltas during node drag and show guides.
- [x] Theme resources for guide line styling.

### Phase 2: Alignment and distribution tools
- [x] Add `NodeAlignment` and `NodeDistribution` enums to the model.
- [x] Implement `AlignSelectedNodes` and `DistributeSelectedNodes` in the editor service.
- [x] Expose commands on `IDrawingNode`/`DrawingNodeViewModel`.
- [x] Add context-menu entries for align/distribute in the editor template.

### Phase 3: Ordering (Z-order)
- [x] Add `NodeOrder` enum and `OrderSelectedNodes` service implementation.
- [x] Expose ordering command and context-menu entries.

### Phase 4: Settings surface for guides
- [x] Add guide properties to `DrawingNodeProperties` control.
- [x] Bind guide settings in sample settings UI.

## Notes
- These changes bring the core editor experience to a Visio-like baseline. For advanced Visio features (stencils, multi-page docs, data linking), the recommended approach is to implement them at the host application layer that composes NodeEditor.
