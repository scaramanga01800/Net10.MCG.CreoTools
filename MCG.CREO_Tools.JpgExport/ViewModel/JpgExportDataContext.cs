using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.JpgExport.View;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.JpgExport.ViewModel
{
    public class JpgExportDataContext : ObservableObject, IJpgExportDataContext
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

        public ObservableCollection<JpgExportComboBoxValue> ListDisplayStyle { get; set; } = new ObservableCollection<JpgExportComboBoxValue>();
        public ObservableCollection<JpgExportComboBoxValue> ListView3D { get; set; } = new ObservableCollection<JpgExportComboBoxValue>();
        public ObservableCollection<JpgExportComboBoxValue> ListResolution { get; set; } = new ObservableCollection<JpgExportComboBoxValue>();

        private JpgExportComboBoxValue _SelectedView3D;
        public JpgExportComboBoxValue SelectedView3D
        {
            get { return this._SelectedView3D; }
            set
            {
                if (this._SelectedView3D != value)
                {
                    this._SelectedView3D = value;
                    OnPropertyChanged();
                }
            }
        }

        private JpgExportComboBoxValue _SelectedDisplayStyle;
        public JpgExportComboBoxValue SelectedDisplayStyle
        {
            get { return this._SelectedDisplayStyle; }
            set
            {
                if (this._SelectedDisplayStyle != value)
                {
                    this._SelectedDisplayStyle = value;
                    OnPropertyChanged();
                }
            }
        }

        private JpgExportComboBoxValue _SelectedResolution;
        public JpgExportComboBoxValue SelectedResolution
        {
            get { return this._SelectedResolution; }
            set
            {
                if (this._SelectedResolution != value)
                {
                    this._SelectedResolution = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<JpgExportItem> ListItems { get; set; } = new ObservableCollection<JpgExportItem>();

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

        private JpgExportItem _SelectedItem;
        public JpgExportItem SelectedItem
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
        #endregion
    }
}
