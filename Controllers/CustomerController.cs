using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Services.Data;
using WebAPI.Services.Data.Processors;

namespace WebAPI.Controllers;

/// <summary>
/// The controller to change customer data. This is for testing only really
/// </summary>
[ApiController]
[Route("api/customer")]
public class CustomerController : ControllerBase {
    private readonly ICustomerDataProvider provider;
    private readonly IDataProcessor<Customer?, CustomerDTO?> processor;

    public CustomerController(ICustomerDataProvider provider, IDataProcessor<Customer?, CustomerDTO?> processor) {
        this.provider = provider;
        this.processor = processor;
    }

    [HttpGet("{number}")]
    public async Task<ActionResult<CustomerDTO>> GetCustomerAsync(long number, CancellationToken cancellationToken = default) {
        var customer = await provider.GetSingleCustomerAsync(number, cancellationToken);
        var result = processor.Process(customer);

        if (result == null) {
            return NotFound();
        } else {
            return result;
        }
    }

    [HttpPost("{name}")]
    public async Task<ActionResult<CustomerDTO>> CreateNewCustomerAsync(string name, CancellationToken cancellationToken = default) {
        var customer = await provider.InsertNewCustomerAsync(name, cancellationToken);
        var result = processor.Process(customer);

        return CreatedAtAction(nameof(GetCustomerAsync), new { number = customer.Id }, result);
    }

    [HttpPost("create/{count}")]
    public async Task<IActionResult> CreateManyCustomersAsync(int count, CancellationToken cancellationToken = default) {
        await provider.InsertRandomCustomersAsync(count, cancellationToken);
        return Ok();
    }

    [HttpDelete("{number}")]
    public async Task<IActionResult> DeleteCustomerAsync(long number, CancellationToken cancellationToken = default) {
        var is_deleted = await provider.RemoveCustomerAsync(number, cancellationToken);
        
        if (is_deleted) {
            return Ok();
        } else {
            return NotFound();
        }
    }
}
