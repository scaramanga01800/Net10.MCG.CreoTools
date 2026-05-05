using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Interfaces.Models;

namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public class EFU_EcnEcoFollowUp : ObservableObject, IEFU_EcnEcoFollowUp
    {
        #region [REGION] Properties from Interface
        private string _Ecn_Number = string.Empty;
        public string Ecn_Number
        {
            get { return this._Ecn_Number; }
            set
            {
                if (this._Ecn_Number != value)
                {
                    this._Ecn_Number = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Ecn_Name = string.Empty;
        public string Ecn_Name
        {
            get { return this._Ecn_Name; }
            set
            {
                if (this._Ecn_Name != value)
                {
                    this._Ecn_Name = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Ecn_State = string.Empty;
        public string Ecn_State
        {
            get { return this._Ecn_State; }
            set
            {
                if (this._Ecn_State != value)
                {
                    this._Ecn_State = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Pdm_Product = string.Empty;
        public string Pdm_Product
        {
            get { return this._Pdm_Product; }
            set
            {
                if (this._Pdm_Product != value)
                {
                    this._Pdm_Product = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Ecn_Creator_Name = string.Empty;
        public string Ecn_Creator_Name
        {
            get { return this._Ecn_Creator_Name; }
            set
            {
                if (this._Ecn_Creator_Name != value)
                {
                    this._Ecn_Creator_Name = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Eco_Status = string.Empty;
        public string Eco_Status
        {
            get { return this._Eco_Status; }
            set
            {
                if (this._Eco_Status != value)
                {
                    this._Eco_Status = value;
                    OnPropertyChanged();
                }
                UpdateSapStatusOder();
            }
        }

        private string _Eco_Urgence = string.Empty;
        public string Eco_Urgence
        {
            get { return this._Eco_Urgence; }
            set
            {
                if (this._Eco_Urgence != value)
                {
                    this._Eco_Urgence = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Eco_Project = string.Empty;
        public string Eco_Project
        {
            get { return this._Eco_Project; }
            set
            {
                if (this._Eco_Project != value)
                {
                    this._Eco_Project = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Eco_Categ = string.Empty;
        public string Eco_Categ
        {
            get { return this._Eco_Categ; }
            set
            {
                if (this._Eco_Categ != value)
                {
                    this._Eco_Categ = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Eco_Sub_Line = string.Empty;
        public string Eco_Sub_Line
        {
            get { return this._Eco_Sub_Line; }
            set
            {
                if (this._Eco_Sub_Line != value)
                {
                    this._Eco_Sub_Line = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Eco_Next_Step = string.Empty;
        public string Eco_Next_Step
        {
            get { return this._Eco_Next_Step; }
            set
            {
                if (this._Eco_Next_Step != value)
                {
                    this._Eco_Next_Step = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _Eco_IsCreated = false;
        public bool Eco_IsCreated
        {
            get { return _Eco_IsCreated; }
            set
            {
                if (this._Eco_IsCreated != value)
                {
                    this._Eco_IsCreated = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _Nb_Part;
        public int Nb_Part
        {
            get { return this._Nb_Part; }
            set
            {
                if (this._Nb_Part != value)
                {
                    this._Nb_Part = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _Nb_Drw;
        public int Nb_Drw
        {
            get { return this._Nb_Drw; }
            set
            {
                if (this._Nb_Drw != value)
                {
                    this._Nb_Drw = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _Nb_Epm_Doc;
        public int Nb_Epm_Doc
        {
            get { return this._Nb_Epm_Doc; }
            set
            {
                if (this._Nb_Epm_Doc != value)
                {
                    this._Nb_Epm_Doc = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _Nb_Wt_Doc;
        public int Nb_Wt_Doc
        {
            get { return this._Nb_Wt_Doc; }
            set
            {
                if (this._Nb_Wt_Doc != value)
                {
                    this._Nb_Wt_Doc = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _Eco_Status_Order = 0;
        public int Eco_Status_Order
        {
            get { return this._Eco_Status_Order; }
            set
            {
                if (this._Eco_Status_Order != value)
                {
                    this._Eco_Status_Order = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _Ecn_Created_On;
        public DateTime? Ecn_Created_On
        {
            get { return this._Ecn_Created_On; }
            set
            {
                if (this._Ecn_Created_On != value)
                {
                    this._Ecn_Created_On = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _Designer_Start_App_Date;
        public DateTime? Designer_Start_App_Date
        {
            get { return this._Designer_Start_App_Date; }
            set
            {
                if (this._Designer_Start_App_Date != value)
                {
                    this._Designer_Start_App_Date = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _First_Approval_Date;
        public DateTime? First_Approval_Date
        {
            get { return this._First_Approval_Date; }
            set
            {
                if (this._First_Approval_Date != value)
                {
                    this._First_Approval_Date = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _Qual_Check_Approval_Date;
        public DateTime? Qual_Check_Approval_Date
        {
            get { return this._Qual_Check_Approval_Date; }
            set
            {
                if (this._Qual_Check_Approval_Date != value)
                {
                    this._Qual_Check_Approval_Date = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _CAIII_Approval_Date;
        public DateTime? CAIII_Approval_Date
        {
            get { return this._CAIII_Approval_Date; }
            set
            {
                if (this._CAIII_Approval_Date != value)
                {
                    this._CAIII_Approval_Date = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _Eco_Created_On;
        public DateTime? Eco_Created_On
        {
            get { return this._Eco_Created_On; }
            set
            {
                if (this._Eco_Created_On != value)
                {
                    this._Eco_Created_On = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _Eco_Wf_Started_On;
        public DateTime? Eco_Wf_Started_On
        {
            get { return this._Eco_Wf_Started_On; }
            set
            {
                if (this._Eco_Wf_Started_On != value)
                {
                    this._Eco_Wf_Started_On = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _Eco_Effectivity_Date;
        public DateTime? Eco_Effectivity_Date
        {
            get { return this._Eco_Effectivity_Date; }
            set
            {
                if (this._Eco_Effectivity_Date != value)
                {
                    this._Eco_Effectivity_Date = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _Eco_Closed_On;
        public DateTime? Eco_Closed_On
        {
            get { return this._Eco_Closed_On; }
            set
            {
                if (this._Eco_Closed_On != value)
                {
                    this._Eco_Closed_On = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _Eco_Last_Wi_Created_On;
        public DateTime? Eco_Last_Wi_Created_On
        {
            get { return _Eco_Last_Wi_Created_On; }
            set
            {
                if (this._Eco_Last_Wi_Created_On != value)
                {
                    this._Eco_Last_Wi_Created_On = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _MainPlant = string.Empty;
        public string MainPlant
        {
            get { return _MainPlant; }
            set
            {
                if (this._MainPlant != value)
                {
                    this._MainPlant = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _MainPlantDescription = string.Empty;
        public string MainPlantDescription
        {
            get { return _MainPlantDescription; }
            set
            {
                if (this._MainPlantDescription != value)
                {
                    this._MainPlantDescription = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Eco_Next_Step_Secondary = string.Empty;
        public string Eco_Next_Step_Secondary
        {
            get { return _Eco_Next_Step_Secondary; }
            set
            {
                if (this._Eco_Next_Step_Secondary != value)
                {
                    this._Eco_Next_Step_Secondary = value;
                    OnPropertyChanged();
                }

            }
        }

        private DateTime? _Eco_Last_Wi_Created_On_Secondary;
        public DateTime? Eco_Last_Wi_Created_On_Secondary
        {
            get { return _Eco_Last_Wi_Created_On_Secondary; }
            set
            {
                if (this._Eco_Last_Wi_Created_On_Secondary != value)
                {
                    this._Eco_Last_Wi_Created_On_Secondary = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Eco_Wi_Secondary_Plants = string.Empty;
        public string Eco_Wi_Secondary_Plants
        {
            get { return _Eco_Wi_Secondary_Plants; }
            set
            {
                if (this._Eco_Wi_Secondary_Plants != value)
                {
                    this._Eco_Wi_Secondary_Plants = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Properties not from interface
        public string CaIII_Name { get; set; } = string.Empty;
        public string Ecn_Description { get; set; } = string.Empty;
        public string Pdm_Context { get; set; } = string.Empty;
        public string Pdm_Ecn_Id { get; set; } = string.Empty;
        public string Pdm_Update_Status { get; set; } = string.Empty;
        public string Sap_Update_Status { get; set; } = string.Empty;
        public int? Eco_Tmlpse_Wi_Close { get; set; }
        public int ID { get; set; }
        #endregion

        private void UpdateSapStatusOder()
        {
            if (Eco_Status != null)
            {
                if (Eco_Status == "99") Eco_Status_Order = 1;
                else if (Eco_Status == "01") Eco_Status_Order = 2;
                else if (Eco_Status == "02") Eco_Status_Order = 3;
                else if (Eco_Status == "03") Eco_Status_Order = 4;
                else Eco_Status_Order = 0;
                Eco_IsCreated = Eco_Status_Order != 0;
            }
            else
                Eco_Status_Order = 0;
        }

        public static EFU_EcnEcoFollowUp GetEFU_EcnEcoFollowUp(Ecnecofollowup Item)
        {
            try
            {
                if (Item == null) return null;

                return new EFU_EcnEcoFollowUp()
                {
                    CAIII_Approval_Date = Item.CaiiiApprovalDate.HasValue ? Item.CaiiiApprovalDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    Designer_Start_App_Date = Item.DesignerStartAppDate.HasValue ? Item.DesignerStartAppDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    Ecn_Created_On = Item.EcnCreatedOn.HasValue ? Item.EcnCreatedOn.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    First_Approval_Date = Item.FirstApprovalDate.HasValue ? Item.FirstApprovalDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    Qual_Check_Approval_Date = Item.QualCheckApprovalDate.HasValue ? Item.QualCheckApprovalDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    Eco_Created_On = Item.EcoCreatedOn.HasValue ? Item.EcoCreatedOn.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    Eco_Wf_Started_On = Item.EcoWfStartedOn.HasValue ? Item.EcoWfStartedOn.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    Eco_Closed_On = Item.EcoClosedOn.HasValue ? Item.EcoClosedOn.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    Eco_Effectivity_Date = Item.EcoEffectivityDate.HasValue ? Item.EcoEffectivityDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    Eco_Last_Wi_Created_On = Item.EcoLastWiCreatedOn.HasValue ? Item.EcoLastWiCreatedOn.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    Eco_Last_Wi_Created_On_Secondary = Item.EcoLastWiCreatedOnSecondary.HasValue ? Item.EcoLastWiCreatedOnSecondary.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,

                    CaIII_Name = Item.CaiiiName,
                    Ecn_Creator_Name = Item.EcnCreatedBy,
                    Ecn_Description = Item.EcnDescription,
                    Ecn_Name = Item.EcnName,
                    Ecn_Number = Item.EcnNumber,
                    Ecn_State = Item.EcnState,
                    Eco_Categ = Item.EcoCateg,
                    Eco_Next_Step = Item.EcoNextStep,
                    Eco_Project = Item.EcoProject,
                    Eco_Status = Item.EcoStatus,
                    Eco_Sub_Line = Item.EcoSubLine,
                    Eco_Tmlpse_Wi_Close = Item.EcoTmlpseWiClose,
                    Eco_Urgence = Item.EcoUrge,
                    ID = Item.Id,
                    Nb_Drw = Item.NbDrw ?? 0,
                    Nb_Epm_Doc = Item.NbEpmDoc ?? 0,
                    Nb_Part = Item.NbPart ?? 0,
                    Nb_Wt_Doc = Item.NbWtDoc ?? 0,
                    Pdm_Context = Item.PdmContext,
                    Pdm_Ecn_Id = Item.PdmEcnId,
                    Pdm_Product = Item.PdmProduct,
                    Pdm_Update_Status = Item.PdmUpdateStatus,
                    Sap_Update_Status = Item.SapUpdateStatus,
                    MainPlant = Item.EcoMainPlant,
                    MainPlantDescription = Item.EcoMainPlantDesc,
                    Eco_Next_Step_Secondary = Item.EcoNextStepSecondary,
                    Eco_Wi_Secondary_Plants = Item.EcoWiSecondaryPlants
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Ecnecofollowup GetEFU_EcnEcoFollowUp(EFU_EcnEcoFollowUp Item)
        {
            try
            {
                return new Ecnecofollowup()
                {
                    CaiiiApprovalDate = Item.CAIII_Approval_Date.HasValue ? DateOnly.FromDateTime(Item.CAIII_Approval_Date.Value) : (DateOnly?)null,
                    CaiiiName = Item.CaIII_Name,
                    DesignerStartAppDate = Item.Designer_Start_App_Date.HasValue ? DateOnly.FromDateTime(Item.Designer_Start_App_Date.Value) : (DateOnly?)null,
                    EcnCreatedOn = Item.Ecn_Created_On.HasValue ? DateOnly.FromDateTime(Item.Ecn_Created_On.Value) : (DateOnly?)null,
                    EcnCreatedBy = Item.Ecn_Creator_Name,
                    EcnDescription = Item.Ecn_Description,
                    EcnName = Item.Ecn_Name,
                    EcnNumber = Item.Ecn_Number,
                    EcnState = Item.Ecn_State,
                    EcoCateg = Item.Eco_Categ,
                    EcoClosedOn = Item.Eco_Closed_On.HasValue ? DateOnly.FromDateTime(Item.Eco_Closed_On.Value) : (DateOnly?)null,
                    EcoCreatedOn = Item.Eco_Created_On.HasValue ? DateOnly.FromDateTime(Item.Eco_Created_On.Value) : (DateOnly?)null,
                    EcoEffectivityDate = Item.Eco_Effectivity_Date.HasValue ? DateOnly.FromDateTime(Item.Eco_Effectivity_Date.Value) : (DateOnly?)null,
                    EcoNextStep = Item.Eco_Next_Step,
                    EcoProject = Item.Eco_Project,
                    EcoStatus = Item.Eco_Status,
                    EcoSubLine = Item.Eco_Sub_Line,
                    EcoTmlpseWiClose = Item.Eco_Tmlpse_Wi_Close,
                    EcoUrge = Item.Eco_Urgence,
                    EcoWfStartedOn = Item.Eco_Wf_Started_On.HasValue ? DateOnly.FromDateTime(Item.Eco_Wf_Started_On.Value) : (DateOnly?)null,
                    FirstApprovalDate = Item.First_Approval_Date.HasValue ? DateOnly.FromDateTime(Item.First_Approval_Date.Value) : (DateOnly?)null,
                    Id = Item.ID,
                    NbDrw = Item.Nb_Drw,
                    NbEpmDoc = Item.Nb_Epm_Doc,
                    NbPart = Item.Nb_Part,
                    NbWtDoc = Item.Nb_Wt_Doc,
                    PdmContext = Item.Pdm_Context,
                    PdmEcnId = Item.Pdm_Ecn_Id,
                    PdmProduct = Item.Pdm_Product,
                    PdmUpdateStatus = Item.Pdm_Update_Status,
                    QualCheckApprovalDate = Item.Qual_Check_Approval_Date.HasValue ? DateOnly.FromDateTime(Item.Qual_Check_Approval_Date.Value) : (DateOnly?)null,
                    SapUpdateStatus = Item.Sap_Update_Status,

                    EcoMainPlant = Item.MainPlant,
                    EcoMainPlantDesc = Item.MainPlantDescription,
                    EcoNextStepSecondary = Item.Eco_Next_Step_Secondary,
                    EcoLastWiCreatedOnSecondary = Item.Eco_Last_Wi_Created_On_Secondary.HasValue ? DateOnly.FromDateTime(Item.Eco_Last_Wi_Created_On_Secondary.Value) : (DateOnly?)null,
                    EcoWiSecondaryPlants = Item.Eco_Wi_Secondary_Plants
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
