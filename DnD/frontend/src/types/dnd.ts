export type Ability =
    | 'Strength'
    | 'Dexterity'
    | 'Constitution'
    | 'Intelligence'
    | 'Wisdom'
    | 'Charisma';

export interface StatBlock {
    strength: number;
    dexterity: number;
    constitution: number;
    intelligence: number;
    wisdom: number;
    charisma: number;
}

export interface RacePreset {
    id: string;
    name: string;
    statBonuses: StatBlock;
    speed: number;
    size: string;
    darkvision: number;
    hpPerLevelBonus: number;
}

export interface SkillPreset {
    id: string;
    name: string;
    ability: Ability;
}

export interface BackgroundPreset {
    id: string;
    name: string;
    abilityOptions: Ability[];
    featId: string;
    featName: string;
    skills: string[];
    tool: string;
}

export interface ClassPreset {
    id: string;
    name: string;
    hitDie: number;
    baseStats: StatBlock;
    savingThrows: Ability[];
    skillChoices: number;
    skillOptions: string[];
    armorTraining: string[];
    weapons: string[];
    spellcastingAbility: Ability | null;
    unarmored: string;
}

export interface ArmorPreset {
    id: string;
    name: string;
    category: string;
    baseAc: number;
    strengthRequired: number;
}

export interface PresetsResponse {
    races: RacePreset[];
    classes: ClassPreset[];
    backgrounds: BackgroundPreset[];
    skills: SkillPreset[];
    armors: ArmorPreset[];
    shieldAcBonus: number;
    xpThresholds: number[];
}

export interface RoomCreatedResponse {
    id: string;
    code: string;
    name: string;
    dmToken: string;
}

export interface CharacterResponse {
    id: string;
    namePlayer: string;
    nameCharacter: string;
    description: string;
    alignment: string;
    classCharacter: string;
    raceCharacter: string;
    backgroundCharacter: string;
    level: number;
    xp: number;
    xpForNextLevel: number;
    proficiencyBonus: number;
    strength: number;
    strengthMod: number;
    dexterity: number;
    dexterityMod: number;
    constitution: number;
    constitutionMod: number;
    intelligence: number;
    intelligenceMod: number;
    wisdom: number;
    wisdomMod: number;
    charisma: number;
    charismaMod: number;
    hpCurrent: number;
    hpMax: number;
    tempHp: number;
    hitDiceLeft: number;
    hitDiceTotal: number;
    hitDieType: number;
    deathSaveSuccesses: number;
    deathSaveFailures: number;
    heroicInspiration: boolean;
    armorClass: number;
    armorId: string | null;
    hasShield: boolean;
    effectiveSpeed: number;
    size: string;
    darkvision: number;
    passivePerception: number;
    initiativeBonus: number;
    spellSaveDc: number | null;
    spellAttackBonus: number | null;
    savingThrows: Record<string, number>;
    skills: Record<string, number>;
    armorTraining: string[];
    weaponProficiencies: string[];
    toolProficiencies: string[];
    feats: string[];
    classId: string;
    raceId: string;
    backgroundId: string;
    speed: number;
    initiativeScore: number;
    carryCapacityLb: number;
    saveProficiencies: string[];
    skillProficiencies: string[];
    skillExpertise: string[];
}

export interface RoomDetailsResponse {
    id: string;
    code: string;
    name: string;
    isDm: boolean;
    characters: CharacterResponse[];
}

export interface CharacterCreatedResponse {
    character: CharacterResponse;
    playerToken: string;
}

export interface CreateCharacterRequest {
    namePlayer: string;
    nameCharacter: string;
    description?: string | null;
    alignment?: string | null;
    raceId: string;
    classId: string;
    backgroundId?: string | null;
    plusTwo?: Ability | null;
    plusOne?: Ability | null;
    classSkills?: string[] | null;
}

export interface UpdateCharacterStatsRequest {
    hpCurrent?: number;
    tempHp?: number;
    hitDiceSpent?: number;
    deathSaveSuccesses?: number;
    deathSaveFailures?: number;
    heroicInspiration?: boolean;
    armorId?: string;
    hasShield?: boolean;
    xpToAdd?: number;
    description?: string;
}