# Samples

This repository includes several sample apps that show different usage patterns.

## NodeEditor.Desktop

Desktop host for the base editor sample (controls, theming, and view locator setup).

```bash
dotnet run --project samples/NodeEditor.Desktop/NodeEditor.Desktop.csproj -c Release
```

## NodeEditor.Logic.Desktop

LogicLab is a digital logic editor/simulator built on top of the NodeEditor control.

```bash
dotnet run --project samples/NodeEditor.Logic.Desktop/NodeEditor.Logic.Desktop.csproj -c Release
```

The `samples/NodeEditor.Base` and `samples/NodeEditor.Logic` projects contain the shared assets and view models used by the desktop hosts.
