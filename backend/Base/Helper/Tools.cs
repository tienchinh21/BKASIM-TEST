using System.Text.RegularExpressions;
using HtmlAgilityPack;
namespace MiniAppGIBA.Base.Helper
{
    public class Tools
    {
        public static string SummarizeHtmlContent(string html, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            string textContent = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText).Trim();
            textContent = Regex.Replace(textContent, @"\s+", " ");
            textContent = textContent.Replace("\\\"", "\"");
            if (textContent.Length <= maxLength)
            {
                return textContent;
            }
            int lastSpaceIndex = textContent.LastIndexOf(' ', maxLength);
            return lastSpaceIndex > 0 ? textContent.Substring(0, lastSpaceIndex) + "..." : textContent.Substring(0, maxLength) + "...";
        }



    }
}