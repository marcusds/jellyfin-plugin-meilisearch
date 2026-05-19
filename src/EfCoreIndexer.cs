using System.Collections.Immutable;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Meilisearch;

public class EfCoreIndexer(
    IJellyfinDatabaseProvider dbProvider,
    MeilisearchClientHolder clientHolder,
    ILogger<EfCoreIndexer> logger
) : Indexer(clientHolder, logger)
{
    protected override async Task<ImmutableList<MeilisearchItem>> GetItems(IReadOnlySet<string> includedTypes)
    {
        using var context = dbProvider.DbContextFactory!.CreateDbContext();
        Status["Database"] = context.Database.GetDbConnection().ConnectionString;

        var entities = await context.BaseItems
            .AsNoTracking()
            .Where(x => x.Type != null && includedTypes.Contains(x.Type))
            .Select(x => new
            {
                Item = x,
                Actors = x.Peoples!
                    .Where(p => p.People.PersonType == "Actor" || p.People.PersonType == "VoiceActor")
                    .Select(p => p.People.Name).ToArray(),
                Directors = x.Peoples!
                    .Where(p => p.People.PersonType == "Director")
                    .Select(p => p.People.Name).ToArray(),
                Writers = x.Peoples!
                    .Where(p => p.People.PersonType == "Writer")
                    .Select(p => p.People.Name).ToArray(),
                Producers = x.Peoples!
                    .Where(p => p.People.PersonType == "Producer")
                    .Select(p => p.People.Name).ToArray(),
            })
            .ToListAsync();

        var config = Plugin.Instance?.Configuration;
        int overviewMax = config?.OverviewMaxLength ?? 500;
        int listMax = config?.MaxListItems ?? 10;
        return entities.Select(e => ToMeilisearchItem(e.Item, e.Actors, e.Directors, e.Writers, e.Producers, overviewMax, listMax)).ToImmutableList();
    }

    private static MeilisearchItem ToMeilisearchItem(BaseItemEntity item, string[] actors, string[] directors, string[] writers, string[] producers, int overviewMax, int listMax)
    {
        string[] Limit(string[]? arr) => arr is null ? [] : (listMax > 0 ? arr.Take(listMax).ToArray() : arr);
        string[] LimitSplit(string? s) => s is null ? [] : Limit(s.Split('|'));

        return new MeilisearchItem(
            Guid: item.Id.ToString(),
            Type: item.Type,
            ParentId: item.ParentId.ToString(),
            Name: item.Name,
            Overview: overviewMax > 0 && item.Overview?.Length > overviewMax ? item.Overview[..overviewMax] : item.Overview,
            OriginalTitle: item.OriginalTitle,
            SeriesName: item.SeriesName,
            Studios: LimitSplit(item.Studios),
            Genres: LimitSplit(item.Genres),
            Tags: LimitSplit(item.Tags),
            CommunityRating: item.CommunityRating,
            ProductionYear: item.ProductionYear,
            Path: item.Path?[0] == '%' ? null : item.Path,
            Artists: LimitSplit(item.Artists),
            AlbumArtists: LimitSplit(item.AlbumArtists),
            CriticRating: item.CriticRating,
            IsFolder: item.IsFolder,
            Tagline: item.Tagline,
            SortName: item.SortName,
            Actors: actors.Length > 0 ? Limit(actors) : null,
            Directors: directors.Length > 0 ? Limit(directors) : null,
            Writers: writers.Length > 0 ? Limit(writers) : null,
            Producers: producers.Length > 0 ? Limit(producers) : null,
            OfficialRating: item.OfficialRating
        );
    }
}
