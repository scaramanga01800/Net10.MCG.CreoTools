using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using MCG.CREO_Tools.QuickSearch.Exceptions;
using MCG.CREO_Tools.QuickSearch.View;
using System.Windows.Input;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchWindowClassSubClassFromNumberViewModel : ObservableObject, IQuickSearchWindowClassSubClassFromNumberViewModel
    {
        #region [REGION] Properties from Interface

        private string _Number;
        public string Number
        {
            get { return this._Number; }
            set
            {
                if (this._Number != value)
                {
                    this._Number = value;
                    OnPropertyChanged();
                }
            }
        }

        private QuickSearchShortCutViewModel _ClassSubClass;
        public QuickSearchShortCutViewModel ClassSubClass
        {
            get { return _ClassSubClass; }
            set
            {
                if (this._ClassSubClass != value)
                {
                    this._ClassSubClass = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _IsClassSubFound = false;
        public bool IsClassSubFound
        {
            get { return this._IsClassSubFound; }
            set
            {
                if (this._IsClassSubFound != value)
                {
                    this._IsClassSubFound = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Internal variables
        private List<string> listStandard { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandOpenClassSubClass { get => new RelayCommand(() => ExecuteOpenClassSubClass()); }
        public ICommand CommandSearchClassSubClass { get => new RelayCommand(() => ExecuteSearchClassSubClass()); }
        public ICommand CommandClose { get => new RelayCommand(() => RaiseCloseEvent()); }
        #endregion

        #region [REGION] Events
        public event EventHandler OpenClassSubClassEvent;

        public void RaiseOpenClassSubClassEvent()
        {
            try
            {
                OpenClassSubClassEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }


        public event EventHandler CloseEvent;

        public void RaiseCloseEvent()
        {
            try
            {
                CloseEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region [REGION] Init
        private readonly IMcgToolDictionary _mcgToolDictionary;
        private readonly IQuickSearchService _IQuickSearchService;

        public QuickSearchWindowClassSubClassFromNumberViewModel(IMcgToolDictionary mcgToolDictionary,
                                                                 IQuickSearchService quickSearchService)
        {
            try
            {
                _mcgToolDictionary = mcgToolDictionary;
                _IQuickSearchService = quickSearchService;
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void SetProperties(List<string> listStdShown)
        {
            listStandard = listStdShown;
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteOpenClassSubClass()
        {
            try
            {
                if (ClassSubClass != null)
                    RaiseOpenClassSubClassEvent();
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSearchClassSubClass()
        {
            try
            {
                ClassSubClass = null;


                var CurrentPart = _IQuickSearchService.GetOnePartByRecPart(Number);
                if (CurrentPart != null)
                {
                    var CurrentSubClass = _IQuickSearchService.GetPartSubClasses(CurrentPart.Idsubclass, listStandard).FirstOrDefault();
                    if (CurrentSubClass != null)
                    {

                        var CurrentClass = _IQuickSearchService.GetOnePartClass(CurrentSubClass.Idclass);
                        if (CurrentClass != null)
                        {
                            IsClassSubFound = true;
                            ClassSubClass = new QuickSearchShortCutViewModel()
                            {
                                Class = _mcgToolDictionary.GetTerm(CurrentClass.Idclassname),
                                SubClass = _mcgToolDictionary.GetTerm(CurrentSubClass.Subclassname),
                                ParentData = new QuickSearchShortCutData()
                                {
                                    SubClass = CurrentSubClass.Idsubclass,
                                    Class = CurrentClass.Idclassname
                                }
                            };
                        }
                        else
                            IsClassSubFound = false;
                    }
                    else
                        IsClassSubFound = false;
                }
                else
                    IsClassSubFound = false;

            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
