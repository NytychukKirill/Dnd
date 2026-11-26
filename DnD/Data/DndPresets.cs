namespace DnD.Data;

public enum Ability { Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma }

public enum ArmorCategory { Light, Medium, Heavy }

public enum UnarmoredDefense
{
    Standard, 
    Barbarian, 
    Monk       
}

public record StatBlock(
    int Strength = 0,
    int Dexterity = 0,
    int Constitution = 0,
    int Intelligence = 0,
    int Wisdom = 0,
    int Charisma = 0
)
{
    public int Get(Ability a) => a switch
    {
        Ability.Strength => Strength,
        Ability.Dexterity => Dexterity,
        Ability.Constitution => Constitution,
        Ability.Intelligence => Intelligence,
        Ability.Wisdom => Wisdom,
        Ability.Charisma => Charisma,
        _ => throw new ArgumentOutOfRangeException(nameof(a))
    };
}

public record RacePreset(
    string Id,
    string Name,
    StatBlock StatBonuses,
    int Speed = 30,                 
    string Size = "Середній",
    int Darkvision = 0,             
    int HpPerLevelBonus = 0         
);

public record SkillPreset(string Id, string Name, Ability Ability);

public record BackgroundPreset(
    string Id,
    string Name,
    Ability[] AbilityOptions,
    string FeatId,
    string FeatName,
    string[] Skills,
    string Tool
);

public record ClassPreset(
    string Id,
    string Name,
    int HitDie,             
    StatBlock BaseStats,    
    Ability[] SavingThrows, 
    int SkillChoices,      
    string[] SkillOptions, 
    string[] ArmorTraining,
    string[] Weapons,       
    Ability? SpellcastingAbility = null,
    UnarmoredDefense Unarmored = UnarmoredDefense.Standard
);

public record ArmorPreset(
    string Id,
    string Name,
    ArmorCategory Category,
    int BaseAc,
    int StrengthRequired = 0 
);

public class DndPresets
{
    public static readonly IReadOnlyList<SkillPreset> Skills =
    [
        new("athletics",       "Athletics",       Ability.Strength),
        new("acrobatics",      "Acrobatics",      Ability.Dexterity),
        new("sleight-of-hand", "Sleight of hand", Ability.Dexterity),
        new("stealth",         "Stealth",         Ability.Dexterity),
        new("arcana",          "Arcana",          Ability.Intelligence),
        new("history",         "History",         Ability.Intelligence),
        new("investigation",   "Investigation",   Ability.Intelligence),
        new("nature",          "Nature",          Ability.Intelligence),
        new("religion",        "Religion",        Ability.Intelligence),
        new("animal-handling", "Animal Handling", Ability.Wisdom),
        new("insight",         "Insight",         Ability.Wisdom),
        new("medicine",        "Medicine",        Ability.Wisdom),
        new("perception",      "Perception",      Ability.Wisdom),
        new("survival",        "Survival",        Ability.Wisdom),
        new("deception",       "Deception",       Ability.Charisma),
        new("intimidation",    "Intimidation",    Ability.Charisma),
        new("performance",     "Performance",     Ability.Charisma),
        new("persuasion",      "Persuasion",      Ability.Charisma)
    ];

    private static readonly string[] AllSkillIds = Skills.Select(s => s.Id).ToArray();

    public static readonly IReadOnlyList<RacePreset> Races =
    [
        new("aasimar",    "Aasimar",    new StatBlock(), Speed: 30, Size: "Medium or Small", Darkvision: 60),
        new("gnome",      "Gnome",      new StatBlock(), Speed: 30, Size: "Small",           Darkvision: 60),
        new("goliath",    "Goliath",    new StatBlock(), Speed: 35, Size: "Medium"),
        new("dwarf",      "Dwarf",      new StatBlock(), Speed: 30, Size: "Medium",          Darkvision: 120, HpPerLevelBonus: 1),
        new("dragonborn", "Dragonborn", new StatBlock(), Speed: 30, Size: "Medium",          Darkvision: 60),
        new("orc",        "Orc",        new StatBlock(), Speed: 30, Size: "Medium",          Darkvision: 120),
        new("halfling",   "Halfling",   new StatBlock(), Speed: 30, Size: "Small"),
        new("tiefling",   "Tiefling",   new StatBlock(), Speed: 30, Size: "Medium or Small", Darkvision: 60),
        new("human",      "Human",      new StatBlock(), Speed: 30, Size: "Medium or Small"),
        new("elf",        "Elf",        new StatBlock(), Speed: 30, Size: "Medium",          Darkvision: 60) // Wood Elf (lineage): Speed 35
    ];

