# Architecture Overview

NodeEditor is split into three main layers that you can use independently or together:

1. **Model layer** (`NodeEditorAvalonia.Model`)
   - Pure interfaces and enums that describe nodes, pins, connectors, and editor behavior.
   - No Avalonia types except for primitive concepts (e.g., connectors and pins are model-only).

2. **UI layer** (`NodeEditorAvalonia`)
   - Avalonia controls, behaviors, and rendering helpers.
   - Implements selection, hit testing, routing, snapping, guides, and interaction behavior.

3. **MVVM layer** (`NodeEditorAvalonia.Mvvm`)
   - Default view models for nodes, pins, connectors, and drawings.
   - Implements undo/redo, clipboard, serialization, and ink features on top of the model interfaces.

On top of these, the **LogicLab sample** (`NodeEditorLogic.Core` + `NodeEditorLogic.Editor`) demonstrates a full domain-specific editor with component libraries, buses, and simulation.

## How the layers connect

- The **Editor control** binds to an `IDrawingNode` instance.
- `IDrawingNode` exposes nodes, connectors, and editor commands.
- The MVVM layer provides concrete `DrawingNodeViewModel`, `NodeViewModel`, `ConnectorViewModel`, and `PinViewModel` classes that implement the model interfaces.
- The UI layer reads the model interfaces and handles visuals, selection, routing, and interaction.

## Typical usage patterns

- **Application / product use**: Use MVVM types directly.
- **Framework integration**: Implement your own model types for custom persistence or performance.
- **Sample-based**: Reuse LogicLab infrastructure to build a logic or graph editor quickly.

## Dependency summary

- `NodeEditorAvalonia.Model` has no UI dependencies.
- `NodeEditorAvalonia` depends on Avalonia (controls, behaviors).
- `NodeEditorAvalonia.Mvvm` depends on CommunityToolkit.Mvvm + Newtonsoft.Json.
- `NodeEditorLogic.*` depends on the MVVM layer and adds domain-specific logic.
