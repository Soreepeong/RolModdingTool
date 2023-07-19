using System.Collections.Generic;
using System.Linq;

namespace SynergyLib.Util;

public static class StringExtensions {
    public static bool IsTrueyString(this string s, bool emptyIsTrue = false) => s.ToLowerInvariant() switch {
        "" => emptyIsTrue,
        "t" => true,
        "true" => true,
        "1" => true,
        "y" => true,
        "yes" => true,
        _ => false
    };

    public static List<string> StripCommonParentPaths(this IEnumerable<string> fullNamesEnumerable) {
        var namesDepths = new List<List<string>>();
        var fullNames = fullNamesEnumerable.Select(x => x.Replace("\\", "/")).ToList();
        if (!fullNames.Any())
            return fullNames;
        var maxDepth = fullNames.Max(x => x.Count(y => y == '/'));

        var names = new List<string>();
        for (var i = 0; i < fullNames.Count; i++) {
            var nameDepth = 0;
            for (; nameDepth < namesDepths.Count; nameDepth++) {
                if (namesDepths[nameDepth].Count(x => x == namesDepths[nameDepth][i]) < 2)
                    break;
            }

            if (nameDepth == namesDepths.Count) {
                namesDepths.Add(
                    fullNames.Select(x => string.Join('/', x.Split('/').TakeLast(nameDepth + 1))).ToList());
                if (nameDepth < maxDepth) {
                    i--;
                    continue;
                }
            }

            names.Add(namesDepths[nameDepth][i]);
        }

        return names;
    }
}
