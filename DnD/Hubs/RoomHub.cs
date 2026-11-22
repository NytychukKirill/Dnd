using DnD.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DnD.Hubs;

public class RoomHub(AppDbContext db) : Hub
{
    public async Task JoinRoomGroup(string roomCode)
    {
        var code = Normalize(roomCode);

        // Группа создаётся только для существующей комнаты
        var exists = await db.Rooms.AsNoTracking().AnyAsync(r => r.Code == code);
        if (!exists)
            throw new HubException("Кімнату не знайдено.");

        await Groups.AddToGroupAsync(Context.ConnectionId, code);
    }

    public Task LeaveRoomGroup(string roomCode) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, Normalize(roomCode));

    // Группа называется кодом комнаты в верхнем регистре, как и в контроллерах (room.Code)
    private static string Normalize(string? code) => (code ?? string.Empty).Trim().ToUpperInvariant();
}