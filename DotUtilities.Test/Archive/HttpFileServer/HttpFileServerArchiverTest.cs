#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (HttpFileServerArchiverTest.cs) is part of Oetools.Utilities.Test.
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotUtilities.Archive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oetools.Utilities.Test.Lib.Http;

namespace Oetools.Utilities.Test.Archive.HttpFileServer {

    [TestClass]
    public class HttpFileServerArchiverTest : ArchiveTest {

        private static string _testFolder;
        private static string TestFolder => _testFolder ?? (_testFolder = TestHelper.GetTestFolder(nameof(HttpFileServerArchiverTest)));

        [ClassInitialize]
        public static void Init(TestContext context) {
            Cleanup();
            Directory.CreateDirectory(TestFolder);
        }

        [ClassCleanup]
        public static void Cleanup() {
            if (Directory.Exists(TestFolder)) {
                Directory.Delete(TestFolder, true);
            }
        }

        [TestMethod]
        public void Test() {
            var serverPort = TestHelper.GetNextAvailablePort(2050);
            var proxyPort = TestHelper.GetNextAvailablePort(serverPort + 1);

            // hostname to use
            // we need something different than 127.0.0.1 or localhost for the proxy!
            IPHostEntry hostEntry;
            try {
                hostEntry = Dns.GetHostEntry("mylocalhost");
            } catch (Exception) {
                hostEntry = null;
            }
            var host = hostEntry == null ? "127.0.0.1" : "mylocalhost";

            var archiver = Archiver.NewHttpFileServerArchiver();
            Assert.IsNotNull(archiver);

            var baseDir = Path.Combine(TestFolder, "http");

            archiver.SetProxy($"http://{host}:{proxyPort}/", "jucai69d", "julien caillon");
            archiver.SetBasicAuthentication("admin", "admin123");

            var listFiles = GetPackageTestFilesList(TestFolder, $"http://{host}:{serverPort}/server1");
            listFiles.AddRange(GetPackageTestFilesList(TestFolder, $"http://{host}:{serverPort}/server2"));

            var fileServer = new SimpleHttpFileServer(baseDir, "admin", "admin123");
            var proxyServer = new SimpleHttpProxyServer("jucai69d", "julien caillon");

            var cts = new CancellationTokenSource();
            var task1 = HttpServer.ListenAsync(serverPort, cts.Token, fileServer.OnHttpRequest, true);
            var task2 = HttpServer.ListenAsync(proxyPort, cts.Token, proxyServer.OnHttpRequest, true);

            PartialTestForHttpFileServer(archiver, listFiles);

            if (!host.Equals("127.0.0.1")) {
                Assert.AreEqual(61, proxyServer.NbRequestsHandledOk);
            }

            HttpServer.Stop(cts, task1, task2);
        }
    }
}
