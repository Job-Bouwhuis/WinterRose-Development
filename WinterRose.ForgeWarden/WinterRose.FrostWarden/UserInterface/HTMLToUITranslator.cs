using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface.Content;

namespace WinterRose.ForgeWarden.UserInterface;


public static partial class HtmlToUiTranslator
{
    // Public entry point. Produces a UIRows container representing the document.
    public static List<UIContent> TranslateDocument(string html)
    {
        var root = ParseHtmlToDom(html);

        return ConvertChildrenToContents(root.Children);
    }

    // --- Simple DOM representation ---
    private class DomNode
    {
        public string Tag;
        public string Text;
        public Dictionary<string, string> Attributes = new();
        public List<DomNode> Children = new();
        public bool IsText => Tag == null;

        public DomNode() { }
        public static DomNode TextNode(string text) => new DomNode { Tag = null, Text = text };
        public static DomNode Element(string tag) => new DomNode { Tag = tag.ToLowerInvariant() };
    }

    // Very small tokenizer + DOM builder. Handles nested tags and self-closing tags (img, br, hr).
    private static DomNode ParseHtmlToDom(string html)
    {
        if (string.IsNullOrEmpty(html)) return DomNode.Element("document");

        // normalize some common HTML constructs to keep parsing predictable
        html = html.Replace("\r", "").Replace("\n", "\n");

        var root = DomNode.Element("document");
        var stack = new Stack<DomNode>();
        stack.Push(root);

        var tagRegex = HtmlToUiTranslator.tagRegex();
        var attrRegex = HtmlToUiTranslator.attrRegex();

        foreach (Match m in tagRegex.Matches(html))
        {
            if (m.Groups[4].Success)
            {
                string txt = m.Groups[4].Value;
                if (txt == "\n")
                    continue;

                txt = System.Net.WebUtility.HtmlDecode(txt);
                var parent = stack.Peek();
                parent.Children.Add(DomNode.TextNode(txt));
                continue;
            }

            bool isClose = m.Groups[1].Success && m.Groups[1].Value == "/";
            string tag = m.Groups[2].Value.ToLowerInvariant();
            string rawAttrs = m.Groups[3].Value ?? string.Empty;

            // self-closing tags
            if (!isClose && (IsVoidElement(tag) || rawAttrs.TrimEnd().EndsWith("/")))
            {
                var el = DomNode.Element(tag);
                // parse attributes
                foreach (Match a in attrRegex.Matches(rawAttrs))
                {
                    string name = a.Groups[1].Success ? a.Groups[1].Value : (a.Groups[3].Success ? a.Groups[3].Value : a.Groups[5].Value);
                    string val = a.Groups[2].Success ? a.Groups[2].Value : (a.Groups[4].Success ? a.Groups[4].Value : a.Groups[6].Value);
                    el.Attributes[name.ToLowerInvariant()] = val;
                }
                stack.Peek().Children.Add(el);
                continue;
            }

            if (isClose)
            {
                // pop until matching tag found (robust against malformed html)
                var closing = tag;
                var popped = new List<DomNode>();
                while (stack.Count > 1)
                {
                    var top = stack.Pop();
                    if (top.Tag == closing)
                    {
                        break;
                    }
                    popped.Add(top);
                }
                continue;
            }

            // open tag
            var node = DomNode.Element(tag);
            foreach (Match a in attrRegex.Matches(rawAttrs))
            {
                string name = a.Groups[1].Success ? a.Groups[1].Value : (a.Groups[3].Success ? a.Groups[3].Value : a.Groups[5].Value);
                string val = a.Groups[2].Success ? a.Groups[2].Value : (a.Groups[4].Success ? a.Groups[4].Value : a.Groups[6].Value);
                node.Attributes[name.ToLowerInvariant()] = val;
            }
            stack.Peek().Children.Add(node);
            stack.Push(node);
        }

        return root;
    }

    private static bool IsVoidElement(string tag)
    {
        return tag switch
        {
            "img" or "br" or "hr" or "input" or "meta" or "link" => true,
            _ => false,
        };
    }

