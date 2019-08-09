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

using System.Collections.Generic;

namespace Oetools.Utilities.Archive.HttpFileServer {

    /// <summary>
    /// <para>
    /// An archiver allows CRUD operation on a http file server.
    /// </para>
    /// </summary>
    public interface IHttpFileServerArchiver : IArchiverBasic {

        /// <summary>
        /// Use an http proxy for all the http requests of this archiver.
        /// </summary>
        /// <param name="proxyUrl">The url of the proxy. Format http://{host}:{port}/</param>
        /// <param name="userName">Can be null. Format domain\username.</param>
        /// <param name="userPassword"></param>
        void SetProxy(string proxyUrl, string userName = null, string userPassword = null);
        
        /// <summary>
        /// Use basic authentication for all the http requests of this archiver.
        /// </summary>
        /// <param name="userName">Format domain\username.</param>
        /// <param name="userPassword"></param>
        void SetBasicAuthentication(string userName, string userPassword);
        
        /// <summary>
        /// Use custom headers for all the http requests of this archiver.
        /// </summary>
        /// <param name="headersKeyValue"></param>
        void SetHeaders(Dictionary<string, string> headersKeyValue);
        
    }

}