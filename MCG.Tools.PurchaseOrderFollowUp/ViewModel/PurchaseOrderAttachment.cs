using MCG.Tools.PurchaseOrderFollowUp.View;
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using System;
using System.IO;
using System.Linq;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderAttachment : ObservableObject, IPurchaseOrderAttachment
    {
        #region [REGION] Properties from Interface
        private string _FileName = string.Empty;
        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (this._FileName != value)
                {
                    this._FileName = value;
                    OnPropertyChanged();
                    RaiseIsUpdatedEvent();
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
                    RaiseIsUpdatedEvent();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public byte[] FileContent { get; set; } = Array.Empty<byte>();

        public string CompleteFilename { get; set; } = string.Empty;

        public string TempCompleteFileName { get; set; } = string.Empty;
        public int IdAttachment { get; set; }

        public bool IsDbSaved { get; set; } = false;

        public bool IsRequestFile { get; set; } = false;
        #endregion


        public event EventHandler IsUpdatedEvent;

        public void RaiseIsUpdatedEvent()
        {
            try
            {
                IsUpdatedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }


        #region [REGION] Misc
        public void PurgeIsUpdatedEvent()
        {
            try
            {
                if (IsUpdatedEvent != null)
                {
                    foreach (Delegate d in IsUpdatedEvent.GetInvocationList())
                    {
                        IsUpdatedEvent -= (EventHandler)d;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void Update()
        {
            try
            {
                if (CompleteFilename != null && File.Exists(CompleteFilename))
                {
                    FileName = CompleteFilename.Split('\\').LastOrDefault();
                    ReadFile();
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void ReadFile()
        {
            try
            {
                using (FileStream fs = new FileStream(CompleteFilename, FileMode.Open, FileAccess.Read))
                {
                    FileContent = new byte[fs.Length];
                    fs.Read(FileContent, 0, (int)fs.Length);
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void ReadFileFromMemorystream(MemoryStream MsFileContent)
        {
            try
            {
                FileContent = MsFileContent.ToArray();

                //using (FileStream fs = new FileStream(CompleteFilename, FileMode.Open, FileAccess.Read))
                //{
                //    FileContent = new byte[fs.Length];
                //    fs.Read(FileContent, 0, (int)fs.Length);
                //}
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void WriteFile(string TempFileName = null)
        {
            try
            {
                string TempFolder = System.Environment.GetEnvironmentVariable("TEMP");
                if (TempFileName == null)
                    TempFileName = $"{TempFolder}\\{FileName}";
                TempCompleteFileName = TempFileName;

                using (FileStream fs = new FileStream(TempFileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(FileContent, 0, FileContent.Length);
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void DownloadFile(string TempFileName = null)
        {
            try
            {
                string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string TempFolder = System.IO.Path.Combine(userProfilePath, "Downloads");

                if (TempFileName == null)
                    TempFileName = $"{TempFolder}\\{FileName}";
                TempCompleteFileName = TempFileName;

                using (FileStream fs = new FileStream(TempFileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(FileContent, 0, FileContent.Length);
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void UpdateDbAttachment(PoAttachment pO_ATTACHMENT)
        {
            try
            {
                pO_ATTACHMENT.Attachmentdescription = Description;
                pO_ATTACHMENT.Attachmentfilename = FileName;
                pO_ATTACHMENT.Binary = FileContent;
                pO_ATTACHMENT.Isrequestfile = IsRequestFile;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public PoAttachment GetDbAttachment(int IdRequest)
        {
            try
            {
                PoAttachment pO_ATTACHMENT = new PoAttachment()
                {
                    Attachmentdescription = Description,
                    Attachmentfilename = FileName,
                    Binary = FileContent,
                    Idrequest = IdRequest,
                    Isrequestfile = IsRequestFile
                };
                return pO_ATTACHMENT;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        internal static PurchaseOrderAttachment GetFromDbAttachement(PoAttachment attachment)
        {
            try
            {
                PurchaseOrderAttachment newAttachment = new PurchaseOrderAttachment()
                {
                    FileName = attachment.Attachmentfilename,
                    Description = attachment.Attachmentdescription,
                    FileContent = attachment.Binary,
                    IsRequestFile = attachment.Isrequestfile
                };
                return newAttachment;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException("PurchaseOrderAttachment", ex);
            }
        }

        public PurchaseOrderAttachment Clone()
        {
            try
            {
                PurchaseOrderAttachment clone = new PurchaseOrderAttachment()
                {
                    FileName = this.FileName,
                    Description = this.Description,
                    FileContent = this.FileContent?.ToArray(),
                    CompleteFilename = this.CompleteFilename,
                    TempCompleteFileName = this.TempCompleteFileName,
                    IdAttachment = this.IdAttachment,
                    IsDbSaved = this.IsDbSaved,
                    IsRequestFile = this.IsRequestFile
                };
                return clone;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException("PurchaseOrderItem", ex);
            }

        }
        #endregion
    }
}
