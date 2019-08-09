#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (TypeExtensionsTest.cs) is part of Oetools.Utilities.Test.
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

using System.Collections.Generic;
using DotUtilities.Attributes;
using DotUtilities.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotUtilities.Test.Extensions {

    [TestClass]
    public class TypeExtensionsTest {

        [TestMethod]
        public void DeepCopyPublicProperties_Test_IgnoreAttribute() {

            var obj = new ObjBase {
                Prop1 = "string1",
                Prop2 = "second2"
            };

            var obj7 = new ObjChild {
                Prop1 = "original1",
                Prop2 = "original2",
            };

            obj.DeepCopy(obj7);

            Assert.AreEqual("string1", obj.Prop1);
            Assert.AreEqual("second2", obj.Prop2);

            Assert.AreEqual("original1", obj7.Prop1);
            Assert.AreEqual("second2", obj7.Prop2);
        }

        private class ObjBase {
            [DeepCopy(Ignore = true)]
            public virtual string Prop1 { get; set; }

            [DeepCopy(Ignore = true)]
            public virtual string Prop2 { get; set; }
        }

        private class ObjChild : ObjBase {

            public override string Prop1 { get; set; }

            [DeepCopy(Ignore = false)]
            public override string Prop2 { get; set; }
        }

        [TestMethod]
        public void DeepCopyPublicProperties_Test() {
            var instance = new ObjDeepCopy {
                Prop1 = "cool3",
                Prop2 = 10,
                Prop3 = new[] {
                    "cool6",
                    null,
                    "cool7"
                },
                Prop4 = new ObjDeepCopySub1 {
                    Prop1 = "cool8",
                    Prop2 = "cool9"
                },
                Prop5 = new List<string> {
                    "cool4",
                    null,
                    "cool5"
                },
                Prop6 = new List<ObjDeepCopySub1> {
                    new ObjDeepCopySub1 {
                        Prop1 = "cool1",
                        Prop2 = null,
                        Prop3 = new ObjDeepCopySub1 {
                            Prop1 = "cool3",
                            Prop2 = "cool4"
                        }
                    },
                    null,
                    new ObjDeepCopySub1 {
                        Prop1 = null,
                        Prop2 = "cool2"
                    }
                }
            };

            var copy = instance.GetDeepCopy();

            Assert.IsNotNull(copy);

            Assert.AreEqual("cool3", copy.Prop1);
            Assert.AreEqual(10, copy.Prop2);
            Assert.AreEqual("cool6", copy.Prop3[0]);
            Assert.AreEqual(null, copy.Prop3[1]);
            Assert.AreEqual("cool7", copy.Prop3[2]);
            Assert.AreEqual("cool8", copy.Prop4.Prop1);
            Assert.AreEqual("cool9", copy.Prop4.Prop2);
            Assert.AreEqual("cool4", copy.Prop5[0]);
            Assert.AreEqual(null, copy.Prop5[1]);
            Assert.AreEqual("cool5", copy.Prop5[2]);
            Assert.AreEqual("cool1", copy.Prop6[0].Prop1);
            Assert.AreEqual(null, copy.Prop6[0].Prop2);
            Assert.AreEqual("cool3", copy.Prop6[0].Prop3.Prop1);
            Assert.AreEqual("cool4", copy.Prop6[0].Prop3.Prop2);
            Assert.AreEqual(null, copy.Prop6[0].Prop3.Prop3);
            Assert.AreEqual(null, copy.Prop6[1]);
            Assert.AreEqual(null, copy.Prop6[2].Prop1);
            Assert.AreEqual("cool2", copy.Prop6[2].Prop2);

            copy.Prop2 = 8;
            Assert.AreEqual(8, copy.Prop2);
            Assert.AreEqual(10, instance.Prop2);

            copy.Prop6[0].Prop3.Prop1 = "nice";
            Assert.AreEqual("nice", copy.Prop6[0].Prop3.Prop1);
            Assert.AreEqual("cool3", instance.Prop6[0].Prop3.Prop1);

            copy.Prop4 = new ObjDeepCopySub1();
            Assert.AreEqual(null, copy.Prop4.Prop1);
            Assert.AreEqual("cool8", instance.Prop4.Prop1);

            copy.Prop5.Add("more");
            Assert.AreEqual(4, copy.Prop5.Count);
            Assert.AreEqual(3, instance.Prop5.Count);

            copy.Prop6[0].Prop1 = "nice";
            Assert.AreEqual("nice", copy.Prop6[0].Prop1);
            Assert.AreEqual("cool1", instance.Prop6[0].Prop1);

            copy = new ObjDeepCopy();
            instance.DeepCopy<IObjDeepCopy>(copy);
            Assert.IsNull(copy.Prop1);
            Assert.AreEqual(0, copy.Prop2);
            Assert.IsNull(copy.Prop5);
            Assert.IsNull(copy.Prop6);
            Assert.IsNotNull(copy.Prop3);
            Assert.AreEqual("cool6", copy.Prop3[0]);
            Assert.AreEqual(null, copy.Prop3[1]);
            Assert.AreEqual("cool7", copy.Prop3[2]);
            Assert.IsNotNull(copy.Prop4);
            Assert.AreEqual("cool8", copy.Prop4.Prop1);
            Assert.AreEqual("cool9", copy.Prop4.Prop2);
        }

        private interface IObjDeepCopy {
            string[] Prop3 { get; set; }
            ObjDeepCopySub1 Prop4 { get; set; }
        }

        private class ObjDeepCopy : IObjDeepCopy {
            public string Prop1 { get; set; }
            public int Prop2 { get; set; }
            public string[] Prop3 { get; set; }
            public ObjDeepCopySub1 Prop4 { get; set; }
            public List<string> Prop5 { get; set; }
            public List<ObjDeepCopySub1> Prop6 { get; set; }
        }

        private class ObjDeepCopySub1 {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
            public ObjDeepCopySub1 Prop3 { get; set; }
        }

        [TestMethod]
        public void Overload_instances() {
            var instanceGlobalDefault = new ObjDeepInherit {
                Prop1 = null,
                Prop2 = 10,
                Prop3 = new[] {
                    "cool6",
                    null,
                    "cool7"
                },
                Prop4 = new ObjDeepInheritSub {
                    Prop1 = "cool8",
                    Prop2 = "cool9",
                    Prop3 = new ObjDeepInheritSub {
                        Prop1 = null,
                        Prop2 = "cool2"
                    }
                },
                Prop5 = new List<string> {
                    "cool4",
                    null,
                    "cool5"
                },
                Prop6 = new List<ObjDeepInheritSub> {
                    new ObjDeepInheritSub {
                        Prop1 = "cool1",
                        Prop2 = null,
                        Prop3 = new ObjDeepInheritSub {
                            Prop1 = "cool3",
                            Prop2 = "cool4"
                        }
                    },
                    null,
                    new ObjDeepInheritSub {
                        Prop1 = null,
                        Prop2 = "cool2"
                    }
                }
            };
            var instanceOverload = new ObjDeepInherit {
                Prop1 = "nice3",
                Prop2 = null,
                Prop3 = new[] {
                    "nice6",
                    "nice7"
                },
                Prop4 = new ObjDeepInheritSub {
                    Prop2 = "nice9"
                },
                Prop5 = new List<string> {
                    "nice4",
                    "nice5"
                },
                Prop6 = new List<ObjDeepInheritSub> {
                    new ObjDeepInheritSub {
                        Prop1 = "nice1",
                        Prop2 = "nice2"
                    }
                }
            };

            var copy = instanceGlobalDefault.GetDeepCopy();
            instanceOverload.DeepCopy(copy);

            Assert.AreEqual(3, instanceGlobalDefault.Prop5.Count);
            Assert.AreEqual(2, instanceOverload.Prop5.Count);
            Assert.AreEqual(5, copy.Prop5.Count);

            Assert.AreEqual("nice3", copy.Prop1);
            Assert.AreEqual(10, copy.Prop2);
            Assert.AreEqual(5, copy.Prop3.Length);
            Assert.AreEqual("cool6", copy.Prop3[0]);
            Assert.AreEqual(null, copy.Prop3[1]);
            Assert.AreEqual("cool7", copy.Prop3[2]);
            Assert.AreEqual("nice6", copy.Prop3[3]);
            Assert.AreEqual("nice7", copy.Prop3[4]);
            Assert.AreEqual("cool8", copy.Prop4.Prop1);
            Assert.AreEqual("nice9", copy.Prop4.Prop2);
            Assert.AreEqual(null, copy.Prop4.Prop3.Prop1);
            Assert.AreEqual("cool2", copy.Prop4.Prop3.Prop2);
            Assert.AreEqual("cool4", copy.Prop5[0]);
            Assert.AreEqual(null, copy.Prop5[1]);
            Assert.AreEqual("cool5", copy.Prop5[2]);
            Assert.AreEqual("nice4", copy.Prop5[3]);
            Assert.AreEqual("nice5", copy.Prop5[4]);
            Assert.AreEqual("cool1", copy.Prop6[0].Prop1);
            Assert.AreEqual(null, copy.Prop6[0].Prop2);
            Assert.AreEqual("cool3", copy.Prop6[0].Prop3.Prop1);
            Assert.AreEqual("cool4", copy.Prop6[0].Prop3.Prop2);
            Assert.AreEqual(null, copy.Prop6[0].Prop3.Prop3);
            Assert.AreEqual(null, copy.Prop6[1]);
            Assert.AreEqual(null, copy.Prop6[2].Prop1);
            Assert.AreEqual("cool2", copy.Prop6[2].Prop2);
            Assert.AreEqual("nice1", copy.Prop6[3].Prop1);
            Assert.AreEqual("nice2", copy.Prop6[3].Prop2);
        }

        private class ObjDeepInherit {
            public string Prop1 { get; set; }
            public int? Prop2 { get; set; }
            public string[] Prop3 { get; set; }
            public ObjDeepInheritSub Prop4 { get; set; }
            public List<string> Prop5 { get; set; }
            public List<ObjDeepInheritSub> Prop6 { get; set; }
        }

        private class ObjDeepInheritSub {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
            public ObjDeepInheritSub Prop3 { get; set; }
        }
    }
}
