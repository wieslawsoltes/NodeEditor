# Wire zip-tie browser sample

This sample runs the actual Avalonia `NodeEditor` controls and .NET model in the
browser. It starts with six independently selectable, colored wires bundled
between two movable socket panels.

```sh
dotnet workload install wasm-tools
dotnet run --project samples/NodeEditor.Web/NodeEditor.Web.csproj -c Release
```

Open the URL printed by the development server (normally
`http://localhost:5235`). The first build restores and compiles WebAssembly native
dependencies.

## Try the tools

* Drag a collar to reshape the bundle; move either socket panel to verify that
  the fan-in and fan-out follow their pins.
* Select wires, then choose **Create tie**. **Dissolve** removes a bundle while
  keeping the underlying connectors intact.
* **Attach selected** and **Detach selected** edit bundle membership. Lane order
  stays stable until **Reverse lanes** is chosen.
* **Add bend** inserts an intermediate collar. **Straighten** removes intermediate
  collars. Change **Lane gap** to adjust spacing.
* Switch between **Bezier**, **Straight**, and **Orthogonal** to exercise all
  connector styles.
* Use **Undo** and **Redo** for edits, including whole collar drags.
* **Save snapshot** serializes the complete drawing using the same JSON
  serializer as the desktop sample. Make edits, then **Restore snapshot** to
  verify that membership and connector references survive a round trip. This
  snapshot remains in memory for the current page session.

The reusable sample view and presentation model live in `NodeEditor.Base`; the
browser project only supplies the WebAssembly host and application composition.

## Publish

```sh
dotnet publish samples/NodeEditor.Web/NodeEditor.Web.csproj -c Release -o artifacts/browser
python3 -m http.server 5235 --directory artifacts/browser/wwwroot
```

Deploy the contents of `artifacts/browser/wwwroot` to a static web host. The CI
workflow builds the browser sample and uploads this directory as the
`nodeeditor-browser-sample` artifact for every pull request. The host must serve
`.wasm` files with the `application/wasm` MIME type.
