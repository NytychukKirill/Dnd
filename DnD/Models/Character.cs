namespace DnD.Models;

using DnD.Data;

public class Character
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public Guid PlayerToken { get; set; } = Guid.NewGuid();

    public string NamePlayer { get; set; } = string.Empty;
    public string NameCharacter { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Alignment { get; set; } = string.Empty;

    // ===== Идентичность =====
    public string ClassCharacter { get; set; } = string.Empty;       // отображаемое имя
    public string RaceCharacter { get; set; } = string.Empty;
    public string BackgroundCharacter { get; set; } = string.Empty;

    public string ClassId { get; set; } = string.Empty;              // id пресета, нужен для расчётов
    public string RaceId { get; set; } = string.Empty;
    public string BackgroundId { get; set; } = string.Empty;

    public int Level { get; set; } = 1;
    public int Xp { get; set; } = 0;

    // ===== Значения характеристик (итоговые: набор + бонусы предыстории, максимум 20) =====
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;

    // ===== Хиты и состояние =====
    public int HpCurrent { get; set; } = 10;
    public int HpMax { get; set; } = 10;
    public int TempHp { get; set; } = 0;
    public int HitDiceSpent { get; set; } = 0;
    public int DeathSaveSuccesses { get; set; } = 0; // 0–3
    public int DeathSaveFailures { get; set; } = 0;  // 0–3
    public bool HeroicInspiration { get; set; } = false;

    // ===== Защита =====
    public int ArmorClass { get; set; } = 10;        // пересчитывается через Recalculate()
    public string? ArmorId { get; set; }             // id из DndPresets.Armors, null = без доспеха
    public bool HasShield { get; set; } = false;

    // ===== Вид =====
    public int Speed { get; set; } = 30;
    public string Size { get; set; } = "Середній";
    public int Darkvision { get; set; } = 0;

    // ===== Владения и черты =====
    public List<string> SaveProficiencies { get; set; } = [];   // имена Ability: "Strength", ...
    public List<string> SkillProficiencies { get; set; } = [];  // id навыков
    public List<string> SkillExpertise { get; set; } = [];      // Экспертность (удвоенный Бонус владения)
    public List<string> ArmorTraining { get; set; } = [];       // "light", "medium", "heavy", "shield"
    public List<string> WeaponProficiencies { get; set; } = [];
    public List<string> ToolProficiencies { get; set; } = [];
    public List<string> Feats { get; set; } = [];               // id черт, первая — от предыстории

    // ============================================================
    // ВЫЧИСЛЯЕМЫЕ ПОЛЯ (в БД не хранятся: у них нет сеттеров)
    // ============================================================

    /// <summary>Бонус владения зависит от общего уровня.</summary>
    public int ProficiencyBonus => DndPresets.ProficiencyBonus(Level);

    public int GetScore(Ability a) => a switch
    {
        Ability.Strength => Strength,
        Ability.Dexterity => Dexterity,
        Ability.Constitution => Constitution,
        Ability.Intelligence => Intelligence,
        Ability.Wisdom => Wisdom,
        Ability.Charisma => Charisma,
        _ => throw new ArgumentOutOfRangeException(nameof(a))
    };

    public int GetModifier(Ability a) => DndPresets.Modifier(GetScore(a));

    public int StrengthMod => GetModifier(Ability.Strength);
    public int DexterityMod => GetModifier(Ability.Dexterity);
    public int ConstitutionMod => GetModifier(Ability.Constitution);
    public int IntelligenceMod => GetModifier(Ability.Intelligence);
    public int WisdomMod => GetModifier(Ability.Wisdom);
    public int CharismaMod => GetModifier(Ability.Charisma);

    /// <summary>Спасбросок = модификатор + Бонус владения, если есть владение.</summary>
    public int GetSaveBonus(Ability a) =>
        GetModifier(a) + (SaveProficiencies.Contains(a.ToString()) ? ProficiencyBonus : 0);

    /// <summary>Навык = модификатор характеристики + Бонус владения (×2 при Экспертности).</summary>
    public int GetSkillBonus(string skillId)
    {
        var skill = DndPresets.GetSkill(skillId)
                    ?? throw new ArgumentException($"Невідома навичка: {skillId}");
        var bonus = GetModifier(skill.Ability);
        if (SkillExpertise.Contains(skill.Id)) bonus += ProficiencyBonus * 2;
        else if (SkillProficiencies.Contains(skill.Id)) bonus += ProficiencyBonus;
        return bonus;
    }

    /// <summary>Пассивное Восприятие = 10 + бонус навыка Восприятие.</summary>
    public int PassivePerception => 10 + GetSkillBonus("perception");

    /// <summary>Бросок Инициативы = к20 + это значение (черта «Бдительный» добавляет Бонус владения).</summary>
    public int InitiativeBonus => DexterityMod + (Feats.Contains("alert") ? ProficiencyBonus : 0);

    /// <summary>Фиксированное значение Инициативы = 10 + бонус.</summary>
    public int InitiativeScore => 10 + InitiativeBonus;

    /// <summary>Кости хитов: всего = уровень.</summary>
    public int HitDiceTotal => Level;
    public int HitDiceLeft => Math.Max(0, Level - HitDiceSpent);
    public int HitDieType => DndPresets.GetClass(ClassId)?.HitDie ?? 8;

    /// <summary>Грузоподъёмность (фунты) = Сила × 15.</summary>
    public int CarryCapacityLb => Strength * 15;

    /// <summary>Скорость с учётом требования Силы у доспеха (−10 футов).</summary>
    public int EffectiveSpeed
    {
        get
        {
            var armor = ArmorId is null ? null : DndPresets.GetArmor(ArmorId);
            return armor is not null && armor.StrengthRequired > Strength ? Math.Max(0, Speed - 10) : Speed;
        }
    }

    /// <summary>Сл спасброска от заклинаний = 8 + модификатор + Бонус владения (null у не-заклинателей).</summary>
    public int? SpellSaveDc =>
        DndPresets.GetClass(ClassId)?.SpellcastingAbility is { } a ? 8 + GetModifier(a) + ProficiencyBonus : null;

    /// <summary>Бонус атаки заклинанием = модификатор + Бонус владения.</summary>
    public int? SpellAttackBonus =>
        DndPresets.GetClass(ClassId)?.SpellcastingAbility is { } a ? GetModifier(a) + ProficiencyBonus : null;

    /// <summary>Бонус атаки оружием: модификатор Силы (рукопашное) или Ловкости (дальнобойное/фехтовальное) + Бонус владения.</summary>
    public int GetAttackBonus(Ability ability, bool proficient = true) =>
        GetModifier(ability) + (proficient ? ProficiencyBonus : 0);

    public int XpForNextLevel => Level >= 20 ? DndPresets.XpThresholds[^1] : DndPresets.XpThresholds[Level];

    // ============================================================
    // РАСЧЁТЫ, КОТОРЫЕ СОХРАНЯЮТСЯ В ПОЛЯ
    // ============================================================

    /// <summary>
    /// Максимум Хитов: 1-й уровень = Кость хитов (максимум) + Тел;
    /// каждый следующий = фиксированное значение (кость/2 + 1) + Тел (минимум 1).
    /// Бонусы: Дварф +1 за уровень, черта «Міцний» +2 за уровень.
    /// </summary>
    public int CalculateMaxHp()
    {
        var charClass = DndPresets.GetClass(ClassId);
        if (charClass is null) return HpMax;

        var con = ConstitutionMod;
        var hp = charClass.HitDie + con;
        hp += (Level - 1) * Math.Max(1, charClass.HitDie / 2 + 1 + con);

        hp += (DndPresets.GetRace(RaceId)?.HpPerLevelBonus ?? 0) * Level;
        if (Feats.Contains("tough")) hp += 2 * Level;

        return Math.Max(1, hp);
    }

    /// <summary>
    /// Класс Защиты:
    /// без доспеха 10 + Лов (Варвар: + Тел; Монах без Щита: + Муд);
    /// лёгкий: база + Лов; средний: база + Лов (максимум +2); тяжёлый: база;
    /// Щит +2, только при навыке обращения со Щитами.
    /// </summary>
    public int CalculateArmorClass()
    {
        var armor = ArmorId is null ? null : DndPresets.GetArmor(ArmorId);
        var dex = DexterityMod;
        int ac;

        if (armor is not null)
        {
            ac = armor.Category switch
            {
                ArmorCategory.Light => armor.BaseAc + dex,
                ArmorCategory.Medium => armor.BaseAc + Math.Min(dex, 2),
                _ => armor.BaseAc
            };
        }
        else
        {
            var unarmored = DndPresets.GetClass(ClassId)?.Unarmored ?? UnarmoredDefense.Standard;
            ac = unarmored switch
            {
                UnarmoredDefense.Barbarian => 10 + dex + ConstitutionMod,
                UnarmoredDefense.Monk when !HasShield => 10 + dex + WisdomMod,
                _ => 10 + dex
            };
        }

        if (HasShield && ArmorTraining.Contains("shield")) ac += DndPresets.ShieldAcBonus;
        return ac;
    }

    /// <summary>Пересчитывает сохраняемые производные поля (Хиты и КЗ). Вызывать после смены характеристик, доспеха или уровня.</summary>
    public void Recalculate()
    {
        HpMax = CalculateMaxHp();
        HpCurrent = Math.Min(HpCurrent, HpMax);
        ArmorClass = CalculateArmorClass();
    }

    /// <summary>Выдаёт Опыт и повышает уровень по таблице «Развитие персонажа».</summary>
    public void AddXp(int amount)
    {
        Xp = Math.Max(0, Xp + amount);
        SetLevel(DndPresets.LevelForXp(Xp));
    }

    public void SetLevel(int level)
    {
        level = Math.Clamp(level, 1, 20);
        var oldMax = HpMax;

        Level = level;
        Xp = Math.Max(Xp, DndPresets.XpThresholds[level - 1]);

        HpMax = CalculateMaxHp();
        HpCurrent = Math.Clamp(HpCurrent + (HpMax - oldMax), 0, HpMax);
        ArmorClass = CalculateArmorClass();
    }

    // ============================================================
    // СОЗДАНИЕ ПЕРСОНАЖА (гл. 2)
    // ============================================================

    /// <summary>
    /// Шаги из главы 2: класс → предыстория → вид.
    /// Набор характеристик берётся из пресета класса (стандартный ряд 15/14/13/12/10/8),
    /// затем добавляются бонусы предыстории: либо +2 / +1 (plusTwo / plusOne), либо +1 ко всем трём; максимум 20.
    /// classSkills — навыки, выбранные от класса (количество и список задаёт класс; повторять навыки предыстории нельзя).
    /// </summary>
    public void ApplyPresets(
        string raceId,
        string classId,
        string? backgroundId = null,
        Ability? plusTwo = null,
        Ability? plusOne = null,
        IEnumerable<string>? classSkills = null)
    {
        var race = DndPresets.GetRace(raceId)
                   ?? throw new ArgumentException($"Невідома раса: {raceId}");
        var charClass = DndPresets.GetClass(classId)
                        ?? throw new ArgumentException($"Невідомий клас: {classId}");

        BackgroundPreset? background = null;
        if (backgroundId is not null)
            background = DndPresets.GetBackground(backgroundId)
                         ?? throw new ArgumentException($"Невідома передісторія: {backgroundId}");

        RaceId = race.Id;
        ClassId = charClass.Id;
        BackgroundId = background?.Id ?? string.Empty;
        RaceCharacter = race.Name;
        ClassCharacter = charClass.Name;
        BackgroundCharacter = background?.Name ?? string.Empty;

        // 1) Набір характеристик класа 
        var scores = new Dictionary<Ability, int>();
        foreach (var a in Enum.GetValues<Ability>())
            scores[a] = charClass.BaseStats.Get(a) + race.StatBonuses.Get(a);

        // 2) Бонуси передісторії: +2/+1 або +1/+1/+1 сред трьох перечислених характеристик
        if (background is not null)
        {
            if (plusTwo is null && plusOne is null)
            {
                foreach (var a in background.AbilityOptions) scores[a] += 1;
            }
            else
            {
                if (plusTwo is null || plusOne is null || plusTwo == plusOne)
                    throw new ArgumentException("Потрібно вказати дві різні характеристики: +2 та +1.");
                if (!background.AbilityOptions.Contains(plusTwo.Value) || !background.AbilityOptions.Contains(plusOne.Value))
                    throw new ArgumentException($"Передісторія «{background.Name}» дає бонуси лише до: {string.Join(", ", background.AbilityOptions)}.");
                scores[plusTwo.Value] += 2;
                scores[plusOne.Value] += 1;
            }
        }

        // 3) Максимальне значення - 20
        foreach (var a in Enum.GetValues<Ability>())
            scores[a] = Math.Min(20, scores[a]);

        Strength = scores[Ability.Strength];
        Dexterity = scores[Ability.Dexterity];
        Constitution = scores[Ability.Constitution];
        Intelligence = scores[Ability.Intelligence];
        Wisdom = scores[Ability.Wisdom];
        Charisma = scores[Ability.Charisma];

        // 4) Володіння
        SaveProficiencies = charClass.SavingThrows.Select(a => a.ToString()).ToList();
        ArmorTraining = charClass.ArmorTraining.ToList();
        WeaponProficiencies = charClass.Weapons.ToList();
        SkillExpertise = [];

        SkillProficiencies = background is null ? [] : background.Skills.ToList();
        ToolProficiencies = background is null ? [] : [background.Tool];
        Feats = background is null ? [] : [background.FeatId];

        if (classSkills is not null)
        {
            var chosen = classSkills.Select(s => s.ToLowerInvariant()).Distinct().ToList();
            if (chosen.Count != charClass.SkillChoices)
                throw new ArgumentException($"Клас «{charClass.Name}» обирає навичок: {charClass.SkillChoices}.");
            foreach (var skill in chosen)
            {
                if (!charClass.SkillOptions.Contains(skill))
                    throw new ArgumentException($"Навичка «{skill}» недоступна класу «{charClass.Name}».");
                if (SkillProficiencies.Contains(skill))
                    throw new ArgumentException($"Навичку «{skill}» уже дає передісторія, оберіть іншу.");
            }
            SkillProficiencies.AddRange(chosen);
        }

        // 5) Вигляд
        Speed = race.Speed;
        Size = race.Size;
        Darkvision = race.Darkvision;

        // 6) Стартовий стан: 1-й левел, без броні
        Level = 1;
        Xp = 0;
        ArmorId = null;
        HasShield = false;
        TempHp = 0;
        HitDiceSpent = 0;

        HpMax = CalculateMaxHp();
        HpCurrent = HpMax;
        ArmorClass = CalculateArmorClass();
    }
}


