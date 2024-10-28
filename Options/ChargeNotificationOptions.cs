using System.ComponentModel.DataAnnotations;

namespace WebAPI.Options;

/// <summary>
/// Settings for the charge notification generation.
/// </summary>
public sealed class ChargeNotificationOptions {
    [Required]
    public required string DocumentOutputDirectory { get; set; }

    [Required]
    public required string DocumentTemplateFilename { get; set; }
}

public static class ChargeNotificationOptionsExtensions {
    public static IServiceCollection AddChargeNotificiationOptions(this IServiceCollection collection) {
        collection.AddOptions<ChargeNotificationOptions>()
            .Configure(options => {
                options.DocumentOutputDirectory = Directory.GetCurrentDirectory();
                options.DocumentTemplateFilename = Path.Combine(Directory.GetCurrentDirectory(), "template.xml");
            });
        return collection;
    }
}
