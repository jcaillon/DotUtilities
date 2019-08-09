#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ArchiverEventArgs.cs) is part of DotUtilities.
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

namespace DotUtilities.Archive {

    /// <summary>
    /// an event occuring during the archiving process, giving info about the progression.
    /// </summary>
    public class ArchiverEventArgs : IArchiverEventArgs {

        /// <inheritdoc cref="IArchiverEventArgs.ArchivePath"/>
        public string ArchivePath { get; private set; }

        /// <inheritdoc cref="IArchiverEventArgs.RelativePathInArchive"/>
        public string RelativePathInArchive { get; private set; }

        /// <inheritdoc cref="IArchiverEventArgs.PercentageDone"/>
        public double PercentageDone { get; private set; }

        /// <summary>
        /// Get a new instance of <see cref="ArchiverEventArgs"/>.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="currentRelativePathInArchive"></param>
        /// <param name="percentageDone"></param>
        /// <returns></returns>
        public static ArchiverEventArgs NewProgress(string archivePath, string currentRelativePathInArchive, double percentageDone) {
            return new ArchiverEventArgs {
                ArchivePath = archivePath,
                PercentageDone = percentageDone,
                RelativePathInArchive = currentRelativePathInArchive
            };
        }
    }

}
