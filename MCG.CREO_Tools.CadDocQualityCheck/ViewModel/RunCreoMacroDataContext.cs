using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.CadDocQualityCheck.View;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class RunCreoMacroDataContext : ObservableObject, IRunCreoMacroDataContext
    {
        private bool _ShowActionButton = true;
        public bool IsEnabledActionButton
        {
            get { return _ShowActionButton; }
            set
            {
                if (this._ShowActionButton != value)
                {
                    this._ShowActionButton = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsEnabledCreo = false;
        public bool IsEnabledCreo
        {
            get { return _IsEnabledCreo; }
            set
            {
                if (this._IsEnabledCreo != value)
                {
                    this._IsEnabledCreo = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<CadDocQualityCheckItem> ShownCadModels { get; set; } = new ObservableCollection<CadDocQualityCheckItem>();

        private string _Macro;
        public string Macro
        {
            get { return _Macro; }
            set
            {
                if (this._Macro != value)
                {
                    this._Macro = value;
                    OnPropertyChanged();
                }

            }
        }

    }
}
