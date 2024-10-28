using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Text;
using WebAPI.Models;
using WebAPI.Services.Data.Document.Schema;

namespace WebAPI.Services.Data.Document;

/// <summary>
/// The charge notificiation PDF document template.
/// To create an instance of this template use the <see cref="IOutputDocumentFactory"/>.
/// 
/// This was used as the templating engine before I aded the XML template file.
/// The idea was to provide different IDocument implementations if the template ever needed to change.
/// 
/// Now it relies on the <see cref="ChargeDocumentTemplate"/> class that describes in a very crude way on
/// how the document needs to look like.
/// </summary>
public class ChargeNotificationDocument : IDocument {
    private readonly ChargeNotification data;
    private readonly ChargeDocumentTemplate template;

    public ChargeNotificationDocument(ChargeDocumentTemplate template, ChargeNotification data) {
        this.data = data;
        this.template = template;
    }

    public void Compose(IDocumentContainer container) {
        container.Page(page => {
            page.Margin(50);

            if (template.Header is not null) {
                page.Header().Element(ComposeHeader);
            }
            if (template.Body is not null) {
                page.Content().Element(ComposeContent);
            }
        });
    }

    private void ComposeHeader(IContainer container) {
        container.Row(row => row.RelativeItem().Column(column => {
            foreach (var item in template.Header!.Items) {
                if (item is HorizontalLine line) {
                    column.Item().LineHorizontal(line.Thickness);
                } else if (item is Text text) {
                    column.Item().AddTextFromTemplate(text, data);
                }
            }
        }));
    }

    void ComposeContent(IContainer container)
        => container.PaddingVertical(40).Column(column => {
            column.Spacing(5); // These are not part of the template but could be ofc.

            foreach (var item in template.Body!.Items) {
                if (item is HorizontalLine line) {
                    column.Item().LineHorizontal(line.Thickness);
                } else if (item is Text text) {
                    column.Item().AddTextFromTemplate(text, data);
                } else if (item is ChargeTable template_table) {
                    column.Item().Table(table => {
                        table.ColumnsDefinition(columns => {
                            for (var i = 0; i < template_table.Heading.Length; i++) {
                                columns.RelativeColumn();
                            }
                        });

                        table.Header(header => {
                            foreach (var text in template_table.Heading) {
                                header.Cell().PaddingVertical(5).AddTextFromTemplate(text, data);
                            }
                        });

                        foreach (var row in data.Charges) {
                            foreach (var text in template_table.Cells) {
                                table.Cell().AddTextFromTemplate(text, data, row);
                            }
                        }
                    });
                }
            }
        });
}

/// <summary>
/// Helper methods for the fluent API to apply templated data to it.
/// </summary>
public static class ChargeNotificationDocumentExtensions {
    public static IContainer TemplateTextAlignment(this IContainer container, Text text)
        => text.Alignment switch {
            "Left" => container.AlignLeft(),
            "Right" => container.AlignRight(),
            "Center" => container.AlignCenter(),
            _ => container
        };

    public static TextStyle TemplateTextStyle(this TextStyle style, Text text) {
        // Apply style
        style = text.Style switch {
            "Normal" => style.NormalWeight(),
            "Bold" => style.Bold(),
            "Italic" => style.Italic(),
            _ => style,
        };

        // Apply Color and font size
        var hex_color = ((uint)text.Color.A << 24)
            | ((uint)text.Color.R << 16)
            | ((uint)text.Color.G << 8)
            | text.Color.B;
        return style.FontSize(text.Size)
            .FontColor(new Color(hex_color));
    }

    public static TextBlockDescriptor AddTextFromTemplate(this IContainer container, Text text, ChargeNotification data, ChargeNotificationRow? row = null) {
        var style = TextStyle.Default.TemplateTextStyle(text);
        var content = text.Value.ApplyTemplateData(data, row);
        return container.TemplateTextAlignment(text).Text(content).Style(style);
    }

    private static readonly string[] separator = new string[] { "{{", "}}" };

    /// <summary>
    /// A quick and dirty version of template replacement.
    /// This does not handle a lot of invalid cases well but it is enough as a proof of concept for this task.
    /// </summary>
    public static string ApplyTemplateData(this string value, ChargeNotification data, ChargeNotificationRow? row = null) {
        var sb = new StringBuilder(value.Length);
        var blocks = value.Split(separator, StringSplitOptions.None);
        foreach (var block in blocks!) {
            var temp = ReplaceKeyword(block, data);
            if (row != null) {
                temp = ReplaceRowKeyword(temp, row);
            }
            sb.Append(temp);
        }
        return sb.ToString();

        static string ReplaceKeyword(string keyword, ChargeNotification data)
            => keyword switch {
                "CUSTOMER_NAME" => data.Name,
                "CUSTOMER_NUMBER" => data.Number.ToString(),
                "CUSTOMER_TOTAL" => data.TotalCost.ToString(),
                _ => keyword!
            };

        static string ReplaceRowKeyword(string keyword, ChargeNotificationRow data)
            => keyword switch {
                "CHARGE_NAME" => data.Name,
                "CHARGE_DATE" => data.Date.ToShortDateString(),
                "CHARGE_COST" => data.Cost.ToString(),
                _ => keyword
            };
    }
}
