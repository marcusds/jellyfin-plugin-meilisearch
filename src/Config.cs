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
}
