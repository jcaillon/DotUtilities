#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (TypeUtilsTest.cs) is part of Oetools.Utilities.Test.
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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oetools.Utilities.Lib;
using Oetools.Utilities.Lib.Attributes;

namespace Oetools.Utilities.Test.Lib {
    [TestClass]
    public class TypeUtilsTest {
        private static string _testFolder;

        private static string TestFolder => _testFolder ?? (_testFolder = TestHelper.GetTestFolder(nameof(TypeUtilsTest)));

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
        public void ReplaceEmptyListsByNull() {
            var ojb = new ObjReplaceEmptyListsByNull {
                Prop1 = new List<Obj3>(),
                Prop2 = new List<Obj4> {
                    new Obj4 {
                        Prop1 = new List<Obj4>()
                    },
                    new Obj4 {
                        Prop1 = new List<Obj4> {
                            new Obj4()
                        }
                    }
                }
            };
            Assert.IsNotNull(ojb.Prop1);
            Assert.IsNotNull(ojb.Prop2);
            Assert.IsNotNull(ojb.Prop2[0].Prop1);
            Assert.IsNotNull(ojb.Prop2[1].Prop1);
            Utils.ReplaceEmptyListsByNull(ojb);
            Assert.IsNull(ojb.Prop1);
            Assert.IsNotNull(ojb.Prop2);
            Assert.IsNull(ojb.Prop2[0].Prop1);
            Assert.IsNotNull(ojb.Prop2[1].Prop1);
        }

        [TestMethod]
        public void SetDefaultValues_Test() {
            var ojb = new ObjGetDefault();
            ojb.Prop6 = new Obj2 {
                Prop1 = "already defined"
            };
            Utils.SetDefaultValues(ojb);
            Assert.IsNotNull(ojb.Prop1);
            Assert.IsNotNull(ojb.Prop2);
            Assert.IsNull(ojb.Prop3);
            Assert.AreEqual(10, ojb.Prop4);
            Assert.IsNull(ojb.Prop5);
            Assert.AreEqual("nice", ojb.Prop1.Prop1);
            Assert.IsNull(ojb.Prop1.Prop2);
            Assert.IsNull(ojb.Prop1.Prop3);
            Assert.IsNull(ojb.Prop2[0].Prop1);
            Assert.AreEqual("cool", ojb.Prop2[0].Prop2);
            Assert.AreEqual("already defined", ojb.Prop6.Prop1);
            
        }

        [TestMethod]
        public void ReplacePlaceHolders_SkipReplace() {
            var instance2 = new ObjSkipReplace {
                Prop1 = new Obj2 {
                    Prop1 = "cool1",
                    Prop2 = new List<Obj3> {
                        new Obj3 {
                            Prop1 = "cool2",
                            Prop2 = null
                        },
                        null,
                        new Obj3 {
                            Prop1 = null,
                            Prop2 = "cool3"
                        }
                    },
                    Prop3 = new[] {
                        "cool9", null, "cool10"
                    }
                },
                Prop2 = new List<Obj3> {
                    new Obj3 {
                        Prop1 = "cool4",
                        Prop2 = null
                    },
                    null,
                    new Obj3 {
                        Prop1 = null,
                        Prop2 = "cool5"
                    }
                },
                Prop3 = "cool6",
                Prop4 = 10,
                Prop5 = new List<string> {
                    "cool7",
                    null,
                    "cool8"
                }
            };
            Utils.ForEachPublicPropertyStringInObject(typeof(ObjSkipReplace), instance2, (t, s) => {
                return s?.Replace("cool", "nice");
            });
            Assert.AreEqual("cool1", instance2.Prop1.Prop1);
            Assert.AreEqual("cool2", instance2.Prop1.Prop2.ToList()[0].Prop1);
            Assert.AreEqual("cool3", instance2.Prop1.Prop2.ToList()[2].Prop2);
            Assert.AreEqual("cool9", instance2.Prop1.Prop3[0]);
            Assert.AreEqual("cool10", instance2.Prop1.Prop3[2]);
            Assert.AreEqual("nice4", instance2.Prop2[0].Prop1);
            Assert.AreEqual("nice5", instance2.Prop2[2].Prop2);
            Assert.AreEqual("cool6", instance2.Prop3);
            Assert.AreEqual("nice7", instance2.Prop5[0]);
            Assert.AreEqual("nice8", instance2.Prop5[2]);
        }

