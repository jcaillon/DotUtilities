#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (IArchiver.cs) is part of Oetools.Utilities.
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
using System.Threading;

namespace Oetools.Utilities.Archive {

    /// <summary>
    /// <para>
    /// An archiver allows CRUD operation on an archive.
    /// An archive is simply a container for files, it can take many forms : a zip, a .cab, an ftp server and so on...
    /// </para>
    /// </summary>
    public interface IArchiver {

        /// <summary>
        /// Sets a cancellation token that can be used to interrupt the process if needed.
        /// </summary>
        void SetCancellationToken(CancellationToken? cancelToken);
        
        /// <summary>
        /// <para>
        /// Event published when the archiving process is progressing.
        /// </para>
        /// </summary>
        event EventHandler<ArchiverEventArgs> OnProgress;
        
        /// <summary>
        /// <para>
        /// Archives (i.e. add or replace) files into archives.
        /// Non existing source files will not throw an exception.
        /// You can inspect which files are processed with the <see cref="IFileArchivedBase.Processed"/> property.
        /// Packing into an existing archive will update it.
        /// Packing existing files will update them.
        /// </para>
        /// </summary>
        /// <param name="filesToArchive"></param>
        /// <exception cref="ArchiverException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns>Total number of files actually packed.</returns>
        int ArchiveFileSet(IEnumerable<IFileToArchive> filesToArchive);
    }

}