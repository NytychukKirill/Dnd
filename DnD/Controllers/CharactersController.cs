using DnD.Data;
using DnD.DTOs;
using DnD.Hubs;
using DnD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DnD.Controllers;

[ApiController]
[Route("api")]
public class CharactersController(AppDbContext db, IHubContext<RoomHub> hubContext) : ControllerBase
{
    [HttpPost("rooms/{code}/characters")]
    public async Task<ActionResult<CharacterCreatedResponse>> CreateCharacter(
        string code,
        [FromBody] CreateCharacterRequest request)
    {
        var namePlayer = request.NamePlayer?.Trim();
        var nameCharacter = request.NameCharacter?.Trim();

        if (string.IsNullOrWhiteSpace(namePlayer) || string.IsNullOrWhiteSpace(nameCharacter))
            return BadRequest(new { error = "Ім'я гравця та ім'я персонажа обов'язкові." });

        if (namePlayer.Length > 100 || nameCharacter.Length > 100)
            return BadRequest(new { error = "Імена не можуть бути довшими за 100 символів." });

        var normalizedCode = code.Trim().ToUpperInvariant();
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Code == normalizedCode);

        if (room is null)
            return NotFound(new { error = "Кімнату не знайдено." });

        var character = new Character
        {
            RoomId = room.Id,
            NamePlayer = namePlayer,
            NameCharacter = nameCharacter,
            Description = request.Description?.Trim() ?? string.Empty,
            Alignment = request.Alignment?.Trim() ?? string.Empty
        };

        try
        {
            // Правила 2024: клас + передісторія (бонуси +2/+1 або +1/+1/+1) + вид, навички класу
            character.ApplyPresets(
                request.RaceId,
                request.ClassId,
                request.BackgroundId,
                request.PlusTwo,
                request.PlusOne,
                request.ClassSkills
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        db.Characters.Add(character);
        await db.SaveChangesAsync();

        var responseDto = CharacterResponse.FromEntity(character);

        // Повідомляємо майстра та інших гравців у кімнаті про нового персонажа
        await hubContext.Clients.Group(room.Code).SendAsync("CharacterAdded", responseDto);

        return Ok(new CharacterCreatedResponse(responseDto, character.PlayerToken));
    }

    [HttpPatch("characters/{id:guid}")]
    public async Task<ActionResult<CharacterResponse>> UpdateCharacter(
        Guid id,
        [FromBody] UpdateCharacterStatsRequest request,
        [FromHeader(Name = "X-Player-Token")] Guid? playerToken,
        [FromHeader(Name = "X-DM-Token")] Guid? dmToken)
    {
        var character = await db.Characters
            .Include(c => c.Room)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (character is null)
            return NotFound(new { error = "Персонажа не знайдено." });

        var isOwner = playerToken.HasValue && character.PlayerToken == playerToken.Value;
        var isDm = dmToken.HasValue && character.Room.DmToken == dmToken.Value;

        if (!isOwner && !isDm)
            return Unauthorized(new { error = "Немає прав на редагування цієї анкети." });

        // Досвід видає лише Майстер
        if (request.XpToAdd.HasValue && !isDm)
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Досвід нараховує лише Майстер." });

        // Перевірка обладунку по таблиці з книги
        string? newArmorId = null;
        var armorChanged = false;
        if (request.ArmorId is not null)
        {
            armorChanged = true;
            if (!string.IsNullOrWhiteSpace(request.ArmorId))
            {
                var armor = DndPresets.GetArmor(request.ArmorId.Trim());
                if (armor is null)
                    return BadRequest(new { error = $"Невідомий обладунок: {request.ArmorId}" });
                newArmorId = armor.Id;
            }
        }

        // 1) Спочатку все, що змінює максимум ХП та КЗ (досвід/рівень, обладунок, щит)
        if (armorChanged)
            character.ArmorId = newArmorId;

        if (request.HasShield.HasValue)
            character.HasShield = request.HasShield.Value;

        if (request.XpToAdd.HasValue)
            character.AddXp(request.XpToAdd.Value);   // всередині перераховує HpMax і КЗ
        else
            character.Recalculate();

        // 2) Далі поточні значення, які обмежуються вже оновленим максимумом
        if (request.HpCurrent.HasValue)
        {
            character.HpCurrent = Math.Clamp(request.HpCurrent.Value, 0, character.HpMax);

            // Коли персонаж знову при свідомості, спроби порятунку від смерті скидаються
            if (character.HpCurrent > 0)
            {
                character.DeathSaveSuccesses = 0;
                character.DeathSaveFailures = 0;
            }
        }

        if (request.TempHp.HasValue)
            character.TempHp = Math.Max(0, request.TempHp.Value);

        if (request.HitDiceSpent.HasValue)
            character.HitDiceSpent = Math.Clamp(request.HitDiceSpent.Value, 0, character.HitDiceTotal);

        if (request.DeathSaveSuccesses.HasValue)
            character.DeathSaveSuccesses = Math.Clamp(request.DeathSaveSuccesses.Value, 0, 3);

        if (request.DeathSaveFailures.HasValue)
            character.DeathSaveFailures = Math.Clamp(request.DeathSaveFailures.Value, 0, 3);

        if (request.HeroicInspiration.HasValue)
            character.HeroicInspiration = request.HeroicInspiration.Value;

        if (request.Description is not null)
            character.Description = request.Description.Trim();

        await db.SaveChangesAsync();

        var responseDto = CharacterResponse.FromEntity(character);

        // Оновлюємо дашборд майстра в реальному часі
        await hubContext.Clients.Group(character.Room.Code).SendAsync("CharacterUpdated", responseDto);

        return Ok(responseDto);
    }

    [HttpDelete("characters/{id:guid}")]
    public async Task<IActionResult> DeleteCharacter(
        Guid id,
        [FromHeader(Name = "X-DM-Token")] Guid? dmToken)
    {
        var character = await db.Characters
            .Include(c => c.Room)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (character is null)
            return NotFound();

        if (!dmToken.HasValue || character.Room.DmToken != dmToken.Value)
            return Unauthorized(new { error = "Видаляти персонажів з кімнати може лише Майстер." });

        var roomCode = character.Room.Code;
        db.Characters.Remove(character);
        await db.SaveChangesAsync();

        await hubContext.Clients.Group(roomCode).SendAsync("CharacterRemoved", id);

        return NoContent();
    }
}
