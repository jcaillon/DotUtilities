#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProcessIoNoWait.cs) is part of DotUtilities.
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
using DotUtilities.Process;

namespace DotUtilities {

    /// <summary>
    /// Wrapper for async process.
    /// </summary>
    public class ProcessIoNoWait : ProcessIo, IDisposable {

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="executablePath"></param>
        public ProcessIoNoWait(string executablePath) : base(executablePath) { }

        /// <summary>
        /// Start the process but does not wait for its ending.
        /// Wait for the end with <see cref="WaitForExit"/> or use the <see cref="ProcessIo.OnProcessExit"/> event to know when the process is done.
        /// </summary>
        public void ExecuteNoWait(ProcessArgs arguments = null, bool silent = true) {
            ExecuteNoWaitInternal(arguments, silent);
        }

        /// <inheritdoc cref="ProcessIo.WaitForExitInternal"/>
        public bool WaitForExit(int timeoutMs = 0) {
            return WaitForExitInternal(timeoutMs);
        }

        /// <inheritdoc />
        public void Dispose() {
            _process?.Close();
            _process?.Dispose();
            _process = null;
        }
    }
}
