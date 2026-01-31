# Getting Started

## 1. Install Packages

Most apps only need `NodeEditorAvalonia` (controls + theme). Add `NodeEditorAvalonia.Model` if you want to implement custom models, and `NodeEditorAvalonia.Mvvm` if you want the default MVVM layer.

```bash
dotnet add package NodeEditorAvalonia
dotnet add package NodeEditorAvalonia.Model
dotnet add package NodeEditorAvalonia.Mvvm
```

## 2. Add Themes

Add the NodeEditor theme to your `App.axaml` so default templates are available.

```xml
<Application.Styles>
  <FluentTheme />
  <StyleInclude Source="avares://NodeEditorAvalonia/Themes/NodeEditorTheme.axaml" />
</Application.Styles>
```

## 3. Add the Editor Control

The editor binds to a drawing source that contains nodes, pins, and links.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:editor="clr-namespace:NodeEditor.Controls;assembly=NodeEditorAvalonia">
  <editor:Editor DrawingSource="{Binding Editor.Drawing}" />
</UserControl>
```

## 4. Wire up Views

Node contents are regular Avalonia controls. Use a view locator or data templates to map node view models to views. See the sample projects for concrete patterns.
