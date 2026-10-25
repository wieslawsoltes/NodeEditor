# Wire zip ties

Zip ties gather independent wires into parallel lanes between movable collars.
Wires retain their endpoints, identity, color, connection rules, and individual
routing configuration. Dissolving a bundle restores ordinary wire routing.

Each bundle contains at least two wires, two end collars, and any number of
intermediate collars. A wire belongs to one continuous bundle; use intermediate
collars to route that bundle through several bends. Bundles are visual routing
constraints. They never create a junction, change pin connections, or combine
signals.

## Editing

The default `Editor` theme enables zip-tie input automatically for drawings
implementing `IZipTieHost`. `DrawingNodeViewModel` supplies this capability.

| Action | Input |
| --- | --- |
| Create a bundle | Select at least two unbundled wires, then Ctrl/Cmd+J or **Wire zip ties → Create from selected wires** |
| Move one collar | Drag its band or either end grip |
| Move the whole bundle | Shift-drag a collar or the shared span |
| Add an intermediate collar | Double-click the shared span |
| Remove an intermediate collar | Double-click that collar |
| Move the selected bundle precisely | Arrow keys; Shift uses the configured fast-nudge multiplier |
| Temporarily bypass grid snapping | Hold Alt during a drag |
| Cancel a drag | Escape restores the original geometry |
| Dissolve a bundle | Ctrl/Cmd+Shift+J, Delete while a bundle is active, or the **Dissolve** command |
| Attach/detach wires | Select the wires and use the corresponding zip-tie command |
| Change lane order | **Reverse lanes** reverses member order without swapping pin endpoints |
| Straighten a bundle | **Straighten** removes intermediate collars and preserves the end collars |

Ordinary clicks on wires still select individual wires. The active bundle is
retained while selecting wires, so the attach/detach commands remain available.
Delete targets the active bundle; press Escape to clear it before deleting an
individually selected wire.
Left-clicking a collar clears node and wire selection unless Ctrl/Cmd is held. A
right-click on a collar or shared span preserves that selection, activates its
bundle, and opens the standard editor menu. Text editing, embedded controls, ink
mode, and connection creation retain their own input handling.

Hit tolerance is measured in screen-independent pixels and converted to drawing
coordinates, so collars remain usable after zooming. Dragging preserves the
initial pointer-to-anchor offset. A drag is one undo transaction; a click without
movement and an Escape-cancelled drag do not create history entries. Capture
loss finishes the current drag. Detaching the behavior or changing its drawing
cancels the transaction and releases capture.

Moving one endpoint node leaves collars in place and updates the wire fan-out.
Moving all endpoint nodes of a bundle together moves the bundle with them.
Locked bundles can be selected but cannot be reshaped, dissolved, or reordered.
Hidden bundles stop constraining their wires until shown again.

## Model and MVVM APIs

The extension uses optional capabilities instead of adding requirements to
`IConnector` or `IDrawingNode`.

| API | Responsibility |
| --- | --- |
| `IZipTie` | Ordered member references, `Start`, `End`, intermediate `Waypoints`, `Spacing`, name, visibility, and locking |
| `IZipTieHost` | Drawing-owned bundle collection, active selection, membership lookup, and editing operations |
| `IZipTieCommands` | Commands for creation, dissolution, attach/detach, lane reversal, and straightening |
| `IZipTieLaneProvider` | Optional constant-time lookup of a wire's lane index |
| `ZipTieEditor` | Framework-independent eligibility, creation, and membership rules for custom hosts |
| `ZipTieViewModel` | Reactive model with nested point and collection change notifications |
| `DrawingNodeViewModel` | Default host, membership/lane indexes, command enablement, persistence, and history |

Create a bundle from existing wires in a drawing:

```csharp
using System.Collections.Generic;
using NodeEditor.Model;
using NodeEditor.Mvvm;

public static class HarnessTools
{
    public static IZipTie? Bundle(
        DrawingNodeViewModel drawing,
        IEnumerable<IConnector> wires)
    {
        drawing.BeginUndoBatch();
        try
        {
            var tie = drawing.CreateZipTie(wires);
            if (tie is null)
            {
                return null;
            }

            tie.Name = "Signal harness";
            tie.Start = new ConnectorPoint(350, 270);
            tie.End = new ConnectorPoint(670, 270);
            tie.Spacing = 7;
            return tie;
        }
        finally
        {
            drawing.EndUndoBatch();
        }
    }
}
```

As with other drawing operations, install an `INodeSerializer` through
`SetSerializer` to enable snapshot-based history. The default sample uses
`new NodeSerializer(typeof(ObservableCollection<>))`. Creation ignores duplicate,
already bundled, invisible, locked, incomplete, and foreign-drawing wires. It
returns `null` when fewer than two eligible members remain.

For a custom host, keep membership references identical to the connectors in
the drawing. Use observable collections and implement property-change
notifications for replaced collections and nested geometry. Direct external
mutation of a plain `IList` must be followed by a notification for its owning
property. The default host's editing methods handle this explicitly. An optional
`IZipTieLaneProvider` avoids scanning each bundle's member list when rendering
large harnesses.

