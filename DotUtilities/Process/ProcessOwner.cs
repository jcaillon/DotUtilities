#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProcessUser.cs) is part of DotUtilities.
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

using System.Security;

namespace DotUtilities.Process {

    /// <summary>
    /// Represents the user of a process.
    /// </summary>
    public class ProcessOwner {

        /// <summary>
        /// The domaine name.
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The user password.
        /// </summary>
        public SecureString Password { get; set; }

        /// <summary>
        /// Set the <see cref="Password"/>, highly unsecure, defeats the purpose of <see cref="SecureString"/>...
        /// </summary>
        /// <param name="clearPassword"></param>
        public void SetPasswordInClearText(string clearPassword) {
            Password = new SecureString();
            for (int x = 0; x < clearPassword.Length; x++) {
                Password.AppendChar(clearPassword[x]);
            }
        }
    }
}
