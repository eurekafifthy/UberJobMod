using System;
using System.Collections.Generic;

namespace RideshareSideJobMod
{
    public static class ConversationTracker
    {
        private static readonly System.Random random = new System.Random();
        private static readonly Dictionary<string, Queue<int>> recentlyUsedLines = new Dictionary<string, Queue<int>>();
        private static readonly HashSet<string> recentlyUsedContent = new HashSet<string>();
        private const int MaxRecentLines = 5;
        private const int MaxRecentContent = 10;

        public static string NormalizeComment(string comment)
        {
            return comment.Replace("{playerName}", "")
                         .Replace("*", "")
                         .Trim()
                         .ToLower();
        }

        public static int GetRandomLineIndex(List<string> lines, string contextKey)
        {
            if (!recentlyUsedLines.ContainsKey(contextKey))
            {
                recentlyUsedLines[contextKey] = new Queue<int>(MaxRecentLines);
            }

            var usedIndices = recentlyUsedLines[contextKey];
            int index;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                index = random.Next(0, lines.Count);
                attempts++;
                string normalizedComment = NormalizeComment(lines[index]);
                if (recentlyUsedContent.Contains(normalizedComment) && attempts < maxAttempts)
                {
                    continue;
                }
                break;
            } while (usedIndices.Contains(index) && usedIndices.Count >= lines.Count / 2 && attempts < maxAttempts);

            if (usedIndices.Count >= MaxRecentLines)
            {
                usedIndices.Dequeue();
            }
            usedIndices.Enqueue(index);

            string normalizedCommentFinal = NormalizeComment(lines[index]);
            recentlyUsedContent.Add(normalizedCommentFinal);
            if (recentlyUsedContent.Count > MaxRecentContent)
            {
                recentlyUsedContent.Remove(recentlyUsedContent.First());
            }

            return index;
        }
    }
}