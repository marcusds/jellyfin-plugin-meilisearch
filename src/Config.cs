using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Meilisearch;

public class Config : BasePluginConfiguration
{
    public Config()
    {
        ApiKey = string.Empty;
        Url = string.Empty;
        Debug = false;
        IndexName = string.Empty;
        MatchingStrategy = "last";
        HybridSearchEnabled = false;
        HybridEmbedderName = string.Empty;
        HybridSemanticRatio = 0.5;
        OverviewMaxLength = 500;
        MaxListItems = 10;
    }

    public string ApiKey { get; set; }
    public string Url { get; set; }

    public bool Debug { get; set; }
    public string IndexName { get; set; }

    /// <summary>
    /// Meilisearch matchingStrategy: "last", "all", or "frequency".
    /// </summary>
    public string MatchingStrategy { get; set; }

    public bool HybridSearchEnabled { get; set; }

    /// <summary>
    /// Name of the Meilisearch embedder to use for hybrid/vector search (e.g. "cloudflare").
    /// Required when HybridSearchEnabled is true.
    /// </summary>
    public string HybridEmbedderName { get; set; }

    /// <summary>
    /// Balance between keyword and semantic results (0.0 = full keyword, 1.0 = full semantic).
    /// </summary>
    public double HybridSemanticRatio { get; set; }

    /// <summary>
    /// Maximum number of characters to index from the overview field. 0 = no limit.
    /// </summary>
    public int OverviewMaxLength { get; set; }

    /// <summary>
    /// Maximum number of items to index from list fields (actors, genres, tags, etc.). 0 = no limit.
    /// </summary>
    public int MaxListItems { get; set; }
}
