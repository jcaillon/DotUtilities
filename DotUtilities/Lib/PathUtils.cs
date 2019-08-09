#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (PathUtils.cs) is part of Oetools.Utilities.
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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Oetools.Utilities.Lib.Extension;

namespace Oetools.Utilities.Lib {

    /// <summary>
    /// Class that exposes utility methods.
    /// </summary>
    public static partial class Utils {

        /// <summary>
        /// Adds an extension to the file path given if the file does not have an extension already.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public static string AddFileExtention(this string path, string fileExtension) {
            return string.IsNullOrEmpty(path) || Path.HasExtension(path) ? path : $"{path}{fileExtension}";
        }

        /// <summary>
        /// Returns the home directory on windows (the user directory) and linux (/home/user).
        /// </summary>
        /// <returns></returns>
        public static string GetHomeDirectory() {
            var homePath = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrEmpty(homePath) && IsRuntimeWindowsPlatform) {
                homePath = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
                if (string.IsNullOrEmpty(homePath)) {
                    homePath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
                    if (Environment.OSVersion.Version.Major >= 6) {
                        homePath = Directory.GetParent(homePath).ToString();
                    }
                }
            }
            return homePath;
        }

        /// <summary>
        /// Enumerable to file list.
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static PathList<T> ToFileList<T>(this IEnumerable<T> list) where T : IPathListItem {
            var output = new PathList<T>();
            output.TryAddRange(list);
            return output;
        }

        /// <summary>
        /// Returns true if two path are equals
        /// </summary>
        /// <param name="path"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static bool PathEquals(this string path, string path2) {
            if (path == null || path2 == null) {
                return path == null && path2 == null;
            }
            return path.Equals(path2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Test if two paths are on the same drive (for instance D:\folder and D:\file.ext are on the same drive D:),
        /// if we have no way of knowing (for instance, if
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static bool ArePathOnSameDrive(string path1, string path2) {
            if (!IsRuntimeWindowsPlatform) {
                return true;
            }
            if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2)) {
                return true;
            }
            if (path1.Length < 2 || path1[1] != Path.VolumeSeparatorChar) {
                return true;
            }
            if (path2.Length < 2 || path2[1] != Path.VolumeSeparatorChar) {
                return true;
            }
            return path1[0] == path2[0];
        }

        /// <summary>
        /// Make sure to trim the ending "\" or "/"
        /// </summary>
        public static string TrimEndDirectorySeparator(this string path) {
            if (string.IsNullOrEmpty(path)) {
                return path;
            }
            return path[path.Length - 1] == '\\' || path[path.Length - 1] == '/' ? path.TrimEnd('\\', '/') : path;
        }

        /// <summary>
        /// Make sure to trim the starting "\" or "/"
        /// </summary>
        public static string TrimStartDirectorySeparator(this string path) {
            if (string.IsNullOrEmpty(path)) {
                return path;
            }
            return path[0] == '\\' || path[0] == '/' ? path.TrimStart('\\', '/') : path;
        }

        /// <summary>
        /// Transform a relative to an absolute path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="currentDirectory"></param>
        /// <returns></returns>
        public static string ToAbsolutePath(this string path, string currentDirectory = null) {
            if (IsPathRooted(path)) {
                return path;
            }
            return Path.Combine(currentDirectory ?? Directory.GetCurrentDirectory(), path);
        }

        /// <summary>
        /// Transforms an absolute path into a relative one
        /// </summary>
        /// <param name="absolute"></param>
        /// <param name="pathToDelete"></param>
        /// <param name="startAtDot"></param>
        /// <returns></returns>
        public static string ToRelativePath(this string absolute, string pathToDelete, bool startAtDot = false) {
            if (string.IsNullOrEmpty(absolute) || string.IsNullOrEmpty(pathToDelete)) {
                return absolute;
            }
            var relative = absolute.Replace(pathToDelete, "");

            if (relative.Length == absolute.Length) {
                return absolute;
            }
            return startAtDot ? $".{Path.DirectorySeparatorChar}{relative.TrimStartDirectorySeparator()}" : relative.TrimStartDirectorySeparator();
        }

        /// <summary>
        /// Gets a messy path (can be valid or not) and returns a cleaned path, trimming ending dir sep char.
        /// Uses windows style separator.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ToCleanRelativePathWin(this string path) {
            if (string.IsNullOrEmpty(path)) {
                return path;
            }
            var newPath = path.Trim().TrimStartDirectorySeparator().ToCleanPath();
            return Path.DirectorySeparatorChar == '\\' ? newPath : newPath.Replace('/', '\\');
        }

        /// <summary>
        /// Gets a messy path (can be valid or not) and returns a cleaned path, trimming ending dir sep char.
        /// Uses unix style separator.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ToCleanRelativePathUnix(this string path) {
            if (string.IsNullOrEmpty(path)) {
                return path;
            }
            var newPath = path.Trim().TrimStartDirectorySeparator().ToCleanPath();
            return Path.DirectorySeparatorChar == '/' ? newPath : newPath.Replace('\\', '/');
        }

        /// <summary>
        /// Gets a messy path (can be valid or not) and returns a cleaned path, trimming ending dir sep char
        /// TODO : also replace stuff like /./ or /../
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ToCleanPath(this string path) {
            if (string.IsNullOrEmpty(path)) {
                return path;
            }
            path = path.Trim();
            string newPath;
            bool isWindows = IsRuntimeWindowsPlatform;
            bool startDoubleSlash = false;
            if (isWindows) {
                newPath = path.Replace('/', '\\');
                startDoubleSlash = path.Length >= 2 && newPath[0] == '\\' && newPath[1] == '\\';
                if (newPath.Length > 0 && newPath[0] == '\\' && !startDoubleSlash) {
                    // replace / by C:\
                    newPath = $"{Path.GetFullPath(@"/")}{newPath.Substring(1)}";
                }
            } else {
                newPath = path.Replace('\\', '/');
            }
            // clean consecutive /
            int originalLength;
            do {
                originalLength = newPath.Length;
                newPath = newPath.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", $"{Path.DirectorySeparatorChar}");
            } while (originalLength != newPath.Length);
            if (!string.IsNullOrEmpty(newPath) && startDoubleSlash) {
                newPath = $"{Path.DirectorySeparatorChar}{newPath}";
            }
            return newPath.TrimEndDirectorySeparator();
        }

        /// <summary>
        /// Read all the text of a file in one go, same as File.ReadAllText expect it's truly a read only function
        /// </summary>
        public static string ReadAllText(string path, Encoding encoding = null) {
            try {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    using (var textReader = new StreamReader(fileStream, encoding ?? TextEncodingDetect.GetFileEncoding(path))) {
                        return textReader.ReadToEnd();
                    }
                }
            } catch (Exception e) {
                throw new Exception($"Can not read the file {path.PrettyQuote()}.", e);
            }
        }

        /// <summary>
        /// Delete a dir, recursively, doesn't throw an exception if it does not exists
        /// </summary>
        public static bool DeleteDirectoryIfExists(string path, bool recursive) {
            if (string.IsNullOrEmpty(path)) {
                return false;
            }
            try {
                Directory.Delete(path, recursive);
            } catch (DirectoryNotFoundException) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Deletes the file if it exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool DeleteFileIfNeeded(string path) {
            if (string.IsNullOrEmpty(path)) {
                return false;
            }
            try {
                File.Delete(path);
            } catch (DirectoryNotFoundException) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Creates the directory for the given file path if it doesn't exists, can apply attributes.
        /// </summary>
        public static bool CreateDirectoryForFileIfNeeded(string filePath, FileAttributes attributes = FileAttributes.Directory) {
            var dir = string.IsNullOrEmpty(filePath) ? null : Path.GetDirectoryName(filePath);
            return !string.IsNullOrEmpty(dir) && CreateDirectoryIfNeeded(dir, attributes);
        }

        /// <summary>
        /// Creates the directory if it doesn't exists, can apply attributes.
        /// </summary>
        public static bool CreateDirectoryIfNeeded(string path, FileAttributes attributes = FileAttributes.Directory) {
            if (Directory.Exists(path)) {
                return false;
            }
            var dirInfo = Directory.CreateDirectory(path);
            dirInfo.Attributes |= attributes;
            return true;
        }

        /// <summary>
        /// List all the folders in a folder
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="options"></param>
        /// <param name="excludePatterns">should be regex expressions</param>
        /// <param name="excludeHidden"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static IEnumerable<string> EnumerateAllFolders(string folderPath, SearchOption options = SearchOption.AllDirectories, List<string> excludePatterns = null, bool excludeHidden = false, CancellationToken? cancelToken = null) {
            List<Regex> excludeRegexes = null;
            if (excludePatterns != null) {
                excludeRegexes = excludePatterns.Select(s => new Regex(s)).ToList();
            }
            var hiddenDirList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var folderStack = new Stack<string>();
            folderStack.Push(folderPath);
            while (folderStack.Count > 0) {
                cancelToken?.ThrowIfCancellationRequested();
                foreach (var dir in Directory.EnumerateDirectories(folderStack.Pop(), "*", SearchOption.TopDirectoryOnly)) {
                    if (hiddenDirList.Contains(dir)) {
                        continue;
                    }
                    if (excludeHidden && new DirectoryInfo(dir).Attributes.HasFlag(FileAttributes.Hidden)) {
                        hiddenDirList.Add(dir);
                        continue;
                    }
                    if (excludeRegexes != null && excludeRegexes.Any(r => r.IsMatch(dir))) {
                        continue;
                    }
                    if (options == SearchOption.AllDirectories) {
                        folderStack.Push(dir);
                    }
                    yield return dir;
                }
            }
        }

        /// <summary>
        /// List all the files in a folder
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="options"></param>
        /// <param name="excludePatterns">should be regex expressions</param>
        /// <param name="excludeHiddenFolders"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static IEnumerable<string> EnumerateAllFiles(string folderPath, SearchOption options = SearchOption.AllDirectories, List<string> excludePatterns = null, bool excludeHiddenFolders = false, CancellationToken? cancelToken = null) {
            List<Regex> excludeRegexes = null;
            if (excludePatterns != null) {
                excludeRegexes = excludePatterns.Select(s => new Regex(s)).ToList();
            }
            var folderStack = new Stack<string>();
            folderStack.Push(folderPath);
            while (folderStack.Count > 0) {
                cancelToken?.ThrowIfCancellationRequested();
                var folder = folderStack.Pop();
                foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)) {
                    if (excludeRegexes != null && excludeRegexes.Any(r => r.IsMatch(file))) {
                        continue;
                    }
                    yield return file;
                }
                if (options == SearchOption.AllDirectories) {
                    foreach (var subfolder in EnumerateAllFolders(folder, SearchOption.TopDirectoryOnly, excludePatterns, excludeHiddenFolders)) {
                        folderStack.Push(subfolder);
                    }
                }
            }
        }

        /// <summary>
        ///     Reads all the line of either the filePath (if the file exists) or from byte array dataResources,
        ///     Apply the action toApplyOnEachLine to each line
        ///     Uses encoding as the Encoding to read the file or convert the byte array to a string
        ///     Uses the char # as a comment in the file
        /// </summary>
        public static bool ForEachLine(string filePath, byte[] dataResources, Action<int, string> toApplyOnEachLine, Encoding encoding = null, Action<Exception> onException = null) {
            encoding = encoding ?? TextEncodingDetect.GetFileEncoding(filePath);
            var wentOk = true;
            try {
                SubForEachLine(filePath, dataResources, toApplyOnEachLine, encoding);
            } catch (Exception e) {
                wentOk = false;
                onException?.Invoke(e);

                // read default file, if it fails then we can't do much but to throw an exception anyway...
                if (dataResources != null) {
                    SubForEachLine(null, dataResources, toApplyOnEachLine, encoding);
                }
            }

            return wentOk;
        }

        private static void SubForEachLine(string filePath, byte[] dataResources, Action<int, string> toApplyOnEachLine, Encoding encoding) {
            // to apply on each line
            void Action(TextReader reader) {
                var i = 0;
                string line;
                while ((line = reader.ReadLine()) != null) {
                    if (line.Length > 0) {
                        var idx = line.IndexOf('#');
                        toApplyOnEachLine(i, idx > -1 ? line.Substring(0, idx) : idx == 0 ? string.Empty : line);
                    }
                    i++;
                }
            }

            // either read from the file or from the byte array
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath)) {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (var reader = new StreamReader(fileStream, encoding)) {
                        Action(reader);
                    }
                }
            } else if (dataResources != null) {
                using (var reader = new StringReader(encoding.GetString(dataResources))) {
                    Action(reader);
                }
            }
        }

        /// <summary>
        /// Returns a temporary directory for this application
        /// </summary>
        /// <returns></returns>
        public static string CreateTempDirectory(string subfolder = null) {
            var tmpDir = Path.Combine(Path.GetTempPath(), ".oe_tmp");
            if (!string.IsNullOrEmpty(subfolder)) {
                tmpDir = Path.Combine(tmpDir, subfolder);
            }
            CreateDirectoryIfNeeded(tmpDir);
            return tmpDir;
        }

        /// <summary>
        /// Returns a random file name (can be used for folder aswell)
        /// </summary>
        /// <returns></returns>
        public static string GetRandomName() {
            return $"{DateTime.Now:fff}{Path.GetRandomFileName()}";
        }

        /// <summary>
        ///     Replaces all invalid characters found in the provided name
        /// </summary>
        /// <param name="fileName">A file name without directory information</param>
        /// <param name="replacementChar"></param>
        /// <returns></returns>
        public static string ToValidLocalFileName(this string fileName, char replacementChar = '_') {
            return ReplaceAllChars(fileName, Path.GetInvalidFileNameChars(), replacementChar);
        }

        private static string ReplaceAllChars(string str, char[] oldChars, char newChar) {
            var sb = new StringBuilder(str);
            foreach (var c in oldChars)
                sb.Replace(c, newChar);
            return sb.ToString();
        }

        /// <summary>
        /// Returns the longest valid (and existing) directory in a string, return the current directory if nothing matches
        /// </summary>
        /// <remarks>
        /// for instance
        /// - C:\windows\(any|thing)\(.*)
        /// will return
        /// - C:\windows
        /// and
        /// - **.p
        /// will return
        /// - the current directory
        /// </remarks>
        /// <param name="inputWildCardPath"></param>
        /// <returns></returns>
        public static string GetLongestValidDirectory(string inputWildCardPath) {
            inputWildCardPath = inputWildCardPath.ToCleanPath();
            string outputDirectory = inputWildCardPath;
            while (!Directory.Exists(outputDirectory)) {
                var idxOfLastSep = outputDirectory.LastIndexOf(Path.DirectorySeparatorChar);
                if (idxOfLastSep < 0) {
                    return null;
                }
                outputDirectory = outputDirectory.Substring(0, idxOfLastSep);
            }
            return outputDirectory;
        }

        /// <summary>
        ///     Allows to test if a string matches one of the listOfPattern (wildcards) in the list of patterns,
        ///     Ex : "file.xml".TestAgainstListOfPatterns("*.xls;*.com;*.xml") return true
        ///     Ex : "path/file.xml".TestAgainstListOfPatterns("*.xls;*.com;*.xml") return false!
        /// </summary>
        public static bool TestAgainstListOfPatterns(this string source, string listOfPattern) {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(listOfPattern)) {
                return false;
            }
            return listOfPattern.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToList().Exists(s => new Regex(s.PathWildCardToRegex()).IsMatch(source));
        }

        /// <summary>
        ///     Allows to test if a string matches one of the listOfPattern (wildcards) in the list of patterns,
        ///     Ex : "file.xml".TestAgainstListOfPatterns("*.xls;*.com;*.xml") return true
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static bool TestFileNameAgainstListOfPatterns(this string filePath, string listOfPattern) {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(listOfPattern)) {
                return false;
            }
            return listOfPattern.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToList().Exists(s => new Regex(s.PathWildCardToRegex()).IsMatch(Path.GetFileName(filePath)));
        }

        /// <summary>
        ///     Allows to transform a matching string using **, * and ? (wildcards) into a valid regex expression
        ///     it escapes regex special char so it will work as you expect!
        ///     Ex: foo*.xls? will become ^foo.*\.xls.$
        ///     - ** matches any char any nb of time (greedy match! allows to do stuff like C:\((**))((*)).txt)
        ///     - * matches only non path separators any time
        ///     - ? matches non path separators 1 time
        ///     - (( will be transformed into open capturing parenthesis
        ///     - )) will be transformed into close capturing parenthesis
        ///     - || will be transformed into |
        /// </summary>
        /// <param name="pattern"></param>
        /// <remarks>
        /// validate the pattern first with <see cref="Utils.ValidatePathWildCard"/> to make sure the (( and )) are legit
        /// </remarks>
        /// <returns></returns>
        public static string PathWildCardToRegex(this string pattern) {
            if (string.IsNullOrEmpty(pattern)) {
                return null;
            }
            pattern = Regex.Escape(pattern.Replace("\\", "/"))
                .Replace(@"\(\(", @"(")
                .Replace(@"\)\)", @")")
                .Replace(@"\|\|", @"|")
                .Replace(@"/", @"[\\/]")
                .Replace(@"\*\*", ".*?")
                .Replace(@"\*", @"[^\\/]*")
                .Replace(@"\?", @"[^\\/]")
                ;
            return $"^{pattern}$";
        }

        /// <summary>
        /// - Test if the path wild card has correct matches &lt; &gt; place holders
        /// - Test if the path contains any invalid characters
        /// </summary>
        /// <remarks>We need this to know if the new Regex() will fail with this PathWildCard or not</remarks>
        /// <param name="pattern"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public static void ValidatePathWildCard(string pattern) {
            if (string.IsNullOrEmpty(pattern)) {
                throw new Exception("The path is null or empty.");
            }
            foreach (char c in Path.GetInvalidPathChars()) {
                if (c == '*' || c == '?' || c == '|') {
                    continue;
                }
                if (pattern.IndexOf(c) >= 0) {
                    throw new Exception($"Illegal character path {c} at column {pattern.IndexOf(c)}.");
                }
            }
            pattern.ValidatePlaceHolders("((", "))");
        }

        /// <summary>
        /// Equivalent to <see cref="Path.IsPathRooted"/> but throws no exceptions
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsPathRooted(string path) {
            try {
                return Path.IsPathRooted(path);
            } catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Returns the Md5 print of a file as a string
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMd5FromFilePath(string filePath) {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(filePath)) {
                    StringBuilder sBuilder = new StringBuilder();
                    foreach (var b in md5.ComputeHash(stream)) {
                        sBuilder.Append(b.ToString("x2"));
                    }
                    // Return the hexadecimal string
                    return sBuilder.ToString();
                }
            }
        }

    }
}
