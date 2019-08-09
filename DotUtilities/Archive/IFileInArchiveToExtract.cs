﻿#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (IFileInArchiveToExtract.cs) is part of DotUtilities.
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

namespace DotUtilities.Archive {

    /// <summary>
    /// Represents a file in an archive that needs to be extracted from it.
    /// </summary>
    public interface IFileInArchiveToExtract : IFileArchivedBase {

        /// <summary>
        /// The path to which this file should be extracted from the archive.
        /// </summary>
        string ExtractionPath { get; }
    }
}
