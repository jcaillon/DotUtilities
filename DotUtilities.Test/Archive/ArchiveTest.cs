#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (IArchiveTest.cs) is part of Oetools.Utilities.Test.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oetools.Utilities.Archive;
using Oetools.Utilities.Lib;

namespace Oetools.Utilities.Test.Archive {
    
    public class ArchiveTest {

        private bool _hasReceivedGlobalProgression;
        private CancellationTokenSource _cancelSource;

        protected void WholeTest(IArchiverFullFeatured archiver, List<FileInArchive> listFiles) {
            archiver.OnProgress += ArchiverOnOnProgress;
            CreateArchive(archiver, listFiles);
            MoveInArchives(archiver, listFiles);
            ListArchive(archiver, listFiles);
            CheckExistenceInArchive(archiver, listFiles);
            Extract(archiver, listFiles);
            DeleteFilesInArchive(archiver, listFiles);
            CheckForEmptyArchives(archiver, listFiles);
            archiver.OnProgress -= ArchiverOnOnProgress;
        }

        protected void PartialTestForHttpFileServer(IArchiverBasic archiverBasic, List<FileInArchive> listFiles) {
            archiverBasic.OnProgress += ArchiverOnOnProgress;
            CreateArchive(archiverBasic, listFiles);
            Extract(archiverBasic, listFiles);
            DeleteFilesInArchive(archiverBasic, listFiles);
            archiverBasic.OnProgress -= ArchiverOnOnProgress;
        }
        
        protected void CreateArchive(IArchiver archiverBasic, List<FileInArchive> listFiles) {
            
            var modifiedList = listFiles.GetRange(1, listFiles.Count - 1);
            
            // Test the cancellation.
            _cancelSource = new CancellationTokenSource();
            archiverBasic.SetCancellationToken(_cancelSource.Token);
            var list = modifiedList;
            Assert.ThrowsException<OperationCanceledException>(() => archiverBasic.ArchiveFileSet(list));
            _cancelSource = null;
            archiverBasic.SetCancellationToken(null);
            
            CleanupArchives(listFiles);
            
            // try to add a non existing file
            modifiedList.Add(new FileInArchive {
                ArchivePath = listFiles.First().ArchivePath,
                ExtractionPath = listFiles.First().ExtractionPath,
                PathInArchive = "random.name"
            });
            
            Assert.AreEqual(modifiedList.Count - 1, archiverBasic.ArchiveFileSet(modifiedList));
            Assert.AreEqual(modifiedList.Count - 1, modifiedList.Count(f => f.Processed), "Wrong number of processed files.");
            
            // test the update of archives
            modifiedList = listFiles.GetRange(0, 1);
            Assert.AreEqual(modifiedList.Count, archiverBasic.ArchiveFileSet(modifiedList));
 
            foreach (var archive in listFiles.GroupBy(f => f.ArchivePath)) {
                if (Directory.Exists(Path.GetDirectoryName(archive.Key))) {
                    Assert.IsTrue(File.Exists(archive.Key) || Directory.Exists(archive.Key), $"The archive does not exist : {archive}");
                }
            }
            
            // check progress
            Assert.IsTrue(_hasReceivedGlobalProgression, "Should have received a progress event.");
            Assert.AreEqual(listFiles.Count, listFiles.Count(f => f.Processed), "Wrong number of processed files.");
            
        }
        
        protected void MoveInArchives(IArchiverMove archiver, List<FileInArchive> listFiles) {

            var modifiedList = listFiles.ToList();
            modifiedList.Add(new FileInArchive {
                ArchivePath = listFiles.First().ArchivePath,
                ExtractionPath = listFiles.First().ExtractionPath,
                PathInArchive = "random.name"
            });
            
            modifiedList.ForEach(f => f.NewRelativePathInArchive = $"{f.PathInArchive}_move");
            
            // Test the cancellation.
            _cancelSource = new CancellationTokenSource();
            archiver.SetCancellationToken(_cancelSource.Token);
            var list = modifiedList;
            Assert.ThrowsException<OperationCanceledException>(() => archiver.MoveFileSet(list));
            _cancelSource = null;
            archiver.SetCancellationToken(null);
            
            CreateArchive(archiver, listFiles);

            // try to move a non existing file
            Assert.AreEqual(modifiedList.Count - 1, archiver.MoveFileSet(modifiedList));
            Assert.AreEqual(modifiedList.Count - 1, modifiedList.Count(f => f.Processed), "Wrong number of processed files.");
            
            // check progress
            Assert.IsTrue(_hasReceivedGlobalProgression, "Should have received a progress event.");
            Assert.AreEqual(listFiles.Count, listFiles.Count(f => f.Processed), "Wrong number of processed files.");
            
            // move them back
            modifiedList.ForEach(f => {
                f.PathInArchive = f.NewRelativePathInArchive;
                f.NewRelativePathInArchive = f.NewRelativePathInArchive.Substring(0, f.NewRelativePathInArchive.Length - 5);
            });
            
            Assert.AreEqual(modifiedList.Count - 1, archiver.MoveFileSet(modifiedList));
            Assert.AreEqual(modifiedList.Count - 1, modifiedList.Count(f => f.Processed), "Wrong number of processed files.");
            
            modifiedList.ForEach(f => {
                f.PathInArchive = f.NewRelativePathInArchive;
            });
        }
        
