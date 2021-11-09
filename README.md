# NodeEditor

[![Build Status](https://dev.azure.com/wieslawsoltes/GitHub/_apis/build/status/wieslawsoltes.NodeEditor?branchName=main)](https://dev.azure.com/wieslawsoltes/GitHub/_build/latest?definitionId=104&branchName=main)
[![CI](https://github.com/wieslawsoltes/NodeEditor/actions/workflows/build.yml/badge.svg)](https://github.com/wieslawsoltes/NodeEditor/actions/workflows/build.yml)

[![NuGet](https://img.shields.io/nuget/v/NodeEditorAvalonia.svg)](https://www.nuget.org/packages/NodeEditorAvalonia)
[![NuGet](https://img.shields.io/nuget/dt/NodeEditorAvalonia.svg)](https://www.nuget.org/packages/NodeEditorAvalonia)

A node editor control for Avalonia.

# About

The node editor is an Avalonia control for editing and rendering nodes and connectors. Node contents can be defined from xaml. Node controls, connectors and pins can be retemplated from xaml or used with the provided default theme. The contents of the nodes are rendered based on provided view models. The default implementation of the view models is done using ReactiveUI, users can create their own view models based on core model interfaces. The node contents are resolved using the provided object type by view locator.

![NodeEditorDemo_J3cXfNsgEJ](https://user-images.githubusercontent.com/2297442/140658747-5db2e999-9bd8-4f46-964a-9ab32b6e1080.png)

## Building NodeEditor

First, clone the repository or download the latest zip.
```
git clone https://github.com/wieslawsoltes/NodeEditor.git
```

### Build on Windows using script

* [.NET Core](https://www.microsoft.com/net/download?initial-os=windows).

Open up a command-prompt and execute the commands:
```
.\build.ps1
```

### Build on Linux using script

* [.NET Core](https://www.microsoft.com/net/download?initial-os=linux).

Open up a terminal prompt and execute the commands:
```
./build.sh
```

### Build on OSX using script

* [.NET Core](https://www.microsoft.com/net/download?initial-os=macos).

Open up a terminal prompt and execute the commands:
```
./build.sh
```

## NuGet

NodeEditor is delivered as a NuGet package.

You can find the packages here [NuGet](https://www.nuget.org/packages/NodeEditorAvalonia/) and install the package like this:

`Install-Package NodeEditorAvalonia`

## Available Packages

* [NodeEditorAvalonia](https://www.nuget.org/packages/NodeEditorAvalonia) - The main package with Avalonia controls and default theme.
* [NodeEditorAvalonia.ReactiveUI](https://www.nuget.org/packages/NodeEditorAvalonia.ReactiveUI) - The ReactiveUI view models with default implementation.
* [NodeEditorAvalonia.Model](https://www.nuget.org/packages/NodeEditorAvalonia.Model) - The base interfaces used in controls and view models.
* [NodeEditorAvalonia.Serializer](https://www.nuget.org/packages/NodeEditorAvalonia.Serializer) - The serializer for ReactiveUI view models.
* [NodeEditorAvalonia.Export](https://www.nuget.org/packages/NodeEditorAvalonia.Export) - The renderers for exporting drawings as svg, pdf and png files.

### Package Sources

* https://api.nuget.org/v3/index.json
* https://www.myget.org/F/avalonia-ci/api/v2

## Resources

* [GitHub source code repository.](https://github.com/wieslawsoltes/NodeEditor)

## License

NodeEditor is licensed under the [MIT license](LICENSE.TXT).
