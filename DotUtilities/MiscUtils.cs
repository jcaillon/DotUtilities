#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (MiscUtils.cs) is part of DotUtilities.
//
// DotUtilities is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// DotUtilities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with DotUtilities. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.Net;

#if !WINDOWSONLYBUILD
using System.Runtime.InteropServices;
#endif

namespace DotUtilities {

    /// <summary>
    ///     Class that exposes utility methods
    /// </summary>
    public static partial class Utils {

        /// <summary>
        /// Returns the current hostname if it exists, or 127.0.0.1 otherwise.
        /// </summary>
        /// <returns></returns>
        public static string GetHostName() {
            try {
                var hostname = Dns.GetHostName();
                Dns.GetHostEntry(hostname);
                return hostname;
            } catch (Exception) {
                return "127.0.0.1";
            }
        }

#if !WINDOWSONLYBUILD
        private static bool? _isRuntimeWindowsPlatform;
#endif

        /// <summary>
        /// Returns true if the current execution is done on windows platform
        /// </summary>
        public static bool IsRuntimeWindowsPlatform {
            get {
#if WINDOWSONLYBUILD
                return true;
#else
                return (_isRuntimeWindowsPlatform ?? (_isRuntimeWindowsPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows))).Value;
#endif
            }
        }

        /// <summary>
        /// Returns true if the current executable targets .net framework (false if .net core)
        /// </summary>
        public static bool IsNetFrameworkBuild {
            get {
#if WINDOWSONLYBUILD
                return true;
#else
                return false;
#endif
            }
        }

    }
}
