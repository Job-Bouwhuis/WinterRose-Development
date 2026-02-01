using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRoseUtilityApp.DrinkReminder;

internal class DrinkSettings
{
    public static DrinkSettings? Default { get;set; } = new DrinkSettings()
    {
        ReminderIntervalMinutes = 60,
        NotificationsEnabled = true,
        Volume = 1,
        Messages = [
            "Do not forget to drink some liquid!",
            "Drink water before you turn into a sea cucumber!",
            "Hydrate or suffer the wrath of dehydration!",
            "Water: it's not just for fish!",
            "Sip, sip, hooray! Time to drink!",
            "Even Subnautica aliens drink water!",
            "Warning: Hydration level critical. Drink water before your HP drops.",
            "Hull integrity stable... but you are not. Time to drink.",
            "Even on Planet 4546B you cannot survive without water.",
            "Your brain runs on water, not hope.",
            "Fish drink. You should too.",
            "This is not a drill. This is a drink reminder.",
            "Scans show: you are 60% dehydrated.",
            "Water found. Consumption recommended.",
            "Stop. Drink. Survive.",
            "Dehydration detected. Moral support unavailable.",
            "Do not become a dried-up leviathan. Drink water.",
            "You have gone too long without drinking.",
            "Diving is more fun with enough hydration.",
            "If you do not drink water now, we will laugh at you.",
            "Water is OP. Just kidding. Go drink.",
            "Even aliens understand hydration.",
            "Your body asks for water, not coffee.",
            "Achievement unlocked: Time to drink.",
            "Drink, so that Dajuska can have a fully functional boyfriend >:(",
            "I love you! Mwah <3\n- Dajuska"
        ]
    };

    public float Volume { get; set; } = 1;
    public List<string> Messages { get; set; } = new List<string>();
    
    public int ReminderIntervalMinutes { get; set; }
    public int ReminderIntervalMilliseconds => ReminderIntervalMinutes * 60 * 1000;
    public bool NotificationsEnabled { get; set; }
}
