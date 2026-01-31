# Commands and Undo/Redo

`IDrawingNode` exposes a command surface so UI layers can bind to editing actions.

## Built-in commands

Commands include:

- Clipboard: `CutNodesCommand`, `CopyNodesCommand`, `PasteNodesCommand`, `DuplicateNodesCommand`.
- Selection: `SelectAllNodesCommand`, `DeselectAllNodesCommand`.
- Edit: `DeleteNodesCommand`, `AlignNodesCommand`, `DistributeNodesCommand`, `OrderNodesCommand`.
- Visibility: `HideSelectionCommand`, `ShowSelectionCommand`, `ShowAllCommand`.
- Locking: `LockSelectionCommand`, `UnlockSelectionCommand`.
- Ink: `DrawInkCommand`, `ConvertInkCommand`, `AddPenCommand`, `ClearInkCommand`.
- History: `UndoCommand`, `RedoCommand`.

The MVVM layer wires these using `RelayCommand`.

## Undo/redo snapshots

`DrawingNodeViewModel` implements `IUndoRedoHost` using serialized snapshots:

- Snapshots are captured via `INodeSerializer`.
- Undo history is capped (`MaxUndoEntries`).
- Commands are wrapped in undo batches for atomic changes.

If no serializer is set, undo/redo is disabled and commands fall back to direct edits.

## Undo batching

The UI behaviors call `BeginUndoBatch` / `EndUndoBatch` when dragging or resizing nodes so an entire gesture becomes a single undo step.

## Example: custom action with undo

```csharp
if (drawing is IUndoRedoHost host)
{
    host.BeginUndoBatch();
    try
    {
        // mutate drawing
    }
    finally
    {
        host.EndUndoBatch();
    }
}
```
