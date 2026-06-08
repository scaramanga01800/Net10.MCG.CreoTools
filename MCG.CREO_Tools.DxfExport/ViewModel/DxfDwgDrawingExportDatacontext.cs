using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.DxfExport.View;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.DxfExport.ViewModel
{
    public class DxfDwgDrawingExportDatacontext : ObservableObject, IDxfDwgDrawingExportDatacontext
    {
        #region [REGION] Properties from Interface
        private string _CurrentFolder;
        public string CurrentFolder
        {
            get { return this._CurrentFolder; }
            set
            {
                if (this._CurrentFolder != value)
                {
                    this._CurrentFolder = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _CurrentFileName;
        public string CurrentFileName
        {
            get { return this._CurrentFileName; }
            set
            {
                if (this._CurrentFileName != value)
                {
                    this._CurrentFileName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _DrawingTemplate;
        public string DrawingTemplate
        {
            get { return _DrawingTemplate; }
            set
            {
                if (this._DrawingTemplate != value)
                {
                    this._DrawingTemplate = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<DxfExportItem> ListItems { get; set; } = new ObservableCollection<DxfExportItem>();

        private DxfExportItem _SelectedItem;
        public DxfExportItem SelectedItem
        {
            get { return this._SelectedItem; }
            set
            {
                if (this._SelectedItem != value)
                {
                    this._SelectedItem = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _StatusBarMessage;
        public string StatusBarMessage
        {
            get { return this._StatusBarMessage; }
            set
            {
                if (this._StatusBarMessage != value)
                {
                    this._StatusBarMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsCreoEnable = false;
        public bool IsCreoEnable
        {
            get { return this._IsCreoEnable; }
            set
            {
                if (this._IsCreoEnable != value)
                {
                    this._IsCreoEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsDxfSelected = true;
        public bool IsDxfSelected
        {
            get { return _IsDxfSelected; }
            set
            {
                if (this._IsDxfSelected != value)
                {
                    this._IsDxfSelected = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsDwgSelected = true;
        public bool IsDwgSelected
        {
            get { return _IsDwgSelected; }
            set
            {
                if (this._IsDwgSelected != value)
                {
                    this._IsDwgSelected = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion
    }
}
