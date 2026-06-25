using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.Main;

namespace MCG.Tools.CREOToolsFluentInterface.ViewModel
{

    public partial class CREOToolsLanguageSelection : ObservableObject
    {
        [ObservableProperty] private MCGLanguage _language;

        [ObservableProperty] private bool _isSelected;

        public event EventHandler IsSelectedEvent;

        partial void OnIsSelectedChanged(bool value)
            => IsSelectedEvent?.Invoke(this, EventArgs.Empty);
    }
}
