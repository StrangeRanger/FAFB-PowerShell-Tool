using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAFB_PowerShell_Tool.Tests
{
    [TestClass]
    public class PowerShellExecutorTest
    {
        [TestMethod]
        public void ThrowArgumentNullExceptionWhenCommandTextIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => PowerShellExecutor.Execute(null!));
        }

        [TestMethod]
        public void ThrowArgumentExceptionWhenCommandTextIsWhitespace()
        {
            Assert.ThrowsException<ArgumentException>(() => PowerShellExecutor.Execute(""));
            Assert.ThrowsException<ArgumentException>(() => PowerShellExecutor.Execute(" "));
        }
    }
}
