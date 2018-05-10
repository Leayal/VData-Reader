using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VData_Explorer.Interop
{
    internal static class Helpers
    {
        // TODO: Remove Helpers class, refactor
        internal static Window GetDefaultOwnerWindow()
        {
            Window defaultWindow = null;

            // TODO: Detect active window and change to that instead
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                defaultWindow = Application.Current.MainWindow;
            }
            return defaultWindow;
        }

        public static IEnumerable<string> SortFilenameWithNumber(this IEnumerable<string> list)
        {
            int maxLen = list.Select(s => s.Length).Max();

            return list.Select(s => new
            {
                OrgStr = s,
                SortStr = Regex.Replace(s, @"(\d+)|(\D+)", m => m.Value.PadLeft(maxLen, char.IsDigit(m.Value[0]) ? ' ' : '\xffff'))
            })
            .OrderBy(x => x.SortStr)
            .Select(x => x.OrgStr);
        }

        /// <summary>
        /// Get the required height and width of the specified text. Uses FormattedText
        /// </summary>
        public static FormattedText MeasureTextSize(string text, Typeface typeface, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            return new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black);
        }

        public static FormattedText MeasureTextSize(string text, Typeface typeface, double fontSize)
        {
            return new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, null);
        }

        /// <summary>
        /// Get the required height and width of the specified text. Uses FormattedText
        /// </summary>
        public static FormattedText MeasureTextSize(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            return new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(fontFamily, fontStyle, fontWeight, fontStretch), fontSize, null);
        }

        /// <summary>
        /// Get the required height and width of the specified text. Uses Glyph's
        /// </summary>
        public static Size MeasureText(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            Typeface typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
            GlyphTypeface glyphTypeface;

            if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
            {
                var ft = MeasureTextSize(text, typeface, fontFamily, fontStyle, fontWeight, fontStretch, fontSize);
                return new Size(ft.Width, ft.Height);
            }

            double totalWidth = 0;
            double height = 0;

            ushort glyphIndex;
            double width, glyphHeight;
            for (int n = 0; n < text.Length; n++)
            {
                glyphIndex = glyphTypeface.CharacterToGlyphMap[text[n]];
                if (glyphTypeface.CharacterToGlyphMap.ContainsKey(text[n]))
                {
                    glyphIndex = glyphTypeface.CharacterToGlyphMap[text[n]];
                    width = glyphTypeface.AdvanceWidths[glyphIndex] * fontSize;
                    glyphHeight = glyphTypeface.AdvanceHeights[glyphIndex] * fontSize;
                    if (glyphHeight > height)
                        height = glyphHeight;
                    totalWidth += width;
                }
                else
                {
                    var ft = MeasureTextSize(text, typeface, fontFamily, fontStyle, fontWeight, fontStretch, fontSize);
                    return new Size(ft.Width, ft.Height);
                }
            }
            return new Size(totalWidth, height);
        }

        public delegate void JustAction();
        public delegate void JustActionOneArg(object obj);
        public delegate void JustProgress(int current, int total);

        private static readonly char[] invalidfilenamechars = System.IO.Path.GetInvalidFileNameChars();
        public static string ReplaceInvalidFilenameChars(string str, char newchar)
        {
            StringBuilder sb = new StringBuilder(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                if (Array.IndexOf(invalidfilenamechars, str[i]) == -1)
                    sb.Append(str[i]);
                else
                    sb.Append(newchar);
            }
            return sb.ToString();
        }

        public static bool CanBeFullySeen(FrameworkElement container, FrameworkElement element)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect = new Rect(0, 0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds);
        }

        public static bool CanBePartialSeen(FrameworkElement container, FrameworkElement element)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds =
                element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            var rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        public static bool CanBeSeen(FrameworkElement container, FrameworkElement element)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect = new Rect(0, 0, container.ActualWidth, container.ActualHeight);
            return rect.IntersectsWith(bounds);
        }

        public static bool CanBeSeen(FrameworkElement container, FrameworkElement element, Rect viewport)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            return viewport.IntersectsWith(bounds);
        }
    }
}
