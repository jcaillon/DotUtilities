#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (TimeExtensions.cs) is part of DotUtilities.
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

namespace DotUtilities.Extensions {

    /// <summary>
    /// A collection of extensions for time.
    /// </summary>
    public static class TimeExtensions {

        /// <summary>
        /// Get the time elapsed in a human readable format
        /// </summary>
        public static string ConvertToHumanTime(this TimeSpan tn) {
            if (tn.Hours > 0)
                return $"{tn.Hours:D2}h:{tn.Minutes:D2}m:{tn.Seconds:D2}s";
            if (tn.Minutes > 0)
                return $"{tn.Minutes:D2}m:{tn.Seconds:D2}s";
            if (tn.Seconds > 0)
                return $"{tn.Seconds:D2}s";
            return $"{tn.Milliseconds:D3}ms";
        }
    }
}
