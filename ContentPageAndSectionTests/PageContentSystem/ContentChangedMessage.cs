namespace ContentPageAndSectionTests.PageContentSystem;

public sealed record ContentChangedMessage(
    IReadOnlyCollection<string> Keys);