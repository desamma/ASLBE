USE [ASL]
GO

-- =============================================
-- SEED ITEMS
-- Rarity: Common (3★), Rare (4★), Legendary (5★)
-- =============================================

-- ──────────────────────────────────────────
-- LEGENDARY (5★) — 1 item mỗi banner
-- ──────────────────────────────────────────
INSERT [dbo].[Items] ([Id],[DictionaryKey],[Name],[Description],[Type],[Rarity],[ImagePath],[IsGachaOnly],[IsActive],[StatsLines]) VALUES
-- Banner 1: Inferno Forge
(N'55555555-5555-5555-5555-555555555555', N'item_godslayer_blade',
 N'Godslayer Blade',
 N'A blade forged in the core of a dying star. Said to cleave through divine armor.',
 N'Weapon', N'Legendary', N'/images/items/godslayer_blade.png', 1, 1,
 N'["ATK +520","CRIT Rate +12%","Skill DMG +25%","Ignores 15% DEF"]'),

-- Banner 2: Shadow of the Abyss
(N'9b644c96-6596-461a-a1fb-070add389deb', N'item_ashenwhisper',
 N'Ashenwhisper',
 N'A cursed blade that whispers the names of those it has slain.',
 N'Weapon', N'Legendary', N'/images/items/ashenwhisper.png', 1, 1,
 N'["ATK +490","Shadow DMG +30%","CRIT DMG +40%","Lifesteal +8%"]'),

-- Banner 3: Eternal Bulwark
(N'a34294ad-2ea6-4301-a836-0c8a60a1d661', N'item_last_bulwark',
 N'The Last Bulwark',
 N'Armor of a fallen king who stood alone against ten thousand.',
 N'Armor - Body', N'Legendary', N'/images/items/last_bulwark.png', 1, 1,
 N'["DEF +680","HP +2400","Block Rate +18%","DMG Reduction +12%"]');

-- ──────────────────────────────────────────
-- RARE (4★) — 1 item mỗi banner
-- ──────────────────────────────────────────
INSERT [dbo].[Items] ([Id],[DictionaryKey],[Name],[Description],[Type],[Rarity],[ImagePath],[IsGachaOnly],[IsActive],[StatsLines]) VALUES
-- Banner 1: Inferno Forge
(N'44444444-4444-4444-4444-444444444444', N'item_heroic_shield',
 N'Heroic Shield',
 N'Tempered in volcanic fire, this shield has never been shattered.',
 N'Armor', N'Rare', N'/images/items/heroic_shield.png', 1, 1,
 N'["DEF +310","HP +800","Block Rate +10%"]'),

-- Banner 2: Shadow of the Abyss
(N'622b1b01-12cb-47ed-ab02-146293c6257d', N'item_soulchain_amulet',
 N'Soulchain Amulet',
 N'An amulet that binds fragments of defeated souls to amplify its wearer.',
 N'Accessory', N'Rare', N'/images/items/soulchain_amulet.png', 1, 1,
 N'["ATK +180","CRIT Rate +8%","Elemental RES +12%"]'),

-- Banner 3: Eternal Bulwark
(N'a44f8ea0-57b7-45c1-b7fb-4a1670440aa2', N'item_crimson_warden_greaves',
 N'Crimson Warden Greaves',
 N'Leg armor worn by the elite guards of the Crimson Citadel.',
 N'Armor - Leg', N'Rare', N'/images/items/crimson_greaves.png', 1, 1,
 N'["DEF +220","SPD +15","HP +600"]');

-- ──────────────────────────────────────────
-- COMMON (3★) — 1 item mỗi banner (filler)
-- ──────────────────────────────────────────
INSERT [dbo].[Items] ([Id],[DictionaryKey],[Name],[Description],[Type],[Rarity],[ImagePath],[IsGachaOnly],[IsActive],[StatsLines]) VALUES
-- Banner 1: Inferno Forge
(N'11111111-2222-3333-4444-555555555555', N'item_basic_potion',
 N'Basic Potion',
 N'A simple healing concoction used by novice adventurers.',
 N'Consumable', N'Common', N'/images/items/basic_potion.png', 0, 1,
 N'["Restores 500 HP on use"]'),

-- Banner 2: Shadow of the Abyss
(N'1922e575-6be6-4d41-ab2e-05ce357210db', N'item_shadowwalker_boots',
 N'Shadowwalker Boots',
 N'Light boots enchanted to muffle footsteps in darkness.',
 N'Armor - Shoes', N'Common', N'/images/items/shadowwalker_boots.png', 1, 1,
 N'["SPD +20","EVA +5%","Stealth +8"]'),

-- Banner 3: Eternal Bulwark
(N'ac6dd9db-532c-4d33-a392-30d1f10dbdc7', N'item_iron_plated_vest',
 N'Iron-Plated Vest',
 N'A basic reinforced vest issued to frontline soldiers.',
 N'Armor - Body', N'Common', N'/images/items/iron_plated_vest.png', 0, 1,
 N'["DEF +120","HP +300"]');
GO