    public static readonly IReadOnlyList<BackgroundPreset> Backgrounds =
    [
        new("entertainer", "Entertainer", [Ability.Strength, Ability.Dexterity, Ability.Charisma],       "musician",              "Musician",                ["acrobatics", "performance"],     "Musical Instrument (one of your choice)"),
        new("noble",       "Noble",       [Ability.Strength, Ability.Intelligence, Ability.Charisma],    "skilled",               "Skilled",                 ["history", "persuasion"],         "Gaming Set (one of your choice)"),
        new("wayfarer",    "Wayfarer",    [Ability.Dexterity, Ability.Wisdom, Ability.Charisma],         "lucky",                 "Lucky",                   ["insight", "stealth"],            "Thieves' Tools"),
        new("sailor",      "Sailor",      [Ability.Strength, Ability.Dexterity, Ability.Wisdom],         "tavern-brawler",        "Tavern Brawler",          ["acrobatics", "perception"],      "Navigator's Tools"),
        new("sage",        "Sage",        [Ability.Constitution, Ability.Intelligence, Ability.Wisdom],  "magic-initiate-wizard", "Magic Initiate (Wizard)", ["history", "arcana"],             "Calligrapher's Supplies"),
        new("hermit",      "Hermit",      [Ability.Constitution, Ability.Wisdom, Ability.Charisma],      "healer",                "Healer",                  ["medicine", "religion"],          "Herbalism Kit"),
        new("scribe",      "Scribe",      [Ability.Dexterity, Ability.Intelligence, Ability.Wisdom],     "skilled",               "Skilled",                 ["perception", "investigation"],   "Calligrapher's Supplies"),
        new("criminal",    "Criminal",    [Ability.Dexterity, Ability.Constitution, Ability.Intelligence],"alert",                "Alert",                   ["sleight-of-hand", "stealth"],    "Thieves' Tools"),
        new("acolyte",     "Acolyte",     [Ability.Intelligence, Ability.Wisdom, Ability.Charisma],      "magic-initiate-cleric", "Magic Initiate (Cleric)", ["insight", "religion"],           "Calligrapher's Supplies"),
        new("guide",       "Guide",       [Ability.Dexterity, Ability.Constitution, Ability.Wisdom],     "magic-initiate-druid",  "Magic Initiate (Druid)",  ["survival", "stealth"],           "Cartographer's Tools"),
        new("artisan",     "Artisan",     [Ability.Strength, Ability.Dexterity, Ability.Intelligence],   "crafter",               "Crafter",                 ["investigation", "persuasion"],   "Artisan's Tools (one of your choice)"),
        new("soldier",     "Soldier",     [Ability.Strength, Ability.Dexterity, Ability.Constitution],   "savage-attacker",       "Savage Attacker",         ["athletics", "intimidation"],     "Gaming Set (one of your choice)"),
        new("guard",       "Guard",       [Ability.Strength, Ability.Intelligence, Ability.Wisdom],      "alert",                 "Alert",                   ["athletics", "perception"],       "Gaming Set (one of your choice)"),
        new("merchant",    "Merchant",    [Ability.Constitution, Ability.Intelligence, Ability.Charisma],  "lucky",                 "Lucky",                   ["persuasion", "animal-handling"], "Navigator's Tools"),
        new("farmer",      "Farmer",      [Ability.Strength, Ability.Constitution, Ability.Wisdom],      "tough",                 "Tough",                   ["nature", "animal-handling"],     "Carpenter's Tools"),
        new("charlatan",   "Charlatan",   [Ability.Dexterity, Ability.Constitution, Ability.Charisma],   "skilled",               "Skilled",                 ["sleight-of-hand", "deception"],  "Forgery Kit")
    ];

