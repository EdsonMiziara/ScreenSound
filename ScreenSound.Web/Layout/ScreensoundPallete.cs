using MudBlazor;
using MudBlazor.Utilities;

namespace ScreenSound.Web.Layout;

public sealed class ScreendsoundPallete : PaletteDark
{
    private ScreendsoundPallete()
    {
        Primary = new MudColor("#9B5CFF");
        Secondary = new MudColor("#15121F");
        Tertiary = new MudColor("#23212E1");
    }


    public static ScreendsoundPallete CreatePallete => new();
}