namespace BookApp.Data
{
    public interface IContentsXmlService
    {
        string ToXml(string html);

        string ToHtml(string xml);

        string ToPlainText(string html);
    }
}
