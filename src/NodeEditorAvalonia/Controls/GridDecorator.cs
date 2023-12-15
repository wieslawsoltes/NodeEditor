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

public class GridDecorator : Decorator
{
    public static readonly StyledProperty<bool> EnableGridProperty = 
        AvaloniaProperty.Register<GridDecorator, bool>(nameof(EnableGrid));

    public static readonly StyledProperty<double> GridCellWidthProperty = 
        AvaloniaProperty.Register<GridDecorator, double>(nameof(GridCellWidth));

    public static readonly StyledProperty<double> GridCellHeightProperty = 
        AvaloniaProperty.Register<GridDecorator, double>(nameof(GridCellHeight));

    public bool EnableGrid
    {
        get => GetValue(EnableGridProperty);
        set => SetValue(EnableGridProperty, value);
    }

    public double GridCellWidth
    {
        get => GetValue(GridCellWidthProperty);
        set => SetValue(GridCellWidthProperty, value);
    }

    public double GridCellHeight
    {
        get => GetValue(GridCellHeightProperty);
        set => SetValue(GridCellHeightProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EnableGridProperty
            || change.Property == GridCellWidthProperty
            || change.Property == GridCellHeightProperty)
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!EnableGrid)
        {
            return;
        }

        var cw = GridCellWidth;
        var ch = GridCellHeight;
        if (cw <= 0 || ch <= 0.0)
        {
            return;
        }
        
        var rect = Bounds;
        var thickness = 1.0;

        var brush = new ImmutableSolidColorBrush(Color.FromArgb(255, 222, 222, 222));
        var pen = new ImmutablePen(brush, thickness);

        using var _ = context.PushTransform(Matrix.CreateTranslation(-0.5d, -0.5d));

        var ox = rect.X;
        var ex = rect.X + rect.Width;
        var oy = rect.Y;
        var ey = rect.Y + rect.Height;
        for (var x = ox + cw; x < ex; x += cw)
        {
            var p0 = new Point(x + 0.5, oy + 0.5);
            var p1 = new Point(x + 0.5, ey + 0.5);
            context.DrawLine(pen, p0, p1);
        }

        for (var y = oy + ch; y < ey; y += ch)
        {
            var p0 = new Point(ox + 0.5, y + 0.5);
            var p1 = new Point(ex + 0.5, y + 0.5);
            context.DrawLine(pen, p0, p1);
        }

        context.DrawRectangle(null, pen, rect);
    }
}
