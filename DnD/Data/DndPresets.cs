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
        new("athletics",       "Атлетика",             Ability.Strength),
        new("acrobatics",      "Акробатика",           Ability.Dexterity),
        new("sleight-of-hand", "Спритність рук",      Ability.Dexterity),
        new("stealth",         "Прихованість",         Ability.Dexterity),
        new("arcana",          "Таємна магія",         Ability.Intelligence),
        new("history",         "Історія",              Ability.Intelligence),
        new("investigation",   "Розслідування",        Ability.Intelligence),
        new("nature",          "Природа",              Ability.Intelligence),
        new("religion",        "Релігія",              Ability.Intelligence),
        new("animal-handling", "Догляд за тваринами",  Ability.Wisdom),
        new("insight",         "Проникливість",        Ability.Wisdom),
        new("medicine",        "Медицина",             Ability.Wisdom),
        new("perception",      "Сприйняття",           Ability.Wisdom),
        new("survival",        "Виживання",            Ability.Wisdom),
        new("deception",       "Обман",                Ability.Charisma),
        new("intimidation",    "Залякування",          Ability.Charisma),
        new("performance",     "Виступ",               Ability.Charisma),
        new("persuasion",      "Переконання",          Ability.Charisma)
    ];

    private static readonly string[] AllSkillIds = Skills.Select(s => s.Id).ToArray();

    public static readonly IReadOnlyList<RacePreset> Races =
    [
        new("aasimar",    "Аасімар",           new StatBlock(), Speed: 30, Size: "Середній або малий", Darkvision: 60),
        new("gnome",      "Гном",              new StatBlock(), Speed: 30, Size: "Малий",              Darkvision: 60),
        new("goliath",    "Голіаф",            new StatBlock(), Speed: 35, Size: "Середній"),
        new("dwarf",      "Дварф",             new StatBlock(), Speed: 30, Size: "Середній",           Darkvision: 120, HpPerLevelBonus: 1),
        new("dragonborn", "Дракононароджений", new StatBlock(), Speed: 30, Size: "Середній",           Darkvision: 60),
        new("orc",        "Орк",               new StatBlock(), Speed: 30, Size: "Середній",           Darkvision: 120),
        new("halfling",   "Напіврослик",       new StatBlock(), Speed: 30, Size: "Малий"),
        new("tiefling",   "Тифлінг",           new StatBlock(), Speed: 30, Size: "Середній або малий", Darkvision: 60),
        new("human",      "Людина",            new StatBlock(), Speed: 30, Size: "Середній або малий"),
        new("elf",        "Ельф",              new StatBlock(), Speed: 30, Size: "Середній",           Darkvision: 60) // Лісовий ельф (рід): Швидкість 35
    ];

    // ===== Предыстории (16) =====
    public static readonly IReadOnlyList<BackgroundPreset> Backgrounds =
    [
        new("entertainer", "Артист",      [Ability.Strength, Ability.Dexterity, Ability.Charisma],     "musician",               "Музикант",                          ["acrobatics", "performance"],      "Музичний інструмент (на вибір)"),
        new("noble",       "Аристократ",  [Ability.Strength, Ability.Intelligence, Ability.Charisma],  "skilled",                "Обдарований",                       ["history", "persuasion"],          "Ігровий набір (на вибір)"),
        new("wayfarer",    "Бродяга",     [Ability.Dexterity, Ability.Wisdom, Ability.Charisma],       "lucky",                  "Везунчик",                          ["insight", "stealth"],             "Злодійські інструменти"),
        new("sailor",      "Моряк",       [Ability.Strength, Ability.Dexterity, Ability.Wisdom],       "tavern-brawler",         "Забіяка",                           ["acrobatics", "perception"],       "Інструменти навігатора"),
        new("sage",        "Мудрець",     [Ability.Constitution, Ability.Intelligence, Ability.Wisdom],"magic-initiate-wizard",  "Присвячений у магію (Чарівник)",    ["history", "arcana"],              "Інструменти каліграфа"),
        new("hermit",      "Відлюдник",   [Ability.Constitution, Ability.Wisdom, Ability.Charisma],    "healer",                 "Цілитель",                          ["medicine", "religion"],           "Набір травника"),
        new("scribe",      "Писар",       [Ability.Dexterity, Ability.Intelligence, Ability.Wisdom],   "skilled",                "Обдарований",                       ["perception", "investigation"],    "Інструменти каліграфа"),
        new("criminal",    "Злочинець",   [Ability.Dexterity, Ability.Constitution, Ability.Intelligence], "alert",              "Пильний",                           ["sleight-of-hand", "stealth"],     "Злодійські інструменти"),
        new("acolyte",     "Прислужник",  [Ability.Intelligence, Ability.Wisdom, Ability.Charisma],    "magic-initiate-cleric",  "Присвячений у магію (Жрець)",       ["insight", "religion"],            "Інструменти каліграфа"),
        new("guide",       "Провідник",   [Ability.Dexterity, Ability.Constitution, Ability.Wisdom],   "magic-initiate-druid",   "Присвячений у магію (Друїд)",       ["survival", "stealth"],            "Інструменти картографа"),
        new("artisan",     "Ремісник",    [Ability.Strength, Ability.Dexterity, Ability.Intelligence], "crafter",                "Умілець",                           ["investigation", "persuasion"],    "Ремісничі інструменти (на вибір)"),
        new("soldier",     "Солдат",      [Ability.Strength, Ability.Dexterity, Ability.Constitution], "savage-attacker",        "Дикий нападник",                    ["athletics", "intimidation"],      "Ігровий набір (на вибір)"),
        new("guard",       "Вартовий",    [Ability.Strength, Ability.Intelligence, Ability.Wisdom],    "alert",                  "Пильний",                           ["athletics", "perception"],        "Ігровий набір (на вибір)"),
        new("merchant",    "Торговець",   [Ability.Constitution, Ability.Intelligence, Ability.Charisma], "lucky",               "Везунчик",                          ["persuasion", "animal-handling"],  "Інструменти навігатора"),
        new("farmer",      "Фермер",      [Ability.Strength, Ability.Constitution, Ability.Wisdom],    "tough",                  "Міцний",                            ["nature", "animal-handling"],      "Інструменти тесляра"),
        new("charlatan",   "Шарлатан",    [Ability.Dexterity, Ability.Constitution, Ability.Charisma], "skilled",                "Обдарований",                       ["sleight-of-hand", "deception"],   "Набір для підробки")
    ];

    public static readonly IReadOnlyList<ClassPreset> Classes =
    [
        new("fighter", "Воїн", HitDie: 10,
            new StatBlock(Strength: 15, Dexterity: 13, Constitution: 14, Intelligence: 10, Wisdom: 12, Charisma: 8),
            SavingThrows: [Ability.Strength, Ability.Constitution],
            SkillChoices: 2,
            SkillOptions: ["acrobatics", "animal-handling", "athletics", "history", "insight", "intimidation", "persuasion", "perception", "survival"],
            ArmorTraining: ["light", "medium", "heavy", "shield"],
            Weapons: ["simple", "martial"]),

        new("barbarian", "Варвар", HitDie: 12,
            new StatBlock(Strength: 15, Dexterity: 13, Constitution: 14, Intelligence: 8, Wisdom: 12, Charisma: 10),
            SavingThrows: [Ability.Strength, Ability.Constitution],
            SkillChoices: 2,
            SkillOptions: ["animal-handling", "athletics", "intimidation", "nature", "perception", "survival"],
            ArmorTraining: ["light", "medium", "shield"],
            Weapons: ["simple", "martial"],
            Unarmored: UnarmoredDefense.Barbarian),

        new("rogue", "Плут (Розбійник)", HitDie: 8,
            new StatBlock(Strength: 8, Dexterity: 15, Constitution: 13, Intelligence: 14, Wisdom: 12, Charisma: 10),
            SavingThrows: [Ability.Dexterity, Ability.Intelligence],
            SkillChoices: 4,
            SkillOptions: ["acrobatics", "athletics", "deception", "insight", "intimidation", "investigation", "perception", "persuasion", "sleight-of-hand", "stealth"],
            ArmorTraining: ["light"],
            Weapons: ["simple", "martial-finesse-light"]),

        new("wizard", "Чарівник (Маг)", HitDie: 6,
            new StatBlock(Strength: 8, Dexterity: 14, Constitution: 13, Intelligence: 15, Wisdom: 12, Charisma: 10),
            SavingThrows: [Ability.Intelligence, Ability.Wisdom],
            SkillChoices: 2,
            SkillOptions: ["arcana", "history", "insight", "investigation", "medicine", "nature", "religion"],
            ArmorTraining: [],
            Weapons: ["simple"],
            SpellcastingAbility: Ability.Intelligence),

        new("cleric", "Жрець", HitDie: 8,
            new StatBlock(Strength: 13, Dexterity: 10, Constitution: 14, Intelligence: 8, Wisdom: 15, Charisma: 12),
            SavingThrows: [Ability.Wisdom, Ability.Charisma],
            SkillChoices: 2,
            SkillOptions: ["history", "insight", "medicine", "persuasion", "religion"],
            ArmorTraining: ["light", "medium", "shield"],
            Weapons: ["simple"],
            SpellcastingAbility: Ability.Wisdom),

        new("paladin", "Паладин", HitDie: 10,
            new StatBlock(Strength: 15, Dexterity: 10, Constitution: 13, Intelligence: 8, Wisdom: 12, Charisma: 14),
            SavingThrows: [Ability.Wisdom, Ability.Charisma],
            SkillChoices: 2,
            SkillOptions: ["athletics", "insight", "intimidation", "medicine", "persuasion", "religion"],
            ArmorTraining: ["light", "medium", "heavy", "shield"],
            Weapons: ["simple", "martial"],
            SpellcastingAbility: Ability.Charisma),

        new("bard", "Бард", HitDie: 8,
            new StatBlock(Strength: 8, Dexterity: 14, Constitution: 13, Intelligence: 12, Wisdom: 10, Charisma: 15),
            SavingThrows: [Ability.Dexterity, Ability.Charisma],
            SkillChoices: 3,
            SkillOptions: AllSkillIds, 
            ArmorTraining: ["light"],
            Weapons: ["simple"],
            SpellcastingAbility: Ability.Charisma),

        new("druid", "Друїд", HitDie: 8,
            new StatBlock(Strength: 8, Dexterity: 13, Constitution: 14, Intelligence: 12, Wisdom: 15, Charisma: 10),
            SavingThrows: [Ability.Intelligence, Ability.Wisdom],
            SkillChoices: 2,
            SkillOptions: ["arcana", "animal-handling", "insight", "medicine", "nature", "perception", "religion", "survival"],
            ArmorTraining: ["light", "shield"],
            Weapons: ["simple"],
            SpellcastingAbility: Ability.Wisdom),

        new("monk", "Монах", HitDie: 8,
            new StatBlock(Strength: 12, Dexterity: 15, Constitution: 13, Intelligence: 10, Wisdom: 14, Charisma: 8),
            SavingThrows: [Ability.Strength, Ability.Dexterity],
            SkillChoices: 2,
            SkillOptions: ["acrobatics", "athletics", "history", "insight", "religion", "stealth"],
            ArmorTraining: [],
            Weapons: ["simple", "martial-light"],
            Unarmored: UnarmoredDefense.Monk),

        new("ranger", "Слідопит (Рейнджер)", HitDie: 10,
            new StatBlock(Strength: 12, Dexterity: 15, Constitution: 13, Intelligence: 10, Wisdom: 14, Charisma: 8),
            SavingThrows: [Ability.Strength, Ability.Dexterity],
            SkillChoices: 3,
            SkillOptions: ["animal-handling", "athletics", "insight", "investigation", "nature", "perception", "stealth", "survival"],
            ArmorTraining: ["light", "medium", "shield"],
            Weapons: ["simple", "martial"],
            SpellcastingAbility: Ability.Wisdom),

        new("sorcerer", "Чародій", HitDie: 6,
            new StatBlock(Strength: 8, Dexterity: 13, Constitution: 14, Intelligence: 12, Wisdom: 10, Charisma: 15),
            SavingThrows: [Ability.Constitution, Ability.Charisma],
            SkillChoices: 2,
            SkillOptions: ["arcana", "deception", "insight", "intimidation", "persuasion", "religion"],
            ArmorTraining: [],
            Weapons: ["simple"],
            SpellcastingAbility: Ability.Charisma),

        new("warlock", "Чорнокнижник", HitDie: 8,
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
        new("padded",      "Стьобаний",          ArmorCategory.Light,  11),
        new("leather",     "Шкіряний",           ArmorCategory.Light,  11),
        new("studded",     "Клепаний шкіряний",  ArmorCategory.Light,  12),
        new("hide",        "Шкурний",            ArmorCategory.Medium, 12),
        new("chain-shirt", "Кольчужна сорочка",  ArmorCategory.Medium, 13),
        new("scale-mail",  "Лускатий",           ArmorCategory.Medium, 14),
        new("breastplate", "Кіраса",             ArmorCategory.Medium, 14),
        new("half-plate",  "Напівлати",          ArmorCategory.Medium, 15),
        new("ring-mail",   "Кільчастий",         ArmorCategory.Heavy,  14),
        new("chain-mail",  "Кольчуга",           ArmorCategory.Heavy,  16, StrengthRequired: 13),
        new("splint",      "Наборний",           ArmorCategory.Heavy,  17, StrengthRequired: 15),
        new("plate",       "Лати",               ArmorCategory.Heavy,  18, StrengthRequired: 15)
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
