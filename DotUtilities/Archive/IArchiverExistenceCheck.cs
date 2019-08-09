#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (IArchiverMove.cs) is part of Oetools.Utilities.
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
    /// An archiver that can check the existence of a file in the archive.
    /// </summary>
    public interface IArchiverExistenceCheck : IArchiver {

        /// <summary>
        /// <para>
        /// Check the existence of the given files within archives.
        /// You can inspect which files actually exist with the <see cref="IFileArchivedBase.Processed"/> property.
        /// </para>
        /// </summary>
        /// <param name="filesToCheck"></param>
        /// <exception cref="ArchiverException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns>Total number of files actually existing.</returns>
        int CheckFileSet(IEnumerable<IFileInArchiveToCheck> filesToCheck);
    }
}
