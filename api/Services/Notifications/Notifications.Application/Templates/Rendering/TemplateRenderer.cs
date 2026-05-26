using System.Text;

namespace Notifications.Application.Templates.Rendering;

public static class TemplateRenderer
{
    public static string Render(string template, IReadOnlyDictionary<string, string?> values)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        var result = new StringBuilder(template);
        foreach (var (key, value) in values)
        {
            result.Replace("{" + key + "}", value ?? string.Empty);
        }

        return result.ToString();
    }
}
