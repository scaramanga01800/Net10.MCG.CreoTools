using MCG.CREO_Tools.QuickSearch.View;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.QuickSearch.Exceptions;
using MCG.CREO_Tools.QuickSearch.Configuration;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchUpdatePartViewModel : ObservableObject, IQuickSearchUpdatePartViewModel
    {
        #region [REGION] Properties from Interface
        private QuickSearchPart _PartItem;
        public QuickSearchPart PartItem
        {
            get { return _PartItem; }
            set
            {
                if (this._PartItem != value)
                {
                    this._PartItem = value;
                    OnPropertyChanged();
                    if (this._PartItem != null)
                        if (this._PartItem.SubClassItem.CurrentPartSubClass.Showpartpicture!=null && this._PartItem.SubClassItem.CurrentPartSubClass.Showpartpicture.ToUpper() == "TRUE")
                            IsPartPictureShow = true;
                        else
                            IsPartPictureShow = false;
                }

            }
        }

        public MessageBoxResult Return { get; set; } = MessageBoxResult.Cancel;

        private bool _IsPartPictureShow = false;
        public bool IsPartPictureShow
        {
            get { return _IsPartPictureShow; }
            set
            {
                if (this._IsPartPictureShow != value)
                {
                    this._IsPartPictureShow = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        #endregion

        #region [REGION] Commands
        public ICommand CommandCreateUpdatePart { get => new RelayCommand(() => ExecuteCreateUpdatePart()); }
        public ICommand CommandDragAndDropImage { get => new RelayCommand<DragEventArgs>((obj) => ExecuteDragAndDropImage(obj)); }
        public ICommand CommandChangeImage { get => new RelayCommand<QuickSearchPart>((qSearchPart) => ExecuteChangeImage(qSearchPart)); }
        #endregion

        #region [REGION] Init
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteCreateUpdatePart(bool InAsynch = false)
        {
            try
            {
                Return = MessageBoxResult.Yes;
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDragAndDropImage(DragEventArgs obj)
        {
            try
            {
                if (obj != null && obj.Data != null && obj.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])obj.Data.GetData(DataFormats.FileDrop);
                    QuickSearchPart qSearchObj = null;
                    string fileName = null;

                    if (files != null && files.Length > 0)
                    {
                        fileName = files.FirstOrDefault();

                        FileInfo info = new FileInfo(fileName);

                        if (info.Length / 1024 <= QuickSearchConstants.MaxImageFileSize)
                        {

                            if (obj.Source.GetType() == typeof(System.Windows.Controls.Image))
                            {
                                qSearchObj = ((QuickSearchUpdatePartViewModel)((System.Windows.Controls.Image)obj.Source).DataContext).PartItem;
                            }
                            else if (obj.Source.GetType() == typeof(System.Windows.Controls.Button))
                            {
                                qSearchObj = ((QuickSearchUpdatePartViewModel)((System.Windows.Controls.Button)obj.Source).DataContext).PartItem;
                            }

                            if (qSearchObj != null)
                            {
                                byte[] image = File.ReadAllBytes(fileName);
                                qSearchObj.UpdatedPart.Partpicturebin = image;
                                qSearchObj.UpdatedPart.Partpicture = fileName.Split('\\').LastOrDefault();
                                qSearchObj.UpdatedImage = image;
                            }
                        }
                        else
                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("QS_ErrorMsgFileSize"), QuickSearchConstants.MaxImageFileSize, McgWpfTools.GetStringResource("QS_ErrorMsgTitleFileSize"), MessageBoxButton.OK, MessageBoxImage.Warning));
                    }
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteChangeImage(QuickSearchPart qSearchObj)
        {
            try
            {
                if (qSearchObj != null)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.ShowDialog();
                    if (openFileDialog.FileName != null && openFileDialog.FileName != "")
                    {
                        string Filename = openFileDialog.FileName;

                        FileInfo info = new FileInfo(Filename);
                        if (info.Length / 1024 <= QuickSearchConstants.MaxImageFileSize)
                        {
                            byte[] image = File.ReadAllBytes(Filename);
                            qSearchObj.UpdatedPart.Partpicturebin = image;
                            qSearchObj.UpdatedPart.Partpicture = Filename.Split('\\').LastOrDefault();
                            qSearchObj.UpdatedImage = image;
                        }
                        else
                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("QS_ErrorMsgFileSize"), QuickSearchConstants.MaxImageFileSize), McgWpfTools.GetStringResource("QS_ErrorMsgTitleFileSize"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        #endregion

    }
}
