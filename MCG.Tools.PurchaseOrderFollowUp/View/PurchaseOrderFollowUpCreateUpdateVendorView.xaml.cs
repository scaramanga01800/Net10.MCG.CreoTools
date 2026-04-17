using Fluent;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    /// <summary>
    /// Logique d'interaction pour PurchaseOrderFollowUpCreateUpdateVendorView.xaml
    /// </summary>
    public partial class PurchaseOrderFollowUpCreateUpdateVendorView : RibbonWindow
    {
        private List<System.Windows.Controls.TextBox> textBoxList { get; set; } = new List<System.Windows.Controls.TextBox>();

        public PurchaseOrderFollowUpCreateUpdateVendorView()
        {
            try
            {
                InitializeComponent();
                textBoxList.Add(Iban1);
                textBoxList.Add(Iban2);
                textBoxList.Add(Iban3);
                textBoxList.Add(Iban4);
                textBoxList.Add(Iban5);
                textBoxList.Add(Iban6);
                textBoxList.Add(Iban7);
                textBoxList.Add(Iban8);
                textBoxList.Add(Iban9);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }


        #region [REGION] Methods for Drag and Drop
        private void MainSP_Drop(object sender, System.Windows.DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Collapsed;
        }

        private void MainSP_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Visible;
        }

        private void MainSP_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region [REGION] Methods for Iban 
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                System.Windows.Controls.TextBox currentTextBox = (System.Windows.Controls.TextBox)sender;
                if (!IsValidInput(e.Text))
                    e.Handled = true;

                if (currentTextBox.Text.Length == 4)
                {
                    MoveFocusToNextTextBox(currentTextBox); // Déplacer le focus sur la TextBox suivante
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void MoveFocusToNextTextBox(System.Windows.Controls.TextBox currentTextBox)
        {
            try
            {
                // Trouver l'index de la TextBox actuelle dans la liste des TextBox
                int currentTextBoxIndex = textBoxList.IndexOf(currentTextBox);

                // Si la TextBox actuelle est la dernière dans la liste, ne rien faire
                if (currentTextBoxIndex == textBoxList.Count - 1)
                {
                    return;
                }

                // Déplacer le focus sur la TextBox suivante
                System.Windows.Controls.TextBox nextTextBox = textBoxList[currentTextBoxIndex + 1];
                nextTextBox.IsEnabled = true;
                nextTextBox.Focus();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private bool IsValidInput(string input)
        {
            try
            {
                // Vérifie si l'entrée est vide
                if (string.IsNullOrEmpty(input))
                {
                    return true;
                }

                // Vérifie si l'entrée contient uniquement des chiffres de 0 à 9 ou des lettres de A à Z (sans accents)
                return input.All(c => char.IsDigit(c) || char.IsLetter(c) && (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z'));
            }
            catch (Exception ex)
            {
              throw new  PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void Iban1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
                {
                    string clipboardText = System.Windows.Forms.Clipboard.GetText();
                    SplitIbanText(clipboardText);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void SplitIbanText(string inputString)
        {
            try
            {
                List<string> stringList = new List<string>();
                string regexPattern = @"^[A-Z]{2}\d{2}[A-Z\d]*$";

                if (inputString != null && inputString.Length > 0)
                {
                    inputString = inputString.Replace(" ", "").ToUpper();
                    if (Regex.IsMatch(inputString, regexPattern))
                    {
                        // Split IBAN in 4 carac string
                        for (int i = 0; i < inputString.Length; i += 4)
                        {
                            if (i + 4 <= inputString.Length)
                            {
                                stringList.Add(inputString.Substring(i, 4)); // Ajouter la sous-chaîne à la liste
                            }
                            else if (i < inputString.Length)
                            {
                                stringList.Add(inputString.Substring(i)); // Ajouter la dernière sous-chaîne à la liste
                            }
                        }

                        // Update Ibans TextBox

                        for (int index = 0; index < stringList.Count; index++)
                        {
                            if (index <= 9)
                            {
                                System.Windows.Controls.TextBox nextTextBox = textBoxList[index];
                                nextTextBox.IsEnabled = true;
                                nextTextBox.Text = stringList[index];
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                string clipboardText = System.Windows.Forms.Clipboard.GetText();
                SplitIbanText(clipboardText);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

    }
}
