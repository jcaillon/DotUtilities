#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ArchiveFtpTest.cs) is part of Oetools.Utilities.Test.
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
using System.Threading;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oetools.Utilities.Archive.Ftp;

namespace Oetools.Utilities.Test.Archive.Ftp {

    [TestClass]
    public class ArchiveFtpTest : ArchiveTest {

        private static string _testFolder;

        private static string TestFolder => _testFolder ?? (_testFolder = TestHelper.GetTestFolder(nameof(ArchiveFtpTest)));

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
            var serverHost = @"127.0.0.1";
            var serverIp = TestHelper.GetNextAvailablePort(2024);
            var ftpUri = $"ftp://{serverHost}:{serverIp}/";

            // Setup dependency injection
            var services = new ServiceCollection();

            services.Configure<DotNetFileSystemOptions>(opt => opt
                .RootPath = Path.Combine(TestFolder, "archives"));

            services.AddFtpServer(builder => builder
                .UseDotNetFileSystem() // Use the .NET file system functionality
                .EnableAnonymousAuthentication()); // allow anonymous logins

            services.Configure<FtpServerOptions>(opt => {
                opt.ServerAddress = serverHost;
                opt.Port = serverIp;
            });

            using (var serviceProvider = services.BuildServiceProvider()) {
                // Initialize the FTP server
                var ftpServer = serviceProvider.GetRequiredService<IFtpServer>();

                // Start the FTP server
                ftpServer.Start();

                SpinWait.SpinUntil(() => ftpServer.Ready, 5000);

                var archiver = new FtpArchiver();

                var listFiles = GetPackageTestFilesList(TestFolder, ftpUri);

                WholeTest(archiver, listFiles);

                // Stop the FTP server
                ftpServer.Stop();
            }
        }
    }
}
