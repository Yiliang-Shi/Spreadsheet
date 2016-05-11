using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System.Collections.Generic;
using SS;
namespace PS5Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadBadFile1()
        {
            Spreadsheet test = new Spreadsheet("BadFile2.txt", x => true, x=>x, "j");

        }
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestReadBadFile2()
        {
            Spreadsheet test = new Spreadsheet("TextFile1.txt", x => true, x => x,  "j");

        }
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestVariableNameException()
        {
            Spreadsheet test = new Spreadsheet();
            test.SetContentsOfCell("ab", "This is a String");
           
        }
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestVariableNameException2()
        {
            Spreadsheet test = new Spreadsheet();
            test.SetContentsOfCell("12", "This is a String");

        }
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestVariableNameException3()
        {
            Spreadsheet test = new Spreadsheet();
            test.SetContentsOfCell("_a12", "This is a String");

        }
        [ExpectedException(typeof(InvalidNameException))]
        public void TestVariableNameException4()
        {
            Spreadsheet test = new Spreadsheet();
            test.SetContentsOfCell("a12", "=1B+3");

        }

        [TestMethod]
        public void TestChanged()
        {
            Spreadsheet test = new Spreadsheet();
            Assert.IsFalse(test.Changed);
            test.SetContentsOfCell("ab12", "This is a String");
            Assert.IsTrue(test.Changed);
            test.Save("text3.xml");
            Assert.IsFalse(test.Changed);
            test.SetContentsOfCell("ab12", "This is a String");
            Assert.IsTrue(test.Changed);
        }

        [TestMethod]
        public void TestGetVersion()
        {
            Spreadsheet test = new Spreadsheet();
            test.SetContentsOfCell("ab12", "This is a String");
            test.SetContentsOfCell("ab123", "2");
            test.SetContentsOfCell("ab1234", "=2+ab123");
            test.Save("test2.xml");
            Assert.AreEqual("default", test.GetSavedVersion("test2.xml"));
            Spreadsheet test2 = new Spreadsheet("test2.xml", x => true, x => x,  "V1");
            test2.Save("test2.xml");
            Assert.AreEqual("V1", test.GetSavedVersion("test2.xml"));
        }
        /// <summary>
        /// Tests saving. Manually check format since code to ensure format is likely to be
        /// more error prone than the actual xml file
        /// </summary>
        [TestMethod]
        public void TestSave()
        {
            Spreadsheet test = new Spreadsheet();
            test.SetContentsOfCell("ab12", "This is a String");
            test.SetContentsOfCell("ab123", "2");
            test.SetContentsOfCell("ab1234", "=2+ab123");
            test.Save("test.xml");

        }
        /// <summary>
        /// Huge blackbox test. If this pass, code fulfills requirements. Other tests are simple portions of this
        /// and are only usefully for isolating exact problems
        /// </summary>
        [TestMethod]
        public void TestReadAndSetCellContent()
        {
            Spreadsheet test = new Spreadsheet();
            test.SetContentsOfCell("ab12", "This is a String");
            test.SetContentsOfCell("ab123", "2");
            test.SetContentsOfCell("ab1234", "=2+ab123");
            test.Save("test.xml");

            Spreadsheet test2 = new Spreadsheet("test.xml", x => true, x => x, "V1");
            Assert.AreEqual("default", test2.GetSavedVersion("test.xml"));
            Assert.AreEqual("V1", test2.Version);
            Assert.IsTrue(test2.IsValid("dfafasfdasadf"));
            Assert.AreEqual("V1", test2.Version);
            Assert.AreEqual("asdfafda", test2.Normalize("asdfafda"));

            ISet <string> expected = new HashSet<string>();
            expected.Add("ab12");
            expected.Add("ab123");
            expected.Add("ab1234");
            Assert.IsTrue(expected.SetEquals(new HashSet<string>(test2.GetNamesOfAllNonemptyCells())));

            Assert.AreEqual("This is a String", test2.GetCellContents("ab12"));
            Assert.AreEqual(2.0, test2.GetCellContents("ab123"));
            Assert.AreEqual("2+ab123", test2.GetCellContents("ab1234").ToString());

            Assert.AreEqual("This is a String", test2.GetCellValue("ab12"));
            Assert.AreEqual(2.0, test2.GetCellValue("ab123"));
            Assert.AreEqual(4.0, test2.GetCellValue("ab1234"));

            Assert.IsFalse(test2.Changed);


        }
    }
}
