#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ArchiverBase.cs) is part of DotUtilities.
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
using System.IO;
using System.Threading;

namespace DotUtilities.Archive {

    /// <summary>
    /// Base archiver class.
    /// </summary>
    public abstract class ArchiverBase {

        /// <summary>
        /// Cancel token.
        /// </summary>
        protected CancellationToken? _cancelToken;

        /// <inheritdoc cref="IArchiver.SetCancellationToken"/>
        public virtual void SetCancellationToken(CancellationToken? cancelToken) {
            _cancelToken = cancelToken;
        }

        /// <summary>
        /// Creates the folder so that the given archive file can be created, returns the folder path.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <exception cref="Exception"></exception>
        protected string CreateArchiveFolder(string archivePath) {
            var archiveFolder = Path.GetDirectoryName(archivePath);
            if (!string.IsNullOrEmpty(archiveFolder) && !Directory.Exists(archiveFolder)) {
                Directory.CreateDirectory(archiveFolder);
            }
            return archiveFolder;
        }
    }
}
