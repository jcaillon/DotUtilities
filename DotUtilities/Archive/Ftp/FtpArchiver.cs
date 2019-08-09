#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (FtpArchiver.cs) is part of DotUtilities.
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
using System.Linq;
using System.Net;
using System.Text;
using DotUtilities.Archive.Ftp.Core;
using DotUtilities.Extensions;

namespace DotUtilities.Archive.Ftp {

    internal class FtpArchiver : ArchiverBase, IArchiverFullFeatured {

        /// <inheritdoc />
        public int CheckFileSet(IEnumerable<IFileInArchiveToCheck> filesToCheck) {
            int total = 0;
            foreach (var ftpGroupedFiles in filesToCheck.ToNonNullEnumerable().GroupBy(f => f.ArchivePath)) {
                try {
                    if (!ftpGroupedFiles.Key.ParseFtpAddress(out var uri, out var userName, out var passWord, out var host, out var port, out var relativePath)) {
                        throw new ArchiverException($"The ftp uri is invalid, the typical format is ftp://user:pass@server:port/path. Input uri was : {uri.PrettyQuote()}.");
                    }

                    _cancelToken?.ThrowIfCancellationRequested();

                    var ftp = FtpsClient.Instance.Get(uri);
                    ConnectOrReconnectFtp(ftp, userName, passWord, host, port);

                    foreach (var file in ftpGroupedFiles) {
                        _cancelToken?.ThrowIfCancellationRequested();
                        try {
                            ulong? size = null;
                            try {
                                size = ftp.GetFileTransferSize(Path.Combine(relativePath ?? "", file.PathInArchive));
                            } catch (FtpCommandException e) {
                                if (e.ErrorCode != 550) {
                                    // != than path does not exist
                                    throw;
                                }
                            }
                            if (size != null && size > 0) {
                                file.Processed = true;
                                total++;
                            } else {
                                file.Processed = false;
                            }
                        } catch (Exception e) {
                            throw new ArchiverException($"Failed to get the size of {file.PathInArchive.PrettyQuote()} from {file.ArchivePath.PrettyQuote()}.", e);
                        }
                    }

                    FtpsClient.Instance.DisconnectFtp();
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception e) {
                    throw new ArchiverException($"Failed to check files from {ftpGroupedFiles.Key.PrettyQuote()}.", e);
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
            foreach (var ftpGroupedFiles in filesToPack.GroupBy(f => f.ArchivePath)) {
                try {
                    if (!ftpGroupedFiles.Key.ParseFtpAddress(out var uri, out var userName, out var passWord, out var host, out var port, out var relativePath)) {
                        throw new ArchiverException($"The ftp uri is invalid, the typical format is ftp://user:pass@server:port/path. Input uri was : {uri.PrettyQuote()}.");
                    }

                    _cancelToken?.ThrowIfCancellationRequested();

                    var ftp = FtpsClient.Instance.Get(uri);
                    ConnectOrReconnectFtp(ftp, userName, passWord, host, port);

                    foreach (var file in ftpGroupedFiles) {
                        file.Processed = false;
                        if (!File.Exists(file.SourcePath)) {
                            continue;
                        }
                        _cancelToken?.ThrowIfCancellationRequested();
                        try {
                            var pathInArchive = Path.Combine(relativePath ?? "", file.PathInArchive);
                            var filesDone = totalFilesDone;
                            void TransferCallback(FtpsClient sender, ETransferActions action, string local, string remote, ulong done, ulong? total, ref bool cancel) {
                                OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(ftpGroupedFiles.Key, file.PathInArchive, Math.Round((filesDone + (double) done / (total ?? 0)) / totalFiles * 100, 2)));
                            }
                            try {
                                ftp.PutFile(file.SourcePath, pathInArchive, TransferCallback);
                            } catch (Exception) {
                                // try to create the directory and then push the file again
                                ftp.MakeDir(Path.GetDirectoryName(pathInArchive) ?? "", true);
                                ftp.SetCurrentDirectory("/");
                                ftp.PutFile(file.SourcePath, pathInArchive, TransferCallback);
                            }
                            totalFilesDone++;
                            file.Processed = true;
                        } catch (Exception e) {
                            throw new ArchiverException($"Failed to send {file.SourcePath.PrettyQuote()} to {file.ArchivePath.PrettyQuote()} and distant path {file.PathInArchive.PrettyQuote()}.", e);
                        }
                    }

                    FtpsClient.Instance.DisconnectFtp();
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception e) {
                    throw new ArchiverException($"Failed to send files to {ftpGroupedFiles.Key.PrettyQuote()}.", e);
                }
            }

            return totalFilesDone;
        }

        /// <inheritdoc cref="IArchiver.OnProgress"/>
        public event EventHandler<ArchiverEventArgs> OnProgress;

        /// <inheritdoc cref="IArchiverList.ListFiles"/>
        public IEnumerable<IFileInArchive> ListFiles(string ftpUri) {

            if (!ftpUri.ParseFtpAddress(out var uri, out var userName, out var passWord, out var host, out var port, out var relativePath)) {
                throw new ArchiverException($"The ftp uri is invalid, the typical format is ftp://user:pass@server:port/path. Input uri was : {uri.PrettyQuote()}.");
            }

            var ftp = FtpsClient.Instance.Get(uri);
            ConnectOrReconnectFtp(ftp, userName, passWord, host, port);

            var folderStack = new Stack<string>();
            folderStack.Push(relativePath);
            while (folderStack.Count > 0) {

                var folder = folderStack.Pop();
                ftp.SetCurrentDirectory(folder);

                foreach (var file in ftp.GetDirectoryList()) {
                    _cancelToken?.ThrowIfCancellationRequested();
                    if (file.IsDirectory) {
                        folderStack.Push(Path.Combine(folder, file.Name).Replace("\\", "/"));
                    } else {
                        yield return new FileInFtp {
                            PathInArchive = Path.Combine(folder, file.Name).ToCleanRelativePathUnix(),
                            LastWriteTime = file.CreationTime,
                            SizeInBytes = file.Size,
                            ArchivePath = uri
                        };
                    }
                }
            }

            FtpsClient.Instance.DisconnectFtp();
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
            foreach (var ftpGroupedFiles in filesToExtract.GroupBy(f => f.ArchivePath)) {
                try {
                    // create all necessary extraction folders
                    foreach (var extractDirGroupedFiles in ftpGroupedFiles.GroupBy(f => Path.GetDirectoryName(f.ExtractionPath))) {
                        if (!Directory.Exists(extractDirGroupedFiles.Key)) {
                            Directory.CreateDirectory(extractDirGroupedFiles.Key);
                        }
                    }

                    if (!ftpGroupedFiles.Key.ParseFtpAddress(out var uri, out var userName, out var passWord, out var host, out var port, out var relativePath)) {
                        throw new ArchiverException($"The ftp uri is invalid, the typical format is ftp://user:pass@server:port/path. Input uri was : {uri.PrettyQuote()}.");
                    }

                    _cancelToken?.ThrowIfCancellationRequested();

                    var ftp = FtpsClient.Instance.Get(uri);
                    ConnectOrReconnectFtp(ftp, userName, passWord, host, port);

                    foreach (var file in ftpGroupedFiles) {
                        _cancelToken?.ThrowIfCancellationRequested();
                        try {
                            try {
                                var filesDone = totalFilesDone;
                                void TransferCallback(FtpsClient sender, ETransferActions action, string local, string remote, ulong done, ulong? total, ref bool cancel) {
                                    OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(ftpGroupedFiles.Key, file.PathInArchive, Math.Round((filesDone + (double) done / (total ?? 0)) / totalFiles * 100, 2)));
                                }
                                ftp.GetFile(Path.Combine(relativePath ?? "", file.PathInArchive), file.ExtractionPath, TransferCallback);
                            } catch (FtpCommandException e) {
                                if (e.ErrorCode == 550) {
                                    // path does not exist
                                    continue;
                                }
                                throw;
                            }
                            totalFilesDone++;
                            file.Processed = true;
                        } catch (Exception e) {
                            throw new ArchiverException($"Failed to get {file.ExtractionPath.PrettyQuote()} from {file.ArchivePath.PrettyQuote()} and distant path {file.PathInArchive.PrettyQuote()}.", e);
                        }
                    }

                    FtpsClient.Instance.DisconnectFtp();
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception e) {
                    throw new ArchiverException($"Failed to get files from {ftpGroupedFiles.Key.PrettyQuote()}.", e);
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
            foreach (var ftpGroupedFiles in filesToDelete.GroupBy(f => f.ArchivePath)) {
                try {
                    if (!ftpGroupedFiles.Key.ParseFtpAddress(out var uri, out var userName, out var passWord, out var host, out var port, out var relativePath)) {
                        throw new ArchiverException($"The ftp uri is invalid, the typical format is ftp://user:pass@server:port/path. Input uri was : {uri.PrettyQuote()}.");
                    }

                    _cancelToken?.ThrowIfCancellationRequested();

                    var ftp = FtpsClient.Instance.Get(uri);
                    try {
                        ConnectOrReconnectFtp(ftp, userName, passWord, host, port);
                    } catch (ArchiverException) {
                        // do not throw exception when we can't connect to the server
                        continue;
                    }

                    foreach (var file in ftpGroupedFiles) {
                        _cancelToken?.ThrowIfCancellationRequested();
                        try {
                            try {
                                ftp.DeleteFile(Path.Combine(relativePath ?? "", file.PathInArchive));
                            } catch (FtpCommandException e) {
                                if (e.ErrorCode == 550) {
                                    // path does not exist
                                    continue;
                                }
                                throw;
                            }
                            totalFilesDone++;
                            file.Processed = true;
                            OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(ftpGroupedFiles.Key, file.PathInArchive, Math.Round(totalFilesDone / (double) totalFiles * 100, 2)));
                        } catch (Exception e) {
                            throw new ArchiverException($"Failed to delete {file.PathInArchive.PrettyQuote()} from {file.ArchivePath.PrettyQuote()}.", e);
                        }
                    }

                    FtpsClient.Instance.DisconnectFtp();
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception e) {
                    throw new ArchiverException($"Failed to delete files from {ftpGroupedFiles.Key.PrettyQuote()}.", e);
                }
            }

            return totalFilesDone;
        }

        /// <inheritdoc />
        public int MoveFileSet(IEnumerable<IFileInArchiveToMove> filesToMoveIn) {
            if (filesToMoveIn == null) {
                return 0;
            }

            var filesToMove = filesToMoveIn.ToList();
            filesToMove.ForEach(f => f.Processed = false);
            int totalFiles = filesToMove.Count;
            int totalFilesDone = 0;
            foreach (var ftpGroupedFiles in filesToMove.GroupBy(f => f.ArchivePath)) {
                try {
                    if (!ftpGroupedFiles.Key.ParseFtpAddress(out var uri, out var userName, out var passWord, out var host, out var port, out var relativePath)) {
                        throw new ArchiverException($"The ftp uri is invalid, the typical format is ftp://user:pass@server:port/path. Input uri was : {uri.PrettyQuote()}.");
                    }

                    _cancelToken?.ThrowIfCancellationRequested();

                    var ftp = FtpsClient.Instance.Get(uri);
                    ConnectOrReconnectFtp(ftp, userName, passWord, host, port);

                    foreach (var file in ftpGroupedFiles) {
                        _cancelToken?.ThrowIfCancellationRequested();
                        try {
                            var pathInArchive = Path.Combine(relativePath ?? "", file.PathInArchive);
                            var newPathInArchive = Path.Combine(relativePath ?? "", file.NewRelativePathInArchive);
                            try {
                                ftp.RenameFile(pathInArchive, newPathInArchive);
                            } catch (FtpCommandException e) {
                                if (e.ErrorCode == 550) {
                                    // path does not exist
                                    continue;
                                }
                                if (e.ErrorCode == 553) {
                                    // target already exists
                                    ftp.DeleteFile(newPathInArchive);
                                    ftp.RenameFile(pathInArchive, newPathInArchive);
                                } else {
                                    throw;
                                }
                            }
                            totalFilesDone++;
                            file.Processed = true;
                            OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(ftpGroupedFiles.Key, file.PathInArchive, Math.Round(totalFilesDone / (double) totalFiles * 100, 2)));
                        } catch (Exception e) {
                            throw new ArchiverException($"Failed to move {file.PathInArchive.PrettyQuote()} to {file.NewRelativePathInArchive.PrettyQuote()} in {file.ArchivePath.PrettyQuote()}.", e);
                        }
                    }

                    FtpsClient.Instance.DisconnectFtp();
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception e) {
                    throw new ArchiverException($"Failed to move files from {ftpGroupedFiles.Key.PrettyQuote()}.", e);
                }
            }
            return totalFilesDone;
        }

