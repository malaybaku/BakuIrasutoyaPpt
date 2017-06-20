using System.Windows.Forms.Integration;
using System.Windows.Forms;
using System.IO;
using Microsoft.Office.Interop.PowerPoint;

namespace Baku.IrasutoyaPpt
{
    public partial class ThisAddIn
    {
        internal static ThisAddIn Instance { get; private set; } = null;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Instance = this;
            PrepareCustomControl();

        }


        private void PrepareCustomControl()
        {
            var userControl = new UserControl();
            userControl.Controls.Add(new ElementHost()
            {
                Child = new IrasutoyaSearchPane()
                {
                    DataContext = new IrasutoyaSearchViewModel()
                },
                Dock = DockStyle.Fill
            });

            var pane = this.CustomTaskPanes.Add(userControl, "Irasutoya Search");
            pane.Visible = true;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO で生成されたコード

        /// <summary>
        /// デザイナーのサポートに必要なメソッドです。
        /// このメソッドの内容をコード エディターで変更しないでください。
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion

        public void AddImageToCurrentSlide(byte[] binImage)
        {
            string fileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            File.WriteAllBytes(fileName, binImage);

            (this.Application?.ActiveWindow?.View?.Slide as Slide)
                ?.Shapes
                ?.AddPicture(
                fileName, 
                Microsoft.Office.Core.MsoTriState.msoFalse, 
                Microsoft.Office.Core.MsoTriState.msoTrue,
                100, 100
                );
        }

        public void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Irasutoya Search", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
