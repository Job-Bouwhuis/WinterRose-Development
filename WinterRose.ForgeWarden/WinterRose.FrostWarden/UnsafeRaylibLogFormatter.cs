using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden;
internal class UnsafeRaylibLogFormatter
{
    public static unsafe int GetVaListSize(sbyte* fmt)
    {
        int size = 0;

        for (int i = 0; ; i++)
        {
            char c = (char)fmt[i];
            if (c == '\0') break;

            if (c == '%' && fmt[i + 1] != '%')
            {
                i++; // skip %
                char spec = (char)fmt[i];

                // Treat all arguments as 8 bytes
                if (spec == 'i' || spec == 'f' || spec == 's')
                    size += 8;
                else
                    size += 0; // ignore unknown
            }
        }

        return size;
    }

    public static unsafe string FormatFromPointer(string msg, byte[] args)
    {
        var sb = new System.Text.StringBuilder();
        int offset = 0; // tracks position in args array

        for (int i = 0; i < msg.Length; i++)
        {
            char c = msg[i];
            if (c == '\0') break;

            if (c == '%' && i + 1 < msg.Length && msg[i + 1] != '%')
            {
                i++;
                char spec = msg[i];

                if (spec == 'i')
                {
                    // read 4 bytes as int, skip 8 bytes in args
                    int val = BitConverter.ToInt32(args, offset);
                    sb.Append(val);
                    offset += 8;
                }
                else if (spec == 'f')
                {
                    // read 8 bytes as double
                    double val = BitConverter.ToDouble(args, offset);
                    sb.Append(val);
                    offset += 8;
                }
                else if (spec == 's')
                {
                    // read 8 bytes as pointer (IntPtr) and convert to string
                    long ptrVal = BitConverter.ToInt64(args, offset);
                    string val = Marshal.PtrToStringUTF8((IntPtr)ptrVal) ?? "";
                    sb.Append(val);
                    offset += 8;
                }
                else
                {
                    sb.Append('%').Append(spec);
                }
            }
            else if (c == '%' && i + 1 < msg.Length && msg[i + 1] == '%')
            {
                // handle %%
                sb.Append('%');
                i++;
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

}
