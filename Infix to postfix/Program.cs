using System.Text.RegularExpressions;

public class ExpressionEvaluator

{
    // store the precdence of the operatorys
    private Dictionary<string, int> operatorPrecedence = new Dictionary<string, int>
    {
        { "(",  10 },
        { ")",  10 },
        { "`",  9 },  // unary minus
        { "+",  6 },
        { "-",  6 },
        { "*",  7 },
        { "/",  7 },
        { "^",  8 },
        { ">",  5 },
        { "<",  5 },
        { ">=", 5 },
        { "<=", 5 },
        { "!=", 5 },
        { "=",  5 },
        { "&",  4 },
        { "||", 3 },
        { "!",  9 },
      };



    public object Evaluate(string infixExpression)
    {
        string postfixExpression = InfixToPostfix(infixExpression);
        return EvaluatePostfix(postfixExpression);
    }



    // Convert expression from infix to posfix
    public string InfixToPostfix(string infixExpression)
    {
        infixExpression = SimplifyOperatorSequences(infixExpression);
        string postfixExpression = "";
        Stack<string> operatorStack = new Stack<string>();

        for (int i = 0; i < infixExpression.Length; i++)
        {
            char c = infixExpression[i];

            //  unary minus
            if (c == '-' && (i == 0 || infixExpression[i - 1] == '(' || IsOperator(infixExpression[i - 1].ToString())))
            {
                operatorStack.Push("`"); // Push the unary minus symbol
            }
            else if (char.IsLetterOrDigit(c))
            {
                postfixExpression += c;
                if (i + 1 == infixExpression.Length || !char.IsLetterOrDigit(infixExpression[i + 1]))
                {
                    postfixExpression += " ";
                }
            }


            // Brackets

            else if (c == '(')
            {
                operatorStack.Push(c.ToString());
            }
            else if (c == ')')
            {
                while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                {
                    postfixExpression += operatorStack.Pop() + " ";
                }
                //if (operatorStack.Count == 0)
                //{
                //    throw new ArgumentException("Mismatched parentheses: more closing parentheses than opening ones.");
                //}
                operatorStack.Pop(); // Pop the '(' from the stack
            }


            // Operator

            else if (IsOperator(c.ToString()))
            {
                string op = c.ToString();
                if (i + 1 < infixExpression.Length)
                {
                    string potentialOperator = op + infixExpression[i + 1];
                    if (IsOperator(potentialOperator))
                    {
                        op = potentialOperator;
                        i++;
                    }
                }




                while (operatorStack.Count > 0 && GetPrecedence(op) <= GetPrecedence(operatorStack.Peek()))
                {
                    if (operatorStack.Peek() == "(")
                    {
                        break;
                    }

                    postfixExpression += operatorStack.Pop() + " ";
                }
                operatorStack.Push(op);

            }
        }

        while (operatorStack.Count > 0)
        {
            postfixExpression += operatorStack.Pop() + " ";
        }

        return postfixExpression.Trim();
    }


    private string SimplifyOperatorSequences(string expression)
    {

        expression = expression.Replace("--", "+");
        expression = expression.Replace("-+", "-");

        return expression;
    }


    private object EvaluatePostfix(string postfixExpression)
    {
        Stack<object> operandStack = new Stack<object>();
        string[] tokens = postfixExpression.Split(' ');

        foreach (string token in tokens)
        {

            if (int.TryParse(token, out int number))
            {
                operandStack.Push(number);

            }
            else if (IsOperator(token))
            {


                if (token == "!")
                {
                    int operand = (int)operandStack.Pop();
                    object result = PerformOperation(token, operand, operand);
                    operandStack.Push(result);
                }
                else
                {
                    int operand2 = (int)operandStack.Pop();
                    int operand1 = operandStack.Count > 0 ? (int)operandStack.Pop() : 0;
                    object result = PerformOperation(token, operand1, operand2);
                    operandStack.Push(result);
                }

            }
        }

        return operandStack.Pop();
    }




    private int GetPrecedence(string op)
    {
        if (operatorPrecedence.TryGetValue(op, out int precedence))
        {
            return precedence;
        }
        throw new ArgumentException("Invalid operator");
    }

    //Performs Operations
    private object PerformOperation(string op, int operand1, int operand2)
    {


        switch (op)
        {
            case "+":
                return operand1 + operand2;
            case "-":
                return operand1 - operand2;
            case "*":
                return operand1 * operand2;
            case "/":
                if (operand2 == 0)
                {
                    throw new DivideByZeroException("Division by zero is not allowed");
                }
                return operand1 / operand2;
            case "^":
                return (int)Math.Pow(operand1, operand2);
            case "`": // Unary minus
                return -operand2;
            case ">":
                return operand1 > operand2 ? "true" : "false";
            case "<":
                return operand1 < operand2 ? "true" : "false";
            case ">=":
                return operand1 >= operand2 ? "true" : "false";
            case "<=":
                return operand1 <= operand2 ? "true" : "false";
            case "=":
                return operand1 == operand2 ? "true" : "false";
            case "!=":
                return operand1 != operand2 ? "true" : "false";
            case "&":
                bool boolOperand1 = Convert.ToBoolean(operand1);
                bool boolOperand2 = Convert.ToBoolean(operand2);
                return boolOperand1 && boolOperand2 ? "true" : "false";
            case "||":
                bool boolOperand11 = Convert.ToBoolean(operand1);
                bool boolOperand12 = Convert.ToBoolean(operand2);
                return boolOperand11 || boolOperand12 ? "true" : "false";
            case "!":
                bool boolOperand = Convert.ToBoolean(operand1);
                return !boolOperand ? "true" : "false";
            default:
                throw new ArgumentException("Invalid operator");
        }
    }

    //Check if character is an arithmetic or comparison operator that expression evaluator 
    private bool IsOperator(string op)
    {
        return operatorPrecedence.ContainsKey(op);
    }

}





class Program
{
    static void Main()
    {

        Console.WriteLine("Enter any infix expression:");
        string inputExpression = Console.ReadLine();

        var variables = Regex.Matches(inputExpression, "[a-zA-Z]+")
                                 .Select(match => match.Value)
                                 .Distinct();


        foreach (var variable in variables)
        {
            Console.WriteLine($"Enter the value for {variable}:");
            string value = Console.ReadLine();
            inputExpression = inputExpression.Replace(variable, value);
        }




        ExpressionEvaluator evaluator = new ExpressionEvaluator();
        string postfixExpression = evaluator.InfixToPostfix(inputExpression);
        object result = evaluator.Evaluate(inputExpression);



        Console.WriteLine($"Postfix Expression: {postfixExpression}");
        Console.WriteLine("Result:");
        if (result is bool)
        {
            Console.WriteLine(result.ToString().ToLower());

        }
        else
        {
            Console.WriteLine(result);
        }



    }
}

