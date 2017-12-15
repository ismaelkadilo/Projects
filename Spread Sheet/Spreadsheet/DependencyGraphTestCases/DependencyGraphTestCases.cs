// Tests by Nathan Milot, u1063587. 

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dependencies {
    [TestClass, ExcludeFromCodeCoverage]
    public class DependencyGraphTestCases {
        
        // Old Tests: 

        // Setup: 
        /// <summary>
        /// For use in multiple test cases, so I can setup a huge graph and then test methods on it in separate cases
        /// </summary>
        private static DependencyGraph hugeGraph;

        /// <summary>
        /// Initializes hugeGraph for use
        /// </summary>
        [ClassInitialize]
        public static void Setup(TestContext context) {
            hugeGraph = new DependencyGraph();
        }

        // Constructor Tests: 
        /// <summary>
        /// Runs tests on an empty graph to ensure it initialized properly
        /// </summary>
        [TestMethod]
        public void EmptyGraphTests() {
            DependencyGraph g = new DependencyGraph();
            Assert.AreEqual(g.Size, 0);
            Assert.IsFalse(g.HasDependents("a"));
            Assert.IsFalse(g.HasDependees("b"));
        }

        /// <summary>
        /// Tests basic setup of a dependency graph
        /// </summary>
        [TestMethod]
        public void SimpleGraphSetup() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("a", "b");
            g.AddDependency("a", "c");
            g.AddDependency("b", "c");
            Assert.AreEqual(g.Size, 3);
        }

        /// <summary>
        /// Tests setting up a large dependency graph
        /// </summary>
        [TestMethod]
        public void LargeGraphSetup() {
            DependencyGraph g = new DependencyGraph();
            //for (int i = 0; i < 100000; i++) {
            // New limit to improve performance for legacy tests
            int limit = 1000;
            for (int i = 0; i < limit; i++) {
                g.AddDependency(i + "a", i + "b");
            }
            Assert.AreEqual(limit, g.Size);
        }

        /// <summary>
        /// Tests setting up a huge dependency graph
        /// </summary>
        [TestMethod]
        public void HugeGraphSetup1() {
            DependencyGraph g = new DependencyGraph();
            //for (int i = 0; i < 1000000; i++) {
            // New limits to reduce overall test time
            int limit = 1000;
            for (int i = 0; i < limit; i++) {
                    g.AddDependency(i + "a", i + "b");
            }
            Assert.AreEqual(limit, g.Size);
        }

        /// <summary>
        /// Tests setting up a huge sized dependency graph for use in other methods
        /// </summary>
        [TestMethod]
        public void HugeGraphSetup2() {
            //for (int i = 0; i < 1000; i++) {
            //for (int j = 0; j < 1000; j++) {
            // New limits to reduce overall test time
            int limit = 300;
            for (int i = 0; i < limit; i++) {
                for (int j = 0; j < limit; j++) {

                    hugeGraph.AddDependency(i + "a", j + "b");
                }
            }
            Assert.AreEqual(limit * limit, hugeGraph.Size);
        }

        // Mutator Tests: 
        [TestMethod]
        public void MethodsOnHugeGraph() {
            // Checks HasDependents and HasDependees Methods
            Assert.IsTrue(hugeGraph.HasDependents("0a"));
            Assert.IsTrue(hugeGraph.HasDependees("0b"));

            // Creates list for ReplaceDependents and ReplaceDependees Methods
            List<string> stringList = new List<string>();
            for (int i = 0; i < 10; i++) {
                stringList.Add("new " + i);
            }

            // Checks ReplaceDependents and ReplaceDependees Methods
            hugeGraph.ReplaceDependents("0a", stringList);
            Assert.IsTrue(hugeGraph.HasDependents("0a"));

            hugeGraph.ReplaceDependees("0b", stringList);
            Assert.IsTrue(hugeGraph.HasDependees("0b"));

            // Checks HasDependents and HasDependees Methods to make sure the new dependencies have been added properly
            Assert.IsTrue(hugeGraph.HasDependents("new 0"));
            Assert.IsTrue(hugeGraph.HasDependees("new 0"));

            // Checks RemoveDependency Method
            hugeGraph.RemoveDependency("0a", "0b");
            hugeGraph.RemoveDependency("0a", "new 0");

            Assert.IsFalse(hugeGraph.HasDependees("new 0"));
            Assert.IsTrue(hugeGraph.HasDependents("0a"));

            // Checks GetDependents and GetDependees Methods
            foreach (string s in hugeGraph.GetDependees("0a")) {
                if (s.Equals("new 1")) {
                    break;
                } else {
                    Assert.Fail();
                }
            }

            foreach (string s in hugeGraph.GetDependents("new 0")) {
                if (s.Equals("0b")) {
                    break;
                } else {
                    Assert.Fail();
                }
            }


        }

        /// <summary>
        /// Tests to see if HasDependents and HasDependees methods are working correctly
        /// </summary>
        [TestMethod]
        public void HasDependentAndDependee() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("a", "b");
            g.AddDependency("a", "b");
            Assert.AreEqual(1, g.Size);
            Assert.IsTrue(g.HasDependents("a"));
            Assert.IsTrue(g.HasDependees("b"));
            Assert.IsFalse(g.HasDependees("a"));
            Assert.IsFalse(g.HasDependents("b"));
        }

        /// <summary>
        /// Tests the remove dependency method
        /// </summary>
        [TestMethod]
        public void RemoveDependency() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("Dependent", "Dependee");
            g.RemoveDependency("Dependent", "Dependee");
            Assert.IsFalse(g.HasDependents("Dependent"));
            Assert.IsFalse(g.HasDependees("Dependee"));
            Assert.AreEqual(0, g.Size);
        }

        /// <summary>
        /// Tests the remove dependency method on empty graph
        /// </summary>
        [TestMethod]
        public void RemoveDependencyOnEmpty() {
            DependencyGraph g = new DependencyGraph();
            g.RemoveDependency("Dependee", "Dependent");
            Assert.IsFalse(g.HasDependees("Dependee"));
            Assert.IsFalse(g.HasDependents("Dependent"));
            Assert.AreEqual(0, g.Size);
            Console.WriteLine(g.ToString());
        }

        // <summary>
        /// Tests the replace dependents method
        /// </summary>
        [TestMethod]
        public void ReplaceDependents() {
            List<string> stringList = new List<string>();
            for (int i = 0; i < 10; i++) {
                stringList.Add(i + "a");
            }
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("Dependee", "Dependent");
            g.AddDependency("A", "B");
            stringList.Add("A");
            g.ReplaceDependents("Dependee", stringList);
            Assert.IsFalse(g.HasDependees("Dependent"));
            Assert.IsTrue(g.HasDependents("Dependee"));
            Assert.AreEqual(12, g.Size);
        }

        // <summary>
        /// Tests the replace dependents method on empty graph
        /// </summary>
        [TestMethod]
        public void ReplaceDependentsOnEmpty() {
            List<string> stringList = new List<string>();
            for (int i = 0; i < 10; i++) {
                stringList.Add(i + "a");
            }
            DependencyGraph g = new DependencyGraph();
            g.ReplaceDependents("Dependee", stringList);
            Assert.IsFalse(g.HasDependees("0a"));
            Assert.AreEqual(0, g.Size);
        }

        // <summary>
        /// Tests the replace dependees method
        /// </summary>
        [TestMethod]
        public void ReplaceDependees() {
            List<string> stringList = new List<string>();
            for (int i = 0; i < 10; i++) {
                stringList.Add(i + "a");
            }
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("Dependee", "Dependent");
            g.AddDependency("A", "B");
            stringList.Add("B");
            g.ReplaceDependees("Dependent", stringList);
            Assert.IsTrue(g.HasDependees("Dependent"));
            Assert.IsFalse(g.HasDependents("Dependee"));
            Assert.AreEqual(12, g.Size);
        }

        // <summary>
        /// Tests the replace dependees method on empty graph
        /// </summary>
        [TestMethod]
        public void ReplaceDependeesOnEmpty() {
            List<string> stringList = new List<string>();
            for (int i = 0; i < 10; i++) {
                stringList.Add(i + "a");
            }
            DependencyGraph g = new DependencyGraph();
            g.ReplaceDependees("Dependent", stringList);
            Assert.IsFalse(g.HasDependents("0a"));
            Assert.AreEqual(0, g.Size);
        }

        /// <summary>
        /// Tests get dependents method
        /// </summary>
        [TestMethod]
        public void GetDependentsTest() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("A", "B");
            g.AddDependency("A", "C");
            string result = "";
            foreach (string s in g.GetDependents("A")) {
                result += s + " ";
            }
            Assert.AreEqual("B C ", result);
        }

        /// <summary>
        /// Tests get dependents method on empty dependee
        /// </summary>
        [TestMethod]
        public void GetDependentsEmptyTest() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("B", "A");
            g.AddDependency("B", "C");
            string result = "";
            foreach (string s in g.GetDependents("A")) {
                result += s + " ";
            }
            Assert.AreEqual("", result);
        }

        /// <summary>
        /// Tests get dependees method
        /// </summary>
        [TestMethod]
        public void GetDependeesTest() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("A", "C");
            g.AddDependency("B", "C");
            string result = "";
            foreach (string s in g.GetDependees("C")) {
                result += s + " ";
            }
            Assert.AreEqual("A B ", result);
        }

        /// <summary>
        /// Tests get dependees method on empty dependent
        /// </summary>
        [TestMethod]
        public void GetDependeesEmptyTest() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("A", "B");
            g.AddDependency("A", "C");
            string result = "";
            foreach (string s in g.GetDependees("A")) {
                result += s + " ";
            }
            Assert.AreEqual("", result);
        }

        // Miscellaneous Tests: 
        /// <summary>
        /// Tests passing null values into dependency graph methods
        /// </summary>
        [TestMethod]
        public void PassingNullTests() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("A", "B");
            List<string> stringList = new List<string>();
            stringList.Add("1a");
            stringList.Add(null);

            try {
                g.AddDependency("A", null);
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                g.AddDependency(null, "B");
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                g.AddDependency(null, null);
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                g.GetDependees(null);
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                g.GetDependents(null);
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                g.HasDependees(null);
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                g.HasDependents(null);
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                g.RemoveDependency("A", null);
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                g.RemoveDependency(null, "B");
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                g.ReplaceDependees(null, stringList);
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                stringList = null;
                g.ReplaceDependees("A", stringList);
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                g.ReplaceDependents(null, stringList);
                Assert.Fail();
            } catch (Exception) {
            }

            try {
                stringList = null;
                g.ReplaceDependents("A", stringList);
                Assert.Fail();
            } catch (Exception) {
            }
        }

        /// <summary>
        /// Tests to see if ReplaceDependents and ReplaceDependees methods work on string with no dependees or dependents respectively
        /// </summary>
        [TestMethod]
        public void ReplaceMethodsOnEmpty() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("Dependee", "Dependent");

            List<string> stringList = new List<string>();
            for (int i = 1; i < 11; i++) {
                stringList.Add(i + "");
            }

            Assert.IsFalse(g.HasDependees("Dependee"));
            g.ReplaceDependees("Dependee", stringList);
            Assert.IsTrue(g.HasDependees("Dependee"));

            Assert.IsFalse(g.HasDependents("Dependent"));
            g.ReplaceDependents("Dependent", stringList);
            Assert.IsTrue(g.HasDependents("Dependent"));
            Console.WriteLine(g.ToString());
        }

        /// <summary>
        /// Checks to see if self-dependency is allowed
        /// </summary>
        [TestMethod]
        public void AddSelfAsDependent() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("A", "B");
            g.AddDependency("A", "A");
            Assert.AreEqual(2, g.Size);
        }

        //PS4 Tests: 

        // Constructor Tests: 
        /// <summary>
        /// Checks to see if the new constructor works properly
        /// i.e. it copies dependencies properly and doesn't modify the other graphs dependencies
        /// </summary>
        [TestMethod]
        public void NewConstructorTest() {
            DependencyGraph g1 = new DependencyGraph();
            g1.AddDependency("A1", "B1");
            g1.AddDependency("A2", "B2");
            Assert.AreEqual(2, g1.Size);

            DependencyGraph g2 = new DependencyGraph(g1);
            Assert.AreEqual(2, g2.Size);
            g2.RemoveDependency("A1", "B1");

            Assert.AreEqual(1, g2.Size);
            Assert.AreEqual(2, g1.Size);

            Assert.IsTrue(g1.HasDependents("A1"));
            Assert.IsFalse(g2.HasDependents("A1"));

            g1.AddDependency("A3", "B3");
            Assert.AreEqual(3, g1.Size);
            Assert.AreEqual(1, g2.Size);

            g2.AddDependency("A4", "B4");
            Assert.AreEqual(3, g1.Size);
            Assert.AreEqual(2, g2.Size);

            Assert.AreEqual("(A1, B1) (A2, B2) (A3, B3)", g1.ToString());
            Assert.AreEqual("(A2, B2) (A4, B4)", g2.ToString());
        }

        /// <summary>
        /// Checks to see if the new constructor works properly
        /// For additional Code Coverage
        /// </summary>
        [TestMethod]
        public void NewConstructorTest2() {
            DependencyGraph g1 = new DependencyGraph();
            g1.AddDependency("A1", "B1");
            g1.AddDependency("A2", "B2");
            g1.AddDependency("A2", "B1");
            g1.AddDependency("B2", "A1");
            Assert.AreEqual(4, g1.Size);

            DependencyGraph g2 = new DependencyGraph(g1);
            Assert.AreEqual(4, g2.Size);
            g2.RemoveDependency("A1", "B1");

            Assert.AreEqual(3, g2.Size);
            Assert.AreEqual(4, g1.Size);

            Assert.IsTrue(g1.HasDependents("A1"));
            Assert.IsFalse(g2.HasDependents("A1"));

            g1.AddDependency("A3", "B3");
            Assert.AreEqual(5, g1.Size);
            Assert.AreEqual(3, g2.Size);

            g2.AddDependency("A4", "B4");
            Assert.AreEqual(5, g1.Size);
            Assert.AreEqual(4, g2.Size);

            Assert.AreEqual("(A1, B1) (A2, B2) (A2, B1) (B2, A1) (A3, B3)", g1.ToString());
            Assert.AreEqual("(B2, A1) (A2, B2) (A2, B1) (A4, B4)", g2.ToString());
        }

        // Null Tests: 
        /// <summary>
        /// Tests if methods throw ArgumentNullExceptions and if a null string is passed when 
        /// </summary>
        [TestMethod]
        public void NullConstructor() {
            // Constructor
            try {
                DependencyGraph NewGraph = new DependencyGraph(null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                DependencyGraph NullGraph = null;
                DependencyGraph NewGraph = new DependencyGraph(NullGraph);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }
        }

        /// <summary>
        /// Tests adding a null object
        /// </summary>
        [TestMethod]
        public void NullAdd() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("A", "B");
            // AddDependency
            try {
                g.AddDependency(null, "B1");
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                g.AddDependency("A1", null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }
        }

        /// <summary>
        /// Tests get dependees and dependents for a null object
        /// </summary>
        [TestMethod]
        public void NullGet() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("A", "B");
            // Get Dependees / Dependents
            try {
                g.AddDependency("A1", "B1");
                g.GetDependees(null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                g.AddDependency("A1", "B1");
                g.GetDependents(null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }
        }

        /// <summary>
        /// Tests has dependees and dependents for a null object
        /// </summary>
        [TestMethod]
        public void NullHas() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("A", "B");
            // Has Dependees / Dependents
            try {
                g.AddDependency("A1", "B1");
                g.HasDependees(null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                g.AddDependency("A1", "B1");
                g.HasDependents(null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }
        }

        /// <summary>
        /// Tests removing a dependency with a null object
        /// </summary>
        [TestMethod]
        public void NullRemove() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("A", "B");
            // RemoveDependency
            try {
                g.RemoveDependency(null, "B");
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                g.RemoveDependency("A", null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }
        }

        /// <summary>
        /// Tests replacing dependees with at least one null object
        /// </summary>
        [TestMethod]
        public void NullReplaceDependees() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("A", "B");
            List<string> NullList = null;
            List<string> ListWithNull = new List<string>();
            ListWithNull.Add("Z");
            ListWithNull.Add(null);
            List<string> NormalList = new List<string>();
            NormalList.Add("X");
            NormalList.Add("Y");
            NormalList.Add("Z");

            // ReplaceDependees
            try {
                g.ReplaceDependees(null, NormalList);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                g.ReplaceDependees("A", null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                g.ReplaceDependees("A", NullList);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                g.ReplaceDependees("A", ListWithNull);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            
        }

        /// <summary>
        /// Tests replacing dependents with at least one null object
        /// </summary>
        [TestMethod]
        public void NullReplaceDependents() {
            DependencyGraph g = new DependencyGraph();
            g.AddDependency("A", "B");
           
            List<string> NullList = null;
            List<string> ListWithNull = new List<string>();
            ListWithNull.Add("Z");
            ListWithNull.Add(null);
            List<string> NormalList = new List<string>();
            NormalList.Add("X");
            NormalList.Add("Y");
            NormalList.Add("Z");

            // ReplaceDependents
            try {
                g.ReplaceDependents(null, NormalList);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                g.ReplaceDependents("A", null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                g.ReplaceDependents("A", NullList);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                g.ReplaceDependents("A", ListWithNull);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }
        }

    }
}
