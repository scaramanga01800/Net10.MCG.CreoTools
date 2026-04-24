using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.NumberingTool.Exceptions;
using MCG.Tools.NumberingTool.View;

namespace MCG.Tools.NumberingTool.ViewModel
{
    public class NumberingToolTemplate:  ObservableObject, INumberingToolTemplate
    {
        #region [REGION] Properties from Interface
        private string _NumberingTemplate = string.Empty;
        public string NumberingTemplate
        {
            get { return _NumberingTemplate; }
            set
            {
                if (this._NumberingTemplate != value)
                {
                    this._NumberingTemplate = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Description = string.Empty;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (this._Description != value)
                {
                    this._Description = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsRangeAuthorized = false;
        public bool IsRangeAuthorized
        {
            get { return _IsRangeAuthorized; }
            set
            {
                if (this._IsRangeAuthorized != value)
                {
                    this._IsRangeAuthorized = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _MaxRange = 1;
        public int MaxRange
        {
            get { return _MaxRange; }
            set
            {
                if (this._MaxRange != value)
                {
                    this._MaxRange = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public string SequenceName { get; set; } = string.Empty;
        public string LeadingZeroFormat { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Prefix { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        #endregion

        #region [REGION] Misc
        public static NumberingToolTemplate GetNumberingToolTemplate( NumberingTemplate template, bool NoRangeAuthorized = false)
        {
            try
            {
                return new NumberingToolTemplate()
                {
                    Description = template.Description ?? string.Empty,
                    IsActive = template.Isactive,
                    IsRangeAuthorized = template.Israngeauthorized && !NoRangeAuthorized,
                    LeadingZeroFormat = template.Leadingzeroformat ?? string.Empty,
                    MaxRange = template.Maxrange,
                    NumberingTemplate = template.Templateid ?? string.Empty,
                    Prefix = template.Prefix ?? string.Empty,
                    SequenceName = template.Sequenceprocid ?? string.Empty,
                    Suffix = template.Suffix ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                throw new NumberingToolException("NumberingToolTemplate", ex);
            }
        }

        public override string ToString()
        {
            return NumberingTemplate;
        }
        #endregion


    }
}
