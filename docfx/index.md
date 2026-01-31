# NodeEditor for Avalonia

**NodeEditor** is a node editor control for Avalonia. It lets you build interactive graphs with nodes, pins, and connectors, while keeping node content and styling fully customizable in XAML. The default MVVM layer is built with CommunityToolkit.Mvvm, but you can plug in your own view models by implementing the core model interfaces.

## Getting Started

### Install

```bash
dotnet add package NodeEditorAvalonia
```

```xml
<PackageReference Include="NodeEditorAvalonia" Version="..." />
```

### Add Themes in App.axaml

```xml
<Application.Styles>
  <FluentTheme />
  <StyleInclude Source="avares://NodeEditorAvalonia/Themes/NodeEditorTheme.axaml" />
</Application.Styles>
```

### Basic Usage

```xml
<editor:Editor DrawingSource="{Binding Editor.Drawing}" />
```

## Documentation Sections

- **[Articles](articles/intro.md)**: Guides for concepts, setup, and samples.
- **[API Documentation](api/index.md)**: Reference for all public types and members.

## License

NodeEditor is licensed under the MIT License. See the [License](articles/license.md) page for details.
