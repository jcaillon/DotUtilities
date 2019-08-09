#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (IArchiveProgressionEventArgs.cs) is part of Oetools.Utilities.
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
    /// Sent through the <see cref="IArchiver.OnProgress"/> event.
    /// </summary>
    public interface IArchiverEventArgs {
        
        /// <summary>
        /// The path of the archive file concerned by this event.
        /// </summary>
        string ArchivePath { get; }

        /// <summary>
        /// The relative path, within the archive file, concerned by this event.
        /// </summary>
        string RelativePathInArchive { get; }

        /// <summary>
        /// The total percentage already done for the current process, from 0 to 100.
        /// </summary>
        /// <remarks>
        /// This is a TOTAL percentage for the current process, it is not a number for a single file or a single cabinet file.
        /// </remarks>
        double PercentageDone { get; }
        
    }
}