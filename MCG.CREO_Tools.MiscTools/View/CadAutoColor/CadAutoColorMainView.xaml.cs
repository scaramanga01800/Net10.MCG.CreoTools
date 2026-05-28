using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor;
using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColr;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.CadAutoColor
{
    public partial class CadAutoColorMainView : RibbonWindow
    {
        private CadAutoColorViewModel CurrentDataContext { get; set; }

        public CadAutoColorMainView(CadAutoColorViewModel currentViewModel)
        {
            try
            {
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"CadAutoColorMainView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);
                CurrentDataContext = currentViewModel;
                DataContext = currentViewModel;

                CurrentDataContext.CurrentDataContext.ListItem.CollectionChanged += new NotifyCollectionChangedEventHandler((newsender, newe) => SubscribeToIsSelectedEvent(DgPartsMaterial, newe));
                CurrentDataContext.CurrentDataContext.ListItemName.CollectionChanged += new NotifyCollectionChangedEventHandler((newsender, newe) => SubscribeToIsSelectedEvent(DgPartsName, newe));
                CurrentDataContext.CurrentDataContext.ListItemPart.CollectionChanged += new NotifyCollectionChangedEventHandler((newsender, newe) => SubscribeToIsSelectedEvent(DgPartsPart, newe));

                InitializeComponent();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }


        #region [REGION] Methods for Multiselection with Shift
        public int PreviousSelectedIndex { get; set; } = -1;
        public int SelectedIndex { get; set; } = -1;
        public bool IsMultiSelectionInProgress { get; set; } = false;

        private void SubscribeToIsSelectedEvent(object sender, EventArgs e)
        {
            try
            {
                if (e != null && e.GetType() == typeof(NotifyCollectionChangedEventArgs))
                {
                    var item = (NotifyCollectionChangedEventArgs)e;
                    if (item.NewItems != null && item.NewItems.Count > 0)
                    {
                        var FirstItem = item.NewItems[0];
                        if (FirstItem.GetType() == typeof(CadAutoColorItem))
                        {
                            //((CadAutoColorItem)FirstItem).IsSelectedEvent += CheckIfMultiselection;
                            ((CadAutoColorItem)FirstItem).IsSelectedEvent += new EventHandler((newsender, newe) => CheckIfMultiselectionAuto(sender, e));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void CheckIfMultiselectionAuto(object sender, EventArgs e)
        {
            try
            {
                if (!IsMultiSelectionInProgress)
                {
                    if (sender.GetType() == typeof(DataGrid) && e != null && e.GetType() == typeof(NotifyCollectionChangedEventArgs))
                    {
                        DataGrid CurrentDg = sender as DataGrid;
                        var item = (NotifyCollectionChangedEventArgs)e;
                        if (item.NewItems != null && item.NewItems.Count > 0)
                        {
                            var FirstItem = item.NewItems[0];
                            if (FirstItem.GetType() == typeof(CadAutoColorItem))
                            {
                                CadAutoColorItem CurrentItem = FirstItem as CadAutoColorItem;
                                if (Keyboard.Modifiers == ModifierKeys.Shift)
                                {
                                    SelectedIndex = GetSelectedIndexAuto(CurrentItem, CurrentDg);
                                    MultiSelectionActionAuto(CurrentItem.IsSelected, CurrentDg);
                                }
                                else
                                    PreviousSelectedIndex = GetSelectedIndexAuto(CurrentItem, CurrentDg);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        public void MultiSelectionActionAuto(bool SelectedValue, DataGrid CurrentDg)
        {
            try
            {
                IsMultiSelectionInProgress = true;
                for (int index = Math.Min(PreviousSelectedIndex, SelectedIndex); index <= Math.Max(PreviousSelectedIndex, SelectedIndex); index++)
                    ((CadAutoColorItem)CurrentDg.Items[index]).IsSelected = SelectedValue;
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

        private int GetSelectedIndexAuto(object SelectedItem, DataGrid CurrentDg)
        {
            try
            {
                int CurrentIndex = 0;
                if (CurrentDg.Items != null)
                {
                    foreach (var item in CurrentDg.Items)
                    {
                        if (item.GetHashCode() == SelectedItem.GetHashCode())
                            return CurrentIndex;
                        CurrentIndex++;
                    }
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
