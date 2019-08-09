#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (Archiver.cs) is part of Oetools.Utilities.
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

using System.Runtime.CompilerServices;
using Oetools.Utilities.Archive;
using Oetools.Utilities.Archive.Filesystem;
using Oetools.Utilities.Archive.Ftp;
using Oetools.Utilities.Archive.HttpFileServer;
using Oetools.Utilities.Archive.Zip;

[assembly: InternalsVisibleTo("DotUtilities.Test")]

namespace DotUtilities.Archive {

    /// <summary>
    /// An archiver allows CRUD operation on an archive.
    /// </summary>
    public static class Archiver {

        /// <summary>
        /// Get a new instance of an archiver.
        /// </summary>
        /// <returns></returns>
        public static IZipArchiver NewZipArchiver() => new ZipArchiver();

        /// <summary>
        /// Get a new instance of an archiver.
        /// </summary>
        /// <returns></returns>
        public static IArchiverFullFeatured NewFtpArchiver() => new FtpArchiver();

        /// <summary>
        /// Get a new instance of an archiver.
        /// </summary>
        /// <returns></returns>
        public static IArchiverFullFeatured NewFileSystemArchiver() => new FileSystemArchiver();

        /// <summary>
        /// Get a new instance of an archiver.
        /// </summary>
        /// <returns></returns>
        public static IHttpFileServerArchiver NewHttpFileServerArchiver() => new HttpFileServerArchiver();

    }
}
