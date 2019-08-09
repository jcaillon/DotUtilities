#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ArchiverType.cs) is part of Oetools.Utilities.
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

namespace Oetools.Utilities.Archive {
    
    /// <summary>
    /// The different type of archiver.
    /// </summary>
    public enum ArchiverType {
        
        /// <summary>
        /// CRUD operations for windows cabinet file format.
        /// </summary>
        Cab,
        
        /// <summary>
        /// CRUD operations for zip files.
        /// </summary>
        Zip,
        
        /// <summary>
        /// CRUD operations for pro-libraries files.
        /// </summary>
        Prolib,
        
        /// <summary>
        /// CRUD operations for an ftp server.
        /// </summary>
        Ftp,
        
        /// <summary>
        /// CRUD operations for a file system.
        /// </summary>
        FileSystem,
        
        /// <summary>
        /// CRUD operations for an HTTP file server.
        /// </summary>
        HttpFileServer
    }
}