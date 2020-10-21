using System;
using System.Collections.Generic;
using System.Text;
using LEDmatrixSDK;

namespace LEDTest.Models
{
    public class LedModel
    {
        /// <summary>
        /// 連線句柄
        /// </summary>
        uint m_dwCurHand;
        public int type;
        public uint i;
        public byte[] AreaText;
        public Led5kSDK.bx_5k_area_header bx_5k;
        public string AreaTxt;

        public string LedMatrixIndicatorReadinessLOGO = "STE鈞耀科技";

        /// <summary>
        /// 字幕機安裝出口0是前1是後
        /// </summary>
        public int LedMatrixEquipmentExports = 0;

        /// <summary>
        /// 字幕機通訊ComPort
        /// </summary>
        private int _comPort;
        public int ComPort
        {
            get { return _comPort; }
            set { _comPort = value; }
        }

        /// <summary>
        /// 托盤寬有幾個分隔
        /// </summary>
        public int _trayAreaCount_X;
        public int TrayAreaCount_X
        {
            get { return _trayAreaCount_X; }
            set { _trayAreaCount_X = value; }
        }

        /// <summary>
        /// 字幕機區域寬度(LED板上的方塊寬度以塊為單位)
        /// </summary>
        private int _blockWidth;

        public int BlockWidth
        {
            get { return _blockWidth; }
            set { _blockWidth = value; }
        }

        /// <summary>
        /// 箭頭區域指示座標
        /// </summary>
        private double _reaX { get; set; }

        /// <summary>
        /// 物料編號指示座標
        /// </summary>
        private double _reaX2 { get; set; }

        /// <summary>
        /// 箭頭區域寬度
        /// </summary>
        private int _areaWidth { get; set; }

        /// <summary>
        /// 物料區域寬度
        /// </summary>
        private int _areaWidth2 { get; set; }

        /// <summary>
        /// Led微調參數X
        /// </summary>
        public static int Fine_tuning_Left_X = 0;

        /// <summary>
        /// Led微調參數Y
        /// </summary>
        public int Fine_tuning_Left_Y = 0;


        public bool SetParameter(string TrayCode)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(TrayCode))
                {
                    //ste_wms _ste_wms = new ste_wms();
                    //DataTable _trayParameter = new DataTable();
                    //int _trayStyleCode = 0;

                    ////設定托盤格式
                    //_trayParameter = _ste_wms.Tray_X_By_TrayCode(TrayCode);

                    //_trayStyleCode = Convert.ToInt32(_trayParameter.Rows[0]["Tray_StyleCode"].ToString());

                    _trayAreaCount_X = 8080 / 1000;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }


        /// <summary>
        /// 485通訊連線
        /// </summary>
        public void Connect()
        {
            //485字幕機初始化及呼叫連線
            LEDmatrixSDK.Led5kSDK.InitSdk(2, 2);
            //屏幕
            ushort ScreenID = Convert.ToUInt16(1);//最小是1必填
            //通訊COM Port
            byte com = Convert.ToByte(_comPort);
            //控制卡版本
            byte[] card_type_list = new byte[12];
            card_type_list[0] = 0xFE;
            card_type_list[1] = 0x51;
            //傳輸速率值
            uint[] baudrate_list = new uint[2];
            baudrate_list[0] = 9600;
            baudrate_list[1] = 57600;
            //連線參數句柄
            uint hand = LEDmatrixSDK.Led5kSDK.CreateComClient(com, baudrate_list[1], (Led5kSDK.bx_5k_card_type)card_type_list[1], 0, ScreenID);
            m_dwCurHand = hand;
            if (hand == 0)
            {
                
            }
        }

        public Tuple<int, int> TextContentChinese(string strText)
        {
            string str = strText;
            int strlength = str.Length;
            byte[] AllByte = System.Text.Encoding.GetEncoding(950).GetBytes(str);
            int ByteLength = AllByte.Length;
            int ChineseSUM = ByteLength - strlength;
            return Tuple.Create(ChineseSUM, strlength - ChineseSUM);//中文字數,英數字數
        }
        public void Cleaner()
        {
            //句柄加上255為刪除全區域
            int err = LEDmatrixSDK.Led5kSDK.SCREEN_DelDynamicArea(m_dwCurHand, 255);
            if (err == 0)
            {
                Tuple<int, int> ContentSUM = null;
                string _LOGO_TEXT = LedMatrixIndicatorReadinessLOGO;
                ContentSUM = TextContentChinese(_LOGO_TEXT);
                _areaWidth2 = (ContentSUM.Item1 * 16) + (ContentSUM.Item2 * 8);
                _reaX2 = 0;
                SendGoodSIdText(_LOGO_TEXT);
                UpdateArea(1);
            }
            else
            {
                //MessageBox.Show("刪除動態區域失敗" + err);
            }
        }

        public void Active(string AreaName, string Goods_Id)
        {
            //句柄加上255為刪除全區域
            int err = LEDmatrixSDK.Led5kSDK.SCREEN_DelDynamicArea(m_dwCurHand, 255);
            if (err == 0)
            {
                //MessageBox.Show("刪除動態區域成功" + err);
            }
            else
            {
                //MessageBox.Show("刪除動態區域失敗" + err);
            }
            LedMatrixIndicatorSelectionChanged(AreaName, Goods_Id);
            SendAreaNameText(AreaName);
            UpdateArea(0);
            SendGoodSIdText(Goods_Id);
            UpdateArea(1);
        }

