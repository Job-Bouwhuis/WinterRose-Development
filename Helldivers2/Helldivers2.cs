namespace WinterRose.Helldivers2;

using System.Net.Http.Headers;

public sealed class Helldivers2 : IDisposable
{
    private readonly HelldiversClient _client;
    private readonly HttpClient _httpClient;

    public const string SUPER_CLIENT_HEADER_VALUE = "WinterRose-UtilityApp";
    public const string SUPER_CONTACT_HEADER_VALUE = "Discord: thesnowowl - Email: thesnowowl738@pm.me";

    public Helldivers2()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));

        _client = new HelldiversClient("https://api.helldivers2.dev/", _httpClient);
        client = this;
    }

    public static Helldivers2 client { get; private set; }

    #region Raw /raw/api/... Endpoints

    public Task<WarId> GetRawApiWarSeasonCurrentWarIDAsync()
        => _client.GetRawApiWarSeasonCurrentWarIDAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<WarStatus> GetRawApiWarSeason801StatusAsync()
        => _client.GetRawApiWarSeason801StatusAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<WarInfo> GetRawApiWarSeason801WarInfoAsync()
        => _client.GetRawApiWarSeason801WarInfoAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<WarSummary> GetRawApiStatsWar801SummaryAsync()
        => _client.GetRawApiStatsWar801SummaryAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<ICollection<NewsFeedItem>> GetRawApiNewsFeed801Async()
        => _client.GetRawApiNewsFeed801Async(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<ICollection<Assignment>> GetRawApiV2AssignmentWar801Async()
        => _client.GetRawApiV2AssignmentWar801Async(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<ICollection<Assignment>> GetRawApiV2SpaceStationWar801Async(
        long index)
        => _client.GetRawApiV2SpaceStationWar801Async(index, SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    #endregion

    #region v1 endpoints

    public Task<War> GetApiV1WarAsync()
        => _client.GetApiV1WarAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<ICollection<Assignment>> GetApiV1AssignmentsAllAsync()
        => _client.GetApiV1AssignmentsAllAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<Assignment> GetApiV1AssignmentsAsync(long index)
        => _client.GetApiV1AssignmentsAsync(index, SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<ICollection<Campaign2>> GetApiV1CampaignsAllAsync()
        => _client.GetApiV1CampaignsAllAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<Campaign2> GetApiV1CampaignsAsync(int index)
        => _client.GetApiV1CampaignsAsync(index, SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<ICollection<Dispatch>> GetApiV1DispatchesAllAsync()
        => _client.GetApiV1DispatchesAllAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<Dispatch> GetApiV1DispatchesAsync(int index)
        => _client.GetApiV1DispatchesAsync(index, SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<ICollection<Planet>> GetApiV1PlanetsAllAsync()
        => _client.GetApiV1PlanetsAllAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<Planet> GetApiV1PlanetsAsync(int index)
        => _client.GetApiV1PlanetsAsync(index, SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<ICollection<Planet>> GetApiV1PlanetEventsAsync()
        => _client.GetApiV1PlanetEventsAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<ICollection<SteamNews>> GetApiV1SteamAsync()
        => _client.GetApiV1SteamAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    #endregion

    #region v2 endpoints

    public Task<ICollection<Dispatch2>> GetApiV2DispatchesAllAsync()
        => _client.GetApiV2DispatchesAllAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<Dispatch2> GetApiV2DispatchesAsync(int index)
        => _client.GetApiV2DispatchesAsync(index, SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<ICollection<SpaceStation>> GetApiV2SpaceStationsAllAsync()
        => _client.GetApiV2SpaceStationsAllAsync(SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    public Task<SpaceStation> GetApiV2SpaceStationsAsync(long index)
        => _client.GetApiV2SpaceStationsAsync(index, SUPER_CLIENT_HEADER_VALUE, SUPER_CONTACT_HEADER_VALUE);

    #endregion

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

