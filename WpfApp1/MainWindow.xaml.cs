using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using Panuon.UI.Silver;
using System.Windows.Threading;
using S7.Net;
using WpfApp1.Entity;
using System.IO;
using Newtonsoft.Json;
using WpfApp1.Services;
using log4net;
using HslCommunication;
using HslCommunication.Profinet.Siemens;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private GetInfoService service = new GetInfoService();
        private DispatcherTimer ShowTimer;
        private DispatcherTimer timer;
        private ConfigData config;
        private BarCodeStr codeStr;
        //private Plc plc;
        private SiemensS7Net splc;
        private OperateResult connect;
        private GDbStr GunStr;
        private int markN = 0;
        private bool remark = false;
        private List<GDbData> ReList = new List<GDbData>();
        private static BitmapImage ILogo = new BitmapImage(new Uri("/Images/logo.png", UriKind.Relative));
        private static BitmapImage IFalse = new BitmapImage(new Uri("/Images/01.png", UriKind.Relative));
        private static BitmapImage ITrue = new BitmapImage(new Uri("/Images/02.png", UriKind.Relative));
        private ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();
            #region 启动时串口最大化显示
            this.WindowState = WindowState.Maximized;
            Rect rc = SystemParameters.WorkArea; //获取工作区大小
            //this.Topmost = true;
            this.Left = 0; //设置位置
            this.Top = 0;
            this.Width = rc.Width;
            this.Height = rc.Height;
            #endregion

            try
            {
                //读取本地配置JSON文件
                LoadJsonData();
                Init();
                //MainPlanLoad();
                //plc = new Plc(CpuType.S71200, config.IpAdress, 0, 1);
                splc = new SiemensS7Net(SiemensPLCS.S1200, config.IpAdress)
                {
                    ConnectTimeOut = 5000
                };
                switch (config.GWNo)
                {
                    case 20:
                        TM_Copy.Text = "CC特性：\r\n \r\n       9 + 1 Nm \r\n       25 + 3 Nm";
                        break;
                    case 40:
                        TM_Copy.Text = "CC特性：\r\n \r\n        \r\n        ";
                        break;
                    case 04052:
                        TM_Copy.Text = "CC特性：\r\n \r\n       8 + 2 Nm\r\n        ";
                        break;
                    case 04053:
                        TM_Copy.Text = "CC特性：\r\n \r\n        \r\n        ";
                        break;
                    case 04061:
                        TM_Copy.Text = "CC特性：\r\n \r\n       2.4 ± 1Nm\r\n       ";
                        break;
                    case 04063:
                        TM_Copy.Text = "CC特性：\r\n \r\n       20 + 5Nm \r\n       ";
                        break;
                }

                connect = splc.ConnectServer();

                #region PLC连接定时器
                timer = new System.Windows.Threading.DispatcherTimer();
                timer.Tick += new EventHandler(ThreadCheck);
                timer.Interval = new TimeSpan(0, 0, 0, 5);
                timer.Start();
                #endregion




                //if (plc.IsAvailable)
                //{

                //    var result = plc.Open();
                //    if (!plc.IsConnected)
                //    {
                //        PLCImage.Source = IFalse;
                //        log.Info("PLC Not Connected!");
                //    }
                //    else
                //    {
                //        PLCImage.Source = ITrue;
                //        log.Info("PLC Connected!");

                //        DataReload();
                //    }
                //}
                //else
                //{
                //    PLCImage.Source = IFalse;
                //    log.Info("PLC Not Connected!");
                //}
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void Init()
        {

            #region 时间定时器
            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowTimer1);
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1);
            ShowTimer.Start();
            #endregion

            var count = config.Station.Count;
            SetStepData(count, config.Station);

            ReList.Clear();
            pImage.Source = new BitmapImage(new Uri(config.ImageUri, UriKind.Absolute)); ;
            Logo.Source = ILogo;

        }

        /// <summary>
        /// 预留主线 计划xxx信息读取,报警信息
        /// </summary>
        private void MainPlanLoad()
        {
            //定时查询-定时器
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (s, e) =>
            {

                PlanNo.Text = "123";
                PlanTime.Text = "123";
                PlanSum.Text = "2";
                Output.Text = "";
                ZongType.Text = "";
            };
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1);
            dispatcherTimer.Start();
        }

        /// <summary>
        /// PLC数据读取处理
        /// </summary>
        private void DataReload()
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (s, e) =>
            {
                try
                {
                    //读取PLC工序步骤状态
                    //var sta = (ushort)plc.Read(service.GetStaStr(config.StationNo));
                    //ModifyStep(sta, config.GWNo);
                    var sta = splc.ReadUInt16(service.GetStaStr(config.StationNo));
                    if (sta.IsSuccess)
                    {
                        ModifyStep(sta.Content, config.GWNo);
                    }
                    else
                    {
                        throw new Exception("工序步骤状态读取失败");
                    }

                    //型号获取  ushort 0405,int 0406
                    //var type = (ushort)plc.Read(service.GetTypeStr(config.ProductNo));
                    //switch (type)
                    //{
                    //    case 1:
                    //        XingHao.Text = "正驾";
                    //        break;
                    //    case 2:
                    //        XingHao.Text = "副驾";
                    //        break;
                    //    default: break;
                    //}
                    var type = splc.ReadUInt16(service.GetTypeStr(config.ProductNo));
                    if (type.IsSuccess)
                    {
                        switch (type.Content)
                        {
                            case 1:
                                XingHao.Text = "正驾";
                                break;
                            case 2:
                                XingHao.Text = "副驾";
                                break;
                            default: break;
                        }
                    }
                    else
                    {
                        throw new Exception("型号读取失败");
                    }

                    if (config.GWNo == 20 || config.GWNo == 40) //旧工位适用
                    {
                        #region OLD BarCode Get 
                        if (config.BarCount > 0)
                        {
                            int k = config.BarNo;  //address get;
                            Barcode1.Text = "";
                            Barcode2.Text = "";
                            Barcode3.Text = "";
                            Barcode4.Text = "";
                            BarYz.Text = "";
                            for (int i = 1; i <= config.BarCount; i++)
                            {
                                var i1 = i + k - 1;
                                if (i1 == k)
                                {
                                    codeStr = service.GetBarCodeStr(k);
                                    //var temp = (string)plc.Read(DataType.DataBlock, 2000, codeStr.BarStr, VarType.String, 40);
                                    //temp = temp.Trim();
                                    //var BarResult = (bool)plc.Read(codeStr.ResultStr);

                                    string temp = null;
                                    bool BarResult = false;
                                    var tempS = splc.ReadString(codeStr.BarStr, 40);
                                    var barS = splc.ReadBool(codeStr.ResultStr);
                                    if (tempS.IsSuccess)
                                    {
                                        temp = tempS.Content.Replace("\0", "").Trim();
                                    }
                                    else
                                    {
                                        throw new Exception("条码读取失败！");
                                    }

                                    if (barS.IsSuccess)
                                    {
                                        BarResult = barS.Content;
                                    }
                                    else
                                    {
                                        throw new Exception("条码比对结果读取失败！");
                                    }

                                    if (!temp.IsNullOrEmpty())
                                    {
                                        switch (i)
                                        {
                                            case 1:
                                                Barcode1.Text = temp;
                                                break;
                                            case 2:
                                                Barcode2.Text = temp;
                                                break;
                                            case 3:
                                                Barcode3.Text = temp;
                                                break;
                                            case 4:
                                                Barcode4.Text = temp;
                                                break;
                                        }
                                        if (BarResult)
                                        {
                                            BarYz.Text = "比对成功";
                                        }
                                        else
                                        {
                                            BarYz.Text = "比对失败";
                                        }
                                        k += 1;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    else //新工位（405,406）
                    {
                        #region New BarCodeGet
                        int startAddr = service.GetNewBarCodeStr(config.GWNo);
                        Barcode1.Text = "";
                        Barcode2.Text = "";
                        Barcode3.Text = "";
                        Barcode4.Text = "";
                        BarYz.Text = "";
                        for (int i = 1; i <= 4; i++)
                        {
                            if (i != 1)
                            {
                                startAddr += 72;
                            }
                            string tStr = "DB2000." + startAddr;

                            //var temp = (string)plc.Read(DataType.DataBlock, 2000, startAddr, VarType.String, config.BarLengh);
                            //temp = temp.Trim();
                            string temp = null;
                            var tempS = splc.ReadString(tStr, config.BarLengh);
                            if (tempS.IsSuccess)
                            {
                                temp = tempS.Content.Replace("\0", "").Trim();
                                if (temp.Length > 1)
                                {
                                    temp = temp.Substring(1, temp.Length - 1);
                                }
                                else
                                {
                                    temp = null;
                                }

                            }
                            else
                            {
                                throw new Exception("条码读取失败！");
                            }

                            if (!temp.IsNullOrEmpty())
                            {
                                switch (i)
                                {
                                    case 1:
                                        Barcode1.Text = temp;
                                        break;
                                    case 2:
                                        Barcode2.Text = temp;
                                        break;
                                    case 3:
                                        Barcode3.Text = temp;
                                        break;
                                    case 4:
                                        Barcode4.Text = temp;
                                        break;
                                }
                                BarYz.Text = "比对成功";
                            }
                        }
                        #endregion
                    }

                    #region 拧紧枪数据获取
                    ReList.Clear();
                    if (config.GunCount > 0)
                    {
                        for (int i = 1; i <= config.GunCount; i++)
                        {
                            var i1 = i + config.GunNo - 1;
                            GunStr = service.GetGunStr(i1);
                            //var torque1 = ((uint)plc.Read(GunStr.TorqueStr)).ConvertToDouble();
                            //torque1 = double.Parse(torque1.ToString("F2"));
                            //var angle1 = ((uint)plc.Read(GunStr.AngleStr)).ConvertToDouble();
                            //angle1 = double.Parse(angle1.ToString("F2"));
                            //var result1 = (bool)plc.Read(GunStr.ResultStr);
                            double torque1 = 0;
                            double angle1 = 0;
                            bool result1 = false;
                            var t = splc.ReadDouble(GunStr.TorqueStr);
                            var a = splc.ReadDouble(GunStr.AngleStr);
                            var r = splc.ReadBool(GunStr.ResultStr);
                            if (t.IsSuccess)
                            {
                                torque1 = double.Parse(t.Content.ToString("F2"));
                            }
                            else
                            {
                                throw new Exception("扭矩读取失败！");
                            }

                            if (a.IsSuccess)
                            {
                                angle1 = double.Parse(a.Content.ToString("F2"));

                                angle1 = Math.Round(angle1, 2);
                            }
                            else
                            {
                                throw new Exception("角度读取失败！");
                            }

                            if (r.IsSuccess)
                            {
                                result1 = r.Content;
                            }
                            else
                            {
                                throw new Exception("结果读取失败！");
                            }

                            string rest;
                            if (torque1 != 0)
                            {
                                if (i == 1)
                                {
                                    ReList.Clear();
                                    markN = 0;
                                }
                                if (result1)
                                {
                                    rest = "OK";
                                }
                                else
                                {
                                    rest = "NG";
                                }
                                markN += 1;
                                ReList.Add(new GDbData(markN, torque1, angle1, rest));
                                ReList.Sort((x, y) => -x.Num.CompareTo(y.Num));
                                DataList.ItemsSource = null;
                                DataList.ItemsSource = ReList;
                                DataList.Items.Refresh();
                            }
                        }
                    }
                    #endregion

                    remark = true;
                }
                catch (Exception exc)
                {
                    log.Error("------PLC访问出错------");
                    log.Error(exc.Message);
                    dispatcherTimer.Stop();
                    remark = false;
                }

            };
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(2);
            dispatcherTimer.Start();
        }

        public void ShowTimer1(object sender, EventArgs e)
        {
            this.TM.Text = " ";
            //获得年月日 
            this.TM.Text += DateTime.Now.ToString("yyyy年MM月dd日");   //yyyy年MM月dd日 
            this.TM.Text += "\r\n       ";
            //获得时分秒 
            this.TM.Text += DateTime.Now.ToString("HH:mm:ss");
            this.TM.Text += "              ";
            this.TM.Text += DateTime.Now.ToString("dddd", new System.Globalization.CultureInfo("zh-cn"));
            this.TM.Text += " ";
        }

        /// <summary>
        /// 本地配置文件读取
        /// </summary>
        private void LoadJsonData()
        {
            try
            {
                using (var sr = File.OpenText("C:\\config\\config.json"))
                {
                    string JsonStr = sr.ReadToEnd();
                    config = JsonConvert.DeserializeObject<ConfigData>(JsonStr);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        /// <summary>
        /// 初始化步骤
        /// </summary>
        /// <param name="count">总步骤数</param>
        /// <param name="list">详情</param>
        private void SetStepData(int count, List<StationData> list)
        {
            #region who care
            switch (count)
            {
                case 1:
                    Step1.Text = list[0].Name;
                    StepImage1.Source = IFalse;
                    break;
                case 2:
                    Step1.Text = list[0].Name;
                    Step2.Text = list[1].Name;
                    StepImage1.Source = IFalse;
                    StepImage2.Source = IFalse;
                    break;
                case 3:
                    Step1.Text = list[0].Name;
                    Step2.Text = list[1].Name;
                    Step3.Text = list[2].Name;
                    StepImage1.Source = IFalse;
                    StepImage2.Source = IFalse;
                    StepImage3.Source = IFalse;
                    break;
                case 4:
                    Step1.Text = list[0].Name;
                    Step2.Text = list[1].Name;
                    Step3.Text = list[2].Name;
                    Step4.Text = list[3].Name;
                    StepImage1.Source = IFalse;
                    StepImage2.Source = IFalse;
                    StepImage3.Source = IFalse;
                    StepImage4.Source = IFalse;
                    break;
                case 5:
                    Step1.Text = list[0].Name;
                    Step2.Text = list[1].Name;
                    Step3.Text = list[2].Name;
                    Step4.Text = list[3].Name;
                    Step5.Text = list[4].Name;
                    StepImage1.Source = IFalse;
                    StepImage2.Source = IFalse;
                    StepImage3.Source = IFalse;
                    StepImage4.Source = IFalse;
                    StepImage5.Source = IFalse;
                    break;
                case 6:
                    Step1.Text = list[0].Name;
                    Step2.Text = list[1].Name;
                    Step3.Text = list[2].Name;
                    Step4.Text = list[3].Name;
                    Step5.Text = list[4].Name;
                    Step6.Text = list[5].Name;
                    StepImage1.Source = IFalse;
                    StepImage2.Source = IFalse;
                    StepImage3.Source = IFalse;
                    StepImage4.Source = IFalse;
                    StepImage5.Source = IFalse;
                    StepImage6.Source = IFalse;
                    break;
                case 7:
                    Step1.Text = list[0].Name;
                    Step2.Text = list[1].Name;
                    Step3.Text = list[2].Name;
                    Step4.Text = list[3].Name;
                    Step5.Text = list[4].Name;
                    Step6.Text = list[5].Name;
                    Step7.Text = list[6].Name;
                    StepImage1.Source = IFalse;
                    StepImage2.Source = IFalse;
                    StepImage3.Source = IFalse;
                    StepImage4.Source = IFalse;
                    StepImage5.Source = IFalse;
                    StepImage6.Source = IFalse;
                    StepImage7.Source = IFalse;
                    break;
                case 8:
                    Step1.Text = list[0].Name;
                    Step2.Text = list[1].Name;
                    Step3.Text = list[2].Name;
                    Step4.Text = list[3].Name;
                    Step5.Text = list[4].Name;
                    Step6.Text = list[5].Name;
                    Step7.Text = list[6].Name;
                    Step8.Text = list[7].Name;
                    StepImage1.Source = IFalse;
                    StepImage2.Source = IFalse;
                    StepImage3.Source = IFalse;
                    StepImage4.Source = IFalse;
                    StepImage5.Source = IFalse;
                    StepImage6.Source = IFalse;
                    StepImage7.Source = IFalse;
                    StepImage8.Source = IFalse;
                    break;
                case 9:
                    Step1.Text = list[0].Name;
                    Step2.Text = list[1].Name;
                    Step3.Text = list[2].Name;
                    Step4.Text = list[3].Name;
                    Step5.Text = list[4].Name;
                    Step6.Text = list[5].Name;
                    Step7.Text = list[6].Name;
                    Step8.Text = list[7].Name;
                    Step9.Text = list[8].Name;
                    StepImage1.Source = IFalse;
                    StepImage2.Source = IFalse;
                    StepImage3.Source = IFalse;
                    StepImage4.Source = IFalse;
                    StepImage5.Source = IFalse;
                    StepImage6.Source = IFalse;
                    StepImage7.Source = IFalse;
                    StepImage8.Source = IFalse;
                    StepImage9.Source = IFalse;
                    break;
                case 10:
                    Step1.Text = list[0].Name;
                    Step2.Text = list[1].Name;
                    Step3.Text = list[2].Name;
                    Step4.Text = list[3].Name;
                    Step5.Text = list[4].Name;
                    Step6.Text = list[5].Name;
                    Step7.Text = list[6].Name;
                    Step8.Text = list[7].Name;
                    Step9.Text = list[8].Name;
                    Step10.Text = list[9].Name;
                    StepImage1.Source = IFalse;
                    StepImage2.Source = IFalse;
                    StepImage3.Source = IFalse;
                    StepImage4.Source = IFalse;
                    StepImage5.Source = IFalse;
                    StepImage6.Source = IFalse;
                    StepImage7.Source = IFalse;
                    StepImage8.Source = IFalse;
                    StepImage9.Source = IFalse;
                    StepImage10.Source = IFalse;
                    break;
                case 11:
                    Step1.Text = list[0].Name;
                    Step2.Text = list[1].Name;
                    Step3.Text = list[2].Name;
                    Step4.Text = list[3].Name;
                    Step5.Text = list[4].Name;
                    Step6.Text = list[5].Name;
                    Step7.Text = list[6].Name;
                    Step8.Text = list[7].Name;
                    Step9.Text = list[8].Name;
                    Step10.Text = list[9].Name;
                    Step11.Text = list[10].Name;
                    StepImage1.Source = IFalse;
                    StepImage2.Source = IFalse;
                    StepImage3.Source = IFalse;
                    StepImage4.Source = IFalse;
                    StepImage5.Source = IFalse;
                    StepImage6.Source = IFalse;
                    StepImage7.Source = IFalse;
                    StepImage8.Source = IFalse;
                    StepImage9.Source = IFalse;
                    StepImage10.Source = IFalse;
                    StepImage11.Source = IFalse;
                    break;
                case 12:
                    Step1.Text = list[0].Name;
                    Step2.Text = list[1].Name;
                    Step3.Text = list[2].Name;
                    Step4.Text = list[3].Name;
                    Step5.Text = list[4].Name;
                    Step6.Text = list[5].Name;
                    Step7.Text = list[6].Name;
                    Step8.Text = list[7].Name;
                    Step9.Text = list[8].Name;
                    Step10.Text = list[9].Name;
                    Step11.Text = list[10].Name;
                    Step12.Text = list[11].Name;
                    StepImage1.Source = IFalse;
                    StepImage2.Source = IFalse;
                    StepImage3.Source = IFalse;
                    StepImage4.Source = IFalse;
                    StepImage5.Source = IFalse;
                    StepImage6.Source = IFalse;
                    StepImage7.Source = IFalse;
                    StepImage8.Source = IFalse;
                    StepImage9.Source = IFalse;
                    StepImage10.Source = IFalse;
                    StepImage11.Source = IFalse;
                    StepImage12.Source = IFalse;
                    break;
            }
            #endregion
        }

        /// <summary>
        /// 更改步骤状态
        /// </summary>
        /// <param name="type">步骤状态</param>
        /// <param name="GWNo">工位号</param>
        private void ModifyStep(int type, int GWNo)
        {
            switch (GWNo)
            {
                case 20: //20工位
                    switch (type)
                    {
                        case 10:
                            StepImage1.Source = ITrue;
                            StepImage2.Source = ITrue;
                            StepImage3.Source = IFalse;
                            StepImage4.Source = IFalse;
                            StepImage5.Source = IFalse;
                            break;
                        case 20:
                            StepImage3.Source = ITrue;
                            break;
                        case 100:
                            StepImage3.Source = ITrue;
                            StepImage4.Source = ITrue;
                            break;
                        case 110:
                            StepImage5.Source = ITrue;
                            break;
                    }
                    break;
                case 40: //40工位
                    switch (type)
                    {
                        case 10:
                            StepImage1.Source = ITrue;
                            StepImage2.Source = IFalse;
                            StepImage3.Source = IFalse;
                            StepImage4.Source = IFalse;
                            StepImage5.Source = IFalse;
                            break;
                        case 20:
                            StepImage2.Source = ITrue;
                            break;
                        case 30:
                            StepImage3.Source = ITrue;
                            break;
                        case 35:
                            StepImage4.Source = ITrue;
                            break;
                        case 40:
                            StepImage5.Source = ITrue;
                            break;
                    }
                    break;
                case 04052:
                    switch (type)
                    {
                        case 100:
                            StepImage1.Source = ITrue;
                            StepImage2.Source = IFalse;
                            StepImage3.Source = IFalse;
                            StepImage4.Source = IFalse;
                            StepImage5.Source = IFalse;
                            StepImage6.Source = IFalse;
                            StepImage7.Source = IFalse;
                            StepImage8.Source = IFalse;
                            StepImage9.Source = IFalse;
                            StepImage10.Source = IFalse;
                            StepImage11.Source = IFalse;
                            break;
                        case 110:
                            StepImage2.Source = ITrue;
                            break;
                        case 150:
                            StepImage3.Source = ITrue;
                            break;
                        case 200:
                            StepImage4.Source = ITrue;
                            break;
                        case 400:
                            StepImage5.Source = ITrue;
                            break;
                        case 700:
                            StepImage6.Source = ITrue;
                            break;
                        case 900:
                            StepImage7.Source = ITrue;
                            break;
                        case 1100:
                            StepImage8.Source = ITrue;
                            break;
                        case 1300:
                            StepImage9.Source = ITrue;
                            break;
                        case 1400:
                            StepImage10.Source = ITrue;
                            break;
                        case 1500:
                            StepImage11.Source = ITrue;
                            break;
                    }
                    break;
                case 04053:
                    switch (type)
                    {
                        case 100:
                            StepImage1.Source = ITrue;
                            StepImage2.Source = IFalse;
                            StepImage3.Source = IFalse;
                            StepImage4.Source = IFalse;
                            StepImage5.Source = IFalse;
                            StepImage6.Source = IFalse;
                            StepImage7.Source = IFalse;
                            StepImage8.Source = IFalse;
                            StepImage9.Source = IFalse;
                            StepImage10.Source = IFalse;
                            StepImage11.Source = IFalse;
                            StepImage12.Source = IFalse;
                            break;
                        case 110:
                            StepImage2.Source = ITrue;
                            break;
                        case 150:
                            StepImage3.Source = ITrue;
                            break;
                        case 200:
                            StepImage4.Source = ITrue;
                            break;
                        case 300:
                            StepImage5.Source = ITrue;
                            break;
                        case 400:
                            StepImage6.Source = ITrue;
                            break;
                        case 500:
                            StepImage7.Source = ITrue;
                            break;
                        case 600:
                            StepImage8.Source = ITrue;
                            break;
                        case 700:
                            StepImage9.Source = ITrue;
                            break;
                        case 800:
                            StepImage10.Source = ITrue;
                            break;
                        case 900:
                            StepImage11.Source = ITrue;
                            break;
                        case 1000:
                            StepImage12.Source = ITrue;
                            break;
                    }
                    break;
                case 04061:
                    switch (type)
                    {
                        case 100:
                            StepImage1.Source = ITrue;
                            StepImage2.Source = IFalse;
                            StepImage3.Source = IFalse;
                            StepImage4.Source = IFalse;
                            StepImage5.Source = IFalse;
                            StepImage6.Source = IFalse;
                            StepImage7.Source = IFalse;
                            StepImage8.Source = IFalse;
                            StepImage9.Source = IFalse;
                            StepImage10.Source = IFalse;
                            StepImage11.Source = IFalse;
                            StepImage12.Source = IFalse;
                            break;
                        case 110:
                            StepImage2.Source = ITrue;
                            break;
                        case 200:
                            StepImage3.Source = ITrue;
                            break;
                        case 300:
                            StepImage4.Source = ITrue;
                            break;
                        case 600:
                            StepImage5.Source = ITrue;
                            break;
                        case 800:
                            StepImage6.Source = ITrue;
                            break;
                        case 900:
                            StepImage7.Source = ITrue;
                            break;
                        case 1400:
                            StepImage8.Source = ITrue;
                            break;
                        case 1600:
                            StepImage9.Source = ITrue;
                            break;
                        case 1800:
                            StepImage10.Source = ITrue;
                            break;
                        case 1900:
                            StepImage11.Source = ITrue;
                            break;
                        case 2000:
                            StepImage12.Source = ITrue;
                            break;
                    }
                    break;
                case 04063:
                    switch (type)
                    {
                        case 106:
                            StepImage1.Source = ITrue;
                            StepImage2.Source = IFalse;
                            StepImage3.Source = IFalse;
                            StepImage4.Source = IFalse;
                            StepImage5.Source = IFalse;
                            StepImage6.Source = IFalse;
                            StepImage7.Source = IFalse;
                            StepImage8.Source = IFalse;
                            StepImage9.Source = IFalse;
                            StepImage10.Source = IFalse;
                            StepImage11.Source = IFalse;
                            StepImage12.Source = IFalse;
                            break;
                        case 110:
                            StepImage2.Source = ITrue;
                            break;
                        case 150:
                            StepImage3.Source = ITrue;
                            break;
                        case 500:
                            StepImage4.Source = ITrue;
                            break;
                        case 700:
                            StepImage5.Source = ITrue;
                            break;
                        case 900:
                            StepImage6.Source = ITrue;
                            break;
                        case 1000:
                            StepImage7.Source = ITrue;
                            break;
                        case 1300:
                            StepImage8.Source = ITrue;
                            break;
                        case 1400:
                            StepImage9.Source = ITrue;
                            break;
                        case 1500:
                            StepImage10.Source = ITrue;
                            break;
                        case 1600:
                            StepImage11.Source = ITrue;
                            break;
                        case 2100:
                            StepImage12.Source = ITrue;
                            break;
                    }
                    break;
            }

        }

        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (MessageBoxX.Show("是否要关闭？", "确认", Application.Current.MainWindow, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //{
            //    e.Cancel = false;
            //    //if (plc.IsConnected)
            //    //{
            //    //    plc.Close();
            //    //    log.Info("PLC Disconnected!");
            //    //}
            //    if (connect.IsSuccess)
            //    {
            //        splc.ConnectClose();
            //    }
            //}
            //else
            //{
            //    e.Cancel = true;
            //}
            if (connect.IsSuccess)
            {
                splc.ConnectClose();
            }
            log.Info("PLC Disconnected!");
        }

        public void ThreadCheck(object sender, EventArgs e)
        {
            var check = splc.ReadUInt16("DB2000.0");
            if (check.IsSuccess)
            {
                PLCImage.Source = ITrue;
                //log.Info("PLC Connected!");

                if (!remark)
                {
                    DataReload();
                }
            }
            else
            {
                PLCImage.Source = IFalse;
                //log.Info("PLC Not Connected!");
            }
        }
    }
}
