using Harmony;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace LogsNotIncluded.Patches
{
    [HarmonyPatch(typeof(KMod.Manager), "Load")]
    static class PKModManager_PLoad
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("KMod.Manager");
        static bool Prefix(ref Stopwatch __state, KMod.Content content)
        {
            logger.Info("Loading Content {content}", content);
            __state = Stopwatch.StartNew();
            return true;
        }

        static void Postfix(Stopwatch __state, KMod.Content content)
        {
            __state.Stop();
            logger.Info("Finished loading Content {content}. Took {time}ms", content, __state.ElapsedMilliseconds);
        }

        static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            logger.Debug("Loggin Codes: KMod.Manager.Load");
            foreach (CodeInstruction code in instructions) {
                TestPatch.LogCode(code);
                if (code.opcode == OpCodes.Callvirt) {
                    logger.Debug("Found VIRTUAL CALL::::::: {code}", code);
                    logger.Debug("OPPERAND: {OP} : {module}", code.operand.GetType(), code.operand.GetType().Module.Name);
                }
                yield return code;
            }
            yield break;
        }
    }
}
