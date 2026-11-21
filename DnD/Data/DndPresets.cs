namespace DnD.Data;

public class DndPresets
{
    public static readonly IReadOnlyList<RacePreset> Races =
    [
        new("human", "Людина", new StatBlock(1, 1, 1, 1, 1, 1), Speed: 30),
        new("elf", "Ельф", new StatBlock(Dexterity: 2, Intelligence: 1), Speed: 30),
        new("dwarf", "Дварф", new StatBlock(Strength: 2, Constitution: 2), Speed: 25),
        new("halfling", "Напіврослик", new StatBlock(Dexterity: 2, Charisma: 1), Speed: 25),
        new("tiefling", "Тифлінг", new StatBlock(Intelligence: 1, Charisma: 2), Speed: 30),
        new("dragonborn", "Дракононароджений", new StatBlock(Strength: 2, Charisma: 1), Speed: 30),
        new("half-orc", "Напіворк", new StatBlock(Strength: 2, Constitution: 1), Speed: 30)
    ];

    public static readonly IReadOnlyList<ClassPreset> Classes =
    [
        new("fighter", "Воїн", HitDie: 10, new StatBlock(Strength: 15, Dexterity: 13, Constitution: 14, Intelligence: 10, Wisdom: 12, Charisma: 8)),
        new("barbarian", "Варвар", HitDie: 12, new StatBlock(Strength: 15, Dexterity: 13, Constitution: 14, Intelligence: 8, Wisdom: 12, Charisma: 10)),
        new("rogue", "Плут (Розбійник)", HitDie: 8, new StatBlock(Strength: 8, Dexterity: 15, Constitution: 13, Intelligence: 14, Wisdom: 12, Charisma: 10)),
        new("wizard", "Чарівник (Маг)", HitDie: 6, new StatBlock(Strength: 8, Dexterity: 14, Constitution: 13, Intelligence: 15, Wisdom: 12, Charisma: 10)),
        new("cleric", "Жрець", HitDie: 8, new StatBlock(Strength: 13, Dexterity: 10, Constitution: 14, Intelligence: 8, Wisdom: 15, Charisma: 12)),
        new("paladin", "Паладин", HitDie: 10, new StatBlock(Strength: 15, Dexterity: 10, Constitution: 13, Intelligence: 8, Wisdom: 12, Charisma: 14)),
        new("bard", "Бард", HitDie: 8, new StatBlock(Strength: 8, Dexterity: 14, Constitution: 13, Intelligence: 12, Wisdom: 10, Charisma: 15))
    ];

    public static RacePreset? GetRace(string id) => 
        Races.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public static ClassPreset? GetClass(string id) => 
        Classes.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}

public record StatBlock(
    int Strength = 0,
    int Dexterity = 0,
    int Constitution = 0,
    int Intelligence = 0,
    int Wisdom = 0,
    int Charisma = 0
);

public record RacePreset(
    string Id,
    string Name,
    StatBlock StatBonuses,
    int Speed = 30
);

public record ClassPreset(
    string Id,
    string Name,
    int HitDie,             // Наприклад: 12 для Варвара, 10 для Воїна, 8 для Плута, 6 для Мага
    StatBlock BaseStats     // Рекомендований розподіл стандартного набору [15, 14, 13, 12, 10, 8]
);