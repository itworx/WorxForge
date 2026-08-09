using System.Reflection;
using FluentAssertions;
using ErrorsCatalog = WorxForge.Domain.Errors.Errors;

namespace WorxForge.Domain.UnitTests.Errors;

public sealed class ErrorsCatalogTests
{
    private static IReadOnlyList<WorxForge.Domain.Errors.Error> AllCatalogEntries =>
        typeof(ErrorsCatalog)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(fieldInfo => fieldInfo.FieldType == typeof(WorxForge.Domain.Errors.Error))
            .Select(fieldInfo => (WorxForge.Domain.Errors.Error)fieldInfo.GetValue(null)!)
            .ToList();

    [Fact]
    public void EveryCode_IsUnique_SoACodeCanNeverMeanTwoDifferentThings()
    {
        var codes = AllCatalogEntries.Select(error => error.Code).ToList();

        codes.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void EveryEntry_HasANonEmptyMessage()
    {
        AllCatalogEntries.Should().OnlyContain(error => !string.IsNullOrWhiteSpace(error.Message));
    }
}
