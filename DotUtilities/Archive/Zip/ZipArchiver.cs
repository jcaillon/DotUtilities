#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ZipArchiver.cs) is part of DotUtilities.
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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DotUtilities.Extensions;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace DotUtilities.Archive.Zip {

    /// <summary>
    /// Allows to pack files into zip
    /// </summary>
    internal class ZipArchiver : ArchiverBase, IZipArchiver {

        private CompressionLevel _compressionLevel = CompressionLevel.NoCompression;

        /// <inheritdoc cref="IZipArchiver.SetCompressionLevel"/>
        public void SetCompressionLevel(ArchiveCompressionLevel archiveCompressionLevel) {
            switch (archiveCompressionLevel) {
                case ArchiveCompressionLevel.None:
                    _compressionLevel = CompressionLevel.NoCompression;
                    break;
                case ArchiveCompressionLevel.Fastest:
                    _compressionLevel = CompressionLevel.Fastest;
                    break;
                case ArchiveCompressionLevel.Optimal:
                    _compressionLevel = CompressionLevel.Optimal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(archiveCompressionLevel), archiveCompressionLevel, null);
            }
        }

        /// <inheritdoc cref="IArchiver.OnProgress"/>
        public event EventHandler<ArchiverEventArgs> OnProgress;

        /// <inheritdoc />
        public int CheckFileSet(IEnumerable<IFileInArchiveToCheck> filesToCheck) {
            int total = 0;
            foreach (var groupedFiles in filesToCheck.ToNonNullEnumerable().GroupBy(f => f.ArchivePath)) {
                try {
                    _cancelToken?.ThrowIfCancellationRequested();
                    HashSet<string> list = null;
                    if (File.Exists(groupedFiles.Key)) {
                        using (var archive = ZipFile.OpenRead(groupedFiles.Key)) {
                            list = archive.Entries.Select(f => f.FullName.ToCleanRelativePathUnix()).ToHashSet(new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                        }
                    }
                    foreach (var file in groupedFiles) {
                        if (list != null && list.Contains(file.PathInArchive.ToCleanRelativePathUnix())) {
                            file.Processed = true;
                            total++;
                        } else {
                            file.Processed = false;
                        }
                    }
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception e) {
                    throw new ArchiverException($"Failed to check files from {groupedFiles.Key}.", e);
                }
            }
            return total;
        }

        /// <inheritdoc cref="IArchiver.ArchiveFileSet"/>
        public int ArchiveFileSet(IEnumerable<IFileToArchive> filesToArchive) {
            if (filesToArchive == null) {
                return 0;
            }

            var filesToPack = filesToArchive.ToList();
            filesToPack.ForEach(f => f.Processed = false);
            int totalFiles = filesToPack.Count;
            int totalFilesDone = 0;
            foreach (var zipGroupedFiles in filesToPack.GroupBy(f => f.ArchivePath)) {
                try {
                    CreateArchiveFolder(zipGroupedFiles.Key);
                    var zipMode = File.Exists(zipGroupedFiles.Key) ? ZipArchiveMode.Update : ZipArchiveMode.Create;
                    using (var zip = ZipFile.Open(zipGroupedFiles.Key, zipMode)) {
                        foreach (var file in zipGroupedFiles) {
                            _cancelToken?.ThrowIfCancellationRequested();
                            if (!File.Exists(file.SourcePath)) {
                                continue;
                            }
                            try {
                                zip.CreateEntryFromFile(file.SourcePath, file.PathInArchive.ToCleanRelativePathUnix(), _compressionLevel);
                            } catch (Exception e) {
                                throw new ArchiverException($"Failed to pack {file.SourcePath.PrettyQuote()} into {zipGroupedFiles.Key.PrettyQuote()} and relative archive path {file.PathInArchive}.", e);
                            }
                            totalFilesDone++;
                            file.Processed = true;
                            OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(zipGroupedFiles.Key, file.PathInArchive, Math.Round(totalFilesDone / (double) totalFiles * 100, 2)));
                        }
                    }
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception e) {
                    throw new ArchiverException($"Failed to pack to {zipGroupedFiles.Key.PrettyQuote()}.", e);
                }
            }
            return totalFilesDone;
        }

        /// <inheritdoc cref="IArchiverList.ListFiles"/>
        public IEnumerable<IFileInArchive> ListFiles(string archivePath) {
            if (!File.Exists(archivePath)) {
                return Enumerable.Empty<IFileInArchive>();
            }
            using (var archive = ZipFile.OpenRead(archivePath)) {
                return archive.Entries
                    .Select(entry => new FileInZip {
                        PathInArchive = entry.FullName.ToCleanRelativePathUnix(),
                        SizeInBytes = (ulong) entry.Length,
                        LastWriteTime = entry.LastWriteTime.DateTime,
                        ArchivePath = archivePath
                    } as IFileInArchive);
            }
        }

        /// <inheritdoc cref="IArchiverExtract.ExtractFileSet"/>
        public int ExtractFileSet(IEnumerable<IFileInArchiveToExtract> filesToExtractIn) {
            if (filesToExtractIn == null) {
                return 0;
            }

            var filesToExtract = filesToExtractIn.ToList();
            filesToExtract.ForEach(f => f.Processed = false);
            int totalFiles = filesToExtract.Count;
            int totalFilesDone = 0;
            foreach (var zipGroupedFiles in filesToExtract.GroupBy(f => f.ArchivePath)) {
                if (!File.Exists(zipGroupedFiles.Key)) {
                    continue;
                }
                try {
                    // create all necessary extraction folders
                    foreach (var extractDirGroupedFiles in zipGroupedFiles.GroupBy(f => Path.GetDirectoryName(f.ExtractionPath))) {
                        if (!Directory.Exists(extractDirGroupedFiles.Key)) {
                            Directory.CreateDirectory(extractDirGroupedFiles.Key);
                        }
                    }
                    using (var zip = ZipFile.OpenRead(zipGroupedFiles.Key)) {
                        foreach (var entry in zip.Entries) {
                            _cancelToken?.ThrowIfCancellationRequested();
                            var fileToExtract = zipGroupedFiles.FirstOrDefault(f => entry.FullName.ToCleanRelativePathUnix().EqualsCi(f.PathInArchive.ToCleanRelativePathUnix()));
                            if (fileToExtract != null) {
                                try {
                                    entry.ExtractToFile(fileToExtract.ExtractionPath, true);
                                } catch (Exception e) {
                                    throw new ArchiverException($"Failed to extract {fileToExtract.ExtractionPath.PrettyQuote()} from {zipGroupedFiles.Key.PrettyQuote()} and relative archive path {fileToExtract.PathInArchive}.", e);
                                }
                                totalFilesDone++;
                                fileToExtract.Processed = true;
                                OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(zipGroupedFiles.Key, fileToExtract.PathInArchive, Math.Round(totalFilesDone / (double) totalFiles * 100, 2)));
                            }
                        }
                    }
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception e) {
                    throw new ArchiverException($"Failed to extract files from {zipGroupedFiles.Key.PrettyQuote()}.", e);
                }
            }

            return totalFilesDone;
        }

        /// <inheritdoc cref="IArchiverDelete.DeleteFileSet"/>
        public int DeleteFileSet(IEnumerable<IFileInArchiveToDelete> filesToDeleteIn) {
            if (filesToDeleteIn == null) {
                return 0;
            }

            var filesToDelete = filesToDeleteIn.ToList();
            filesToDelete.ForEach(f => f.Processed = false);
            int totalFiles = filesToDelete.Count;
            int totalFilesDone = 0;
            foreach (var zipGroupedFiles in filesToDelete.GroupBy(f => f.ArchivePath)) {
                if (!File.Exists(zipGroupedFiles.Key)) {
                    continue;
                }
                try {
                    using (var zip = ZipFile.Open(zipGroupedFiles.Key, ZipArchiveMode.Update)) {
                        foreach (var entry in zip.Entries.ToList()) {
                            _cancelToken?.ThrowIfCancellationRequested();
                            var fileToDelete = zipGroupedFiles.FirstOrDefault(f => entry.FullName.ToCleanRelativePathUnix().EqualsCi(f.PathInArchive.ToCleanRelativePathUnix()));
                            if (fileToDelete != null) {
                                try {
                                    entry.Delete();
                                } catch (Exception e) {
                                    throw new ArchiverException($"Failed to delete {fileToDelete.PathInArchive.PrettyQuote()} from {zipGroupedFiles.Key.PrettyQuote()}.", e);
                                }
                                totalFilesDone++;
                                fileToDelete.Processed = true;
                                OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(zipGroupedFiles.Key, fileToDelete.PathInArchive, Math.Round(totalFilesDone / (double) totalFiles * 100, 2)));
                            }
                        }
                    }
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception e) {
                    throw new ArchiverException($"Failed to delete files from {zipGroupedFiles.Key.PrettyQuote()}.", e);
                }
            }

            return totalFilesDone;
        }

        /// <inheritdoc cref="IArchiverMove.MoveFileSet"/>
        public int MoveFileSet(IEnumerable<IFileInArchiveToMove> filesToMoveIn) {
            if (filesToMoveIn == null) {
                return 0;
            }

            var filesToMove = filesToMoveIn.ToList();
            filesToMove.ForEach(f => f.Processed = false);
            int totalFiles = filesToMove.Count;
            int totalFilesDone = 0;

            foreach (var zipGroupedFiles in filesToMove.GroupBy(f => f.ArchivePath)) {
                if (!File.Exists(zipGroupedFiles.Key)) {
                    continue;
                }
                var tempPath = Path.Combine(Path.GetDirectoryName(zipGroupedFiles.Key) ?? Path.GetTempPath(), $"~{Path.GetRandomFileName()}");
                Utils.CreateDirectoryIfNeeded(tempPath, FileAttributes.Hidden);
                try {
                    using (var zip = ZipFile.Open(zipGroupedFiles.Key, ZipArchiveMode.Update)) {
                        foreach (var entry in zip.Entries.ToList()) {
                            _cancelToken?.ThrowIfCancellationRequested();
                            var fileToMove = zipGroupedFiles.FirstOrDefault(f => entry.FullName.ToCleanRelativePathUnix().EqualsCi(f.PathInArchive.ToCleanRelativePathUnix()));
                            if (fileToMove != null) {
                                try {
                                    var exportPath = Path.Combine(tempPath, "temp");
                                    entry.ExtractToFile(exportPath);
                                    entry.Delete();
                                    zip.CreateEntryFromFile(exportPath, fileToMove.NewRelativePathInArchive, _compressionLevel);
                                    File.Delete(exportPath);
                                } catch (Exception e) {
                                    throw new ArchiverException($"Failed to move {fileToMove.PathInArchive.PrettyQuote()} to {fileToMove.NewRelativePathInArchive.PrettyQuote()} in {zipGroupedFiles.Key.PrettyQuote()}.", e);
                                }
                                totalFilesDone++;
                                fileToMove.Processed = true;
                                OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(zipGroupedFiles.Key, fileToMove.PathInArchive, Math.Round(totalFilesDone / (double) totalFiles * 100, 2)));
                            }
                        }
                    }
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception e) {
                    throw new ArchiverException($"Failed to move files from {zipGroupedFiles.Key.PrettyQuote()}.", e);
                } finally {
                    Directory.Delete(tempPath, true);
                }
            }

            return totalFilesDone;
        }
    }
}
