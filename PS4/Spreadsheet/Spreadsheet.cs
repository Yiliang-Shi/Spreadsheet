using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

namespace SS
{

    public class Spreadsheet : AbstractSpreadsheet
    {
        //Representation of a spreadsheet. Contains all non-empty cells
        private Dictionary<string, Cell> spreadsheet;

        //Represents dependency graph of formulas in spreadsheet
        private DependencyGraph dependencyGraph = new DependencyGraph();

        /// <summary>
        /// Default spreadsheet constructor that sets IsValid to true,
       ///  version to default and normalize to return itself
        /// </summary>
        public Spreadsheet():this( x => true, x => x, "default")
        {
            Changed = false;
        }
        /// <summary>
        /// Spreadsheet constructor that builds a spreadsheet
        /// </summary>
        /// <param name="normalize">A normalizer</param>
        /// <param name="isValid">A validator</param>
        /// <param name="version">string representing spreadsheet version</param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, String version):base (isValid, normalize, version)
        {
            spreadsheet = new Dictionary<string, Cell>();
            Changed = false;
        }
        /// <summary>
        /// Spreadsheet constructor that builds a spreadsheet from an xml file
        /// </summary>
        /// <param name="pathname">pathname of file</param>
        ///  <param name="normalize">A normalizer</param>
        /// <param name="isValid">A validator</param>
        /// <param name="version">string representing spreadsheet version</param>
        public Spreadsheet(String pathname, Func<string, bool> isValid, Func<string, string> normalize, String version): base (isValid, normalize, version)
        {
            spreadsheet = new Dictionary<string, Cell>();
            if (!ReadSpreadsheet(pathname))
                throw new SpreadsheetReadWriteException("Cannot read spreadsheet: format error");
            Changed = false;
            Version = version;

        }

