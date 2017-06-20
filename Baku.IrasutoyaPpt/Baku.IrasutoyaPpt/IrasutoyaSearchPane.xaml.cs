using System.Windows.Controls;

namespace Baku.IrasutoyaPpt
{
    /// <summary>
    /// IrasutoyaSearchPane.xaml の相互作用ロジック
    /// </summary>
    public partial class IrasutoyaSearchPane : UserControl
    {
        public IrasutoyaSearchPane()
        {
            InitializeComponent();

            DispatcherHolder.UIDispatcher = this.Dispatcher;
        }
    }
}
