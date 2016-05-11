using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System.Collections.Generic;
using System.Linq;

namespace FormulaTester
{
    [TestClass]
    public class FormulaUnitTest
    {
        /// <summary>
        /// Tests Valid default constructors
        /// </summary>
        [TestMethod]
        public void TestDefaultConstructorEvaluate()
        {
            Formula expr1 = new Formula("0");
            Assert.AreEqual(0.0, expr1.Evaluate(s => 1));
            Formula expr2 = new Formula("0+2");
            Assert.AreEqual(2.0, expr2.Evaluate(s => 1));
            Formula expr3 = new Formula("0+s2");
            Assert.AreEqual(1.0, expr3.Evaluate(s => 1));

        }

        [TestMethod]
        public void TestDefaultConstructorGetVariables()
        {
            Formula expr1 = new Formula("0+s+S");
            List<string> normalizdVars = new List<string>(expr1.GetVariables());
            List<string> expected = new List<string>() { "s", "S" };
            Assert.AreEqual(expected[0], normalizdVars[0]);
            Assert.AreEqual(expected[1], normalizdVars[1]);

        }

        [TestMethod]
        public void TestDefaultConstructorToString()
        {
            Formula expr1 = new Formula("0");
            Assert.AreEqual("0", expr1.ToString());

        }

        [TestMethod]
        public void TestDefaultConstructorEquals()
        {
            Formula expr1 = new Formula("0");
            Formula expr2 = new Formula("0");
            Formula expr3 = new Formula("0+0");
            Formula expr5 = null;
            Assert.IsFalse(expr2.Equals(expr3));
            Assert.IsTrue(expr1.Equals(expr2));
            Assert.IsFalse(expr3.Equals(expr5));


        }
        [TestMethod]
        public void TestDefaultConstructorEquals2()
        {
            Formula expr1 = new Formula("0");
            Formula expr2 = new Formula("0");
            Formula expr3 = new Formula("0+0");
            Formula expr4 = null;
            Formula expr5 = null;
            Assert.IsFalse(expr2 == (expr3));
            Assert.IsTrue(expr1 == (expr2));
            Assert.IsTrue(expr4 == (expr5));


        }

        [TestMethod]
        public void TestDefaultConstructorNotEquals()
        {
            Formula expr1 = new Formula("0");
            Formula expr2 = new Formula("0");
            Formula expr3 = new Formula("0+0");
            Formula expr4 = null;
            Formula expr5 = null;
            Assert.IsTrue(expr2 != (expr3));
            Assert.IsFalse(expr1 != (expr2));
            Assert.IsFalse(expr4 != (expr5));


        }

        [TestMethod]
        public void TestDefaultConstructorGetHashCodes()
        {
            Formula expr1 = new Formula("0");
            Formula expr2 = new Formula("0");
            Assert.AreEqual(expr2.GetHashCode(), expr1.GetHashCode());


        }

        /// <summary>
        /// Tests Valid Overloaded constructors
        /// </summary>
        [TestMethod]
        public void TestOverloadedConstructorEvaluate()
        {
            Formula expr1 = new Formula("0+s2", s => s.ToUpper(), s => s.Length == 2);
            Assert.AreEqual(2.0, expr1.Evaluate(s => 2));
 
        }

        [TestMethod]
        public void TestOverloadedConstructorGetVariables()
        {
            Formula expr1 = new Formula("0+s2+S2", s => s.ToUpper(), s => s.Length == 2);
            List<string> normalizdVars = new List<string>(expr1.GetVariables());
            List<string> expected = new List<string>() { "S2" };
            Assert.AreEqual(expected[0],normalizdVars[0]);

        }

        [TestMethod]
        public void TestOverloadedConstructorToString()
        {
            Formula expr1 = new Formula("0+s2+S2", s => s.ToUpper(), s => s.Length == 2);
            Assert.AreEqual("0+S2+S2", expr1.ToString());

        }

        [TestMethod]
        public void TestOverloadedConstructorEquals()
        {
            Formula expr1 = new Formula("0+s2+S2", s => s.ToUpper(), s => s.Length == 2);
            Formula expr2 = new Formula("0+S2+S2", s => s.ToUpper(), s => s.Length == 2);
            Formula expr3 = new Formula("0+S2", s => s.ToUpper(), s => s.Length == 2);
            Formula expr5 = null;
            Assert.IsFalse(expr2.Equals(expr3));
            Assert.IsTrue(expr1.Equals(expr2));
            Assert.IsFalse(expr3.Equals(expr5));


        }
        [TestMethod]
        public void TestOverloadedConstructorEquals2()
        {
            Formula expr1 = new Formula("0+s2+S2", s => s.ToUpper(), s => s.Length == 2);
            Formula expr2 = new Formula("0+S2+S2", s => s.ToUpper(), s => s.Length == 2);
            Formula expr3 = new Formula("0+S2", s => s.ToUpper(), s => s.Length == 2);
            Formula expr5 = null;
            Formula expr4 = null;
            Assert.IsFalse(expr2==(expr3));
            Assert.IsTrue(expr1==(expr2));
            Assert.IsTrue(expr4==(expr5));


        }

