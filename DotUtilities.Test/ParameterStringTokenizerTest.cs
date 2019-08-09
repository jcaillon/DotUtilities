#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ParameterStringTokenizerTest.cs) is part of Oetools.Utilities.Test.
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

using System.Text;
using DotUtilities.ParameterString;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotUtilities.Test {

    [TestClass]
    public class ParameterStringTokenizerTest {

        [DataTestMethod]
        [DataRow("-option", "oe", "-option")]
        [DataRow("\"-option\"", "oe", "-option")]
        [DataRow("\"-option", "oe", "-option")]
        [DataRow("\"-option value", "oe", "-option value")]
        [DataRow("\"-option \"value", "oe", "-option value")]
        [DataRow("\"-option\"\"value", "oe", "-option\"value")]
        [DataRow("\"-option\"-\"value", "oe", "-option-value")]
        [DataRow("value \"quoted\"", "vsve", "value;quoted")]
        [DataRow("\"value \"\"quoted\"\"\"", "ve", "value \"quoted\"")]
        [DataRow("-option \"value with spaces\"", "osve", "-option;value with spaces")]
        [DataRow("-option=\"value with spaces\"", "oe", "-option=value with spaces")]
        [DataRow("val,\"value with spaces\",\"yet another\"", "ve", "val,value with spaces,yet another")]
        [DataRow("\"val,value with spaces,yet another\"", "ve", "val,value with spaces,yet another")]
        public void Parse(string input, string tokenTypes, string csvExpected) {

            var types = new StringBuilder();
            var csv = new StringBuilder();
            var tokenizer = ParameterStringTokenizer.New(input);
            while (tokenizer.MoveToNextToken()) {
                var token = tokenizer.PeekAtToken(0);
                switch (token) {
                    case ParameterStringTokenOption _:
                        types.Append("o");
                        csv.Append(token.Value).Append(';');
                        break;
                    case ParameterStringTokenValue _:
                        types.Append("v");
                        csv.Append(token.Value).Append(';');
                        break;
                    case ParameterStringTokenWhiteSpace _:
                        types.Append("s");
                        break;
                    default:
                        types.Append("e");
                        break;
                }
            }

            Assert.AreEqual(tokenTypes, types.ToString());
            Assert.AreEqual($"{csvExpected};", csv.ToString());
        }
    }
}