        protected void ListArchive(IArchiverList archiver, List<FileInArchive> listFiles) {
            foreach (var groupedTheoreticalFiles in listFiles.GroupBy(f => f.ArchivePath)) {
                var actualFiles = archiver.ListFiles(groupedTheoreticalFiles.Key).ToList();
                foreach (var theoreticalFile in groupedTheoreticalFiles) {
                    Assert.IsTrue(actualFiles.ToList().Exists(f => f.PathInArchive.ToCleanRelativePathUnix().Equals(theoreticalFile.PathInArchive.ToCleanRelativePathUnix())), $"Can't find file in list : {theoreticalFile.PathInArchive}");
                }
                Assert.AreEqual(groupedTheoreticalFiles.Count(), actualFiles.Count, $"Wrong number of files listed : {groupedTheoreticalFiles.Count()}!={actualFiles.Count}");
            }
        }
        
        protected void CheckExistenceInArchive(IArchiverExistenceCheck archiver, List<FileInArchive> listFiles) {
            listFiles.ForEach(f => f.Processed = false);
            var modifiedList = listFiles.ToList();
            modifiedList.Add(new FileInArchive {
                ArchivePath = Path.Combine(listFiles.First().ArchivePath, "random"),
                PathInArchive = listFiles.First().PathInArchive
            });
            modifiedList.Add(new FileInArchive {
                ArchivePath = listFiles.First().ArchivePath,
                PathInArchive = "random"
            });
            Assert.AreEqual(0, modifiedList.Count(f => f.Processed));
            
            Assert.AreEqual(listFiles.Count, archiver.CheckFileSet(modifiedList), "Incorrect number of existing files.");
            Assert.AreEqual(listFiles.Count, modifiedList.Count(f => f.Processed), "Should only count existing files.");
            Assert.AreEqual(2, modifiedList.Count(f => !f.Processed), "The 2 fake files do not exist.");
        }

        protected void Extract(IArchiverExtract archiverBasic, List<FileInArchive> listFiles) {

            var modifiedList = listFiles.ToList();
            modifiedList.Add(new FileInArchive {
                ArchivePath = listFiles.First().ArchivePath,
                ExtractionPath = listFiles.First().ExtractionPath,
                PathInArchive = "random.name"
            });
            
            // Test the cancellation.
            _cancelSource = new CancellationTokenSource();
            archiverBasic.SetCancellationToken(_cancelSource.Token);
            var list = modifiedList;
            Assert.ThrowsException<OperationCanceledException>(() => archiverBasic.ExtractFileSet(list));
            _cancelSource = null;
            archiverBasic.SetCancellationToken(null);
            
            CreateArchive(archiverBasic, listFiles);
            CleanupExtractedFiles(listFiles);
            
            // try to extract a non existing file
            Assert.AreEqual(modifiedList.Count - 1, archiverBasic.ExtractFileSet(modifiedList));
            Assert.AreEqual(modifiedList.Count - 1, modifiedList.Count(f => f.Processed), "Wrong number of processed files.");
            
            foreach (var fileToExtract in listFiles) {
                Assert.IsTrue(File.Exists(fileToExtract.ExtractionPath), $"Extracted file does not exist : {fileToExtract.ExtractionPath}");
                Assert.AreEqual(File.ReadAllText(fileToExtract.SourcePath), File.ReadAllText(fileToExtract.ExtractionPath), "Incoherent extracted file content");
            }
            
            // check progress
            Assert.IsTrue(_hasReceivedGlobalProgression, "Should have received a progress event.");
            Assert.AreEqual(listFiles.Count, listFiles.Count(f => f.Processed), "Wrong number of processed files.");
        }
        
