#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (AttributesExtensions.cs) is part of Oetools.Utilities.Test.
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

using System.Xml.Serialization;
using DotUtilities.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotUtilities.Test.Extensions {

    [TestClass]
    public class AttributesExtensions {

        [TestMethod]
        public void GetAttributeFrom_Test() {
            Assert.AreEqual("NameOfProperty1", typeof(Obj1).GetAttributeFrom<XmlElementAttribute>(nameof(Obj1.Property1)).ElementName);
            Assert.AreEqual(null, typeof(Obj1).GetAttributeFrom<XmlElementAttribute>("_field1"));
        }

        [TestMethod]
        public void GetXmlName_Test() {
            Assert.AreEqual("NameOfProperty1", typeof(Obj1).GetXmlName(nameof(Obj1.Property1)));
            Assert.AreEqual("NameOfProperty2", typeof(Obj1).GetXmlName(nameof(Obj1.Property2)));
        }

        [TestMethod]
        public void GetXmlName_Test_root() {
            Assert.AreEqual("NameOfObj1", typeof(Obj1).GetXmlName());
        }

        [XmlRoot("NameOfObj1")]
        private class Obj1 {

            [XmlElement("NameOfProperty1")]
            public string Property1 {
                get => _field1;
                set => _field1 = value;
            }

            [XmlAttribute("NameOfProperty2")]
            public string Property2 { get; set; }

            [XmlElement("NameOfField1")]
            private string _field1;
        }

    }
}
