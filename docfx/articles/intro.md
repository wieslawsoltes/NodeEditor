# Introduction

NodeEditor is an Avalonia control for building node-based editors. It renders nodes, pins, and connectors with fully templated visuals, while keeping behavior in view models that you can customize or replace. Node contents are regular Avalonia controls, so you can compose complex UIs inside each node.

## Core Concepts

- **Editor**: The main control that hosts the canvas, nodes, connectors, and selection.
- **Nodes and Pins**: Nodes represent items on the canvas, while pins expose connection points.
- **Connectors**: Visuals and hit-testing for links between pins.
- **Templates and Styles**: All visuals are defined in XAML and can be re-templated.
- **View Models**: The default implementation lives in `NodeEditorAvalonia.Mvvm`, while interfaces are in `NodeEditorAvalonia.Model`.

## Packages

| Package | Description |
| --- | --- |
| [NodeEditorAvalonia](https://www.nuget.org/packages/NodeEditorAvalonia) | Avalonia controls and default theme. |
| [NodeEditorAvalonia.Model](https://www.nuget.org/packages/NodeEditorAvalonia.Model) | Core model interfaces shared by controls and view models. |
| [NodeEditorAvalonia.Mvvm](https://www.nuget.org/packages/NodeEditorAvalonia.Mvvm) | Default MVVM view models built on CommunityToolkit.Mvvm. |
| [NodeEditorLogic.Core](https://www.nuget.org/packages/NodeEditorLogic.Core) | Core logic sample models and services used by LogicLab. |
| [NodeEditorLogic.Editor](https://www.nuget.org/packages/NodeEditorLogic.Editor) | LogicLab editor-specific types and tooling. |
| [NodeEditor.Base](https://www.nuget.org/packages/NodeEditor.Base) | Base sample assets and view models. |
| [NodeEditor.Logic](https://www.nuget.org/packages/NodeEditor.Logic) | LogicLab sample assets and view models. |