## Geometry and rendering

Creation computes a span from the endpoint groups and chooses stable initial
lane order from projected pin positions. Projection keys are calculated once;
ordering is `O(n log n)`. The default host indexes wire ownership and lane number
for constant-time lookups. Attaching a wire appends a lane, preserving the order
of existing members.

The lane offset is centered around the bundle spine:

```text
offset = (laneIndex - (memberCount - 1) / 2) * spacing
```

Spine frames supply tangents and bounded miter offsets at each collar. Forward
and reversed wire endpoints use the appropriate traversal without exchanging
physical lanes. The Bezier route uses cubic fan-in and fan-out segments aligned
with the pin direction and the bundle tangent, followed by smooth shared-spine
segments. Parent-node rotation is included in pin direction. Straight routes use
line segments; orthogonal routes use axis-aligned segments through the lane
anchors. Degenerate spans, coincident anchors, duplicate points, and sharp
reversals have finite fallbacks.

The explicit bundle spine takes precedence over ordinary obstacle routing while
the bundle is visible. Existing per-wire `Waypoints`, `RoutingMode`, and endpoint
configuration are preserved and become active again when the bundle is dissolved
or hidden. Manual waypoint handles are suppressed while a bundle owns the wire
route.

`ConnectorPath` is the common segment representation for drawing and flattening.
Rendering, labels, arrow directions, selection overlays, crossings, and the
spatial hit-test index derive from the same route. Adaptive cubic subdivision
uses a drawing-space tolerance instead of allocating one sample per pixel of
wire length. This also keeps hit geometry aligned with rounded ordinary routes.

`ZipTieGeometry` exposes collar geometry and route queries:

```csharp
int count = ZipTieGeometry.GetAnchorCount(tie);
Point center = ZipTieGeometry.GetAnchor(tie, 0);
ZipTieGeometry.GetBandSegment(tie, 0, out Point bandStart, out Point bandEnd);
bool hit = ZipTieGeometry.TryHitTest(
    tie, pointerInDrawing, tolerance: 8, includeSpan: true, out int anchorIndex);
int insertionIndex = ZipTieGeometry.GetInsertionIndex(tie, pointerInDrawing);
```

Anchor index `0` is the first collar, indices `1..N` are intermediate collars,
and `N+1` is the final collar. A span hit has index `-1`. Collars have hit priority
over spans.
Insertion queries follow the rendered spine, including its curved and orthogonal
segments, so adding a collar selects the interval actually under the pointer.

`ZipTieLayer` draws the collars without intercepting input. Its `Stroke`,
`SelectedStroke`, `Fill`, `StrokeThickness`, and `ShowSelection` properties can be
styled. The default theme provides these dynamic brush resources:

- `ZipTieStrokeBrush`
- `ZipTieSelectedStrokeBrush`
- `ZipTieFillBrush`

Export rendering retains collars while suppressing selection outlines and drag
grips. The suppression is scoped to rendering and does not modify document
selection; it is restored even when export fails.

## Persistence and topology changes

The existing reference-preserving JSON serializer saves complete bundle geometry
and member identities. Active selection and commands are transient. Undo/redo
restores bundles and their references together with nodes and wires.

Copy, cut, paste, and duplication include a bundle when at least two of its
members are copied. Membership is filtered to the copied wires. Pasting translates
the end collars and intermediate points by the same delta as the copied nodes.
Old active-bundle selection is cleared so later editing does not accidentally
target the source drawing's bundle.

Deleting a wire or either endpoint node prunes bundle membership. A bundle with
fewer than two members is dissolved automatically, including when its geometry
was locked. This cleanup does not delete any remaining wire. Collection
normalization removes duplicate or invalid memberships, and old subscriptions
are released when collections, models, or visual controls are replaced.

## Browser sample and verification

The sample runs the same Avalonia controls, geometry, and .NET models as desktop:

```sh
dotnet workload install wasm-tools
dotnet run --project samples/NodeEditor.Web/NodeEditor.Web.csproj -c Release
```

It includes six colored wires, movable source/destination panels, all bundle
commands, lane spacing, all three route styles, history, and an in-memory JSON
snapshot round trip. Snapshot state belongs to the current page session.

Publish a static build with:

```sh
dotnet publish samples/NodeEditor.Web/NodeEditor.Web.csproj -c Release -o artifacts/browser
```

CI publishes `artifacts/browser/wwwroot` as the `nodeeditor-browser-sample`
artifact. Serve it with the correct `application/wasm` MIME type. The browser
sample deliberately retains the existing JSON serializer's metadata by disabling
trimming; this is a functional validation sample rather than a trimmed-download
benchmark.

The test project covers domain eligibility, membership, history and reference
identity, clipboard topology, degenerate and reversed geometry, exact route
selection, export cleanup, spatial-index invalidation, and real Avalonia Headless
pointer/keyboard events under translated and scaled input surfaces. Run it with:

```sh
dotnet test tests/NodeEditorAvalonia.UnitTests/NodeEditorAvalonia.UnitTests.csproj -c Release
```
