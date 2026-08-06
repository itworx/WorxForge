namespace WorxForge.Core.Search;

// Parsed shape of a raw search query string, decoupled from how it gets turned into SQL.
public sealed record ParsedSearchQuery(IReadOnlyList<string> Terms, IReadOnlyList<string> Hashtags);

public static class SearchQueryParser
{
    public static ParsedSearchQuery Parse(string? rawQuery)
    {
        if (string.IsNullOrWhiteSpace(rawQuery))
        {
            return new ParsedSearchQuery([], []);
        }

        // Defensive: query binding already URL-decodes, but this guards against a raw/undecoded "%23devops" reaching here.
        var decoded = Uri.UnescapeDataString(rawQuery);

        var terms = new List<string>();
        var hashtags = new List<string>();
        var seenHashtags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawToken in decoded.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var token = rawToken.Trim();
            if (token.Length == 0)
            {
                continue;
            }

            if (token[0] == '#')
            {
                var tag = token[1..].Trim().ToLowerInvariant();
                if (tag.Length > 0 && seenHashtags.Add(tag))
                {
                    hashtags.Add(tag);
                }
            }
            else
            {
                terms.Add(token);
            }
        }

        return new ParsedSearchQuery(terms, hashtags);
    }
}