        [TestMethod]
        public void ReplacePlaceHoldersInAllPublicProperties_Test() {
            
            var instance = new ObjReplacePublicProp {
                Prop1 = new Obj2 {
                    Prop1 = "cool1",
                    Prop2 = new List<Obj3> {
                        new Obj3 {
                            Prop1 = "cool2",
                            Prop2 = null
                        },
                        null,
                        new Obj3 {
                            Prop1 = null,
                            Prop2 = "cool3"
                        }
                    },
                    Prop3 = new[] {
                        "cool9", null, "cool10"
                    }
                },
                Prop2 = new List<Obj3> {
                    new Obj3 {
                        Prop1 = "cool4",
                        Prop2 = null
                    },
                    null,
                    new Obj3 {
                        Prop1 = null,
                        Prop2 = "cool5"
                    }
                },
                Prop3 = "cool6",
                Prop4 = 10,
                Prop5 = new List<string> {
                    "cool7",
                    null,
                    "cool8"
                }
            };
            Utils.ForEachPublicPropertyStringInObject(typeof(ObjReplacePublicProp), instance, (t, s) => {
                return s?.Replace("cool", "nice");
            });
            Assert.AreEqual("nice1", instance.Prop1.Prop1);
            Assert.AreEqual("nice2", instance.Prop1.Prop2.ToList()[0].Prop1);
            Assert.AreEqual("nice3", instance.Prop1.Prop2.ToList()[2].Prop2);
            Assert.AreEqual("nice9", instance.Prop1.Prop3[0]);
            Assert.AreEqual("nice10", instance.Prop1.Prop3[2]);
            Assert.AreEqual("nice4", instance.Prop2[0].Prop1);
            Assert.AreEqual("nice5", instance.Prop2[2].Prop2);
            Assert.AreEqual("nice6", instance.Prop3);
            Assert.AreEqual("nice7", instance.Prop5[0]);
            Assert.AreEqual("nice8", instance.Prop5[2]);

        }

        private class ObjReplacePublicProp {
            public Obj2 Prop1 { get; set; }

            public List<Obj3> Prop2 { get; set; }

            public string Prop3 { get; set; }

            public int Prop4 { get; set; }

            public List<string> Prop5 { get; set; }
        }

        private class Obj2 {
            
            [DefaultValueMethod(nameof(GetDefaultProp1))]
            public string Prop1 { get; set; }
            public static string GetDefaultProp1() => "nice";

            public IEnumerable<Obj3> Prop2 { get; set; }

            public string[] Prop3 { get; set; }
        }

        private class Obj3 {
            public string Prop1 { get; set; }

            
            [DefaultValueMethod(nameof(GetDefaultProp2))]
            public string Prop2 { get; set; }
            public static string GetDefaultProp2() => "cool";
        }

        private class ObjSkipReplace {
            [ReplaceStringProperty(SkipReplace = true)]
            public Obj2 Prop1 { get; set; }

            public List<Obj3> Prop2 { get; set; }

            [ReplaceStringProperty(SkipReplace = true)]
            public string Prop3 { get; set; }

            public int Prop4 { get; set; }

            public List<string> Prop5 { get; set; }
        }
        
        private class ObjGetDefault {
            
            [DefaultValueMethod(nameof(GetDefaultProp1))]
            public Obj2 Prop1 { get; set; }
            public static Obj2 GetDefaultProp1() => new Obj2();
            
            [DefaultValueMethod(nameof(GetDefaultProp2))]
            public List<Obj3> Prop2 { get; set; }
            public static List<Obj3> GetDefaultProp2() => new List<Obj3> { new Obj3() };

            public string Prop3 { get; set; }
            
            [DefaultValueMethod(nameof(GetDefaultProp4))]
            public int? Prop4 { get; set; }
            public static int GetDefaultProp4() => 10;

            public List<string> Prop5 { get; set; }
            
            [DefaultValueMethod(nameof(GetDefaultProp6))]
            public Obj2 Prop6 { get; set; }
            public static Obj2 GetDefaultProp6() => new Obj2();
        }
        
        private class ObjReplaceEmptyListsByNull {
            
            public List<Obj3> Prop1 { get; set; }
            public List<Obj4> Prop2 { get; set; }

        }
        
        private class Obj4 {
            public List<Obj4> Prop1 { get; set; }
        }
    }
}