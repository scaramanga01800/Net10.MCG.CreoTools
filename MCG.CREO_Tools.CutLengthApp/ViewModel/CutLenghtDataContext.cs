using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.CutLengthApp.Exceptions;
using MCG.CREO_Tools.CutLengthApp.View;
using pfcls;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.CutLengthApp.ViewModel
{
    public class CutLenghtDataContext : ObservableObject, ICutLenghtDataContext
    {
        #region [REGION] Properties from Interface
        public ObservableCollection<CutLengthType> ListCutLengthType { get; set; } = new ObservableCollection<CutLengthType>();
        public ObservableCollection<CutLengthCutPart> CurrentListPartNumber { get; set; } = new ObservableCollection<CutLengthCutPart>();

        private CutLengthType _SelectedCutLengthType;
        public CutLengthType SelectedCutLengthType
        {
            get { return _SelectedCutLengthType; }
            set
            {
                if (this._SelectedCutLengthType != value)
                {
                    this._SelectedCutLengthType = value;
                    UdpateCurrentListPartNumber();
                    OnPropertyChanged();
                }

            }
        }

        private CutLengthCutPart _SelectedCutLengthPart;
        public CutLengthCutPart SelectedCutLengthPart
        {
            get { return _SelectedCutLengthPart; }
            set
            {
                if (this._SelectedCutLengthPart != value)
                {
                    this._SelectedCutLengthPart = value;
                    OnPropertyChanged();
                }

            }
        }

        private double _Quantity;
        public double Quantity
        {
            get { return _Quantity; }
            set
            {
                if (this._Quantity != value)
                {
                    this._Quantity = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _ActiveModelFileName;
        public string ActiveModelFileName
        {
            get { return _ActiveModelFileName; }
            set
            {
                if (this._ActiveModelFileName != value)
                {
                    this._ActiveModelFileName = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsCreoEnable = false;
        public bool IsCreoEnable
        {
            get { return _IsCreoEnable; }
            set
            {
                if (this._IsCreoEnable != value)
                {
                    this._IsCreoEnable = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _BulkSelected = true;
        public bool BulkSelected
        {
            get { return _BulkSelected; }
            set
            {
                if (this._BulkSelected != value)
                {
                    this._BulkSelected = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ThreeDSelected = true;
        public bool ThreeDSelected
        {
            get { return _ThreeDSelected; }
            set
            {
                if (this._ThreeDSelected != value)
                {
                    this._ThreeDSelected = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ThreeDIsEnable = true;
        public bool ThreeDIsEnable
        {
            get { return _ThreeDIsEnable; }
            set
            {
                if (this._ThreeDIsEnable != value)
                {
                    this._ThreeDIsEnable = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsEditMode = false;
        public bool IsEditMode
        {
            get { return _IsEditMode; }
            set
            {
                if (this._IsEditMode != value)
                {
                    this._IsEditMode = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAdminToolsEnabled = false;
        public bool IsAdminToolsEnabled
        {
            get { return _IsAdminToolsEnabled; }
            set
            {
                if (this._IsAdminToolsEnabled != value)
                {
                    this._IsAdminToolsEnabled = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public List<CutLengthCutPart> CompleteListPartNumber { get; set; }
        public IpfcModel ActiveModel { get; set; }
        public string CurrentCutLenghtFileName { get; set; }
        #endregion

        #region [REGION] Misc
        public void UdpateCurrentListPartNumber()
        {
            try
            {
                if (SelectedCutLengthType != null)
                {
                    CurrentListPartNumber.Clear();
                    List<CutLengthCutPart> tempList;
                    if (SelectedCutLengthType.ClassName == "All Types")
                        tempList = CompleteListPartNumber.ToList();
                    else
                        tempList = CompleteListPartNumber.Where((item) => item.IdClass == SelectedCutLengthType.IdClass).ToList();

                    foreach (var part in tempList)
                        CurrentListPartNumber.Add(part);

                    if (SelectedCutLengthType.BulkOnly != null && SelectedCutLengthType.BulkOnly.ToUpper() == "TRUE")
                    {
                        ThreeDIsEnable = false;
                        BulkSelected = true;
                        ThreeDSelected = false;
                    }
                    else
                    {
                        ThreeDIsEnable = true;
                        ThreeDSelected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CutLengthException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
