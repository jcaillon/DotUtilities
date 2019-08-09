#region header

// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (Resources.cs) is part of Oetools.Utilities.
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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Oetools.Utilities.Lib.Extension;

namespace Oetools.Utilities.Lib {

    /// <summary>
    /// This class helps accessing embedded resources.
    /// </summary>
    public class ResourcesHelper {

        private Assembly _assembly;

        /// <summary>
        /// New instance.
        /// </summary>
        /// <code>
        /// new ResourcesHelper(Assembly.GetAssembly(typeof(OpenedgeResources)));
        /// </code>
        /// <param name="assembly"></param>
        public ResourcesHelper(Assembly assembly) {
            _assembly = assembly;
        }

        /// <summary>
        /// Gets the byte array of a resource.
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] GetBytesFromResource(string resourcePath) {
            using (Stream resFilestream = _assembly.GetManifestResourceStream(resourcePath)) {
                if (resFilestream == null) {
                    throw new Exception($"Unknown dll resource: {resourcePath.PrettyQuote()}.");
                }
                var output = new byte[resFilestream.Length];
                resFilestream.Read(output, 0, output.Length);
                return output;
            }
        }

        /// <summary>
        /// Gets the byte array of the file in a zipped resource.
        /// </summary>
        /// <param name="zipResourcePath"></param>
        /// <param name="entryFileName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] GetBytesFromZippedResource(string zipResourcePath, string entryFileName) {
            using (Stream resFilestream = _assembly.GetManifestResourceStream(zipResourcePath)) {
                if (resFilestream == null) {
                    throw new Exception($"Unknown zipped dll resource: {zipResourcePath.PrettyQuote()}.");
                }
                var zippedBytes = new byte[resFilestream.Length];
                resFilestream.Read(zippedBytes, 0, zippedBytes.Length);
                using (var zippedStream = new MemoryStream(zippedBytes)) {
                    using (var archive = new ZipArchive(zippedStream)) {
                        var entry = entryFileName == null ? archive.Entries.FirstOrDefault() : archive.Entries.FirstOrDefault(e => e.FullName.Equals(entryFileName));
                        if (entry == null) {
                            throw new Exception($"Unknown entry {entryFileName.PrettyQuote()} in zipped dll resource: {zipResourcePath.PrettyQuote()}.");
                        }
                        using (var unzippedEntryStream = entry.Open()) {
                            using (var ms = new MemoryStream()) {
                                unzippedEntryStream.CopyTo(ms);
                                return ms.ToArray();
                            }
                        }
                    }
                }
            }
        }
    }
}
