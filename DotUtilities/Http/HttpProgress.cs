#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (HttpProgress.cs) is part of DotUtilities.
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
    /// The progression of a request.
    /// </summary>
    public class HttpProgress {

        /// <summary>
        /// Is it an upstream request (upload).
        /// </summary>
        public bool IsUpStream { get; }

        /// <summary>
        /// Total nb of bytes that needs to be exchanged.
        /// </summary>
        public long NumberOfBytesTotal { get; }

        /// <summary>
        /// The nb of bytes already exchanged.
        /// </summary>
        public long NumberOfBytesDoneTotal { get; }

        /// <summary>
        /// The nb of bytes exchanged since the last progression event.
        /// </summary>
        public long NumberOfBytesDoneSinceLastProgress { get; }

        internal HttpProgress(bool isUpStream, long numberOfBytesTotal, long numberOfBytesDoneTotal, long numberOfBytesDoneSinceLastProgress) {
            IsUpStream = isUpStream;
            NumberOfBytesTotal = numberOfBytesTotal;
            NumberOfBytesDoneTotal = numberOfBytesDoneTotal;
            NumberOfBytesDoneSinceLastProgress = numberOfBytesDoneSinceLastProgress;
        }
    }
}
