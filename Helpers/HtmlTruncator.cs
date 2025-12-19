    using HtmlAgilityPack;
    using System.Text;
    using System.Xml;

    namespace AnnouncmentHub.Helpers
    {
        public static class HtmlTruncator
        {
            public static string TruncateHtml(string html, int maxLength)
            {
                if (string.IsNullOrWhiteSpace(html))
                    return "";

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                int length = 0;
                var sb = new StringBuilder();

                foreach (var node in doc.DocumentNode.ChildNodes)
                {
                    if (!AppendNode(node, sb, ref length, maxLength))
                        break;
                }

                return sb.ToString() + (length >= maxLength ? "..." : "");
            }

            private static bool AppendNode(HtmlNode node, StringBuilder sb, ref int length, int maxLength)
            {
                if (length >= maxLength) return false;

                switch (node.NodeType)
                {
                    case HtmlNodeType.Text:
                        string text = node.InnerText;

                        if (length + text.Length > maxLength)
                            text = text.Substring(0, maxLength - length);

                        sb.Append(text);
                        length += text.Length;
                        return length < maxLength;

                    case HtmlNodeType.Element:
                        sb.Append($"<{node.Name}");

                        foreach (var attr in node.Attributes)
                            sb.Append($" {attr.Name}=\"{attr.Value}\"");

                        sb.Append(">");

                        foreach (var child in node.ChildNodes)
                        {
                            if (!AppendNode(child, sb, ref length, maxLength))
                                break;
                        }

                        sb.Append($"</{node.Name}>");
                        return length < maxLength;

                    default:
                        return true;
                }
            }
        }
    }


