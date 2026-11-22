using DnD.Data;
using Microsoft.AspNetCore.Mvc;

namespace DnD.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PresetsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllPresets()
    {
        return Ok(new
        {
            races = DndPresets.Races,
            classes = DndPresets.Classes,
            backgrounds = DndPresets.Backgrounds,
            skills = DndPresets.Skills,
            armors = DndPresets.Armors,
            shieldAcBonus = DndPresets.ShieldAcBonus,
            xpThresholds = DndPresets.XpThresholds
        });
    }
}