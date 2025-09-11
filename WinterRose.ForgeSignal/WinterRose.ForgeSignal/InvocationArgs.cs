using System.Reflection;
using System.Text;

namespace WinterRose.ForgeSignal;

public sealed class InvocationArgs
{
    public static object? ValidateAndInvoke(Delegate method, params object?[]? args)
    {
        ParameterInfo[] parms = method.GetMethodInfo().GetParameters();

        if (parms.Length != args.Length)
        {
            throw new InvalidOperationException($"Expected parameter count of {parms.Length} but got {args.Length}");
        }

        for (int i = 0; i < parms.Length; i++)
        {
            object? given = args[i];
            Type expected = parms[i].ParameterType;

            if(given != null && !given.GetType().IsAssignableTo(expected))
            {
                throw new InvalidOperationException(FormatError(args, parms.Select(p => p.ParameterType).ToArray()));
            }
        }

        return method.DynamicInvoke(args);
    }

    private static string FormatError(object?[]? args, Type[] expected)
    {
        StringBuilder sb = new StringBuilder("Expected: (");
        if(expected.Length > 0)
        {
            foreach (Type e in expected)
            {
                sb.Append(e.Name).Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
        }

        sb.Append(") but got: (");

        if(args is not null)
        {
            foreach (object? e in args)
            {
                sb.Append(e is not null ? e.GetType().Name : "null").Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
        }

        sb.Append(")");

        return sb.ToString();
    }
}
