# Visio Parity Plan v2 (NodeEditor Avalonia)

Goal
- Deliver a Visio-style editing baseline for the core object model, routing, and rendering that enables professional diagram creation (rotation, locking/visibility, auto-routed orthogonal connectors, arrowheads, and richer settings).

Implemented Plan
- [x] Extend the object model for professional editing
  - [x] Add node rotation, visibility, and lock state.
  - [x] Add connector routing mode and arrowhead styles.
  - [x] Add routing settings to drawing settings (enable, grid size, obstacle padding).
- [x] Add routing service with obstacle avoidance
  - [x] Implement a grid-based orthogonal router that avoids node bounds.
  - [x] Fall back to midpoint routing when auto-routing is disabled or infeasible.
- [x] Upgrade rendering/interaction
  - [x] Apply node rotation transforms in the control template.
  - [x] Render connector arrowheads (arrow, circle, diamond) based on model settings.
  - [x] Respect visibility/lock state in selection, movement, and connector editing.
- [x] Update UI and configuration surfaces
  - [x] Add routing settings to the properties flyout.
  - [x] Add state actions (lock/unlock/hide/show/show all) to the editor context menu.
  - [x] Add connector routing/arrow settings to the connector context menu.
  - [x] Add node rotation/lock/visibility controls in the node context menu.

Implementation Notes (Key Files)
- Model extensions: `src/NodeEditorAvalonia.Model/INode.cs`, `src/NodeEditorAvalonia.Model/IConnector.cs`, `src/NodeEditorAvalonia.Model/IDrawingNode.cs`
- New enums: `src/NodeEditorAvalonia.Model/ConnectorRoutingMode.cs`, `src/NodeEditorAvalonia.Model/ConnectorArrowStyle.cs`
- View models: `src/NodeEditorAvalonia.Mvvm/NodeViewModel.cs`, `src/NodeEditorAvalonia.Mvvm/ConnectorViewModel.cs`, `src/NodeEditorAvalonia.Mvvm/DrawingNodeViewModel.cs`
- Routing service: `src/NodeEditorAvalonia/OrthogonalRouter.cs`
- Rendering updates: `src/NodeEditorAvalonia/Controls/Connector.cs`, `src/NodeEditorAvalonia/Themes/Controls/Node.axaml`
- Settings UI: `src/NodeEditorAvalonia/Controls/DrawingNodeProperties.cs`, `src/NodeEditorAvalonia/Themes/Controls/DrawingNodeProperties.axaml`
- Context menus: `src/NodeEditorAvalonia/Themes/Controls/Editor.axaml`, `src/NodeEditorAvalonia/Themes/Controls/Connector.axaml`

Out-of-Scope (Future Extensions)
- Layer manager UI (visibility/locking by layer)
- Group/ungroup with nested selection and transforms
- Advanced text formatting and shape data tables
- Connector line jumps, routing around connectors, and full flowchart stencil set
