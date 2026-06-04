using MCG.CREO_Tools.CutLengthApp.View;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.CutLengthApp.Exceptions;

namespace MCG.CREO_Tools.CutLengthApp.ViewModel
{
    public class CutLengthCutUpdatePartViewModel : ObservableObject, ICutLengthCutUpdatePartViewModel
    {
        #region [REGION] Properties from Interface
        private CutLengthCutPart _PartItem;
        public CutLengthCutPart PartItem
        {
            get { return _PartItem; }
            set
            {
                if (this._PartItem != value)
                {
                    this._PartItem = value;
                    OnPropertyChanged();
                }
            }
        }

        public MessageBoxResult Return { get; set; } = MessageBoxResult.Cancel;
        #endregion

        #region [REGION] Internal variables
        #endregion

        #region [REGION] Commands
        public ICommand CommandCreateUpdatePart { get => new RelayCommand(() => ExecuteCreateUpdatePart()); }
        #endregion

        #region [REGION] Init
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteCreateUpdatePart(bool InAsynch = false)
        {
            try
            {
                Return = MessageBoxResult.Yes;
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        #endregion
    }
}