        /// <summary>
        /// 計算托盤顯示座標
        /// 依照不同出口給予指示區域
        /// </summary>
        /// <param name="AreaName"></param>
        public void LedMatrixIndicatorSelectionChanged(string AreaName, string Goods_Id)
        {
            int LedAreaposition = 0;
            //格位寬度=托盤寬度/總行數
            double tableArea = 125 / _trayAreaCount_X;
            //區域極限
            int MaxArea = _blockWidth;
            //指示區域
            if (LedMatrixEquipmentExports == 1)
            {
                LedAreaposition = Math.Abs(Convert.ToInt16(AreaName.Substring(1)) - _trayAreaCount_X - 1);
            }
            else
            {
                LedAreaposition = Convert.ToInt16(AreaName.Substring(1));
            }

            //箭頭區域指示座標X
            double ordinate = ((tableArea * LedAreaposition) - (tableArea / 2)) / 0.375;
            _reaX = Math.Round(ordinate);
            _areaWidth2 = Goods_Id.Length * 8;
            _areaWidth = (AreaName.Length * 8) + 16;
            //剩餘能顯示範圍LED塊數
            double lastArea = MaxArea - Math.Round(((ordinate + _areaWidth) / 8));
            //設定物料顯示區座標
            if (lastArea < Goods_Id.Length)
            {
                _reaX2 = Math.Round(ordinate) - _areaWidth2;
            }
            else
            {
                _reaX2 = Math.Round(ordinate) + _areaWidth;
            }
        }

        /// <summary>
        /// 設置送出箭頭區文字參數
        /// </summary>
        /// <param name="AreaName"></param>
        public void SendAreaNameText(string AreaName)
        {
            bx_5k.DynamicAreaLoc = Convert.ToByte("0");//選擇區域可省略
            bx_5k.AreaType = 0x06;
            bx_5k.AreaX = (ushort)((_reaX + Fine_tuning_Left_X) / 8);
            bx_5k.AreaY = 0;
            bx_5k.AreaWidth = (ushort)(_areaWidth / 8);
            bx_5k.AreaHeight = 16;
            bx_5k.Lines_sizes = Convert.ToByte(0);
            //運行模式
            bx_5k.RunMode = Convert.ToByte(0);
            //超時
            bx_5k.Timeout = 2;
            //未知變數
            bx_5k.Reserved1 = 0;
            bx_5k.Reserved2 = 0;
            bx_5k.Reserved3 = 0;
            //是否單行
            bx_5k.SingleLine = Convert.ToByte(0x01);
            //換行方式
            bx_5k.NewLine = Convert.ToByte(0x01);
            //文字動畫
            bx_5k.DisplayMode = Convert.ToByte(0x06);//向下移動
            bx_5k.ExitMode = 0x00;//退出方式
            bx_5k.Speed = Convert.ToByte(10);//速度
            bx_5k.StayTime = Convert.ToByte(10);//停留
            AreaTxt = ("↓" + AreaName).Trim();
            //需要將文字轉為GBK格式
            AreaText = System.Text.Encoding.GetEncoding("GBK").GetBytes(("↑" + AreaName).Trim());//↓
            bx_5k.DataLen = AreaText.Length;
        }

        /// <summary>
        /// 設置送出物料區文字參數
        /// </summary>
        /// <param name="Goods_Id"></param>
        public void SendGoodSIdText(string Goods_Id)
        {
            bx_5k.DynamicAreaLoc = Convert.ToByte("1");//選擇區域可省略
            bx_5k.AreaType = 0x06;
            bx_5k.AreaX = (ushort)((_reaX2 + Fine_tuning_Left_X) / 8);//區域二座標
            bx_5k.AreaY = 0;
            bx_5k.AreaWidth = (ushort)(_areaWidth2 / 8);//區域寬度
            bx_5k.AreaHeight = 16;
            bx_5k.Lines_sizes = Convert.ToByte(0);
            //運行模式
            bx_5k.RunMode = Convert.ToByte(0);
            //超時
            bx_5k.Timeout = 2;
            //未知變數
            bx_5k.Reserved1 = 0;
            bx_5k.Reserved2 = 0;
            bx_5k.Reserved3 = 0;
            //是否單行
            bx_5k.SingleLine = Convert.ToByte(0x01);
            //換行方式
            bx_5k.NewLine = Convert.ToByte(0x01);
            //文字動畫
            bx_5k.DisplayMode = Convert.ToByte(0x01);//靜止顯示
            bx_5k.ExitMode = 0x00;//退出方式
            bx_5k.Speed = Convert.ToByte(10);//速度
            bx_5k.StayTime = Convert.ToByte(10);//停留
            AreaTxt = Goods_Id.Trim();
            //需要將文字轉為GBK格式
            AreaText = System.Text.Encoding.GetEncoding("GBK").GetBytes(Goods_Id.Trim());
            bx_5k.DataLen = AreaText.Length;
        }

        /// <summary>
        /// 更新區域文字
        /// </summary>
        /// <param name="i"></param>
        public void UpdateArea(uint i)
        {

            //動態區域區分
            bx_5k.DynamicAreaLoc = (byte)i;
            //送出顯示動態區域文字
            int x = LEDmatrixSDK.Led5kSDK.SCREEN_SendDynamicArea(m_dwCurHand, bx_5k, (ushort)bx_5k.DataLen, AreaText);
            if (x == 0)
            {

            }
            else
            {
               
            }
        }

        /// <summary>
        /// 中斷485字幕機連線
        /// </summary>
        public void Disconnect()
        {
            LEDmatrixSDK.Led5kSDK.Destroy(m_dwCurHand);
        }
    }
}
