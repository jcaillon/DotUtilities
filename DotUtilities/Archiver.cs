#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (Archiver.cs) is part of DotUtilities.
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

using System.Runtime.CompilerServices;
using DotUtilities.Archive;
using DotUtilities.Archive.Filesystem;
using DotUtilities.Archive.Ftp;
using DotUtilities.Archive.HttpFileServer;
using DotUtilities.Archive.Zip;

[assembly: InternalsVisibleTo("DotUtilities.Test")]

namespace DotUtilities {

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
