# Visio Parity Plan v3 (Ink + Macros)

Goal
- Deliver a richer Visio-like authoring baseline by adding ink/pen annotation, conversion to editable shapes, and a macro picker for rapid command execution.

Implemented Plan
- [x] Expand the object model for ink
  - [x] Add `InkPoint`, `InkStroke`, `InkPen`, and `InkShape` to the model.
  - [x] Extend `IDrawingNodeSettings` with ink enable/mode + active pen metadata.
  - [x] Extend `IDrawingNode` with ink strokes and commands (draw/convert/add/clear).
- [x] Add ink rendering + interaction in the core control
  - [x] Render ink in a dedicated `InkLayer` with round caps and dot handling.
  - [x] Add an `InkStrokePresenter` + data template for converted ink shapes.
  - [x] Route pointer input to ink when ink mode is active and bypass selection/connector interactions.
- [x] Implement Draw/Convert/Add Pen workflows
  - [x] `Draw` toggles ink mode.
  - [x] `Add Pen` creates a new pen and sets it active.
  - [x] `Convert Ink` turns strokes into editable nodes using `InkShape`.
  - [x] `Clear Ink` removes strokes.
- [x] Add macro picker with command routing
  - [x] Introduce macro definitions + picker view model.
  - [x] Add a palette dialog to Base + Logic samples.
  - [x] Include debug/start actions in macros (Run/Stop, Step, Reset, Clear Waveforms).
  - [x] Add View/Macros menu entries + keybinding (`Ctrl+Shift+P` / `Cmd+Shift+P`).
- [x] Fix AVLN3001 sources
  - [x] Make `ExportRoot` public with parameterless constructors.
  - [x] Make converter types referenced in XAML public.

Implementation Notes (Key Files)
- Model: `src/NodeEditorAvalonia.Model/InkPoint.cs`, `src/NodeEditorAvalonia.Model/InkStroke.cs`, `src/NodeEditorAvalonia.Model/InkPen.cs`, `src/NodeEditorAvalonia.Model/InkShape.cs`
- Interfaces: `src/NodeEditorAvalonia.Model/IDrawingNode.cs`
- View models: `src/NodeEditorAvalonia.Mvvm/DrawingNodeViewModel.cs`, `src/NodeEditorAvalonia.Mvvm/MacroDefinition.cs`, `src/NodeEditorAvalonia.Mvvm/MacroPickerViewModel.cs`
- Rendering + input: `src/NodeEditorAvalonia/Controls/InkLayer.cs`, `src/NodeEditorAvalonia/Controls/InkStrokePresenter.cs`
- Templates: `src/NodeEditorAvalonia/Themes/Controls/DrawingNode.axaml`, `src/NodeEditorAvalonia/Themes/Controls/Nodes.axaml`
- Menus + keybindings: `samples/NodeEditor.Base/Views/MenuView.axaml`, `samples/NodeEditor.Logic/Views/MenuView.axaml`, `samples/NodeEditor.Base/Views/MainView.axaml`, `samples/NodeEditor.Logic/Views/MainView.axaml`
- Macro UI: `samples/NodeEditor.Base/Views/MacroPickerWindow.axaml`, `samples/NodeEditor.Logic/Views/MacroPickerWindow.axaml`
- AVLN3001 fixes: `samples/NodeEditor.Base/Services/ExportRoot.cs`, `samples/NodeEditor.Logic/Services/ExportRoot.cs`, `src/NodeEditorAvalonia/Converters/EnumToCheckedConverter.cs`

Out-of-Scope (Future Extensions)
- Ink recognition (shape/text recognition) and snapping converted shapes to stencils.
- Layer management for ink vs nodes/connectors.
- Advanced pen properties (pressure curves, highlighters, erasers).
