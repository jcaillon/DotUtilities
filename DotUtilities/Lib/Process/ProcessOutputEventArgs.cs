#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProcessOutput.cs) is part of Oetools.Utilities.
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
    /// Event args for process output (write on standard/error stream).
    /// </summary>
    public class ProcessOutputEventArgs : EventArgs {

        /// <summary>
        /// The data string written in the output stream.
        /// </summary>
        public string Data { get; }

        /// <summary>
        /// Is the data coming from the error output stream? (standard stream otherwise).
        /// </summary>
        public bool FromErrorOutput { get; }

        internal ProcessOutputEventArgs(string data, bool fromErrorOutput) {
            Data = data;
            FromErrorOutput = fromErrorOutput;
        }
    }
}
