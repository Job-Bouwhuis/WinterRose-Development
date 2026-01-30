using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.AssetPipeline;

namespace GiftApp;

internal class MessageList
{
    private Dictionary<string, double> messages = [];
    private List<string> seenMessages = [];

    public MessageList()
    {
        if (Assets.Exists("seen"))
            seenMessages = new List<string>(Assets.Load<List<string>>("seen"));
        else
            Assets.CreateAsset<List<string>>("seen");

        messages = Assets.Load<Dictionary<string, double>>("GiftApp.Assets.messages");
    }

    public string GetRandomMessage()
    {
        List<KeyValuePair<string, double>> unseenMessages =
            messages.Where(m => !seenMessages.Contains(m.Key)).ToList();

        if (unseenMessages.Count == 0)
        {
            seenMessages.Clear();
            Assets.Save("seen", seenMessages);
            unseenMessages = messages.ToList();
        }

        double totalWeight = unseenMessages.Sum(m => m.Value);
        double roll = Random.Shared.NextDouble() * totalWeight;

        double accumulator = 0;
        foreach (var pair in unseenMessages)
        {
            accumulator += pair.Value;
            if (roll <= accumulator)
            {
                seenMessages.Add(pair.Key);
                Assets.Save("seen", seenMessages);
                return pair.Key;
            }
        }

        string fallback = unseenMessages[0].Key;
        seenMessages.Add(fallback);
        Assets.Save("seen", seenMessages);
        return fallback;
    }

}
