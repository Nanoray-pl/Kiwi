using System.Text;
using System.Text.RegularExpressions;
using Nanoray.Kiwi;

namespace Kiwi.Tests;

internal sealed class ConstraintParser
{
    private static readonly Regex Regex = new("([^!]+)(<=|==|>=|[GL]?EQ)([^!]+)\\s*(!(?:required|strong|medium|weak))?");
    private const string Ops = "-+/*^";

    public interface ICassowaryVariableResolver
    {
        IVariable? ResolveVariable(string variableName);
        Expression? ResolveConstant(string name);
    }

    public static Constraint ParseConstraint(string constraintString, ICassowaryVariableResolver variableResolver)
    {
        var match = Regex.Match(constraintString) ?? throw new ArgumentException($"Could not parse {constraintString}");
        var variable = variableResolver.ResolveVariable(match.Groups[1].Value.Trim()) ?? throw new ArgumentException($"Could not parse {constraintString}");
        var @operator = ParseOperator(match.Groups[2].Value.Trim());
        var expression = ResolveExpression(match.Groups[3].Value.Trim(), variableResolver);
        double strength = ParseStrength(match.Groups[4].Value.Trim());
        return new(variable - expression, @operator, strength);
    }

    private static RelationalOperator ParseOperator(string operatorString)
        => operatorString switch
        {
            "EQ" or "==" => RelationalOperator.Equal,
            "GEQ" or ">=" => RelationalOperator.GreaterThanOrEqual,
            "LEQ" or "<=" => RelationalOperator.LessThanOrEqual,
            _ => throw new ArgumentException($"Could not parse {operatorString}"),
        };

    private static double ParseStrength(string strengthString)
        => strengthString switch
        {
            "!required" or "" => Strength.Required,
            "!strong" => Strength.Strong,
            "!medium" => Strength.Medium,
            "!weak" => Strength.Weak,
            _ => throw new ArgumentException($"Could not parse {strengthString}"),
        };

    public static Expression ResolveExpression(string expressionString, ICassowaryVariableResolver variableResolver)
    {
        List<string> postfixExpression = InfixToPostfix(TokenizeExpression(expressionString));
        List<Expression> expressionStack = new();

        void Push(Expression expression)
            => expressionStack.Add(expression);

        Expression Pop()
        {
            var result = expressionStack[^1];
            expressionStack.RemoveAt(expressionStack.Count - 1);
            return result;
        }

        foreach (string expression in postfixExpression)
        {
            switch (expression)
            {
                case "+":
                    Push(Pop() + Pop());
                    break;
                case "-":
                    Push(Pop() - Pop());
                    break;
                case "*":
                    Push(Pop() * Pop());
                    break;
                case "/":
                    {
                        var rhs = Pop();
                        var lhs = Pop();
                        Push(lhs / rhs);
                        break;
                    }
                default:
                    if (variableResolver.ResolveConstant(expression) is { } constant)
                        Push(constant);
                    else if (variableResolver.ResolveVariable(expression) is { } variable)
                        Push(new(new Term(variable)));
                    else
                        throw new ArgumentException($"Could not parse {expressionString}");
                    break;

            }
        }

        return expressionStack[^1];
    }

    public static List<string> InfixToPostfix(List<string> tokenList)
    {
        List<int> stack = new();
        List<string> postfix = new();

        foreach (string token in tokenList)
        {
            char c = token[0];
            int idx = Ops.IndexOf(c);
            if (idx != -1 && token.Length == 1)
            {
                if (stack.Count == 0)
                {
                    stack.Add(idx);
                }
                else
                {
                    while (stack.Count != 0)
                    {
                        int prec2 = stack[^1] / 2;
                        int prec1 = idx / 2;
                        if (prec2 > prec1 || (prec2 == prec1 && c != '^'))
                        {
                            postfix.Add($"{Ops[stack[^1]]}");
                            stack.RemoveAt(stack.Count - 1);
                        }
                        else
                        {
                            break;
                        }
                    }
                    stack.Add(idx);
                }
            }
            else if (c == '(')
            {
                stack.Add(-2);
            }
            else if (c == ')')
            {
                while (stack[^1] != -2)
                {
                    postfix.Add($"{Ops[stack[^1]]}");
                    stack.RemoveAt(stack.Count - 1);
                }
                stack.RemoveAt(stack.Count - 1);
            }
            else
            {
                postfix.Add(token);
            }
        }

        while (stack.Count != 0)
        {
            postfix.Add($"{Ops[stack[^1]]}");
            stack.RemoveAt(stack.Count - 1);
        }

        return postfix;
    }

    public static List<string> TokenizeExpression(string expressionString)
    {
        List<string> tokenList = new();

        StringBuilder sb = new();
        foreach (char c in expressionString)
        {
            switch (c)
            {
                case '+': case '-': case '*': case '/': case '(': case ')':
                    if (sb.Length != 0)
                    {
                        tokenList.Add($"{sb}");
                        sb.Clear();
                    }
                    tokenList.Add($"{c}");
                    break;
                case ' ':
                    // ignore space
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        if (sb.Length != 0)
            tokenList.Add($"{sb}");
        return tokenList;
    }
}
