using Microsoft.AspNetCore.Components;

namespace CloudScribe.Blazor.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    bool _drawerOpen = true;

    void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }
}