        /// <summary>
        /// Actually reads from a saved file
        /// </summary>
        /// <param name="pathname"></param>
        /// <returns></returns>
        private bool ReadSpreadsheet(String pathname)
        {
            bool isValidXML = true;
            StreamReader reader =null;
            XmlReader xmlReader = null;
            try
            {
                reader = new StreamReader(pathname);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                xmlReader = XmlReader.Create(reader,settings);

                //keeps track of element for easy reading - assumes that xml must strictly follow documentation format and order
                int elementCount = 0;
                //Read file a node at a time
                while (xmlReader.Read())
                {
                    switch (elementCount)
                    {
                        //first node should be some version info, ignore
                        case 0:elementCount++;
                            break;
                        //2nd node should be spreadsheet start element with attribute version
                        case 1:
                            if (!xmlReader.IsStartElement() || !xmlReader.Name.Equals("spreadsheet") || !xmlReader.HasAttributes)
                                return false;
                            String newVersion = xmlReader.GetAttribute("version");
                            if (!newVersion.Equals(Version))
                                throw new SpreadsheetReadWriteException("Version of file does not match read file");
                            Version = newVersion; 
                            elementCount++;   
                            break;
                                    
                        case 2:
                            //Check for cell tag
               
                            if (xmlReader.IsStartElement() && xmlReader.Name.Equals("cell")) { }
                            else if (xmlReader.NodeType==XmlNodeType.EndElement&& xmlReader.Name.Equals("spreadsheet"))
                            {
                                elementCount++;
                                break;
                            }
                            xmlReader.Read();
                            //Check for name tag
                            if (!xmlReader.IsStartElement() || !xmlReader.Name.Equals("name"))
                                return false;
                            xmlReader.Read();
                            //Read name 
                            if (xmlReader.NodeType != XmlNodeType.Text)
                                return false;
                            String cellName = xmlReader.Value;
                            xmlReader.Read();
                            //read name close tag
                            if (xmlReader.NodeType != XmlNodeType.EndElement||!xmlReader.Name.Equals("name"))
                                return false;
                            xmlReader.Read();
                            //Check for content tag
                            if (!xmlReader.IsStartElement() || !xmlReader.Name.Equals("contents"))
                                return false;
                            xmlReader.Read();
                            //Read Content 
                            if (xmlReader.NodeType != XmlNodeType.Text)
                                return false;
                            String cellContent = xmlReader.Value;
                            xmlReader.Read();
                            //read content close tag
                            if (xmlReader.NodeType != XmlNodeType.EndElement||!xmlReader.Name.Equals("contents"))
                                return false;
                            xmlReader.Read();
                            //Check cell close tag
                            if (xmlReader.NodeType != XmlNodeType.EndElement || !xmlReader.Name.Equals("cell"))
                                return false;

                            //Create new cell
                            SetContentsOfCell(cellName, cellContent);
                            break;
                       
                        case 3:return false;

                    }
                }

            }
            catch (Exception ex)
            {
                throw new SpreadsheetReadWriteException("Read exception: "+ ex.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (xmlReader != null)
                    xmlReader.Dispose();

            }
            return isValidXML;
        }
        private bool changed;

        /// <summary>
        /// Reflects if file have been changed since last save or open
        /// Assumes change if setcontent has been called, regardless of if content has changed
        /// </summary>
        public override bool Changed
        {
            protected set
            {
                changed = value;
            }
            get
            {
                return changed;
            }
        }

        /// <summary>
        ///  Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        /// <param name="filename">filename</param>
        /// <returns></returns>
        public override string GetSavedVersion(string filename)
        {
            StreamReader reader = null;
            XmlReader xmlReader = null;
            try
            {
                reader = new StreamReader(filename);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                xmlReader = XmlReader.Create(reader, settings);
                //reads first line with xml version and ignore content
                xmlReader.Read();
                //reads second line which should be spreadsheet tag
                xmlReader.Read();
                if (!xmlReader.IsStartElement() || !xmlReader.Name.Equals("spreadsheet"))
                    throw new SpreadsheetReadWriteException("Spreadsheet read error: xml not to specification");
                return xmlReader.GetAttribute("version");
            }
            catch (Exception ex) { throw new SpreadsheetReadWriteException("Spreadsheet read error: " + ex.Message); }
            finally
            {
                xmlReader.Dispose();
                reader.Close();
            }
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>
        /// cell name goes here
        /// </name>
        /// <contents>
        /// cell contents goes here
        /// </contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        /// <param name="filename">filename</param>
        public override void Save(string filename)
        {
            //flip changed to false
            Changed = false;
            StreamWriter writer = null;
            XmlWriter xmlWriter = null;
            try
            {
                writer = new StreamWriter(filename);
                XmlWriterSettings ws = new XmlWriterSettings();
                ws.Indent = true;
                xmlWriter = XmlWriter.Create(writer, ws);

                //Save version and spreadsheet info
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("spreadsheet");
                xmlWriter.WriteAttributeString("version", Version);

                //For each cell
                foreach (string name in GetNamesOfAllNonemptyCells())
                {
                    Cell cell;
                    spreadsheet.TryGetValue(name, out cell);
                    xmlWriter.WriteStartElement("cell");

                    xmlWriter.WriteStartElement("name");
                    xmlWriter.WriteString(name);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("contents");
                    //if content is a string
                    if(cell.ContentType==0)
                        xmlWriter.WriteString((string)cell.Content);
                    //if content is a double
                    if(cell.ContentType==1)
                        xmlWriter.WriteString(((double)cell.Content).ToString());
                    //if content is a formula
                    if (cell.ContentType == 2)
                        xmlWriter.WriteString("="+((Formula)cell.Content).ToString());

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();                    
                }
                //Close spreadsheet tags
                xmlWriter.WriteEndElement();
               xmlWriter.WriteEndDocument();
            }
            catch(Exception ex)
            {
                throw new SpreadsheetReadWriteException("Write exception: "+ ex.Message);
            }
            finally
            {
                if (xmlWriter != null)
                    xmlWriter.Close();
                if (writer != null)
                    writer.Close();
            }
        }

        /// <summary>
        ///If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        /// <param name="name">name of cell</param>
        /// <returns></returns>
        public override object GetCellValue(string name)
        {
            if (name == null)
                throw new InvalidNameException();
            name = Normalize(name);
            if (!NameIsValid(name)||!IsValid(name))
                throw new InvalidNameException();         
            Object cellContent = GetCellContents(name);

            if (cellContent is double)
                return (double)cellContent;
            else if (cellContent is string)
                return (string)cellContent;
            else 
            {
                try {
                    return ((Formula)cellContent).Evaluate(getValue);
                }
                catch(Exception e)
                {
                    return new FormulaError("Formula Error: Formula is invalid");
                }
            }
        }

        /// <summary>
        /// private method the forces evaluate to return a double or exception
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private double getValue(String name)
        {
            Object cellContent = GetCellValue(name);

            if (cellContent is FormulaError)
                throw new FormulaFormatException("Bad formula");
            else if (cellContent is double)
                return (double)cellContent;
            else
            {

                    return (double)((Formula)cellContent).Evaluate(getValue);

            }
        }

        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public override ISet<String> SetContentsOfCell(String name, String content)
        {
            if (content == null)
                throw new ArgumentNullException();
            //Check name
            if (name == null)
                throw new InvalidNameException();
            name = Normalize(name);
            if ( !NameIsValid(name) || !IsValid(name))
                throw new InvalidNameException();
            Changed = true;
            double contentValue;
            if (content.Length > 0 && content.ElementAt(0).Equals('='))
            {
                    Formula formula = new Formula(content.Substring(1), Normalize, IsValid);
                    return SetCellContents(name, formula);
               
               
            }
            //Check if its a double
            else if (Double.TryParse(content, out contentValue))
            {                          
                return SetCellContents(name, contentValue);
            }
            //Check if its a formula
            
            else
            {
               return SetCellContents(name, content);
            }
            
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            name = Normalize(name);
            if (name == null || !NameIsValid(name))
                throw new InvalidNameException();

            //if non-empty return content
            if (spreadsheet.ContainsKey(name))
            {
                return spreadsheet[name].Content;
            }
            //if empty return empty string
            else
                return "";
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            foreach (string s in spreadsheet.Keys)
                yield return s;
        }

        /// <summary>
        /// If the formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.  (No change is made to the spreadsheet.)
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)

        {

            if (formula == null)

                throw new ArgumentNullException();

            try

            {

                GetCellsToRecalculate(name);

            }

            catch (CircularException)

            {

                throw new CircularException();

            }

            ISet<string> nameAndDependees = new HashSet<string>(GetCellsToRecalculate(name));





            IEnumerable<string> oldDependents = null;

            //Check if new fomula cause an error

            try

            {



                //Updates dependency graph (Since the content cannot change other cells, only dependents need to be updated)

                IEnumerable<string> newVariables = formula.GetVariables();

                foreach (string var in newVariables)

                    if (var.Equals(name))

                        throw new CircularException();

                oldDependents = dependencyGraph.GetDependents(name);

                dependencyGraph.ReplaceDependents(name, newVariables);





                //Check if the new formula cause an error

                foreach (string newName in newVariables)

                {

                    GetCellsToRecalculate(newName);

                }





                //If data is valid and there are no cycles, add text to spreadsheet



                if (spreadsheet.ContainsKey(name))

                    spreadsheet[name].Content = formula;

                else

                {

                    Cell newCell = new Cell();

                    newCell.Content = formula;

                    spreadsheet.Add(name, newCell);

                }





            }

            catch (CircularException)

            {

                dependencyGraph.ReplaceDependents(name, oldDependents);

                throw new CircularException();

            }

            return nameAndDependees;

        }



        protected ISet<string> SetCellContents(string name, FormulaError error)
        {
           
            //If data is valid, add it to the set to return 
            ISet<string> nameAndDependees = new HashSet<string>(GetCellsToRecalculate(name));

            //If data is valid and there are no cycles, add text to spreadsheet

            if (spreadsheet.ContainsKey(name))
            {
                spreadsheet[name].Value = error;
            }
            else
            {
                Cell newCell = new Cell();
                newCell.Value = error;
                spreadsheet.Add(name, newCell);
            }

            //Updates dependency graph by removing all dependents
            IEnumerable<string> newVariables = new List<string>();
            dependencyGraph.ReplaceDependents(name, newVariables);
            
            return nameAndDependees;
        }

        private bool NameIsValid(string name)
        {
            String varPattern = @"^[a-zA-Z]([a-zA-Z]|\d)*\d$";
            return Regex.IsMatch(name, varPattern);
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            if (text == null)
                throw new ArgumentNullException();

            //If data is valid, add it to the set to return 
            ISet<string> nameAndDependees = new HashSet<string>(GetCellsToRecalculate(name));

            //If data is valid and there are no cycles, add text to spreadsheet

            if (spreadsheet.ContainsKey(name))
                spreadsheet[name].Content = text;
            else
            {
                Cell newCell = new Cell();
                newCell.Content = text;
                spreadsheet.Add(name, newCell);
            }

            //Updates dependency graph by removing all dependents
            IEnumerable<string> newVariables = new List<string>();
            dependencyGraph.ReplaceDependents(name, newVariables);

            //Check if it new string is an empty string. if it is, remove from spreadsheet's non empty cell dictionary
            if (text.Equals(""))
                spreadsheet.Remove(name);
            return nameAndDependees;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            //Add it to the set to return 
            ISet<string> nameAndDependees = new HashSet<string>(GetCellsToRecalculate(name));

            //If data is valid and there are no cycles, add formula to spreadsheet
            if (spreadsheet.ContainsKey(name))
            { 
                spreadsheet[name].Content = number;
            }
            else
            {
                Cell newCell = new Cell();
                newCell.Content = number;
                spreadsheet.Add(name, newCell);
            }


            //Updates dependency graph by removing all dependents
            IEnumerable<string> newVariables = new List<string>();
            dependencyGraph.ReplaceDependents(name, newVariables);

            return nameAndDependees;
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// <param name="name">string of interest</param>
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (name == null || !NameIsValid(name))
                throw new ArgumentNullException();
            if (!NameIsValid(name))
                throw new InvalidNameException();

            return dependencyGraph.GetDependees(name);
        }
    }

