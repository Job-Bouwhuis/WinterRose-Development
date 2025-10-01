using System;
using System.Net;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.NetworkServer;

public class IPAddressValueProvider : CustomValueProvider<IPAddress>
{
    public override object CreateString(IPAddress obj, ObjectSerializer serializer)
    {
        return obj.ToString();
    }

    public override IPAddress? CreateObject(object value, WinterForgeVM executor)
    {
        return IPAddress.Parse((string)value);
    }
}