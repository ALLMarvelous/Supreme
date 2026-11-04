using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;

namespace Supreme.Game;

internal static class HiddenChart
{
    private const int HiddenDifficulty = 4;
    private const int LanguageCount = 5;
    private const int NoInvokeType = 0;
    private const string CustomChartPrefix = "999-";
    private const string RenamedSong = "21-2";

    private static readonly HashSet<string> Excluded = new() { "39-0", "0-54", "0-56", "0-58", "0-60", "4-5", "8-3" };

    private static readonly HashSet<string> SwappedPreview = new() { RenamedSong, "51-4" };

    internal static bool IsActivating { get; private set; }

    private static SpecialSongManager Special => Singleton<SpecialSongManager>.instance;

    internal static bool TryGetTrigger(MusicInfo? info, out int trigger)
    {
        trigger = 0;
        var uid = info?.uid;
        if (string.IsNullOrEmpty(uid) || Excluded.Contains(uid)) return false;

        var special = Special;
        if (special == null) return false;

        special.InitHideBmsInfoDic();
        if (special.IsBarrageBms(uid)) return false;
        if (!special.m_HideBmsInfos.TryGetValue(uid, out var hidden) || hidden == null) return false;
        if (hidden.m_HideDiff != HiddenDifficulty || hidden.triggerDiff is < 1 or > 3) return false;
        if (!IsPlayableLevel(info!.difficulty4)) return false;

        DisableSecretInput(special, uid);
        trigger = hidden.triggerDiff;
        return true;
    }

    internal static bool IsCurrentEligible()
    {
        return TryGetTrigger(GlobalDataBase.s_DbMusicTag?.CurMusicInfo(), out _);
    }

    internal static bool IsInvoked(MusicInfo info)
    {
        return Special.IsInvokeHideBms(info.uid);
    }

    internal static bool IsPlayableLevel(string? level)
    {
        return !string.IsNullOrEmpty(level) && level != "0";
    }

    internal static void Activate(MusicInfo info, int trigger)
    {
        if (info.uid.StartsWith(CustomChartPrefix))
        {
            Special.m_IsInvokeHideDic[info.uid] = true;
            info.AddMaskValue($"difficulty{trigger}", info.difficulty4);
            info.AddMaskValue($"levelDesigner{trigger}", info.levelDesigner4 ?? info.levelDesigner);
            info.SetDifficulty(trigger, HiddenDifficulty);
            return;
        }

        IsActivating = true;
        try
        {
            Special.InvokeHideBms(info, true);
        }
        finally
        {
            IsActivating = false;
        }
    }

    internal static void Deactivate(MusicInfo info)
    {
        Special.m_IsInvokeHideDic[info.uid] = false;
        info.ClearMaskValue();

        if (info.uid == RenamedSong) ClearLocalizedName(info);
        if (SwappedPreview.Contains(info.uid))
            Singleton<AudioManager>.instance.PlayBGM(info.demo);
    }
    
    private static void DisableSecretInput(SpecialSongManager special, string uid)
    {
        if (special.m_ConfigHideMusic.m_HideMusicObjectMapping.TryGetValue(uid, out var config) && config != null)
            config.invokeType = NoInvokeType;
    }

    private static void ClearLocalizedName(MusicInfo info)
    {
        for (var language = 0; language < LanguageCount; language++) info.GetLocal(language)?.ClearMaskValue();
    }
}