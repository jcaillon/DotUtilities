#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (DefaultValueMethodAttribute.cs) is part of DotUtilities.
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

namespace DotUtilities.Attributes {

    /// <summary>
    /// Special attribute that allows to know the name of the method that should be used in order to get a property default value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultValueMethodAttribute : Attribute {

        /// <summary>
        /// The name of the method to use in order to get the default value for this property.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="methodName">The name of the method that returns the default value of a property.</param>
        public DefaultValueMethodAttribute(string methodName) {
            MethodName = methodName;
        }
    }
}
