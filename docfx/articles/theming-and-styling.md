# Theming and Styling

NodeEditor ships with a default theme in `NodeEditorAvalonia/Themes/NodeEditorTheme.axaml`.

## Include the theme

```xml
<Application.Styles>
  <FluentTheme />
  <StyleInclude Source="avares://NodeEditorAvalonia/Themes/NodeEditorTheme.axaml" />
</Application.Styles>
```

## Theme resources

The theme exposes resource keys you can override in your app:

- `PinBackgroundBrush`, `PinPointerOverBackgroundBrush`
- `ConnectorBackgroundBrush`
- `NodeResizeHandleFillBrush`, `NodeResizeHandleBorderBrush`
- `GuideLineBrush`
- `ConnectionRejectedBrush` and related label brushes
- `ConnectorCrossingStrokeBrush`, `ConnectorCrossingStrokeThickness`, `ConnectorCrossingArcRadius`
- `RotationSnapReadoutBackgroundBrush`, `RotationSnapReadoutBorderBrush`, `RotationSnapReadoutForegroundBrush`

Override these in your application styles to match your design system.

## Control templates

Default templates live under:

```
NodeEditorAvalonia/Themes/Controls/
```

You can replace any control template (Node, Pin, Connector, Editor, etc.) by overriding styles with the same selectors in your app.

## Selected styles

The theme includes default styles for selected connectors and adorners. You can customize selection visuals by overriding the styles:

- `controls|Connector:selected`
- `controls|GuidesAdorner`
- `controls|ConnectorSelectedAdorner`
- `controls|ConnectorCrossingsAdorner`
