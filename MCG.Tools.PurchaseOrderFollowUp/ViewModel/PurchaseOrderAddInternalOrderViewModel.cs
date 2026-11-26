using CommunityToolkit.Mvvm.ComponentModel;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    /// <summary>
    /// Données saisies dans la fenêtre d'ajout d'un ordre interne.
    /// </summary>
    public class PurchaseOrderAddInternalOrderViewModel : ObservableObject
    {
        private string _number = string.Empty;
        public string Number
        {
            get => _number;
            set
            {
                if (SetProperty(ref _number, value))
                    OnPropertyChanged(nameof(IsValid));
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                    OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(Number) && !string.IsNullOrWhiteSpace(Description);
    }
}
