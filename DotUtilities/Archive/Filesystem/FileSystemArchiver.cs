#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileSystemArchiver.cs) is part of Oetools.Utilities.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Oetools.Utilities.Lib;
using Oetools.Utilities.Lib.Extension;

namespace Oetools.Utilities.Archive.Filesystem {
    
    /// <summary>
    /// In that case, a folder on the file system represents an archive.
    /// </summary>
    internal class FileSystemArchiver : ArchiverBase, IArchiverFullFeatured {
        
        private const int BufferSize = 1024 * 1024;

        /// <inheritdoc cref="IArchiver.OnProgress"/>
        public event EventHandler<ArchiverEventArgs> OnProgress;
        
        /// <inheritdoc cref="IArchiver.ArchiveFileSet"/>
        public int ArchiveFileSet(IEnumerable<IFileToArchive> filesToArchive) {
            return DoForFiles(filesToArchive, ActionType.Pack);
        }

        /// <inheritdoc />
        public int CheckFileSet(IEnumerable<IFileInArchiveToCheck> filesToCheck) {
            int total = 0;
            foreach (var fileToCheck in filesToCheck.ToNonNullEnumerable()) {
                if (File.Exists(Path.Combine(fileToCheck.ArchivePath ?? "", fileToCheck.PathInArchive))) {
                    fileToCheck.Processed = true;
                    total++;
                } else {
                    fileToCheck.Processed = false;
                }
            }
            return total;
        }

        /// <inheritdoc cref="IArchiverList.ListFiles"/>
        public IEnumerable<IFileInArchive> ListFiles(string archivePath) {
            if (!Directory.Exists(archivePath)) {
                return Enumerable.Empty<IFileInArchive>();
            }
            var archivePathNormalized = archivePath.ToCleanPath();
            return Utils.EnumerateAllFiles(archivePath, SearchOption.AllDirectories, null, false, _cancelToken)
                .Select(path => {
                    var fileInfo = new FileInfo(path);
                    return new FileInFilesystem {
                        PathInArchive = path.ToRelativePath(archivePathNormalized),
                        ArchivePath = archivePath,
                        SizeInBytes = (ulong) fileInfo.Length,
                        LastWriteTime = fileInfo.LastWriteTime
                    };
                });
        }

        /// <inheritdoc cref="IArchiverExtract.ExtractFileSet"/>
        public int ExtractFileSet(IEnumerable<IFileInArchiveToExtract> filesToExtract) {
            return DoForFiles(filesToExtract, ActionType.Extract);
        }

        /// <inheritdoc cref="IArchiverDelete.DeleteFileSet"/>
        public int DeleteFileSet(IEnumerable<IFileInArchiveToDelete> filesToDelete) {
            return DoForFiles(filesToDelete, ActionType.Delete);
        }

        /// <inheritdoc cref="IArchiverMove.MoveFileSet"/>
        public int MoveFileSet(IEnumerable<IFileInArchiveToMove> filesToMove) {
            return DoForFiles(filesToMove, ActionType.Move);
        }
        
        private int DoForFiles(IEnumerable<IFileArchivedBase> filesIn, ActionType action) {
            if (filesIn == null) {
                return 0;
            }

            var files = filesIn.ToList();
            files.ForEach(f => f.Processed = false);
            
            var totalFiles = files.Count;
            var totalFilesDone = 0;

            try {
                foreach (var file in files) {
                    _cancelToken?.ThrowIfCancellationRequested();
                    string source = (action == ActionType.Pack ? ((IFileToArchive) file).SourcePath : Path.Combine(file.ArchivePath ?? "", file.PathInArchive)).ToCleanPath();
                    string target = null;
                    switch (action) {
                        case ActionType.Pack:
                            target = Path.Combine(file.ArchivePath ?? "", ((IFileToArchive) file).PathInArchive).ToCleanPath();
                            break;
                        case ActionType.Extract:
                            target = ((IFileInArchiveToExtract) file).ExtractionPath.ToCleanPath();
                            break;
                        case ActionType.Move:
                            target = Path.Combine(file.ArchivePath ?? "", ((IFileInArchiveToMove) file).NewRelativePathInArchive).ToCleanPath();
                            break;
                    }
                    
                    // ignore non existing files
                    if (string.IsNullOrEmpty(source) || !File.Exists(source)) {
                        continue;
                    }

                    // create the necessary target folder
                    string dir;
                    if (!string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(dir = Path.GetDirectoryName(target))) {
                        if (!Directory.Exists(dir)) {
                            Directory.CreateDirectory(dir);
                        }
                    }
                    
                    try {
                        switch (action) {
                            case ActionType.Pack:
                            case ActionType.Extract:
                                if (string.IsNullOrEmpty(target)) {
                                    throw new NullReferenceException("Target should not be null.");
                                }
                                if (!source.PathEquals(target)) {
                                    if (File.Exists(target)) {
                                        File.Delete(target);
                                    }
                                    try {
                                        var buffer = new byte[BufferSize];
                                        using (var sourceFileStream = File.OpenRead(source)) {
                                            long fileLength = sourceFileStream.Length;
                                            using (var dest = File.OpenWrite(target)) {
                                                long totalBytes = 0;
                                                int currentBlockSize;
                                                while ((currentBlockSize = sourceFileStream.Read(buffer, 0, buffer.Length)) > 0) {
                                                    totalBytes += currentBlockSize;
                                                    dest.Write(buffer, 0, currentBlockSize);
                                                    if (totalBytes != fileLength) {
                                                        OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(file.ArchivePath, file.PathInArchive, Math.Round((totalFilesDone + (double) totalBytes / fileLength) / totalFiles * 100, 2)));
                                                    }
                                                    _cancelToken?.ThrowIfCancellationRequested();
                                                }
                                            }
                                        }
                                    } catch (OperationCanceledException) {
                                        // cleanup the potentially unfinished file copy
                                        if (File.Exists(target)) {
                                            File.Delete(target);
                                        }
                                        throw;
                                    }
                                }
                                break;
                            case ActionType.Move:
                                if (string.IsNullOrEmpty(target)) {
                                    throw new NullReferenceException("Target should not be null.");
                                }
                                if (!source.PathEquals(target)) {
                                    if (File.Exists(target)) {
                                        File.Delete(target);
                                    }
                                    File.Move(source, target);
                                }
                                break;
                            case ActionType.Delete:
                                File.Delete(source);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(action), action, null);
                        }
                    } catch (OperationCanceledException) {
                        throw;
                    } catch (Exception e) {
                        throw new ArchiverException($"Failed to {action.ToString().ToLower()} {source.PrettyQuote()}{(string.IsNullOrEmpty(target) ? "" : $" in {target.PrettyQuote()}")}.", e);
                    }
                    
                    totalFilesDone++;
                    file.Processed = true;
                    OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(file.ArchivePath, file.PathInArchive, Math.Round((double) totalFilesDone / totalFiles * 100, 2)));
                }
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception e) {
                throw new ArchiverException($"Failed to {action.ToString().ToLower()} files.", e);
            }
            return totalFilesDone;
        }

        private enum ActionType {
            Pack,
            Extract,
            Move,
            Delete
        }
    }
}