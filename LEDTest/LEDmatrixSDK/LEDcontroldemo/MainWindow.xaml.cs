using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Xml;
using System.Threading;
using LEDmatrixSDK;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace LEDcontroldemo
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 連線句柄
        /// </summary>
        uint m_dwCurHand;
        public int type;
        public uint i;
        public static byte[] AreaText;
        public Led5kSDK.bx_5k_area_header bx_5k;
        public static string AreaTxt;
        public MainWindow()
        {
            InitializeComponent();
            string[] names = SerialPort.GetPortNames();
            //初始化函數
            LEDmatrixSDK.Led5kSDK.InitSdk(2, 2);
        }
       
        //連線
        private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            //屏幕
            ushort ScreenID = Convert.ToUInt16(ScreenID_TextBox.Text);
            //通訊COM Port
            byte com = Convert.ToByte(com_TextBox.Text);
            //控制卡版本
            byte[] card_type_list = new byte[12];
            card_type_list[0] = 0xFE;
            card_type_list[1] = 0x51;
            int t = card_type_ComboBox.SelectedIndex;
            type = t;

            //傳輸速率值
            uint[] baudrate_list = new uint[2];
            baudrate_list[0] = 9600;
            baudrate_list[1] = 57600;
            int bl = baudrate_comboBox.SelectedIndex;

            //必要連線參數句柄
            uint hand = LEDmatrixSDK.Led5kSDK.CreateComClient(com, baudrate_list[bl], (Led5kSDK.bx_5k_card_type)card_type_list[t], 0, ScreenID);
            m_dwCurHand = hand;

            if (hand == 0)
            {
                MessageBox.Show("連接控制器失敗");
            }
            else
            {
                MessageBox.Show("連接控制器成功");
            }
        }

        //485設置
        private void nTime_button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint nTime = Convert.ToUInt32(nTime_textBox.Text);
                LEDmatrixSDK.Led5kSDK.SetTimeout(m_dwCurHand, nTime);
            }
            catch (Exception)
            {
                MessageBox.Show("請輸入超時的時間");
            }
        }

        //計算托盤顯示座標
        private void tableArea_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //格位寬度
            double tableArea = Convert.ToDouble(tableWidth_textBox.Text) / Convert.ToDouble(totalRow_textBox.Text);
            //格位座標
            double[] tableArea_Selected = new double[8];
            tableArea_Selected[0] = 1;
            tableArea_Selected[1] = 2;
            tableArea_Selected[2] = 3;
            tableArea_Selected[3] = 4;
            tableArea_Selected[4] = 5;
            tableArea_Selected[5] = 6;
            tableArea_Selected[6] = 7;
            tableArea_Selected[7] = 8;
            int SEL = tableArea_comboBox.SelectedIndex;

            double ordinate = ((tableArea * tableArea_Selected[SEL]) - (tableArea / 2)) / 0.375;
            double lastArea = Convert.ToDouble(MaxArea_textBox.Text) - Math.Round(((ordinate + Convert.ToDouble(AreaWidth_textBox.Text)) / 8));
            AreaX_textBox.Text = Math.Round(ordinate).ToString();
            AreaWidth2_textBox.Text = Convert.ToString(AreaText2_textBox.Text.Length * 8);
            if (lastArea < AreaText2_textBox.Text.Length)
            {
                AreaX2_textBox.Text = Convert.ToString(Math.Round(ordinate) - Convert.ToDouble(AreaWidth2_textBox.Text));
            }
            else
            {
                AreaX2_textBox.Text = Convert.ToString(Math.Round(ordinate) + Convert.ToDouble(AreaWidth_textBox.Text));
            }
        }

        private void SendText_button_Click(object sender, RoutedEventArgs e)
        {
            
            bx_5k.DynamicAreaLoc = Convert.ToByte("0");//選擇區域可省略
            bx_5k.AreaType = 0x06;
            bx_5k.AreaX = (ushort)(Convert.ToUInt16(AreaX_textBox.Text) / 8);
            bx_5k.AreaY = Convert.ToUInt16(AreaYtextBox.Text);
            bx_5k.AreaWidth = (ushort)(Convert.ToUInt16(AreaWidth_textBox.Text) / 8);
            bx_5k.AreaHeight = Convert.ToUInt16(AreaHeight_textBox.Text);

            bx_5k.Lines_sizes = Convert.ToByte(Lines_sizes_textBox.Text);

            //運行模式
            byte[] RunMode_list = new byte[3];
            RunMode_list[0] = 0;
            RunMode_list[1] = 1;
            RunMode_list[2] = 2;
            int rl = RunMode_comboBox.SelectedIndex;
            bx_5k.RunMode = RunMode_list[rl];
            //bx_5k.RunMode = Convert.ToByte(comboBox3.SelectedIndex+1);
            //超時
            bx_5k.Timeout = Convert.ToInt16(TimeOut_textBox.Text);

            //未知變數
            bx_5k.Reserved1 = 0;
            bx_5k.Reserved2 = 0;
            bx_5k.Reserved3 = 0;

            //是否單行
            byte[] SingleLine_list = new byte[2];
            SingleLine_list[0] = 0x01;
            SingleLine_list[1] = 0x02;
            int sll = SingleLine_comboBox.SelectedIndex;
            bx_5k.SingleLine = SingleLine_list[sll];
            //bx_5k.SingleLine = Convert.ToByte(comboBox1.SelectedIndex);

            //換行方式
            byte[] NewLine_list = new byte[2];
            NewLine_list[0] = 0x01;
            NewLine_list[1] = 0x02;
            int nl = NewLine_comboBox.SelectedIndex;
            bx_5k.NewLine = NewLine_list[nl];
            //bx_5k.NewLine = Convert.ToByte(comboBox2.SelectedIndex);

            //文字動畫
            byte[] DisplayMode_list = new byte[6];
            DisplayMode_list[0] = 0x01;
            DisplayMode_list[1] = 0x02;
            DisplayMode_list[2] = 0x03;
            DisplayMode_list[3] = 0x04;
            DisplayMode_list[4] = 0x05;
            DisplayMode_list[5] = 0x06;
            int dml = DisplayMode_comboBox.SelectedIndex;
            bx_5k.DisplayMode = DisplayMode_list[5];//預設向下移動
            //bx_5k.DisplayMode = Convert.ToByte(comboBox4.SelectedIndex);

            bx_5k.ExitMode = 0x00;


            bx_5k.Speed = (byte)Speed_comboBox.SelectedIndex;
            //bx_5k.Speed=Convert.ToByte(comboBox5.SelectedIndex);

            bx_5k.StayTime = Convert.ToByte(StayTime_textBox.Text);
            AreaTxt = AreaText_textBox.Text.Trim();
            //需要將文字轉為GBK格式
            AreaText = System.Text.Encoding.GetEncoding("GBK").GetBytes(AreaText_textBox.Text.Trim());

            bx_5k.DataLen = AreaText.Length;
            UpdateArea(0);
        }
        private void SendText2_button_Click(object sender, RoutedEventArgs e)
        {

            bx_5k.DynamicAreaLoc = Convert.ToByte("1");//選擇區域可省略
            bx_5k.AreaType = 0x06;
            bx_5k.AreaX = (ushort)(Convert.ToUInt16(AreaX2_textBox.Text) / 8);//區域二座標
            bx_5k.AreaY = Convert.ToUInt16(AreaYtextBox.Text);
            bx_5k.AreaWidth = (ushort)(Convert.ToUInt16(AreaWidth2_textBox.Text) / 8);//區域寬度
            bx_5k.AreaHeight = Convert.ToUInt16(AreaHeight_textBox.Text);

            bx_5k.Lines_sizes = Convert.ToByte(Lines_sizes_textBox.Text);

            //運行模式
            byte[] RunMode_list = new byte[3];
            RunMode_list[0] = 0;
            RunMode_list[1] = 1;
            RunMode_list[2] = 2;
            int rl = RunMode_comboBox.SelectedIndex;
            bx_5k.RunMode = RunMode_list[rl];
            //bx_5k.RunMode = Convert.ToByte(comboBox3.SelectedIndex+1);
            //超時
            bx_5k.Timeout = Convert.ToInt16(TimeOut_textBox.Text);

            //未知變數
            bx_5k.Reserved1 = 0;
            bx_5k.Reserved2 = 0;
            bx_5k.Reserved3 = 0;

            //是否單行
            byte[] SingleLine_list = new byte[2];
            SingleLine_list[0] = 0x01;
            SingleLine_list[1] = 0x02;
            int sll = SingleLine_comboBox.SelectedIndex;
            bx_5k.SingleLine = SingleLine_list[sll];
            //bx_5k.SingleLine = Convert.ToByte(comboBox1.SelectedIndex);

            //換行方式
            byte[] NewLine_list = new byte[2];
            NewLine_list[0] = 0x01;
            NewLine_list[1] = 0x02;
            int nl = NewLine_comboBox.SelectedIndex;
            bx_5k.NewLine = NewLine_list[nl];
            //bx_5k.NewLine = Convert.ToByte(comboBox2.SelectedIndex);

            //文字動畫
            byte[] DisplayMode_list = new byte[6];
            DisplayMode_list[0] = 0x01;
            DisplayMode_list[1] = 0x02;
            DisplayMode_list[2] = 0x03;
            DisplayMode_list[3] = 0x04;
            DisplayMode_list[4] = 0x05;
            DisplayMode_list[5] = 0x06;
            int dml = DisplayMode_comboBox.SelectedIndex;
            bx_5k.DisplayMode = DisplayMode_list[dml];
            //bx_5k.DisplayMode = Convert.ToByte(comboBox4.SelectedIndex);

            bx_5k.ExitMode = 0x00;


            bx_5k.Speed = (byte)Speed_comboBox.SelectedIndex;
            //bx_5k.Speed=Convert.ToByte(comboBox5.SelectedIndex);

            bx_5k.StayTime = Convert.ToByte(StayTime_textBox.Text);

            AreaTxt = AreaText2_textBox.Text.Trim();
            AreaText = System.Text.Encoding.GetEncoding("GBK").GetBytes(AreaText2_textBox.Text.Trim());
            bx_5k.DataLen = AreaText.Length;
            UpdateArea(1);
        }

        public void UpdateArea(uint i)
        {
            
            //動態區域區分
            bx_5k.DynamicAreaLoc = (byte)i;
            //送出顯示動態區域文字
            int x = LEDmatrixSDK.Led5kSDK.SCREEN_SendDynamicArea(m_dwCurHand, bx_5k, (ushort)bx_5k.DataLen, AreaText);
            if (x == 0)
            {
                //MessageBox.Show("動態區域更新成功");
            }
            else
            {
                MessageBox.Show("動態區域更新失敗");
            }
           
        }

        private void Delete_button_Click(object sender, RoutedEventArgs e)
        {
            //句柄加上255為刪除全區域
            int err = LEDmatrixSDK.Led5kSDK.SCREEN_DelDynamicArea(m_dwCurHand, 255);
            if (err == 0)
            {
                //MessageBox.Show("刪除動態區域成功" + err);
            }
            else
            { MessageBox.Show("刪除動態區域失敗" + err); }
            
        }
        
        private void disconnect_button_Click(object sender, RoutedEventArgs e)
        {
            LEDmatrixSDK.Led5kSDK.Destroy(m_dwCurHand);
            MessageBox.Show("ok");
        }

        private void double2_button_Click(object sender, RoutedEventArgs e)
        {
            ButtonAutomationPeer BAP3 = new ButtonAutomationPeer(Delete_button);
            ButtonAutomationPeer BAP = new ButtonAutomationPeer(SendText_button);
            ButtonAutomationPeer BAP2 = new ButtonAutomationPeer(SendText2_button);
            IInvokeProvider IIP3 = BAP3.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            IInvokeProvider IIP = BAP.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            IInvokeProvider IIP2 = BAP2.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            if (IIP != null & IIP2 != null)
            {
                IIP3.Invoke();
                IIP.Invoke();
                IIP2.Invoke();
            }
        }
    }
}
