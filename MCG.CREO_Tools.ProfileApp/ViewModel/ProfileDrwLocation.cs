namespace MCG.CREO_Tools.ProfileApp.ViewModel
{
    public class ProfileDrwLocation
    {
        #region [REGION] Internal variables
        public string Location { get; set; }
        public string DrwSuffix { get; set; }
        public string WebtermLang { get; set; }
        #endregion

        #region [REGION] Read/update information in SQL Server DataBase
        public override string ToString()
        {
            return Location;
        }
        #endregion
    }
}
