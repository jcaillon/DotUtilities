#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (PathUtilsTest.cs) is part of Oetools.Utilities.Test.
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
using System.Text.RegularExpressions;
using DotUtilities.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotUtilities.Test {

    [TestClass]
    public class PathUtilsTest {

        private static string _testFolder;

        private static string TestFolder => _testFolder ?? (_testFolder = TestHelper.GetTestFolder(nameof(PathUtilsTest)));

        [ClassInitialize]
        public static void Init(TestContext context) {
            Cleanup();
            Utils.CreateDirectoryIfNeeded(TestFolder);
        }


        [ClassCleanup]
        public static void Cleanup() {
            Utils.DeleteDirectoryIfExists(TestFolder, true);
        }

        [TestMethod]
        [DataRow(@"file.ext", @".df", "file.ext")]
        [DataRow(@"file", @".df", "file.df")]
        public void AddFileExtention(string path1, string ext, string expect) {
            Assert.AreEqual(expect, path1.AddFileExtention(ext), $"{path1 ?? "null"} {ext ?? "null"}");
        }

        [TestMethod]
        [DataRow(null, null, true, DisplayName = "null")]
        [DataRow(@"", @"", true, DisplayName = "empty")]
        [DataRow(@"a", @"", false)]
        [DataRow(@"a", @"b", false)]
        [DataRow(@"a", @"a", true)]
        [DataRow(@"aa", @"ab", false)]
        [DataRow(@"aa", @"aa", true)]
        [DataRow(@"aaa", @"aba", false)]
        [DataRow(@"aaa", @"aaa", true)]
        [DataRow(@"file", @"file2", false)]
        [DataRow(@"file", @"file", true)]
        [DataRow(@"d:\folder\file.ext", @"c:\folder\file.ext", false)]
        [DataRow(@"c:\folder\file.ext", @"c:\folder\file.ext", true)]
        public void PathEquals(string path1, string path2, bool expect) {
            Assert.AreEqual(expect, path1.PathEquals(path2), $"{path1 ?? "null"} vs {path2 ?? "null"}");
        }

        [TestMethod]
        [DataRow(null, null, true, DisplayName = "null")]
        [DataRow(@"", @"", true, DisplayName = "empty")]
        [DataRow(@"\\server\folder\file", @"C:\", true)]
        [DataRow(@"d:\zefef", @"/bouh", true)]
        [DataRow(@"d:\zefef", @"c:\folder", false)]
        [DataRow(@"d:", @"c:", false)]
        [DataRow(@"c:", @"c:", true)]
        public void ArePathOnSameDrive_Test(string path1, string path2, bool expect) {
            Assert.AreEqual(!Utils.IsRuntimeWindowsPlatform || expect, Utils.ArePathOnSameDrive(path1, path2));
        }

        [TestMethod]
        [DataRow(null, null, null, DisplayName = "null")]
        [DataRow(@"", null, @"", DisplayName = "empty")]
        [DataRow(@"\\server\folder\file", null, @"\\server\folder\file")]
        [DataRow(@"\\server\folder\file", @"\\server", @"folder\file")]
        [DataRow(@"\\server\folder\file", @"\\server\", @"folder\file")]
        [DataRow(@"\\server\folder\file", @"\\server\folder", @"file")]
        [DataRow(@"c:\server\folder\file", @"c:\server\folder", @"file")]
        [DataRow(@"c:\server\folder\file", @"c:\derp", @"c:\server\folder\file")]
        public void FromAbsolutePathToRelativePath_Test(string pattern, string pathToDelete, string expect) {
            if (!Utils.IsRuntimeWindowsPlatform) {
                return;
            }
            Assert.AreEqual(expect, pattern.ToRelativePath(pathToDelete));
        }

        [TestMethod]
        [DataRow(null, null, DisplayName = "null")]
        [DataRow(@"", @"", DisplayName = "empty")]
        [DataRow(@"\\server/", @"\\server")]
        [DataRow(@"C:\windows", @"C:\windows")]
        [DataRow(@"C:\windows/", @"C:\windows")]
        [DataRow(@"C:\windows/\", @"C:\windows")]
        public void TrimEndDirectorySeparator_Test(string pattern, string expect) {
            if (!Utils.IsRuntimeWindowsPlatform) {
                return;
            }
            Assert.AreEqual(expect, pattern.TrimEndDirectorySeparator());
        }

        [TestMethod]
        [DataRow(null, null, DisplayName = "null")]
        [DataRow(@"", @"", DisplayName = "empty")]
        [DataRow(@"\//\\/server/", @"\\server")]
        [DataRow(@"/windows", @"C:\windows")]
        [DataRow(@"d:/folder/file   ", @"d:\folder\file")]
        [DataRow(@"   d:/folder/\\\\\\\file", @"d:\folder\file")]
        public void ToCleanPath_Test(string pattern, string expect) {
            if (!Utils.IsRuntimeWindowsPlatform) {
                return;
            }
            Assert.AreEqual(expect, pattern.ToCleanPath());
        }

        [TestMethod]
        [DataRow(null, null, DisplayName = "null")]
        [DataRow(@"", @"", DisplayName = "empty")]
        [DataRow(@"\//\\/server/", @"server")]
        [DataRow(@"/windows", @"windows")]
        [DataRow(@"d:/folder/\\\\\\\file   ", @"d:/folder/file")]
        [DataRow(@"    /folder//cool/derp", @"folder/cool/derp")]
        public void ToCleanRelativePathUnix_Test(string pattern, string expect) {
            if (!Utils.IsRuntimeWindowsPlatform) {
                return;
            }
            Assert.AreEqual(expect, pattern.ToCleanRelativePathUnix());
        }

        [TestMethod]
        [DataRow(null, null, DisplayName = "null")]
        [DataRow(@"", @"", DisplayName = "empty")]
        [DataRow(@"\//\\/server/", @"server")]
        [DataRow(@"/windows", @"windows")]
        [DataRow(@"d:/folder/\\\\\\\file   ", @"d:\folder\file")]
        [DataRow(@"    /folder//cool/derp", @"folder\cool\derp")]
        public void ToCleanRelativePathWin_Test(string pattern, string expect) {
            if (!Utils.IsRuntimeWindowsPlatform) {
                return;
            }
            Assert.AreEqual(expect, pattern.ToCleanRelativePathWin());
        }

        [TestMethod]
        [DataRow(null, false, DisplayName = "null")]
        [DataRow(@"", false, DisplayName = "empty")]
        [DataRow("zf\tezefdze", false, DisplayName = "Invalid char")]
        [DataRow(@"((000", false, DisplayName = "unbalanced ((")]
        [DataRow(@"))00000", false, DisplayName = "too many ))")]
        [DataRow(@"C:\windows\**\?*", true)]
        [DataRow(@"/linux/**/?*", true)]
        public void ValidatePathWildCard_Test(string pattern, bool expectOk) {
            if (!expectOk) {
                Assert.ThrowsException<Exception>(() => Utils.ValidatePathWildCard(pattern), pattern ?? "null");
            } else {
                Utils.ValidatePathWildCard(pattern);
            }
        }

        [TestMethod]
        public void EnumerateFolders_Test() {
            Directory.CreateDirectory(Path.Combine(TestFolder, "test1"));
            Directory.CreateDirectory(Path.Combine(TestFolder, "test2"));
            Directory.CreateDirectory(Path.Combine(TestFolder, "test2", "subtest2"));
            Directory.CreateDirectory(Path.Combine(TestFolder, "test2", "subtest2", "end2"));
            Directory.CreateDirectory(Path.Combine(TestFolder, "test3"));
            Directory.CreateDirectory(Path.Combine(TestFolder, "test3", "subtest3"));

            var dirInfo = Directory.CreateDirectory(Path.Combine(TestFolder, "test1_hidden"));
            dirInfo.Attributes |= FileAttributes.Hidden;
            dirInfo = Directory.CreateDirectory(Path.Combine(TestFolder, "test2", "test2_hidden"));
            dirInfo.Attributes |= FileAttributes.Hidden;

            var list = Utils.EnumerateAllFolders(TestFolder).ToList();
            Assert.AreEqual(8, list.Count);

            list = Utils.EnumerateAllFolders(TestFolder, excludeHidden: true).ToList();
            Assert.AreEqual(6, list.Count);
            Assert.AreEqual(false, list.Any(s => s.Contains("_hidden")));

            list = Utils.EnumerateAllFolders(TestFolder, SearchOption.TopDirectoryOnly).ToList();
            Assert.AreEqual(4, list.Count);

            list = Utils.EnumerateAllFolders(TestFolder, SearchOption.AllDirectories, new List<string> {
                @".*_hid.*",
                @"test3"
            }).ToList();
            Assert.AreEqual(4, list.Count);
        }

        [DataTestMethod]
        [DataRow(@"C**", null)]
        [DataRow(@"improbable_thing", null)]
        [DataRow(@"C:\**", @"C:")]
        [DataRow(@"C:\windows**", @"C:")]
        [DataRow(@"C:\windows\<pozdzek!>", @"C:\windows")]
        [DataRow(@"C:\windows(bla|bla)\<pozdzek!>", @"C:")]
        [DataRow(@"**", null)]
        public void GetLongestValidDirectory_Test(string input, string expected) {
            if (Utils.IsRuntimeWindowsPlatform) {
                Assert.AreEqual(expected, Utils.GetLongestValidDirectory(input));
            }
        }

        [TestMethod]
        public void EnumerateFiles_Test() {
            File.WriteAllText(Path.Combine(TestFolder, "file1"), "");
            Directory.CreateDirectory(Path.Combine(TestFolder, "test1"));
            File.WriteAllText(Path.Combine(TestFolder, "test1", "file2"), "");
            Directory.CreateDirectory(Path.Combine(TestFolder, "test2"));
            File.WriteAllText(Path.Combine(TestFolder, "test2", "file3"), "");
            Directory.CreateDirectory(Path.Combine(TestFolder, "test2", "subtest2"));
            Directory.CreateDirectory(Path.Combine(TestFolder, "test2", "subtest2", "end2"));
            File.WriteAllText(Path.Combine(TestFolder, "test2", "subtest2", "end2", "file4"), "");
            Directory.CreateDirectory(Path.Combine(TestFolder, "test3"));
            Directory.CreateDirectory(Path.Combine(TestFolder, "test3", "subtest3"));
            File.WriteAllText(Path.Combine(TestFolder, "test3", "subtest3", "file5"), "");

            var dirInfo = Directory.CreateDirectory(Path.Combine(TestFolder, "test1_hidden"));
            File.WriteAllText(Path.Combine(TestFolder, "test1_hidden", "file6"), "");
            dirInfo.Attributes |= FileAttributes.Hidden;
            dirInfo = Directory.CreateDirectory(Path.Combine(TestFolder, "test2", "test2_hidden"));
            File.WriteAllText(Path.Combine(TestFolder, "test2", "test2_hidden", "file7"), "");
            dirInfo.Attributes |= FileAttributes.Hidden;

            var list = Utils.EnumerateAllFiles(TestFolder).ToList();
            Assert.AreEqual(7, list.Count);

            list = Utils.EnumerateAllFiles(TestFolder, SearchOption.TopDirectoryOnly).ToList();
            Assert.AreEqual(1, list.Count);

            list = Utils.EnumerateAllFiles(TestFolder, excludeHiddenFolders: true).ToList();
            Assert.AreEqual(5, list.Count);

            list = Utils.EnumerateAllFiles(TestFolder, SearchOption.AllDirectories , new List<string> {
                @"file1", // exclude file
                @".*[\\\/]test2[\\\/].*" // exclude folder
            }).ToList();
            Assert.AreEqual(3, list.Count);
        }

        [TestMethod]
        [DataRow(@"file.xml", @"*.xml", true)]
        [DataRow(@"file.xml", @"*.fk;*.xml", true)]
        [DataRow(@"file.derp", @"*.fk;*.xml", false)]
        [DataRow(@"path/file.xml", @"*.fk;*.xml", true)]
        [DataRow(@"path2\file.xml", @"*.fk;*.xml", true)]
        [DataRow(@"path2\file", @"*.fk;*.xml", false)]
        [DataRow(@"file.fk", @"*.fk;*.xml", true)]
        [DataRow(@"", @"*.fk;*.xml", false)]
        [DataRow(@"file", @"", false)]
        [DataRow(null, null, false)]
        public void TestFileAgainstListOfExtensions_Test(string source, string pattern, bool expected) {
            Assert.AreEqual(expected, source.TestFileNameAgainstListOfPatterns(pattern));
        }

        [TestMethod]
        [DataRow(@"c/folder/file.ext", @"fack", false)]
        [DataRow(@"c/folder/file.ext", @"fack;**folder**", true)]
        [DataRow(@"c/folder/file.ext", @"f((old))er", false)]
        [DataRow(@"c/folder/file.ext", @"**folder**", true)]
        [DataRow(@"c/folder/file.ext", @"**/file", false)]
        [DataRow(@"c/folder/file.ext", @"((**))/file.ext", true)]
        [DataRow(@"c\folder\file.ext", @"**\file.ext", true)]
        [DataRow(@"c\folder\file.ext", @"**\file.*", true)]
        [DataRow(@"c\folder\file.ext", @"((**\??le.*))", true)]
        [DataRow(@"c\folder\file.ext", @"*\file.ext", false)]
        [DataRow(@"c/folder\file.ext", @"d/**", false)]
        [DataRow(@"c/folder\file.ext", @"?/**", true)]
        [DataRow(@"c/folder\file.ext", @"c/**", true)]
        [DataRow(@"c/folder\file.ext", @"c/**((*.ext))", true)]
        [DataRow(@"c/folder\file.ext", @"c/*/*", true)]
        [DataRow(@"c/folder\file.ext", @"c/*/*/*", false)]
        [DataRow(@"c/folder/file.ext", @"c\folder/((**))", true)]
        [DataRow(@"c\folder/file.ext", @"c/((folder/((**))file.ext))", true)]
        [DataRow(@"c\folder/file.ext", @"((**))\((fo((ld))er))/**file**", true)]
        [DataRow(@"c\folder/file.ext", @"**/*/**", true)]
        [DataRow(@"/folder/sub/file.ext", @"/**/file.ext", true)]
        [DataRow(@"/file.ext", @"/**/file.ext", false)]
        [DataRow(@"file.ext", @"**/*/**/*", false)]
        [DataRow(@"file.ext", null, false)]
        [DataRow(@"file.ext", @"", false)]
        public void TestAgainstListOfPatterns_Test(string source, string pattern, bool expected) {
            Assert.AreEqual(expected, source.TestAgainstListOfPatterns(pattern));
        }

        [TestMethod]
        [DataRow(@"c\file.ext", @"c/((**))((*)).ext", @"d/{{2}}.new", @"d/file.new")]
        [DataRow(@"c\folder/file.ext", @"c/((**))((*)).ext", @"d/{{2}}.new", @"d/file.new")]
        [DataRow(@"c\folder/file.ext", @"c/((**))((movie||file)).ext", @"d/{{2}}.new", @"d/file.new")]
        [DataRow(@"c\folder/file.ext", @"c/((**))((movie||derp)).ext", @"d/{{2}}.new", @"d/2.new")]
        [DataRow(@"c\folder/file.ext", @"c/((**))((*)).ext", @"{{0}}", @"c\folder/file.ext")]
        [DataRow(@"c\folder/file.ext", @"c/((**))((*)).ext", @"nop", @"nop")]
        [DataRow(@"c\folder/file.ext", @"c/f((old))er/((?))ile.**", @"{{1}} {{2}}k", @"old fk")]
        [DataRow(@"c\folder/file.ext", @"c/((*))/**", @"{{1}} {{name}}", @"folder name")]
        public void ReplacePlaceHoldersInPathRegex_Test(string path, string pattern, string replacementString, string expected) {
            var match = new Regex(pattern.PathWildCardToRegex()).Match(path);
            Assert.AreEqual(expected, replacementString.ReplacePlaceHolders(s => {
                if (match.Success && match.Groups[s].Success) {
                    return match.Groups[s].Value;
                }
                return s;
            }), pattern);
        }
    }
}
