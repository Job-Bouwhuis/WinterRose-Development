using System.Globalization;

namespace WinterRose.Formatting.TimeFormats;

/// <summary>
/// Provides methods for formatting <see cref="DateTime"/> values using the
/// DateFormat expression language.
/// </summary>
/// <remarks>
/// <para>
/// DateFormat supports relative, absolute, and conditional date formatting.
/// Absolute date and time representations use the standard .NET
/// <see cref="DateTime"/> format strings.
/// </para>
/// <para>
/// The following formatters are supported:
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Format</term>
/// <description>Description</description>
/// </listheader>
/// <item>
/// <term><c>relative</c></term>
/// <description>
/// Formats the value as human-readable relative time, such as
/// <c>5 minutes ago</c>, <c>in 2 hours</c>, <c>yesterday</c>, or
/// <c>tomorrow</c>.
/// </description>
/// </item>
/// <item>
/// <term><c>relative[short]</c></term>
/// <description>
/// Formats relative time using a compact representation, such as
/// <c>5m ago</c>, <c>in 2h</c>, or <c>1d ago</c>.
/// </description>
/// </item>
/// <item>
/// <term><c>relative[no-calendar]</c></term>
/// <description>
/// Formats relative time without calendar-specific expressions such as
/// <c>yesterday</c>, <c>today</c>, or <c>tomorrow</c>.
/// </description>
/// </item>
/// <item>
/// <term><c>relative[short,no-calendar]</c></term>
/// <description>
/// Combines the <c>short</c> and <c>no-calendar</c> relative formatting options.
/// </description>
/// </item>
/// <item>
/// <term><c>date[FORMAT]</c></term>
/// <description>
/// Formats only the date portion using the supplied .NET
/// <see cref="DateTime"/> format string.
/// </description>
/// </item>
/// <item>
/// <term><c>time[FORMAT]</c></term>
/// <description>
/// Formats only the time portion using the supplied .NET
/// <see cref="DateTime"/> format string.
/// </description>
/// </item>
/// <item>
/// <term><c>datetime[FORMAT]</c></term>
/// <description>
/// Formats the complete date and time using the supplied .NET
/// <see cref="DateTime"/> format string.
/// </description>
/// </item>
/// </list>
/// <para>
/// Format expressions can contain conditional expressions using the following
/// syntax:
/// </para>
/// <code>
/// CONDITION?WHEN_TRUE:WHEN_FALSE
/// </code>
/// <para>
/// Duration conditions support the following comparison operators:
/// </para>
/// <list type="bullet">
/// <item><description><c>&lt;</c> — less than</description></item>
/// <item><description><c>&lt;=</c> — less than or equal to</description></item>
/// <item><description><c>&gt;</c> — greater than</description></item>
/// <item><description><c>&gt;=</c> — greater than or equal to</description></item>
/// <item><description><c>=</c> — equal to</description></item>
/// </list>
/// <para>
/// Supported duration units are:
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Unit</term>
/// <description>Meaning</description>
/// </listheader>
/// <item>
/// <term><c>ms</c></term>
/// <description>Milliseconds</description>
/// </item>
/// <item>
/// <term><c>s</c></term>
/// <description>Seconds</description>
/// </item>
/// <item>
/// <term><c>m</c></term>
/// <description>Minutes</description>
/// </item>
/// <item>
/// <term><c>h</c></term>
/// <description>Hours</description>
/// </item>
/// <item>
/// <term><c>d</c></term>
/// <description>Days</description>
/// </item>
/// <item>
/// <term><c>w</c></term>
/// <description>Weeks</description>
/// </item>
/// <item>
/// <term><c>mo</c></term>
/// <description>Months</description>
/// </item>
/// <item>
/// <term><c>y</c></term>
/// <description>Years</description>
/// </item>
/// </list>
/// <para>
/// The following keyword conditions are supported:
/// </para>
/// <list type="bullet">
/// <item><description><c>past</c></description></item>
/// <item><description><c>future</c></description></item>
/// <item><description><c>today</c></description></item>
/// <item><description><c>tomorrow</c></description></item>
/// <item><description><c>yesterday</c></description></item>
/// </list>
/// <para>
/// Conditions can be nested to create multiple formatting branches. For example:
/// </para>
/// <code>
/// &lt;1m?relative:&lt;1h?relative[short]:datetime[yyyy-MM-dd HH:mm]
/// </code>
/// <para>
/// This expression formats values less than one minute from the reference time
/// using normal relative formatting, values less than one hour using compact
/// relative formatting, and all other values using an absolute date and time.
/// </para>
/// <para>
/// The format strings supplied to <c>date[...]</c>, <c>time[...]</c>, and
/// <c>datetime[...]</c> use the standard .NET <see cref="DateTime"/> format
/// specifiers. For example:
/// </para>
/// <code>
/// date[yyyy-MM-dd]
/// time[HH:mm:ss]
/// datetime[dd MMM yyyy HH:mm]
/// </code>
/// </remarks>
public static class DateFormat
{
    /// <summary>
    /// Formats a <see cref="DateTime"/> value using the specified date format expression.
    /// </summary>
    /// <param name="value">
    /// The <see cref="DateTime"/> value to format.
    /// </param>
    /// <param name="format">
    /// The date format expression to apply to <paramref name="value"/>.
    /// The expression may contain absolute formatters such as
    /// <c>date[yyyy-MM-dd]</c>, <c>time[HH:mm]</c>, and
    /// <c>datetime[yyyy-MM-dd HH:mm]</c>, relative formatters such as
    /// <c>relative</c> and <c>relative[short]</c>, and conditional expressions
    /// such as <c>&lt;7d?relative:date[yyyy-MM-dd]</c>.
    /// </param>
    /// <returns>
    /// The formatted representation of <paramref name="value"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="format"/> is <see langword="null"/>, empty,
    /// or consists only of whitespace.
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="format"/> contains invalid syntax, an unknown
    /// formatter, an invalid condition, or an otherwise unsupported format expression.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Relative formats are evaluated against the current local date and time.
    /// For example, <c>relative</c> may produce values such as
    /// <c>just now</c>, <c>5 minutes ago</c>, <c>tomorrow</c>, or
    /// <c>in 2 hours</c>, depending on the value being formatted.
    /// </para>
    /// <para>
    /// This overload uses <see cref="DateTime.Now"/> as the reference point for
    /// evaluating relative formats and conditions. Use
    /// <see cref="Format(DateTime, string, DateTime)"/> when a specific reference
    /// time is required, such as when testing or formatting a value relative to
    /// a time other than the current local time.
    /// </para>
    /// <example>
    /// <code>
    /// string result = DateFormat.Format(
    ///     DateTime.Now.AddMinutes(-5),
    ///     "relative");
    ///
    /// // result: "5 minutes ago"
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// string result = DateFormat.Format(
    ///     DateTime.Now.AddDays(3),
    ///     "&lt;7d?relative:date[yyyy-MM-dd]");
    ///
    /// // result: "in 3 days"
    /// </code>
    /// </example>
    /// </remarks>
    public static string Format(DateTime value, string format)
    {
        return Format(value, format, DateTime.Now);
    }

