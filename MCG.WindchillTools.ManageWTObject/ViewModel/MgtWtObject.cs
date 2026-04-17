using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Services.Statics;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.Exceptions;
using MCG.WindchillTools.ManageWTObject.View;

namespace MCG.WindchillTools.ManageWTObject.ViewModel
{
    public class MgtWtObject : ObservableObject, IMgtWtObject
    {
        #region [REGION] Properties from Interface
        private string _NUMBER;
        public string NUMBER
        {
            get { return _NUMBER; }
            set
            {
                if (this._NUMBER != value)
                {
                    this._NUMBER = value;
                    OnPropertyChanged();
                    RaiseNumberChangeEvent();
                }

            }
        }

        private string _REVISION;
        public string REVISION
        {
            get { return _REVISION; }
            set
            {
                if (this._REVISION != value)
                {
                    this._REVISION = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _PTCCOMMONNAME;
        public string PTCCOMMONNAME
        {
            get { return _PTCCOMMONNAME; }
            set
            {
                if (this._PTCCOMMONNAME != value)
                {
                    this._PTCCOMMONNAME = value;
                    OnPropertyChanged();
                    RaisePtcCommonNameChangeEvent();
                }
                //RaisePtcCommonNameChangeEvent();
            }
        }

        private string _DESCRIPTION2;
        public string DESCRIPTION2
        {
            get { return _DESCRIPTION2; }
            set
            {
                if (this._DESCRIPTION2 != value)
                {
                    this._DESCRIPTION2 = value;
                    OnPropertyChanged();
                    RaiseVersionParamChangeEvent();
                }

            }
        }

        private string _DESCRIPTION21;
        public string DESCRIPTION21
        {
            get { return _DESCRIPTION21; }
            set
            {
                if (this._DESCRIPTION21 != value)
                {
                    this._DESCRIPTION21 = value;
                    OnPropertyChanged();
                    RaiseDescription21ChangeEvent();
                    RaiseVersionParamChangeEvent();
                }
                //RaiseDescription21ChangeEvent();
            }
        }

        private string _DESCRIPTION22;
        public string DESCRIPTION22
        {
            get { return _DESCRIPTION22; }
            set
            {
                if (this._DESCRIPTION22 != value)
                {
                    this._DESCRIPTION22 = value;
                    OnPropertyChanged();
                    RaiseVersionParamChangeEvent();
                }

            }
        }

        private string _GROUPCREATOR;
        public string GROUPCREATOR
        {
            get { return _GROUPCREATOR; }
            set
            {
                if (this._GROUPCREATOR != value)
                {
                    this._GROUPCREATOR = value;
                    OnPropertyChanged();
                    RaiseVersionParamChangeEvent();
                }

            }
        }

        private string _QUALINSPGRP;
        public string QUALINSPGRP
        {
            get { return _QUALINSPGRP; }
            set
            {
                if (this._QUALINSPGRP != value)
                {
                    this._QUALINSPGRP = value;
                    OnPropertyChanged();
                    RaiseVersionParamChangeEvent();
                }

            }
        }

        private double _MASS;
        public double MASS
        {
            get { return _MASS; }
            set
            {
                if (this._MASS != value)
                {
                    this._MASS = value;
                    OnPropertyChanged();
                    RaiseVersionParamChangeEvent();
                }

            }
        }

        private string _MATERIAL = "UNDEFINED";
        public string MATERIAL
        {
            get { return _MATERIAL; }
            set
            {
                if (this._MATERIAL != value)
                {
                    this._MATERIAL = value;
                    OnPropertyChanged();
                    RaiseVersionParamChangeEvent();
                }

            }
        }

        private string _GROUP;
        public string GROUP
        {
            get { return _GROUP; }
            set
            {
                if (this._GROUP != value)
                {
                    this._GROUP = value;
                    OnPropertyChanged();
                    RaiseVersionParamChangeEvent();
                }

            }
        }

        private string _SUB_GROUP;
        public string SUB_GROUP
        {
            get { return _SUB_GROUP; }
            set
            {
                if (this._SUB_GROUP != value)
                {
                    this._SUB_GROUP = value;
                    OnPropertyChanged();
                    RaiseVersionParamChangeEvent();
                }

            }
        }

        private string _BRAND;
        public string BRAND
        {
            get { return _BRAND; }
            set
            {
                if (this._BRAND != value)
                {
                    this._BRAND = value;
                    OnPropertyChanged();
                    RaiseVersionParamChangeEvent();
                }

            }
        }

        private string _MODEL;
        public string MODEL
        {
            get { return _MODEL; }
            set
            {
                if (this._MODEL != value)
                {
                    this._MODEL = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _OPTION;
        public string OPTION
        {
            get { return _OPTION; }
            set
            {
                if (this._OPTION != value)
                {
                    this._OPTION = value;
                    OnPropertyChanged();
                }

            }
        }

        private WindchillContext _SelectedWindchillContext;
        public WindchillContext SelectedWindchillContext
        {
            get { return _SelectedWindchillContext; }
            set
            {
                if (this._SelectedWindchillContext != value)
                {
                    this._SelectedWindchillContext = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _State = "Unknown";
        public string State
        {
            get { return _State; }
            set
            {
                if (this._State != value)
                {
                    this._State = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Unit;
        public string Unit
        {
            get { return _Unit; }
            set
            {
                if (this._Unit != value)
                {
                    this._Unit = value;
                    OnPropertyChanged();
                    RaiseCommonParamChangeEvent();
                }

            }
        }


        private ObjectState _Status = ObjectState.UNKNOWN;
        public ObjectState Status
        {
            get { return _Status; }
            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsObjectEditable;
        public bool IsObjectEditable
        {
            get { return _IsObjectEditable; }
            set
            {
                if (this._IsObjectEditable != value)
                {
                    this._IsObjectEditable = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsWtNonVersionAttributeEditable = true;
        public bool IsWtNonVersionAttributeEditable
        {
            get { return _IsWtNonVersionAttributeEditable; }
            set
            {
                if (this._IsWtNonVersionAttributeEditable != value)
                {
                    this._IsWtNonVersionAttributeEditable = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsWtVersionAttributeEditable = true;
        public bool IsWtVersionAttributeEditable
        {
            get { return _IsWtVersionAttributeEditable; }
            set
            {
                if (this._IsWtVersionAttributeEditable != value)
                {
                    this._IsWtVersionAttributeEditable = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsWtCommonAttributeEditable = true;
        public bool IsWtCommonAttributeEditable
        {
            get { return _IsWtCommonAttributeEditable; }
            set
            {
                if (this._IsWtCommonAttributeEditable != value)
                {
                    this._IsWtCommonAttributeEditable = value;
                    OnPropertyChanged();
                }

            }
        }

        #endregion

        #region [REGION] Internal variables
        public string CompleteFileName { get; set; }
        public MgtWtDocumentItem ParentDocument { get; set; }
        #endregion

        #region [REGION] Event
        public event EventHandler PtcCommonNameChangeEvent;
        public void RaisePtcCommonNameChangeEvent()
        {
            try
            {
                PtcCommonNameChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler Description21ChangeEvent;
        public void RaiseDescription21ChangeEvent()
        {
            try
            {
                Description21ChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler VersionParamChangeEvent;
        public void RaiseVersionParamChangeEvent()
        {
            try
            {
                VersionParamChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler CommonParamChangeEvent;
        public void RaiseCommonParamChangeEvent()
        {
            try
            {
                CommonParamChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler NumberChangeEvent;
        public void RaiseNumberChangeEvent()
        {
            try
            {
                NumberChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Init
        public static MgtWtObject CreateMgtWtObject(RestOdataWtObject CurrentItem)
        {
            try
            {
                MgtWtObject newOb = new MgtWtObject();
                newOb.UpdateFromRestOdataWtObject(CurrentItem);
                return newOb;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException("MgtWtObject", ex);
            }
        }
        #endregion

        #region [REGION] Misc Methods
        public void UpdateMgtWtDocumentItem(MgtWtDocumentItem CurrentItem)
        {
            try
            {
                if (CurrentItem.WindchillWtDocument == null)
                    CurrentItem.WindchillWtDocument = new RestOdataWtDocument()
                    {
                        Name = PTCCOMMONNAME?.Trim().ToUpper(),
                        DESCRIPTION_2 = DESCRIPTION2?.Trim().ToUpper(),
                        DESCRIPTION2_1 = DESCRIPTION21?.Trim().ToUpper(),
                        DESCRIPTION2_2 = DESCRIPTION22?.Trim().ToUpper(),
                        GROUP_CREATOR = GROUPCREATOR?.Trim().ToUpper(),
                        QUALINSPGRP = QUALINSPGRP?.Trim().ToUpper(),
                        MASS = MASS
                    };
                if (CurrentItem.WindchillWtPart == null)
                    CurrentItem.WindchillWtPart = new RestOdataWtPart()
                    {
                        Name = PTCCOMMONNAME?.Trim().ToUpper(),
                        DESCRIPTION_2 = DESCRIPTION2?.Trim().ToUpper(),
                        DESCRIPTION2_1 = DESCRIPTION21?.Trim().ToUpper(),
                        DESCRIPTION2_2 = DESCRIPTION22?.Trim().ToUpper(),
                        GROUP_CREATOR = GROUPCREATOR?.Trim().ToUpper(),
                        QUALINSPGRP = QUALINSPGRP?.Trim().ToUpper(),
                        MASS = MASS,
                        BRAND = BRAND,
                        GROUP = GROUP,
                        SUB_GROUP = SUB_GROUP,
                        MODEL = MODEL,
                        OPTION = OPTION
                    };

                if (CurrentItem.WtDocumentFound)
                {
                    if (PTCCOMMONNAME != null && PTCCOMMONNAME.Trim() != "")
                        CurrentItem.WindchillWtDocument.Name = PTCCOMMONNAME.Trim().ToUpper();
                    if (DESCRIPTION2 != null && DESCRIPTION2.Trim() != "")
                        CurrentItem.WindchillWtDocument.DESCRIPTION_2 = DESCRIPTION2.Trim().ToUpper();
                    if (DESCRIPTION21 != null && DESCRIPTION21.Trim() != "")
                        CurrentItem.WindchillWtDocument.DESCRIPTION2_1 = DESCRIPTION21.Trim().ToUpper();
                    if (DESCRIPTION22 != null && DESCRIPTION22.Trim() != "")
                        CurrentItem.WindchillWtDocument.DESCRIPTION2_2 = DESCRIPTION22.Trim().ToUpper();
                    if (GROUPCREATOR != null && GROUPCREATOR.Trim() != "")
                        CurrentItem.WindchillWtDocument.GROUP_CREATOR = GROUPCREATOR.Trim().ToUpper();
                    if (QUALINSPGRP != null && QUALINSPGRP.Trim() != "")
                        CurrentItem.WindchillWtDocument.QUALINSPGRP = QUALINSPGRP.Trim().ToUpper();
                    if (MASS != 0)
                        CurrentItem.WindchillWtDocument.MASS = MASS;
                }
                if (CurrentItem.PartFound)
                {
                    if (PTCCOMMONNAME != null && PTCCOMMONNAME.Trim() != "")
                        CurrentItem.WindchillWtPart.Name = PTCCOMMONNAME.Trim().ToUpper();
                    if (DESCRIPTION2 != null && DESCRIPTION2.Trim() != "")
                        CurrentItem.WindchillWtPart.DESCRIPTION_2 = DESCRIPTION2.Trim().ToUpper();
                    if (DESCRIPTION21 != null && DESCRIPTION21.Trim() != "")
                        CurrentItem.WindchillWtPart.DESCRIPTION2_1 = DESCRIPTION21.Trim().ToUpper();
                    if (DESCRIPTION22 != null && DESCRIPTION22.Trim() != "")
                        CurrentItem.WindchillWtPart.DESCRIPTION2_2 = DESCRIPTION22.Trim().ToUpper();
                    if (GROUPCREATOR != null && GROUPCREATOR.Trim() != "")
                        CurrentItem.WindchillWtPart.GROUP_CREATOR = GROUPCREATOR.Trim().ToUpper();
                    if (QUALINSPGRP != null && QUALINSPGRP.Trim() != "")
                        CurrentItem.WindchillWtPart.QUALINSPGRP = QUALINSPGRP.Trim().ToUpper();
                    if (MASS != 0)
                        CurrentItem.WindchillWtPart.MASS = MASS;
                    if (BRAND != null && BRAND.Trim() != "")
                        CurrentItem.WindchillWtPart.BRAND = BRAND.Trim().ToUpper();
                    if (GROUP != null && GROUP.Trim() != "")
                        CurrentItem.WindchillWtPart.GROUP = GROUP.Trim().ToUpper();
                    if (SUB_GROUP != null && SUB_GROUP.Trim() != "")
                        CurrentItem.WindchillWtPart.SUB_GROUP = SUB_GROUP.Trim().ToUpper();
                    if (MODEL != null && MODEL.Trim() != "")
                        CurrentItem.WindchillWtPart.MODEL = MODEL.Trim().ToUpper();
                    if (OPTION != null && OPTION.Trim() != "")
                        CurrentItem.WindchillWtPart.OPTION = OPTION.Trim().ToUpper();
                }
                CurrentItem.WtDocumentObject.UpdateFromRestOdataWtObject(CurrentItem.WindchillWtDocument);
                CurrentItem.WtPartObject.UpdateFromRestOdataWtObject(CurrentItem.WindchillWtPart);
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        public MgtWtDocumentItem GetMgtWtDocumentItem()
        {
            try
            {
                MgtWtDocumentItem CurrentItem = new MgtWtDocumentItem()
                {
                    Number = NUMBER?.Trim().ToUpper(),
                    Revision = McgMiscTools.GetEnumValue<McgRevisionSchemaEnum>(REVISION),
                    WindchillWtDocument = new RestOdataWtDocument()
                    {
                        Name = PTCCOMMONNAME?.Trim().ToUpper(),
                        DESCRIPTION_2 = DESCRIPTION2?.Trim().ToUpper(),
                        DESCRIPTION2_1 = DESCRIPTION21?.Trim().ToUpper(),
                        DESCRIPTION2_2 = DESCRIPTION22?.Trim().ToUpper(),
                        GROUP_CREATOR = GROUPCREATOR?.Trim().ToUpper(),
                        QUALINSPGRP = QUALINSPGRP?.Trim().ToUpper(),
                        MASS = MASS
                    },
                    WindchillWtPart = new RestOdataWtPart()
                    {
                        Name = PTCCOMMONNAME?.Trim().ToUpper(),
                        DESCRIPTION_2 = DESCRIPTION2?.Trim().ToUpper(),
                        DESCRIPTION2_1 = DESCRIPTION21?.Trim().ToUpper(),
                        DESCRIPTION2_2 = DESCRIPTION22?.Trim().ToUpper(),
                        GROUP_CREATOR = GROUPCREATOR?.Trim().ToUpper(),
                        QUALINSPGRP = QUALINSPGRP?.Trim().ToUpper(),
                        MASS = MASS,
                        BRAND = BRAND,
                        GROUP = GROUP,
                        SUB_GROUP = SUB_GROUP,
                        OPTION = OPTION,
                        MODEL = MODEL
                    },
                };
                CurrentItem.WtDocumentObject = MgtWtObject.CreateMgtWtObject(CurrentItem.WindchillWtDocument);
                CurrentItem.WtPartObject = MgtWtObject.CreateMgtWtObject(CurrentItem.WindchillWtPart);
                CurrentItem.WtDocumentObject.ParentDocument = CurrentItem;
                CurrentItem.WtPartObject.ParentDocument = CurrentItem;
                return CurrentItem;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        public void UpdateFromRestOdataWtObject(RestOdataWtObject CurrentItem)
        {
            try
            {
                NUMBER = CurrentItem.Number;
                REVISION = CurrentItem.Revision;
                PTCCOMMONNAME = CurrentItem.Name;
                DESCRIPTION2 = CurrentItem.DESCRIPTION_2;
                DESCRIPTION21 = CurrentItem.DESCRIPTION2_1;
                DESCRIPTION22 = CurrentItem.DESCRIPTION2_2;
                GROUPCREATOR = CurrentItem.GROUP_CREATOR;
                QUALINSPGRP = CurrentItem.QUALINSPGRP;
                BRAND = CurrentItem.BRAND;
                GROUP = CurrentItem.GROUP;
                SUB_GROUP = CurrentItem.SUB_GROUP;
                MODEL = CurrentItem.MODEL;
                OPTION = CurrentItem.OPTION;
                if (CurrentItem.MASS != null)
                    MASS = CurrentItem.MASS.GetValueOrDefault();
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
