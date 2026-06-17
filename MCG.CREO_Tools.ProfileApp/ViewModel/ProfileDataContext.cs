using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CREO_Tools.ProfileApp.Exceptions;
using MCG.CREO_Tools.ProfileApp.View;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace MCG.CREO_Tools.ProfileApp.ViewModel
{
    public class ProfileDataContext : ObservableObject, IProfileDataContext
    {
        #region [REGION] Properties from Interface
        public ObservableCollection<ProfileTypeItem> ListProfileType { get; set; } = new ObservableCollection<ProfileTypeItem>();
        private ProfileTypeItem _CurrentProfileType;
        public ProfileTypeItem CurrentProfileType
        {
            get { return _CurrentProfileType; }
            set
            {
                if (this._CurrentProfileType != value)
                {
                    this._CurrentProfileType = value;
                    OnPropertyChanged();
                    RaiseChangeProfileTypeEvent();
                }

            }
        }

        private BitmapImage _ProfileTypeImage;
        public BitmapImage ProfileTypeImage
        {
            get { return _ProfileTypeImage; }
            set
            {
                if (this._ProfileTypeImage != value)
                {
                    this._ProfileTypeImage = value;
                    OnPropertyChanged();
                }

            }
        }

        private byte[] _ProfileTypeImageFromDb;
        public byte[]  ProfileTypeImageFromDb
        {
            get { return _ProfileTypeImageFromDb; }
            set
            {
                if (this._ProfileTypeImageFromDb != value)
                {
                    this._ProfileTypeImageFromDb = value;
                    OnPropertyChanged();
                }

            }
        }


        public ObservableCollection<string> ListMaterial { get; set; } = new ObservableCollection<string>();
        private string _SelectedMaterial;
        public string SelectedMaterial
        {
            get { return _SelectedMaterial; }
            set
            {
                if (this._SelectedMaterial != value)
                {
                    this._SelectedMaterial = value;
                    OnPropertyChanged();
                    RaiseChangeMaterialEvent();
                }

            }
        }

        public ObservableCollection<string> ListGrpCreator { get; set; } = new ObservableCollection<string>();
        private string _SelectedGrpCreator;
        public string SelectedGrpCreator
        {
            get { return _SelectedGrpCreator; }
            set
            {
                if (this._SelectedGrpCreator != value)
                {
                    this._SelectedGrpCreator = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<ProfileDrwLocation> ListDrwLocation { get; set; } = new ObservableCollection<ProfileDrwLocation>();
        private ProfileDrwLocation _SelectedDrwLocation;
        public ProfileDrwLocation SelectedDrwLocation
        {
            get { return _SelectedDrwLocation; }
            set
            {
                if (this._SelectedDrwLocation != value)
                {
                    this._SelectedDrwLocation = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _CurrentPartNumber;
        public string CurrentPartNumber
        {
            get { return _CurrentPartNumber; }
            set
            {
                if (this._CurrentPartNumber != value)
                {
                    this._CurrentPartNumber = value;
                    OnPropertyChanged();
                }

            }
        }

        private double _CurrentLength = 100;
        public double CurrentLength
        {
            get { return _CurrentLength; }
            set
            {
                if (this._CurrentLength != value)
                {
                    this._CurrentLength = value;
                    UpdateRatio();
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsDrwBrokenView;
        public bool IsDrwBrokenView
        {
            get { return _IsDrwBrokenView; }
            set
            {
                if (this._IsDrwBrokenView != value)
                {
                    this._IsDrwBrokenView = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<ProfileGenericItem> ListProfileShown { get; set; } = new ObservableCollection<ProfileGenericItem>();

        private ProfileGenericItem _SelectedItem;
        public ProfileGenericItem SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (this._SelectedItem != value)
                {
                    this._SelectedItem = value;
                    UpdateRatio();
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

        private bool _ActionInProgress = false;
        public bool ActionInProgress
        {
            get { return _ActionInProgress; }
            set
            {
                if (this._ActionInProgress != value)
                {
                    this._ActionInProgress = value;
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
        public List<DrwScaleItem> ListDrwScale { get; set; }
        public List<Profilegeneric> AllListProfileGeneric { get; set; }
        public List<Profiletype> AllListProfileType { get; set; }
        public List<ProfileGenericItem> ListProfileGenericFromSelectedType { get; set; }
        public double MainDrawingScale { get; set; }
        public double ThreeDViewScale { get; set; }
        #endregion

        #region [REGION] Events
        public event EventHandler ChangeProfileTypeEvent;
        public void RaiseChangeProfileTypeEvent()
        {
            try
            {
                ChangeProfileTypeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler ChangeMaterialEvent;
        public void RaiseChangeMaterialEvent()
        {
            try
            {
                ChangeMaterialEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Misc
        private void UpdateRatio()
        {
            try
            {
                if (SelectedItem != null)
                {
                    MainDrawingScale = GetDrwScale(SelectedItem.OrigProfileGeneric.Height.Value, 1).Scale;

                    double Lenght3DView = (CurrentLength + SelectedItem.OrigProfileGeneric.Height.Value) * 0.71;
                    double Height3DView = CurrentLength * 0.71 / 1.75 + SelectedItem.OrigProfileGeneric.Width.Value * 1.2;

                    ThreeDViewScale = GetDrwScale(Height3DView, Lenght3DView, false).Scale;

                    // auto select type of drawing, with or without broken view
                    // if lenght > 2xheigth --> broken view
                    if (CurrentLength / SelectedItem.OrigProfileGeneric.Height.Value > 2)
                        IsDrwBrokenView = true;
                    else
                        IsDrwBrokenView = false;
                }
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }

        private DrwScaleItem GetDrwScale(double Height, double width, bool MainView = true)
        {
            try
            {
                DrwScaleItem CurrentGetDrwScale = null/* TODO Change to default(_) if this is not a reference type */;
                double TestHeight = 1;
                double TestWidth = 1;
                double TestScaleHeight;
                double TestScaleWidth;
                bool IsFound = false;

                var TempListDrwScale = ListDrwScale.OrderByDescending((item) => item.Scale);

                foreach (var aDrwScale in TempListDrwScale)
                {
                    if (MainView)
                    {
                        TestHeight = aDrwScale.MainViewHeight;
                        TestWidth = aDrwScale.MainViewWidth;
                    }
                    else
                    {
                        TestHeight = aDrwScale.IsoViewHeight;
                        TestWidth = aDrwScale.IsoViewWidth;
                    }
                    TestScaleHeight = TestHeight / Height;
                    TestScaleWidth = TestWidth / width;

                    if (TestScaleHeight < TestScaleWidth)
                    {
                        if (TestScaleHeight >= aDrwScale.Scale)
                        {
                            IsFound = true;
                            CurrentGetDrwScale = aDrwScale;
                            break;
                        }
                    }
                    else if (TestScaleWidth >= aDrwScale.Scale)
                    {
                        IsFound = true;
                        CurrentGetDrwScale = aDrwScale;
                        break;
                    }
                }

                if (!IsFound)
                    CurrentGetDrwScale = ListDrwScale.FirstOrDefault((item) => item.Scale == ListDrwScale.Min((scale) => scale.Scale));

                return CurrentGetDrwScale;
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
