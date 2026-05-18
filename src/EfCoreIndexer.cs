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
            })
            .ToListAsync();

        return entities.Select(e => ToMeilisearchItem(e.Item, e.Actors, e.Directors, e.Writers)).ToImmutableList();
    }

    private static MeilisearchItem ToMeilisearchItem(BaseItemEntity item, string[] actors, string[] directors, string[] writers)
    {
        return new MeilisearchItem(
            Guid: item.Id.ToString(),
            Type: item.Type,
            ParentId: item.ParentId.ToString(),
            Name: item.Name,
            Overview: item.Overview,
            OriginalTitle: item.OriginalTitle,
            SeriesName: item.SeriesName,
            Studios: item.Studios?.Split('|'),
            Genres: item.Genres?.Split('|'),
            Tags: item.Tags?.Split('|'),
            CommunityRating: item.CommunityRating,
            ProductionYear: item.ProductionYear,
            Path: item.Path?[0] == '%' ? null : item.Path,
            Artists: item.Artists?.Split('|'),
            AlbumArtists: item.AlbumArtists?.Split('|'),
            CriticRating: item.CriticRating,
            IsFolder: item.IsFolder,
            Tagline: item.Tagline,
            SortName: item.SortName,
            Actors: actors.Length > 0 ? actors : null,
            Directors: directors.Length > 0 ? directors : null,
            Writers: writers.Length > 0 ? writers : null
        );
    }
}
