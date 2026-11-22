using DnD.Data;
using DnD.Models;

namespace DnD.DTOs;

// ===== Запити =====

public record CreateRoomRequest(string Name);

public record CreateCharacterRequest(
    string NamePlayer,
    string NameCharacter,
    string? Description,
    string? Alignment,
    string RaceId,
    string ClassId,
    string? BackgroundId,
    Ability? PlusTwo,
    Ability? PlusOne,
    List<string>? ClassSkills
);

public record UpdateCharacterStatsRequest(
    int? HpCurrent,
    int? TempHp,
    int? HitDiceSpent,
    int? DeathSaveSuccesses,
    int? DeathSaveFailures,
    bool? HeroicInspiration,
    string? ArmorId,        // "" = зняти обладунок
    bool? HasShield,
    int? XpToAdd,           // лише Майстер
    string? Description
);

// ===== Відповіді =====

public record RoomCreatedResponse(Guid Id, string Code, string Name, Guid DmToken);

public record RoomDetailsResponse(
    Guid Id,
    string Code,
    string Name,
    bool IsDm,
    List<CharacterResponse> Characters
);

public record CharacterCreatedResponse(CharacterResponse Character, Guid PlayerToken);

public record CharacterResponse(
    Guid Id,
    string NamePlayer,
    string NameCharacter,
    string Description,
    string Alignment,
    string ClassCharacter,
    string RaceCharacter,
    string BackgroundCharacter,
    int Level,
    int Xp,
    int XpForNextLevel,
    int ProficiencyBonus,
    // Характеристики та модифікатори
    int Strength, int StrengthMod,
    int Dexterity, int DexterityMod,
    int Constitution, int ConstitutionMod,
    int Intelligence, int IntelligenceMod,
    int Wisdom, int WisdomMod,
    int Charisma, int CharismaMod,
    // Бойові та життєві показники
    int HpCurrent,
    int HpMax,
    int TempHp,
    int HitDiceLeft,
    int HitDiceTotal,
    int HitDieType,
    int DeathSaveSuccesses,
    int DeathSaveFailures,
    bool HeroicInspiration,
    int ArmorClass,
    string? ArmorId,
    bool HasShield,
    int EffectiveSpeed,
    string Size,
    int Darkvision,
    int PassivePerception,
    int InitiativeBonus,
    int? SpellSaveDc,
    int? SpellAttackBonus,
    // Порахований бонус кожного спасброска та навички (ключі: "Strength"…, "athletics"…)
    Dictionary<string, int> SavingThrows,
    Dictionary<string, int> Skills,
    List<string> ArmorTraining,
    List<string> WeaponProficiencies,
    List<string> ToolProficiencies,
    List<string> Feats,
    // --- Додано: id пресетів і прапорці володіння (для позначок на бланку) ---
    string ClassId,
    string RaceId,
    string BackgroundId,
    int Speed,
    int InitiativeScore,
    int CarryCapacityLb,
    List<string> SaveProficiencies,
    List<string> SkillProficiencies,
    List<string> SkillExpertise
)
{
    public static CharacterResponse FromEntity(Character c) => new(
        c.Id,
        c.NamePlayer,
        c.NameCharacter,
        c.Description,
        c.Alignment,
        c.ClassCharacter,
        c.RaceCharacter,
        c.BackgroundCharacter,
        c.Level,
        c.Xp,
        c.XpForNextLevel,
        c.ProficiencyBonus,
        c.Strength, c.StrengthMod,
        c.Dexterity, c.DexterityMod,
        c.Constitution, c.ConstitutionMod,
        c.Intelligence, c.IntelligenceMod,
        c.Wisdom, c.WisdomMod,
        c.Charisma, c.CharismaMod,
        c.HpCurrent,
        c.HpMax,
        c.TempHp,
        c.HitDiceLeft,
        c.HitDiceTotal,
        c.HitDieType,
        c.DeathSaveSuccesses,
        c.DeathSaveFailures,
        c.HeroicInspiration,
        c.ArmorClass,
        c.ArmorId,
        c.HasShield,
        c.EffectiveSpeed,
        c.Size,
        c.Darkvision,
        c.PassivePerception,
        c.InitiativeBonus,
        c.SpellSaveDc,
        c.SpellAttackBonus,
        Enum.GetValues<Ability>().ToDictionary(a => a.ToString(), a => c.GetSaveBonus(a)),
        DndPresets.Skills.ToDictionary(s => s.Id, s => c.GetSkillBonus(s.Id)),
        [.. c.ArmorTraining],
        [.. c.WeaponProficiencies],
        [.. c.ToolProficiencies],
        [.. c.Feats],
        c.ClassId,
        c.RaceId,
        c.BackgroundId,
        c.Speed,
        c.InitiativeScore,
        c.CarryCapacityLb,
        [.. c.SaveProficiencies],
        [.. c.SkillProficiencies],
        [.. c.SkillExpertise]
    );
}
    