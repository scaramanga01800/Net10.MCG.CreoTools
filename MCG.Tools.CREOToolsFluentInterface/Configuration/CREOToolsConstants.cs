namespace MCG.Tools.CREOToolsFluentInterface.Configuration
{
    public static class CREOToolsConstants
    {
        public const string MainDictionary = "CREOToolsMainDictionary.xaml";
        public const string ConfigurationFile = "CREOToolsConfiguration.xml";
        public const string CreoToolsUserConfigXmlFile = "CreoToolsUserConfig.xml";
        public const string Version = "12.13";
        public const string Year = "2026";

        /// <summary>
        /// Version de reference du fichier de configuration utilisateur.
        /// Toute configuration locale portant une version inferieure est migree
        /// silencieusement au demarrage (voir MigrateUserConfiguration).
        /// A incrementer a chaque ajout d'application dans AppVisible.
        /// </summary>
        public const string UserConfigVersion = "12.13";

        /// <summary>
        /// Version attribuee a une configuration utilisateur ne portant aucune version.
        /// Volontairement basse : ces fichiers sont les plus anciens et doivent passer
        /// par toutes les etapes de migration.
        /// </summary>
        public const string UserConfigLegacyVersion = "0.0";
    }
}
