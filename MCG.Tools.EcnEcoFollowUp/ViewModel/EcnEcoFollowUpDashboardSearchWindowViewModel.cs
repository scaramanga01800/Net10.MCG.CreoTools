using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.View;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MCG.Tools.EcnEcoFollowUp.ViewModel
{
    public class EcnEcoFollowUpDashboardSearchWindowViewModel : ObservableObject, IEcnEcoFollowUpDashboardSearchWindowViewModel
    {
        #region [REGION] Properties from Interface
        public EcnEcoFollowUpDashboardSearchWindow ParentWindow { set; get; }

        private string _CreatedByFullName = "";
        public string CreatedByFullName
        {
            get { return this._CreatedByFullName; }
            set
            {
                if (this._CreatedByFullName != value)
                {
                    this._CreatedByFullName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _CreatedById = "";
        public string CreatedById
        {
            get { return this._CreatedById; }
            set
            {
                if (this._CreatedById != value)
                {
                    this._CreatedById = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _DashboardName = "";
        public string DashboardName
        {
            get { return this._DashboardName; }
            set
            {
                if (this._DashboardName != value)
                {
                    this._DashboardName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _DashboardID = "";
        public string DashboardID
        {
            get { return this._DashboardID; }
            set
            {
                if (this._DashboardID != value)
                {
                    if (value.Length <= 6)
                    {
                        this._DashboardID = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public ObservableCollection<EFU_DashboardItem> ListSearchedDashboard { set; get; } = new ObservableCollection<EFU_DashboardItem>();
        #endregion

        #region [REGION] Properties not from Interface
        public List<EFU_DashboardItem> ListSelectedDashboard { get; set; }
        private readonly IEcnEcoFollowUpService _ecnEcoFollowUpService;
        public bool AddDashboard { get; set; } = false;
        #endregion

        #region [REGION] Commands
        public ICommand CommandSearchDashboard { get => new RelayCommand(() => ExecuteSearchDashboard()); }
        public ICommand CommandAddDashboard { get => new RelayCommand(() => ExecuteAddDashboard()); }
        public ICommand CommandClose { get => new RelayCommand(() => ExecuteClose()); }
        #endregion

        #region [REGION] Init
        public EcnEcoFollowUpDashboardSearchWindowViewModel(IEcnEcoFollowUpService ecnEcoFollowUpService)
        {
            try
            {
                _ecnEcoFollowUpService = ecnEcoFollowUpService;
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteSearchDashboard()
        {
            try
            {
                ListSearchedDashboard.Clear();

                Regex DashboardNameRegex = new Regex(DashboardName, RegexOptions.IgnoreCase);
                Regex CreatedByIdRegex = new Regex(CreatedById, RegexOptions.IgnoreCase);
                Regex CreatedByFullNameRegex = new Regex(CreatedByFullName, RegexOptions.IgnoreCase);
                Regex DashboardIDRegex = new Regex(DashboardID, RegexOptions.IgnoreCase);

                var ListDashboard = _ecnEcoFollowUpService.GetAllActiveEcnEcoDashboard();
                //var ListDashboard = CreoEntities.ECNECODASHBOARD.Where((item) => item.ISACTIVE.Value).ToList();

                ListDashboard = ListDashboard.Where((item) => DashboardNameRegex.IsMatch(item.Dashboardname)
                                                  && CreatedByIdRegex.IsMatch(item.Createdby)
                                                  && CreatedByFullNameRegex.IsMatch(item.Createdbyfullname)
                                                  && DashboardIDRegex.IsMatch(item.Dashboardid.ToString("000000"))
                                                  && item.Isshared.Value).ToList();
                if (ListDashboard.Count == 0)
                    MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgSearchDashboardEmpty"), McgWpfTools.GetStringResource("EFU_TitleSearchDashboard"), MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    foreach (var item in ListDashboard)
                        ListSearchedDashboard.Add(new EFU_DashboardItem()
                        {
                            CreatedBy = item.Createdbyfullname,
                            CreatedOn = item.Createdon.HasValue ? item.Createdon.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                            Name = item.Dashboardname,
                            Id = item.Dashboardid.ToString("000000")
                        });
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddDashboard()
        {
            try
            {
                ListSelectedDashboard = ListSearchedDashboard.Where((item) => item.IsSelected).ToList();

                if (ListSelectedDashboard.Count == 0)
                    MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgSlectedDashboardEmpty"), McgWpfTools.GetStringResource("EFU_TitleSearchDashboard"), MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                {
                    AddDashboard = true;
                    ParentWindow.Close();
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteClose()
        {
            try
            {
                AddDashboard = false;
                ParentWindow.Close();
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
