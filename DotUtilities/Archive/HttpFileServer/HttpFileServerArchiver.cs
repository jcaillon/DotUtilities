#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (HttpFileServerArchiver.cs) is part of DotUtilities.
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
using System.Threading;
using DotUtilities.Http;

namespace DotUtilities.Archive.HttpFileServer {

    /// <summary>
    /// In that case, a folder on the file system represents an archive.
    /// </summary>
    internal class HttpFileServerArchiver : IHttpFileServerArchiver {

        private HttpRequest _httpRequest;

        private HttpRequest HttpRequest => _httpRequest ?? (_httpRequest = new HttpRequest(""));

        /// <inheritdoc cref="IHttpFileServerArchiver.SetProxy"/>
        public void SetProxy(string proxyUrl, string userName = null, string userPassword = null) {
            HttpRequest.UseProxy(proxyUrl, userName, userPassword);
        }

        /// <inheritdoc cref="IHttpFileServerArchiver.SetBasicAuthentication"/>
        public void SetBasicAuthentication(string userName, string userPassword) {
            HttpRequest.UseBasicAuthorizationHeader(userName, userPassword);
        }

        /// <inheritdoc cref="IHttpFileServerArchiver.SetHeaders"/>
        public void SetHeaders(Dictionary<string, string> headersKeyValue) {
            HttpRequest.UseHeaders(headersKeyValue);
        }

        /// <inheritdoc cref="IArchiver.SetCancellationToken"/>
        public void SetCancellationToken(CancellationToken? cancelToken) {
            HttpRequest.UseCancellationToken(cancelToken);
        }

        /// <inheritdoc cref="IArchiver.OnProgress"/>
        public event EventHandler<ArchiverEventArgs> OnProgress;

        /// <inheritdoc cref="IArchiver.ArchiveFileSet"/>
        public int ArchiveFileSet(IEnumerable<IFileToArchive> filesToArchive) {
            return DoAction(filesToArchive, Action.Upload);
        }

        /// <inheritdoc />
        public int ExtractFileSet(IEnumerable<IFileInArchiveToExtract> filesToExtract) {
            return DoAction(filesToExtract, Action.Download);
        }

        /// <inheritdoc />
        public int DeleteFileSet(IEnumerable<IFileInArchiveToDelete> filesToDelete) {
            return DoAction(filesToDelete, Action.Delete);
        }

        private int DoAction(IEnumerable<IFileArchivedBase> filesIn, Action action) {
            if (filesIn == null) {
                return 0;
            }

            var files = filesIn.ToList();

            // total size to handle
            long totalSizeDone = 0;
            long totalSize = 0;
                try {
                switch (action) {
                    case Action.Upload:
                        foreach (var file in files.OfType<IFileToArchive>()) {
                            if (File.Exists(file.SourcePath)) {
                                totalSize += new FileInfo(file.SourcePath).Length;
                            }
                        }
                        break;
                    case Action.Download:
                        foreach (var file in files.OfType<IFileInArchiveToExtract>()) {
                            var response = HttpRequest.GetFileSize(WebUtility.UrlEncode(file.PathInArchive.ToCleanRelativePathUnix()), out long size);
                            if (response.Success) {
                                totalSize += size;
                            }
                        }
                        break;
                }
            } catch (Exception e) {
                throw new ArchiverException($"Failed to assess the total file size to handle during {action.ToString().ToLower()}.", e);
            }

            bool totalSizeNotFound = totalSize == 0;

            int nbFilesProcessed = 0;
            foreach (var serverGroupedFiles in files.GroupBy(f => f.ArchivePath)) {
                try {
                    HttpRequest.UseBaseUrl(serverGroupedFiles.Key);
                    foreach (var file in serverGroupedFiles) {
                        bool requestOk;
                        HttpResponse response;
                        var fileRelativePath = WebUtility.UrlEncode(file.PathInArchive.ToCleanRelativePathUnix());

                        switch (action) {
                            case Action.Upload:
                                if (!File.Exists(((IFileToArchive) file).SourcePath)) {
                                    // skip to next file
                                    continue;
                                }
                                response = HttpRequest.PutFile(fileRelativePath, ((IFileToArchive) file).SourcePath, progress => {
                                    totalSizeDone += progress.NumberOfBytesDoneSinceLastProgress;
                                    OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(serverGroupedFiles.Key, fileRelativePath, Math.Round(totalSizeDone / (double) totalSize * 100, 2)));
                                });
                                requestOk = response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created;
                                break;
                            case Action.Download:
                                response = HttpRequest.DownloadFile(fileRelativePath, ((IFileInArchiveToExtract) file).ExtractionPath, progress => {
                                    if (totalSizeNotFound && progress.NumberOfBytesDoneSinceLastProgress == progress.NumberOfBytesDoneTotal) {
                                        totalSize += progress.NumberOfBytesTotal;
                                    }
                                    totalSizeDone += progress.NumberOfBytesDoneSinceLastProgress;
                                    OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(serverGroupedFiles.Key, fileRelativePath, Math.Round(totalSizeDone / (double) totalSize * 100, 2)));
                                });

                                requestOk = response.StatusCode == HttpStatusCode.OK;
                                if (response.StatusCode == HttpStatusCode.NotFound || response.Exception is WebException we && we.Status == WebExceptionStatus.NameResolutionFailure) {
                                    // skip to next file
                                    continue;
                                }
                                break;
                            case Action.Delete:
                                response = HttpRequest.DeleteFile(fileRelativePath);
                                requestOk = response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
                                if (response.StatusCode == HttpStatusCode.NotFound || response.Exception is WebException we1 && we1.Status == WebExceptionStatus.NameResolutionFailure) {
                                    // skip to next file
                                    continue;
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(action), action, null);
                        }

                        if (!requestOk) {
                            if (response.Exception != null) {
                                throw response.Exception;
                            }
                            throw new ArchiverException($"The server returned {response.StatusCode} : {response.StatusDescription} for {fileRelativePath}.");
                        }

                        nbFilesProcessed++;
                        file.Processed = true;
                        OnProgress?.Invoke(this, ArchiverEventArgs.NewProgress(serverGroupedFiles.Key, fileRelativePath, Math.Round(nbFilesProcessed / (double) files.Count * 100, 2)));
                    }
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception e) {
                    throw new ArchiverException($"Failed to {action.ToString().ToLower()} at {serverGroupedFiles.Key}.", e);
                }
            }
            return nbFilesProcessed;
        }

        private enum Action {
            Upload,
            Download,
            Delete
        }
    }
}
