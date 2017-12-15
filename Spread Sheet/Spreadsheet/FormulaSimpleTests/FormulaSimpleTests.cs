// Additional Tests by Nathan Milot, u1063587. Written by Joe Zachary for CS 3500, January 2017.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Formulas;
using System;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace FormulaTestCases {
    /// <summary>
    /// These test cases are in no sense comprehensive!  They are intended to show you how
    /// client code can make use of the Formula class, and to show you how to create your
    /// own (which we strongly recommend).  To run them, pull down the Test menu and do
    /// Run > All Tests.
    /// </summary>
    [TestClass, ExcludeFromCodeCoverage]
    public class UnitTests {
        
        // Old Tests: 
        
        // Construct Unit Tests 
        /// <summary>
        /// This tests that a syntactically incorrect parameter to Formula results
        /// in a FormulaFormatException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Construct1() {
            Formula f = new Formula("_");
        }

        /// <summary>
        /// This is another syntax error
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Construct2() {
            Formula f = new Formula("2++3");
        }

        /// <summary>
        /// Another syntax error.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void Construct3() {
            Formula f = new Formula("2 3");
        }

        /// <summary>
        /// This tests that if no tokens are passed it results
        /// in a FormulaFormatException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ConstructEmpty() {
            Formula f = new Formula("");
        }

        /// <summary>
        /// This tests that if an illegal token is passed as the first token it results
        /// in a FormulaFormatException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ConstructIllegalStart() {
            Formula f = new Formula("+7+8");
        }

        /// <summary>
        /// This tests that if an illegal token is passed as the last token it results
        /// in a FormulaFormatException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ConstructIllegalEnd() {
            Formula f = new Formula("7+8+");
        }

        /// <summary>
        /// This tests that if an illegal token is passed as the last token it results
        /// in a FormulaFormatException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ConstructIllegalCloseParenthesis() {
            Formula f = new Formula(")(7)");
        }

        /// <summary>
        /// Non-operator or non-right-parenthesis following number, variable, or right parenthesis
        /// </summary>
        [TestMethod]
        public void ConstructIllegalTokens() {
            // Following right parenthesis
            try {
                Formula f = new Formula("(4+5)(7+3)");
                Assert.Fail();
            } catch (FormulaFormatException) {
            }

            // Following number
            try {
                Formula f = new Formula("(4+5)(7+3)");
                Assert.Fail();
            } catch (FormulaFormatException) {
            }

            // Following variable
            try {
                Formula f = new Formula("x + y 8");
                Assert.Fail();
            } catch (FormulaFormatException) {
            }

            // Following left parenthesis
            try {
                Formula f = new Formula("()");
                Assert.Fail();
            } catch (FormulaFormatException) {
            }

            // Number Following Number
            try {
                Formula f = new Formula("1 2 + 9");
                Assert.Fail();
            } catch (FormulaFormatException) {
            }

            // Variable Following Variable
            try {
                Formula f = new Formula("x y + 9");
                Assert.Fail();
            } catch (FormulaFormatException) {
            }
        }

        // Evaluate Unit Tests 
        /// <summary>
        /// Makes sure that "2+3" evaluates to 5.  Since the Formula
        /// contains no variables, the delegate passed in as the
        /// parameter doesn't matter.  We are passing in one that
        /// maps all variables to zero.
        /// </summary>
        [TestMethod]
        public void Evaluate1() {
            Formula f = new Formula("2+3");
            Assert.AreEqual(f.Evaluate(v => 0), 5.0, 1e-6);
        }

        /// <summary>
        /// The Formula consists of a single variable (x5).  The value of
        /// the Formula depends on the value of x5, which is determined by
        /// the delegate passed to Evaluate.  Since this delegate maps all
        /// variables to 22.5, the return value should be 22.5.
        /// </summary>
        [TestMethod]
        public void Evaluate2() {
            Formula f = new Formula("x5");
            Assert.AreEqual(f.Evaluate(v => 22.5), 22.5, 1e-6);
        }

        /// <summary>
        /// Here, the delegate passed to Evaluate always throws a
        /// UndefinedVariableException (meaning that no variables have
        /// values).  The test case checks that the result of
        /// evaluating the Formula is a FormulaEvaluationException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaEvaluationException))]
        public void Evaluate3() {
            Formula f = new Formula("x + y");
            f.Evaluate(v => { throw new UndefinedVariableException(v); });
        }

        /// <summary>
        /// The delegate passed to Evaluate is defined below.  We check
        /// that evaluating the formula returns in 10.
        /// </summary>
        [TestMethod]
        public void Evaluate4() {
            Formula f = new Formula("x + y");
            Assert.AreEqual(f.Evaluate(Lookup4), 10.0, 1e-6);
        }

        /// <summary>
        /// This uses one of each kind of token.
        /// </summary>
        [TestMethod]
        public void Evaluate5() {
            Formula f = new Formula("(x + y) * (z / x) * 1.0");
            Assert.AreEqual(f.Evaluate(Lookup4), 20.0, 1e-6);
        }

        /// <summary>
        /// Tests additional miscellaneous cases
        /// </summary>
        [TestMethod]
        public void EvaluateCustomCases() {
            // Multiple +'s
            Formula f = new Formula("4 + 5 + 6");
            Assert.AreEqual(f.Evaluate(v => 0), 15);

            // Multiple -'s
            f = new Formula("10 - 2 - 3");
            Assert.AreEqual(f.Evaluate(Lookup4), 5);

            // Division after other operations
            f = new Formula("(x + y) / (z / x)");
            Assert.AreEqual(f.Evaluate(v => 10), 20);

            // Divide by zero after other operations
            try {
                f = new Formula("(x + y) / (0)");
                f.Evaluate(v => 0);
                Assert.Fail();
            } catch (FormulaEvaluationException) {
            }

            // Divide by 0
            try {
                f = new Formula("10/0");
                f.Evaluate(Lookup4);
                Assert.Fail();
            } catch (FormulaEvaluationException) {
            }

            // Addition after parenthesis
            f = new Formula("(10 - 5) + 6");
            Assert.AreEqual(f.Evaluate(Lookup4), 11);

            // Subtraction after parenthesis
            f = new Formula("(10 - 5) - 6");
            Assert.AreEqual(f.Evaluate(Lookup4), -1);

            // Ending with subtraction
            f = new Formula("10 - 5");
            Assert.AreEqual(f.Evaluate(Lookup4), 5);

            // Ending with division
            f = new Formula("x/2");
            Assert.AreEqual(f.Evaluate(v => 6), 3);
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void EvaluateAddition() {
            Formula f;
            for (int i = 0; i < 1000; i++) {
                f = new Formula("x + " + (2 * i));
                Assert.AreEqual(f.Evaluate(v => i), i + (2 * i));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void EvaluateSubtraction() {
            Formula f;
            for (int i = 0; i < 1000; i++) {
                f = new Formula("x - " + i);
                Assert.AreEqual(f.Evaluate(v => 2 * i), i);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void EvaluateMultiplication() {
            Formula f;
            for (int i = 0; i < 1000; i++) {
                f = new Formula("x * " + 10);
                Assert.AreEqual(f.Evaluate(v => i), i * 10);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void EvaluateDivision() {
            Formula f;
            for (int i = 0; i < 1000; i++) {
                f = new Formula("x / " + 2.0);
                Assert.AreEqual(f.Evaluate(v => i), i / 2.0);
            }
        }

        // Additional Methods 
        /// <summary>
        /// A Lookup method that maps x to 4.0, y to 6.0, and z to 8.0.
        /// All other variables result in an UndefinedVariableException.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double Lookup4(string v) {
            switch (v) {
                case "x": return 4.0;
                case "y": return 6.0;
                case "z": return 8.0;
                default: throw new UndefinedVariableException(v);
            }
        }

        // Assignment 4 Tests: 
        
        // Constructor Tests: 
        /// <summary>
        /// Tests Struct to build a zero parameter constructor
        /// </summary>
        [TestMethod]
        public void ZeroParamConstructorTest() {
            Formula f = new Formula();
            Assert.AreEqual(0, f.Evaluate(v => 0));
            // Make sure behavior matches 
            Formula f2 = new Formula("0");
            Assert.AreEqual(0, f2.Evaluate(v => 0));
        }

        /// <summary>
        /// Tests to make sure handling null parameters is done correctly
        /// </summary>
        [TestMethod]
        public void NullParameterTest() {
            // Single parameter null formula
            try {
                Formula f = new Formula(null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            // Three parameter null formula
            try {
                Formula f = new Formula(null, n => n.ToUpper(), v => Regex.IsMatch(v, @"[A-Z][0-9]"));
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            // Three parameter null normalizer
            try {
                Formula f = new Formula("x1 + x2", null, v => Regex.IsMatch(v, @"[A-Z][0-9]"));
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            // Three parameter null validator
            try {
                Formula f = new Formula("x1 + x2", n => n.ToUpper(), null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            // Three parameter none null
            try {
                Formula f = new Formula("x1 + x2", n => n.ToUpper(), v => Regex.IsMatch(v, @"[A-Z][0-9]"));
            } catch (ArgumentNullException) {
                Assert.Fail();
            }
        }

        /// <summary>
        /// Tests multiple three parameter formulas 
        /// </summary>
        [TestMethod]
        public void ThreeParamConstructorTest() {
            // Valid and normalized
            Formula f = new Formula("x1 + y1", n => n.ToUpper(), v => Regex.IsMatch(v, @"[A-Z][0-9]"));
            Assert.AreEqual("X1+Y1", f.ToString());

            // Not valid, throws formula format exception
            try {
                Formula f2 = new Formula("x + 2 + y1", n => n.ToUpper(), v => Regex.IsMatch(v, @"[A-Z][0-9]"));
                Assert.Fail();
            } catch (FormulaFormatException) {
            }
        }

        // Method Tests: 
        /// <summary>
        /// Tests GetVariables method to make sure it is working properly
        /// </summary>
        [TestMethod]
        public void GetVariablesTest() {
            // Two variables
            Formula f = new Formula("x1 + 7 * y1 + 10", n => n.ToUpper(), v => Regex.IsMatch(v, @"[A-Z][0-9]"));
            Assert.AreEqual("X1Y1", string.Join("", f.GetVariables()));

            // One variable repeated multiple times
            Formula f2 = new Formula("x1 - x1 * x1 + x1 / x1", n => n.ToUpper(), v => Regex.IsMatch(v, @"[A-Z][0-9]"));
            Assert.AreEqual("X1", string.Join("", f2.GetVariables()));
        }

        /// <summary>
        /// Tests the ToString Method
        /// </summary>
        [TestMethod]
        public void ToStringTest() {
            Formula f1 = new Formula("x1 + 7 /10", n => n.ToUpper(), v => Regex.IsMatch(v, @"[A-Z][0-9]"));
            Formula f2 = new Formula(f1.ToString(), s => s, s => true);
            Assert.AreEqual(f1.ToString(), f2.ToString());
            Assert.AreEqual(f1.Evaluate(x => 1),  f2.Evaluate(x => 1));
        }

        // Miscellaneous Tests: 
        /// <summary>
        /// Tests zero parameter constructor get variables and evaluate
        /// </summary>
        [TestMethod]
        public void CheckMethodsOnZeroConstructor() {
            Formula f = new Formula();
            Assert.AreEqual("", string.Join("", f.GetVariables()));
            Assert.AreEqual(0, f.Evaluate(v => 10));

            f = new Formula();
            Assert.AreEqual(0, f.Evaluate(v => 10));
            Assert.AreEqual("", string.Join("", f.GetVariables()));
        }

        /// <summary>
        /// Makes sure the GetVariables method returns a copy and not a reference
        /// </summary>
        [TestMethod]
        public void ModifyGetVaribles() {
            Formula f = new Formula("x1 + y1");
            ISet<string> varSet = f.GetVariables();
            varSet.Clear();
            varSet.Add("z1");
            Assert.AreEqual("x1y1", string.Join("", f.GetVariables()));
        }
            
        /// <summary>
        /// Tests to ensure scientific notation is handled properly with the normalizer and validator
        /// </summary>
        [TestMethod]
        public void ScientificNotation() {
            Formula f = new Formula("1e3", s => s.ToUpper(), v => Regex.IsMatch(v, @"^[A-Z][0-9]$"));
            Assert.AreEqual(1000, f.Evaluate(v => 0));

            Formula f2 = new Formula(f.ToString(), s => s, v => true);
            Assert.AreEqual(f.Evaluate(v => 0), f2.Evaluate(v => 0));
        }

        /// <summary>
        /// Tests to see if f is syntactically correct but contains a variable x such that N(x) is not a legal variable according to the standard Formula rules, the constructor should throw a FormulaFormatException with an explanatory message.
        /// Also tests to see if the normalized value is used in place
        /// </summary>
        [TestMethod]
        public void NormalizerTest() {
            try {
                Formula f1 = new Formula("x + 7", s => s+ ")", v => true);
                Assert.Fail();
            }catch (FormulaFormatException) {
            }

            try {
                Formula f1 = new Formula("x + 7", s => "_" + s, v => true);
                Assert.Fail();
            } catch (FormulaFormatException) {
            }

            try {
                Formula f1 = new Formula("x + 7", s => "y_" + s, v => true);
                Assert.Fail();
            } catch (FormulaFormatException) {
            }

            Formula f2 = new Formula("x + 7", s => s.ToUpper(), v => true);
            Assert.AreEqual("X+7", f2.ToString());
        }

    }
}
