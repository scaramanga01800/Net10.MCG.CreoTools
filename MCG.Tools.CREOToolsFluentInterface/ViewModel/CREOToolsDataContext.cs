using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.WpfComponent;
using MCG.CommonLib.WpfComponent.Models;
using MCG.Tools.CREOToolsFluentInterface.View;
using System.Collections.ObjectModel;

namespace MCG.Tools.CREOToolsFluentInterface.ViewModel
{
    public partial class CREOToolsDataContext : ObservableObject, ICREOToolsDataContext
    {
        #region [REGION] Static data

        private static readonly IReadOnlyList<string> AvailableColorSchemes = new[]
        {
         "Blue", "Red", "Green", "Purple", "Orange", "Lime", "Emerald", "Teal",
         "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson",
         "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna"
         };

        #endregion

        #region [REGION] Languages

        [ObservableProperty] private CREOToolsLanguageSelection _langCn;
        [ObservableProperty] private CREOToolsLanguageSelection _langEn;
        [ObservableProperty] private CREOToolsLanguageSelection _langFr;
        [ObservableProperty] private CREOToolsLanguageSelection _langDe;

        public CREOToolsLanguageSelection CurrentLang { get; set; }

        #endregion

        #region [REGION] App availability / visibility

        [ObservableProperty] private CREOToolsAppAvailability _appAvailable;
        [ObservableProperty] private CREOToolsAppAvailability _appVisible;

        #endregion

        #region [REGION] UI States

        [ObservableProperty] private bool _isScrollingTextVisible = true;
        [ObservableProperty] private bool _isCreoConnected;
        [ObservableProperty] private bool _isPleaseWaitShown;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLight))]
        private bool _isDark;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDark))]
        private bool _isLight = true;

        partial void OnIsDarkChanged(bool value)
        {
            if (value && IsLight) IsLight = false;
            RaiseColorInterfaceChangeEvent();
        }

        partial void OnIsLightChanged(bool value)
        {
            if (value && IsDark) IsDark = false;
            RaiseColorInterfaceChangeEvent();
        }

        #endregion

        #region [REGION] Theme & Font

        public ObservableCollection<string> ListColorSchemes { get; }
            = new(AvailableColorSchemes);

        public ObservableCollection<string> ListFont { get; set; } = new();

        [ObservableProperty]
        private string _selectedColorScheme;

        [ObservableProperty]
        private string _selectedFont;


        partial void OnSelectedColorSchemeChanged(string value)
        {
            RaiseColorInterfaceChangeEvent();
        }

        partial void OnSelectedFontChanged(string value)
        {
            RaiseFontInterfaceChangeEvent();
        }
        #endregion

        #region [REGION] Scrolling text

        [ObservableProperty] private string _scrollingText;

        private static string BuildScrollingText()
        {
            var now = DateTime.Now;
            return $"Welcome to Engineering Hub. {now:dd/MM/yyyy} at {now:HH:mm}";
        }

        #endregion

        #region [REGION] Constructor

        public CREOToolsDataContext()
        {
            try
            {
                SelectedColorScheme = AvailableColorSchemes.FirstOrDefault();
                ScrollingText = BuildScrollingText();
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(GetType().Name, ex);
            }
        }

        #endregion

        #region [REGION] Events
        public event EventHandler ColorInterfaceChangeEvent;
        public event EventHandler FontInterfaceChangeEvent;

        public void RaiseColorInterfaceChangeEvent()
            => ColorInterfaceChangeEvent?.Invoke(this, EventArgs.Empty);

        public void RaiseFontInterfaceChangeEvent()
            => FontInterfaceChangeEvent?.Invoke(this, EventArgs.Empty);
        #endregion
    }
}
