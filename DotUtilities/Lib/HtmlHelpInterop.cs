#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (HtmlHelpInterop.cs) is part of Oetools.Utilities.
//
// Oetools.Utilities is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Oetools.Utilities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Oetools.Utilities. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Oetools.Utilities.Lib {

    /// <summary>
    /// A utility class to interact with .htm help file on windows.
    /// </summary>
    public static class HtmlHelpInterop {

        [Flags]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private enum HTMLHelpCommand : uint {
            HH_DISPLAY_TOPIC = 0,
            HH_DISPLAY_TOC = 1,
            HH_DISPLAY_INDEX = 2,
            HH_DISPLAY_SEARCH = 3,
            HH_DISPLAY_TEXT_POPUP = 0x000E,
            HH_HELP_CONTEXT = 0x000F,
            HH_CLOSE_ALL = 0x0012
        }

        [DllImport("hhctrl.ocx", SetLastError=true, EntryPoint = "HtmlHelpW", CharSet = CharSet.Unicode)]
        private static extern IntPtr HtmlHelp(IntPtr hWndCaller, [MarshalAs(UnmanagedType.LPWStr)] string helpFile, HTMLHelpCommand command, IntPtr data);

        /// <summary>
        /// Open a windows .htm file at the given index.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static IntPtr DisplayIndex(string file, string index) {
            if (!Utils.IsRuntimeWindowsPlatform) {
                return IntPtr.Zero;
            }
            var data = Marshal.StringToHGlobalUni(index);
            var res = HtmlHelp(IntPtr.Zero, file, HTMLHelpCommand.HH_DISPLAY_INDEX, data);
            Marshal.FreeHGlobal(data);
            return res;
        }

    }
}
