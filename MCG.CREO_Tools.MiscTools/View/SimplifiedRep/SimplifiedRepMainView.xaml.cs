using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.ViewModel.SimplifiedRep;
using System.IO;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.SimplifiedRep
{
    public partial class SimplifiedRepMainView : RibbonWindow
    {
        public SimplifiedRepViewModel CurrentDataContext { get; set; }

        public SimplifiedRepMainView(SimplifiedRepViewModel currentViewModel, ISharedAppContext sharedAppContext)
        {
            try
            {
                TraceLog.AddTraceLog("Create SimplifiedRepMainView");
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"SimplifiedRepMainView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);
                CurrentDataContext = currentViewModel;
                DataContext = CurrentDataContext;

                InitializeComponent();
                McgWpfTools.UpdateMergeDictionaries(sharedAppContext.CurrentLanguage?.Language?.CultureInfo?.Substring(0, 2));

                // La grille est alimentee de facon asynchrone : il faut se réabonner
                // a chaque renouvellement de la collection.
                CurrentDataContext.CurrentDataContext.ListItem.CollectionChanged +=
                    (sender, e) => SubscribeToIsIncludedEvent();

                SubscribeToIsIncludedEvent();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        #region [REGION] Methods for Multiselection with Shift
        /// <summary>
        /// Transmet la selection courante au view model : le ruban s'appuie dessus pour
        /// activer les actions de masse et calculer les representations communes.
        /// </summary>
        private void DgComponents_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                CurrentDataContext.UpdateSelection(DgComponents.SelectedItems
                                                                .OfType<SimplifiedRepComponentItem>());
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private int PreviousSelectedIndex { get; set; } = -1;
        private int SelectedIndex { get; set; } = -1;
        private bool IsMultiSelectionInProgress { get; set; } = false;

        /// <summary>
        /// Abonne chaque ligne de la grille au suivi des changements de la case "Inclus".
        /// </summary>
        private void SubscribeToIsIncludedEvent()
        {
            try
            {
                foreach (var item in CurrentDataContext.CurrentDataContext.ListItem)
                {
                    item.IsIncludedEvent -= CheckIfMultiselection;
                    item.IsIncludedEvent += CheckIfMultiselection;
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Applique la meme valeur a toutes les lignes comprises entre la derniere case cochee
        /// et la case cliquee lorsque la touche MAJ est enfoncee.
        /// </summary>
        private void CheckIfMultiselection(object? sender, EventArgs e)
        {
            try
            {
                if (IsMultiSelectionInProgress || sender is not SimplifiedRepComponentItem currentItem) return;

                if (Keyboard.Modifiers == ModifierKeys.Shift && PreviousSelectedIndex >= 0)
                {
                    SelectedIndex = GetSelectedIndex(currentItem);
                    MultiSelectionAction(currentItem.IsIncluded);
                }
                else
                {
                    PreviousSelectedIndex = GetSelectedIndex(currentItem);
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        /// <summary>Propage la valeur sur toute la plage de lignes selectionnee.</summary>
        private void MultiSelectionAction(bool includedValue)
        {
            try
            {
                IsMultiSelectionInProgress = true;

                var items = CurrentDataContext.CurrentDataContext.ListItem;
                var firstIndex = Math.Max(0, Math.Min(PreviousSelectedIndex, SelectedIndex));
                var lastIndex = Math.Min(items.Count - 1, Math.Max(PreviousSelectedIndex, SelectedIndex));

                for (int index = firstIndex; index <= lastIndex; index++)
                    items[index].IsIncluded = includedValue;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
            finally
            {
                IsMultiSelectionInProgress = false;
            }
        }

        /// <summary>Retourne la position de la ligne dans la grille.</summary>
        private int GetSelectedIndex(object selectedItem)
        {
            try
            {
                int currentIndex = 0;

                foreach (var item in CurrentDataContext.CurrentDataContext.ListItem)
                {
                    if (ReferenceEquals(item, selectedItem))
                        return currentIndex;

                    currentIndex++;
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
