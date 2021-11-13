using System.Reflection;
using HarmonyLib;
using Verse;

namespace Euphoric.FixRandomFactionPawns
{
    [StaticConstructorOnStartup]
    // ReSharper disable once UnusedType.Global
    public static class FixRandomFactionPawnsHarmonyPatch
    {
        static FixRandomFactionPawnsHarmonyPatch()
        {
            //Harmony.DEBUG = true;
            
            var harmonyInstance = new Harmony("Euphoric.FixRandomFactionPawns");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            
            Log.Message("Euphoric.FixRandomFactionPawns patched");
        }
    }
}