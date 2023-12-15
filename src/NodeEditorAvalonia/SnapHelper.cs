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

namespace NodeEditor;

internal static class SnapHelper
{
    public static double Snap(double value, double snap)
    {
        if (snap == 0.0)
        {
            return value;
        }
        var c = value % snap;
        var r = c >= snap / 2.0 ? value + snap - c : value - c;
        return r;
    }

    public static Point Snap(Point point, double snapX, double snapY, bool enabled)
    {
        if (enabled)
        {
            var pointX = Snap(point.X, snapX);
            var pointY = Snap(point.Y, snapY);
            return new Point(pointX, pointY);
        }

        return point;
    }
}
