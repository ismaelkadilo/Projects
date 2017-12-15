// Implantation by Nathan Milot, u1063587. Skeleton implementation written by Joe Zachary for CS 3500, January 2017.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dependencies {

    /// <summary>
    /// A DependencyGraph can be modeled as a set of dependencies, where a dependency is an ordered 
    /// pair of strings.  Two dependencies (s1,t1) and (s2,t2) are considered equal if and only if 
    /// s1 equals s2 and t1 equals t2.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that the dependency (s,t) is in DG 
    ///    is called the dependents of s, which we will denote as dependents(s).
    ///        
    ///    (2) If t is a string, the set of all strings s such that the dependency (s,t) is in DG 
    ///    is called the dependees of t, which we will denote as dependees(t).
    ///    
    /// The notations dependents(s) and dependees(s) are used in the specification of the methods of this class.
    ///
    /// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    ///     dependents("a") = {"b", "c"}
    ///     dependents("b") = {"d"}
    ///     dependents("c") = {}
    ///     dependents("d") = {"d"}
    ///     dependees("a") = {}
    ///     dependees("b") = {"a"}
    ///     dependees("c") = {"a"}
    ///     dependees("d") = {"b", "d"}
    ///     
    /// All of the methods below require their string parameters to be non-null.  This means that 
    /// the behavior of the method is undefined when a string parameter is null.  
    ///
    /// IMPORTANT IMPLEMENTATION NOTE
    /// 
    /// The simplest way to describe a DependencyGraph and its methods is as a set of dependencies, 
    /// as discussed above.
    /// 
    /// However, physically representing a DependencyGraph as, say, a set of ordered pairs will not
    /// yield an acceptably efficient representation.  DO NOT USE SUCH A REPRESENTATION.
    /// 
    /// You'll need to be more clever than that.  Design a representation that is both easy to work
    /// with as well acceptably efficient according to the guidelines in the PS3 writeup. Some of
    /// the test cases with which you will be graded will create massive DependencyGraphs.  If you
    /// build an inefficient DependencyGraph this week, you will be regretting it for the next month.
    /// 
    /// s => dependee
    /// t => dependent
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DependencyGraph {
        /// <summary>
        /// Helps Store the backing structure of the DependencyGraph, which stores a dependent and its dependees
        /// </summary>
        private class DependancyNode {

            // Member variables: 
            /// <summary>
            /// The name of the dependent
            /// </summary>
            private string Name;
            /// <summary>
            /// The backing structure to hold dependents
            /// </summary>
            private Dictionary<string, DependancyNode> Dependents;
            /// <summary>
            /// The backing structure to hold dependees
            /// </summary>
            private Dictionary<string, DependancyNode> Dependees;

            // Constructor:   
            /// <summary>
            /// Default Constructor that only adds a name and establishes backing structures
            /// </summary>
            /// <param name="name"></param>
            public DependancyNode(string name) {
                Name = name;
                Dependents = new Dictionary<string, DependancyNode>();
                Dependees = new Dictionary<string, DependancyNode>();
            }

            // Methods: 
            /// <summary>
            /// Adds a dependent to the object
            /// </summary>
            /// <param name="n"></param>
            public void AddDependent(DependancyNode n) {
                if (!Dependents.ContainsKey(n.ToString())) {
                    Dependents.Add(n.ToString(), n);
                }
            }

            /// <summary>
            /// Adds a dependee to the object
            /// </summary>
            /// <param name="n"></param>
            public void AddDependee(DependancyNode n) {
                if (!Dependees.ContainsKey(n.ToString())) {
                    Dependees.Add(n.ToString(), n);
                }
            }

            /// <summary>
            /// Removes a given dependents
            /// </summary>
            public void RemoveDependent(string t) {
                if (Dependents.ContainsKey(t)) {
                    Dependents.Remove(t);
                }
            }

            /// <summary>
            /// Removes all dependents
            /// </summary>
            public void RemoveDependents() {
                foreach (DependancyNode n in Dependents.Values) {
                    n.RemoveDependee(this.Name);
                }
                Dependents.Clear();
            }

            /// <summary>
            /// Removes a given dependees
            /// </summary>
            public void RemoveDependee(string s) {
                if (Dependees.ContainsKey(s)) {
                    Dependees.Remove(s);
                }
            }

            /// <summary>
            /// Removes all dependees
            /// </summary>
            public void RemoveDependees() {
                foreach (DependancyNode n in Dependees.Values) {
                    n.RemoveDependent(this.Name);
                }
                Dependees.Clear();
            }

            /// <summary>
            /// Returns a string representation of the object
            /// </summary>
            /// <returns></returns>
            public override string ToString() {
                return Name;
            }

            /// <summary>
            /// Returns the dictionary of the dependents strings
            /// </summary>
            /// <returns></returns>
            public Dictionary<string, DependancyNode> GetDependents() {
                return Dependents;
            }

            /// <summary>
            /// Returns the dictionary of the dependees
            /// </summary>
            /// <returns></returns>
            public Dictionary<string, DependancyNode> GetDependees() {
                return Dependees;
            }

        }

        /// <summary>
        /// Stores a set of all nodes
        /// </summary>
        private Dictionary<string, DependancyNode> allNodes;

        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        public DependencyGraph() {
            allNodes = new Dictionary<string, DependancyNode>();
        }

        /// <summary>
        /// Creates a DependencyGraph containing a copy of all the dependencies in the other graph
        /// Throws a ArgumentNullException if OtherGraph is null.
        /// </summary>
        /// <param name="OtherGraph">The other dependency graph from which the dependencies are copied</param>
        public DependencyGraph(DependencyGraph OtherGraph) {
            if(OtherGraph == null) {
                throw new ArgumentNullException("Argument(s) must be non-null");
            }

            allNodes = new Dictionary<string, DependancyNode>();
            foreach (DependancyNode n in OtherGraph.allNodes.Values) {
                DependancyNode NewNode;
                if (allNodes.ContainsKey(n.ToString())) {
                    NewNode = allNodes[n.ToString()];
                } else {
                    NewNode = new DependancyNode(n.ToString());
                    allNodes.Add(NewNode.ToString(), NewNode);
                }

                foreach (DependancyNode n2 in n.GetDependees().Values) {
                    DependancyNode NewDependee;
                    if (allNodes.ContainsKey(n2.ToString())) {
                        NewDependee = allNodes[n2.ToString()];
                    } else {
                        NewDependee = new DependancyNode(n2.ToString());
                        allNodes.Add(NewDependee.ToString(), NewDependee);
                    }
                    NewNode.AddDependee(NewDependee);
                }

                foreach (DependancyNode n2 in n.GetDependents().Values) {
                    DependancyNode NewDependent;
                    if (allNodes.ContainsKey(n2.ToString())) {
                        NewDependent = allNodes[n2.ToString()];
                    } else {
                        NewDependent = new DependancyNode(n2.ToString());
                        allNodes.Add(NewDependent.ToString(), NewDependent);
                    }
                    NewNode.AddDependent(NewDependent);
                }
            }
        }

        /// <summary>
        /// The number of dependencies in the DependencyGraph.
        /// </summary>
        public int Size {
            get {
                int size = 0;
                foreach (DependancyNode n in allNodes.Values) {
                    size += n.GetDependees().Count;
                }
                return size;
            }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// Throws a ArgumentNullException if s is null.
        /// </summary>
        public bool HasDependents(string s) {
            if (s == null) {
                throw new ArgumentNullException("Argument(s) must be non-null");
            }
            if (allNodes.ContainsKey(s)) {
                DependancyNode dependee = allNodes[s];
                return dependee.GetDependents().Count > 0;
            }
            return false;
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.  
        /// Throws a ArgumentNullException if s is null
        /// </summary>
        public bool HasDependees(string s) {
            if (s == null) {
                throw new ArgumentNullException("Argument(s) must be non-null");
            }
            if (allNodes.ContainsKey(s)) {
                DependancyNode dependent = allNodes[s];
                return dependent.GetDependees().Count > 0;
            }
            return false;
        }

        /// <summary>
        /// Enumerates dependents(s).  
        /// Throws a ArgumentNullException if s is null.
        /// </summary>
        public IEnumerable<string> GetDependents(string s) {
            if (s == null) {
                throw new ArgumentNullException("Argument(s) must be non-null");
            }
            if (!allNodes.ContainsKey(s)) {
                return new DependancyNode(s).GetDependees().Keys;
            }
            DependancyNode dependeeNode = allNodes[s];
            return dependeeNode.GetDependents().Keys;
        }

        /// <summary>
        /// Enumerates dependees(s).  
        /// Throws a ArgumentNullException if s is null.
        /// </summary>
        public IEnumerable<string> GetDependees(string s) {
            if (s == null) {
                throw new ArgumentNullException("Argument(s) must be non-null");
            }
            if (!allNodes.ContainsKey(s)) {
                return new DependancyNode(s).GetDependees().Keys;
            }
            DependancyNode dependentNode = allNodes[s];
            return dependentNode.GetDependees().Keys;
        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// Throws a ArgumentNullException if s or t is null.
        /// </summary>
        public void AddDependency(string s, string t) {
            if (s == null || t == null) {
                throw new ArgumentNullException("Argument(s) must be non-null");
            }

            DependancyNode dependeeNode;
            DependancyNode dependentNode;

            if (allNodes.ContainsKey(s)) {
                dependeeNode = allNodes[s];
            } else {
                dependeeNode = new DependancyNode(s);
                allNodes.Add(s, dependeeNode);
            }

            if (allNodes.ContainsKey(t)) {
                dependentNode = allNodes[t];
            } else {
                dependentNode = new DependancyNode(t);
                allNodes.Add(t, dependentNode);
            }

            dependeeNode.AddDependent(dependentNode);
            dependentNode.AddDependee(dependeeNode);
        }

        /// <summary>
        /// Removes the dependency (s,t) from this DependencyGraph.
        /// Does nothing if (s,t) doesn't belong to this DependencyGraph.
        /// Throws a ArgumentNullException if s or t is null.
        /// </summary>
        public void RemoveDependency(string s, string t) {
            if (s == null || t == null) {
                throw new ArgumentNullException("Argument(s) must be non-null");
            }
            if (allNodes.ContainsKey(s) && allNodes.ContainsKey(t)) {
                DependancyNode dependee = allNodes[s];
                DependancyNode dependent = allNodes[t];
                dependee.RemoveDependent(t);
                dependent.RemoveDependee(s);
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (s,r).  Then, for each
        /// t in newDependents, adds the dependency (s,t).
        /// Throws a ArgumentNullException if s or t is null.
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents) {
            if (s == null || newDependents == null) {
                throw new ArgumentNullException("Argument(s) must be non-null");
            }
            if (allNodes.ContainsKey(s)) {
                DependancyNode dependee = allNodes[s];
                dependee.RemoveDependents();
                foreach (string n in newDependents) {
                    if (n != null) {
                        DependancyNode newDependent;
                        if (allNodes.ContainsKey(n)) {
                            newDependent = allNodes[n];
                        } else {
                            newDependent = new DependancyNode(n);
                            allNodes.Add(n, newDependent);
                        }
                        dependee.AddDependent(newDependent);
                        newDependent.AddDependee(dependee);
                    } else {
                        throw new ArgumentNullException("Can't replace dependents with null");
                    }
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (r,t).  Then, for each 
        /// s in newDependees, adds the dependency (s,t).
        /// Throws a ArgumentNullException if s or t is null.
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees) {
            if (t == null || newDependees == null) {
                throw new ArgumentNullException("Argument(s) must be non-null");
            }
            if (allNodes.ContainsKey(t)) {
                DependancyNode dependent = allNodes[t];
                Dictionary<string, DependancyNode> dependees = dependent.GetDependents();
                dependent.RemoveDependees();
                foreach (string n in newDependees) {
                    if (n != null) {
                        DependancyNode newDependee;
                        if (allNodes.ContainsKey(n)) {
                            newDependee = allNodes[n];
                        } else {
                            newDependee = new DependancyNode(n);
                            allNodes.Add(n, newDependee);
                        }
                        dependent.AddDependee(newDependee);
                        newDependee.AddDependent(dependent);
                    }else {
                        throw new ArgumentNullException("Can't replace dependees with null");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a string representation of the dependency graph
        /// Used for testing and visualization purposes
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override string ToString() {
            string result = "";
            foreach (DependancyNode n in allNodes.Values) {
                foreach (string d in n.GetDependents().Keys) {
                    result += "(" + n + ", " + d + ") ";
                }
            }
            if (result.Length > 1) {
                return result.Substring(0, result.Length - 1);
            } else {
                return result;
            }
        }

    }
}
