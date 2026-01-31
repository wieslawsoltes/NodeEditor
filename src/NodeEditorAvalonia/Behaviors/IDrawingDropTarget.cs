using System.Collections.Generic;
using Avalonia;
using Avalonia.Platform.Storage;

namespace NodeEditor.Behaviors;

public interface IDrawingDropTarget
{
    bool CanDropText(string text, Point point);
    void DropText(string text, Point point);
    bool CanDropFiles(IReadOnlyList<IStorageItem> files, Point point);
    void DropFiles(IReadOnlyList<IStorageItem> files, Point point);
}
