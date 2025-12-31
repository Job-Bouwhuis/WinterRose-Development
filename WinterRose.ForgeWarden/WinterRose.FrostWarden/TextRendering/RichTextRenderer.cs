namespace WinterRose.ForgeWarden.TextRendering;

using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes.Boxes;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

public static class RichTextRenderer
{
    private static readonly Dictionary<string, Vector2> MEASURE_CACHE = new();
    private static readonly Dictionary<string, Vector2> SPRITE_SIZE_CACHE = new();

    public static void DrawRichText(string text, Vector2 position, float maxWidth, InputContext input)
        => DrawRichText(RichText.Parse(text, Color.White), position, maxWidth, input);

    public static void DrawRichText(RichText richText, Vector2 position, float maxWidth, InputContext input)
        => DrawRichText(richText, position, maxWidth, Color.White, input);

    private static string GetMeasureKey(Font font, string s, float fontSize, float spacing)
    => $"{font.GetHashCode()}|{fontSize}|{spacing}|{s}";

    private static Vector2 MeasureTextExCached(Font font, string s, float fontSize, float spacing)
    {
        string key = GetMeasureKey(font, s, fontSize, spacing);
        if (MEASURE_CACHE.TryGetValue(key, out var cached))
            return cached;

        var measured = Raylib.MeasureTextEx(font, s, fontSize, spacing);
        MEASURE_CACHE[key] = measured;
        return measured;
    }

