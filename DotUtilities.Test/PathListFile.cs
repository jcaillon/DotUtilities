#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (U.cs) is part of DotUtilities.Test.
//
// DotUtilities.Test is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// DotUtilities.Test is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with DotUtilities.Test. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using Oetools.Utilities.Lib;

namespace Oetools.Utilities.Test {
    /// <summary>
    ///     This class represents a file that needs to be compiled
    /// </summary>
    public class PathListFile : IPathListItem {

        /// <summary>
        /// The path to the source file
        /// </summary>
        public string Path { get; set; }

        public PathListFile(string sourceFilePath) {
            Path = sourceFilePath;
        }
    }
}
