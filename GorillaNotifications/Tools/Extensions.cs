using System;
using System.Text;
using System.Text.RegularExpressions;

namespace GorillaNotifications.Tools;

internal static class Extensions
{
    // returns 90 for 1920 which is my sweet spot and should return proportionally for other screen widths (*hopefully*)
    public static int MapForScreenWidth(this int screenWidth) => screenWidth * 3 / 64;

    public static string NormaliseNotification(this string currentNotification, int newLineInterval)
    {
        if (currentNotification.IsNullOrEmpty())
            return currentNotification;

        currentNotification = Regex.Replace(currentNotification, @"<size\s*=\s*[^>]+>", "", RegexOptions.IgnoreCase);
        currentNotification = Regex.Replace(currentNotification, @"</size>",            "", RegexOptions.IgnoreCase);

        StringBuilder output                       = new();
        int           visibleCount                 = 0;
        int           lastWhitespaceIndex          = -1;
        int           outputLengthAtLastWhitespace = -1;

        for (int i = 0; i < currentNotification.Length; i++)
        {
            char c = currentNotification[i];

            if (c == '<')
            {
                int tagEnd = currentNotification.IndexOf('>', i);
                if (tagEnd == -1)
                {
                    output.Append(c);

                    continue;
                }

                output.Append(currentNotification.AsSpan(i, tagEnd - i + 1));
                i = tagEnd;

                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                lastWhitespaceIndex          = i;
                outputLengthAtLastWhitespace = output.Length;
            }

            output.Append(c);
            visibleCount++;

            if (visibleCount < newLineInterval)
                continue;

            if (outputLengthAtLastWhitespace != -1)
            {
                output[outputLengthAtLastWhitespace] = '\n';
                visibleCount                         = i - lastWhitespaceIndex;
                lastWhitespaceIndex                  = -1;
                outputLengthAtLastWhitespace         = -1;
            }
            else
            {
                output.Append('\n');
                visibleCount = 0;
            }
        }

        return output.ToString();
    }
}