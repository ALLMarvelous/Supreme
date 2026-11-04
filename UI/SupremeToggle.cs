using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.GeneralLocalization;
using Il2CppAssets.Scripts.PeroTools.Managers;
using Il2CppAssets.Scripts.PeroTools.Nice.Components;
using Il2CppAssets.Scripts.UI.Panels;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Supreme.Game;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Supreme.UI;

internal sealed class SupremeToggle
{
    private const string TemplateName = "TglDifficultyMaster";
    private const string LockPath = "ImgStarBg/ImgLock";
    private const string StarPath = "ImgStarBg/ImgStar";
    private const string LabelName = "TxtDifficulty";
    private const string ValueName = "TxtStarValue";
    private const int LabelChildIndex = 2;
    private const int MasterSlot = 3;

    private static readonly Color Fill = new Color32(255, 210, 63, 255);
    private static readonly Color Accent = new Color32(255, 138, 31, 255);
    private static readonly Color Disc = new Color32(178, 58, 18, 255);
    private static readonly Color LabelColor = new Color32(224, 97, 15, 255);

    private static readonly Dictionary<string, string> Labels = new()
    {
        ["English"] = "SUPREME",
        ["ChineseS"] = "至尊",
        ["ChineseT"] = "至尊",
        ["Japanese"] = "スプリーム",
        ["Korean"] = "슈프림"
    };

    private static readonly string[] SlotLabelKeys =
        { "pnl_stage_diff_easy", "pnl_stage_diff_hard", "pnl_stage_diff_master" };

    private static SupremeToggle? current;
    private readonly GameObject cloneLock;
    private readonly GameObject cloneStar;
    private readonly Text label;

    private readonly ButtonToggle owner;
    private readonly GameObject root;
    private readonly GameObject template;
    private readonly GameObject templateLock;
    private readonly GameObject templateStar;
    private readonly Toggle toggle;
    private readonly Text value;
    private bool isOpen;
    private bool isStarting;
    private bool isSyncing;

    private int visibleTrigger;

    private SupremeToggle(ButtonToggle owner, GameObject template)
    {
        this.owner = owner;
        this.template = template;

        root = Object.Instantiate(template, template.transform.parent);
        root.name = "TglDifficultySupreme";
        toggle = root.GetComponent<Toggle>();
        toggle.SetIsOnWithoutNotify(false);

        // The localization component would rewrite the label back to MASTER.
        foreach (var localization in root.GetComponentsInChildren<Localization>(true))
            Object.DestroyImmediate(localization);

        label = FindText(LabelName);
        value = FindText(ValueName);
        cloneLock = root.transform.Find(LockPath).gameObject;
        cloneStar = root.transform.Find(StarPath).gameObject;
        templateLock = template.transform.Find(LockPath).gameObject;
        templateStar = template.transform.Find(StarPath).gameObject;

        ApplyColors();
        root.SetActive(false);
    }

    private static DBMusicTag Tags => GlobalDataBase.s_DbMusicTag;

    internal static void Attach(ButtonToggle owner)
    {
        try
        {
            if (owner.toggles.Count != MasterSlot) return;
            var template = owner.toggles[MasterSlot - 1].gameObject;
            if (template.name != TemplateName) return;
            if (owner.GetComponentInParent<PnlPreparationForHideBms>() != null) return;

            current = new SupremeToggle(owner, template);
            owner.toggles.Add(current.toggle);
        }
        catch (Exception ex)
        {
            current = null;
            Main.Log.Error($"Could not add the Supreme button: {ex}");
        }
    }

    internal static SupremeToggle? For(ButtonToggle owner)
    {
        return current != null && current.owner.Pointer == owner.Pointer ? current : null;
    }

    internal static void MarkStarting()
    {
        if (current != null) current.isStarting = true;
    }

    internal void Refresh()
    {
        var info = Tags.CurMusicInfo();
        if (!HiddenChart.TryGetTrigger(info, out var trigger))
        {
            Hide();
            return;
        }

        visibleTrigger = trigger;
        if (!owner.toggles.Contains(toggle)) owner.toggles.Add(toggle);
        root.SetActive(true);

        var templateLabel = template.transform.GetChild(LabelChildIndex).GetComponent<Text>();
        label.font = templateLabel.font;
        label.text = Labels.TryGetValue(DataHelper.userLanguage ?? "", out var text) ? text : Labels["English"];
        value.text = info.difficulty4;

        if (HiddenChart.IsInvoked(info)) RestoreSlot(info, trigger);
    }

    internal bool BeforeToggleDiff()
    {
        isSyncing = true;
        var info = Tags.CurMusicInfo();
        if (!IsSelected(info, out var trigger)) return true;

        toggle.isOn = true;
        ApplySelection(info, trigger);
        return false;
    }

    internal void AfterToggleDiff()
    {
        isSyncing = false;
    }

