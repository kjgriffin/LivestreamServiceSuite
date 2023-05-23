using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LutheRun.Pilot
{

    internal class CamsUseTracker
    {

        internal Dictionary<CameraID, Dictionary<string, int>> Solutions { get; private set; } = new Dictionary<CameraID, Dictionary<string, int>>();

        internal Dictionary<CameraID, HashSet<string>> Presets { get; private set; } = new Dictionary<CameraID, HashSet<string>>();

        internal void Solve(CameraID cam, string preset, int eID)
        {
            Dictionary<string, int> csols;
            if (!Solutions.TryGetValue(cam, out csols))
            {
                csols = new Dictionary<string, int>();
                Solutions[cam] = csols;
            }
            csols[preset] = eID;
        }

        internal void Require(CameraID cam, string preset)
        {
            HashSet<string> presets;
            if (Presets.TryGetValue(cam, out presets))
            {
                presets.Add(preset);
            }
            else
            {
                presets = new HashSet<string>(preset.ItemAsEnumerable());
                Presets[cam] = presets;
            }
        }
        internal void Free(CameraID cam)
        {
            if (Presets.ContainsKey(cam))
            {
                Presets.Remove(cam);
            }
        }

        internal CamsUseTracker Copy()
        {
            CamsUseTracker res = new();
            foreach (var cam in Presets)
            {
                foreach (var pst in cam.Value)
                {
                    res.Require(cam.Key, pst);
                }
            }
            return res;
        }

        internal Dictionary<CameraID, List<string>> ComputeConflicts(CamsUseTracker inUse, bool strict = true)
        {
            Dictionary<CameraID, List<string>> res = new Dictionary<CameraID, List<string>>();

            foreach (var kvp in inUse.Presets)
            {
                if (Presets.TryGetValue(kvp.Key, out var reqs))
                {
                    // if in strict mode warn about every preset for this camera
                    if (strict)
                    {
                        res[kvp.Key] = reqs.ToList();
                    }
                    else
                    {
                        // only warn about presets that are different
                        var conflicts = reqs.Where(x => !kvp.Value.Contains(x)).ToList(); // oooooh this smells like an expensive operation- but then again for small values of n O(n^2) is smallish....
                        if (conflicts.Any())
                        {
                            res[kvp.Key] = conflicts;
                        }
                    }
                }
            }
            return res;
        }

        internal string Format(bool showSolutions = false)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in Presets)
            {
                sb.Append($"{kvp.Key}:{{");

                if (showSolutions)
                {
                    foreach (var pst in kvp.Value)
                    {
                        bool solved = false;
                        int solution = -1;
                        // check if it's solved
                        if (Solutions.TryGetValue(kvp.Key, out var solns))
                        {
                            solved = solns.TryGetValue(pst, out solution);
                        }
                        sb.Append($"{pst} {(solved ? $"<setup@{solution}>" : "UNSOLVED")},");
                    }
                }
                else
                {
                    sb.Append(string.Join(",", kvp.Value));
                }

                sb.Append("}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

    }


}
