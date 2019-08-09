#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ILog.cs) is part of Oetools.Utilities.
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

namespace Oetools.Utilities.Lib {

    /// <summary>
    /// A simple interface to log messages.
    /// </summary>
    public interface ILog {

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        void Error(string message, Exception e = null);

        /// <summary>
        /// Log a warning.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        void Warn(string message, Exception e = null);

        /// <summary>
        /// Log an information.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        void Info(string message, Exception e = null);

        /// <summary>
        /// Log a debug message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        void Debug(string message, Exception e = null);
    }
}
