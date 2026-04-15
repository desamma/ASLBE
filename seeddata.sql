-- 1. PREMIUM CURRENCY PACK (buy with real money)
INSERT INTO ShopItems (
    Id,
	Name, 
	Description, 
	Category, 
	ItemType, 
	ImagePath, --img path
    Price,
	CurrencyAmount, 
	IsUsingPremiumCurrency,
    ItemQuantity,
	ItemId,
	IsActive,
	IsFeatured,
	CreatedDate
)
VALUES (
    NEWID(),
    'Small Gem Pack',
    'Get 100 premium gems',
    'Currency',
    'PremiumCurrency',
    '/images/shop/gem_small.png',
    0.99,              -- real money
    100,               -- amount received
    0,                 -- NOT using premium currency
    NULL,
    NULL,
    1,
    1,
    GETDATE()
);
-- 2. BIGGER CURRENCY PACK
INSERT INTO ShopItems (
    Id, Name, Description, Category, ItemType, ImagePath,
    Price, CurrencyAmount, IsUsingPremiumCurrency,
    ItemQuantity, ItemId, IsActive, IsFeatured, CreatedDate
)
VALUES (
    NEWID(),
    'Mega Gem Pack',
    'Get 1000 premium gems (Best Value)',
    'Currency',
    'PremiumCurrency',
    '/images/shop/gem_mega.png',
    7.99,
    1000,
    0,
    NULL,
    NULL,
    1,
    1,
    GETDATE()
);
-- 3. EQUIPMENT ITEM (buy using premium currency)
INSERT INTO ShopItems (
    Id, Name, Description, Category, ItemType, ImagePath,
    Price, CurrencyAmount, IsUsingPremiumCurrency,
    ItemQuantity, ItemId, IsActive, IsFeatured, CreatedDate
)
VALUES (
    NEWID(),
    'Health Potion',
    'Restores 150 HP instantly.',
    'Item',
    'Consumable',
    '/images/items/health_potion.png',
    10,              -- cost in premium currency
    NULL,
    1,                -- uses premium currency
    1,
    'c958b823-16b8-4736-9144-114cd6d8d508', -- existing ItemId
    1,
    0,
    GETDATE()
);
-- 5. STARTER BUNDLE (multiple items)
INSERT INTO ShopItems (
    Id, Name, Description, Category, ItemType, ImagePath,
    Price, CurrencyAmount, IsUsingPremiumCurrency,
    ItemQuantity, ItemId, IsActive, IsFeatured, CreatedDate
)
VALUES (
    NEWID(),
    'Starter Pack',
    'Includes sword + potions',
    'Bundle',
    'Bundle',
    '/images/shop/starter_pack.png',
    4.99,           -- real money bundle
    NULL,
    0,
    1,
    'd0bfbae9-e7fd-4e3d-8c46-77786132349a',
    1,
    1,
    GETDATE()
);

-- 1. COMMON CONSUMABLE
INSERT INTO Items (Id, Name, Description, Type, Rarity, ImagePath)
VALUES (
    NEWID(),
    'Health Potion',
    'Restores 50 HP instantly.',
    'Consumable',
    'Common',
    '/images/items/health_potion.png'
);
-- 2. RARE CONSUMABLE
INSERT INTO Items (Id, Name, Description, Type, Rarity, ImagePath)
VALUES (
    NEWID(),
    'Mana Potion',
    'Restores 40 MP instantly.',
    'Consumable',
    'Rare',
    '/images/items/mana_potion.png'
);
-- 3. COMMON WEAPON
INSERT INTO Items (Id, Name, Description, Type, Rarity, ImagePath)
VALUES (
    NEWID(),
    'Iron Sword',
    'A basic sword made of iron.',
    'Weapon',
    'Common',
    '/images/items/iron_sword.png'
);
-- 4. RARE WEAPON
INSERT INTO Items (Id, Name, Description, Type, Rarity, ImagePath)
VALUES (
    NEWID(),
    'Flame Sword',
    'A sword imbued with fire, dealing burn damage.',
    'Weapon',
    'Rare',
    '/images/items/flame_sword.png'
);
-- 5. EPIC WEAPON
INSERT INTO Items (Id, Name, Description, Type, Rarity, ImagePath)
VALUES (
    NEWID(),
    'Thunder Blade',
    'Calls lightning on hit. High burst damage.',
    'Weapon',
    'Epic',
    '/images/items/thunder_blade.png'
);
-- 6. LEGENDARY WEAPON
INSERT INTO Items (Id, Name, Description, Type, Rarity, ImagePath)
VALUES (
    NEWID(),
    'Ashen Greatsword',
    'A legendary blade forged from fallen ashes.',
    'Weapon',
    'Legendary',
    '/images/items/ashen_greatsword.png'
);
-- 7. ARMOR
INSERT INTO Items (Id, Name, Description, Type, Rarity, ImagePath)
VALUES (
    NEWID(),
    'Knight Armor',
    'Heavy armor that increases defense significantly.',
    'Armor',
    'Rare',
    '/images/items/knight_armor.png'
);
-- 8. ACCESSORY
INSERT INTO Items (Id, Name, Description, Type, Rarity, ImagePath)
VALUES (
    NEWID(),
    'Ring of Speed',
    'Increases attack speed slightly.',
    'Accessory',
    'Epic',
    '/images/items/ring_speed.png'
);

INSERT INTO NPCs (
    Id,
    Name,
    Description,
    ImagePath,
    Location,
    NPCType,
    CreatedDate,
    UpdatedDate
)
VALUES (
    NEWID(),
    N'Sly',
    N'Sly used to be the Great Nailsage. During his previous occupation, he trained some disciples in the Nail Arts: Oro, Mato, and Sheo. Eventually, Sly abandoned his occupation as Nailsage. He became convinced of the strength of accumulating Geo over honing one''s nail. He thus turned to the simple life of a merchant, finding and selling wares from his shop in Dirtmouth. He did not, however, forget his mastery and respect of the nail and those who practice its arts. He keeps his greatnail in the basement of his shop and still remembers his pupils.
	At some point, a dream led him to wander off into the caverns below Dirtmouth. He also lost his Shopkeeper''s Key which ended up in a room in Crystal Peak.',
    '/images/npcs/sly.png',
    N'Bottom of the Forgotten Crossroads',
    'Merchant',
    GETDATE(),
    NULL
);

INSERT INTO NPCs (
    Id,
    Name,
    Description,
    ImagePath,
    Location,
    NPCType,
    CreatedDate,
    UpdatedDate
)
VALUES (
    NEWID(),
    N'Steel Soul Jinn',
    N'Jinn resides in the locked cavern by the base of Crystal Peak in Dirtmouth, where she has been sleeping. She has unseen masters who do not seek order. Jinn speaks in rhythm and claims to be too young to tell anything about herself beside her purpose of providing and trading. Her metallic body cannot take damage, although she still reacts to strikes at her.',
    '/images/npcs/steelsouljinn.png',
    N'Dirtmouth',
    'DialogOnly',
    GETDATE(),
    GETDATE()
);