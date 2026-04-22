using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Services.Interfaces;
using MCG.CommonLib.WpfComponent.View.WindchillContextSelection;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.Exceptions;
using MCG.WindchillTools.ManageWTObject.View;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace MCG.WindchillTools.ManageWTObject.ViewModel
{
    public class CreateWtDocumentViewModel : ObservableObject, ICreateWtDocumentViewModel
    {
        #region [REGION] Properties from Interface
        private MgtWtObject _WtObject;
        public MgtWtObject WtObject
        {
            get { return _WtObject; }
            set
            {
                if (this._WtObject != value)
                {
                    this._WtObject = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> ListWindchillDocumentType { get; set; } = new ObservableCollection<string>();

        private string _SelectedWindchillDocumentType;
        public string SelectedWindchillDocumentType
        {
            get { return _SelectedWindchillDocumentType; }
            set
            {
                if (this._SelectedWindchillDocumentType != value)
                {
                    this._SelectedWindchillDocumentType = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> ListWindchillPartType { get; set; } = new ObservableCollection<string>();

        private string _SelectedWindchillPartType;
        public string SelectedWindchillPartType
        {
            get { return _SelectedWindchillPartType; }
            set
            {
                if (this._SelectedWindchillPartType != value)
                {
                    this._SelectedWindchillPartType = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> ListWebterm { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListWebtermLocal { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListGroup { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListBrand { get; set; } = new ObservableCollection<string>();

        private string _SelectedWebterm;
        public string SelectedWebterm
        {
            get { return _SelectedWebterm; }
            set
            {
                if (this._SelectedWebterm != value)
                {
                    this._SelectedWebterm = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _SelectedLocalWebterm;
        public string SelectedLocalWebterm
        {
            get { return _SelectedLocalWebterm; }
            set
            {
                if (this._SelectedLocalWebterm != value)
                {
                    this._SelectedLocalWebterm = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<WindchillContext> WindchillContextList { get; set; } = new ObservableCollection<WindchillContext>();
        #endregion

        #region [REGION] Internal variables
        private List<Webterm> AllWebterm { get; set; }
        private MCGLanguage SelectedLanguage { get; set; }
        private string WebtermDb { get; set; } = McgWpfTools.GetPropertiesFromMainApp<string>("WEBTERMDB");
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        #endregion

        #region [REGION] Commands
        public ICommand CommandSelectContext { get => new RelayCommand(() => ExecuteSelectContext()); }
        #endregion

        #region [REGION] Init
        public CreateWtDocumentViewModel(IMcgCommonLibWindowService mcgCommonLibWindowService)
        {
            _mcgCommonLibWindowService = mcgCommonLibWindowService;
        }

        public void SetCreateWtDocumentProperties(List<string> pListWindchillDocumentType,
                                                  List<string> pListWindchillPartType,
                                                  List<Webterm> pAllWebterm,
                                                  MCGLanguage LocalLanguage,
                                                  List<WindchillContext> pWindchillContextList,
                                                  List<string> pListGroup,
                                                  List<string> pListBrand)
        {
            try
            {
                if (pListWindchillDocumentType != null && pListWindchillPartType != null && pAllWebterm != null && pWindchillContextList != null)
                {
                    AllWebterm = pAllWebterm;
                    SelectedLanguage = LocalLanguage;
                    foreach (var item in pListWindchillDocumentType)
                        ListWindchillDocumentType.Add(item);

                    foreach (var item in pListWindchillPartType)
                        ListWindchillPartType.Add(item);

                    foreach (var term in pAllWebterm.OrderBy((item) => item.English))
                        ListWebterm.Add(term.English);

                    ListWebtermLocal.Clear();
                    if (LocalLanguage?.DataTableColonne?.ToUpper() == "ENGLISH")
                        ListWebtermLocal.Add("-");
                    else
                    {
                        PropertyInfo LangProp = typeof(Webterm).GetProperty(LocalLanguage?.DataTableColonne);
                        if (LangProp != null)
                        {
                            List<string> TempLocalList = new List<string>();
                            foreach (var term in pAllWebterm)
                                TempLocalList.Add(LangProp.GetValue(term).ToString());
                            foreach (var term in TempLocalList.OrderBy((item) => item))
                                ListWebtermLocal.Add(term);
                        }
                    }

                    foreach (var item in pWindchillContextList)
                        WindchillContextList.Add(item);
                    foreach (var item in pListGroup)
                        ListGroup.Add(item);
                    foreach (var item in pListBrand)
                        ListBrand.Add(item);

                    WtObject = new MgtWtObject()
                    {
                        REVISION = default(McgRevisionSchemaEnum).ToString(),
                        QUALINSPGRP = default(QualInspGrpValueEnum).ToString()
                    };
                    WtObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                    WtObject.Description21ChangeEvent += Description21ChangeEventAction;
                    SelectedWindchillDocumentType = ListWindchillDocumentType.FirstOrDefault();
                    SelectedWindchillPartType = ListWindchillPartType.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void Description21ChangeEventAction(object sender, EventArgs e)
        {
            try
            {
                WtObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;

                PropertyInfo LangProp = typeof(Webterm).GetProperty(SelectedLanguage?.DataTableColonne);
                if (LangProp != null)
                {
                    Webterm CurrentWebterm = AllWebterm.FirstOrDefault((item) => LangProp.GetValue(item).ToString() == SelectedLocalWebterm);
                    SelectedWebterm = ListWebterm.FirstOrDefault((item) => item == CurrentWebterm?.English);
                }

                WtObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PtcCommonNameChangeEventAction(object sender, EventArgs e)
        {
            try
            {
                WtObject.Description21ChangeEvent -= Description21ChangeEventAction;
                if (SelectedLanguage?.DataTableColonne.ToUpper() == "ENGLISH")
                    SelectedLocalWebterm = ListWebtermLocal.FirstOrDefault();
                else
                {
                    PropertyInfo LangProp = typeof(Webterm).GetProperty(SelectedLanguage?.DataTableColonne);
                    if (LangProp != null)
                    {
                        Webterm CurrentWebterm = AllWebterm.FirstOrDefault((item) => item.English == SelectedWebterm);
                        if (CurrentWebterm != null)
                            SelectedLocalWebterm = ListWebtermLocal.FirstOrDefault((item) => item == LangProp.GetValue(CurrentWebterm).ToString());
                    }
                }
                WtObject.Description21ChangeEvent += Description21ChangeEventAction;

            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteSelectContext()
        {
            try
            {
                var returnWindow = _mcgCommonLibWindowService.ShowDialogMcgWindchillContextSelection(WindchillContextList, WindchillContextList.FirstOrDefault());

                if (returnWindow.DialogResult.Value)
                {
                    WindchillContext SelectedContext = returnWindow.SelectedContext.Clone();
                    WtObject.SelectedWindchillContext = SelectedContext;
                    WtObject.SelectedWindchillContext.Folder = SelectedContext.OdataFolder.Name;
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
