using System;
using System.IO;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing
{
    /// <summary>
    /// Resultat de la preparation du dossier de travail dedie a une operation de mise a jour.
    /// </summary>
    public sealed class ManufacturingUpdateWorkFolderResult
    {
        public required bool Success { get; init; }
        public required string FolderPath { get; init; }
        public string Detail { get; init; } = string.Empty;
    }

    /// <summary>
    /// Prepare et verifie un dossier de travail local dedie et identifiable pour la copie des
    /// modeles Creo pendant la mise a jour Manufacturing.
    ///
    /// Ne depend d'aucune API Creo : uniquement des API .NET standard (Directory, File, DriveInfo).
    /// Le dossier est nomme de facon identifiable (prefixe fixe + horodatage) et n'ecrase jamais un
    /// dossier existant : en cas de collision (tres improbable vu l'horodatage a la seconde), un
    /// suffixe numerique est ajoute.
    /// </summary>
    public static class ManufacturingUpdateWorkFolder
    {
        private const string FolderPrefix = "MCG_ManufacturingUpdate_";

        /// <summary>Espace disque minimal requis, par prudence, pour accueillir la copie de travail.</summary>
        private const long MinimumRequiredFreeBytes = 100L * 1024 * 1024; // 100 Mo

        /// <summary>
        /// Cree un dossier de travail dedie sous <paramref name="baseFolder"/> (ou le dossier temp
        /// utilisateur si non fourni), verifie l'espace disque disponible et les droits d'ecriture
        /// reels (ecriture/suppression d'un fichier de test).
        /// </summary>
        public static ManufacturingUpdateWorkFolderResult PrepareWorkFolder(string? baseFolder = null)
        {
            string root = string.IsNullOrWhiteSpace(baseFolder) ? Path.GetTempPath() : baseFolder;
            string folderName = FolderPrefix + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string candidate = Path.Combine(root, folderName);

            // Ne jamais ecraser silencieusement un dossier existant : en cas de collision, on
            // ajoute un suffixe numerique jusqu'a trouver un nom libre.
            int suffix = 1;
            while (Directory.Exists(candidate))
            {
                candidate = Path.Combine(root, $"{folderName}_{suffix}");
                suffix++;
            }

            try
            {
                var driveCheck = CheckDiskSpace(root);
                if (!driveCheck.Success)
                    return driveCheck;

                Directory.CreateDirectory(candidate);

                var writeCheck = CheckWriteAccess(candidate);
                if (!writeCheck.Success)
                    return writeCheck;

                return new ManufacturingUpdateWorkFolderResult
                {
                    Success = true,
                    FolderPath = candidate,
                    Detail = "Dossier de travail cree et verifie."
                };
            }
            catch (Exception ex)
            {
                return new ManufacturingUpdateWorkFolderResult
                {
                    Success = false,
                    FolderPath = candidate,
                    Detail = $"Impossible de creer le dossier de travail : {ex.Message}"
                };
            }
        }

        private static ManufacturingUpdateWorkFolderResult CheckDiskSpace(string root)
        {
            try
            {
                string driveRoot = Path.GetPathRoot(Path.GetFullPath(root)) ?? root;
                var drive = new DriveInfo(driveRoot);

                if (drive.IsReady && drive.AvailableFreeSpace < MinimumRequiredFreeBytes)
                {
                    return new ManufacturingUpdateWorkFolderResult
                    {
                        Success = false,
                        FolderPath = root,
                        Detail = $"Espace disque insuffisant sur '{driveRoot}' " +
                                 $"({drive.AvailableFreeSpace / (1024 * 1024)} Mo disponibles)."
                    };
                }
            }
            catch
            {
                // Verification best effort : si l'espace disque ne peut pas etre determine
                // (lecteur reseau, chemin UNC non standard, ...), on ne bloque pas l'operation
                // pour autant ; l'echec reel d'ecriture sera detecte par CheckWriteAccess.
            }

            return new ManufacturingUpdateWorkFolderResult { Success = true, FolderPath = root };
        }

        private static ManufacturingUpdateWorkFolderResult CheckWriteAccess(string folderPath)
        {
            string probeFile = Path.Combine(folderPath, ".mcg_write_check.tmp");

            try
            {
                File.WriteAllText(probeFile, "check");
                File.Delete(probeFile);

                return new ManufacturingUpdateWorkFolderResult
                {
                    Success = true,
                    FolderPath = folderPath,
                    Detail = "Droits d'ecriture verifies."
                };
            }
            catch (Exception ex)
            {
                return new ManufacturingUpdateWorkFolderResult
                {
                    Success = false,
                    FolderPath = folderPath,
                    Detail = $"Droits d'ecriture insuffisants sur '{folderPath}' : {ex.Message}"
                };
            }
        }
    }
}
