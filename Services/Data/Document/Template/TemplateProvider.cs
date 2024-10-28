using System.Xml.Serialization;
using WebAPI.Options;
using WebAPI.Services.Data.Document.Schema;

namespace WebAPI.Services.Data.Document.Template;

/// <summary>
/// A template provider/factory to get templates for the document generation.
/// </summary>
public interface ITemplateProvider {
    ChargeDocumentTemplate ReadChargeDocumentTemplate();
}

/// <summary>
/// The default <see cref="ITemplateProvider"/> implementation that creates a 
/// template from an XML file at the configured location.
/// See <see cref="ChargeNotificationOptions.DocumentTemplateFilename"/>.
/// </summary>
public class TemplateProvider : ITemplateProvider {
    private readonly ChargeNotificationOptions options;

    public TemplateProvider(IConfiguration configuration) {
        options = configuration.GetSection(nameof(ChargeNotificationOptions))
                    .Get<ChargeNotificationOptions>()!;
    }

    public ChargeDocumentTemplate ReadChargeDocumentTemplate() {
        var serializer = new XmlSerializer(typeof(ChargeDocumentTemplate));

        using var fs = new FileStream(options.DocumentTemplateFilename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan); // NOTE: Increase 4KiB buffer to something higher if templates turn out to be larger to avoid multiple file reads. Modern SSDs perform quite well with buffers in the megabytes range (2-8 MiB).
        var result = (ChargeDocumentTemplate?)serializer.Deserialize(fs);
        if (result is null) {
            result = new ChargeDocumentTemplate();
        }
        return result;
    }
}

public static class TemplateProviderExtensions {
    public static IServiceCollection AddTemplateProvider(this IServiceCollection collection) {
        collection.AddSingleton<ITemplateProvider, TemplateProvider>();
        return collection;
    }
}