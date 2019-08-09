#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (SimpleHttpFileServer.cs) is part of Oetools.Utilities.Test.
// 
// Oetools.Utilities.Test is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Oetools.Utilities.Test is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Oetools.Utilities.Test. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Oetools.Utilities.Test.Lib.Http;

namespace Oetools.Utilities.Test.Archive.HttpFileServer {

    internal class SimpleHttpFileServer {
        
        private const int BufferSize = 1024 * 32;
        private string _rootPath;
        private string _basicAuthent;

        public SimpleHttpFileServer(string rootPath, string user, string password) {
            _rootPath = rootPath;
            _basicAuthent = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}"));
        }

        public void OnHttpRequest(HttpListenerRequest request, HttpListenerResponse response) {
            // handle basic authent
            var receivedAuthent = request.Headers.GetHeaderValues("Authorization")?.FirstOrDefault();
            if (!string.IsNullOrEmpty(_basicAuthent)) {
                if (string.IsNullOrEmpty(receivedAuthent)) {
                    // or HttpListener.AuthenticationSchemes
                    response.WithHeader("WWW-Authenticate", "Basic").WithCode(HttpStatusCode.Unauthorized).AsText("Authentication required.");
                    return;
                }
                if (receivedAuthent.Length <= 6 || !_basicAuthent.Equals(receivedAuthent.Substring(6))) {
                    response.WithCode(HttpStatusCode.Forbidden).AsText("Incorrect user/password.");
                    return;
                }
            }

            // get filename path
            string filePath = request.Url.AbsolutePath;
            if (!string.IsNullOrEmpty(filePath) && filePath.Length > 1) {
                // handle spaces with urldecode
                filePath = WebUtility.UrlDecode(filePath.Substring(1));
            }

            if (string.IsNullOrEmpty(filePath)) {
                response.WithCode(HttpStatusCode.NotFound).AsText("Path not specified.");
                return;
            }

            filePath = Path.Combine(_rootPath, filePath);

            switch (request.HttpMethod.ToUpper()) {
                case "PUT":
                    // curl -v -H "Expect:" -u admin:admin123 --upload-file myfile http://127.0.0.1:8084/repository/raw-hoster/remotefile.txt --proxy 127.0.0.1:8888

                    bool expect = request.Headers.GetHeaderValues("Expect")?.ToList().Exists(s => s.Equals("100-continue", StringComparison.OrdinalIgnoreCase)) ?? false;

                    // TODO : handle the continue... I can't figure out how to do this one
                    if (expect) {
                        response.WithCode(HttpStatusCode.InternalServerError).AsText("Can't handle continue.");
                        // response.Abort();
                        return;
                    }

                    // create necessary dir
                    var dir = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                        Directory.CreateDirectory(dir);
                    }

                    using (var fileStr = File.OpenWrite(filePath)) {
                        byte[] buffer = new byte[BufferSize];
                        int nbBytesRead;
                        while ((nbBytesRead = request.InputStream.Read(buffer, 0, buffer.Length)) > 0) {
                            fileStr.Write(buffer, 0, nbBytesRead);
                        }
                    }

                    // finish
                    response.WithHeader("Location", request.RawUrl).WithCode(HttpStatusCode.Created);
                    break;

                case "GET":
                case "HEAD":
                    // curl -v -u admin:admin123 -o mydownloadedfile http://127.0.0.1:8084/repository/raw-hoster/remotefile.txt --proxy 127.0.0.1:8888

                    if (!File.Exists(filePath)) {
                        response.StatusCode = (int) HttpStatusCode.NotFound;
                    } else {
                        using (var stream = File.OpenRead(filePath)) {
                            response.ContentType = "application/octet-stream";
                            response.ContentLength64 = stream.Length;

                            if (request.HttpMethod.ToUpper().Equals("GET")) {
                                byte[] buffer = new byte[BufferSize];
                                int nbBytesRead;
                                while ((nbBytesRead = stream.Read(buffer, 0, buffer.Length)) > 0) {
                                    response.OutputStream.Write(buffer, 0, nbBytesRead);
                                }

                                response.OutputStream.Flush();
                            }
                        }

                        var lastTimeWrite = File.GetLastWriteTimeUtc(filePath);
                        response.WithHeader("ETag", lastTimeWrite.Ticks.ToString("x")).WithHeader("Last-Modified", lastTimeWrite.ToString("R")).WithCode();
                    }

                    break;

                case "DELETE":
                    // curl -v -u admin:admin123 -X DELETE http://127.0.0.1:8084/repository/raw-hoster/remotefile.txt --proxy 127.0.0.1:8888

                    if (!File.Exists(filePath)) {
                        response.WithCode(HttpStatusCode.NotFound);
                    } else {
                        File.Delete(filePath);
                        response.WithCode(HttpStatusCode.NoContent);
                    }

                    break;
                default:
                    throw new Exception($"Unknown http verb : {request.HttpMethod}.");
            }
        }
    }
}