    internal bool OnToggleChanged(Toggle changed, bool state)
    {
        var isSupreme = changed.Pointer == toggle.Pointer;
        if (isSyncing) return !isSupreme;
        if (!state || !changed.isActiveAndEnabled) return !isSupreme;

        var info = Tags.CurMusicInfo();
        if (isSupreme)
        {
            Select(info);
            return false;
        }

        if (HiddenChart.TryGetTrigger(info, out _) && HiddenChart.IsInvoked(info))
        {
            HiddenChart.Deactivate(info);

            if (!Tags.m_LockMusic && Tags.m_SelectedDiffTglIndex == SlotOf(changed)) Tags.m_SelectedDiffTglIndex = 0;
        }

        return true;
    }

    internal static void Tick()
    {
        if (current == null) return;
        if (current.owner == null || current.root == null)
        {
            current = null;
            return;
        }

        current.Update();
    }

    private void Update()
    {
        if (owner.isActiveAndEnabled)
        {
            isOpen = true;
            if (root.activeSelf) SyncLock();
            return;
        }

        if (!isOpen) return;
        isOpen = false;
        if (!isStarting) ClearAfterBackingOut();
        isStarting = false;
    }

    private void Select(MusicInfo info)
    {
        if (!HiddenChart.TryGetTrigger(info, out var trigger)) return;

        Singleton<AudioManager>.instance.PlayOneShot(owner.clickSfx, DataHelper.sfxVolume);
        Tags.selectedDiffTglIndex = trigger;
        HiddenChart.Activate(info, trigger);
        ApplySelection(info, trigger);
        RestoreSlot(info, trigger);

        Singleton<EventManager>.instance.Invoke("UI/DifficultyChanged",
            new Il2CppReferenceArray<Il2CppSystem.Object>(0));
        owner.onDifficultyChanged?.Invoke();
    }

    private void ApplySelection(MusicInfo info, int trigger)
    {
        GlobalDataBase.s_DbBattleStage.m_MapDifficulty = info.GetDifficulty(trigger);
        owner.levelDesigner.text = info.GetLevelDesignerStringByIndex(trigger);
    }

    private bool IsSelected(MusicInfo info, out int trigger)
    {
        return HiddenChart.TryGetTrigger(info, out trigger) && HiddenChart.IsInvoked(info) &&
               Tags.m_SelectedDiffTglIndex == trigger;
    }

    private void RestoreSlot(MusicInfo info, int trigger)
    {
        var slot = trigger - 1;
        var level = info.GetMusicLevelStringByDiff(trigger, false);
        owner.toggles[slot].gameObject.SetActive(HiddenChart.IsPlayableLevel(level));
        owner.texts[slot].text = level;
        owner.toggles[slot].transform.GetChild(LabelChildIndex).GetComponent<Text>().text =
            DBConfigUILocalization.GetLocalization(SlotLabelKeys[slot]);
    }

    private void Hide()
    {
        owner.toggles.Remove(toggle);

        root.SetActive(false);
        toggle.SetIsOnWithoutNotify(false);
    }

    private static void ClearAfterBackingOut()
    {
        var info = Tags.CurMusicInfo();
        if (!HiddenChart.TryGetTrigger(info, out var trigger) || !HiddenChart.IsInvoked(info)) return;

        HiddenChart.Deactivate(info);

        var stage = Object.FindObjectOfType<PnlStage>();
        if (stage == null) return;

        var level = info.GetMusicLevelStringByDiff(trigger, false);
        var slotText = trigger switch
        {
            1 => stage.difficulty1,
            2 => stage.difficulty2,
            _ => stage.difficulty3
        };
        slotText.text = level;
        stage.RefreshDiffUI(info);
    }

    private void SyncLock()
    {
        var followsMaster = visibleTrigger == MasterSlot;
        cloneLock.SetActive(followsMaster && templateLock.activeSelf);
        cloneStar.SetActive(!followsMaster || templateStar.activeSelf);
    }

    private int SlotOf(Toggle changed)
    {
        for (var i = 0; i < owner.toggles.Count; i++)
            if (owner.toggles[i].Pointer == changed.Pointer)
                return i + 1;
        return -1;
    }

    private Text FindText(string name)
    {
        foreach (var text in root.GetComponentsInChildren<Text>(true))
            if (text.name == name)
                return text;
        throw new InvalidOperationException($"The Master toggle has no {name}");
    }

    private void ApplyColors()
    {
        root.GetComponent<Image>().color = Fill;
        root.transform.Find("ImgMask").GetComponent<Image>().color = Accent;
        root.transform.Find("ImgStarBg").GetComponent<Image>().color = Disc;
        cloneLock.GetComponent<Image>().color = Fill;

        label.color = LabelColor;
        value.color = Disc;
        label.GetComponent<Outline>().effectColor = Fill;
        value.GetComponent<Outline>().effectColor = Fill;

        var star = cloneStar.GetComponent<Image>();
        try
        {
            star.sprite = StarSprite.Recolor(star.sprite, Fill, Accent);
        }
        catch (Exception ex)
        {
            Main.Log.Warning($"Could not recolor the Supreme star: {ex.Message}");
        }
    }
}