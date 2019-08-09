#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProcessArgsTest.cs) is part of Oetools.Utilities.Test.
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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oetools.Utilities.Lib;

namespace Oetools.Utilities.Test.Lib {

    [TestClass]
    public class ProcessArgsTest {

        [TestMethod]
        [DataRow(null, true, null)]
        [DataRow(@"", true, @"""""")]
        [DataRow("mot", true, @"mot", DisplayName = "does not quote if unnecessary")]
        [DataRow("mot", false, @"mot")]
        [DataRow("mot deux", true, @"""mot deux""", DisplayName = "necessary quotes because of the space")]
        [DataRow("mot deux", false, @"""mot deux""")]
        [DataRow("mot\"deux\"", true, @"mot\""deux\""", DisplayName = "not surrounding quotes, escaped quote with backslash")]
        [DataRow("mot\"deux\"", false, @"mot\""deux\""")]
        [DataRow("mot \"deux\"", true, @"""mot \""deux\""""")]
        [DataRow("mot \"deux\"", false, @"""mot \""deux\""""")]
        [DataRow("mot\\deux", true, @"mot\deux", DisplayName = "on windows no need to escape a \\ in the middle")]
        [DataRow("mot\\deux", false, @"mot\\deux", DisplayName = "on linux yes")]
        [DataRow("mot \\deux", true, @"""mot \deux""")]
        [DataRow("mot \\deux", false, @"""mot \\deux""")]
        [DataRow("mot \\\"deux", true, @"""mot \\\""deux""", DisplayName = "special case 1 on windows, need 3 backslash here")]
        [DataRow("mot\\", true, @"mot\", DisplayName = "special case 2 on windows no need to escape a final backslash if no surrounding quotes")]
        [DataRow("mot deux\\", true, @"""mot deux\\""", DisplayName = "special case 3 on windows, need to escape the last backslash because of the surrounding quotes")]
        public void ToCliArg(string input, bool isWindows, string expected) {
            Assert.AreEqual(expected, ProcessArgs.ToCliArg(input, isWindows));
        }

        [TestMethod]
        [DataRow("arg1", @"arg1")]
        [DataRow("arg1 \"arg2\" -opt:\"my \"\"quoted\"\" value\" -opt2", "arg1;arg2;-opt:my \"quoted\" value;-opt2")]
        [DataRow("arg1 \"arg2\" \"-opt:my \"\"quoted\"\" value\" -opt2", "arg1;arg2;-opt:my \"quoted\" value;-opt2")]
        [DataRow("arg1 a\"r\"g2 \"-opt:my \"\"quoted\"\" value\" -opt2", "arg1;arg2;-opt:my \"quoted\" value;-opt2")]
        public void AppendFromQuotedArgs(string input, string expected) {
            Assert.AreEqual(expected, string.Join(";",  new ProcessArgs().AppendFromQuotedArgs(input)));
        }

        [TestMethod]
        public void Append() {
            var args = new ProcessArgs();
            Assert.AreEqual(0, args.Count());
            args.Append("arg 1");
            args.Append((string) null);
            Assert.AreEqual(1, args.Count());
            args.Append(new ProcessArgs().Append("arg2").Append("arg 3"));
            args.Append((ProcessArgs) null);
            Assert.AreEqual(3, args.Count());
            args.Append(4, "arg5", null, "arg6");
            Assert.AreEqual(6, args.Count());
            args.Append(new [] {"ar\"g\"7", "arg8"});
            args.Append((string[]) null);
            Assert.AreEqual(8, args.Count());

            Assert.AreEqual("\"arg 1\" arg2 \"arg 3\" 4 arg5 arg6 ar\"\"g\"\"7 arg8", args.ToString());
            Assert.AreEqual("\"arg 1\" arg2 \"arg 3\" 4 arg5 arg6 ar\\\"g\\\"7 arg8", args.ToCliArgs());
        }

    }
}
