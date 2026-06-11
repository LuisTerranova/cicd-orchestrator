using System.Text.RegularExpressions;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Infrastructure.Services;

public sealed class ConditionEvaluator : IConditionEvaluator
{
    public bool Evaluate(string expression, BuildContext context)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return true;

        var vars = new Dictionary<string, object>
        {
            ["branch"] = context.Branch,
            ["event"] = context.Event,
            ["actor"] = context.Actor,
            ["repo"] = context.Repo,
            ["tag"] = context.Tag ?? "",
            ["is_pr"] = context.IsPr,
            ["is_main"] = context.IsMain,
        };

        var result = EvaluateExpression(expression.Trim(), vars);
        return result is true;
    }

    private static object EvaluateExpression(string expr, Dictionary<string, object> vars)
    {
        expr = expr.Trim();

        // Handle parenthesized sub-expressions first
        while (expr.Contains('(') && !IsFunctionCall(expr))
        {
            expr = ResolveParens(expr, vars);
        }

        // Handle NOT
        var notMatch = Regex.Match(expr, @"^!\s*(.+)$");
        if (notMatch.Success)
        {
            var inner = EvaluateExpression(notMatch.Groups[1].Value, vars);
            return inner is bool boolVal ? !boolVal : false;
        }

        // Handle OR (lowest precedence)
        var orParts = SplitByLogicalOp(expr, "||");
        if (orParts.Count > 1)
        {
            foreach (var part in orParts)
            {
                if (EvaluateExpression(part, vars) is true)
                    return true;
            }
            return false;
        }

        // Handle AND
        var andParts = SplitByLogicalOp(expr, "&&");
        if (andParts.Count > 1)
        {
            foreach (var part in andParts)
            {
                var result = EvaluateExpression(part, vars);
                if (result is not true)
                    return false;
            }
            return true;
        }

        // Handle function calls: contains(), startsWith(), endsWith()
        if (IsFunctionCall(expr))
        {
            return EvaluateFunction(expr, vars);
        }

        // Handle 'in' operator: value in [a, b, c]
        var inMatch = Regex.Match(expr, @"^(.+?)\s+in\s+\[(.+)\]$");
        if (inMatch.Success)
        {
            var left = ResolveValue(inMatch.Groups[1].Value.Trim(), vars);
            var rightStr = inMatch.Groups[2].Value;
            var items = rightStr.Split(',')
                .Select(x => ResolveValue(x.Trim(), vars)?.ToString())
                .Where(x => x != null)
                .ToHashSet();
            return items.Contains(left?.ToString());
        }

        // Handle ==
        var eqMatch = Regex.Match(expr, @"^(.+?)\s*==\s*(.+)$");
        if (eqMatch.Success)
        {
            var left = ResolveValue(eqMatch.Groups[1].Value.Trim(), vars);
            var right = ResolveValue(eqMatch.Groups[2].Value.Trim(), vars);
            return Equals(left, right);
        }

        // Handle !=
        var neqMatch = Regex.Match(expr, @"^(.+?)\s*!=\s*(.+)$");
        if (neqMatch.Success)
        {
            var left = ResolveValue(neqMatch.Groups[1].Value.Trim(), vars);
            var right = ResolveValue(neqMatch.Groups[2].Value.Trim(), vars);
            return !Equals(left, right);
        }

        // Bare variable or bool literal
        return ResolveValue(expr, vars) is bool b && b;
    }

    private static List<string> SplitByLogicalOp(string expr, string op)
    {
        var parts = new List<string>();
        var depth = 0;
        var start = 0;

        for (var i = 0; i <= expr.Length - op.Length; i++)
        {
            if (expr[i] == '(') depth++;
            if (expr[i] == ')') depth--;

            if (depth == 0 && expr.Substring(i, op.Length) == op)
            {
                parts.Add(expr[start..i].Trim());
                start = i + op.Length;
                i += op.Length - 1;
            }
        }

        if (start < expr.Length)
            parts.Add(expr[start..].Trim());

        return parts;
    }

    private static string ResolveParens(string expr, Dictionary<string, object> vars)
    {
        var depth = 0;
        var start = -1;

        for (var i = 0; i < expr.Length; i++)
        {
            if (expr[i] == '(' && (i == 0 || !char.IsLetter(expr[i - 1])))
            {
                if (depth == 0) start = i;
                depth++;
            }
            else if (expr[i] == ')')
            {
                depth--;
                if (depth == 0 && start >= 0)
                {
                    var inner = expr[(start + 1)..i];
                    var result = EvaluateExpression(inner, vars);
                    return expr[..start] + FormatResult(result) + expr[(i + 1)..];
                }
            }
        }

        return expr;
    }

    private static bool IsFunctionCall(string expr)
    {
        return Regex.IsMatch(expr, @"^(contains|startsWith|endsWith)\s*\(");
    }

    private static object EvaluateFunction(string expr, Dictionary<string, object> vars)
    {
        var funcMatch = Regex.Match(expr, @"^(contains|startsWith|endsWith)\((.+?),\s*(.+)\)$");
        if (!funcMatch.Success)
            return false;

        var funcName = funcMatch.Groups[1].Value;
        var arg1 = ResolveValue(funcMatch.Groups[2].Value.Trim(), vars)?.ToString() ?? "";
        var arg2 = ResolveValue(funcMatch.Groups[3].Value.Trim(), vars)?.ToString() ?? "";

        return funcName switch
        {
            "contains" => arg1.Contains(arg2, StringComparison.Ordinal),
            "startsWith" => arg1.StartsWith(arg2, StringComparison.Ordinal),
            "endsWith" => arg1.EndsWith(arg2, StringComparison.Ordinal),
            _ => false,
        };
    }

    private static object? ResolveValue(string token, Dictionary<string, object> vars)
    {
        if (token == "true") return true;
        if (token == "false") return false;

        if (token.StartsWith('"') && token.EndsWith('"'))
            return token[1..^1];
        if (token.StartsWith('\'') && token.EndsWith('\''))
            return token[1..^1];

        if (int.TryParse(token, out var intVal))
            return intVal;

        if (vars.TryGetValue(token, out var val))
            return val;

        return null;
    }

    private static string FormatResult(object result)
    {
        return result is bool b ? b.ToString().ToLowerInvariant() : result.ToString() ?? "null";
    }
}
