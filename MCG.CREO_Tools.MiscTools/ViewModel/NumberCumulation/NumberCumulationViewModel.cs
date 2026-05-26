using MCG.CommonLib;
using MCG.CREO_Tools.MiscTools.View.NumberCumulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.Configuration;

namespace MCG.CREO_Tools.MiscTools.ViewModel.NumberCumulation
{
    public class NumberCumulationViewModel : ObservableObject, INumberCumulationViewModel
    {
        #region [REGION] Properties from Interface
        public NumberCumulationDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private List<NumberCumulationItem> ListItemInProgress { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandPaste { get => new RelayCommand<KeyEventArgs>((obj) => ExecuteCommandPaste(obj)); }
        public ICommand CommandMenuItemPaste { get => new RelayCommand(() => ExecuteMenuItemPaste()); }
        public ICommand CommandUpdateNumberCumul { get => new RelayCommand(() => ExecuteUpdateNumberCumul()); }
        public ICommand CommandRemoveAll { get => new RelayCommand(() => ExecuteRemoveAll()); }
        public ICommand CommandCopy { get => new RelayCommand<string>((obj) => ExecuteCopy(obj)); }
        public ICommand CommandMenuRemoveItem { get => new RelayCommand<NumberCumulationItem>((obj) => ExecuteMenuRemoveItem(obj)); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        #endregion

        #region [REGION] Init
        public NumberCumulationViewModel()
        {
            try
            {

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                CurrentDataContext = new NumberCumulationDataContext();
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteCommandPaste(KeyEventArgs e = null)
        {
            try
            {
                if (e == null || (Keyboard.Modifiers == ModifierKeys.Control && e != null && e.Key == Key.V))
                {
                    ExecuteMenuItemPaste();
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                //CurrentDataContext.ListNumbers.CollectionChanged += ListNumbers_CollectionChanged;
            }
        }

        private void ExecuteMenuItemPaste()
        {
            try
            {

                ListItemInProgress = new List<NumberCumulationItem>();
                string CompleteString = null;
                if (Clipboard.GetData(DataFormats.Text) != null)
                    CompleteString = Clipboard.GetData(DataFormats.Text).ToString();

                if (CompleteString != null)
                {
                    var AllLines = CompleteString.Split('\n');

                    string linePurged = null;
                    string TempNumber;
                    foreach (var line in AllLines)
                    {
                        linePurged = line.Split('\r').FirstOrDefault();
                        var AllValues = linePurged.Split('\t');
                        if (AllValues != null && AllValues.Count() > 0)
                        {
                            TempNumber = AllValues.FirstOrDefault().Trim().ToUpper();
                            if (TempNumber != null && TempNumber.Trim() != "" && TempNumber.Trim() != "*")
                            {
                                if (ListItemInProgress.FirstOrDefault((item) => item.Number == TempNumber) == null)
                                    ListItemInProgress.Add(new NumberCumulationItem() { Number = TempNumber });
                            }
                        }
                    }
                }

                foreach (var number in ListItemInProgress)
                {
                    if (number != null && CurrentDataContext.ListNumbers.FirstOrDefault(item => item.Number == number.Number) == null)
                        CurrentDataContext.ListNumbers.Add(number);
                }

                ExecuteUpdateNumberCumul();

            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                //CurrentDataContext.ListNumbers.CollectionChanged += ListNumbers_CollectionChanged;
            }
        }

        private void ExecuteUpdateNumberCumul()
        {
            try
            {
                CurrentDataContext.CumulNumberSuf = "";
                CurrentDataContext.CumulNumberSufPre = "";
                CurrentDataContext.CumulNumberPre = "";
                CurrentDataContext.CumulNumberOnly = "";
                int index = 0;
                string number;
                foreach (var item in CurrentDataContext.ListNumbers)
                {
                    number = item.Number;
                    if (!string.IsNullOrWhiteSpace(number))
                    {
                        if (index == 0)
                        {
                            CurrentDataContext.CumulNumberSuf = $"{number}*";
                            CurrentDataContext.CumulNumberSufPre = $"*{number}*";
                            CurrentDataContext.CumulNumberPre = $"*{number}";
                            CurrentDataContext.CumulNumberOnly = $"{number}";
                        }
                        else
                        {
                            CurrentDataContext.CumulNumberSuf = $"{CurrentDataContext.CumulNumberSuf};{number}*";
                            CurrentDataContext.CumulNumberSufPre = $"{CurrentDataContext.CumulNumberSufPre};*{number}*";
                            CurrentDataContext.CumulNumberPre = $"{CurrentDataContext.CumulNumberPre};*{number}";
                            CurrentDataContext.CumulNumberOnly = $"{CurrentDataContext.CumulNumberOnly};{number}";
                        }
                        index++;
                    }
                }


            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveAll()
        {
            try
            {
                if (MessageBox.Show(McgWpfTools.GetStringResource("NBC_MsgRemoveAll"), McgWpfTools.GetStringResource("NBC_TitleRemoveAll"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    CurrentDataContext.ListNumbers.Clear();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopy(string Val)
        {
            try
            {
                Clipboard.Clear();
                switch (Val)
                {
                    case "ONLY":
                        Clipboard.SetData(DataFormats.Text, CurrentDataContext.CumulNumberOnly);
                        break;
                    case "PRE":
                        Clipboard.SetData(DataFormats.Text, CurrentDataContext.CumulNumberPre);
                        break;
                    case "SUF":
                        Clipboard.SetData(DataFormats.Text, CurrentDataContext.CumulNumberSuf);
                        break;
                    case "SUFPRE":
                        Clipboard.SetData(DataFormats.Text, CurrentDataContext.CumulNumberSufPre);
                        break;
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenuRemoveItem(NumberCumulationItem obj)
        {
            try
            {
                if(CurrentDataContext.SelectedItem != null)
                {
                    CurrentDataContext.ListNumbers.Remove(CurrentDataContext.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }

        }

        private void ExecuteOpenHelp()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("NBC_UserGuide"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
