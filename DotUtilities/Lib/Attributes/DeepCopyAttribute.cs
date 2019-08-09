#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (DeepCopy.cs) is part of Oetools.Utilities.
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

namespace Oetools.Utilities.Lib.Attributes {

    /// <summary>
    /// Special attribute that allows to decide wether or not properties should be written when using object deep copy
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DeepCopyAttribute : Attribute {

        /// <summary>
        /// Do not replace the value of this property
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DeepCopyAttribute() { }
    }
}
