using MCG.Tools.NumberingTool.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.NumberingTool.Exceptions;
using MCG.CommonLib.Services.Statics;

namespace MCG.Tools.NumberingTool.ViewModel
{
    public class NumberingToolUpdateCreateViewModel : ObservableObject, INumberingToolUpdateCreateViewModel
    {
        #region [REGION] Properties from Interface
        private string _LabelBtCreateUpdate = "OK";
        public string LabelBtCreateUpdate
        {
            get { return _LabelBtCreateUpdate; }
            set
            {
                if (this._LabelBtCreateUpdate != value)
                {
                    this._LabelBtCreateUpdate = value;
                    OnPropertyChanged();
                }
            }
        }

        private NumberingToolItem _CurrentItem;
        public NumberingToolItem CurrentItem
        {
            get { return _CurrentItem; }
            set
            {
                if (this._CurrentItem != value)
                {
                    this._CurrentItem = value;
                    OnPropertyChanged();
                }

            }
        }

        private NumberingToolTemplate _SelectedNumberingTemplate;
        public NumberingToolTemplate SelectedNumberingTemplate
        {
            get { return _SelectedNumberingTemplate; }
            set
            {
                if (this._SelectedNumberingTemplate != value)
                {
                    this._SelectedNumberingTemplate = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> ListProduct { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListFormat { get; set; } = new ObservableCollection<string>();

        private bool _IsUpdateShown = false;
        public bool IsUpdateShown
        {
            get { return _IsUpdateShown; }
            set
            {
                if (this._IsUpdateShown != value)
                {
                    this._IsUpdateShown = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsDetailShown = true;
        public bool IsDetailShown
        {
            get { return _IsDetailShown; }
            set
            {
                if (this._IsDetailShown != value)
                {
                    this._IsDetailShown = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public bool IsCreateUpdateNumber { get; set; } = false;
        public bool IsNewNumber { get; set; } = true;
        public string RangeNumber { get; set; } = "Unknown";
        #endregion

        #region [REGION] Events
        public event EventHandler CreateNumberEvent;
        public void RaiseCreateNumberEvent()
        {
            try
            {
                CreateNumberEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UpdateNumberEvent;
        public void RaiseUpdateNumberEvent()
        {
            try
            {
                UpdateNumberEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Commands
        public ICommand CommandCreateNumber { get => new RelayCommand(() => ExecuteCreateNumber()); }
        public ICommand CommandCancel { get => new RelayCommand(() => { Application.Current.Windows.OfType<Window>().First((item) => item.IsActive).Close(); }); }
        public ICommand CommandUpdateNumber { get => new RelayCommand(() => ExecuteUpdateNumber()); }
        #endregion

        #region [REGION] Init
        public void SetNumberingToolUpdateCreateViewModelProperties(bool CurrentIsNewNumber, NumberingToolTemplate CurrentSelectedNumberingTemplate, List<string> CurrentSearchProductList, List<string> CurrentListFormat, NumberingToolItem AlreadyCreatedItem = null, bool CurrentIsDetailShown = true)
        {
            try
            {
                TraceLog.AddTraceLog($"Enter NumberingToolUpdateCreateViewModel");
                IsDetailShown = CurrentIsDetailShown;
                if (AlreadyCreatedItem != null)
                    CurrentItem = AlreadyCreatedItem;
                else
                    CurrentItem = new NumberingToolItem()
                    {
                        CreatedBy = McgActiveDirectoryTools.GetWindowsSessionUserShortName(),
                        CreatedById = Environment.UserName,
                        Format = "N/A",
                        Product = "N/A",
                        CreatedOn = DateTime.Today,
                        Description = " "
                    };

                IsNewNumber = CurrentIsNewNumber;
                IsUpdateShown = !CurrentIsNewNumber;
                SelectedNumberingTemplate = CurrentSelectedNumberingTemplate;
                if (CurrentSearchProductList != null)
                    foreach (var item in CurrentSearchProductList)
                        ListProduct.Add(item);
                if (CurrentListFormat != null)
                    foreach (var item in CurrentListFormat)
                        ListFormat.Add(item);
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteCreateNumber()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteCreateNumber Action");
                IsCreateUpdateNumber = true;
                RaiseCreateNumberEvent();
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateNumber()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteUpdateNumber Action");
                IsCreateUpdateNumber = true;
                RaiseUpdateNumberEvent();
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
