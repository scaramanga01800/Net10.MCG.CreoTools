using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    interface IMassUpdateAttributeItem
    {
        bool IsSelected { get; set; }
        string PartNumber { get; set; }
        string PTC_COMMON_NAME { get; set; }
        string BasePartNumber { get; set; }
        string Status { get; set; }
        bool IsUpdated { get; set; }
        bool IsPtcCommonNameModifiable { get; set; }
        bool IsBasePartNumberFound { get; set; }
        bool IsCheckedIn { get; set; }
        bool IsCheckedOut { get; set; }
        bool IsLocallyModified { get; set; }
        bool IsReadOnly { get; set; }

        ObservableCollection<string> WebtermList { get; set; }

        ObservableCollection<string> ListGroup { get; set; }
        ObservableCollection<string> ListSubGroup { get; set; }
        ObservableCollection<string> ListBrand { get; set; }
        ObservableCollection<string> ListOption { get; set; }

        string SelectedBrand { get; set; }
        string SelectedGroup { get; set; }
        string SelectedSubGroup { get; set; }
        string SelectedOption { get; set; }


        event EventHandler IsSelectedEvent;
    }
}
