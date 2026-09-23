using System.Text.RegularExpressions;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace BookApp.Data
{
    public class ContentsXmlService : IContentsXmlService
    {
        private static readonly string[] RemovedTags = ["script", "iframe", "object", "embed", "noscript"];

        private static readonly HashSet<string> BlockTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "p", "div", "br", "li", "h1", "h2", "h3", "h4", "h5", "h6", "tr", "blockquote"
        };

        public string ToXml(string html)
        {
            var fragment = ToXhtmlFragment(html);
            var body = XElement.Parse("<body>" + fragment + "</body>", LoadOptions.PreserveWhitespace);
            return new XElement("toc", body).ToString(SaveOptions.DisableFormatting);
        }

        public string ToHtml(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                return string.Empty;
            }

            var document = XDocument.Parse(xml);
            var body = document.Root?.Element("body");
            if (body is null)
            {
                return string.Empty;
            }

            return NormalizeSpaces(string.Concat(body.Nodes().Select(node => node.ToString(SaveOptions.DisableFormatting))));
        }

        public string ToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var document = Load(html);
            foreach (var node in document.DocumentNode.Descendants().ToList())
            {
                if (BlockTags.Contains(node.Name) && node.ParentNode is not null)
                {
                    node.ParentNode.InsertAfter(document.CreateTextNode(" "), node);
                }
            }

            var text = HtmlEntity.DeEntitize(document.DocumentNode.InnerText);
            return Whitespace.Replace(text, " ").Trim();
        }

        private static string ToXhtmlFragment(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var document = Load(html);
            document.OptionOutputAsXml = true;
            var fragment = document.DocumentNode.InnerHtml.Trim();
            return NormalizeSpaces(fragment);
        }

        private static HtmlDocument Load(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            RemoveDangerousContent(document);
            return document;
        }

        private static void RemoveDangerousContent(HtmlDocument document)
        {
            foreach (var node in document.DocumentNode.Descendants()
                         .Where(node => RemovedTags.Contains(node.Name, StringComparer.OrdinalIgnoreCase))
                         .ToList())
            {
                node.Remove();
            }

            foreach (var node in document.DocumentNode.Descendants().ToList())
            {
                foreach (var attribute in node.Attributes.ToList())
                {
                    if (attribute.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase)
                        || IsJavaScriptUrl(attribute))
                    {
                        attribute.Remove();
                    }
                }
            }
        }

        private static bool IsJavaScriptUrl(HtmlAttribute attribute)
        {
            if (attribute.Name is not ("href" or "src"))
            {
                return false;
            }

            var value = attribute.Value ?? string.Empty;
            return value.Trim().StartsWith("javascript:", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeSpaces(string value)
        {
            return value
                .Replace("&amp;nbsp;", " ", StringComparison.OrdinalIgnoreCase)
                .Replace("&nbsp;", " ", StringComparison.OrdinalIgnoreCase)
                .Replace("\u00A0", " ");
        }

        private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);
    }
}
