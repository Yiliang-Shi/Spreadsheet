using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System.Collections.Generic;
using SS;

namespace SpreadsheetTest
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestStringNameGetCellContent()
        {
            Spreadsheet test = new Spreadsheet();
            test.SetContentsOfCell("ab12", "This is a String");
            Assert.AreEqual("This is a String", test.GetCellContents("ab12"));
            test.Save("test.xml");
        }

    }
}
