//PS3 solution by Yiliang Shi
//Date: 9/22/2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax; variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        //Stores the formula string
        private string[] tokensArray;

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {

       

        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {

        
        string inputFormula = formula;
            //Check that the string is not null or empty
            if (!IsValidString(inputFormula))
                throw new FormulaFormatException("Invalid formula syntax - input is empty or null. Enter a valid function");
            IEnumerable<string> tokenizedInput = GetTokens(inputFormula);
            String [] defaultTokensArray = tokenizedInput.ToArray<string>();
            //checks the token's validity
            IsValidTokensArray(defaultTokensArray);
            tokensArray = new string[defaultTokensArray.Length];
            //Normalizes variables
            for(int i = 0; i < defaultTokensArray.Length ; i++)
            {
                //if token is a variable
                if (TokenType(defaultTokensArray[i]) == 4)
                {
                    tokensArray[i] = normalize(defaultTokensArray[i]);
                    if (TokenType(tokensArray[i]) != 4)
                        throw new FormulaFormatException("variable after being normalized does not fulfill the criterias of a variable. Change either variable or normalizer");
                
                    //Check if the variable is valid
                    if(!isValid(tokensArray[i]))
                        throw new FormulaFormatException("variable after being normalized does not pass the validator's test. Change either validator or variable");
                }
                //if token is a double
                else if (TokenType(defaultTokensArray[i]) == 5)
                {
                    double number;
                    Double.TryParse(defaultTokensArray[i], out number);
                    tokensArray[i] = number.ToString();
                }
                else
                {
                    tokensArray[i] = defaultTokensArray[i];
                }
            }


        }

        private bool IsValidTokensArray(string [] tokens)
        {
            int rightParanCount = 0;
            int leftParanCount = 0;
            int tokenType = 0;
            int previousTokenType = 0;
            //Check that there is at least one token
            if (tokens.Length == 0)
                throw new FormulaFormatException("Invalid formula syntax - there are no valid tokens in input. Enter a valid token");

            for(int i = 0; i < tokens.Length; i++)
            {
                //trims beginning and ending whitespace. Impossible for whitespace to exist in the middle of a token
                tokens[i] = tokens[i].Trim();
                tokenType = TokenType(tokens[i]);
                if (tokenType == -1)
                    throw new FormulaFormatException("Invalid formula syntax - input contains strings that is neither an operator, double, paranthesis or variable. Remove invalid string");

               //if token if an open parenthesis
                if (tokenType == 1)
                { 
                    leftParanCount++;
                    if (previousTokenType == 2 || previousTokenType == 4 || previousTokenType == 5)
                        throw new FormulaFormatException("Invalid formula syntax - open parentasis appeared after a close parenthasis, variable or double. Change to operator or close parenthesis");
                }
                //if token is a close parenthesis
                if (tokenType == 2)
                {
                    rightParanCount++;
                    if (previousTokenType == 1 || previousTokenType == 3)
                        throw new FormulaFormatException("Invalid formula syntax - close parentasis appeared after an open parenthasis or operator. Change to open parenthasis, variable, or double ");
                }
                //Checks that there are never more right than left parenthesis
                if (rightParanCount > leftParanCount)
                    throw new FormulaFormatException("Invalid formula syntax - formula contains instance where there are more closing than opening parenthesis. Reduce closing or increase opening parenthesis");

                //if token is an operator
                if (tokenType == 3)
                {
                    if (previousTokenType == 1 || previousTokenType == 3)
                        throw new FormulaFormatException("Invalid formula syntax - an operator appeared after another operator or open parenthasis. Change to open parenthasis, variable or double");
                }
                
                //if token is a variable
                if(tokenType==4)
                    if(previousTokenType == 2 || previousTokenType == 4 || previousTokenType == 5)
                        throw new FormulaFormatException("Invalid formula syntax - variable appeared after a close parenthasis, variable or double. Change to operator or close parenthasis");

                //if token is a double
                if (tokenType == 5)
                    if (previousTokenType == 2 || previousTokenType == 4 || previousTokenType == 5)
                        throw new FormulaFormatException("Invalid formula syntax - number appeared after a close parenthasis, variable or double. Change to operator or close parenthasis");

                previousTokenType = tokenType;
            }

            //Checks that 1st token is either open paran, var or double.
            if (TokenType(tokens[0]) == 2 || TokenType(tokens[0]) == 3)
                throw new FormulaFormatException("Invalid formula syntax - formula began with close parenthesis or operator. Change to variable, open parenthesis or double");
            //Checks that the last token is either close paran, var or double
            if (TokenType(tokens[tokens.Length-1]) == 1 || TokenType(tokens[tokens.Length-1]) == 3)
                throw new FormulaFormatException("Invalid formula syntax - formula ends with open parenthesis or operator. Change to variable, double or close parenthesis");

            //checks that open and close parenthesis are balanced
            if (leftParanCount != rightParanCount)
                throw new FormulaFormatException("Invalid formula syntax - unequal number of open and close parenthasis.Ensure they are balanced");

            return true;
        }

        /// <summary>
        /// Finds the type of token
        /// </summary>
        /// <param name="input"></param>
        /// <returns>-1 for invalid tokens, 1 for lp, 2 for rp, 3 for operation, 4 for variable, 5 for double</returns>
        private int TokenType(string input)
        {
            String lpPattern = @"^\($";
            String rpPattern = @"^\)$";
            String opPattern = @"^[\+\-*/]$";
            String varPattern = @"^[a-zA-Z_]([a-zA-Z_]|\d)*$";

            if (Regex.IsMatch(input, lpPattern))
                return 1;
            if (Regex.IsMatch(input, rpPattern))
                return 2;
            if (Regex.IsMatch(input, opPattern))
                return 3;
            if (Regex.IsMatch(input, varPattern))
                return 4;

            double temp;
            if (Double.TryParse(input, out temp))
                return 5;

            return -1;
        }

        /// <summary>
        /// Checks that the string is not empty or null
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool IsValidString(String input)
        {
            if (input == null || input.Equals(""))
                return false;
           return true;
        }
        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            double result = 0.0;
            Stack<string> operatorStack = new Stack<string>();  //Stack that keeps track of operation (non-numeric data)
            Stack<double> valueStack = new Stack<double>();     //Stack that keeps track of numeric data
            double number;  //value of a token if it represents a number
            try {
                for (int i = 0; i < tokensArray.Length; i++)
                {
                    //removes whitespace around the tokens
                    tokensArray[i] = tokensArray[i].Trim();

                    //assign operator to value or operator stack
                    switch (tokensArray[i])
                    {
                        //push ( onto the operator stack
                        case "(":
                            operatorStack.Push(tokensArray[i]);
                            break;
                        //Dealing with ")"
                        case ")":
                            if (HasOnTopOperator(operatorStack, "+", "-"))     //if + or - is on top of operationStack, apply it to the top two numbers on valueStack and push result to valuestack
                                valueStack.Push(AddSubtract(operatorStack, valueStack));

                            if (!HasOnTopOperator(operatorStack, "("))       //if ( is not on top of the stack, throw argumentException
                                throw new ArgumentException("Missing opening bracket");
                            operatorStack.Pop();                            //remove (

                            if (HasOnTopOperator(operatorStack, "*", "/"))      //if * or / is on top, apply it to the top to values on valueStack and push the result to valuestack
                                valueStack.Push(MultiplyDivide(operatorStack, valueStack));
                            break;
                        //dealing with "-"
                        case "-":
                            if (HasOnTopOperator(operatorStack, "+", "-"))     //if + or - is on top of operationStack, apply it to the top two numbers on valueStack and push result to valuestack
                                valueStack.Push(AddSubtract(operatorStack, valueStack));
                            operatorStack.Push(tokensArray[i]);                  //push the - token to operationStack
                            break;
                        //dealing with "+"
                        case "+":
                            if (HasOnTopOperator(operatorStack, "+", "-"))     //if + or - is on top of operationStack, apply it to the top two numbers on valueStack and push result to valuestack
                                valueStack.Push(AddSubtract(operatorStack, valueStack));
                            operatorStack.Push(tokensArray[i]);                  //push the + token to operationStack
                            break;
                        //push * onto the operator stack
                        case "*":
                            operatorStack.Push(tokensArray[i]);
                            break;
                        //push * onto the operator stack
                        case "/":
                            operatorStack.Push(tokensArray[i]);
                            break;
                        // if token is not an operation or parentasis 
                        default:
                            bool isNumber = Double.TryParse(tokensArray[i], out number); //Try and see if the token is a number
                            if (!isNumber) //If not, check if its a valid variable
                            {
                               
                                number = lookup(tokensArray[i]); //lookup the value of the variable and set number to it
                            }
                            if (HasOnTopOperator(operatorStack, "*", "/"))      //if * or / is on top, apply it to the top value on valueStack and number, then push the result to valuestack
                                valueStack.Push(MultiplyDivide(operatorStack, valueStack, number));
                            else
                                valueStack.Push(number);                        //if other symbols are on top, push number to top of stack
                            break;
                    }


                }
                if (operatorStack.Count() == 0 && valueStack.Count() == 1)
                {
                    if (double.IsInfinity(result))
                        throw new DivideByZeroException();
                    return result = valueStack.Pop();
                }
                else
                    return result = AddSubtract(operatorStack, valueStack);

            }
            catch (DivideByZeroException e) { return new FormulaError("Invalid operation -divided by 0"); }
            catch (NotFiniteNumberException e) { return new FormulaError("Invalid operation -result is not finite"); }
            catch (OverflowException e) { return new FormulaError("Invalid operation - has an overflow"); }
            catch(Exception e) { return new FormulaError("Something went wrong with the lookup function. Either variable DNE or returns null"); }
        }

        private double AddSubtract(Stack<string> operatorStack, Stack<double> valueStack)
        {
            double result = 0;
            double secondNumber = 0;
            secondNumber = valueStack.Pop();

            if (HasOnTopOperator(operatorStack, "+")) //Add values
            {
                operatorStack.Pop();
                result = valueStack.Pop() + secondNumber;
            }
            else //subtract values
            {
                operatorStack.Pop();
                result = valueStack.Pop() - secondNumber;
            }

            return result;
        }

        /// <summary>
        /// Multiples or divides the top two values on the valueStack according to the top operator on operatorStack
        /// </summary>
        /// <param name="operatorStack">Stack of operators. Expected to contain either multiply or divide</param>
        /// <param name="valueStack">Stack of values. Expected to contain at least two value</param>
        /// <returns>double contianing result of operation</returns>
        private double MultiplyDivide(Stack<string> operatorStack, Stack<double> valueStack)
        {
            double result = 0;
            double secondNumber = 0;
            //Check if there are at least one operator and 2 values
            secondNumber = valueStack.Pop();

            if (HasOnTopOperator(operatorStack, "*"))
            {
                operatorStack.Pop();
                //multiply values
                result = valueStack.Pop() * secondNumber;
            }
            else //divide values
            {
                operatorStack.Pop();
                if (secondNumber == 0)
                    throw new ArithmeticException();
                result = valueStack.Pop() / secondNumber;
            }
            return result;
        }

        /// <summary>
        /// Multiplies or divides top value of valueStack by secondNumber depending on top value of operatorStack. 
        /// </summary>
        /// <param name="operatorStack">stack of operators. Expected to contain at least one string</param>
        /// <param name="valueStack">Stack of numbers. Expected to contain at least one</param>
        /// <param name="secondNumber">second number that acts on the one from the stack</param>
        /// <returns>double containing result of operation</returns>
        private double MultiplyDivide(Stack<string> operatorStack, Stack<double> valueStack, double secondNumber)
        {
            double result = 0;
            //Check if there are at least one operator and 1 values
            result = valueStack.Pop();
            if (HasOnTopOperator(operatorStack, "*")) //multiply values
            {
                operatorStack.Pop();
                result *= secondNumber;
            }
            else//divide values
            {
                operatorStack.Pop();
                if (secondNumber == 0)
                    throw new ArithmeticException();
                result /= secondNumber;
            }
            return result;
        }

        /// <summary>
        /// Determines if the top of the stack is one of the strings symbols passed as parameters
        /// </summary>
        /// <param name="stack">stack tested</param>
        /// <param name="symbols">strings that the top string of the stack is tested against</param>
        /// <returns>true if top element of stack corrospond to one of the symbol parameters</returns>
        private bool HasOnTopOperator( Stack<string> stack, params string[] symbols)
        {
            //if stack is empty, return false
            if (stack.Count == 0)
                return false;
            foreach (string token in symbols)         //test against each symbol parameter
            {
                if ((stack.Peek()).Equals(token))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            HashSet<string> variables = new HashSet<string>();
            foreach (string s in tokensArray)
            {
                if (TokenType(s) == 4)
                    variables.Add(s);
            }
            foreach(string s in variables)
            {
                yield return s;
            }
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            string result = "";
            foreach(string s in tokensArray)
            {
                result += s;
            }
            return result;
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens, which are compared as doubles, and variable tokens,
        /// whose normalized forms are compared as strings.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Formula))
                return false;

            string[] objArray = ((Formula)obj).tokensArray;
            if (objArray.Length != this.tokensArray.Length)
                return false;

            for(int i = 0; i < objArray.Length; i++)
            {
                
                if (!objArray[i].Equals(this.tokensArray[i]))
                    return false;

            }
            return true;
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if (object.ReferenceEquals(f1, null) && object.ReferenceEquals(f2, null))
                return true;
            if (object.ReferenceEquals(f1, null) || object.ReferenceEquals(f2, null))
                return false;
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            if (f1 == null && f2 == null)
                return false;
            if (f1 == null || f2 == null)
                return true;
            return !f1.Equals(f2);
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return (this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";
            
            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}

