# Phase 1 — Database Migration

## Goal

Rename `Asset.Title` to `Asset.UniqueName`, enforce global uniqueness at the database level, and make `Description` NOT NULL.

---

## Why These Changes

### `Title` → `UniqueName`

The original `Title` column had no uniqueness constraint. Any two users could publish assets with the same name. The domain decision is that an asset's name must be a **permanent, globally unique identifier** — similar to a package name on npm or a repository slug on GitHub. Renaming the column from `title` to `unique_name` reflects this semantic change throughout the stack.

### Unique Index

The uniqueness constraint is enforced **at the database level** as the final guard. Application-level checks (`ExistsByUniqueNameAsync`) handle the common path, but two concurrent publish requests can both pass the application check before either commits. The database index is the only reliable guard in that scenario.

### `Description` NOT NULL

The publish screen requires a description. Making it NOT NULL at the database level enforces that a published asset always has a description, regardless of how the data was inserted.

---

## Files Changed

| File | Change |
|---|---|
| `WorxForge.Infrastructure/Configuration/WorxForgeDbContext.cs` | Added `IsRequired()`, `HasMaxLength()`, and `HasIndex().IsUnique()` for `UniqueName`; `IsRequired()` + `HasMaxLength()` for `Description` |
| `WorxForge.Infrastructure/Migrations/20260806112754_RenameAssetTitleToUniqueName.cs` | Generated migration |
| `WorxForge.Infrastructure/Migrations/WorxForgeDbContextModelSnapshot.cs` | Auto-updated by EF tooling |

---

## DbContext Configuration

```csharp
private static void ConfigureAsset(EntityTypeBuilder<Asset> entity)
{
    entity.ToTable("ASSET");
    entity.HasKey(e => e.AssetId);
    entity.Property(e => e.UniqueName).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
    entity.HasIndex(e => e.UniqueName).IsUnique();
    entity.HasOne<User>()
        .WithMany()
        .HasForeignKey(e => e.AuthorId)
        .OnDelete(DeleteBehavior.NoAction);
    // ... remaining FK relationships
    ApplySnakeCaseColumnNames(entity);
}
```

The `ApplySnakeCaseColumnNames` helper converts `UniqueName` → `unique_name` automatically.

---

## Migration SQL (simplified)

```sql
-- Drop old title column
ALTER TABLE [ASSET] DROP COLUMN [title];

-- Make description NOT NULL with max length
ALTER TABLE [ASSET] ALTER COLUMN [description] nvarchar(1000) NOT NULL;

-- Add unique_name column
ALTER TABLE [ASSET] ADD [unique_name] nvarchar(200) NOT NULL DEFAULT N'';

-- Enforce uniqueness
CREATE UNIQUE INDEX [IX_ASSET_unique_name] ON [ASSET] ([unique_name]);
```

---

## Column Constraints Summary

| Column | Type | Nullable | Constraint |
|---|---|---|---|
| `unique_name` | nvarchar(200) | NOT NULL | UNIQUE INDEX (`IX_ASSET_unique_name`) |
| `description` | nvarchar(1000) | NOT NULL | — |
| `markdown_content` | nvarchar(max) | NOT NULL | — |

---

## Applying the Migration

Applied automatically on startup, or manually:

```bash
cd src/WorxForge.Infrastructure
dotnet ef database update --startup-project ../WorxForge.API
```

---

## Rollback

```bash
dotnet ef database update 20260804224229_ChangeUserGoogleIdToString --startup-project ../WorxForge.API
```

The `Down()` method restores `title` as a required column and makes `description` nullable again.