    /// <summary>
    /// Represents a cell in a spreadsheet, with the properties ContentType, Content and Value
    /// </summary>
    class Cell
    {
        private int contentType;    //Specifies whether cell content is a string, double or formula, represented by 0,1,2 respectively
        private double doubleValue;
        private string stringValue;
        private Formula formulaContent;
        private Object formulaValue;

        public Cell()
        {
            contentType = 0;//sets default content type to string
            stringValue = "";//sets default content to empty string
            formulaValue = "";
        }

        /// <summary>
        /// Property: read only property that contains an int that represents the type of data in the cell. 0 - string; 1 - double; 2 - formula
        /// </summary>
        public int ContentType
        {
            get
            {
                return contentType;
            }
        }

        /// <summary>
        /// Property: Content of a cell. May contain a string, double or Formula
        /// </summary>
        public object Content
        {
            set
            {
                if (value is double)
                {
                    contentType = 1;
                    doubleValue = (double)value;
                }
                if (value is string)
                {
                    contentType = 0;
                    stringValue = (string)value;
                }
                if (value is Formula)
                {
                    contentType = 2;
                    formulaContent = (Formula)value;
                }
            }
            get
            {
                if (contentType == 0)
                    return stringValue;
                else if (contentType == 1)
                    return doubleValue;
                else
                    return formulaContent;
            }
        }

        //Property: Value property may contain a string, double or formulaError
        //to be implemented
        public object Value
        {
            set
            {
                formulaValue = value;

            }
            get
            {
                if (contentType == 0)
                    return stringValue;
                else if (contentType == 1)
                    return doubleValue;
                else
                {
                    return formulaValue;
                }
                    
            }

        }

    }
}
