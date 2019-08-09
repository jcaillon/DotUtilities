#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProcessIoTest.cs) is part of Oetools.Utilities.Test.
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oetools.Utilities.Lib;

namespace Oetools.Utilities.Test.Lib {
    [TestClass]
    public class ProcessIoNoWaitTest {
        private static string _testFolder;

        private static string TestFolder => _testFolder ?? (_testFolder = TestHelper.GetTestFolder(nameof(ProcessIoNoWaitTest)));

        [TestMethod]
        public void OeExecution_Test_WaitFor_with_cancel_source() {
            using (var process = new ProcessIoNoWait(@"ping")) {
                process.ExecuteNoWait(new ProcessArgs().Append("127.0.0.1", "-n", "1000"));
                using (var cancel = new CancellationTokenSource()) {
                    process.CancelToken = cancel.Token;
                    process.WaitForExit(100);
                    Assert.IsFalse(process.Killed, "not killed");
                    process.WaitForExit(100);
                    Assert.IsFalse(process.Killed, "not killed");
                    Task.Factory.StartNew(() => {
                        Thread.Sleep(100);
                        cancel.Cancel();
                    });
                    var d = DateTime.Now;
                    process.WaitForExit(5000);
                    Assert.IsTrue(process.Killed, "killed");
                    Assert.IsTrue(DateTime.Now.Subtract(d).TotalMilliseconds < 3000, "it should have waited for the cancel and not for 5000ms (note that it has a rough precision...)");
                }
            }
        }
    }
}
