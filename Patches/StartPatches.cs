using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.PeroTools.Managers;
using Supreme.Game;
using Supreme.UI;

namespace Supreme.Patches;

[HarmonyPatch(typeof(PnlPreparation), nameof(PnlPreparation.OnBattleStart))]
internal static class BattleStartPatch
{
    private static void Prefix()
    {
        SupremeToggle.MarkStarting();
    }
}

[HarmonyPatch(typeof(PnlPreparation), nameof(PnlPreparation.GameStart))]
internal static class GameStartPatch
{
    private static void Prefix()
    {
        SupremeToggle.MarkStarting();
    }
}

[HarmonyPatch(typeof(EventManager), nameof(EventManager.Invoke))]
internal static class SpecialsEventPatch
{
    private const string SpecialsEvent = "UI/OnSpecialsMusic";

    private static bool Prefix(string uid)
    {
        return !HiddenChart.IsActivating || uid != SpecialsEvent;
    }
}