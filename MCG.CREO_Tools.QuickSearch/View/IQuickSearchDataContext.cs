using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace MCG.CREO_Tools.QuickSearch.View
{
    public interface IQuickSearchDataContext
    {
        bool IsCreoEnable { get; set; }

        bool CRWLocalShown { get; set; }
        bool DGLocalShown { get; set; }
        bool SGLocalShown { get; set; }
        bool TWRLocalShown { get; set; }
        bool MFGTWRLocalShown { get; set; }
        bool STDGlobalShown { get; set; }

        bool CRWLocalEnabled { get; set; }
        bool DGLocalEnabled { get; set; }
        bool SGLocalEnabled { get; set; }
        bool TWRLocalEnabled { get; set; }
        bool MFGTWRLocalEnabled { get; set; }
        bool STDGlobalEnabled { get; set; }

        ObservableCollection<QuickSearchPartClass> ListClass { get; set; }
        QuickSearchPartClass SelectedClassItem { get; set; }
        ObservableCollection<QuickSearchPartSubClass> ListSubClass { get; set; }
        QuickSearchPartSubClass SelectedSubClassItem { get; set; }

        string RefDocument { get; set; }

        List<QuickSearchExtraCompMenu> ListExtraMenu { get; set; }
        bool IsExtraComponentShown { get; set; }
        bool IsExtraComponentPossible { get; set; }
        bool IsPartPictureShown { get; set; }
        BitmapImage MainPictureShown { get; set; }
        BitmapImage ExtraPictureShown { get; set; }
        BitmapImage PartPictureShown { get; set; }

        ObservableCollection<QuickSearchPart> ListPartItemShown { get; set; }
        QuickSearchPart SelectedPartItem { get; set; }

        ObservableCollection<SapPlant> ListSapPlant { get; set; }
        SapPlant SelectedSapPlant { get; set; }
        bool IsMsgSearchSap { get; set; }

        bool ShowSapCostVolumeInfo { get; set; }

        ObservableCollection<QuickSearchShortCutViewModel> ListShortCut { get; set; }

        bool IsEditMode { get; set; }
        bool IsAdminToolsEnabled { get; set; }

        event EventHandler SubClassChangedEvent;
        event EventHandler ShortCutChangedEvent;

        bool IsRefDocHtmlLink { get; set; }
    }
}