        protected void DeleteFilesInArchive(IArchiverDelete archiverBasic, List<FileInArchive> listFiles) {

            var modifiedList = listFiles.ToList();
            modifiedList.Add(new FileInArchive {
                ArchivePath = listFiles.First().ArchivePath,
                ExtractionPath = listFiles.First().ExtractionPath,
                PathInArchive = "random.name"
            });
            
            // Test the cancellation.
            _cancelSource = new CancellationTokenSource();
            archiverBasic.SetCancellationToken(_cancelSource.Token);
            var list = modifiedList;
            Assert.ThrowsException<OperationCanceledException>(() => archiverBasic.DeleteFileSet(list));
            _cancelSource = null;
            archiverBasic.SetCancellationToken(null);
            
            CreateArchive(archiverBasic, listFiles);

            // try to delete a non existing file
            Assert.AreEqual(modifiedList.Count - 1, archiverBasic.DeleteFileSet(modifiedList));
            Assert.AreEqual(modifiedList.Count - 1, modifiedList.Count(f => f.Processed), "Wrong number of processed files.");
            
            // check progress
            Assert.IsTrue(_hasReceivedGlobalProgression, "Should have received a progress event.");
            Assert.AreEqual(listFiles.Count, listFiles.Count(f => f.Processed), "Wrong number of processed files.");
        }

        protected void CheckForEmptyArchives(IArchiverList archiver, List<FileInArchive> listFiles) {
            foreach (var groupedFiles in listFiles.GroupBy(f => f.ArchivePath)) {
                var files = archiver.ListFiles(groupedFiles.Key);
                Assert.AreEqual(0, files.Count(), $"The archive is not empty : {groupedFiles.Key}");
            }
        }

        protected void ArchiverOnOnProgress(object sender, ArchiverEventArgs e) {
            if (e.PercentageDone < 0 || e.PercentageDone > 100) {
                throw new Exception($"Wrong value for percentage done : {e.PercentageDone}%.");
            }
            _hasReceivedGlobalProgression = true;
            _cancelSource?.Cancel();
        }
        
        protected List<FileInArchive> GetPackageTestFilesList(string testFolder, string archivePath) {
            
            var outputList = new List<FileInArchive> {
                new FileInArchive {
                    SourcePath = Path.Combine(testFolder, "file 0.txt"),
                    ArchivePath = archivePath,
                    PathInArchive = "file 0.txt",
                    ExtractionPath = Path.Combine(testFolder, "extract", Path.GetFileName(archivePath) ?? "", "file 0.txt")
                },
                new FileInArchive {
                    SourcePath = Path.Combine(testFolder, "file1.txt"),
                    ArchivePath = archivePath,
                    PathInArchive = "file1.txt",
                    ExtractionPath = Path.Combine(testFolder, "extract", Path.GetFileName(archivePath) ?? "", "file1.txt")
                },
                new FileInArchive {
                    SourcePath = Path.Combine(testFolder, "file2.txt"),
                    ArchivePath = archivePath,
                    PathInArchive = "subfolder1/file2.txt",
                    ExtractionPath = Path.Combine(testFolder, "extract", Path.GetFileName(archivePath) ?? "", "subfolder1", "file2.txt")
                },
                new FileInArchive {
                    SourcePath = Path.Combine(testFolder, "file3.txt"),
                    ArchivePath = archivePath,
                    PathInArchive = "subfolder1\\bla bla/file3.txt",
                    ExtractionPath = Path.Combine(testFolder, "extract", Path.GetFileName(archivePath) ?? "", "subfolder1", "bla bla", "file3.txt")
                }
            };

            CreateSourceFiles(outputList);
            CleanupExtractedFiles(outputList);
            CleanupArchives(outputList);
            
            return outputList;
        }
        
        protected void CreateSourceFiles(List<FileInArchive> fileList) {
            byte[] fileBuffer = Enumerable.Repeat((byte) 42, 1000).ToArray();
            foreach (var file in fileList) {
                // File.WriteAllText(file.SourcePath, $"\"{Path.GetFileName(file.SourcePath)}\"");
                using (Stream sourceStream = File.OpenWrite(file.SourcePath)) {
                    for (int i = 0; i < 2000; i++) {
                        sourceStream.Write(fileBuffer, 0, fileBuffer.Length);
                    }
                }
            }
        }
        
        protected void CleanupExtractedFiles(List<FileInArchive> fileList) {
            foreach (var file in fileList) {
                if (File.Exists(file.ExtractionPath)) {
                    File.Delete(file.ExtractionPath);
                }
            }
        }
        
        protected void CleanupArchives(List<FileInArchive> fileList) {
            foreach (var cabGrouped in fileList.GroupBy(f => f.ArchivePath)) {
                if (File.Exists(cabGrouped.Key)) {
                    File.Delete(cabGrouped.Key);
                }
            }
        }
        
    }
}