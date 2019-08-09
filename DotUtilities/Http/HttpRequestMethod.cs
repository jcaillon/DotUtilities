#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (HttpRequestMethod.cs) is part of DotUtilities.
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

namespace DotUtilities.Http {

    /// <summary>
    /// Represents an http method.
    /// </summary>
    public enum HttpRequestMethod {
        /// <summary>
        /// Post
        /// </summary>
        Post,
        /// <summary>
        /// Get
        /// </summary>
        Get,
        /// <summary>
        /// Head
        /// </summary>
        Head,
        /// <summary>
        /// Put
        /// </summary>
        Put,
        /// <summary>
        /// Delete
        /// </summary>
        Delete,
        /// <summary>
        /// Connect
        /// </summary>
        Connect,
        /// <summary>
        /// Options
        /// </summary>
        Options,
        /// <summary>
        /// Trace
        /// </summary>
        Trace,
        /// <summary>
        /// Patch
        /// </summary>
        Patch
    }
}