    public static readonly IReadOnlyList<ClassPreset> Classes =
    [
        new("fighter", "Fighter", HitDie: 10,
            new StatBlock(Strength: 15, Dexterity: 13, Constitution: 14, Intelligence: 10, Wisdom: 12, Charisma: 8),
            SavingThrows: [Ability.Strength, Ability.Constitution],
            SkillChoices: 2,
            SkillOptions: ["acrobatics", "animal-handling", "athletics", "history", "insight", "intimidation", "persuasion", "perception", "survival"],
            ArmorTraining: ["light", "medium", "heavy", "shield"],
            Weapons: ["simple", "martial"]),

        new("barbarian", "Barbarian", HitDie: 12,
            new StatBlock(Strength: 15, Dexterity: 13, Constitution: 14, Intelligence: 8, Wisdom: 12, Charisma: 10),
            SavingThrows: [Ability.Strength, Ability.Constitution],
            SkillChoices: 2,
            SkillOptions: ["animal-handling", "athletics", "intimidation", "nature", "perception", "survival"],
            ArmorTraining: ["light", "medium", "shield"],
            Weapons: ["simple", "martial"],
            Unarmored: UnarmoredDefense.Barbarian),

        new("rogue", "Rogue", HitDie: 8,
            new StatBlock(Strength: 8, Dexterity: 15, Constitution: 13, Intelligence: 14, Wisdom: 12, Charisma: 10),
            SavingThrows: [Ability.Dexterity, Ability.Intelligence],
            SkillChoices: 4,
            SkillOptions: ["acrobatics", "athletics", "deception", "insight", "intimidation", "investigation", "perception", "persuasion", "sleight-of-hand", "stealth"],
            ArmorTraining: ["light"],
            Weapons: ["simple", "martial-finesse-light"]),

        new("wizard", "Wizard", HitDie: 6,
            new StatBlock(Strength: 8, Dexterity: 14, Constitution: 13, Intelligence: 15, Wisdom: 12, Charisma: 10),
            SavingThrows: [Ability.Intelligence, Ability.Wisdom],
            SkillChoices: 2,
            SkillOptions: ["arcana", "history", "insight", "investigation", "medicine", "nature", "religion"],
            ArmorTraining: [],
            Weapons: ["simple"],
            SpellcastingAbility: Ability.Intelligence),

        new("cleric", "Cleric", HitDie: 8,
            new StatBlock(Strength: 13, Dexterity: 10, Constitution: 14, Intelligence: 8, Wisdom: 15, Charisma: 12),
            SavingThrows: [Ability.Wisdom, Ability.Charisma],
            SkillChoices: 2,
            SkillOptions: ["history", "insight", "medicine", "persuasion", "religion"],
            ArmorTraining: ["light", "medium", "shield"],
            Weapons: ["simple"],
            SpellcastingAbility: Ability.Wisdom),

        new("paladin", "Paladin", HitDie: 10,
            new StatBlock(Strength: 15, Dexterity: 10, Constitution: 13, Intelligence: 8, Wisdom: 12, Charisma: 14),
            SavingThrows: [Ability.Wisdom, Ability.Charisma],
            SkillChoices: 2,
            SkillOptions: ["athletics", "insight", "intimidation", "medicine", "persuasion", "religion"],
            ArmorTraining: ["light", "medium", "heavy", "shield"],
            Weapons: ["simple", "martial"],
            SpellcastingAbility: Ability.Charisma),

        new("bard", "Bard", HitDie: 8,
            new StatBlock(Strength: 8, Dexterity: 14, Constitution: 13, Intelligence: 12, Wisdom: 10, Charisma: 15),
            SavingThrows: [Ability.Dexterity, Ability.Charisma],
            SkillChoices: 3,
            SkillOptions: AllSkillIds, 
            ArmorTraining: ["light"],
            Weapons: ["simple"],
            SpellcastingAbility: Ability.Charisma),

        new("druid", "Druid", HitDie: 8,
            new StatBlock(Strength: 8, Dexterity: 13, Constitution: 14, Intelligence: 12, Wisdom: 15, Charisma: 10),
            SavingThrows: [Ability.Intelligence, Ability.Wisdom],
            SkillChoices: 2,
            SkillOptions: ["arcana", "animal-handling", "insight", "medicine", "nature", "perception", "religion", "survival"],
            ArmorTraining: ["light", "shield"],
            Weapons: ["simple"],
            SpellcastingAbility: Ability.Wisdom),

        new("monk", "Monk", HitDie: 8,
            new StatBlock(Strength: 12, Dexterity: 15, Constitution: 13, Intelligence: 10, Wisdom: 14, Charisma: 8),
            SavingThrows: [Ability.Strength, Ability.Dexterity],
            SkillChoices: 2,
            SkillOptions: ["acrobatics", "athletics", "history", "insight", "religion", "stealth"],
            ArmorTraining: [],
            Weapons: ["simple", "martial-light"],
            Unarmored: UnarmoredDefense.Monk),

        new("ranger", "Ranger", HitDie: 10,
            new StatBlock(Strength: 12, Dexterity: 15, Constitution: 13, Intelligence: 10, Wisdom: 14, Charisma: 8),
            SavingThrows: [Ability.Strength, Ability.Dexterity],
            SkillChoices: 3,
            SkillOptions: ["animal-handling", "athletics", "insight", "investigation", "nature", "perception", "stealth", "survival"],
            ArmorTraining: ["light", "medium", "shield"],
            Weapons: ["simple", "martial"],
            SpellcastingAbility: Ability.Wisdom),

        new("sorcerer", "Sorcerer", HitDie: 6,
            new StatBlock(Strength: 8, Dexterity: 13, Constitution: 14, Intelligence: 12, Wisdom: 10, Charisma: 15),
            SavingThrows: [Ability.Constitution, Ability.Charisma],
            SkillChoices: 2,
            SkillOptions: ["arcana", "deception", "insight", "intimidation", "persuasion", "religion"],
            ArmorTraining: [],
            Weapons: ["simple"],
            SpellcastingAbility: Ability.Charisma),

        new("warlock", "Warlock", HitDie: 8,
            new StatBlock(Strength: 8, Dexterity: 13, Constitution: 14, Intelligence: 10, Wisdom: 12, Charisma: 15),
            SavingThrows: [Ability.Wisdom, Ability.Charisma],
            SkillChoices: 2,
            SkillOptions: ["arcana", "deception", "history", "intimidation", "investigation", "nature", "religion"],
            ArmorTraining: ["light"],
            Weapons: ["simple"],
            SpellcastingAbility: Ability.Charisma)
    ];

