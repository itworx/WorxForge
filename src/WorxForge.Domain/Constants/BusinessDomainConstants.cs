namespace WorxForge.Domain.Constants;

// Fixed lookup rows so foreign keys and seed data stay stable across environments and re-seeds.
public static class BusinessDomainConstants
{
    public static readonly LookupEntry Development = new(new Guid("21111111-1111-1111-1111-111111111101"), "Development");
    public static readonly LookupEntry QualityAssurance = new(new Guid("21111111-1111-1111-1111-111111111102"), "Quality Assurance");
    public static readonly LookupEntry Product = new(new Guid("21111111-1111-1111-1111-111111111103"), "Product");
    public static readonly LookupEntry Architecture = new(new Guid("21111111-1111-1111-1111-111111111104"), "Architecture");
    public static readonly LookupEntry DevOps = new(new Guid("21111111-1111-1111-1111-111111111105"), "DevOps");
    public static readonly LookupEntry Security = new(new Guid("21111111-1111-1111-1111-111111111106"), "Security");
    public static readonly LookupEntry Data = new(new Guid("21111111-1111-1111-1111-111111111107"), "Data");
    public static readonly LookupEntry General = new(new Guid("21111111-1111-1111-1111-111111111108"), "General");

    public static readonly IReadOnlyList<LookupEntry> All =
    [
        Development, QualityAssurance, Product, Architecture, DevOps, Security, Data, General
    ];
}