    public static void DrawRichText(RichText richText, Vector2 position, float maxWidth, Color overallTint, InputContext input)
    {
        var lines = WrapText(richText, maxWidth);

        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            float x = position.X;
            float y = position.Y + lineIndex * (richText.FontSize + richText.Spacing);

            // collect link hitboxes for this line and process clicks after drawing
            var linkHitboxes = new List<(string Url, Rectangle Rect, Color Tint)>();
            var line = lines[lineIndex];

            int i = 0;
            while (i < line.Count)
            {
                // handle glyph runs
                if (line[i] is RichGlyph)
                {
                    var runSb = new StringBuilder();
                    Color runTint = default;
                    string runUrl = null;
                    bool runTintInit = false;

                    int j = i;
                    for (; j < line.Count; j++)
                    {
                        if (line[j] is not RichGlyph g) break;

                        // compute tinted color for this glyph
                        Color glyphTint = new(
                            (byte)(g.Color.R * overallTint.R / 255),
                            (byte)(g.Color.G * overallTint.G / 255),
                            (byte)(g.Color.B * overallTint.B / 255),
                            (byte)(g.Color.A * overallTint.A / 255)
                        );

                        if (!runTintInit)
                        {
                            runTint = glyphTint;
                            runUrl = g.GlyphLinkUrl;
                            runTintInit = true;
                        }
                        else
                        {
                            // break run if tint or url differ
                            if (glyphTint.R != runTint.R || glyphTint.G != runTint.G || glyphTint.B != runTint.B || glyphTint.A != runTint.A
                                || (g.GlyphLinkUrl != runUrl))
                            {
                                break;
                            }
                        }

                        runSb.Append(g.Character);
                    }

                    string runText = runSb.ToString();
                    if (runText.Length > 0)
                    {
                        var runSize = MeasureTextExCached(richText.Font, runText, richText.FontSize, richText.Spacing);
                        Raylib.DrawTextEx(richText.Font, runText, new Vector2(x, y), richText.FontSize, richText.Spacing, runTint);

                        if (runUrl is not null)
                        {
                            // underline whole run
                            Raylib.DrawLineEx(new Vector2(x, y + runSize.Y + 2), new Vector2(x + runSize.X, y + runSize.Y + 2), 1, runTint);
                            linkHitboxes.Add((runUrl, new Rectangle((int)x, (int)y, (int)runSize.X, (int)runSize.Y), runTint));
                        }

                        if (j < line.Count)
                            x += runSize.X + richText.Spacing;
                        else
                            x += runSize.X;
                    }

                    i = j;
                    continue;
                }

                // handle sprites
                if (line[i] is RichSprite sprite)
                {
                    var texture = RichSpriteRegistry.GetSprite(sprite.SpriteKey);
                    if (texture is not null)
                    {
                        float spriteHeight = sprite.BaseSize * richText.FontSize;
                        float scale = spriteHeight / texture.Height;

                        Color tintedSpriteColor = new Color(
                            (byte)(sprite.Tint.R * overallTint.R / 255),
                            (byte)(sprite.Tint.G * overallTint.G / 255),
                            (byte)(sprite.Tint.B * overallTint.B / 255),
                            (byte)(sprite.Tint.A * overallTint.A / 255)
                        );

                        Raylib.DrawTextureEx(texture, new Vector2(x, y), 0, scale, tintedSpriteColor);

                        if (sprite.Clickable && input != null)
                        {
                            var imageRect = new Rectangle((int)x, (int)y, (int)(texture.Width * scale), (int)(texture.Height * scale));
                            
                            if (ray.CheckCollisionPointRec(input.MousePosition, imageRect) && input.IsDown(MouseButton.Left))
                            {
                                Toasts.Error("Sprite dialog is temporarily out of order");
                            }
                        }

                        if (i + 1 < line.Count)
                            x += texture.Width * scale + richText.Spacing;
                        else
                            x += texture.Width * scale;
                    }

                    i++;
                    continue;
                }

                if (line[i] is RichWord word)
                {
                    Color tinted = new Color(
                        (byte)(word.Color.R * overallTint.R / 255),
                        (byte)(word.Color.G * overallTint.G / 255),
                        (byte)(word.Color.B * overallTint.B / 255),
                        (byte)(word.Color.A * overallTint.A / 255)
                    );

                    var runSize = MeasureTextExCached(richText.Font, word.Text, richText.FontSize, richText.Spacing);
                    Raylib.DrawTextEx(richText.Font, word.Text, new Vector2(x, y), richText.FontSize, richText.Spacing, tinted);

                    if (!string.IsNullOrEmpty(word.LinkUrl))
                    {
                        Raylib.DrawLineEx(new Vector2(x, y + runSize.Y + 2), new Vector2(x + runSize.X, y + runSize.Y + 2), 1, tinted);
                        linkHitboxes.Add((word.LinkUrl, new Rectangle((int)x, (int)y, (int)runSize.X, (int)runSize.Y), tinted));
                    }

                    if (i < line.Count)
                        x += runSize.X + richText.Spacing;
                    else
                        x += runSize.X;
                    i++;
                    continue;
                }

                i++;
            }

            // process link clicks for this line (one check per run)
            foreach (var (Url, Rect, Tint) in linkHitboxes)
            {
                if (Url is not null && ray.CheckCollisionPointRec(input.MousePosition, Rect) && ray.IsMouseButtonPressed(MouseButton.Left))
                {
                    Dialogs.Show(new BrowserDialog(Url, DialogPlacement.CenterBig, DialogPriority.EngineNotifications));
                }
            }
        }
    }

    public static Rectangle MeasureRichText(RichText richText, float maxWidth)
    {
        // Use existing WrapText and CalculateElementsSize (cached) to compute bounds
        var lines = WrapText(richText, maxWidth);

        float width = 0f;
        for (int i = 0; i < lines.Count; i++)
        {
            var v = CalculateElementsSize(richText, lines[i]);
            if (v.X > width) width = v.X;
        }

        // Keep same line-height semantics as DrawRichText: line step = FontSize + Spacing
        float height = lines.Count * (richText.FontSize + richText.Spacing);

        return new Rectangle(0, 0, (int)Math.Ceiling(width), (int)Math.Ceiling(height));
    }

    public static Rectangle MeasureElements(RichText richText, List<RichElement> elements)
    {
        var v = CalculateElementsSize(richText, elements);
        return new Rectangle(0, 0, (int)Math.Ceiling(v.X), (int)Math.Ceiling(v.Y));
    }

    internal static List<List<RichElement>> WrapText(RichText text, float maxWidth)
    {
        var elements = text.Elements;
        var lines = new List<List<RichElement>>();
        var currentLine = new List<RichElement>();
        var currentWord = new List<RichElement>();
        float currentLineWidth = 0f;
        var spaceSize = MeasureTextExCached(text.Font, " ", text.FontSize, text.Spacing).X;

        static float MeasureElementsWidth(RichText t, List<RichElement> elems)
        {
            if (elems == null || elems.Count == 0) return 0f;
            var v = CalculateElementsSize(t, elems);
            return v.X;
        }

        void FlushWordIntoLine()
        {
            if (currentWord.Count == 0) return;
            // Use the append function so splitting/abbrev rules are respected
            AppendWordWithWrapping(text, maxWidth, ref currentLine, ref currentLineWidth, ref currentWord, spaceSize, lines);
        }

        for (int ei = 0; ei < elements.Count; ei++)
        {
            var element = elements[ei];

            // explicit newline -> flush and force new line (do NOT keep the newline glyph in the resulting line)
            if (element is RichGlyph gg && gg.Character == '\n')
            {
                FlushWordIntoLine();
                // finalize current line (even if empty, preserve blank line)
                lines.Add(new List<RichElement>(currentLine));
                currentLine.Clear();
                currentLineWidth = 0f;
                currentWord.Clear();
                continue;
            }

            // space glyph handling -> completes a word (spaces are preserved as elements)
            // space glyph handling -> completes a word (spaces are preserved as elements)
            if (element is RichGlyph g && g.Character == ' ')
            {
                float wordWidth = MeasureElementsWidth(text, currentWord);

                // would word + optional preceding space overflow?
                float required = (currentLine.Count > 0 ? spaceSize : 0f) + wordWidth;
                if (currentLineWidth + required > maxWidth && currentLine.Count > 0)
                {
                    // push current line and start new
                    lines.Add(new List<RichElement>(currentLine));
                    currentLine.Clear();
                    currentLineWidth = 0f;
                }

                // append currentWord (may be empty)
                if (currentWord.Count > 0)
                {
                    // if line has content already, account for a space separator before this word
                    if (currentLine.Count > 0)
                        currentLineWidth += spaceSize;

                    currentLine.AddRange(currentWord);
                    currentLineWidth += wordWidth;
                    currentWord.Clear();
                }

                // now append the actual space glyph (we keep original semantics)
                // If adding the space would overflow, move it to next line instead (rare but possible)
                if (currentLineWidth + spaceSize > maxWidth && currentLine.Count > 0)
                {
                    lines.Add(new List<RichElement>(currentLine));
                    currentLine.Clear();
                    currentLineWidth = 0f;
                }

                // If the line is empty after possible wrapping, skip appending the space glyph entirely.
                if (currentLine.Count > 0)
                {
                    currentLine.Add(element);
                    currentLineWidth += spaceSize;
                }

                continue;
            }

            // accumulation into a word (words can contain glyphs, words, sprites)
            currentWord.Add(element);

            // If this word ends here (end of input or next is space/newline), flush it with wrapping logic
            bool isEnd = (ei == elements.Count - 1);
            bool nextIsSpaceOrNewline = !isEnd && (elements[ei + 1] is RichGlyph ng && (ng.Character == ' ' || ng.Character == '\n'));

            if (isEnd || nextIsSpaceOrNewline)
            {
                FlushWordIntoLine();
            }
        }

        // flush any trailing word
        if (currentWord.Count > 0)
        {
            AppendWordWithWrapping(text, maxWidth, ref currentLine, ref currentLineWidth, ref currentWord, spaceSize, lines);
        }

        // flush last line
        //if (currentLine.Count > 0 || lines.Count == 0) // ensure at least one line exists
            lines.Add(new List<RichElement>(currentLine));

        return lines;
    }

    private static void AppendWordWithWrapping(
        RichText text,
        float maxWidth,
        ref List<RichElement> currentLine,
        ref float currentLineWidth,
        ref List<RichElement> currentWord,
        float spaceSize,
        List<List<RichElement>> lines)
    {
        if (currentWord == null || currentWord.Count == 0) return;

        float wordWidth = CalculateElementsSize(text, currentWord).X;
        float availableForWord = maxWidth - currentLineWidth - (currentLine.Count > 0 ? spaceSize : 0f);

        // If the word fits on the current line (or fits on a fresh line), append whole word
        if (wordWidth <= availableForWord || (currentLine.Count == 0 && wordWidth <= maxWidth))
        {
            // if it doesn't fit on this line but fits on an empty line, push current and start new
            if (currentLine.Count > 0 && currentLineWidth + spaceSize + wordWidth > maxWidth)
            {
                lines.Add(new List<RichElement>(currentLine));
                currentLine.Clear();
                currentLineWidth = 0f;
            }

            // account for separating space if needed
            if (currentLine.Count > 0)
                currentLineWidth += spaceSize;

            currentLine.AddRange(currentWord);
            currentLineWidth += wordWidth;
            currentWord.Clear();
            return;
        }

        //
        // Prefer to wrap at the previous boundary (space/newline/tab) instead of splitting this word.
        //
        if (currentLine.Count > 0)
        {
            // push the current line (this performs the "wrap at space" behaviour)
            lines.Add(new List<RichElement>(currentLine));
            currentLine.Clear();
            currentLineWidth = 0f;

            // recalc available for a fresh line
            availableForWord = maxWidth;

            // if the whole word fits on the empty line, place it and return
            if (wordWidth <= availableForWord)
            {
                currentLine.AddRange(currentWord);
                currentLineWidth += wordWidth;
                currentWord.Clear();
                return;
            }

            // otherwise continue — now we're on an empty line and the word STILL doesn't fit, so mid-word logic is allowed
        }

        // Word does not fit as a whole on an empty line -> apply safe strategies

        // 1) Numeric abbreviation if the token is numeric (we prefer abbreviations over splitting numbers)
        if (IsNumericElementSequence(currentWord))
        {
            float target = (currentLine.Count > 0 ? availableForWord : maxWidth);
            var abbrev = AbbreviateNumericElementsToFit(text, currentWord, target);
            if (abbrev != null && abbrev.Count > 0)
            {
                // move to new line if needed (currentLine is empty in normal flow, but keep check)
                if (currentLine.Count > 0 && currentLineWidth + spaceSize + CalculateElementsSize(text, abbrev).X > maxWidth)
                {
                    lines.Add(new List<RichElement>(currentLine));
                    currentLine.Clear();
                    currentLineWidth = 0f;
                }

                if (currentLine.Count > 0)
                    currentLineWidth += spaceSize;

                currentLine.AddRange(abbrev);
                currentLineWidth += CalculateElementsSize(text, abbrev).X;
                currentWord.Clear();
                return;
            }

            // abbreviation failed -> put the whole numeric token onto the line (avoid splitting numbers)
            if (currentLine.Count > 0)
            {
                lines.Add(new List<RichElement>(currentLine));
                currentLine.Clear();
                currentLineWidth = 0f;
            }
            currentLine.AddRange(currentWord);
            currentLineWidth += wordWidth;
            currentWord.Clear();
            return;
        }

        // 2) Try preferred split points (non-letter, non-digit)
        float remaining = maxWidth - currentLineWidth - (currentLine.Count > 0 ? spaceSize : 0f);
        if (TrySplitAtPreferredCharacter(text, currentWord, remaining, out var leftPart, out var rightPart))
        {
            float leftW = CalculateElementsSize(text, leftPart).X;
            // ensure left fits on current line; if not, push current line first
            if (currentLine.Count > 0 && currentLineWidth + spaceSize + leftW > maxWidth)
            {
                lines.Add(new List<RichElement>(currentLine));
                currentLine.Clear();
                currentLineWidth = 0f;
            }

            if (currentLine.Count > 0)
                currentLineWidth += spaceSize;

            // append left part and optionally hyphen (only when both sides are letters)
            var hyphen = CreateHyphenElement(leftPart.LastOrDefault(), text);
            if (ShouldAddHyphen(leftPart, rightPart))
                leftPart.Add(hyphen);

            currentLine.AddRange(leftPart);
            currentLineWidth += CalculateElementsSize(text, leftPart).X;

            // finalize this line
            lines.Add(new List<RichElement>(currentLine));
            currentLine.Clear();
            currentLineWidth = 0f;

            // Now handle the right part *recursively* so it can be split again if needed
            currentWord = rightPart;
            AppendWordWithWrapping(text, maxWidth, ref currentLine, ref currentLineWidth, ref currentWord, spaceSize, lines);
            return;
        }

        // 3) Conservative char-level split (avoid splitting numeric runs)
        if (TryCharacterSplit(text, currentWord, remaining, out leftPart, out rightPart))
        {
            float leftW = CalculateElementsSize(text, leftPart).X;
            if (currentLine.Count > 0 && currentLineWidth + spaceSize + leftW > maxWidth)
            {
                lines.Add(new List<RichElement>(currentLine));
                currentLine.Clear();
                currentLineWidth = 0f;
            }

            if (currentLine.Count > 0)
                currentLineWidth += spaceSize;

            // add hyphen only if this is a true word-break
            if (ShouldAddHyphen(leftPart, rightPart))
                leftPart.Add(CreateHyphenElement(leftPart.LastOrDefault(), text));

            currentLine.AddRange(leftPart);
            currentLineWidth += CalculateElementsSize(text, leftPart).X;

            // push completed line
            lines.Add(new List<RichElement>(currentLine));
            currentLine.Clear();
            currentLineWidth = 0f;

            // recursive handling of remainder
            currentWord = rightPart;
            AppendWordWithWrapping(text, maxWidth, ref currentLine, ref currentLineWidth, ref currentWord, spaceSize, lines);
            return;
        }

        // 4) Fallback: push current line (if any) and put entire word on next line (may overflow)
        if (currentLine.Count > 0)
        {
            lines.Add(new List<RichElement>(currentLine));
            currentLine.Clear();
            currentLineWidth = 0f;
        }
        currentLine.AddRange(currentWord);
        currentLineWidth += wordWidth;
        currentWord.Clear();
    }


    private static bool IsNumericElementSequence(List<RichElement> elems)
    {
        if (elems == null || elems.Count == 0) return false;

        var sb = new StringBuilder();
        foreach (var e in elems)
        {
            if (e is RichWord rw)
                sb.Append(rw.Text);
            else if (e is RichGlyph rg)
                sb.Append(rg.Character);
            else
                return false; // contains sprite or other -> not numeric
        }

        string s = sb.ToString().Trim();
        if (s.Length == 0) return false;

        // allow digits, optional leading sign, optional single decimal point, optional commas
        int dotCount = 0;
        int digitCount = 0;
        foreach (char c in s)
        {
            if (char.IsDigit(c)) digitCount++;
            else if (c == '.') dotCount++;
            else if (c == ',') continue;
            else if (c == '+' || c == '-') continue;
            else return false;
        }
        return digitCount > 0 && dotCount <= 1;
    }

    // Helper: produce abbreviated RichElement list for numeric sequence that fits in targetWidth
    private static List<RichElement>? AbbreviateNumericElementsToFit(RichText text, List<RichElement> original, float targetWidth)
    {
        // build original numeric string
        var sb = new StringBuilder();
        foreach (var e in original)
        {
            if (e is RichWord rw) sb.Append(rw.Text);
            else if (e is RichGlyph rg) sb.Append(rg.Character);
        }
        string orig = sb.ToString();

        // try suffix-based abbreviations for large integers
        if (double.TryParse(orig.Replace(",", ""), out double val))
        {
            // try suffixes K, M, B, T
            var suffixes = new (double div, string suf)[] {
                (1_000_000_000_000d, "T"),
                (1_000_000_000d, "B"),
                (1_000_000d, "M"),
                (1_000d, "K")
            };

            foreach (var sfx in suffixes)
            {
                if (Math.Abs(val) >= sfx.div)
                {
                    double v = val / sfx.div;
                    // try with 1 decimal if needed
                    string candidate = v % 1 == 0 ? $"{(long)v}{sfx.suf}" : $"{Math.Round(v, 1)}{sfx.suf}";
                    var candidateElems = new List<RichElement> { new RichWord(candidate, original.LastOrDefault() is RichWord rw ? rw.Color : Color.White) };
                    float w = CalculateElementsSize(text, candidateElems).X;
                    if (w <= targetWidth) return candidateElems;
                    // try without decimals
                    candidate = $"{(long)Math.Round(v)}{sfx.suf}";
                    candidateElems = new List<RichElement> { new RichWord(candidate, original.LastOrDefault() is RichWord rw2 ? rw2.Color : Color.White) };
                    w = CalculateElementsSize(text, candidateElems).X;
                    if (w <= targetWidth) return candidateElems;
                }
            }

            // if decimal number or small integer that doesn't need suffixing, shorten decimal precision with "..."
            if (orig.Contains("."))
            {
                // try progressively fewer decimal digits until it fits, append "..."
                int maxDecimals = 6;
                int decimals = maxDecimals;
                while (decimals >= 0)
                {
                    string fmt = Math.Round(val, decimals).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    // if rounding removed decimals entirely, we still want to append "..." only if original had more precision
                    string candidate = fmt;
                    if (fmt.Length < orig.Length) candidate += "...";
                    var candidateElems = new List<RichElement> { new RichWord(candidate, original.LastOrDefault() is RichWord rw3 ? rw3.Color : Color.White) };
                    float w = CalculateElementsSize(text, candidateElems).X;
                    if (w <= targetWidth) return candidateElems;
                    decimals--;
                }
            }
            else
            {
                // integer but too long: try formatting with grouping (1,000) then suffix fallback
                string candidate = AbbreviateIntegerSimple((long)val);
                var candidateElems = new List<RichElement> { new RichWord(candidate, original.LastOrDefault() is RichWord rw4 ? rw4.Color : Color.White) };
                if (CalculateElementsSize(text, candidateElems).X <= targetWidth) return candidateElems;
            }
        }

        return null;
    }

    private static string AbbreviateIntegerSimple(long v)
    {
        if (Math.Abs(v) >= 1_000_000_000_000L) return $"{v / 1_000_000_000_000L}T";
        if (Math.Abs(v) >= 1_000_000_000L) return $"{v / 1_000_000_000L}B";
        if (Math.Abs(v) >= 1_000_000L) return $"{v / 1_000_000L}M";
        if (Math.Abs(v) >= 1_000L) return $"{v / 1_000L}K";
        return v.ToString();
    }

    // Try to split at preferred characters (non a-z, non-digit) from the end.
    // Returns left and right parts as element lists (preserving colors and link metadata).
    // Replace the old TrySplitAtPreferredCharacter with this
    private static bool TrySplitAtPreferredCharacter(RichText text, List<RichElement> wordElems, float remainingWidth, out List<RichElement> left, out List<RichElement> right)
    {
        left = new List<RichElement>();
        right = new List<RichElement>();

        // build flat char string + mapping to original element indices
        var sb = new StringBuilder();
        var mapping = new List<(int elemIndex, int charIndexInElem)>();
        for (int i = 0; i < wordElems.Count; i++)
        {
            if (wordElems[i] is RichWord rw)
            {
                for (int c = 0; c < rw.Text.Length; c++)
                {
                    sb.Append(rw.Text[c]);
                    mapping.Add((i, c));
                }
            }
            else if (wordElems[i] is RichGlyph rg)
            {
                sb.Append(rg.Character);
                mapping.Add((i, 0));
            }
            else
            {
                // don't attempt to split sequences with sprites or unknown elems
                return false;
            }
        }

        string s = sb.ToString();
        if (s.Length <= 1) return false;

        // Priority groups (in order). We include the split character in the LEFT part for nicer visual flow.
        char[][] priorityGroups = new char[][] {
        // strong punctuation / sentence break characters
        new char[] { ',', '.', '(', ')', '[', ']', '{', '}', ':', ';', '/', '\\' },

        // connector / special characters (prefer these next)
        new char[] { '-', '_', '@', '+' }
    };

        // Helper to test a candidate split where left length = splitIndex + 1 (include split char in left)
        bool TryCandidateAt(int splitIndex, out List<RichElement> lElems, out List<RichElement> rElems)
        {
            lElems = CreateElementsFromTextFragment(wordElems, mapping, 0, splitIndex + 1, text);
            rElems = CreateElementsFromTextFragment(wordElems, mapping, splitIndex + 1, s.Length - (splitIndex + 1), text);
            // require non-empty right side
            if (rElems.Count == 0) return false;
            // ensure left fits into the remaining width
            return CalculateElementsSize(text, lElems).X <= remainingWidth;
        }

        // scan each priority group from the end of the word inward
        foreach (var group in priorityGroups)
        {
            for (int split = s.Length - 1; split > 0; split--)
            {
                char c = s[split];
                // never split numerical digits
                if (char.IsDigit(c)) continue;
                // skip plain ASCII letters a-z (we want non-letter splits first)
                if (char.IsLetter(c) && (char.ToLowerInvariant(c) >= 'a' && char.ToLowerInvariant(c) <= 'z')) continue;

                // if this char is in the current priority group, try it
                bool inGroup = false;
                for (int g = 0; g < group.Length; g++)
                {
                    if (group[g] == c) { inGroup = true; break; }
                }
                if (!inGroup) continue;

                if (TryCandidateAt(split, out var leftElems, out var rightElems))
                {
                    left = leftElems;
                    right = rightElems;
                    return true;
                }
            }
        }

        // If nothing in the prioritized sets matched, fall back to the old "any non-letter non-digit" approach
        for (int split = s.Length - 1; split > 0; split--)
        {
            char c = s[split];
            if (char.IsLetter(c) && (char.ToLowerInvariant(c) >= 'a' && char.ToLowerInvariant(c) <= 'z')) continue;
            if (char.IsDigit(c)) continue;

            if (TryCandidateAt(split, out var leftElems2, out var rightElems2))
            {
                left = leftElems2;
                right = rightElems2;
                return true;
            }
        }

        return false;
    }

    // Conservative char-level split that avoids splitting numeric runs.
    private static bool TryCharacterSplit(RichText text, List<RichElement> wordElems, float remainingWidth, out List<RichElement> left, out List<RichElement> right)
    {
        left = new List<RichElement>();
        right = new List<RichElement>();

        var sb = new StringBuilder();
        var mapping = new List<(int elemIndex, int charIndexInElem)>();
        for (int i = 0; i < wordElems.Count; i++)
        {
            if (wordElems[i] is RichWord rw)
            {
                for (int c = 0; c < rw.Text.Length; c++)
                {
                    sb.Append(rw.Text[c]);
                    mapping.Add((i, c));
                }
            }
            else if (wordElems[i] is RichGlyph rg)
            {
                sb.Append(rg.Character);
                mapping.Add((i, 0));
            }
            else
            {
                return false;
            }
        }

        string s = sb.ToString();
        // avoid splitting inside a digit run: find candidate split index where left contains no trailing digit and right doesn't start with digit
        for (int split = Math.Min(s.Length - 1, Math.Max(1, (int)(remainingWidth / Math.Max(1, text.FontSize)))); split > 0; split--)
        {
            if (char.IsDigit(s[split - 1]) || char.IsDigit(s[split])) continue;

            var leftElems = CreateElementsFromTextFragment(wordElems, mapping, 0, split, text);
            var rightElems = CreateElementsFromTextFragment(wordElems, mapping, split, s.Length - split, text);

            if (CalculateElementsSize(text, leftElems).X <= remainingWidth)
            {
                left = leftElems;
                right = rightElems;
                return true;
            }
        }

        // fallback: try greedy growing left part until it fits
        for (int split = s.Length - 1; split > 0; split--)
        {
            var leftElems = CreateElementsFromTextFragment(wordElems, mapping, 0, split, text);
            var rightElems = CreateElementsFromTextFragment(wordElems, mapping, split, s.Length - split, text);
            if (CalculateElementsSize(text, leftElems).X <= remainingWidth)
            {
                left = leftElems;
                right = rightElems;
                return true;
            }
        }

        return false;
    }

    // Rebuild a list of RichElement from a fragment of the original token (uses mapping to preserve style)
    private static List<RichElement> CreateElementsFromTextFragment(List<RichElement> originalElems, List<(int elemIndex, int charIndexInElem)> mapping, int startIndex, int length, RichText context)
    {
        var result = new List<RichElement>();
        if (length <= 0) return result;

        int endIndex = startIndex + length; // exclusive
        int mapPos = startIndex;
        while (mapPos < endIndex)
        {
            var map = mapping[mapPos];
            var sourceElem = originalElems[map.elemIndex];

            if (sourceElem is RichWord srcWord)
            {
                // gather consecutive chars that come from the same RichWord element
                var sb = new StringBuilder();
                var color = srcWord.Color;
                string link = srcWord.LinkUrl;
                int elemIndex = map.elemIndex;
                while (mapPos < endIndex && mapping[mapPos].elemIndex == elemIndex)
                {
                    sb.Append(srcWord.Text[mapping[mapPos].charIndexInElem]);
                    mapPos++;
                }
                result.Add(new RichWord(sb.ToString(), color, link));
            }
            else if (sourceElem is RichGlyph srcGlyph)
            {
                // glyphs are single characters — preserve the original character and color/link metadata
                result.Add(new RichGlyph(srcGlyph.Character, srcGlyph.Color) { GlyphLinkUrl = (srcGlyph as RichGlyph).GlyphLinkUrl });
                mapPos++;
            }
            else
            {
                // unknown: just clone the original reference rather than mutate it
                result.Add(sourceElem);
                mapPos++;
            }
        }

        return result;
    }

    // Create a hyphen glyph preserving color from a source token element if available
    private static RichGlyph CreateHyphenElement(RichElement? sample, RichText context)
    {
        Color c = Color.White;
        if (sample is RichWord rw) c = rw.Color;
        else if (sample is RichGlyph rg) c = rg.Color;
        return new RichGlyph('-', c);
    }

    private static Vector2 CalculateElementsSize(RichText text, List<RichElement> elements)
    {
        // sums width of a run of glyphs and sprites; returns combined width & max height
        var sb = new StringBuilder();
        float width = 0f;
        float maxHeight = 0f;

        void FlushStringBuilder()
        {
            if (sb.Length == 0) return;
            var m = MeasureTextExCached(text.Font, sb.ToString(), text.FontSize, text.Spacing);
            width += m.X;
            if (m.Y > maxHeight) maxHeight = m.Y;
            sb.Clear();
        }

        for (int idx = 0; idx < elements.Count; idx++)
        {
            var element = elements[idx];

            switch (element)
            {
                case RichGlyph g:
                    sb.Append(g.Character);
                    break;

                case RichWord w:
                    // words are multi-char runs, append into same sb to measure as single run
                    sb.Append(w.Text);
                    break;

                case RichSprite s:
                    // flush any text run before measuring sprite
                    FlushStringBuilder();

                    // cache per sprite key + fontSize because scale depends on fontSize
                    string key = s.SpriteKey + "|" + text.FontSize;
                    if (!SPRITE_SIZE_CACHE.TryGetValue(key, out var size))
                    {
                        var texture = RichSpriteRegistry.GetSprite(s.SpriteKey);
                        if (texture is not null)
                        {
                            float spriteHeight = s.BaseSize * text.FontSize;
                            float scale = spriteHeight / texture.Height;
                            size = new Vector2(texture.Width * scale, texture.Height * scale);
                        }
                        else
                        {
                            size = Vector2.Zero;
                        }
                        SPRITE_SIZE_CACHE[key] = size;
                    }

                    width += size.X;
                    if (size.Y > maxHeight) maxHeight = size.Y;
                    break;

                default:
                    // unknown element kind — flush and try to be safe
                    FlushStringBuilder();
                    break;
            }

            // add inter-element spacing (we will trim trailing spacing at the end)
            width += text.Spacing;
        }

        // final flush of any pending text
        FlushStringBuilder();

        // remove trailing spacing that we always added after each element
        if (width >= text.Spacing)
            width -= text.Spacing;

        return new Vector2(width, maxHeight);
    }

    // Return the first printable character of the element (RichWord or RichGlyph) or null
    private static char? GetFirstCharFromElement(RichElement e)
    {
        if (e is RichWord rw && !string.IsNullOrEmpty(rw.Text))
            return rw.Text[0];
        if (e is RichGlyph rg)
            return rg.Character;
        return null;
    }

    // Return the last printable character of the element (RichWord or RichGlyph) or null
    private static char? GetLastCharFromElement(RichElement e)
    {
        if (e is RichWord rw && !string.IsNullOrEmpty(rw.Text))
            return rw.Text[^1];
        if (e is RichGlyph rg)
            return rg.Character;
        return null;
    }

    // Decide whether to add hyphen between left and right fragments:
    // only add when both sides are letters (word-break)
    private static bool ShouldAddHyphen(List<RichElement> left, List<RichElement> right)
    {
        if (left == null || left.Count == 0) return false;
        if (right == null || right.Count == 0) return false;

        // find last char of left
        char? last = null;
        for (int i = left.Count - 1; i >= 0; i--)
        {
            last = GetLastCharFromElement(left[i]);
            if (last.HasValue) break;
        }

        // find first char of right
        char? first = null;
        for (int i = 0; i < right.Count; i++)
        {
            first = GetFirstCharFromElement(right[i]);
            if (first.HasValue) break;
        }

        return last.HasValue && first.HasValue && char.IsLetter(last.Value) && char.IsLetter(first.Value);
    }

    internal static void DrawRichText(RichText title, Vector2 textPos, float maxTextWidth, object white, object value) => throw new NotImplementedException();
}
