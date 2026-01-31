# LogicLab Templates and Toolbox

`LogicNodeFactory` creates the LogicLab drawing, templates, and toolbox categories.

## Drawing defaults

The factory configures `DrawingNodeSettingsViewModel` to fit logic editing:

- Snap and grid enabled.
- Orthogonal connectors.
- Directional connections required.
- Bus width matching enforced.
- Connection validation set to `LogicConnectionValidation.TypeCompatibility`.

## Toolbox categories

The toolbox is grouped into categories:

- Inputs
- Outputs
- Gates
- Memory
- Components
- Buses
- IC Library
- Annotations

Each category provides a list of `NodeTemplateViewModel` items with live previews.

## Refreshing layouts

LogicLab nodes with dynamic pin counts (bus split/merge) use `LogicNodeFactory.RefreshNodeLayout` to update pin positions and node height as pins change.
