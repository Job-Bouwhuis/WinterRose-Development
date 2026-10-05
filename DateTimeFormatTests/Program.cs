using WinterRose.Formatting.TimeFormats;

Console.WriteLine(DateFormat.Format(DateTime.Now.AddDays(-6), "today?relative[short,no-calendar]:date[dd-MM-yyyy]"));
