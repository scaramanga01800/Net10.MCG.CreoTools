namespace MCG.CREO_Tools.MiscTools.Configuration
{
    public static class MiscToolsConstants
    {
        public const string AppearanceFileName = "CreoToolsAppearance.dmt";
        public const string AppearanceFileName01 = "DefaultAppearance.dmt";
        public const string AppearanceFileName02 = "CreoToolsAppearance.dmt";
        public const string AppearanceFileName03 = "MarketingAppearance.dmt";
        public const string ConfigurationFileCadAutoColor = "CadAutoColorConfiguration.xml";
        public const string ConfigurationSapBomExport = "SapBomExportConfiguration.xml";
        public const string ExcelTabTemplateAnalysis = "MeasureTemplate";
        public const string ExcelTemplateAnalysis = "ExcelTemplateAnalysis.xlsx";
        public const string MainDictionary = "CREOToolsMiscToolsDictionary.xaml";
        public const string TemplateSapBomExport = "Template_SAP_BOM_Export.xlsx";
        public const string TemplateSapBomExport2 = "Template_SAP_BOM_Export2.xlsx";
        public const string TemplateSapBomExportCompTab = "COMPONENTS";
        public const int TemplateSapBomExportFirstCompIndex = 10;
        public const int TemplateSapBomExportFirstCompIndex2 = 6;
        public const string TemplateSapBomExportMainTab = "BOM";
        public const string TemplateWebtermRequest = "WebtermRequestTemplate.txt";
        public const string WebtermRequestMail = "thierry.champier@manitowoc.com";
        public const string WebtermRequestMailCC = "thierry.champier@manitowoc.com";
        public const string WebtermRequestMailSave = "ericka.nivet@manitowoc.com";

        public const string BomExportConfigurationFile = "BomExportConfiguration.xml";
        public const string BomExportUserPreferencesFile = "BomExportUserPreferences.xml";
        public const string BomFromValues = "PDM|SAP";
        public const char CsvSeparator = ',';
        public const int DefaultRepDigit = 4;
        public const string ExcelTemplateBomComparison = "BOM_COMPARISON_Template.xlsx";
        public const string RegexPartNumberToExcludeBomSearch = "^7\\d{9}$|^7\\d{8}[A-Z]$|^7\\d{7}[A-Z]\\d$";
        public const string RegexStateCumulationNumber = "INWORK|REWORK";
        public const int MaxBomLevel = 12;
        public const int MaxRepDigit = 10;
        public const int MinRepDigit = 1;
        public const string SapPlantValues = "Without|1000|1011|1012|1050|1070|1090";
    }
}
