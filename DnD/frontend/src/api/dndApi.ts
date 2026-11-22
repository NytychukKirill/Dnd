import type{
    CharacterCreatedResponse,
    CharacterResponse,
    CreateCharacterRequest,
    PresetsResponse,
    RoomCreatedResponse,
    RoomDetailsResponse,
    UpdateCharacterStatsRequest,
} from '../types/dnd';

export const tokenStorage = {
    getApiBase(): string {
        return localStorage.getItem('dnd_api_base') || 'http://localhost:5062';
    },
    setApiBase(url: string): void {
        localStorage.setItem('dnd_api_base', url.replace(/\/+$/, ''));
    },
    getDmToken(code: string): string | null {
        return localStorage.getItem(`dnd_dm_${code.toUpperCase()}`);
    },
    setDmToken(code: string, token: string): void {
        localStorage.setItem(`dnd_dm_${code.toUpperCase()}`, token);
    },
    getPlayerToken(code: string): string | null {
        return localStorage.getItem(`dnd_player_${code.toUpperCase()}`);
    },
    setPlayerToken(code: string, token: string, charId: string): void {
        const upper = code.toUpperCase();
        localStorage.setItem(`dnd_player_${upper}`, token);
        localStorage.setItem(`dnd_char_${upper}`, charId);
    },
    getSavedCharId(code: string): string | null {
        return localStorage.getItem(`dnd_char_${code.toUpperCase()}`);
    },
};

async function request<T>(
    path: string,
    options: RequestInit = {},
    roomCode?: string
): Promise<T> {
    const base = tokenStorage.getApiBase().replace(/\/+$/, '');
    const headers: Record<string, string> = {
        'Content-Type': 'application/json',
        ...(options.headers as Record<string, string>),
    };

    if (roomCode) {
        const dmToken = tokenStorage.getDmToken(roomCode);
        const playerToken = tokenStorage.getPlayerToken(roomCode);
        if (dmToken) headers['X-DM-Token'] = dmToken;
        if (playerToken) headers['X-Player-Token'] = playerToken;
    }

    const res = await fetch(`${base}${path}`, { ...options, headers });
    if (res.status === 204) return undefined as T;

    let data: unknown = null;
    try {
        data = await res.json();
    } catch {
        data = null;
    }

    if (!res.ok) {
        const errObj = data as { error?: string; title?: string } | null;
        throw new Error(errObj?.error || errObj?.title || `HTTP ${res.status}`);
    }

    return data as T;
}

export const dndApi = {
    getPresets(): Promise<PresetsResponse> {
        return request<PresetsResponse>('/api/presets');
    },

    createRoom(name: string): Promise<RoomCreatedResponse> {
        return request<RoomCreatedResponse>('/api/rooms', {
            method: 'POST',
            body: JSON.stringify({ name }),
        });
    },

    getRoom(code: string): Promise<RoomDetailsResponse> {
        const normalized = code.trim().toUpperCase();
        return request<RoomDetailsResponse>(
            `/api/rooms/${encodeURIComponent(normalized)}`,
            { method: 'GET' },
            normalized
        );
    },

    createCharacter(
        code: string,
        payload: CreateCharacterRequest
    ): Promise<CharacterCreatedResponse> {
        const normalized = code.trim().toUpperCase();
        return request<CharacterCreatedResponse>(
            `/api/rooms/${encodeURIComponent(normalized)}/characters`,
            {
                method: 'POST',
                body: JSON.stringify(payload),
            },
            normalized
        );
    },

    updateCharacter(
        roomCode: string,
        id: string,
        payload: UpdateCharacterStatsRequest
    ): Promise<CharacterResponse> {
        return request<CharacterResponse>(
            `/api/characters/${id}`,
            {
                method: 'PATCH',
                body: JSON.stringify(payload),
            },
            roomCode
        );
    },

    deleteCharacter(roomCode: string, id: string): Promise<void> {
        return request<void>(
            `/api/characters/${id}`,
            { method: 'DELETE' },
            roomCode
        );
    },
};