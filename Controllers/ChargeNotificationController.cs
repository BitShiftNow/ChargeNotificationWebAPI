using Microsoft.AspNetCore.Mvc;
using WebAPI.Services.Work;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/notification")]
public class ChargeNotificationController : Controller {
    private readonly IWorkItemFactory work_item_factory;
    private readonly IWorkItemQueueWriter work_item_queue;
    private readonly IWorkItemStatusChecker work_item_checker;

    public ChargeNotificationController(IWorkItemFactory work_item_factory, IWorkItemQueueWriter work_item_queue, IWorkItemStatusChecker work_item_checker) {
        this.work_item_factory = work_item_factory;
        this.work_item_queue = work_item_queue;
        this.work_item_checker = work_item_checker;
    }

    [HttpPost("{id}/{date}")]
    public async Task<IActionResult> GenerateCustomerChargeNotification(long id, DateTime date, CancellationToken cancellationToken = default) {
        var work_item = work_item_factory.CustomerChargeNotification(id, date);
        var is_success = await work_item_queue.QueueWorkItemAsync(work_item, cancellationToken);

        return is_success ? Ok() : BadRequest("Charge notification service does not accept any requests at this time.");
    }

    [HttpPost("{date}")]
    public async Task<ActionResult<long>> GenerateAllChargeNotifications(DateTime date, CancellationToken cancellationToken = default) {
        var work_item = work_item_factory.AllChargeNotifications(date);
        var is_success = await work_item_queue.QueueWorkItemAsync(work_item, cancellationToken);

        return is_success ? work_item.Id : BadRequest("Charge notification service does not accept any requests at this time.");
    }

    [HttpGet("{id}")]
    public ActionResult<TimeSpan> GetChargeNotificationStatus(long id) {
        var is_complete = work_item_checker.IsWorkItemComplete(id, out var runtime);
        if (is_complete) {
            return runtime;
        } else {
            return NotFound();
        }
    }
}
