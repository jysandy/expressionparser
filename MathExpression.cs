using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sandy.Expression
{
    enum Associativity
    {
        Left,
        Right
    }

    public class MathExpression
    {
        private Queue<Token> postfixExpression;
        private bool isFunction;

        //Tokenizes and converts to postfix notation.
        //Sets the postfixExpression field.
        public MathExpression(string infixExpression)
        {
            this.isFunction = false;
            this.postfixExpression = new Queue<Token>();

            Queue<Token> inputQueue = this.Tokenize(infixExpression);
            Stack<Token> operatorStack = new Stack<Token>();

            Token token;
            do
            {
                token = inputQueue.Dequeue();
                switch (token.Type)
                {
                    case TokenType.Literal:
                        if (token.Data == "x")
                            this.isFunction = true;
                        this.postfixExpression.Enqueue(token);
                        break;

                    case TokenType.Operator:
                        while (operatorStack.Count > 0
                            && operatorStack.Peek().Type == TokenType.Operator
                            && PrecedenceOf(token.Data) <= PrecedenceOf(operatorStack.Peek().Data)
                            )
                        {
                            this.postfixExpression.Enqueue(operatorStack.Pop());
                        }
                        operatorStack.Push(token);
                        break;

                    case TokenType.LeftParenthesis:
                        operatorStack.Push(token);
                        break;

                    case TokenType.RightParenthesis:
                        while (operatorStack.Count > 0
                            && operatorStack.Peek().Type != TokenType.LeftParenthesis)
                        {
                            this.postfixExpression.Enqueue(operatorStack.Pop());
                        }

                        if (operatorStack.Count == 0)
                            throw new InvalidSyntaxException("Mismatched parentheses!");
                        else
                            operatorStack.Pop();

                        if (operatorStack.Peek().Type == TokenType.Function)
                            this.postfixExpression.Enqueue(operatorStack.Pop());
                        break;

                    case TokenType.Function:
                        operatorStack.Push(token);
                        break;

                    case TokenType.Comma:
                        while (operatorStack.Count > 0
                            && operatorStack.Peek().Type != TokenType.LeftParenthesis)
                        {
                            this.postfixExpression.Enqueue(operatorStack.Pop());
                        }
                        if (operatorStack.Count == 0)
                            throw new InvalidSyntaxException("Syntax error! Mismatched parentheses/misplaced comma");
                        break;

                    default:
                        break;
                }
            } while (inputQueue.Count > 0);

            while (operatorStack.Count > 0)
            {
                if (operatorStack.Peek().Type == TokenType.LeftParenthesis)
                    throw new InvalidSyntaxException("Mismatched parentheses!");

                this.postfixExpression.Enqueue(operatorStack.Pop());
            }
        }

        //Attempts to evaluate without substitution.
        public double Evaluate()
        {
            return this.Evaluate(this.postfixExpression);
        }

        
        //Substitutes x into the expression and evaluates.
        public double Evaluate(double x)
        {
            Stack<double> args = new Stack<double>();

            foreach (Token token in this.postfixExpression)
            {
                switch (token.Type)
                {
                    case TokenType.Literal:
                        if (token.Data == "x")
                            args.Push(x);
                        else
                        {
                            double num;
                            if (!double.TryParse(token.Data, out num))
                                throw new InvalidSyntaxException("Unexpected error: Encountered token " + token.Data);
                            args.Push(num);
                        }
                        break;
                    case TokenType.Operator:
                    case TokenType.Function:
                        args.Push(this.Calculate(token.Data, args));
                        break;
                    default:
                        throw new InvalidSyntaxException("Unexpected error: Encountered token " + token.Data);
                }
            }

            if (args.Count != 1)
                throw new InvalidSyntaxException("Invalid number of literals");

            return args.Pop();
        }
        
        /// <summary>
        /// Finds root of the expression between a and b.
        /// </summary>
        /// <exception cref="Sandy.Expression.ComputationException"></exception>
        /// <exception cref="Sandy.Expression.InvalidSyntaxException"></exception>
        /// <param name="a">Lower bound of the search interval</param>
        /// <param name="b">Upper bound of the search interval</param>
        /// <returns>Root in [a, b]</returns>
        public double Root(double a, double b)
        {
            if (!this.isFunction)
                throw new ComputationException("The expression is not a function of x!");
            
            if (!(this.Evaluate(a) * this.Evaluate(b) < 0))
                throw new ComputationException("Root is not present in the given interval!");
            
            int i = 1;
            double m = (a + b) / 2;
            while(i <= 100 && Math.Abs(this.Evaluate(m)) > 1e-10)
            {
                double fa, fb;
                fa = this.Evaluate(a);
                fb = this.Evaluate(b);
                m = (a * fb - b * fa) / (fb - fa);

                if (this.Evaluate(a) * this.Evaluate(m) < 0)
                    b = m;
                else
                    a = m;
            }

            return Math.Round(m, 10);
        }

        //Evaluates a postfix expression.
        //Assumes that substitution has already been done.
        private double Evaluate(Queue<Token> tokens)
        {
            Stack<double> args = new Stack<double>();

            foreach (Token token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.Literal:
                        args.Push(double.Parse(token.Data));
                        break;
                    case TokenType.Operator:
                    case TokenType.Function:
                        args.Push(this.Calculate(token.Data, args));
                        break;
                    default:
                        throw new InvalidSyntaxException("Unexpected error: Encountered token " + token.Data);
                }
            }

            if (args.Count != 1)
                throw new InvalidSyntaxException("Invalid number of literals");

            return args.Pop();
        }

        private double Calculate(string operation, Stack<double> args)
        {
            try
            {
                switch (operation)
                {
                    case "+":
                        {
                            double b = args.Pop();
                            double a = args.Pop();
                            return a + b;
                        }

                    case "-":
                        {
                            double b = args.Pop();
                            double a = args.Pop();
                            return a - b;
                        }

                    case "*":
                        {
                            double b = args.Pop();
                            double a = args.Pop();
                            return a * b;
                        }

                    case "/":
                        {
                            double b = args.Pop();
                            double a = args.Pop();
                            return a / b;
                        }

                    case "^":
                        {
                            double b = args.Pop();
                            double a = args.Pop();

                            return Math.Pow(a, b);
                        }

                    case "unary_minus":
                        return -args.Pop();

                    case "ln":
                        {
                            double a = args.Pop();
                            return Math.Log(a);
                        }

                    case "log":
                        {
                            double b = args.Pop();
                            double a = args.Pop();
                            return Math.Log(a, b);
                        }

                    case "sin":
                        {
                            double a = args.Pop();
                            return Math.Sin(a);
                        }

                    case "cos":
                        {
                            double a = args.Pop();
                            return Math.Cos(a);
                        }

                    case "tan":
                        {
                            double a = args.Pop();
                            return Math.Tan(a);
                        }

                    case "exp":
                        {
                            double a = args.Pop();
                            return Math.Exp(a);
                        }

                    case "sec":
                        {
                            double a = args.Pop();
                            return 1 / Math.Cos(a);
                        }

                    case "cosec":
                        {
                            double a = args.Pop();
                            return 1 / Math.Sin(a);
                        }

                    case "cot":
                        {
                            double a = args.Pop();
                            return 1 / Math.Tan(a);
                        }

                    default:
                        throw new InvalidSyntaxException(string.Format("Unexpected token '{0}'", operation));
                }
            }
            catch(InvalidOperationException)
            {
                throw new InvalidSyntaxException("Not enough operands!");
            }
        }

        //Tokenizes a string.
        private Queue<Token> Tokenize(string expression)
        {
            Regex whitespace = new Regex(@"\s");
            expression = whitespace.Replace(expression, string.Empty);
            
            Regex literal = new Regex(@"(\d+((\.\d+)*))|(?<!\w+)x(?!\w+)");
            Regex op = new Regex(@"(?<=[)]|(\d+((\.\d+)*))|(?<!\w+)x(?!\w+))[+\-*/\^]");
            Regex unaryMinus = new Regex(@"^-|(?<=[(+\-*/^])-(?=((\d+((\.\d+)*))|(?<!\w+)x(?!\w+))|(^[A-Za-z]+(?=[(])|(?<=[+\-*/\^])[A-Za-z]+(?=[(])))");
            Regex leftParenthesis = new Regex(@"[(]");
            Regex rightParenthesis = new Regex(@"[)]");
            Regex function = new Regex(@"^[A-Za-z]+(?=[(])|(?<=[+\-*/\^])[A-Za-z]+(?=[(])");
            Regex comma = new Regex(@",");

            MatchCollection literalMatches = literal.Matches(expression);
            MatchCollection opMatches = op.Matches(expression);
            MatchCollection unaryMinusMatches = unaryMinus.Matches(expression);
            MatchCollection leftParenthesisMatches = leftParenthesis.Matches(expression);
            MatchCollection rightParenthesisMatches = rightParenthesis.Matches(expression);
            MatchCollection functionMatches = function.Matches(expression);
            MatchCollection commaMatches = comma.Matches(expression);

            SortedDictionary<int, Token> tokenMap = new SortedDictionary<int, Token>();

            foreach(Match match in literalMatches)
            {
                tokenMap.Add(match.Index, new Token(match.Value, TokenType.Literal));
            }

            foreach (Match match in opMatches)
            {
                tokenMap.Add(match.Index, new Token(match.Value, TokenType.Operator));
            }

            foreach (Match match in unaryMinusMatches)
            {
                tokenMap.Add(match.Index, new Token("unary_minus", TokenType.Operator));
            }

            foreach (Match match in leftParenthesisMatches)
            {
                tokenMap.Add(match.Index, new Token(match.Value, TokenType.LeftParenthesis));
            }

            foreach (Match match in rightParenthesisMatches)
            {
                tokenMap.Add(match.Index, new Token(match.Value, TokenType.RightParenthesis));
            }

            foreach (Match match in functionMatches)
            {
                tokenMap.Add(match.Index, new Token(match.Value, TokenType.Function));
            }

            foreach (Match match in commaMatches)
            {
                tokenMap.Add(match.Index, new Token(match.Value, TokenType.Comma));
            }

            Queue<Token> outputQueue = new Queue<Token>(tokenMap.Values);

            return outputQueue;
        }

        //Maps an operator to a precedence value.
        private int PrecedenceOf(string op)
        {
            switch (op)
            {
                case "+":
                case "-":
                    return 1;

                case "*":
                case "/":
                    return 2;

                case "^":
                    return 3;

                case "unary_minus":
                    return 4;

                default:
                    return 0;
            }
        }

        //Maps an operator to its associativity.
        private Associativity AssociativityOf(string op)
        {
            switch (op)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                    return Associativity.Left;

                case "^":
                    return Associativity.Right;

                default:
                    return Associativity.Left;
            }
        }
    }
}