    // Convert a list of DOM nodes into a list of UIContent blocks
    private static List<UIContent> ConvertChildrenToContents(List<DomNode> nodes)
    {
        var result = new List<UIContent>();
        var inlineSb = new StringBuilder();

        bool IsInline(DomNode n)
        {
            if (n == null) return false;
            if (n.IsText) return true;
            if (n.Tag == "img") return false;
            return n.Tag == "span" || n.Tag == "strong" || n.Tag == "b" ||
                   n.Tag == "em" || n.Tag == "i" || n.Tag == "a";
        }

        void FlushInlineBuffer()
        {
            if (inlineSb.Length == 0) return;
            var txt = inlineSb.ToString().Trim();
            inlineSb.Clear();
            if (!string.IsNullOrEmpty(txt))
            {
                result.Add(new UIText(RichText.Parse(txt), UIFontSizePreset.Text) { AutoScaleText = true });
            }
        }

        foreach (var n in nodes)
        {
            if (n.Tag is null)
                continue;

            if (IsInline(n))
            {
                if (n.IsText)
                {
                    var s = NormalizeWhitespace(n.Text);
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        inlineSb.Append(s);
                    }
                    continue;
                }
                // inline element with children -> append its inner text (anchors/images handled later when not top-level)
                if (n.Tag == "a")
                {
                    // for inline anchors keep the \L[...] escape so the rich text parser can render it
                    var sb = new StringBuilder();
                    AppendNodeText(n, sb); // AppendNodeText will insert \L[...] for anchors
                    inlineSb.Append(sb.ToString());
                }
                else
                {
                    inlineSb.Append(InnerText(n));
                }
                continue;
            }

            // it's a block-level element: flush any accumulated inline text first
            FlushInlineBuffer();

            if (n.IsText)
            {
                // whitespace or stray text outside inline context
                var s = NormalizeWhitespace(n.Text);
                if (!string.IsNullOrWhiteSpace(s))
                    result.Add(new UIText(RichText.Parse(s), UIFontSizePreset.Text) { AutoScaleText = false });
                continue;
            }

            switch (n.Tag)
            {
                case "p":
                    result.Add(ConvertParagraph(n));
                    break;
                case "div":
                    result.AddRange(ConvertDiv(n));
                    break;
                case "img":
                    result.Add(ConvertImage(n));
                    break;
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    result.Add(ConvertHeader(n));
                    break;
                case "br":
                    result.Add(new UISpacer(8));
                    break;
                case "hr":
                    result.Add(new UISpacer(12));
                    break;
                case "ul":
                case "ol":
                    result.AddRange(ConvertList(n));
                    break;
                default:
                    // fallback: recurse into children and append results
                    result.AddRange(ConvertChildrenToContents(n.Children));
                    break;
            }
        }

