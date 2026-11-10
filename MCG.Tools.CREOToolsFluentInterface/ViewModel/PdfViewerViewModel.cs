using CommunityToolkit.Mvvm.ComponentModel;

namespace MCG.Tools.CREOToolsFluentInterface.ViewModel
{
    /// <summary>
    /// ViewModel du viewer PDF (PdfViewerView). Fournit la Source du document à afficher.
    /// </summary>
    public partial class PdfViewerViewModel : ObservableObject
    {
        [ObservableProperty] private Uri? _source;
    }
}
