import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { dndApi, tokenStorage } from './api/dndApi';
import type{
  Ability,
  CharacterResponse,
  PresetsResponse,
  RoomDetailsResponse,
  UpdateCharacterStatsRequest,
} from './types/dnd';
import './App.css';

type Screen = 'menu' | 'join' | 'create-name' | 'created' | 'char-create' | 'room';
type SheetTab = 'stats' | 'skills' | 'gear';

const HUB_PATH = '/roomHub';

const ABILITIES: { key: Ability; label: string; full: string }[] = [
  { key: 'Strength', label: 'STR', full: 'Strength' },
  { key: 'Dexterity', label: 'DEX', full: 'Dexterity' },
  { key: 'Constitution', label: 'CON', full: 'Constitution' },
  { key: 'Intelligence', label: 'INT', full: 'Intelligence' },
  { key: 'Wisdom', label: 'WIS', full: 'Wisdom' },
  { key: 'Charisma', label: 'CHA', full: 'Charisma' },
];

function formatMod(val: number): string {
  return val >= 0 ? `+${val}` : `${val}`;
}

export default function App() {
  // Navigation & Global State
  const [screen, setScreen] = useState<Screen>('menu');
  const [presets, setPresets] = useState<PresetsResponse | null>(null);
  const [room, setRoom] = useState<RoomDetailsResponse | null>(null);
  const [selectedCharId, setSelectedCharId] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<SheetTab>('stats');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [copiedFlag, setCopiedFlag] = useState(false);

  // API Settings State
  const [showApiConfig, setShowApiConfig] = useState(false);
  const [apiBaseInput, setApiBaseInput] = useState(() => tokenStorage.getApiBase());

  // Join / Create Room Inputs
  const [joinCode, setJoinCode] = useState('');
  const [roomNameInput, setRoomNameInput] = useState('');

  // Character Creation Form State (2024 Rules)
  const [playerName, setPlayerName] = useState('');
  const [charName, setCharName] = useState('');
  const [classId, setClassId] = useState('');
  const [raceId, setRaceId] = useState('');
  const [backgroundId, setBackgroundId] = useState('');
  const [bonusMode, setBonusMode] = useState<'111' | '21'>('111');
  const [plusTwo, setPlusTwo] = useState<Ability | ''>('');
  const [plusOne, setPlusOne] = useState<Ability | ''>('');
  const [selectedSkills, setSelectedSkills] = useState<string[]>([]);
  const [alignment, setAlignment] = useState('');
  const [description, setDescription] = useState('');

  // DM XP Input
  const [xpInput, setXpInput] = useState('');

  const hubRef = useRef<signalR.HubConnection | null>(null);

  // =========================================================================
  // PRESETS LOADER
  // =========================================================================
  const ensurePresets = useCallback(async (): Promise<PresetsResponse> => {
    if (presets) return presets;
    const loaded = await dndApi.getPresets();
    setPresets(loaded);
    return loaded;
  }, [presets]);

  // =========================================================================
  // SIGNALR REAL-TIME SUBSCRIPTION (RoomHub.cs)
  // =========================================================================
  const disconnectHub = useCallback(async (roomCode?: string) => {
    const conn = hubRef.current;
    if (!conn) return;
    hubRef.current = null;
    try {
      if (roomCode) {
        await conn.invoke('LeaveRoomGroup', roomCode);
      }
      await conn.stop();
    } catch {
      // Ignore cleanup errors
    }
  }, []);

  const connectHub = useCallback(
      async (roomCode: string) => {
        await disconnectHub();
        const base = tokenStorage.getApiBase().replace(/\/+$/, '');
        const conn = new signalR.HubConnectionBuilder()
            .withUrl(`${base}${HUB_PATH}`)
            .withAutomaticReconnect()
            .build();

        conn.on('CharacterAdded', (charDto: CharacterResponse) => {
          setRoom((prev) => {
            if (!prev) return prev;
            const exists = prev.characters.some((c) => c.id === charDto.id);
            const nextChars = exists
                ? prev.characters.map((c) => (c.id === charDto.id ? charDto : c))
                : [...prev.characters, charDto];
            return { ...prev, characters: nextChars };
          });
          setSelectedCharId((prevId) => prevId ?? charDto.id);
        });

        conn.on('CharacterUpdated', (charDto: CharacterResponse) => {
          setRoom((prev) => {
            if (!prev) return prev;
            const exists = prev.characters.some((c) => c.id === charDto.id);
            const nextChars = exists
                ? prev.characters.map((c) => (c.id === charDto.id ? charDto : c))
                : [...prev.characters, charDto];
            return { ...prev, characters: nextChars };
          });
        });

        conn.on('CharacterRemoved', (charId: string) => {
          setRoom((prev) => {
            if (!prev) return prev;
            const remaining = prev.characters.filter((c) => c.id !== charId);
            setSelectedCharId((curr) => (curr === charId ? remaining[0]?.id ?? null : curr));
            return { ...prev, characters: remaining };
          });
        });

        conn.onreconnected(async () => {
          try {
            await conn.invoke('JoinRoomGroup', roomCode);
          } catch {
            // Ignore reconnect error
          }
        });

        try {
          await conn.start();
          await conn.invoke('JoinRoomGroup', roomCode);
          hubRef.current = conn;
        } catch (err) {
          console.warn('SignalR connection warning:', err);
        }
      },
      [disconnectHub]
  );

  useEffect(() => {
    return () => {
      void disconnectHub();
    };
  }, [disconnectHub]);

  // =========================================================================
  // DERIVED CHARACTER CREATOR DATA
  // =========================================================================
  const selectedClass = useMemo(
      () => presets?.classes.find((c) => c.id === classId) ?? null,
      [presets, classId]
  );
  const selectedRace = useMemo(
      () => presets?.races.find((r) => r.id === raceId) ?? null,
      [presets, raceId]
  );
  const selectedBackground = useMemo(
      () => presets?.backgrounds.find((b) => b.id === backgroundId) ?? null,
      [presets, backgroundId]
  );

  const openCharacterCreator = async () => {
    setError('');
    const loaded = await ensurePresets();
    const defaultClass = loaded.classes[0];
    const defaultRace = loaded.races[0];
    setClassId(defaultClass?.id ?? '');
    setRaceId(defaultRace?.id ?? '');
    setBackgroundId('');
    setBonusMode('111');
    setPlusTwo('');
    setPlusOne('');
    setSelectedSkills([]);
    setPlayerName('');
    setCharName('');
    setAlignment('');
    setDescription('');
    setScreen('char-create');
  };

  const handleBackgroundChange = (newBgId: string) => {
    setBackgroundId(newBgId);
    const bg = presets?.backgrounds.find((b) => b.id === newBgId);
    if (bg) {
      setPlusTwo(bg.abilityOptions[0] ?? '');
      setPlusOne(bg.abilityOptions[1] ?? bg.abilityOptions[0] ?? '');
      // Remove skills already granted by the selected background
      setSelectedSkills((prev) => prev.filter((s) => !bg.skills.includes(s)));
    } else {
      setPlusTwo('');
      setPlusOne('');
    }
  };

  const handleClassChange = (newClassId: string) => {
    setClassId(newClassId);
    const cls = presets?.classes.find((c) => c.id === newClassId);
    if (cls) {
      setSelectedSkills((prev) =>
          prev.filter((s) => cls.skillOptions.includes(s)).slice(0, cls.skillChoices)
      );
    }
  };

  const toggleSkill = (skillId: string) => {
    if (!selectedClass) return;
    if (selectedBackground?.skills.includes(skillId)) return;

    setSelectedSkills((prev) => {
      if (prev.includes(skillId)) {
        return prev.filter((s) => s !== skillId);
      }
      if (prev.length >= selectedClass.skillChoices) {
        return prev;
      }
      return [...prev, skillId];
    });
  };

  // =========================================================================
  // ROOM ACTIONS
  // =========================================================================
  const handleJoinRoom = async (e?: React.FormEvent) => {
    e?.preventDefault();
    const code = joinCode.trim().toUpperCase();
    if (!code) {
      setError('Please enter a room password.');
      return;
    }

    setLoading(true);
    setError('');
    try {
      await ensurePresets();
      const details = await dndApi.getRoom(code);
      setRoom(details);
      await connectHub(details.code);

      const savedCharId = tokenStorage.getSavedCharId(details.code);
      const existingChar = details.characters.find((c) => c.id === savedCharId);

      if (details.isDm || existingChar) {
        setSelectedCharId(existingChar ? existingChar.id : details.characters[0]?.id ?? null);
        setScreen('room');
      } else {
        await openCharacterCreator();
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to join room.');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateRoom = async (e?: React.FormEvent) => {
    e?.preventDefault();
    const name = roomNameInput.trim();
    if (!name) {
      setError('Room name is required.');
      return;
    }

    setLoading(true);
    setError('');
    try {
      await ensurePresets();
      const created = await dndApi.createRoom(name);
      tokenStorage.setDmToken(created.code, created.dmToken);
      setRoom({
        id: created.id,
        code: created.code,
        name: created.name,
        isDm: true,
        characters: [],
      });
      setScreen('created');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create room.');
    } finally {
      setLoading(false);
    }
  };

  const handleStartGame = async () => {
    if (!room) return;
    setLoading(true);
    try {
      await connectHub(room.code);
      setScreen('room');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateCharacter = async (e?: React.FormEvent) => {
    e?.preventDefault();
    if (!room) return;

    setLoading(true);
    setError('');
    try {
      const created = await dndApi.createCharacter(room.code, {
        namePlayer: playerName.trim(),
        nameCharacter: charName.trim(),
        raceId,
        classId,
        backgroundId: backgroundId || null,
        plusTwo: backgroundId && bonusMode === '21' && plusTwo ? plusTwo : null,
        plusOne: backgroundId && bonusMode === '21' && plusOne ? plusOne : null,
        classSkills: selectedSkills,
        alignment: alignment.trim() || null,
        description: description.trim() || null,
      });

      tokenStorage.setPlayerToken(room.code, created.playerToken, created.character.id);
      setSelectedCharId(created.character.id);
      setRoom((prev) => {
        if (!prev) return prev;
        const exists = prev.characters.some((c) => c.id === created.character.id);
        return {
          ...prev,
          characters: exists ? prev.characters : [...prev.characters, created.character],
        };
      });
      setScreen('room');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create character.');
    } finally {
      setLoading(false);
    }
  };

  const handleLeaveRoom = async () => {
    await disconnectHub(room?.code);
    setRoom(null);
    setSelectedCharId(null);
    setError('');
    setScreen('menu');
  };

  const copyCode = async (code: string) => {
    try {
      await navigator.clipboard.writeText(code);
      setCopiedFlag(true);
      setTimeout(() => setCopiedFlag(false), 1200);
    } catch {
      // Ignore clipboard failure
    }
  };

  // =========================================================================
  // ACTIVE CHARACTER MUTATIONS (PATCH / DELETE)
  // =========================================================================
  const activeChar = useMemo(() => {
    if (!room) return null;
    if (selectedCharId) {
      const found = room.characters.find((c) => c.id === selectedCharId);
      if (found) return found;
    }
    const savedId = tokenStorage.getSavedCharId(room.code);
    return room.characters.find((c) => c.id === savedId) ?? room.characters[0] ?? null;
  }, [room, selectedCharId]);

  const patchCharacter = async (payload: UpdateCharacterStatsRequest) => {
    if (!room || !activeChar) return;
    setError('');
    try {
      const updated = await dndApi.updateCharacter(room.code, activeChar.id, payload);
      setRoom((prev) => {
        if (!prev) return prev;
        return {
          ...prev,
          characters: prev.characters.map((c) => (c.id === updated.id ? updated : c)),
        };
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update character.');
    }
  };

  const handleDeleteCharacter = async () => {
    if (!room || !activeChar) return;
    setError('');
    try {
      await dndApi.deleteCharacter(room.code, activeChar.id);
      setRoom((prev) => {
        if (!prev) return prev;
        const remaining = prev.characters.filter((c) => c.id !== activeChar.id);
        setSelectedCharId(remaining[0]?.id ?? null);
        return { ...prev, characters: remaining };
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete character.');
    }
  };

  const isDm = Boolean(room && (room.isDm || tokenStorage.getDmToken(room.code)));

  // =========================================================================
  // RENDER
  // =========================================================================
  return (
      <div className="app-shell">
        {/* ==================== SCREEN 1: MAIN MENU ==================== */}
        {screen === 'menu' && (
            <section className="screen screen-centered">
              <button
                  type="button"
                  className="settings-corner"
                  onClick={() => setShowApiConfig((v) => !v)}
              >
                API
              </button>

              <div className="menu-stack">
                {showApiConfig && (
                    <div className="info-card" style={{ width: '100%', textAlign: 'left' }}>
                      <div className="form-group" style={{ marginBottom: 8 }}>
                        <label className="form-label" htmlFor="api-url">
                          Backend Base URL
                        </label>
                        <input
                            id="api-url"
                            className="pill-input sm"
                            type="text"
                            value={apiBaseInput}
                            onChange={(e) => setApiBaseInput(e.target.value)}
                            placeholder="http://localhost:5000"
                        />
                      </div>
                      <button
                          type="button"
                          className="pill-btn compact"
                          onClick={() => {
                            if (apiBaseInput.trim()) {
                              tokenStorage.setApiBase(apiBaseInput.trim());
                              setPresets(null);
                            }
                            setShowApiConfig(false);
                          }}
                      >
                        SAVE URL
                      </button>
                    </div>
                )}

                <button
                    type="button"
                    className="pill-btn"
                    onClick={() => {
                      setError('');
                      setJoinCode('');
                      setScreen('join');
                    }}
                >
                  JOIN ROOM
                </button>

                <button
                    type="button"
                    className="pill-btn"
                    onClick={() => {
                      setError('');
                      setRoomNameInput('');
                      setScreen('create-name');
                    }}
                >
                  CREATE ROOM
                </button>
              </div>
            </section>
        )}

        {/* ==================== SCREEN 2: ENTER PASSWORD ==================== */}
        {screen === 'join' && (
            <form className="screen screen-centered" onSubmit={handleJoinRoom}>
              <div className="menu-stack">
                <p className="subtitle">Enter the room&apos;s password to access the game.</p>
                <h1 className="title-xl">ENTER PASSWORD</h1>
                <input
                    className="pill-input uppercase"
                    type="text"
                    maxLength={12}
                    autoFocus
                    autoComplete="off"
                    autoCapitalize="characters"
                    placeholder="Password"
                    value={joinCode}
                    onChange={(e) => setJoinCode(e.target.value.toUpperCase())}
                />
                {error && <div className="error-banner">{error}</div>}
                <button type="submit" className="pill-btn" disabled={loading}>
                  {loading ? 'JOINING...' : 'JOIN'}
                </button>
                <button
                    type="button"
                    className="link-btn"
                    onClick={() => {
                      setError('');
                      setScreen('menu');
                    }}
                >
                  BACK
                </button>
              </div>
            </form>
        )}

        {/* ==================== SCREEN 3A: ROOM NAME ==================== */}
        {screen === 'create-name' && (
            <form className="screen screen-centered" onSubmit={handleCreateRoom}>
              <div className="menu-stack">
                <p className="subtitle">Name your campaign room for the party.</p>
                <h1 className="title-xl">ROOM NAME</h1>
                <input
                    className="pill-input"
                    type="text"
                    maxLength={100}
                    autoFocus
                    autoComplete="off"
                    placeholder="Campaign Name"
                    value={roomNameInput}
                    onChange={(e) => setRoomNameInput(e.target.value)}
                />
                {error && <div className="error-banner">{error}</div>}
                <button type="submit" className="pill-btn" disabled={loading}>
                  {loading ? 'CREATING...' : 'CREATE ROOM'}
                </button>
                <button
                    type="button"
                    className="link-btn"
                    onClick={() => {
                      setError('');
                      setScreen('menu');
                    }}
                >
                  BACK
                </button>
              </div>
            </form>
        )}

        {/* ==================== SCREEN 3B: ROOM CREATED! ==================== */}
        {screen === 'created' && room && (
            <section className="screen screen-centered">
              <div className="menu-stack">
                <h1 className="title-xl">ROOM CREATED!</h1>
                <p className="subtitle">Here is the access password for your room:</p>
                <div
                    className="code-box"
                    title="Tap to copy"
                    onClick={() => void copyCode(room.code)}
                >
                  {copiedFlag ? 'COPIED!' : room.code}
                </div>
                <button
                    type="button"
                    className="pill-btn"
                    disabled={loading}
                    onClick={() => void handleStartGame()}
                >
                  START GAME
                </button>
              </div>
            </section>
        )}

        {/* ==================== SCREEN 4: CHARACTER CREATION (2024 RULES) ==================== */}
        {screen === 'char-create' && room && presets && (
            <form className="screen" onSubmit={handleCreateCharacter}>
              <div className="top-bar">
                <h2 className="top-bar-title">{room.name || 'NEW CHARACTER'}</h2>
                <span className="badge" onClick={() => void copyCode(room.code)}>
              {copiedFlag ? 'COPIED' : room.code}
            </span>
              </div>

              {error && <div className="error-banner">{error}</div>}

              <div className="form-group">
                <label className="form-label" htmlFor="player-name">
                  Player Name *
                </label>
                <input
                    id="player-name"
                    className="pill-input sm"
                    type="text"
                    maxLength={100}
                    required
                    placeholder="Your name"
                    value={playerName}
                    onChange={(e) => setPlayerName(e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="char-name">
                  Character Name *
                </label>
                <input
                    id="char-name"
                    className="pill-input sm"
                    type="text"
                    maxLength={100}
                    required
                    placeholder="Hero name"
                    value={charName}
                    onChange={(e) => setCharName(e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="class-select">
                  Class *
                </label>
                <select
                    id="class-select"
                    className="pill-select"
                    value={classId}
                    onChange={(e) => handleClassChange(e.target.value)}
                >
                  {presets.classes.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name} (d{c.hitDie})
                      </option>
                  ))}
                </select>
                {selectedClass && (
                    <div className="subtitle" style={{ fontSize: 12, marginTop: 4 }}>
                      Base Stats: STR {selectedClass.baseStats.strength}, DEX{' '}
                      {selectedClass.baseStats.dexterity}, CON {selectedClass.baseStats.constitution},
                      INT {selectedClass.baseStats.intelligence}, WIS {selectedClass.baseStats.wisdom},
                      CHA {selectedClass.baseStats.charisma}
                    </div>
                )}
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="race-select">
                  Species (Race) *
                </label>
                <select
                    id="race-select"
                    className="pill-select"
                    value={raceId}
                    onChange={(e) => setRaceId(e.target.value)}
                >
                  {presets.races.map((r) => (
                      <option key={r.id} value={r.id}>
                        {r.name}
                      </option>
                  ))}
                </select>
                {selectedRace && (
                    <div className="subtitle" style={{ fontSize: 12, marginTop: 4 }}>
                      Speed: {selectedRace.speed} ft • Size: {selectedRace.size}
                      {selectedRace.darkvision > 0 ? ` • Darkvision: ${selectedRace.darkvision} ft` : ''}
                    </div>
                )}
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="bg-select">
                  Background
                </label>
                <select
                    id="bg-select"
                    className="pill-select"
                    value={backgroundId}
                    onChange={(e) => handleBackgroundChange(e.target.value)}
                >
                  <option value="">— None —</option>
                  {presets.backgrounds.map((b) => (
                      <option key={b.id} value={b.id}>
                        {b.name}
                      </option>
                  ))}
                </select>
                {selectedBackground && (
                    <div className="subtitle" style={{ fontSize: 12, marginTop: 4 }}>
                      Feat: {selectedBackground.featName} • Skills:{' '}
                      {selectedBackground.skills
                          .map((sid) => presets.skills.find((s) => s.id === sid)?.name || sid)
                          .join(', ')}
                    </div>
                )}
              </div>

              {selectedBackground && (
                  <div className="info-card">
                    <div className="form-label" style={{ marginBottom: 8 }}>
                      Background Ability Bonuses
                    </div>
                    <div className="chip-grid" style={{ marginBottom: 10 }}>
                      <button
                          type="button"
                          className={`chip ${bonusMode === '111' ? 'selected' : ''}`}
                          onClick={() => setBonusMode('111')}
                      >
                        +1 / +1 / +1 (All 3)
                      </button>
                      <button
                          type="button"
                          className={`chip ${bonusMode === '21' ? 'selected' : ''}`}
                          onClick={() => setBonusMode('21')}
                      >
                        +2 / +1 (Choose 2)
                      </button>
                    </div>

                    {bonusMode === '21' && (
                        <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
                          <div>
                            <span className="form-label">+2 Ability:</span>
                            <select
                                className="pill-select"
                                style={{ marginTop: 4 }}
                                value={plusTwo}
                                onChange={(e) => setPlusTwo(e.target.value as Ability)}
                            >
                              {selectedBackground.abilityOptions.map((ab) => (
                                  <option key={ab} value={ab}>
                                    {ab}
                                  </option>
                              ))}
                            </select>
                          </div>
                          <div>
                            <span className="form-label">+1 Ability:</span>
                            <select
                                className="pill-select"
                                style={{ marginTop: 4 }}
                                value={plusOne}
                                onChange={(e) => setPlusOne(e.target.value as Ability)}
                            >
                              {selectedBackground.abilityOptions.map((ab) => (
                                  <option key={ab} value={ab}>
                                    {ab}
                                  </option>
                              ))}
                            </select>
                          </div>
                        </div>
                    )}
                  </div>
              )}

              {selectedClass && (
                  <div className="form-group">
                    <label className="form-label">
                      Class Skills ({selectedSkills.length} / {selectedClass.skillChoices})
                    </label>
                    <div className="chip-grid">
                      {selectedClass.skillOptions.map((skillId) => {
                        const skillObj = presets.skills.find((s) => s.id === skillId);
                        const label = skillObj ? skillObj.name : skillId;
                        const isBgSkill = selectedBackground?.skills.includes(skillId) ?? false;
                        const isSelected = selectedSkills.includes(skillId);

                        return (
                            <button
                                key={skillId}
                                type="button"
                                disabled={isBgSkill}
                                className={`chip ${isSelected ? 'selected' : ''} ${
                                    isBgSkill ? 'disabled' : ''
                                }`}
                                onClick={() => toggleSkill(skillId)}
                            >
                              {isBgSkill ? `${label} (BG)` : label}
                            </button>
                        );
                      })}
                    </div>
                  </div>
              )}

              <div className="form-group">
                <label className="form-label" htmlFor="alignment">
                  Alignment
                </label>
                <input
                    id="alignment"
                    className="pill-input sm"
                    type="text"
                    maxLength={50}
                    placeholder="Neutral Good"
                    value={alignment}
                    onChange={(e) => setAlignment(e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="desc">
                  Description / Notes
                </label>
                <textarea
                    id="desc"
                    className="pill-textarea"
                    placeholder="Appearance, backstory..."
                    value={description}
                    onChange={(e) => setDescription(e.target.value)}
                />
              </div>

              <div className="footer-actions">
                <button type="submit" className="pill-btn compact inverse" disabled={loading}>
                  {loading ? 'JOINING...' : 'JOIN PARTY'}
                </button>
                <button type="button" className="link-btn" onClick={() => void handleLeaveRoom()}>
                  LEAVE ROOM
                </button>
              </div>
            </form>
        )}

        {/* ==================== SCREEN 5: ACTIVE ROOM & CHARACTER SHEET ==================== */}
        {screen === 'room' && room && presets && (
            <section className="screen">
              <div className="top-bar">
                <h2 className="top-bar-title">{room.name}</h2>
                <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                  <span className="badge filled">{isDm ? 'DM' : 'PLAYER'}</span>
                  <span
                      className="badge"
                      title="Tap to copy code"
                      onClick={() => void copyCode(room.code)}
                  >
                {copiedFlag ? 'COPIED' : room.code}
              </span>
                </div>
              </div>

              {error && <div className="error-banner">{error}</div>}

              {/* DM Party Roster */}
              {isDm && (
                  <div style={{ marginBottom: 14 }}>
                    <div
                        style={{
                          display: 'flex',
                          justifyContent: 'space-between',
                          alignItems: 'center',
                          marginBottom: 8,
                        }}
                    >
                      <span className="form-label">Party Roster ({room.characters.length})</span>
                      <button
                          type="button"
                          className="badge"
                          onClick={() => void openCharacterCreator()}
                      >
                        + ADD HERO
                      </button>
                    </div>
                    <div className="chip-grid">
                      {room.characters.map((c) => (
                          <button
                              key={c.id}
                              type="button"
                              className={`chip ${activeChar?.id === c.id ? 'selected' : ''}`}
                              onClick={() => setSelectedCharId(c.id)}
                          >
                            {c.nameCharacter} ({c.hpCurrent}/{c.hpMax})
                          </button>
                      ))}
                    </div>
                  </div>
              )}

              {/* Empty State */}
              {!activeChar ? (
                  <div className="info-card" style={{ textAlign: 'center', padding: '28px 16px' }}>
                    <p className="title-lg" style={{ fontSize: 20, marginBottom: 8 }}>
                      NO HEROES YET
                    </p>
                    <p className="subtitle" style={{ marginBottom: 16 }}>
                      Share room password <strong>{room.code}</strong> with players.
                    </p>
                    <button
                        type="button"
                        className="pill-btn compact"
                        onClick={() => void openCharacterCreator()}
                    >
                      CREATE CHARACTER
                    </button>
                  </div>
              ) : (
                  <div>
                    {/* Character Header & Vitals */}
                    <div className="info-card">
                      <div className="info-card-header">
                        <h3 className="info-card-title">{activeChar.nameCharacter}</h3>
                        <span className="badge filled">
                    LVL {activeChar.level} • {activeChar.xp} XP
                  </span>
                      </div>
                      <p className="subtitle" style={{ fontSize: 13, marginBottom: 4 }}>
                        {[
                          activeChar.raceCharacter,
                          activeChar.classCharacter,
                          activeChar.backgroundCharacter,
                        ]
                            .filter(Boolean)
                            .join(' • ')}
                      </p>
                      <p className="subtitle" style={{ fontSize: 12 }}>
                        Player: {activeChar.namePlayer}
                        {activeChar.alignment ? ` • ${activeChar.alignment}` : ''}
                      </p>

                      <div className="vitals-grid" style={{ marginTop: 12 }}>
                        <div className="vital-box">
                          <div className="vital-label">HP</div>
                          <div className="vital-val">
                            {activeChar.hpCurrent}/{activeChar.hpMax}
                          </div>
                        </div>
                        <div className="vital-box">
                          <div className="vital-label">AC</div>
                          <div className="vital-val">{activeChar.armorClass}</div>
                        </div>
                        <div className="vital-box">
                          <div className="vital-label">INIT / SPD</div>
                          <div className="vital-val">
                            {formatMod(activeChar.initiativeBonus)} / {activeChar.effectiveSpeed}
                          </div>
                        </div>
                      </div>

                      {/* HP Steppers */}
                      <div className="stepper-row">
                        <span className="form-label">Current HP</span>
                        <div className="stepper-controls">
                          <button
                              type="button"
                              className="step-btn"
                              onClick={() => void patchCharacter({ hpCurrent: activeChar.hpCurrent - 5 })}
                          >
                            -5
                          </button>
                          <button
                              type="button"
                              className="step-btn"
                              onClick={() => void patchCharacter({ hpCurrent: activeChar.hpCurrent - 1 })}
                          >
                            -1
                          </button>
                          <button
                              type="button"
                              className="step-btn"
                              onClick={() => void patchCharacter({ hpCurrent: activeChar.hpCurrent + 1 })}
                          >
                            +1
                          </button>
                          <button
                              type="button"
                              className="step-btn"
                              onClick={() => void patchCharacter({ hpCurrent: activeChar.hpCurrent + 5 })}
                          >
                            +5
                          </button>
                        </div>
                      </div>

                      {/* Temp HP */}
                      <div className="stepper-row">
                  <span className="form-label">
                    Temp HP: <strong>{activeChar.tempHp}</strong>
                  </span>
                        <div className="stepper-controls">
                          <button
                              type="button"
                              className="step-btn"
                              onClick={() =>
                                  void patchCharacter({ tempHp: Math.max(0, activeChar.tempHp - 1) })
                              }
                          >
                            -1
                          </button>
                          <button
                              type="button"
                              className="step-btn"
                              onClick={() => void patchCharacter({ tempHp: activeChar.tempHp + 1 })}
                          >
                            +1
                          </button>
                        </div>
                      </div>

                      {/* Hit Dice */}
                      <div className="stepper-row">
                  <span className="form-label">
                    Hit Dice (d{activeChar.hitDieType}):{' '}
                    <strong>
                      {activeChar.hitDiceLeft}/{activeChar.hitDiceTotal}
                    </strong>
                  </span>
                        <div className="stepper-controls">
                          <button
                              type="button"
                              className="step-btn"
                              onClick={() =>
                                  void patchCharacter({
                                    hitDiceSpent: activeChar.hitDiceTotal - activeChar.hitDiceLeft + 1,
                                  })
                              }
                          >
                            USE
                          </button>
                          <button
                              type="button"
                              className="step-btn"
                              onClick={() =>
                                  void patchCharacter({
                                    hitDiceSpent: Math.max(
                                        0,
                                        activeChar.hitDiceTotal - activeChar.hitDiceLeft - 1
                                    ),
                                  })
                              }
                          >
                            +1
                          </button>
                        </div>
                      </div>

                      {/* Heroic Inspiration */}
                      <div className="stepper-row">
                        <span className="form-label">Heroic Inspiration</span>
                        <button
                            type="button"
                            className={`chip ${activeChar.heroicInspiration ? 'selected' : ''}`}
                            onClick={() =>
                                void patchCharacter({ heroicInspiration: !activeChar.heroicInspiration })
                            }
                        >
                          {activeChar.heroicInspiration ? 'ACTIVE' : 'OFF'}
                        </button>
                      </div>

                      {/* Death Saves when at 0 HP */}
                      {activeChar.hpCurrent === 0 && (
                          <div style={{ borderTop: '2px solid #000', marginTop: 10, paddingTop: 10 }}>
                            <div
                                className="stepper-row"
                                style={{ border: 'none', margin: 0, padding: '0 0 6px' }}
                            >
                      <span className="form-label">
                        Death Saves (Pass): <strong>{activeChar.deathSaveSuccesses}/3</strong>
                      </span>
                              <div className="stepper-controls">
                                <button
                                    type="button"
                                    className="step-btn"
                                    onClick={() =>
                                        void patchCharacter({
                                          deathSaveSuccesses: activeChar.deathSaveSuccesses - 1,
                                        })
                                    }
                                >
                                  -
                                </button>
                                <button
                                    type="button"
                                    className="step-btn"
                                    onClick={() =>
                                        void patchCharacter({
                                          deathSaveSuccesses: activeChar.deathSaveSuccesses + 1,
                                        })
                                    }
                                >
                                  +
                                </button>
                              </div>
                            </div>
                            <div className="stepper-row" style={{ border: 'none', margin: 0, padding: 0 }}>
                      <span className="form-label">
                        Death Saves (Fail): <strong>{activeChar.deathSaveFailures}/3</strong>
                      </span>
                              <div className="stepper-controls">
                                <button
                                    type="button"
                                    className="step-btn"
                                    onClick={() =>
                                        void patchCharacter({
                                          deathSaveFailures: activeChar.deathSaveFailures - 1,
                                        })
                                    }
                                >
                                  -
                                </button>
                                <button
                                    type="button"
                                    className="step-btn"
                                    onClick={() =>
                                        void patchCharacter({
                                          deathSaveFailures: activeChar.deathSaveFailures + 1,
                                        })
                                    }
                                >
                                  +
                                </button>
                              </div>
                            </div>
                          </div>
                      )}

                      {/* DM Controls: XP & Delete */}
                      {isDm && (
                          <div style={{ borderTop: '3px solid #000', marginTop: 12, paddingTop: 12 }}>
                            <div className="form-label" style={{ marginBottom: 6 }}>
                              DM Controls • Next Level: {activeChar.xpForNextLevel} XP
                            </div>
                            <div style={{ display: 'flex', gap: 8, marginBottom: 10 }}>
                              <input
                                  className="pill-input sm"
                                  type="number"
                                  placeholder="XP to add (e.g. 150)"
                                  value={xpInput}
                                  onChange={(e) => setXpInput(e.target.value)}
                              />
                              <button
                                  type="button"
                                  className="pill-btn compact"
                                  style={{ width: 'auto', minHeight: 48, fontSize: 15 }}
                                  onClick={() => {
                                    const val = parseInt(xpInput, 10);
                                    if (Number.isFinite(val) && val !== 0) {
                                      setXpInput('');
                                      void patchCharacter({ xpToAdd: val });
                                    }
                                  }}
                              >
                                + XP
                              </button>
                            </div>
                            <button
                                type="button"
                                className="pill-btn compact"
                                style={{ minHeight: 44, fontSize: 14 }}
                                onClick={() => void handleDeleteCharacter()}
                            >
                              REMOVE CHARACTER
                            </button>
                          </div>
                      )}
                    </div>

                    {/* Navigation Tabs */}
                    <div className="tabs">
                      <button
                          type="button"
                          className={`tab-btn ${activeTab === 'stats' ? 'active' : ''}`}
                          onClick={() => setActiveTab('stats')}
                      >
                        STATS &amp; SAVES
                      </button>
                      <button
                          type="button"
                          className={`tab-btn ${activeTab === 'skills' ? 'active' : ''}`}
                          onClick={() => setActiveTab('skills')}
                      >
                        SKILLS
                      </button>
                      <button
                          type="button"
                          className={`tab-btn ${activeTab === 'gear' ? 'active' : ''}`}
                          onClick={() => setActiveTab('gear')}
                      >
                        ARMOR &amp; INFO
                      </button>
                    </div>

                    {/* Tab 1: Stats & Saves */}
                    {activeTab === 'stats' && (
                        <div>
                          <div className="stats-grid">
                            {ABILITIES.map((ab) => {
                              const lower = ab.key.toLowerCase() as keyof CharacterResponse;
                              const modKey = `${lower}Mod` as keyof CharacterResponse;
                              const score = activeChar[lower] as number;
                              const mod = activeChar[modKey] as number;
                              return (
                                  <div key={ab.key} className="stat-box">
                                    <div className="stat-name">{ab.label}</div>
                                    <div className="stat-mod">{formatMod(mod)}</div>
                                    <div className="stat-score">{score}</div>
                                  </div>
                              );
                            })}
                          </div>

                          <div className="info-card">
                            <div className="form-label" style={{ marginBottom: 6 }}>
                              Saving Throws (Prof +{activeChar.proficiencyBonus})
                            </div>
                            {ABILITIES.map((ab) => {
                              const isProf = activeChar.saveProficiencies.includes(ab.key);
                              const bonus = activeChar.savingThrows[ab.key] ?? 0;
                              return (
                                  <div key={ab.key} className="list-row">
                          <span>
                            {isProf ? '● ' : '○ '}
                            {ab.full}
                          </span>
                                    <strong>{formatMod(bonus)}</strong>
                                  </div>
                              );
                            })}
                          </div>
                        </div>
                    )}

                    {/* Tab 2: Skills */}
                    {activeTab === 'skills' && (
                        <div className="info-card">
                          <div className="list-row" style={{ fontWeight: 700 }}>
                            <span>Passive Perception</span>
                            <span>{activeChar.passivePerception}</span>
                          </div>
                          <div style={{ marginTop: 6 }}>
                            {presets.skills.map((sk) => {
                              const isExp = activeChar.skillExpertise.includes(sk.id);
                              const isProf = activeChar.skillProficiencies.includes(sk.id);
                              const marker = isExp ? '◆ ' : isProf ? '● ' : '○ ';
                              const bonus = activeChar.skills[sk.id] ?? 0;
                              return (
                                  <div key={sk.id} className="list-row">
                          <span>
                            {marker}
                            {sk.name}
                          </span>
                                    <strong>{formatMod(bonus)}</strong>
                                  </div>
                              );
                            })}
                          </div>
                        </div>
                    )}

                    {/* Tab 3: Armor & Info */}
                    {activeTab === 'gear' && (
                        <div>
                          <div className="info-card">
                            <div className="form-group">
                              <label className="form-label" htmlFor="armor-select">
                                Equipped Armor
                              </label>
                              <select
                                  id="armor-select"
                                  className="pill-select"
                                  value={activeChar.armorId ?? ''}
                                  onChange={(e) => void patchCharacter({ armorId: e.target.value })}
                              >
                                <option value="">Unarmored</option>
                                {presets.armors.map((a) => (
                                    <option key={a.id} value={a.id}>
                                      {a.name} (AC {a.baseAc})
                                    </option>
                                ))}
                              </select>
                            </div>
                            <div className="stepper-row" style={{ border: 'none', padding: 0, margin: 0 }}>
                              <span className="form-label">Shield (+2 AC)</span>
                              <button
                                  type="button"
                                  className={`chip ${activeChar.hasShield ? 'selected' : ''}`}
                                  onClick={() => void patchCharacter({ hasShield: !activeChar.hasShield })}
                              >
                                {activeChar.hasShield ? 'EQUIPPED (+2)' : 'NONE'}
                              </button>
                            </div>
                          </div>

                          <div className="info-card" style={{ fontSize: 13, lineHeight: 1.6 }}>
                            <div className="form-label" style={{ marginBottom: 6 }}>
                              Traits &amp; Proficiencies
                            </div>
                            {activeChar.spellSaveDc !== null && (
                                <div>
                                  <strong>Spell Save DC:</strong> {activeChar.spellSaveDc} •{' '}
                                  <strong>Spell Attack:</strong>{' '}
                                  {formatMod(activeChar.spellAttackBonus ?? 0)}
                                </div>
                            )}
                            <div>
                              <strong>Carry Capacity:</strong> {activeChar.carryCapacityLb} lb
                            </div>
                            {activeChar.darkvision > 0 && (
                                <div>
                                  <strong>Darkvision:</strong> {activeChar.darkvision} ft
                                </div>
                            )}
                            {activeChar.feats.length > 0 && (
                                <div>
                                  <strong>Feats:</strong> {activeChar.feats.join(', ')}
                                </div>
                            )}
                            {activeChar.armorTraining.length > 0 && (
                                <div>
                                  <strong>Armor:</strong> {activeChar.armorTraining.join(', ')}
                                </div>
                            )}
                            {activeChar.weaponProficiencies.length > 0 && (
                                <div>
                                  <strong>Weapons:</strong> {activeChar.weaponProficiencies.join(', ')}
                                </div>
                            )}
                            {activeChar.toolProficiencies.length > 0 && (
                                <div>
                                  <strong>Tools:</strong> {activeChar.toolProficiencies.join(', ')}
                                </div>
                            )}
                            {activeChar.description && (
                                <div>
                                  <strong>Notes:</strong> {activeChar.description}
                                </div>
                            )}
                          </div>
                        </div>
                    )}
                  </div>
              )}

              <div className="footer-actions">
                <button
                    type="button"
                    className="pill-btn compact"
                    onClick={() => void handleLeaveRoom()}
                >
                  LEAVE ROOM
                </button>
              </div>
            </section>
        )}
      </div>
  );
}