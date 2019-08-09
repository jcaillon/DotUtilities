#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (IArchiverDelete.cs) is part of Oetools.Utilities.
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
using System.Collections.Generic;

namespace Oetools.Utilities.Archive {

    /// <summary>
    /// An archiver that can operate a delete.
    /// </summary>
    public interface IArchiverDelete : IArchiver {

        /// <summary>
        /// Deletes the given files from archives.
        /// Requesting the deletion from a non existing archive will not throw an exception.
        /// Requesting the deletion a file that does not exist in the archive will not throw an exception.
        /// You can inspect which files are processed with the <see cref="IFileArchivedBase.Processed"/> property.
        /// </summary>
        /// <param name="filesToDelete"></param>
        /// <exception cref="ArchiverException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns>Total number of files actually deleted.</returns>
        int DeleteFileSet(IEnumerable<IFileInArchiveToDelete> filesToDelete);
    }
}