        /// <summary>
        ///     Connects to a FTP server trying every methods
        /// </summary>
        private void ConnectOrReconnectFtp(FtpsClient ftp, string userName, string passWord, string host, int port) {
            // try to connect!
            if (!ftp.Connected) {
                ConnectFtp(ftp, userName, passWord, host, port);
            } else {
                try {
                    ftp.GetCurrentDirectory();
                } catch (Exception) {
                    ConnectFtp(ftp, userName, passWord, host, port);
                }
            }
        }

        /// <summary>
        ///     Connects to a FTP server trying every methods
        /// </summary>
        private void ConnectFtp(FtpsClient ftp, string userName, string passWord, string host, int port) {

            NetworkCredential credential = null;
            if (!string.IsNullOrEmpty(userName)) {
                credential = new NetworkCredential(userName, passWord ?? "");
            }

            var modes = new List<EsslSupportMode>();
            typeof(EsslSupportMode).ForEach<EsslSupportMode>((s, l) => {
                modes.Add((EsslSupportMode) l);
            });

            var sb = new StringBuilder();

            ftp.DataConnectionMode = EDataConnectionMode.Passive;
            while (!ftp.Connected && ftp.DataConnectionMode == EDataConnectionMode.Passive) {
                foreach (var mode in modes.OrderByDescending(mode => mode)) {
                    try {
                        var curPort = port > 0 ? port : (mode & EsslSupportMode.Implicit) == EsslSupportMode.Implicit ? 990 : 21;
                        ftp.Connect(host, curPort, credential, mode, 1800);
                        ftp.Connected = true;
                        if (!ftp.Connected) {
                            ftp.Close();
                        }
                        break;
                    } catch (Exception e) {
                        sb.AppendLine($"{mode} >> {e.Message}");
                    }
                }
                ftp.DataConnectionMode = EDataConnectionMode.Active;
            }

            // failed?
            if (!ftp.Connected) {
                throw new ArchiverException($"Failed to connect to a FTP server with : Username : {userName ?? "none"}, Password : {passWord ?? "none"}, Host : {host}, Port : {(port == 0 ? 21 : port)}", new Exception(sb.ToString()));
            }
        }

    }
}
