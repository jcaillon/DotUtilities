#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (IArchiverList.cs) is part of DotUtilities.
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

using System.Collections.Generic;

namespace DotUtilities.Archive {

    /// <summary>
    /// An archiver that can list the files in the archive.
    /// </summary>
    public interface IArchiverList : IArchiver {

        /// <summary>
        /// List all the files in an archive. Returns an empty enumerable if the archive does not exist.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <returns></returns>
        /// <exception cref="ArchiverException"></exception>
        IEnumerable<IFileInArchive> ListFiles(string archivePath);
    }
}
