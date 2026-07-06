using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.CadDocQualityCheck.View;
using pfcls;

namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class CadDocAttributeItem: ObservableObject, ICadDocAttributeItem
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Type;
        public string Type
        {
            get { return _Type; }
            set
            {
                if (this._Type != value)
                {
                    this._Type = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsDesignated;
        public bool IsDesignated
        {
            get { return _IsDesignated; }
            set
            {
                if (this._IsDesignated != value)
                {
                    this._IsDesignated = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsMissing = false;
        public bool IsMissing
        {
            get { return _IsMissing; }
            set
            {
                if (this._IsMissing != value)
                {
                    this._IsMissing = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsDesignatedOk = true;
        public bool IsDesignatedOk
        {
            get { return _IsDesignatedOk; }
            set
            {
                if (this._IsDesignatedOk != value)
                {
                    this._IsDesignatedOk = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsUpdated = false;
        public bool IsUpdated
        {
            get { return _IsUpdated; }
            set
            {
                if (this._IsUpdated != value)
                {
                    this._IsUpdated = value;
                    OnPropertyChanged();
                }

            }
        }

        private CadDocCheckStatus _AttributeStatus = CadDocCheckStatus.OK;
        public CadDocCheckStatus AttributeStatus
        {
            get { return _AttributeStatus; }
            set
            {
                if (this._AttributeStatus != value)
                {
                    this._AttributeStatus = value;
                    OnPropertyChanged();
                }

            }
        }

        public IpfcParameter Attribute { get; set; }
        public bool IsTemplateAttrib { get; set; } = false;
    }
}
