#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (IFileInArchiveToMove.cs) is part of DotUtilities.
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
    /// Describes a file existing in an archive that needs to be deleted.
    /// </summary>
    public interface IFileInArchiveToMove : IFileArchivedBase {

        /// <summary>
        /// <para>
        /// The new relative path of the file within the archive.
        /// This is the destination path for a file to move.
        /// </para>
        /// </summary>
        string NewRelativePathInArchive { get; }
    }
}
