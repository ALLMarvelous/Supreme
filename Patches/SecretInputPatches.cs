using HarmonyLib;
using Il2Cpp;
using Supreme.Game;

namespace Supreme.Patches;

[HarmonyPatch(typeof(SpecialSongManager), nameof(SpecialSongManager.IsSpecialMusic))]
internal static class LongPressPatch
{
    private static void Postfix(ref bool __result)
    {
        if (__result && HiddenChart.IsCurrentEligible()) __result = false;
    }
}

[HarmonyPatch(typeof(SpecialSongManager), nameof(SpecialSongManager.UpdateFDInput))]
internal static class FdInputPatch
{
    private static bool Prefix()
    {
        return !HiddenChart.IsCurrentEligible();
    }
}

[HarmonyPatch(typeof(SpecialSongManager), nameof(SpecialSongManager.XInputCalc))]
internal static class XInputPatch
{
    private static bool Prefix()
    {
        return !HiddenChart.IsCurrentEligible();
    }
}

[HarmonyPatch(typeof(ScreenCDrawingDetector), nameof(ScreenCDrawingDetector.Update))]
internal static class DrawingPatch
{
    private static bool Prefix()
    {
        return !HiddenChart.IsCurrentEligible();
    }
}