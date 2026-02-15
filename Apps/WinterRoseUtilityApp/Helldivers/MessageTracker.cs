using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WinterRose.Helldivers2;
using WinterRose.Recordium;

namespace WinterRoseUtilityApp.Helldivers;

internal class MessageTracker
{
    private readonly Log log;
    private HashSet<long> _shownMajorOrderIds = new();
    private HashSet<int> _shownNewsFeedIds = new();

    private const int MessageAgeThresholdDays = 30;

    public MessageTracker(Log log)
    {
        this.log = log;
    }

    /// <summary>
    /// Checks if a news feed message should be shown based on:
    /// - Age (messages older than threshold are filtered)
    /// - Whether it has been shown before
    /// </summary>
    public bool ShouldShowNewsFeedItem(int messageId, DateTime published)
    {
        if (IsMessageTooOld(published))
        {
            return false;
        }

        // Only show if it hasn't been shown this session
        bool isNew = _shownNewsFeedIds.Add(messageId);
        return isNew;
    }

    /// <summary>
    /// Checks if a major order should be shown.
    /// Major Orders always show on startup (not cached), but tracked to avoid showing multiple times in same session.
    /// </summary>
    public bool ShouldShowMajorOrder(long orderId, DateTimeOffset expiration)
    {
        // Don't show expired orders
        if (DateTimeOffset.UtcNow > expiration)
        {
            return false;
        }

        // Only show if it hasn't been shown this session
        bool isNew = _shownMajorOrderIds.Add(orderId);
        return isNew;
    }

    /// <summary>
    /// Get all shown major order IDs for the current session
    /// </summary>
    public IReadOnlyCollection<long> GetShownMajorOrderIds() => _shownMajorOrderIds;

    private bool IsMessageTooOld(DateTime publishedAt)
    {
        try
        {
            int days = (DateTime.UtcNow - publishedAt).Days;
            return days > MessageAgeThresholdDays;
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error checking message age");
            return true;
        }
    }
}

