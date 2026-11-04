using System.Reflection;
using MelonLoader;
using Supreme.UI;
using Main = Supreme.Main;

[assembly: AssemblyVersion($"{Main.MelonVersion}.0")]
[assembly: AssemblyFileVersion($"{Main.MelonVersion}.0")]
[assembly: MelonInfo(typeof(Main), Main.MelonName, Main.MelonVersion, Main.MelonAuthor)]
[assembly: MelonGame("PeroPeroGames", "MuseDash")]
[assembly: MelonColor(255, 255, 197, 61)]
[assembly: MelonOptionalDependencies("CustomAlbums")]

namespace Supreme;

public class Main : MelonMod
{
    public const string MelonName = "Supreme";
    public const string MelonVersion = "1.0.0";
    public const string MelonAuthor = "MARVELOUS";

    internal static MelonLogger.Instance Log { get; } = new(MelonName);

    public override void OnUpdate()
    {
        SupremeToggle.Tick();
    }
}