namespace NodeEditor.Model;

public interface IUndoRedoHost
{
    bool CanUndo { get; }
    bool CanRedo { get; }
    void Undo();
    void Redo();
    void BeginUndoBatch();
    void EndUndoBatch();
}
