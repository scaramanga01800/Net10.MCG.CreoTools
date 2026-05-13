namespace MCG.Tools.EcnDataCheck.Configuration
{
    public static class EcnDataCheckConstants
    {
        public const string BomComponentStateNotApproved = "INWORK|IN WORK|REWORK";
        public const string ConfigurationFile = "EcnDataCheckConfiguration.xml";
        public const string EcnResolvedState = "RESOLVED";
        public const string ErpValidityDate = "99990101";
        public const string ExcelTemplateBomSheet = "BOM_TEMPLATE";
        public const string ExcelTemplateEcnDataCheck = "ECN_DATA_Check_Template.xlsx";
        public const string ExtractedSapBomFileName = "Extract_SAP_BOM.txt";
        public const string MainDictionary = "EcnDataCheckDictionary.xaml";
        public const string RegExPartRepresentationLoadedFromLegacy = "Loaded from Oracle|Loaded from DVP";
        public const string Version = "2.00";
        public const int MaxLenghtConcatenatedDesc = 30;
        public const int MaxLenghtSapDetailDesc = 20;
        public const string WtDocumentTypePlanTif = "Plan TIF";
    }
}