    public static readonly IReadOnlyList<ArmorPreset> Armors =
    [
        new("padded",      "Padded",          ArmorCategory.Light,  11),
        new("leather",     "Leather",         ArmorCategory.Light,  11),
        new("studded",     "Studded Leather", ArmorCategory.Light,  12),
        new("hide",        "Hide",            ArmorCategory.Medium, 12),
        new("chain-shirt", "Chain Shirt",     ArmorCategory.Medium, 13),
        new("scale-mail",  "Scale Mail",      ArmorCategory.Medium, 14),
        new("breastplate", "Breastplate",     ArmorCategory.Medium, 14),
        new("half-plate",  "Half Plate",      ArmorCategory.Medium, 15),
        new("ring-mail",   "Ring Mail",       ArmorCategory.Heavy,  14),
        new("chain-mail",  "Chain Mail",      ArmorCategory.Heavy,  16, StrengthRequired: 13),
        new("splint",      "Splint",          ArmorCategory.Heavy,  17, StrengthRequired: 15),
        new("plate",       "Plate",           ArmorCategory.Heavy,  18, StrengthRequired: 15)
    ];

    public const int ShieldAcBonus = 2;

    public static readonly int[] XpThresholds =
    [
        0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000,
        85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000
    ];
    
    public static int Modifier(int score) => (int)Math.Floor((score - 10) / 2.0);

    public static int ProficiencyBonus(int level) => 2 + (Math.Clamp(level, 1, 20) - 1) / 4;

    public static int LevelForXp(int xp)
    {
        var level = 1;
        for (var i = 0; i < XpThresholds.Length; i++)
            if (xp >= XpThresholds[i]) level = i + 1;
        return level;
    }
    
    public static RacePreset? GetRace(string id) =>
        Races.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public static ClassPreset? GetClass(string id) =>
        Classes.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public static BackgroundPreset? GetBackground(string id) =>
        Backgrounds.FirstOrDefault(b => b.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public static SkillPreset? GetSkill(string id) =>
        Skills.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public static ArmorPreset? GetArmor(string id) =>
        Armors.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
