USE [ASL]
GO

DECLARE @BannerInferno UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @BannerShadow  UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @BannerBulwark UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';

-- Cleanup
DELETE FROM [dbo].[GachaItem]  WHERE [GachaBannerId] IN (@BannerInferno, @BannerShadow, @BannerBulwark);
DELETE FROM [dbo].[GachaBanner] WHERE [Id]           IN (@BannerInferno, @BannerShadow, @BannerBulwark);

-- ──────────────────────────────────────────
-- BANNERS
-- ──────────────────────────────────────────
INSERT [dbo].[GachaBanner] ([Id],[Name],[Description],[BannerImagePath],
    [CostPerSinglePull],[CostPerMultiPull],[MultiPullCount],
    [PityThreshold],[HardPityThreshold],[IsActive],[StartDate],[EndDate],[CreatedDate])
VALUES
(@BannerInferno, N'Inferno Forge',
 N'The blade that sundered the Ashen Gate — forged once, never again',
 N'/images/banner.png', 100, 1000, 10, 10, 90, 1,
 '2026-04-16T15:39:45', '2026-05-17T15:39:45', GETDATE()),

(@BannerShadow, N'Shadow of the Abyss',
 N'Ancient relics pulled from the dimensional rift.',
 N'/images/shadow_banner.png', 100, 1000, 10, 10, 80, 1,
 '2026-04-20T00:00:00', '2026-06-20T00:00:00', GETDATE()),

(@BannerBulwark, N'Eternal Bulwark',
 N'Forged in the heart of a fallen kingdom, the ultimate defense.',
 N'/images/bulwark_banner.png', 120, 1200, 10, 10, 90, 1,
 '2026-04-20T00:00:00', '2026-05-20T00:00:00', GETDATE());

-- ──────────────────────────────────────────
-- GACHA ITEMS (tổng DropRate mỗi banner = 100%)
-- ──────────────────────────────────────────

-- Banner 1: Inferno Forge
INSERT [dbo].[GachaItem] ([Id],[GachaBannerId],[ItemId],[DropRate],[StarRating],[ItemCategory],[IsFeatured]) VALUES
(NEWID(), @BannerInferno, N'55555555-5555-5555-5555-555555555555', 0.6,  5, N'Weapon',     1), -- Godslayer Blade   (5★ featured)
(NEWID(), @BannerInferno, N'44444444-4444-4444-4444-444444444444', 5.1,  4, N'Armor',      1), -- Heroic Shield     (4★ featured)
(NEWID(), @BannerInferno, N'11111111-2222-3333-4444-555555555555', 94.3, 3, N'Consumable', 0); -- Basic Potion      (3★ filler)

-- Banner 2: Shadow of the Abyss
INSERT [dbo].[GachaItem] ([Id],[GachaBannerId],[ItemId],[DropRate],[StarRating],[ItemCategory],[IsFeatured]) VALUES
(NEWID(), @BannerShadow, N'9b644c96-6596-461a-a1fb-070add389deb', 0.8,  5, N'Weapon',    1), -- Ashenwhisper       (5★ featured)
(NEWID(), @BannerShadow, N'622b1b01-12cb-47ed-ab02-146293c6257d', 6.0,  4, N'Accessory', 1), -- Soulchain Amulet   (4★ featured)
(NEWID(), @BannerShadow, N'1922e575-6be6-4d41-ab2e-05ce357210db', 93.2, 3, N'Armor',     0); -- Shadowwalker Boots (3★ filler)

-- Banner 3: Eternal Bulwark
INSERT [dbo].[GachaItem] ([Id],[GachaBannerId],[ItemId],[DropRate],[StarRating],[ItemCategory],[IsFeatured]) VALUES
(NEWID(), @BannerBulwark, N'a34294ad-2ea6-4301-a836-0c8a60a1d661', 0.6,  5, N'Armor - Body', 1), -- The Last Bulwark         (5★ featured)
(NEWID(), @BannerBulwark, N'a44f8ea0-57b7-45c1-b7fb-4a1670440aa2', 5.0,  4, N'Armor - Leg',  1), -- Crimson Warden Greaves   (4★ featured)
(NEWID(), @BannerBulwark, N'ac6dd9db-532c-4d33-a392-30d1f10dbdc7', 94.4, 3, N'Armor - Body', 0); -- Iron-Plated Vest         (3★ filler)
GO