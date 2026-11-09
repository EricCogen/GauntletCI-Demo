using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Abstractions;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminPolicyController : ControllerBase
{
    private readonly ISystemSettingsService _settingsService;

    public AdminPolicyController(ISystemSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpPost("settings")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> UpdateSystemSettings(SystemSettingsRequest request, CancellationToken ct)
    {
        await _settingsService.UpdateSettingsAsync(request, ct);
        return Ok();
    }
}

public record SystemSettingsRequest(string SettingKey, string SettingValue);
