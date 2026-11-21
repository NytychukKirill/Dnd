namespace DnD.Models;

using Data;

public class Character
{
    public  Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;
    
    public Guid PlayerToken { get; set; } = Guid.NewGuid();
    
    public string NamePlayer { get; set; } = string.Empty;
    public string NameCharacter { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public string ClassCharacter { get; set; } = string.Empty;
    public string RaceCharacter { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    
    public int HpCurrent { get; set; } = 10;
    public int HpMax { get; set; } = 10;
    public int ArmorClass { get; set; } = 10;
    
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;
    
    public void ApplyPresets(string raceId, string classId)
    {
        var race = DndPresets.GetRace(raceId) 
                   ?? throw new ArgumentException($"Невідома раса: {raceId}");
        var charClass = DndPresets.GetClass(classId) 
                        ?? throw new ArgumentException($"Невідомий клас: {classId}");

        RaceCharacter = race.Name;
        ClassCharacter = charClass.Name;

        Strength = charClass.BaseStats.Strength + race.StatBonuses.Strength;
        Dexterity = charClass.BaseStats.Dexterity + race.StatBonuses.Dexterity;
        Constitution = charClass.BaseStats.Constitution + race.StatBonuses.Constitution;
        Intelligence = charClass.BaseStats.Intelligence + race.StatBonuses.Intelligence;
        Wisdom = charClass.BaseStats.Wisdom + race.StatBonuses.Wisdom;
        Charisma = charClass.BaseStats.Charisma + race.StatBonuses.Charisma;

        // На 1 рівні максимальне ХП = максимальне значення кістки хітів + модифікатор Статури
        int conModifier = (int)Math.Floor((Constitution - 10) / 2.0);
        HpMax = Math.Max(1, charClass.HitDie + conModifier);
        HpCurrent = HpMax;

        // Базовий клас обладунку без броні = 10 + модифікатор Спритності
        int dexModifier = (int)Math.Floor((Dexterity - 10) / 2.0);
        ArmorClass = 10 + dexModifier;
    }
}

