//By: Yiliang Shi
//Date:sep 18, 15
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{

    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// s1 depends on t1 --> t1 must be evaluated before s1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// (Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.)
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        //size of the connectionMatrix
        private int internalArraySize;
        //Keeps track of existing varialbes. 
        private List<string> variables;
        //Represents the graph. if [row,col] is true, there is an directed edge from the variable at row to the variable at col. Aka var at col depends on var at row. 
        private bool[,] connectionMatrix;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            variables = new List<string>();
            internalArraySize = 50;
            connectionMatrix = new bool[internalArraySize, internalArraySize];
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            //Checks how many links exist

            get
            {
                int size = 0;
                for (int i = 0; i < internalArraySize; i++)
                {
                    for (int j = 0; j < internalArraySize; j++)
                        if (connectionMatrix[i, j] == true)
                            size++;
                }
                return size;
            }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get
            {
                int size = 0;
                //position of the string. Iterating through the row should give a list of dependees
                int row = variables.IndexOf(s);
                //checks if s exist
                if (row == -1)
                    return 0;

                for (int col = 0; col < internalArraySize; col++)
                    if (connectionMatrix[row, col])
                        size++;
                return size;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            //position of s
            int col = variables.IndexOf(s);

            //checks if s exists
            if (col == -1)
                return false;

            //dependents can be found by iterating over the rows of col
            for (int row = 0; row < internalArraySize; row++)
                if (connectionMatrix[row, col])
                    return true;

            return false;
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary
        public bool HasDependees(string s)
        {
            //position of s
            int row = variables.IndexOf(s);

            //checks if s exists
            if (row == -1)
                return false;

            //dependents can be found by iterating over the rows of col
            for (int col = 0; col < internalArraySize; col++)
                if (connectionMatrix[row, col])
                    return true;

            return false;
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            int col = variables.IndexOf(s);
            //checks if s exists
            if (col == -1)
                yield break;

            for (int row = 0; row < internalArraySize; row++)
            {
                if (connectionMatrix[row, col])
                    yield return variables.ElementAt(row);
            }
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            int row = variables.IndexOf(s);
            //checks if s exists
            if (row == -1)
                yield break;
            for (int col = 0; col < internalArraySize; col++)
            {
                if (connectionMatrix[row, col])
                    yield return variables.ElementAt(col);
            }
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   s depends on t
        ///
        /// </summary>
        /// <param name="s"> s cannot be evaluated until t is</param>
        /// <param name="t"> t must be evaluated first.  S depends on T</param>
        public void AddDependency(string s, string t)
        {
            //try and find if s and t already exisit in graph
            int row = variables.IndexOf(t);
            int col = variables.IndexOf(s);

            //Check for null inputs
            if (s == null || t == null)
                return;

            //if it does not, add it to the list of variables and set row or col to the index of the last element
            if (row == -1)
            {
                variables.Add(t);
                row = variables.Count - 1;

                //Expands the connection matrix array if there are more variables than array size
                if (row > internalArraySize - 1)
                    ExpandArray();
            }
            if (col == -1)
            {
                variables.Add(s);
                col = variables.Count - 1;

                //Expands the connection matrix array if there are more variables than array size
                if (col > internalArraySize - 1)
                    ExpandArray();
            }

            //Sets the connection to true
            connectionMatrix[row, col] = true;

        }

        /// <summary>
        /// Doubles the internal array that represents the graph
        /// </summary>
        private void ExpandArray()
        {
            //make new array
            bool[,] tempArray = new bool[internalArraySize * 2, internalArraySize * 2];

            //copy data from old to new array
            for (int row = 0; row < internalArraySize; row++)
                for (int col = 0; col < internalArraySize; col++)
                    tempArray[row, col] = connectionMatrix[row, col];

            //set connectionMatrix to the new array
            connectionMatrix = tempArray;
            //double the internal array size variable
            internalArraySize *= 2;
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            int row = variables.IndexOf(t);
            int col = variables.IndexOf(s);

            if (row == -1 || col == -1)
                return;
            connectionMatrix[row, col] = false;
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDpendents)
        {
            //get location of dependee variable
            int col = variables.IndexOf(s);

            //Checks if the variable exist 
            if (col != -1)
            {
                //remove ordered pairs
                for (int row = 0; row < internalArraySize; row++)
                    connectionMatrix[row, col] = false;
            }
            //Adds the new ordered pairs
            foreach (string t in newDpendents)
            {
                AddDependency(s, t);
            }


        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            //get location of dependent variable
            int row = variables.IndexOf(s);

            //Checks if variable exist
            if (row == -1)
                return;

            //remove ordered pairs
            for (int col = 0; col < internalArraySize; col++)
                connectionMatrix[row, col] = false;

            //adds new ordered pairs
            foreach (string t in newDependees)
            {
                AddDependency(t, s);
            }
        }

    }




}