    /// <summary>
    /// Formats a <see cref="DateTime"/> value using the specified date format expression
    /// and reference time.
    /// </summary>
    /// <param name="value">
    /// The <see cref="DateTime"/> value to format.
    /// </param>
    /// <param name="format">
    /// The date format expression to apply to <paramref name="value"/>.
    /// The expression may contain absolute formatters such as
    /// <c>date[yyyy-MM-dd]</c>, <c>time[HH:mm]</c>, and
    /// <c>datetime[yyyy-MM-dd HH:mm]</c>, relative formatters such as
    /// <c>relative</c> and <c>relative[short]</c>, and conditional expressions
    /// such as <c>&lt;7d?relative:date[yyyy-MM-dd]</c>.
    /// </param>
    /// <param name="now">
    /// The <see cref="DateTime"/> value to use as the reference point when evaluating
    /// relative formats and date conditions.
    /// </param>
    /// <returns>
    /// The formatted representation of <paramref name="value"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="format"/> is <see langword="null"/>, empty,
    /// or consists only of whitespace.
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="format"/> contains invalid syntax, an unknown
    /// formatter, an invalid condition, or an otherwise unsupported format expression.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Unlike the overload that does not accept a reference time, this method does
    /// not obtain the current time itself. The supplied <paramref name="now"/> value
    /// is used consistently for the entire formatting operation.
    /// </para>
    /// <para>
    /// Supplying the reference time explicitly is particularly useful when formatting
    /// multiple values against the same point in time, when producing deterministic
    /// output in tests, or when the reference time should differ from the current
    /// local time.
    /// </para>
    /// <para>
    /// The reference time affects only formatters and conditions that depend on the
    /// current time. Absolute formatters such as <c>date[...]</c>, <c>time[...]</c>,
    /// and <c>datetime[...]</c> format <paramref name="value"/> directly and do not
    /// depend on <paramref name="now"/>.
    /// </para>
    /// <example>
    /// <code>
    /// DateTime now = new(2026, 10, 5, 12, 0, 0);
    /// DateTime value = new(2026, 10, 5, 12, 5, 0);
    ///
    /// string result = DateFormat.Format(
    ///     value,
    ///     "relative",
    ///     now);
    ///
    /// // result: "in 5 minutes"
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// DateTime reference = new(2026, 10, 5, 12, 0, 0);
    ///
    /// string result = DateFormat.Format(
    ///     new DateTime(2026, 10, 20, 18, 30, 0),
    ///     "&lt;7d?relative:date[yyyy-MM-dd]",
    ///     reference);
    ///
    /// // result: "2026-10-20"
    /// </code>
    /// </example>
    /// </remarks>
    public static string Format(DateTime value, string format, DateTime now)
    {
        DateFormatNode node = new DateFormatParser().Parse(format);
        return FormatNode(value, node, now);
    }

