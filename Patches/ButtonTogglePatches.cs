using HarmonyLib;
using Il2CppAssets.Scripts.PeroTools.Nice.Components;
using Supreme.UI;

namespace Supreme.Patches;

[HarmonyPatch(typeof(ButtonToggle), nameof(ButtonToggle.Awake))]
internal static class ButtonToggleAwakePatch
{
    private static void Prefix(ButtonToggle __instance)
    {
        SupremeToggle.Attach(__instance);
    }
}

[HarmonyPatch(typeof(ButtonToggle), nameof(ButtonToggle.SetDiffTglTxt))]
internal static class SetDiffTglTxtPatch
{
    private static void Postfix(ButtonToggle __instance)
    {
        SupremeToggle.For(__instance)?.Refresh();
    }
}

[HarmonyPatch(typeof(ButtonToggle), nameof(ButtonToggle.ToggleDiff))]
internal static class ToggleDiffPatch
{
    private static bool Prefix(ButtonToggle __instance)
    {
        return SupremeToggle.For(__instance)?.BeforeToggleDiff() ?? true;
    }

    private static void Postfix(ButtonToggle __instance)
    {
        SupremeToggle.For(__instance)?.AfterToggleDiff();
    }
}

[HarmonyPatch(typeof(ButtonToggle.__c__DisplayClass15_0), nameof(ButtonToggle.__c__DisplayClass15_0._Awake_b__2))]
internal static class ToggleChangedPatch
{
    private static bool Prefix(ButtonToggle.__c__DisplayClass15_0 __instance, bool state)
    {
        return SupremeToggle.For(__instance.__4__this)?.OnToggleChanged(__instance.tgl, state) ?? true;
    }
}