        // flush trailing inline
        FlushInlineBuffer();
        return result;
    }

    private static UIText ConvertParagraph(DomNode node)
    {
        var rt = BuildRichTextFromNode(node);
        return new UIText(rt, UIFontSizePreset.Text) { AutoScaleText = true };
    }

    private static List<UIContent> ConvertDiv(DomNode node)
    {
        string classAttr = node.Attributes.TryGetValue("class", out var cval) ? cval : string.Empty;
        string styleAttr = node.Attributes.TryGetValue("style", out var sval) ? sval : string.Empty;

        bool isRow = classAttr.Contains("row") || styleAttr.Contains("flex-direction:row") || styleAttr.Contains("flex-row");
        bool isColumns = classAttr.Contains("columns") || classAttr.Contains("cols") || styleAttr.Contains("column");

        if (isRow || isColumns)
        {
            var cols = new UIColumns();
            var children = ConvertChildrenToContents(node.Children);
            // naive: distribute children across columns in round-robin based on ColumnCount
            int columnCount = Math.Max(1, cols.ColumnCount);
            int idx = 0;
            foreach (var child in children)
            {
                cols.AddToColumn(idx % columnCount, child);
                idx++;
            }
            return new List<UIContent> { cols };
        }

        // otherwise treat as vertical flow: return children as separate blocks
        return ConvertChildrenToContents(node.Children);
    }

    private static UIContent ConvertImage(DomNode node)
    {
        string src = node.Attributes.TryGetValue("src", out var s) ? s : string.Empty;
        if (string.IsNullOrWhiteSpace(src))
            return new UISpacer(4);

        return new AsyncImageContent(src);
    }

    private static UIText ConvertHeader(DomNode node)
    {
        var rt = BuildRichTextFromNode(node);
        UIFontSizePreset preset = UIFontSizePreset.Title;
        switch (node.Tag)
        {
            case "h1": preset = UIFontSizePreset.Title; break;
            case "h2": preset = UIFontSizePreset.Subtitle; break;
            case "h3": preset = UIFontSizePreset.Subtitle; break;
            case "h4": preset = UIFontSizePreset.Text; break;
            case "h5": preset = UIFontSizePreset.Text; break;
            case "h6": preset = UIFontSizePreset.Subtext; break;
        }
        return new UIText(rt, preset) { AutoScaleText = true };
    }

    private static List<UIContent> ConvertList(DomNode node)
    {
        var items = new List<UIContent>();
        foreach (var child in node.Children)
        {
            if (child.Tag == "li")
            {
                var bullet = BuildRichTextFromNode(child);
                // prepend bullet glyph
                var withBullet = new RichText(new List<RichElement> { new RichGlyph('\u2022', Color.White) }) { FontSize = bullet.FontSize };
                withBullet += new RichGlyph(' ', Color.White);
                withBullet += bullet;
                items.Add(new UIText(withBullet));
            }
            else
            {
                items.AddRange(ConvertChildrenToContents(new List<DomNode> { child }));
            }
        }
        return items;
    }

    private static UIText ConvertInlineToText(DomNode node)
    {
        var rt = BuildRichTextFromNode(node);
        return new UIText(rt, UIFontSizePreset.Text) { AutoScaleText = true };
    }

    // Build a RichText instance from a node and its children. Anchors are converted into \L[url|display] escapes.
    private static RichText BuildRichTextFromNode(DomNode node)
    {
        var sb = new StringBuilder();
        AppendNodeText(node, sb);
        return RichText.Parse(sb.ToString(), Color.White, 14);
    }

    private static void AppendNodeText(DomNode node, StringBuilder sb)
    {
        if (node.IsText)
        {
            sb.Append(NormalizeWhitespace(node.Text));
            return;
        }

        if (node.Tag == "a")
        {
            string href = node.Attributes.TryGetValue("href", out var h) ? h : string.Empty;
            string display = InnerText(node);
            if (string.IsNullOrEmpty(display)) display = href;
            // escape '|' in display
            display = display.Replace("|", " ");
            sb.Append($"\\L[{href}|{display}]");
            return;
        }

        if (node.Tag == "img")
        {
            // represent images in-line as sprites using \s[tag]
            string src = node.Attributes.TryGetValue("src", out var s) ? s : string.Empty;
            if (!string.IsNullOrEmpty(src))
            {
                sb.Append($"\\s[{src}]");
            }
            return;
        }

        // formatting tags
        if (node.Tag == "strong" || node.Tag == "b")
        {
            sb.Append("\n");
            foreach (var c in node.Children) AppendNodeText(c, sb);
            sb.Append("\n");
            return;
        }

        if (node.Tag == "em" || node.Tag == "i")
        {
            foreach (var c in node.Children) AppendNodeText(c, sb);
            return;
        }

        // default: recurse into children
        foreach (var c in node.Children) AppendNodeText(c, sb);
    }

    private static string InnerText(DomNode node)
    {
        var sb = new StringBuilder();
        void Walk(DomNode n)
        {
            if (n.IsText) { sb.Append(NormalizeWhitespace(n.Text)); return; }
            if (n.Tag == "img") { return; }
            foreach (var ch in n.Children) Walk(ch);
        }
        Walk(node);
        return sb.ToString().Trim();
    }

    private static string NormalizeWhitespace(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        // collapse whitespace sequences to single space but preserve newlines
        s = Regex.Replace(s, "[\t\f\r ]+", " ");
        s = Regex.Replace(s, "\n{2,}", "\n");
        return s;
    }

    [GeneratedRegex("<(\\/?)\\s*([a-zA-Z0-9]+)([^>]*)>|([^<]+)", RegexOptions.Compiled)]
    private static partial Regex tagRegex();
    [GeneratedRegex(@"([a-zA-Z0-9\-_:]+)\s*=\s*""([^""]*)""|([a-zA-Z0-9\-_:]+)\s*=\s*'([^']*)'|([a-zA-Z0-9\-_:]+)\s*=\s*([^\s>]+)", RegexOptions.Compiled)]
    private static partial Regex attrRegex();
}