        [TestMethod]
        public void TestOverloadedConstructorNotEquals()
        {
            Formula expr1 = new Formula("0+s2+S2", s => s.ToUpper(), s => s.Length == 2);
            Formula expr2 = new Formula("0+S2+S2", s => s.ToUpper(), s => s.Length == 2);
            Formula expr3 = new Formula("0+S2", s => s.ToUpper(), s => s.Length == 2);
            Formula expr5 = null;
            Formula expr4 = null;
            Assert.IsTrue(expr2 != (expr3));
            Assert.IsFalse(expr1 != (expr2));
            Assert.IsFalse(expr4 != (expr5));


        }

        [TestMethod]
        public void TestOverloadedConstructorGetHashCodes()
        {
            Formula expr1 = new Formula("0+s2+S2", s => s.ToUpper(), s => s.Length == 2);
            Formula expr2 = new Formula("0+S2+S2", s => s.ToUpper(), s => s.Length == 2);
      
            Assert.AreEqual(expr2.GetHashCode(), expr1.GetHashCode());


        }
        //Evaluate testing from ps1
        [TestMethod()]
        public void TestNumber()
        {
            Formula expr1 = new Formula("5");
            Assert.AreEqual(5.0, expr1.Evaluate( s => 0));
        }

        [TestMethod()]
        public void TestVar()
        {
            Formula expr1 = new Formula("X5");
            Assert.AreEqual(13.0, expr1.Evaluate( s => 13));
        }

        [TestMethod()]
        public void TestSubract()
        {
            Formula expr1 = new Formula("18-10");
            Assert.AreEqual(8.0, expr1.Evaluate( s => 0));
        }

        [TestMethod()]
        public void Testmultiply()
        {
            Formula expr1 = new Formula("2*4");
            Assert.AreEqual(8.0, expr1.Evaluate( s => 0));
        }

        [TestMethod()]
        public void TestDivide()
        {
            Formula expr1 = new Formula("15/2");
            Assert.AreEqual(7.5, expr1.Evaluate( s => 0));
        }


        [TestMethod()]
        public void TestExpectedFormulaErrorBadLookup()
        {
            Formula expr1 = new Formula("2+X1");
            Assert.IsTrue(expr1.Evaluate(s => { throw new ArgumentException("Unknown variable"); }) is FormulaError);
        }

      

        [TestMethod()]
        public void TestComplexPrecedence()
        {
            Formula expr1 = new Formula("2+3*5+(3+4*8)*5+2");
            Assert.AreEqual(194.0, expr1.Evaluate( s => 0));
        }

        [TestMethod()]
        public void TestDivideby0()
        {
            Formula expr1 = new Formula("2/0");

            Assert.IsTrue(expr1.Evaluate(s => 0) is FormulaError);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestBadFormat()
        {
            Formula expr1 = new Formula("+");
            expr1.Evaluate( s => 0);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestBadFormatOpenPrentesis()
        {
            Formula expr1 = new Formula(")+4");
            expr1.Evaluate(s => 0);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestBadFormatUnbalancedPar()
        {
            Formula expr1 = new Formula("((5+4+4)");
            expr1.Evaluate(s => 0);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestEmptyFormula()
        {
            Formula expr1 = new Formula("");
            expr1.Evaluate(s => 0);
        }
        //------------------Private test methods-----------------------------
        [TestMethod]
        public void TestGetTokens()
        {
            Formula expr1 = new Formula("0+ 1 -32+_Var30o * (43)");
            PrivateType function_accessor = new PrivateType(typeof(Formula));
            IEnumerable<string> tokenizedInput = (IEnumerable<string>)function_accessor.InvokeStatic("GetTokens", new String[1] { "0+ 1 -32+_Var30o * (43)" });
            String[] tokensArray = tokenizedInput.ToArray<string>();
            String[] expected = new string[] {"0","+","1","-","32","+","_Var30o","*","(","43",")" };
            for (int i = 0; i < tokensArray.Length; i++)
                Assert.AreEqual(expected[i], tokensArray[i]);

        }
    }
}
