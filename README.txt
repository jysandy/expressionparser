This is a simple infix expression parser.

The MathExpression class represents an expression that can be evaluated. You can
create it with an infix expression in the form of a string. The result can be
evaluated by calling the Evaluate() method.

The expresion may contain parentheses, basic arithmetic operations and the following 
transcendental functions:
ln, log, sin, cos, tan, cot, sec, cosec, exp
The expression may also contain a variable 'x', whose value can be specified at evaluation time.
If the expression is a function of x, you can attempt to find its root between two points
'a' and 'b' by calling the Root(a, b) method.

Examples:

var expression = new MathExpression("3 * 4 + 2x");	//create a new expression
Console.WriteLine(expression.Evaluate(3));		//evaluate for x=3

try
{
	Console.WriteLine(expression.Root(-1, 4));	//attempt to find the root between -1 and 4
}
catch (ComputationException e)				//if the root is not found
{
	Console.WriteLine(e.Message);
}