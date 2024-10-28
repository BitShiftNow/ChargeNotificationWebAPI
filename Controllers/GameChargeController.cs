using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Services.Data;
using WebAPI.Services.Data.Processors;

namespace WebAPI.Controllers;

/// <summary>
/// The controller to change charge data. This is for testing only really
/// </summary>
[ApiController]
[Route("api/charge")]
public class GameChargeController : Controller {
    private readonly ICustomerDataProvider provider;
    private readonly IDataProcessor<GameCharge?, GameChargeDTO?> processor;

    public GameChargeController(ICustomerDataProvider provider, IDataProcessor<GameCharge?, GameChargeDTO?> processor) {
        this.provider = provider;
        this.processor = processor;
    }

    [HttpGet("{number}")]
    public async Task<ActionResult<GameChargeDTO>> GetGameChargeAsync(long number, CancellationToken cancellationToken = default) {
        var charge = await provider.GetSingleGameChargeAsync(number, cancellationToken);
        var result = processor.Process(charge);

        if (result is not null) {
            return result;
        } else {
            return NotFound();
        }
    }

    [HttpGet("all/{number}/{date}")]
    public async Task<ActionResult<IEnumerable<GameChargeDTO>>> GetGameChargesForCustomerAsync(long number, DateTime date, CancellationToken cancellationToken = default) {
        var customer = await provider.GetCustomerChargeNotificationAsync(number, date, cancellationToken);
        if (customer is null) {
            return NotFound();
        }

        if (customer.GameCharges.Count > 0) {
            return customer.GameCharges.Select(x => processor.Process(x)!).ToList();
        }

        return new List<GameChargeDTO>();
    }

    [HttpPost("create/{count}/{date}")]
    public async Task<IActionResult> InsertRandomChargesForAllCustomersAsync(int count, DateTime date, CancellationToken cancellationToken = default) {
        await provider.InsertRandomCustomerChargesAsync(count, date, cancellationToken);
        return Ok();
    }

    [HttpDelete("{number}")]
    public async Task<IActionResult> DeleteChargeAsync(long number, CancellationToken cancellationToken = default) {
        var is_deleted = await provider.RemoveGameChargeAsync(number, cancellationToken);

        if (is_deleted) {
            return Ok();
        } else {
            return NotFound();
        }
    }
}
