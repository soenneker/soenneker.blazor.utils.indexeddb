namespace Soenneker.Blazor.Utils.IndexedDb.Demo.Pages;

internal sealed class DashboardPreference
{
    public string Theme { get; set; } = "Dark";
    public int PageSize { get; set; } = 25;
    public bool CompactMode { get; set; } = true;
    public bool ShowInsights { get; set; } = true;

    public static DashboardPreference CreateDefault()
    {
        return new DashboardPreference();
    }
}