    private static string FormatNode(
        DateTime value,
        DateFormatNode node,
        DateTime now)
    {
        return node switch
        {
            RelativeNode relative => FormatRelative(value, now, relative),
            DateNode date => value.ToString(date.Format, CultureInfo.CurrentCulture),
            TimeNode time => value.ToString(time.Format, CultureInfo.CurrentCulture),
            DateTimeNode dateTime => value.ToString(dateTime.Format, CultureInfo.CurrentCulture),
            ConditionalNode conditional => FormatConditional(value, conditional, now),
            _ => throw new InvalidOperationException(
                $"Unknown date format node '{node.GetType().Name}'.")
        };
    }

    private static string FormatConditional(
        DateTime value,
        ConditionalNode node,
        DateTime now)
    {
        return EvaluateCondition(value, node.Condition, now)
            ? FormatNode(value, node.WhenTrue, now)
            : FormatNode(value, node.WhenFalse, now);
    }

    private static bool EvaluateCondition(
        DateTime value,
        DateCondition condition,
        DateTime now)
    {
        return condition switch
        {
            DurationCondition duration => EvaluateDurationCondition(value, duration, now),
            KeywordCondition keyword => EvaluateKeywordCondition(value, keyword, now),
            _ => throw new InvalidOperationException(
                $"Unknown date condition '{condition.GetType().Name}'.")
        };
    }

    private static bool EvaluateDurationCondition(
        DateTime value,
        DurationCondition condition,
        DateTime now)
    {
        TimeSpan difference = (value - now).Duration();

        return condition.Comparison switch
        {
            DateComparison.LessThan => difference < condition.Duration,
            DateComparison.LessThanOrEqual => difference <= condition.Duration,
            DateComparison.GreaterThan => difference > condition.Duration,
            DateComparison.GreaterThanOrEqual => difference >= condition.Duration,
            DateComparison.Equal => difference == condition.Duration,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static bool EvaluateKeywordCondition(
        DateTime value,
        KeywordCondition condition,
        DateTime now)
    {
        return condition.Keyword.ToLowerInvariant() switch
        {
            "past" => value < now,
            "future" => value > now,
            "today" => value.Date == now.Date,
            "tomorrow" => value.Date == now.Date.AddDays(1),
            "yesterday" => value.Date == now.Date.AddDays(-1),
            _ => throw new FormatException(
                $"Unknown date condition '{condition.Keyword}'.")
        };
    }

    private static string FormatRelative(
    DateTime value,
    DateTime now,
    RelativeNode options)
    {
        TimeSpan difference = value - now;
        bool future = difference > TimeSpan.Zero;
        TimeSpan absolute = difference.Duration();

        if (options.Calendar)
        {
            string? calendar = TryFormatCalendar(value, now, future);

            if (calendar is not null)
                return options.Short
                    ? calendar
                    : calendar;
        }

        if (absolute < TimeSpan.FromSeconds(1))
            return options.Short ? "now" : "just now";

        if (absolute < TimeSpan.FromMinutes(1))
            return FormatRelativeUnit(
                absolute.TotalSeconds,
                "second",
                "s",
                future,
                options.Short);

        if (absolute < TimeSpan.FromHours(1))
            return FormatRelativeUnit(
                absolute.TotalMinutes,
                "minute",
                "m",
                future,
                options.Short);

        if (absolute < TimeSpan.FromDays(1))
            return FormatRelativeUnit(
                absolute.TotalHours,
                "hour",
                "h",
                future,
                options.Short);

        return FormatRelativeUnit(
            absolute.TotalDays,
            "day",
            "d",
            future,
            options.Short);
    }

    private static string FormatRelativeUnit(
    double amount,
    string unit,
    string shortUnit,
    bool future,
    bool shortFormat)
    {
        int value = Math.Max(1, (int)Math.Round(amount));

        if (shortFormat)
        {
            string result = $"{value}{shortUnit}";
            return future ? $"in {result}" : $"{result} ago";
        }

        string pluralUnit = value == 1 ? unit : $"{unit}s";

        return future
            ? $"in {value} {pluralUnit}"
            : $"{value} {pluralUnit} ago";
    }

    private static string? TryFormatCalendar(
    DateTime value,
    DateTime now,
    bool future)
    {
        DateTime date = value.Date;
        DateTime today = now.Date;

        int days = (date - today).Days;

        return days switch
        {
            -1 => "yesterday",
            0 => "today",
            1 => "tomorrow",
            _ => null
        };
    }
}