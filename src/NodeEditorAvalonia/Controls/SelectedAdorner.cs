/*
 * NodeEditor A node editor control for Avalonia.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace NodeEditor.Controls;

public class SelectedAdorner : Control
{
    public static readonly StyledProperty<Rect> RectProperty =
        AvaloniaProperty.Register<SelectedAdorner, Rect>(nameof(Rect));

    public Rect Rect
    {
        get => GetValue(RectProperty);
        set => SetValue(RectProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
#pragma warning disable 8631
        base.OnPropertyChanged(change);
#pragma warning restore 8631

        if (change.Property == RectProperty)
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var thickness = 2.0;
        var pen = new ImmutablePen(
            new ImmutableSolidColorBrush(new Color(0xFF, 0x17, 0x9D, 0xE3)), 
            thickness);
        var bounds = Rect;
        var rect = bounds.Deflate(thickness * 0.5);
        context.DrawRectangle(null, pen, rect);
    }
}
