#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (Common.cs) is part of DotUtilities.
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
using System.Security.Authentication;

namespace DotUtilities.Archive.Ftp.Core {

    /*
     *  Copyright 2008 Alessandro Pilotti
     *
     *  This program is free software; you can redistribute it and/or modify
     *  it under the terms of the GNU Lesser General Public License as published by
     *  the Free Software Foundation; either version 2.1 of the License, or
     *  (at your option) any later version.
     *
     *  This program is distributed in the hope that it will be useful,
     *  but WITHOUT ANY WARRANTY; without even the implied warranty of
     *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
     *  GNU Lesser General Public License for more details.
     *
     *  You should have received a copy of the GNU Lesser General Public License
     *  along with this program; if not, write to the Free Software
     *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
     */

    internal enum ETransferMode {
        Ascii,
        Binary
    }

    internal enum ETextEncoding {
        Ascii,
        Utf8
    }

    internal class FtpReply {
        public int Code { get; set; }

        public string Message { get; set; }

        public override string ToString() {
            return string.Format("{0} {1}", Code, Message);
        }
    }

    internal class DirectoryListItem {
        public ulong Size { get; set; }

        public string SymLinkTargetPath { get; set; }

        public string Flags { get; set; }

        public string Owner { get; set; }

        public string Group { get; set; }

        public bool IsDirectory { get; set; }

        public bool IsSymLink { get; set; }

        public string Name { get; set; }

        public DateTime CreationTime { get; set; }
    }

    /// <summary>
    ///     Encapsulates the SSL/TLS algorithms connection information.
    /// </summary>
    internal class SslInfo {
        public SslProtocols SslProtocol { get; set; }

        public CipherAlgorithmType CipherAlgorithm { get; set; }

        public int CipherStrength { get; set; }

        public HashAlgorithmType HashAlgorithm { get; set; }

        public int HashStrength { get; set; }

        public ExchangeAlgorithmType KeyExchangeAlgorithm { get; set; }

        public int KeyExchangeStrength { get; set; }

        public override string ToString() {
            return SslProtocol + ", " +
                   CipherAlgorithm + " (" + CipherStrength + " bit), " +
                   KeyExchangeAlgorithm + " (" + KeyExchangeStrength + " bit), " +
                   HashAlgorithm + " (" + HashStrength + " bit)";
        }
    }

    internal class LogCommandEventArgs : EventArgs {
        public string CommandText { get; private set; }

        public LogCommandEventArgs(string commandText) {
            CommandText = commandText;
        }
    }

    internal class LogServerReplyEventArgs : EventArgs {
        public FtpReply ServerReply { get; private set; }

        public LogServerReplyEventArgs(FtpReply serverReply) {
            ServerReply = serverReply;
        }
    }
}
