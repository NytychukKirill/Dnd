using DnD.Data;
using DnD.DTOs;
using DnD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DnD.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController(AppDbContext db) : ControllerBase
{
    private const int MaxCodeAttempts = 5;

    [HttpPost]
    public async Task<ActionResult<RoomCreatedResponse>> CreateRoom([FromBody] CreateRoomRequest request)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Назва кімнати обов'язкова." });

        if (name.Length > 100)
            return BadRequest(new { error = "Назва кімнати не може бути довшою за 100 символів." });

        // Код випадковий; унікальність гарантує індекс у БД, тому при колізії просто пробуємо ще раз
        for (var attempt = 0; attempt < MaxCodeAttempts; attempt++)
        {
            var room = new Room { Name = name };
            db.Rooms.Add(room);

            try
            {
                await db.SaveChangesAsync();
                return Ok(new RoomCreatedResponse(room.Id, room.Code, room.Name, room.DmToken));
            }
            catch (DbUpdateException)
            {
                db.Entry(room).State = EntityState.Detached;
            }
        }

        return StatusCode(500, new { error = "Не вдалося згенерувати унікальний код кімнати. Спробуйте ще раз." });
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<RoomDetailsResponse>> GetRoom(
        string code,
        [FromHeader(Name = "X-DM-Token")] Guid? dmToken)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var room = await db.Rooms
            .AsNoTracking()
            .Include(r => r.Characters)
            .FirstOrDefaultAsync(r => r.Code == normalizedCode);

        if (room is null)
            return NotFound(new { error = "Кімнату з таким кодом не знайдено." });

        var isDm = dmToken.HasValue && room.DmToken == dmToken.Value;

        var characters = room.Characters
            .OrderBy(c => c.NameCharacter)
            .Select(CharacterResponse.FromEntity)
            .ToList();

        return Ok(new RoomDetailsResponse(room.Id, room.Code, room.Name, isDm, characters));
    }
}
