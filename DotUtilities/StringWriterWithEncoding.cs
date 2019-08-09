#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (StringWriterWithEncoding.cs) is part of DotUtilities.
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

using System.IO;
using System.Text;

namespace DotUtilities {

    /// <summary>
    /// A <see cref="StringWriter"/> class with encoding selection.
    /// </summary>
    public sealed class StringWriterWithEncoding : StringWriter {

        private readonly Encoding _encoding;

        /// <inheritdoc />
        public StringWriterWithEncoding(StringBuilder sb) : base(sb) {
            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="encoding"></param>
        public StringWriterWithEncoding(Encoding encoding) {
            _encoding = encoding;
        }

        /// <inheritdoc />
        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding) : base(sb) {
            _encoding = encoding;
        }

        /// <summary>
        /// The encoding to use to write.
        /// </summary>
        public override Encoding Encoding => _encoding;
    }
}
