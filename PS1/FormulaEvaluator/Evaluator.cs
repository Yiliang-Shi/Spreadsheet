// Author: Yiliang Shi

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormulaEvaluator
{
    /// <summary>
    /// Class that contains library that evaluates arithmetic expressions written using standard infix notation and follows precedence rules. 
    /// </summary>
    public static class Evaluator
    {
        /// <summary>
        /// Returns integer value of a variable
        /// </summary>
        /// <param name="v">variable involved</param>
        /// <returns>value of variable</returns>
        public delegate int Lookup(String v);

        /// <summary>
        /// method that evaluates arithmetic expressions written using standard infix notation. Respect the usual precedence rules. Returns either the value of the expression or throws an ArgumentException
        /// </summary>
        /// <param name="exp">String Expression</param>
        /// <param name="variableEvaluator">Lookup function that returns the value of variables</param>
        /// <returns>Returns either the value of the expression or throws an ArgumentException if expression is invalid</returns>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            int result=0;           //result of the expression
            string[] expression = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)"); //array of string tokens from the athrimetic expression
            Stack<string> operatorStack = new Stack<string>();  //Stack that keeps track of operation (non-numeric data)
            Stack<double> valueStack = new Stack<double>();     //Stack that keeps track of numeric data
            double number;  //value of a token if it represents a number

            for(int i = 0; i<expression.Length;i++)
            {
                //removes whitespace in tokens
                expression[i] = expression[i].RemoveWhiteSpace();

                //assign operator to value or operator stack
                switch (expression[i])
                {
                    //ignore whitespace, skip to next loop
                    case "":
                        break;
                    //push ( onto the operator stack
                    case "(":
                        operatorStack.Push(expression[i]);
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
                        operatorStack.Push(expression[i]);                  //push the - token to operationStack
                        break;
                    //dealing with "+"
                    case "+":
                        if (HasOnTopOperator(operatorStack, "+", "-"))     //if + or - is on top of operationStack, apply it to the top two numbers on valueStack and push result to valuestack
                            valueStack.Push(AddSubtract(operatorStack, valueStack));
                        operatorStack.Push(expression[i]);                  //push the + token to operationStack
                        break;
                    //push * onto the operator stack
                    case "*":                                       
                        operatorStack.Push(expression[i]); 
                        break;
                    //push * onto the operator stack
                    case "/":                                       
                        operatorStack.Push(expression[i]); 
                        break;
                    // if token is not an operation or parentasis 
                    default:
                        bool isNumber = Double.TryParse(expression[i], out number); //Try and see if the token is a number
                       if ( !isNumber) //If not, check if its a valid variable
                        {
                            if (!isVariable(expression[i]))
                                throw new ArgumentException("Illegal token in input string"); //if invalid variable, throw exception. 

                            number = variableEvaluator(expression[i]); //lookup the value of the variable and set number to it
                        }

                        if (HasOnTopOperator(operatorStack, "*", "/"))      //if * or / is on top, apply it to the top value on valueStack and number, then push the result to valuestack
                            valueStack.Push(MultiplyDivide(operatorStack, valueStack, number));
                        else
                            valueStack.Push(number);                        //if other symbols are on top, push number to top of stack
                        break;
                }
                
                
            }
            if (operatorStack.Count() == 0 && valueStack.Count() == 1)
               return result = (int)valueStack.Pop();
            else if (operatorStack.Count() == 1 && valueStack.Count() == 2 && HasOnTopOperator(operatorStack, "+", "-"))
               return result = (int)AddSubtract(operatorStack, valueStack);
            throw new ArgumentException("Reached end of string, invalid expression");       
            
        }

        /// <summary>
        /// Tests if string matches the specifiction of a variable (starts with one or more letter, ends with one or more number)
        /// </summary>
        /// <param name="token">string tested</param>
        /// <returns>True if string is a variable. False otherwise</returns>
        private static bool isVariable(string token)
        {
            return Regex.IsMatch(token, @"^[A-Za-z]+[0-9]+$");    
        }

        /// <summary>
        /// Performs either addition or subtraction based on values in the stacks passed
        /// </summary>
        /// <param name="operatorStack">stack of string operators. Expected to contain "+" or "-" as top string</param>
        /// <param name="valueStack">stack containing numeric values. Expected to contain at least 2 values</param>
        /// <returns>returns result of operation</returns>
        private static double AddSubtract(Stack<string> operatorStack, Stack<double> valueStack)
        {
            double result = 0;
            double secondNumber = 0;
            //Check if there are at least one operator and 2 values
            if (operatorStack.Count() == 0 || valueStack.Count < 2)
                throw new ArgumentException("Insufficient data in stack - cannot perform addition or subtraction");
            secondNumber = valueStack.Pop();

            if (HasOnTopOperator(operatorStack, "+")) //Add values
            {
                operatorStack.Pop();
                result = valueStack.Pop() + secondNumber;
            }
            else if (HasOnTopOperator(operatorStack, "-"))//subtract values
            {
                operatorStack.Pop();
                result = valueStack.Pop() - secondNumber;
            }
            else
                throw new ArgumentException("invalid operator: does not contain plus or minus"); //throws exception if neither plus or minus is present
            return result;
        }

        /// <summary>
        /// Multiples or divides the top two values on the valueStack according to the top operator on operatorStack
        /// </summary>
        /// <param name="operatorStack">Stack of operators. Expected to contain either multiply or divide</param>
        /// <param name="valueStack">Stack of values. Expected to contain at least two value</param>
        /// <returns>double contianing result of operation</returns>
        private static double MultiplyDivide(Stack <string> operatorStack, Stack <double> valueStack)
        {
            double result = 0;
            double secondNumber = 0;
             //Check if there are at least one operator and 2 values
            if (operatorStack.Count() == 0 || valueStack.Count < 2)
                throw new ArgumentException("Insufficient data in stack - cannot perform addition or subtraction");
            secondNumber = valueStack.Pop();

            if (HasOnTopOperator(operatorStack, "*"))
            {
                operatorStack.Pop();
                //multiply values
                result = valueStack.Pop() * secondNumber;
            }
            else if (HasOnTopOperator(operatorStack, "/"))//divide values
            {
                if (secondNumber == 0)
                    throw new ArgumentException("Invalid arguement - divides by 0");
                
                    operatorStack.Pop();
                    result = valueStack.Pop() / secondNumber;
            }
            else
                throw new ArgumentException("invalid operator: does not contain plus or minus"); //throws exception if neither multiply or divide is present
            return result;
        }

        /// <summary>
        /// Multiplies or divides top value of valueStack by secondNumber depending on top value of operatorStack. 
        /// </summary>
        /// <param name="operatorStack">stack of operators. Expected to contain at least one string</param>
        /// <param name="valueStack">Stack of numbers. Expected to contain at least one</param>
        /// <param name="secondNumber">second number that acts on the one from the stack</param>
        /// <returns>double containing result of operation</returns>
        private static double MultiplyDivide(Stack<string> operatorStack, Stack<double> valueStack,double secondNumber)
        {
            double result = 0;
            //Check if there are at least one operator and 1 values
            if (operatorStack.Count() == 0 || valueStack.Count < 1)
                throw new ArgumentException("Insufficient data in stack - cannot perform addition or subtraction");
            result = valueStack.Pop();
            if (HasOnTopOperator(operatorStack, "*")) //multiply values
            {
                operatorStack.Pop();
                result *= secondNumber;
            }
            else if (HasOnTopOperator(operatorStack, "/"))//divide values
            {
                if (secondNumber == 0)
                    throw new ArgumentException("Invalid arguement - divides by 0");
                
                    operatorStack.Pop();
                    result /= secondNumber;
            }
            else
                throw new ArgumentException("invalid operator: does not contain plus or minus"); //throws exception if neither multiply or divide is present
            return result;
        }

        /// <summary>
        /// Determines if the top of the stack is one of the strings symbols passed as parameters
        /// </summary>
        /// <param name="stack">stack tested</param>
        /// <param name="symbols">strings that the top string of the stack is tested against</param>
        /// <returns>true if top element of stack corrospond to one of the symbol parameters</returns>
        private static bool HasOnTopOperator(this Stack<string> stack, params string[] symbols)
        {
            //if stack is empty, return false
            if (stack.Count == 0)
                return false;
            foreach (string token in symbols)         //test against each symbol parameter
            {
                if ((stack.Peek()).Equals( token))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Removes whitespace in a string
        /// </summary>
        /// <param name="s">string involved</param>
        /// <returns>string without whitespace</returns>
        private static string RemoveWhiteSpace(this string s)
        {
            return Regex.Replace(s, @"\s+", "");
        }
    }
}
