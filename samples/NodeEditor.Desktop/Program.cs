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
using System;
using System.Runtime.InteropServices;
using Avalonia;

namespace NodeEditorDemo;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    static Program()
    {
        App.EnableInputOutput = true;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
        {
            App.EnableMainMenu = true;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            App.EnableMainMenu = true;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            App.EnableMainMenu = false;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            App.EnableMainMenu = false;
        }
        else
        {
            App.EnableMainMenu = true;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseSkia();
}
