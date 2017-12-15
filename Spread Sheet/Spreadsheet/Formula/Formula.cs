// Implementation by Nathan Milot u1063587. Skeleton written by Joe Zachary for CS 3500, January 2017

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Formulas {
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    [ExcludeFromCodeCoverage]
    public struct Formula {
        /// <summary>
        /// Private member variables that store the formula
        /// </summary>
        private List<string> tokenList;
        private Normalizer N;
        private Validator V;
        private const string lpPattern = @"\(";
        private const string rpPattern = @"\)";
        private const string opPattern = @"[\+\-*/]";
        private const string varPattern = @"[a-zA-Z][0-9a-zA-Z]*";
        private const string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
        private const string spacePattern = @"\s+";

        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///     
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        /// 
        /// If the formula is syntactical invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>
        public Formula(string formula) {
            // Notes:
            // There can be no invalid tokens.
            // There must be at least one token.
            // When reading tokens from left to right, at no point should the number of closing parentheses seen so far be greater than the number of opening parentheses seen so far.
            // The first token of a formula must be a number, a variable, or an opening parenthesis.
            // The last token of a formula must be a number, a variable, or a closing parenthesis.
            // Any token that immediately follows an opening parenthesis or an operator must be either a number, a variable, or an opening parenthesis.
            // Any token that immediately follows a number, a variable, or a closing parenthesis must be either an operator or a closing parenthesis.

            if (formula == null) {
                throw new ArgumentNullException("Can't have a null formula");
            }

            N = new Normalizer(s => s);
            V = new Validator(s => true);

            IEnumerable<string> tokensEnumerable = GetTokens(formula);
            tokenList = new List<string>();


            string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4})", lpPattern, rpPattern, opPattern, varPattern, doublePattern);

            // Check tokens for invalid tokens and too many close parenthesis
            foreach (string token in tokensEnumerable) {
                if (Regex.IsMatch(token, pattern, RegexOptions.IgnorePatternWhitespace)) {
                    tokenList.Add(token);
                } else if (!Regex.IsMatch(token, spacePattern, RegexOptions.IgnorePatternWhitespace)) {
                    throw new FormulaFormatException("Invalid token: [" + token + "]");
                }
            }

            FormulaValidityCheck();

        }

        /// <summary>
        /// Constructs a new formula object with a given normalizer and validator
        /// </summary>
        /// <param name="f">The formula</param>
        /// <param name="N">The Normalizer</param>
        /// <param name="V">The Validator</param>
        public Formula(string f, Normalizer N, Validator V) : this(f) {
            if (N == null || V == null) {
                throw new ArgumentNullException("Can't have a null normalizer or validator");
            }

            this.N = N;
            this.V = V;

            FormulaValidityCheck();

            for (int i = 0; i < tokenList.Count; i++) {
                if (Regex.IsMatch(tokenList[i], "^" + varPattern + "$", RegexOptions.IgnorePatternWhitespace)) {
                    string tmp = N(tokenList[i]);
                    if (!V(tmp)) {
                        throw new FormulaFormatException(tmp + " is not a valid variable");
                    }
                    tokenList[i] = tmp;
                }
            }
        }

        /// <summary>
        /// Checks the validity of the formula
        /// </summary>
        private void FormulaValidityCheck() {
            string startPattern = string.Format("({0}) | ({1}) | ({2})", lpPattern, varPattern, doublePattern);
            string endPattern = string.Format("({0}) | ({1}) | ({2})", rpPattern, varPattern, doublePattern);

            // Check token count
            if (tokenList.Count < 1) {
                throw new FormulaFormatException("No tokens found");
            }

            // Check first token
            if (!Regex.IsMatch(tokenList[0], startPattern, RegexOptions.IgnorePatternWhitespace)) {
                throw new FormulaFormatException("The formula started with an illegal character");
            }

            // Check last token
            if (!Regex.IsMatch(tokenList[tokenList.Count - 1], endPattern, RegexOptions.IgnorePatternWhitespace)) {
                throw new FormulaFormatException("The formula ended with an illegal character");
            }

            int openParCount = 0;
            int closeParCount = 0;
            // Check Any token that immediately follows an opening parenthesis or an operator must be either a number, a variable, or an opening parenthesis.
            // Check Any token that immediately follows a number, a variable, or a closing parenthesis must be either an operator or a closing parenthesis.
            for (int i = 0; i < tokenList.Count - 1; i++) {
                string current = tokenList[i];
                string next = tokenList[i + 1];

                if (Regex.IsMatch(current, "^" + varPattern + "$", RegexOptions.IgnorePatternWhitespace)) {
                    current = N(current);
                    if (!Regex.IsMatch(current, "^" + varPattern + "$", RegexOptions.IgnorePatternWhitespace)) {
                        throw new FormulaFormatException("The normalizer has caused a variable to become invalid");
                    }
                }

                if (current.Equals("(")) {
                    if (!Regex.IsMatch(next, doublePattern + "|" + varPattern + "|" + lpPattern, RegexOptions.IgnorePatternWhitespace)) { // Non-number, non-variable, or non-open-parenthesis after open parenthesis
                        throw new FormulaFormatException("Expected a number, variable or open parenthesis following the open parenthesis");
                    }
                } else if (Regex.IsMatch(current, varPattern + "|" + doublePattern + "|" + rpPattern, RegexOptions.IgnorePatternWhitespace)
                    && !Regex.IsMatch(next, opPattern + "|" + rpPattern, RegexOptions.IgnorePatternWhitespace)) {     // Non-operator or non-right-parenthesis following number, variable, or right parenthesis
                    throw new FormulaFormatException("Expected an operator or closing parenthesis");
                } else if (Regex.IsMatch(current, opPattern, RegexOptions.IgnorePatternWhitespace)
                    && !Regex.IsMatch(current, doublePattern + "e", RegexOptions.IgnorePatternWhitespace)
                    && Regex.IsMatch(next, opPattern, RegexOptions.IgnorePatternWhitespace)
                    && !Regex.IsMatch(next, doublePattern + "e", RegexOptions.IgnorePatternWhitespace)) {                                          // Operator following operator
                    throw new FormulaFormatException("Operators can't follow operators");
                }

                if (current.Contains("(")) {
                    openParCount++;
                }
                if (current.Contains(")") || i == tokenList.Count - 2 && next.Contains(")")) {
                    closeParCount++;
                }
                if (current.Contains(")") && i == tokenList.Count - 2 && next.Contains(")")) {
                    closeParCount++;
                }

                if (closeParCount > openParCount) {
                    throw new FormulaFormatException("Too many close parenthesis");
                }
            }

            if (openParCount != closeParCount) {
                throw new FormulaFormatException("The number of parenthesis does not match.");
            }
        }

        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup) {
            if (tokenList == null) {
                tokenList = new List<string>();
                tokenList.Add("0");
            }

            Stack<string> opStack = new Stack<string>();
            Stack<double> valStack = new Stack<double>();

            foreach (string token in tokenList) {
                if (Regex.IsMatch(token, doublePattern + "|" + varPattern, RegexOptions.IgnorePatternWhitespace)) {
                    double tokenValue;
                    string nonEVarPattern = @"[a-df-zA-DF-Z][0-9a-df-zA-DF-Z]*";
                    if (Regex.IsMatch(token, doublePattern + "e" + doublePattern, RegexOptions.IgnorePatternWhitespace) && !Regex.IsMatch(token, nonEVarPattern, RegexOptions.IgnorePatternWhitespace)) {
                        string[] tmp = token.Split('e');
                        double.TryParse(tmp[0], out tokenValue);
                        tokenValue *= Math.Pow(10, double.Parse(tmp[1]));
                    } else if (Regex.IsMatch(token, varPattern, RegexOptions.IgnorePatternWhitespace)) {
                        try {
                            tokenValue = lookup(token);
                        } catch (UndefinedVariableException e) {
                            throw new FormulaEvaluationException("The following variable is undefined: " + e.Message);
                        }
                    } else {
                        tokenValue = double.Parse(token);
                    }

                    if (opStack.Count > 0 && Regex.IsMatch(opStack.Peek(), @"[*/]")) {
                        double tmp = valStack.Pop();
                        switch (opStack.Pop()) {
                            case "*":
                                tmp *= tokenValue;
                                break;
                            case "/":
                                if (tokenValue == 0) {
                                    throw new FormulaEvaluationException("Can not divide by 0");
                                }
                                tmp /= tokenValue;
                                break;
                        }
                        valStack.Push(tmp);
                    } else {
                        valStack.Push(tokenValue);
                    }
                } else if (Regex.IsMatch(token, @"[\+\-]", RegexOptions.IgnorePatternWhitespace)) {
                    if (opStack.Count > 0 && Regex.IsMatch(opStack.Peek(), @"[\+\-]", RegexOptions.IgnorePatternWhitespace)) {
                        double tmp1 = valStack.Pop();
                        double tmp2 = valStack.Pop();
                        switch (opStack.Pop()) {
                            case "+":
                                tmp2 += tmp1;
                                break;
                            case "-":
                                tmp2 -= tmp1;
                                break;
                        }
                        valStack.Push(tmp2);
                    }
                    opStack.Push(token);
                } else if (Regex.IsMatch(token, @"[*/(]", RegexOptions.IgnorePatternWhitespace)) {
                    opStack.Push(token);
                } else if (token.Equals(")")) {
                    if (Regex.IsMatch(opStack.Peek(), @"[\+\-]", RegexOptions.IgnorePatternWhitespace)) {
                        double tmp1 = valStack.Pop();
                        double tmp2 = valStack.Pop();
                        switch (opStack.Pop()) {
                            case "+":
                                tmp2 += tmp1;
                                break;
                            case "-":
                                tmp2 -= tmp1;
                                break;
                        }
                        valStack.Push(tmp2);
                    }
                    if (opStack.Peek().Equals("(")) {
                        opStack.Pop();
                    }
                    if (opStack.Count > 0 && Regex.IsMatch(opStack.Peek(), @"[*/]")) {
                        double tmp1 = valStack.Pop();
                        double tmp2 = valStack.Pop();
                        switch (opStack.Pop()) {
                            case "*":
                                tmp2 *= tmp1;
                                break;
                            case "/":
                                if (tmp1 == 0) {
                                    throw new FormulaEvaluationException("Can not divide by 0");
                                }
                                tmp2 /= tmp1;
                                break;
                        }
                        valStack.Push(tmp2);
                    }
                }
            }

            if (opStack.Count > 0) {
                double tmp1 = valStack.Pop();
                double tmp2 = valStack.Pop();
                switch (opStack.Pop()) {
                    case "+":
                        tmp2 += tmp1;
                        break;
                    case "-":
                        tmp2 -= tmp1;
                        break;
                }
                valStack.Push(tmp2);
            }

            return valStack.Pop();
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// zero or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(string formula) {
            // Overall pattern.  It contains embedded white space that must be ignored when
            // it is used.  See below for an example of this.
            string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            // PLEASE NOTE:  Notice the second parameter to Split, which says to ignore embedded white space
            // in the pattern.
            foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace)) {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline)) {
                    yield return s;
                }
            }
        }

        /// <summary>
        /// Gets each unique variable from the formula
        /// </summary>
        /// <returns>A set of each unique variable in the formula</returns>
        public ISet<string> GetVariables() {
            if(tokenList == null) {
                tokenList = new List<string>();
                tokenList.Add("0");
            }
            HashSet<string> result = new HashSet<string>();
            foreach (string s in tokenList) {
                if (Regex.IsMatch(s, "^" + varPattern + "$", RegexOptions.IgnorePatternWhitespace)) {
                    result.Add(s);
                }
            }
            return result;
        }

        /// <summary>
        /// Generates a string from the tokens stored in the formula
        /// </summary>
        /// <returns>A string with all the tokens in the formula with no whitespace</returns>
        public override string ToString() {
            if(tokenList == null) {
                tokenList = new List<string>();
                tokenList.Add("0");
            }
            return string.Join("", tokenList.ToArray());
        }

    }

    //Delegates and Exceptions Classes: 
    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate double Lookup(string var);

    /// <summary>
    /// Normalizes variables to canonical form
    /// </summary>
    /// <param name="s">The variable</param>
    /// <returns></returns>
    public delegate string Normalizer(string s);

    /// <summary>
    /// Imposes extra restrictions on the validity of a variable
    /// </summary>
    /// <param name="s">The variable</param>
    /// <returns></returns>
    public delegate bool Validator(string s);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    [Serializable, ExcludeFromCodeCoverage]
    public class UndefinedVariableException : Exception {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(string variable) : base() {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    [Serializable, ExcludeFromCodeCoverage]
    public class FormulaFormatException : Exception {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(string message) : base(message) {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    [Serializable, ExcludeFromCodeCoverage]
    public class FormulaEvaluationException : Exception {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(string message) : base(message) {
        }
    }
}