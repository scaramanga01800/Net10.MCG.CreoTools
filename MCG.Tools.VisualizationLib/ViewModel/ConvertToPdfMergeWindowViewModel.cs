using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MCG.Tools.VisualizationLib.ViewModel
{
    public class ConvertToPdfMergeWindowViewModel : ObservableObject, IConvertToPdfMergeWindowViewModel
    {

        #region [REGION] Properties from Interface
        public ObservableCollection<ConvertToPdfItem> ListFiles { get; set; } = new ObservableCollection<ConvertToPdfItem>();

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
        #endregion

        #region [REGION] Internal variables
        private MessageBoxResult _Return = MessageBoxResult.Cancel;
        public MessageBoxResult Return
        {
            get { return _Return; }
            set { _Return = value; RaiseReturnEvent(); }
        }
        #endregion

        public event EventHandler ReturnEvent;
        public void RaiseReturnEvent()
        {
            try
            {
                ReturnEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        #region [REGION] Commands
        public ICommand CommandStartMerge { get => new RelayCommand(() => Return = MessageBoxResult.OK); }
        public ICommand CommandMoveUpParameter { get => new RelayCommand<ConvertToPdfItem>((param) => ExecuteMoveUpParameter(param)); }
        public ICommand CommandMoveDownParameter { get => new RelayCommand<ConvertToPdfItem>((param) => ExecuteMoveDownParameter(param)); }
        #endregion

        #region [REGION] Init
        public ConvertToPdfMergeWindowViewModel() { }

        public void SetConvertToPdfMergeWindowViewModelProperties(List<ConvertToPdfItem> pListFiles, string defaultFileName)
        {
            try
            {
                FileName = defaultFileName;
                if (pListFiles != null)
                {
                    int order = 0;
                    foreach (ConvertToPdfItem item in pListFiles)
                    {
                        item.Order = order;
                        ListFiles.Add(item);
                        order++;
                    }
                    ListFiles.CollectionChanged += ListFiles_CollectionChanged;
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ListFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                int index = 0;
                foreach (var item in ListFiles)
                    item.Order = index++;
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteMoveUpParameter(ConvertToPdfItem CurrentParam)
        {
            try
            {
                SwitchParameter(CurrentParam, -1);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMoveDownParameter(ConvertToPdfItem CurrentParam)
        {
            try
            {
                SwitchParameter(CurrentParam, +1);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        private void SwitchParameter(ConvertToPdfItem CurrentParam, int increment)
        {
            try
            {
                ListFiles.CollectionChanged -= ListFiles_CollectionChanged;

                ConvertToPdfItem TempParam = ListFiles.FirstOrDefault((param) => param.Order == CurrentParam.Order + increment);
                if (TempParam != null)
                {
                    TempParam.Order = CurrentParam.Order;
                    CurrentParam.Order += increment;
                    ReorderFiles();
                }
                ListFiles.CollectionChanged += ListFiles_CollectionChanged;
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }

        private void ReorderFiles(object sender = null, EventArgs e = null)
        {
            try
            {
                List<ConvertToPdfItem> TempListParam = ListFiles.OrderBy((param) => param.Order).ToList();
                if (TempListParam != null && TempListParam.Count > 0)
                {
                    int Index = 1;
                    ListFiles.Clear();

                    foreach (var param in TempListParam)
                    {
                        param.Order = Index;
                        ListFiles.Add(param);
                        Index++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
