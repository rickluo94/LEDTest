using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using LEDTest.Models;

namespace LEDTest
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        //宣告 10ms Timer
        DispatcherTimer Timer_1000ms = new DispatcherTimer();


        public MainWindow()
        {
            InitializeComponent();
            //設定呼叫間隔時間為10ms
            Timer_1000ms.Interval = TimeSpan.FromMilliseconds(1000);
            //加入callback function
            Timer_1000ms.Tick += Timer_1000ms_Function;
        }

        private async void Timer_1000ms_Function(object sender, EventArgs e)
        {
            Task.Delay(1000).Wait();
            _write();
            Task.Delay(1000).Wait();
            _clear();
        }

        private bool _write()
        {
            LedModel _ledModel = new LedModel();
            _ledModel.ComPort = 3;
            _ledModel.BlockWidth = 40;
            _ledModel.TrayAreaCount_X = 8080 / 1000;
            if (_ledModel.SetParameter("測試") == true)
            {
                _ledModel.Connect();
                string _trayArea = AreaName.Text;
                string Goods_Id = GoodsId.Text;
                _ledModel.Active(_trayArea, "|" + Goods_Id + "|");
                _ledModel.Disconnect();
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool _clear()
        {
            LedModel _ledModel = new LedModel();
            _ledModel.ComPort = 3;
            _ledModel.BlockWidth = 40;
            _ledModel.LedMatrixIndicatorReadinessLOGO = "STE鈞耀科技測試用LOGO";
            _ledModel.Connect();
            _ledModel.Cleaner();
            _ledModel.Disconnect();
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LedModel _ledModel = new LedModel();
            _ledModel.ComPort = 3;
            _ledModel.BlockWidth = 40;
            _ledModel.TrayAreaCount_X = 8080 / 1000;
            if (_ledModel.SetParameter("測試") == true)
            {
                _ledModel.Connect();
                string _trayArea = AreaName.Text;
                string Goods_Id = GoodsId.Text;
                _ledModel.Active(_trayArea, "|" + Goods_Id + "|");
                _ledModel.Disconnect();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LedModel _ledModel = new LedModel();
            _ledModel.ComPort = 3;
            _ledModel.BlockWidth = 40;
            _ledModel.LedMatrixIndicatorReadinessLOGO = "STE鈞耀科技測試用LOGO";
            _ledModel.Connect();
            _ledModel.Cleaner();
            _ledModel.Disconnect();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Timer_1000ms.Start();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Timer_1000ms.Stop();
        }
    }
}
