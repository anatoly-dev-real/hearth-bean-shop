-- Categories and products are seeded by the idempotent C# DataSeeder.
-- Demo account passwords must be configured with dotnet user-secrets.
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Slug = N'bread')
    INSERT dbo.Categories (Name, Slug, SortOrder, IsAvailable) VALUES (N'Bread', N'bread', 0, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Slug = N'pastries')
    INSERT dbo.Categories (Name, Slug, SortOrder, IsAvailable) VALUES (N'Pastries', N'pastries', 1, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Slug = N'cakes')
    INSERT dbo.Categories (Name, Slug, SortOrder, IsAvailable) VALUES (N'Cakes', N'cakes', 2, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Slug = N'coffee')
    INSERT dbo.Categories (Name, Slug, SortOrder, IsAvailable) VALUES (N'Coffee', N'coffee', 3, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Slug = N'seasonal')
    INSERT dbo.Categories (Name, Slug, SortOrder, IsAvailable) VALUES (N'Seasonal', N'seasonal', 4, 1);