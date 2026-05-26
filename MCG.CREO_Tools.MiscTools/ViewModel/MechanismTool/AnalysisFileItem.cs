using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.MechanismTool;

namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class AnalysisFileItem : ObservableObject, IAnalysisFileItem
    {
        private string _FileName;
        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (this._FileName != value)
                {
                    this._FileName = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Status;
        public string Status
        {
            get { return _Status; }
            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    OnPropertyChanged();
                }

            }
        }
    }
}
