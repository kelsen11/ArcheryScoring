using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace C_Sharp_Demo
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //DLL插件添加到程序运行目录
            if (!(File.Exists(Application.StartupPath + "\\LedDynamicArea.dll")))
            {
                //文件不存在，从资源文件写入；
                File.WriteAllBytes(Application.StartupPath + "\\LedDynamicArea.dll", Resource1.LedDynamicArea);
            }

            if (!(File.Exists(Application.StartupPath + "\\borlndmm.dll")))
            {
                //文件不存在，从资源文件写入；
                File.WriteAllBytes(Application.StartupPath + "\\borlndmm.dll", Resource1.borlndmm);
            }

            if (!(File.Exists(Application.StartupPath + "\\TransNet.dll")))
            {
                //文件不存在，从资源文件写入；
                File.WriteAllBytes(Application.StartupPath + "\\TransNet.dll", Resource1.TransNet);
            }

            //界面初始化
            System.Drawing.Text.PrivateFontCollection privateFonts = new System.Drawing.Text.PrivateFontCollection();
            
            if (!(File.Exists(Application.StartupPath + "\\msyh.ttc")))
            {
                //文件不存在，从资源文件写入；
                File.WriteAllBytes(Application.StartupPath + "\\msyh.ttc", Resource1.msyh);
            }
            privateFonts.AddFontFile(Application.StartupPath + "\\msyh.ttc");
            System.Drawing.Font font = new Font(privateFonts.Families[0], 8);
            richBoxPlayerA.Font = font;
            richBoxPlayerB.Font = font;

            //屏幕初始化
            Screen.Init();

            //配置屏幕颜色
            if(Screen.LeftScore.GetScreenColor() == ScoringScreen.ColorStyle.Yellow)
            {
                radioBtnColorSty1.Checked = true;
            }
            else if (Screen.LeftScore.GetScreenColor() == ScoringScreen.ColorStyle.Green)
            {
                radioBtnColorSty2.Checked = true;
            }
            else
            {
                radioBtnColorSty3.Checked = true;
            }

            //开启串口搜寻
            SearchSerialPortWorker.RunWorkerAsync();

            //页面信息初始化
            if (Debugger.IsAttached)
            {
                Text = "射箭计分显示系统 V3 - version" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " 内部版本，仅供测试";
            }

            comboBoxAuxAddScore.SelectedIndex = 0;
            comboBoxAuxChangeScore.SelectedIndex = 0;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //关闭各种后台进程
            windSensorWorker.CancelAsync();
            SearchSerialPortWorker.CancelAsync();
            ScreenSendWorker.CancelAsync();

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void btnLeftScrOpen_Click(object sender, EventArgs e)
        {
            if (Screen.LeftScore.GetOpenState() == true)
            {
                Screen.LeftScore.SetClose();
                btnLeftScrOpen.Text = "打开";
            }
            else
            {
                Screen.LeftScore.SetOpen();
                btnLeftScrOpen.Text = "关闭";

                //自动发送一次
                buttonScrLeftSend_Click(null, null);
            }
        }

        private void btnRightScrOpen_Click(object sender, EventArgs e)
        {
            if (Screen.RightScore.GetOpenState() == true)
            {
                Screen.RightScore.SetClose();
                btnRightScrOpen.Text = "打开";
            }
            else
            {
                Screen.RightScore.SetOpen();
                btnRightScrOpen.Text = "关闭";

                //自动发送一次
                buttonScrRightSend_Click(null, null);
            }
        }

        private void btnWindScrOpen_Click(object sender, EventArgs e)
        {
            if (Screen.Wind.GetOpenState() == true)
            {
                Screen.Wind.SetClose();
                btnWindScrOpen.Text = "打开";
            }
            else
            {
                Screen.Wind.SetOpen();
                btnWindScrOpen.Text = "关闭";
            }
        }

        private void pictureBoxColorSty1_Click(object sender, EventArgs e)
        {
            //修改radio button即可
            radioBtnColorSty1.Checked = true;
        }

        private void pictureBoxColorSty2_Click(object sender, EventArgs e)
        {
            radioBtnColorSty2.Checked = true;
        }

        private void pictureColorSty3_Click(object sender, EventArgs e)
        {
            radioBtnColorSty3.Checked = true;
        }

        private void radioBtnColorSty1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnColorSty1.Checked == true)
            {
                Screen.LeftScore.ChangeColor(ScoringScreen.ColorStyle.Yellow);
                Screen.RightScore.ChangeColor(ScoringScreen.ColorStyle.Yellow);
            }
        }

        private void radioBtnColorSty2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnColorSty2.Checked == true)
            {
                Screen.LeftScore.ChangeColor(ScoringScreen.ColorStyle.Green);
                Screen.RightScore.ChangeColor(ScoringScreen.ColorStyle.Green);
            }
        }

        private void radioBtnColorSty3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnColorSty3.Checked == true)
            {
                Screen.LeftScore.ChangeColor(ScoringScreen.ColorStyle.Mixture);
                Screen.RightScore.ChangeColor(ScoringScreen.ColorStyle.Mixture);
            }
        }

        private void radioBtnLanChinese_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnLanChinese.Checked == true)
            {
                //修改屏幕语言
                Screen.LeftScore.ChangeLanguage(ScoringScreen.Language.Chinese);
                Screen.RightScore.ChangeLanguage(ScoringScreen.Language.Chinese);
            }
        }

        private void radioBtnLanEnglish_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnLanEnglish.Checked == true)
            {
                Screen.LeftScore.ChangeLanguage(ScoringScreen.Language.English);
                Screen.RightScore.ChangeLanguage(ScoringScreen.Language.English);
            }
        }

        private void checkBoxCustomScrTxt_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCustomScrTxt.Checked == true)
            {
                textBoxCustomTxtLine2Area1.Enabled = true;
                textBoxCustomTxtLine3Area1.Enabled = true;
                textBoxCustomTxtLine3Area2.Enabled = true;
                btnCustomTxtOK.Enabled = true;
            }
            else
            {
                textBoxCustomTxtLine2Area1.Enabled = false;
                textBoxCustomTxtLine3Area1.Enabled = false;
                textBoxCustomTxtLine3Area2.Enabled = false;
                btnCustomTxtOK.Enabled = false;

                //清空文字还原配置
                textBoxCustomTxtLine2Area1.Text = "";
                textBoxCustomTxtLine3Area1.Text = "";
                textBoxCustomTxtLine3Area2.Text = "";
                labelRightScore.Text = labelLeftScore.Text = "环数";
                labelRightTotal1.Text = labelLeftTotal1.Text = "积分/总环";
                labelRightTotal2.Text = labelLeftTotal2.Text = "总环";
                btnCustomTxtOK.Text = "确定";

                Screen.LeftScore.RestoreTxt();
                Screen.RightScore.RestoreTxt();
            }
        }

        private void btnCustomTxtOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxCustomTxtLine2Area1.Text.Trim())
                || string.IsNullOrEmpty(textBoxCustomTxtLine3Area1.Text.Trim()))
            {
                MessageBox.Show("第2行区域1、第2行区域1文本框不能为空！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (textBoxCustomTxtLine2Area1.Text.Trim().Length > 10
                || textBoxCustomTxtLine3Area1.Text.Trim().Length > 10
                || textBoxCustomTxtLine3Area2.Text.Trim().Length > 10
                )
            {
                MessageBox.Show("字符长度超长！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (btnCustomTxtOK.Text == "修改")
            {
                btnCustomTxtOK.Text = "确定";
                textBoxCustomTxtLine2Area1.Enabled = true;
                textBoxCustomTxtLine3Area1.Enabled = true;
                textBoxCustomTxtLine3Area2.Enabled = true;
                return;
            }
            else
            {
                textBoxCustomTxtLine2Area1.Enabled = false;
                textBoxCustomTxtLine3Area1.Enabled = false;
                textBoxCustomTxtLine3Area2.Enabled = false;
                btnCustomTxtOK.Text = "修改";
            }

            /*两个屏幕都需要修改*/
            int err = Screen.LeftScore.SetCustomTxt(textBoxCustomTxtLine2Area1.Text.Trim(),
                textBoxCustomTxtLine3Area1.Text.Trim(),
                textBoxCustomTxtLine3Area2.Text.Trim());
            err += Screen.RightScore.SetCustomTxt(textBoxCustomTxtLine2Area1.Text.Trim(),
                textBoxCustomTxtLine3Area1.Text.Trim(),
                textBoxCustomTxtLine3Area2.Text.Trim());
            if (err != 0)
            {
                MessageBox.Show("自定义屏幕文字出错！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("自定义屏幕文字成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            labelRightScore.Text = labelLeftScore.Text = textBoxCustomTxtLine2Area1.Text.Trim();
            labelRightTotal1.Text = labelLeftTotal1.Text = textBoxCustomTxtLine3Area1.Text.Trim();
            if (textBoxCustomTxtLine3Area2.Text.Trim() == "")
            {
                labelRightTotal2.Text = labelLeftTotal2.Text = "[无]";
            }
            else
            {
                labelRightTotal2.Text = labelLeftTotal2.Text = textBoxCustomTxtLine3Area2.Text.Trim();
            }

        }

        private void SearchSerialPortWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            /*新建串口*/
            SerialPort windSerialPort = new SerialPort();
            windSerialPort.BaudRate = 115200;
            windSerialPort.DataBits = 8;
            windSerialPort.Parity = Parity.None;
            windSerialPort.StopBits = StopBits.One;

            /*软件启动后开始搜索串口*/
            while (WindSensor.Connect == false)
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    Thread.Sleep(100);  //进程调度

                    if (windSerialPort.IsOpen)
                    {
                        windSerialPort.Close();
                    }
                    windSerialPort.PortName = port;

                    try
                    {
                        windSerialPort.Open();
                    }
                    catch
                    {
                        //串口被占用
                        WindSensor.SetStatMessage("串口占用");
                        SearchSerialPortWorker.ReportProgress(0);
                        break;
                    }

                    try
                    {
                        byte[] buf = { 0xaa, 0x55 };
                        windSerialPort.Write(buf, 0, 2);

                        Thread.Sleep(520);  //重要延时！

                        int byteNum = windSerialPort.BytesToRead;
                        byte[] buffer = new byte[byteNum];
                        windSerialPort.Read(buffer, 0, byteNum);

                        //检测数据是否正确
                        if (Debugger.IsAttached)
                        {
                            byteNum = 1;
                            buffer = new byte[] { 0xaa, 0x55 };
                            Thread.Sleep(3000);
                        }
                        if (byteNum != 0 && buffer[0] == 0xaa && buffer[1] == 0x55)
                        {
                            /* 查找成功，保存串口并退出循环 */
                            WindSensor.Connect = true;
                            WindSensor.SetSerialPort(port);     /*保存串口*/
                            break;
                        }
                        else
                        {
                            WindSensor.SetStatMessage("数据错误");
                            SearchSerialPortWorker.ReportProgress(0);
                            continue;
                        }
                    }
                    catch
                    {
                        WindSensor.SetStatMessage("未连接");
                        SearchSerialPortWorker.ReportProgress(0);
                        continue;
                    }


                }
                //关闭com口
                if (windSerialPort.IsOpen)
                {
                    windSerialPort.Close();
                }
                Thread.Sleep(10);  //进程调度
            }

        }

        private void SearchSerialPortWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            /*更新页面信息*/
            labelWindSensorStat.Text = WindSensor.GetErrMsg();
        }

        private void SearchSerialPortWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            /*更新页面信息*/
            labelWindSensorStat.Text = "已连接";
            labelWindSensorStat.ForeColor = Color.Green;
            /*开启风速风向检测Worker*/
            windSensorWorker.RunWorkerAsync();
        }

        private void windSensorWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            /*风速风向测量后台进程，间隔一定时间更新实时数据*/
            SerialPort windSerialPort = new SerialPort();//新建串口
            windSerialPort.BaudRate = 115200;
            windSerialPort.DataBits = 8;
            windSerialPort.Parity = Parity.None;
            windSerialPort.StopBits = StopBits.One;

            windSerialPort.PortName = WindSensor.GetSerialPort();

            //如果打开，需要关闭com口，并重新开启
            if (windSerialPort.IsOpen)
            {
                windSerialPort.Close();
            }

            //尝试打开三次
            try
            {
                windSerialPort.Open();
            }
            catch
            {
                Thread.Sleep(500);
                try
                {
                    windSerialPort.Open();
                }
                catch
                {
                    Thread.Sleep(500);
                    try
                    {
                        windSerialPort.Open();
                    }
                    catch
                    {
                        /* 串口一直被占用 */
                        WindSensor.Connect = false;
                        WindSensor.SetStatMessage("串口占用");
                        //windSensorWorker.ReportProgress(0);
                        //windSensorWorker.CancelAsync();
                    }
                }

            }


            byte debugDirect = 0x48;
            byte debugSpeed = 0x38;
            int errTimes = 0;
            while (WindSensor.Connect == true)
            {
                try
                {
                    byte[] buf = { 0xaa, 0x01 };
                    windSerialPort.Write(buf, 0, 2);        //发送读信号

                    Thread.Sleep(800);

                    int byteNum = windSerialPort.BytesToRead;
                    byte[] comBuffer = new byte[byteNum];
                    windSerialPort.Read(comBuffer, 0, byteNum);        //将数据读入缓存

                    if (Debugger.IsAttached)
                    {
                        comBuffer = new byte[10] { 0x00, 0x00, 0x00, 0x45, 0x32, 0x35, 0x2e, 0x30, 0x31, 0x30 };
                        comBuffer[3] = debugDirect--;
                        comBuffer[5] = debugSpeed--;
                        if ((comBuffer[5] & 0x0c) == 0x0)
                        {
                            comBuffer[4] = 0x31;
                        }
                        else
                        {
                            comBuffer[4] = 0x30;
                        }
                        if (debugDirect == 0x40)
                        {
                            debugDirect = 0x48;
                            debugSpeed = 0x38;
                        }
                        byteNum = 1;
                    }

                    if (byteNum != 0)
                    {
                        WindSensor.Direction = Convert.ToChar(comBuffer[3]);      //将方向信息转换为可识别的字符形式

                        //以下是风速数据转换
                        byte[] spTemp = new byte[6];
                        spTemp[0] = comBuffer[4];
                        spTemp[1] = comBuffer[5];
                        spTemp[2] = comBuffer[6];
                        spTemp[3] = comBuffer[7];
                        spTemp[4] = comBuffer[8];
                        spTemp[5] = comBuffer[9];

                        WindSensor.SpeedDouble = Convert.ToDouble(System.Text.Encoding.Default.GetString(spTemp));        //将其转换为double数据，进行运算
                        WindSensor.Speed = WindSensor.SpeedDouble.ToString("0.0") + "m/s";  //保留一位小数

                        windSensorWorker.ReportProgress(0);     //调用更新来更新图片和文字的显示
                        Thread.Sleep(800);      //延时
                    }

                }
                catch
                {
                    errTimes++;
                    WindSensor.SetStatMessage("数据错误");
                    //windSensorWorker.ReportProgress(0);
                    //windSensorWorker.CancelAsync();
                    if (errTimes == 3)
                    {
                        WindSensor.Connect = false;
                    }
                }
                Thread.Sleep(2000);     //2018年3月27日 由于板卡传送速率慢，故增加至3s多
            }

            if (windSerialPort.IsOpen)      //退出时记得关闭串口
                windSerialPort.Close();

        }

        private void windSensorWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            /* 发送最新数据到屏幕 */
            labelWindSpeedTxt.Text = WindSensor.Speed;       //更新界面显示
            switch (WindSensor.Direction)
            {

                case 'A':
                    pictureBoxWindDirection.Image = Resource1.north;
                    break;
                case 'B':
                    pictureBoxWindDirection.Image = Resource1.northeast;
                    break;
                case 'C':
                    pictureBoxWindDirection.Image = Resource1.east;
                    break;
                case 'D':
                    pictureBoxWindDirection.Image = Resource1.southeast;
                    break;
                case 'E':
                    pictureBoxWindDirection.Image = Resource1.south;
                    break;
                case 'F':
                    pictureBoxWindDirection.Image = Resource1.southwest;
                    break;
                case 'G':
                    pictureBoxWindDirection.Image = Resource1.west;
                    break;
                case 'H':
                    pictureBoxWindDirection.Image = Resource1.northwest;
                    break;
                default:    //默认为北方
                    pictureBoxWindDirection.Image = Resource1.north;
                    break;
            }

            /*更新缓存 -> 添加风速风向屏幕的发送队列 -> 调用后台进程*/
            Screen.Wind.RefreshWind(WindSensor.Speed, WindSensor.Direction);

            if(!Debugger.IsAttached)
            {
                //不开debug时候传送数据
                Screen.AddSendQue(Screen.Select.Wind);
            }
            if (ScreenSendWorker.IsBusy == false)
            {
                ScreenSendWorker.RunWorkerAsync();
            }
        }

        private void windSensorWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            /*发生错误重新搜索串口*/
            SearchSerialPortWorker.RunWorkerAsync();

            labelWindSensorStat.Text = "未连接";
            labelWindSensorStat.ForeColor = Color.Yellow;
        }

        private void ScreenSendWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProcResult.ErrType err = ProcResult.ErrType.ErrFailed;
            //循环处理发送队列
            while (Screen.GetQueueCount() > 0)
            {
                Screen.Select scr = Screen.DeSendQue();
                switch (scr)
                {
                    case Screen.Select.LeftScore:
                        err = Screen.LeftScore.SendInfo();
                        break;
                    case Screen.Select.RightScore:
                        err = Screen.RightScore.SendInfo();
                        break;
                    case Screen.Select.Wind:
                        err = Screen.Wind.SendInfo();
                        break;
                }

                //更新软件界面
                object obj = new ProcResult(scr, err);
                ScreenSendWorker.ReportProgress(0, obj);
            }
        }


        private object picLeftScoLock = new object();
        private object picRightScoLock = new object();
        private object picWindLock = new object();

        private void ScreenSendWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcResult result = (ProcResult)e.UserState;

            //获取屏幕内容，更新到界面上
            switch (result.ScreenSelect)
            {
                case Screen.Select.LeftScore:
                    labelLeftScrStat.Text = result.StatText;
                    labelLeftScrStat.ForeColor = result.TextColor;
                    if (result.ErrorNum == ProcResult.ErrType.ErrNotConnect)
                    {
                        //关闭屏幕
                        btnLeftScrOpen.Text = "打开";
                        Screen.LeftScore.SetClose();
                    }
                    lock (picLeftScoLock)
                    {
                        picLeftScoreScr.Image = Screen.LeftScore.GetBmpBufWithLock();
                    }
                    break;
                case Screen.Select.RightScore:
                    labelRightScrStat.Text = result.StatText;
                    labelRightScrStat.ForeColor = result.TextColor;
                    if (result.ErrorNum == ProcResult.ErrType.ErrNotConnect)
                    {
                        //关闭屏幕
                        btnRightScrOpen.Text = "打开";
                        Screen.RightScore.SetClose();
                    }
                    lock (picRightScoLock)
                    {
                        picRightScoreScr.Image = Screen.RightScore.GetBmpBufWithLock();
                    }
                    break;
                case Screen.Select.Wind:
                    labelWindScrStat.Text = result.StatText;
                    labelWindScrStat.ForeColor = result.TextColor;
                    if (result.ErrorNum == ProcResult.ErrType.ErrNotConnect)
                    {
                        btnWindScrOpen.Text = "打开";
                        Screen.Wind.SetClose();
                    }
                    lock (picWindLock)
                    {
                        picWindScr.Image = Screen.Wind.GetBmpBufWithLock();
                    }
                    break;
                case Screen.Select.Err:
                    //成粗调用出错，请打印错误信息
                    break;
            }
        }

        private void ScreenSendWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //不处理
        }

        private void buttonScrLeftSend_Click(object sender, EventArgs e)
        {
            //判断文字是否符合要求
            string title = txtLeftTitle.Text;
            if (Screen.LeftScore.MeasureTitleLenIsOk(title) != Error.ErrSuccess)
            {
                MessageBox.Show("输入文字超过屏幕显示范围，请删除部分字符重试！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int[] score = new int[6] { -1, -1, -1, -1, -1, -1 };
            ScoringScreen.ScoreMark[] scoreMark = new ScoringScreen.ScoreMark[6] { ScoringScreen.ScoreMark.NoMark,
                ScoringScreen.ScoreMark.NoMark, ScoringScreen.ScoreMark.NoMark, ScoringScreen.ScoreMark.NoMark,
                ScoringScreen.ScoreMark.NoMark, ScoringScreen.ScoreMark.NoMark };
            int totalScore1 = -1;
            int totalScore2 = -1;

            bool err;

            if (!string.IsNullOrEmpty(txtLeftScore1.Text.Trim()))
                err = int.TryParse(txtLeftScore1.Text, out score[0]);
            if (!string.IsNullOrEmpty(txtLeftScore2.Text.Trim()))
                err = int.TryParse(txtLeftScore2.Text, out score[1]);
            if (!string.IsNullOrEmpty(txtLeftScore3.Text.Trim()))
                err = int.TryParse(txtLeftScore3.Text, out score[2]);
            if (!string.IsNullOrEmpty(txtLeftScore4.Text.Trim()))
                err = int.TryParse(txtLeftScore4.Text, out score[3]);
            if (!string.IsNullOrEmpty(txtLeftScore5.Text.Trim()))
                err = int.TryParse(txtLeftScore5.Text, out score[4]);
            if (!string.IsNullOrEmpty(txtLeftScore6.Text.Trim()))
                err = int.TryParse(txtLeftScore6.Text, out score[5]);

            if (!string.IsNullOrEmpty(txtLeftTotal1.Text.Trim()))
                err = int.TryParse(txtLeftTotal1.Text, out totalScore1);
            if (!string.IsNullOrEmpty(txtLeftTotal2.Text.Trim()))
                err = int.TryParse(txtLeftTotal2.Text, out totalScore2);

            if (checkBoxLeft1.Checked == true)
                scoreMark[0] = ScoringScreen.ScoreMark.UnderlineMark;
            if (checkBoxLeft2.Checked == true)
                scoreMark[1] = ScoringScreen.ScoreMark.UnderlineMark;
            if (checkBoxLeft3.Checked == true)
                scoreMark[2] = ScoringScreen.ScoreMark.UnderlineMark;
            if (checkBoxLeft4.Checked == true)
                scoreMark[3] = ScoringScreen.ScoreMark.UnderlineMark;
            if (checkBoxLeft5.Checked == true)
                scoreMark[4] = ScoringScreen.ScoreMark.UnderlineMark;
            if (checkBoxLeft6.Checked == true)
                scoreMark[5] = ScoringScreen.ScoreMark.UnderlineMark;



            //2017年4月7日 注：后面建议修改“更新缓存”到队列中，避免同一资源被同时调用
            /*更新缓存 -> 添加发送队列 -> 调用后台进程*/
            Screen.LeftScore.RefreshScore(title, score, scoreMark, totalScore1, totalScore2);

            //调试代码
            /*
            Screen.LeftScore.RefreshScore("山东队", new int[] { 10, 10, 10, 10, 10, 10 },
                new ScoringScreen.ScoreMark[] { ScoringScreen.ScoreMark.NoMark, ScoringScreen.ScoreMark.NoMark,
                    ScoringScreen.ScoreMark.NoMark, ScoringScreen.ScoreMark.UnderlineMark,
                    ScoringScreen.ScoreMark.NoMark, ScoringScreen.ScoreMark.NoMark },
                20, 100);
            */

            Screen.AddSendQue(Screen.Select.LeftScore);
            if (ScreenSendWorker.IsBusy == false)
            {
                ScreenSendWorker.RunWorkerAsync();
            }
        }

        private void buttonScrRightSend_Click(object sender, EventArgs e)
        {
            string title = txtRightTitle.Text;
            if (Screen.RightScore.MeasureTitleLenIsOk(title) != Error.ErrSuccess)
            {
                MessageBox.Show("输入文字超过屏幕显示范围，请删除部分字符重试！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int[] score = new int[6] { -1, -1, -1, -1, -1, -1 };
            ScoringScreen.ScoreMark[] scoreMark = new ScoringScreen.ScoreMark[6] { ScoringScreen.ScoreMark.NoMark,
                ScoringScreen.ScoreMark.NoMark, ScoringScreen.ScoreMark.NoMark, ScoringScreen.ScoreMark.NoMark,
                ScoringScreen.ScoreMark.NoMark, ScoringScreen.ScoreMark.NoMark };
            int totalScore1 = -1;
            int totalScore2 = -1;

            bool err;

            if (!string.IsNullOrEmpty(txtRightScore1.Text.Trim()))
                err = int.TryParse(txtRightScore1.Text, out score[0]);
            if (!string.IsNullOrEmpty(txtRightScore2.Text.Trim()))
                err = int.TryParse(txtRightScore2.Text, out score[1]);
            if (!string.IsNullOrEmpty(txtRightScore3.Text.Trim()))
                err = int.TryParse(txtRightScore3.Text, out score[2]);
            if (!string.IsNullOrEmpty(txtRightScore4.Text.Trim()))
                err = int.TryParse(txtRightScore4.Text, out score[3]);
            if (!string.IsNullOrEmpty(txtRightScore5.Text.Trim()))
                err = int.TryParse(txtRightScore5.Text, out score[4]);
            if (!string.IsNullOrEmpty(txtRightScore6.Text.Trim()))
                err = int.TryParse(txtRightScore6.Text, out score[5]);

            if (!string.IsNullOrEmpty(txtRightTotal1.Text.Trim()))
                err = int.TryParse(txtRightTotal1.Text, out totalScore1);
            if (!string.IsNullOrEmpty(txtRightTotal2.Text.Trim()))
                err = int.TryParse(txtRightTotal2.Text, out totalScore2);

            if (checkBoxRight1.Checked == true)
                scoreMark[0] = ScoringScreen.ScoreMark.UnderlineMark;
            if (checkBoxRight2.Checked == true)
                scoreMark[1] = ScoringScreen.ScoreMark.UnderlineMark;
            if (checkBoxRight3.Checked == true)
                scoreMark[2] = ScoringScreen.ScoreMark.UnderlineMark;
            if (checkBoxRight4.Checked == true)
                scoreMark[3] = ScoringScreen.ScoreMark.UnderlineMark;
            if (checkBoxRight5.Checked == true)
                scoreMark[4] = ScoringScreen.ScoreMark.UnderlineMark;
            if (checkBoxRight6.Checked == true)
                scoreMark[5] = ScoringScreen.ScoreMark.UnderlineMark;

            /*更新缓存 -> 添加发送队列 -> 调用后台进程*/

            /* debug
            Screen.RightScore.RefreshScore("北京队", new int[] { 10, 1, 9, 10, 8, 9 },
                new ScoringScreen.ScoreMark[] { ScoringScreen.ScoreMark.UnderlineMark, ScoringScreen.ScoreMark.NoMark,
                    ScoringScreen.ScoreMark.NoMark, ScoringScreen.ScoreMark.UnderlineMark,
                    ScoringScreen.ScoreMark.NoMark, ScoringScreen.ScoreMark.UnderlineMark },
                10, 100);
            */

            Screen.RightScore.RefreshScore(title, score, scoreMark, totalScore1, totalScore2);
            Screen.AddSendQue(Screen.Select.RightScore);
            if (ScreenSendWorker.IsBusy == false)
            {
                ScreenSendWorker.RunWorkerAsync();
            }
        }


        private void buttonScrLeftSend_Click_New(object sender, EventArgs e)
        {
            bool err;
            int inputScore = 0;
            string inputScoreTxt = "";
            bool flag = false;

            if (checkBoxUseAutoCalcCore.Checked == true)
            {
                if (GloableVar.PlayerIndex != 0)
                {
                    /*不是a队*/
                    MessageBox.Show("点击或输入位置不正确，请重试！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if(!(GloableVar.NextPlayer == Game.GamePlayer.None || GloableVar.NextPlayer == Game.GamePlayer.PlayerA))
                {
                    MessageBox.Show("输入顺序不正确，请重试！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                

                /*获取分数，添加到自动计分中*/
                switch (GloableVar.ScoreIndex)
                {
                    case 1:
                        if (!string.IsNullOrEmpty(txtLeftScore1.Text.Trim()))
                        {
                            err = int.TryParse(txtLeftScore1.Text, out inputScore);
                            inputScoreTxt = txtLeftScore1.Text;
                        }
                        if (checkBoxLeft1.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(txtLeftScore2.Text.Trim()))
                        { 
                            err = int.TryParse(txtLeftScore2.Text, out inputScore);
                            inputScoreTxt = txtLeftScore2.Text;
                        }
                        if (checkBoxLeft2.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                    case 3:
                        if (!string.IsNullOrEmpty(txtLeftScore3.Text.Trim()))
                        { 
                            err = int.TryParse(txtLeftScore3.Text, out inputScore);
                            inputScoreTxt = txtLeftScore3.Text;
                        }
                        if (checkBoxLeft3.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                    case 4:
                        if (!string.IsNullOrEmpty(txtLeftScore4.Text.Trim()))
                        {
                            err = int.TryParse(txtLeftScore4.Text, out inputScore);
                            inputScoreTxt = txtLeftScore4.Text;
                        }
                        if (checkBoxLeft4.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                    case 5:
                        if (!string.IsNullOrEmpty(txtLeftScore5.Text.Trim()))
                        { 
                            err = int.TryParse(txtLeftScore5.Text, out inputScore);
                            inputScoreTxt = txtLeftScore5.Text;
                        }
                        if (checkBoxLeft5.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                    case 6:
                        if (!string.IsNullOrEmpty(txtLeftScore6.Text.Trim()))
                        { 
                            err = int.TryParse(txtLeftScore6.Text, out inputScore);
                            inputScoreTxt = txtLeftScore6.Text;
                        }
                        if (checkBoxLeft6.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                }
                comboBoxAuxAddScore.TabIndex = GloableVar.PlayerIndex;
                txtBoxAuxAddScoreScore.Text = inputScoreTxt;
                checkBoxAuxUncertainScore.Checked = flag;
                checkBoxAuxAdditional.Checked = checkBoxAuxAdditionalScreenInputArea.Checked;

                /*自动积分*/
                btnAddScore_Click( null, null );

            }
            else
            {
                buttonScrLeftSend_Click(null, null);
            }
        }

        private void buttonScrRightSend_Click_New(object sender, EventArgs e)
        {
            bool err;
            int inputScore = 0;
            string inputScoreTxt = "";
            bool flag = false;

            if (checkBoxUseAutoCalcCore.Checked == true)
            {
                if (GloableVar.PlayerIndex != 1)
                {
                    /*不是b队*/
                    MessageBox.Show("点击或输入位置不正确，请重试！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!(GloableVar.NextPlayer == Game.GamePlayer.None || GloableVar.NextPlayer == Game.GamePlayer.PlayerB))
                {
                    MessageBox.Show("输入顺序不正确，请重试！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                

                /*获取分数，添加到自动计分中*/
                switch (GloableVar.ScoreIndex)
                {
                    case 1:
                        if (!string.IsNullOrEmpty(txtRightScore1.Text.Trim()))
                        {
                            err = int.TryParse(txtRightScore1.Text, out inputScore);
                            inputScoreTxt = txtRightScore1.Text;
                        }
                        if (checkBoxRight1.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(txtRightScore2.Text.Trim()))
                        {
                            err = int.TryParse(txtRightScore2.Text, out inputScore);
                            inputScoreTxt = txtRightScore2.Text;
                        }
                        if (checkBoxRight2.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                    case 3:
                        if (!string.IsNullOrEmpty(txtRightScore3.Text.Trim()))
                        {
                            err = int.TryParse(txtRightScore3.Text, out inputScore);
                            inputScoreTxt = txtRightScore3.Text;
                        }
                        if (checkBoxRight3.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                    case 4:
                        if (!string.IsNullOrEmpty(txtRightScore4.Text.Trim()))
                        {
                            err = int.TryParse(txtRightScore4.Text, out inputScore);
                            inputScoreTxt = txtRightScore4.Text;
                        }
                        if (checkBoxRight4.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                    case 5:
                        if (!string.IsNullOrEmpty(txtRightScore5.Text.Trim()))
                        {
                            err = int.TryParse(txtRightScore5.Text, out inputScore);
                            inputScoreTxt = txtRightScore5.Text;
                        }
                        if (checkBoxRight5.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                    case 6:
                        if (!string.IsNullOrEmpty(txtRightScore6.Text.Trim()))
                        {
                            err = int.TryParse(txtRightScore6.Text, out inputScore);
                            inputScoreTxt = txtRightScore6.Text;
                        }
                        if (checkBoxRight6.Checked == true)
                        {
                            flag = true;
                        }
                        break;
                }

                comboBoxAuxAddScore.TabIndex = GloableVar.PlayerIndex;
                txtBoxAuxAddScoreScore.Text = inputScoreTxt;
                checkBoxAuxUncertainScore.Checked = flag;
                checkBoxAuxAdditional.Checked = checkBoxAuxAdditionalScreenInputArea.Checked;

                /*自动积分*/
                btnAddScore_Click(null, null);
            }
            else
            {
                buttonScrRightSend_Click(null, null);
            }
        }


        private void score_KeyPress(object sender, KeyPressEventArgs e)
        {
            //判断是否为数字及退格键
            if (!(Char.IsNumber(e.KeyChar)) && (int)e.KeyChar > 32)
            {
                MessageBox.Show("输入有误！请输入正确的数字！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Handled = true;
            }
        }

        private void checkBoxAuxScoOpen_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAuxScoOpen.Checked == true)
            {
                btnAuxClearAllScore_Click(null,null);   //清空内容

                txtBoxAuxTeamNameA.Enabled = true;
                txtBoxAuxTeamNameB.Enabled = true;
                txtBoxAuxPlayerNameA.Enabled = true;
                txtBoxAuxPlayerNameB.Enabled = true;

                comboBoxAuxAddScore.Enabled = true;
                checkBoxAuxUncertainScore.Enabled = true;
                checkBoxAuxAdditional.Enabled = true;
                txtBoxAuxAddScoreScore.Enabled = true;
                btnAddScore.Enabled = true;

                comboBoxAuxChangeScore.Enabled = true;
                txtBoxChangeScoreRound.Enabled = true;
                txtBoxAuxChangeScoreBranch.Enabled = true;
                txtBoxAuxChangeScoreScore.Enabled = true;
                btnChangeScore.Enabled = true;

                btnAuxClearAllScore.Enabled = true;
                btnAuxBackScore.Enabled = true;
                btnDelScore.Enabled = true;

                //buttonScrLeftLock.Text = "锁定";
                //buttonScrRightLock.Text = "锁定";

                //buttonScrLeftLock_Click(null, null);
                //buttonScrRightLock_Click(null, null);

                radioBtnGameModIndiv.Enabled = true;
                radioBtnGameModTeam.Enabled = true;
                radioBtnGameModMix.Enabled = true;
                radioBtnGameModInteg.Enabled = true;
                radioBtnGameModTot.Enabled = true;

                checkBoxAuxScoAutoSend.Enabled = true;
                checkBoxLockMode.Enabled = true;

                btnAuxSetPlayerName.Enabled = true;

                MessageBox.Show("辅助计分已经开启！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                txtBoxAuxTeamNameA.Enabled = false;
                txtBoxAuxTeamNameB.Enabled = false;
                txtBoxAuxPlayerNameA.Enabled = false;
                txtBoxAuxPlayerNameB.Enabled = false;

                comboBoxAuxAddScore.Enabled = false;
                checkBoxAuxUncertainScore.Enabled = false;
                checkBoxAuxAdditional.Enabled = false;
                txtBoxAuxAddScoreScore.Enabled = false;
                btnAddScore.Enabled = false;

                comboBoxAuxChangeScore.Enabled = false;
                txtBoxChangeScoreRound.Enabled = false;
                txtBoxAuxChangeScoreBranch.Enabled = false;
                txtBoxAuxChangeScoreScore.Enabled = false;
                btnChangeScore.Enabled = false;

                btnAuxClearAllScore.Enabled = false;
                btnAuxBackScore.Enabled = false;
                btnDelScore.Enabled = false;

                //buttonScrLeftLock.Text = "解除锁定";
                //buttonScrRightLock.Text = "解除锁定";

                //buttonScrLeftLock_Click(null, null);
                //buttonScrRightLock_Click(null, null);

                radioBtnGameModIndiv.Enabled = false;
                radioBtnGameModTeam.Enabled = false;
                radioBtnGameModMix.Enabled = false;
                radioBtnGameModInteg.Enabled = false;
                radioBtnGameModTot.Enabled = false;

                checkBoxAuxScoAutoSend.Enabled = false;
                checkBoxLockMode.Enabled = false;

                btnAuxSetPlayerName.Enabled = false;

                /*同时关闭屏幕区域*/
                checkBoxUseAutoCalcCore.Checked = false;
            }
        }

        

        private void radioBtnGameScoreMod_CheckedChanged(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定修改比赛模式？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                    == DialogResult.Cancel)
            {
                return;
            }

            if (radioBtnGameModInteg.Checked == true)
            {
                //修改屏幕设置按钮
                checkBoxScreenSetShowInteg.Checked = true;

                //修改屏幕语言
                Screen.LeftScore.ChangeTextMode(ScoringScreen.TextMode.Intergral);
                Screen.RightScore.ChangeTextMode(ScoringScreen.TextMode.Intergral);

                //设置 是否显示积分
                Game.PlayerA.SetIncludeIntegral(true);
                Game.PlayerB.SetIncludeIntegral(true);

                //设置输入框
                txtLeftTotal2.Enabled = true;
                txtRightTotal2.Enabled = true;
            }
            else if (radioBtnGameModTot.Checked == true)
            {
                //修改屏幕设置按钮
                checkBoxScreenSetShowInteg.Checked = false;

                //修改屏幕语言
                Screen.LeftScore.ChangeTextMode(ScoringScreen.TextMode.TotalScore);
                Screen.RightScore.ChangeTextMode(ScoringScreen.TextMode.TotalScore);

                //设置 是否显示积分
                Game.PlayerA.SetIncludeIntegral(false);
                Game.PlayerB.SetIncludeIntegral(false);

                //设置输入框
                txtLeftTotal2.Enabled = false;
                txtRightTotal2.Enabled = false;
            }

            //刷新页面内容
            if (checkBoxAuxScoOpen.Checked == true)
            {
                RefreshAuxInterface();
            }
        }

        private void radioBtnGamePlayerType_CheckedChanged(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定修改比赛模式？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                    == DialogResult.Cancel)
            {
                return;
            }

            //未开启不刷新
            if (checkBoxAuxScoOpen.Checked == false)
            {
                return;
            }

            if (radioBtnGameModIndiv.Checked == true)
            {
                //设置player A B的 每组射击次数
                Game.PlayerA.SetShootNumOfAGroup(3);
                Game.PlayerB.SetShootNumOfAGroup(3);
            }
            else if (radioBtnGameModTeam.Checked == true)
            {
                //设置player A B的 每组射击次数
                Game.PlayerA.SetShootNumOfAGroup(6);
                Game.PlayerB.SetShootNumOfAGroup(6);
            }
            else if (radioBtnGameModMix.Checked == true)
            {
                //设置player A B的 每组射击次数
                Game.PlayerA.SetShootNumOfAGroup(4);
                Game.PlayerB.SetShootNumOfAGroup(4);
            }

            //刷新界面内容
            if (checkBoxAuxScoOpen.Checked == true)
            {
                RefreshAuxInterface();
            }
        }


        private void txtScore_TextChanged(object sender, EventArgs e)
        {
            TextBox tbCurr = (TextBox)sender;
            int scoreCurr;

            bool err = int.TryParse(tbCurr.Text, out scoreCurr);
            if (err == true)
            {
                if (scoreCurr > 10 || scoreCurr < 0)
                {
                    tbCurr.Text = "";
                    MessageBox.Show("输入有误！请输入范围在0~10之内的数字。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// 显示屏直接输入检测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLeftTitle_TextChanged(object sender, EventArgs e)
        {
            //TextBox tbCurr = (TextBox)sender;
            //MessageBox.Show("文字长度为 "+tbCurr.Text.Length, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            string title = txtLeftTitle.Text;
            if (Screen.LeftScore.MeasureTitleLenIsOk(title) != Error.ErrSuccess)
            {
                MessageBox.Show("输入文字超过屏幕显示范围，无法继续输入！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLeftTitle.Text = txtLeftTitle.Text.Remove(txtLeftTitle.Text.Length - 1, 1);
                txtLeftTitle.SelectionStart = txtLeftTitle.Text.Length;
                return;
            }
        }

        /// <summary>
        /// 显示屏直接输入检测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtRightTitle_TextChanged(object sender, EventArgs e)
        {
            string title = txtRightTitle.Text;
            if (Screen.RightScore.MeasureTitleLenIsOk(title) != Error.ErrSuccess)
            {
                MessageBox.Show("输入文字超过屏幕显示范围，无法继续输入！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRightTitle.Text = txtRightTitle.Text.Remove(txtRightTitle.Text.Length - 1, 1);
                txtRightTitle.SelectionStart = txtRightTitle.Text.Length;
                return;
            }
        }

        private void txtBoxAuxTeamNameA_TextChanged_1(object sender, EventArgs e)
        {
            string textLeft = " " + txtBoxAuxTeamNameA.Text + txtBoxAuxPlayerNameA.Text;
            if (Screen.LeftScore.MeasureTitleLenIsOk(textLeft) != Error.ErrSuccess)
            {
                MessageBox.Show("输入文字超过屏幕显示范围，无法继续输入！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //2017年4月8日 删除一个字符在连续输入多个中文时候存在问题，故修改为全部清空（以下几个函数相同）
                //txtBoxAuxTeamNameA.Text = txtBoxAuxTeamNameA.Text.Remove(txtBoxAuxTeamNameA.Text.Length - 1, 1);
                //txtBoxAuxTeamNameA.SelectionStart = txtBoxAuxTeamNameA.Text.Length;
                txtBoxAuxTeamNameA.Text = "";
                return;
            }

        }

        private void txtBoxAuxPlayerNameA_TextChanged_1(object sender, EventArgs e)
        {
            string textLeft = " " + txtBoxAuxTeamNameA.Text + txtBoxAuxPlayerNameA.Text;
            if (Screen.LeftScore.MeasureTitleLenIsOk(textLeft) != Error.ErrSuccess)
            {
                MessageBox.Show("输入文字超过屏幕显示范围，无法继续输入！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //txtBoxAuxPlayerNameA.Text = txtBoxAuxPlayerNameA.Text.Remove(txtBoxAuxPlayerNameA.Text.Length - 1, 1);
                //txtBoxAuxPlayerNameA.SelectionStart = txtBoxAuxPlayerNameA.Text.Length;
                txtBoxAuxPlayerNameA.Text = "";
                return;
            }
        }

        private void txtBoxAuxTeamNameB_TextChanged_1(object sender, EventArgs e)
        {
            string textRight = " " + txtBoxAuxTeamNameB.Text + txtBoxAuxPlayerNameB.Text;
            if (Screen.RightScore.MeasureTitleLenIsOk(textRight) != Error.ErrSuccess)
            {
                MessageBox.Show("输入文字超过屏幕显示范围，无法继续输入！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //txtBoxAuxTeamNameB.Text = txtBoxAuxTeamNameB.Text.Remove(txtBoxAuxTeamNameB.Text.Length - 1, 1);
                //txtBoxAuxTeamNameB.SelectionStart = txtBoxAuxTeamNameB.Text.Length;
                txtBoxAuxTeamNameB.Text = "";
                return;
            }
        }

        private void txtBoxAuxPlayerNameB_TextChanged_1(object sender, EventArgs e)
        {
            string textRight = " " + txtBoxAuxTeamNameB.Text + txtBoxAuxPlayerNameB.Text;
            if (Screen.RightScore.MeasureTitleLenIsOk(textRight) != Error.ErrSuccess)
            {
                MessageBox.Show("输入文字超过屏幕显示范围，无法继续输入！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //txtBoxAuxPlayerNameB.Text = txtBoxAuxPlayerNameB.Text.Remove(txtBoxAuxPlayerNameB.Text.Length - 1, 1);
                //txtBoxAuxPlayerNameB.SelectionStart = txtBoxAuxPlayerNameB.Text.Length;
                txtBoxAuxPlayerNameB.Text = "";
                return;
            }
        }

        private void buttonScrLeftNextRound_Click(object sender, EventArgs e)
        {
            /*  在辅助计分模式开启的情况下，应当允许用户手动进入下一组屏幕
            if(buttonScrLeftLock.Text == "解除锁定")
            {
                MessageBox.Show("请先将屏幕输入锁定解除！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            */
            {
                txtLeftScore1.Text = "";
                txtLeftScore2.Text = "";
                txtLeftScore3.Text = "";
                txtLeftScore4.Text = "";
                txtLeftScore5.Text = "";
                txtLeftScore6.Text = "";

                checkBoxLeft1.Checked = false;
                checkBoxLeft2.Checked = false;
                checkBoxLeft3.Checked = false;
                checkBoxLeft4.Checked = false;
                checkBoxLeft5.Checked = false;
                checkBoxLeft6.Checked = false;
            }

            {
                txtRightScore1.Text = "";
                txtRightScore2.Text = "";
                txtRightScore3.Text = "";
                txtRightScore4.Text = "";
                txtRightScore5.Text = "";
                txtRightScore6.Text = "";

                checkBoxRight1.Checked = false;
                checkBoxRight2.Checked = false;
                checkBoxRight3.Checked = false;
                checkBoxRight4.Checked = false;
                checkBoxRight5.Checked = false;
                checkBoxRight6.Checked = false;
            }

            if (radioBtnGameModInteg.Checked)
            {
                txtLeftTotal2.Text = "";
                txtRightTotal2.Text = "";
            }

            //调用一下发送，把屏幕修改一下
            buttonScrLeftSend_Click(null, null);
            buttonScrRightSend_Click(null, null);
        }

        //废弃
        private void buttonScrRightNextRound_Click(object sender, EventArgs e)
        {
            /*  在辅助计分模式开启的情况下，应当允许用户手动进入下一组屏幕
            if (buttonScrRightLock.Text == "解除锁定")
            {
                MessageBox.Show("请先将屏幕输入锁定解除！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            */
            {
                txtRightScore1.Text = "";
                txtRightScore2.Text = "";
                txtRightScore3.Text = "";
                txtRightScore4.Text = "";
                txtRightScore5.Text = "";
                txtRightScore6.Text = "";

                checkBoxRight1.Checked = false;
                checkBoxRight2.Checked = false;
                checkBoxRight3.Checked = false;
                checkBoxRight4.Checked = false;
                checkBoxRight5.Checked = false;
                checkBoxRight6.Checked = false;
            }

            //调用一下发送，把屏幕修改一下

        }

        private void buttonScrLeftLock_Click(object sender, EventArgs e)
        {
            if (buttonScrLeftLock.Text == "锁定")
            {
                //按钮进行锁定
                buttonScrLeftLock.Text = "解除锁定";
                txtLeftTitle.ReadOnly = true;
                txtLeftScore1.ReadOnly = true;
                txtLeftScore2.ReadOnly = true;
                txtLeftScore3.ReadOnly = true;
                txtLeftScore4.ReadOnly = true;
                txtLeftScore5.ReadOnly = true;
                txtLeftScore6.ReadOnly = true;

                txtLeftTotal1.ReadOnly = true;
                txtLeftTotal2.ReadOnly = true;

                checkBoxLeft1.Enabled = false;
                checkBoxLeft2.Enabled = false;
                checkBoxLeft3.Enabled = false;
                checkBoxLeft4.Enabled = false;
                checkBoxLeft5.Enabled = false;
                checkBoxLeft6.Enabled = false;
            }
            else
            {
                //已经处于锁定状态，按钮进行解锁
                buttonScrLeftLock.Text = "锁定";
                txtLeftTitle.ReadOnly = false;
                txtLeftScore1.ReadOnly = false;
                txtLeftScore2.ReadOnly = false;
                txtLeftScore3.ReadOnly = false;
                txtLeftScore4.ReadOnly = false;
                txtLeftScore5.ReadOnly = false;
                txtLeftScore6.ReadOnly = false;

                txtLeftTotal1.ReadOnly = false;
                txtLeftTotal2.ReadOnly = false;

                checkBoxLeft1.Enabled = true;
                checkBoxLeft2.Enabled = true;
                checkBoxLeft3.Enabled = true;
                checkBoxLeft4.Enabled = true;
                checkBoxLeft5.Enabled = true;
                checkBoxLeft6.Enabled = true;
            }
        }

        private void buttonScrRightLock_Click(object sender, EventArgs e)
        {
            if (txtRightTitle.ReadOnly == true)
            {
                //已经处于锁定状态，按钮进行解锁
                buttonScrRightLock.Text = "锁定";
                txtRightTitle.ReadOnly = false;
                txtRightScore1.ReadOnly = false;
                txtRightScore2.ReadOnly = false;
                txtRightScore3.ReadOnly = false;
                txtRightScore4.ReadOnly = false;
                txtRightScore5.ReadOnly = false;
                txtRightScore6.ReadOnly = false;

                txtRightTotal1.ReadOnly = false;
                txtRightTotal2.ReadOnly = false;

                checkBoxRight1.Enabled = true;
                checkBoxRight2.Enabled = true;
                checkBoxRight3.Enabled = true;
                checkBoxRight4.Enabled = true;
                checkBoxRight5.Enabled = true;
                checkBoxRight6.Enabled = true;

            }
            else
            {
                //按钮进行锁定
                buttonScrRightLock.Text = "解除锁定";
                txtRightTitle.ReadOnly = true;
                txtRightScore1.ReadOnly = true;
                txtRightScore2.ReadOnly = true;
                txtRightScore3.ReadOnly = true;
                txtRightScore4.ReadOnly = true;
                txtRightScore5.ReadOnly = true;
                txtRightScore6.ReadOnly = true;

                txtRightTotal1.ReadOnly = true;
                txtRightTotal2.ReadOnly = true;

                checkBoxRight1.Enabled = false;
                checkBoxRight2.Enabled = false;
                checkBoxRight3.Enabled = false;
                checkBoxRight4.Enabled = false;
                checkBoxRight5.Enabled = false;
                checkBoxRight6.Enabled = false;
            }
        }

        private void buttonScrLeftClear_Click(object sender, EventArgs e)
        {
            if (buttonScrLeftLock.Text == "解除锁定")
            {
                MessageBox.Show("请先将屏幕输入锁定解除！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要清空所有分数？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                    == DialogResult.Cancel)
            {
                return;
            }


            txtLeftTitle.Text = "";
            txtLeftScore1.Text = "";
            txtLeftScore2.Text = "";
            txtLeftScore3.Text = "";
            txtLeftScore4.Text = "";
            txtLeftScore5.Text = "";
            txtLeftScore6.Text = "";

            checkBoxLeft1.Checked = false;
            checkBoxLeft2.Checked = false;
            checkBoxLeft3.Checked = false;
            checkBoxLeft4.Checked = false;
            checkBoxLeft5.Checked = false;
            checkBoxLeft6.Checked = false;

            txtLeftTotal1.Text = "";
            txtLeftTotal2.Text = "";
        }


        private void buttonScrRightClear_Click(object sender, EventArgs e)
        {
            if (buttonScrRightLock.Text == "解除锁定")
            {
                MessageBox.Show("请先将屏幕输入锁定解除！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("确定要清空所有分数？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                    == DialogResult.Cancel)
            {
                return;
            }

            txtRightTitle.Text = "";
            txtRightScore1.Text = "";
            txtRightScore2.Text = "";
            txtRightScore3.Text = "";
            txtRightScore4.Text = "";
            txtRightScore5.Text = "";
            txtRightScore6.Text = "";

            checkBoxRight1.Checked = false;
            checkBoxRight2.Checked = false;
            checkBoxRight3.Checked = false;
            checkBoxRight4.Checked = false;
            checkBoxRight5.Checked = false;
            checkBoxRight6.Checked = false;

            txtRightTotal1.Text = "";
            txtRightTotal2.Text = "";

        }

        private void txtBoxAuxTeamNameA_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBoxAuxPlayerNameA_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBoxAuxTeamNameB_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBoxAuxPlayerNameB_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkBoxAuxScoAutoSend_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAuxScoAutoSend.Checked == true && checkBoxAuxScoOpen.Checked == true)
            {
                //数据发送一次
                buttonScrLeftSend_Click(null, null);
                buttonScrRightSend_Click(null, null);
            }
        }

        private void txtBoxAuxChangeScoreBranch_TextChanged(object sender, EventArgs e)
        {
            TextBox tbCurr = (TextBox)sender;
            int scoreCurr;

            bool err = int.TryParse(tbCurr.Text, out scoreCurr);
            if (err == true)
            {
                if (scoreCurr > 6 || scoreCurr < 1)
                {
                    tbCurr.Text = "";
                    MessageBox.Show("输入有误！请输入范围在1~6之内的数字。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }


        /// <summary>
        /// 刷新界面内容
        /// </summary>
        private void RefreshAuxInterface()
        {
            /*刷新辅助计分界面内容*/
            richBoxPlayerA.Text = Game.PlayerA.ShowScoreNew();
            labelATimes.Text = Game.PlayerA.GetTotShootNum().ToString();
            labelARound.Text = "第" + Game.PlayerA.GetNowGroupOrder().ToString() + "组";
            labelATotInteg.Text = Game.PlayerA.GetNowIntegralString();
            labelATotScor.Text = Game.PlayerA.GetTotalScoreString();

            richBoxPlayerB.Text = Game.PlayerB.ShowScoreNew();
            labelBTimes.Text = Game.PlayerB.GetTotShootNum().ToString();
            labelBRound.Text = "第" + Game.PlayerB.GetNowGroupOrder().ToString() + "组";
            labelBTotInteg.Text = Game.PlayerB.GetNowIntegralString();
            labelBTotScor.Text = Game.PlayerB.GetTotalScoreString();

            if (labelATotInteg.Text.Length == 0)
            {
                labelATotInteg.Text = "-";
            }
            if (labelBTotInteg.Text.Length == 0)
            {
                labelBTotInteg.Text = "-";
            }

            if (Game.GetResultOfGame() == Game.GamePlayer.PlayerA)
            {
                labelAGameResult.Text = "胜";
                labelBGameResult.Text = "负";
            }
            else if (Game.GetResultOfGame() == Game.GamePlayer.PlayerB)
            {
                labelAGameResult.Text = "负";
                labelBGameResult.Text = "胜";
            }
            else
            {
                labelAGameResult.Text = "--";
                labelBGameResult.Text = "--";
            }

            /*刷新屏幕输入模块内容*/
            txtLeftTitle.Text = txtBoxAuxTeamNameA.Text + " " + txtBoxAuxPlayerNameA.Text;
            txtRightTitle.Text = txtBoxAuxTeamNameB.Text + " " + txtBoxAuxPlayerNameB.Text;

            txtLeftScore1.Text = "";
            txtLeftScore2.Text = "";
            txtLeftScore3.Text = "";
            txtLeftScore4.Text = "";
            txtLeftScore5.Text = "";
            txtLeftScore6.Text = "";

            txtRightScore1.Text = "";
            txtRightScore2.Text = "";
            txtRightScore3.Text = "";
            txtRightScore4.Text = "";
            txtRightScore5.Text = "";
            txtRightScore6.Text = "";

            checkBoxLeft1.Checked = false;
            checkBoxLeft2.Checked = false;
            checkBoxLeft3.Checked = false;
            checkBoxLeft4.Checked = false;
            checkBoxLeft5.Checked = false;
            checkBoxLeft6.Checked = false;

            checkBoxRight1.Checked = false;
            checkBoxRight2.Checked = false;
            checkBoxRight3.Checked = false;
            checkBoxRight4.Checked = false;
            checkBoxRight5.Checked = false;
            checkBoxRight6.Checked = false;

            //获取数组
            int[] nowLeftScore = Game.PlayerA.GetNowGroupScoreArray();
            if(nowLeftScore.Length > 0)
            {
                txtLeftScore1.Text = nowLeftScore[0].ToString();
            }
            if (nowLeftScore.Length > 1)
            {
                txtLeftScore2.Text = nowLeftScore[1].ToString();
            }
            if (nowLeftScore.Length > 2)
            {
                txtLeftScore3.Text = nowLeftScore[2].ToString();
            }
            if (nowLeftScore.Length > 3)
            {
                txtLeftScore4.Text = nowLeftScore[3].ToString();
            }
            if (nowLeftScore.Length > 4)
            {
                txtLeftScore5.Text = nowLeftScore[4].ToString();
            }
            if (nowLeftScore.Length > 5)
            {
                txtLeftScore6.Text = nowLeftScore[5].ToString();
            }

            int[] nowRightScore = Game.PlayerB.GetNowGroupScoreArray();
            if (nowRightScore.Length > 0)
            {
                txtRightScore1.Text = nowRightScore[0].ToString();
            }
            if (nowRightScore.Length > 1)
            {
                txtRightScore2.Text = nowRightScore[1].ToString();
            }
            if (nowRightScore.Length > 2)
            {
                txtRightScore3.Text = nowRightScore[2].ToString();
            }
            if (nowRightScore.Length > 3)
            {
                txtRightScore4.Text = nowRightScore[3].ToString();
            }
            if (nowRightScore.Length > 4)
            {
                txtRightScore5.Text = nowRightScore[4].ToString();
            }
            if (nowRightScore.Length > 5)
            {
                txtRightScore6.Text = nowRightScore[5].ToString();
            }

            //获取争议分标记数组
            bool[] nowUncertainFlagL = Game.PlayerA.GetNowGroupUncertainFlag();
            if (nowLeftScore.Length > 0)
            {
                checkBoxLeft1.Checked = nowUncertainFlagL[0];
            }
            if (nowLeftScore.Length > 1)
            {
                checkBoxLeft2.Checked = nowUncertainFlagL[1];
            }
            if (nowLeftScore.Length > 2)
            {
                checkBoxLeft3.Checked = nowUncertainFlagL[2];
            }
            if (nowLeftScore.Length > 3)
            {
                checkBoxLeft4.Checked = nowUncertainFlagL[3];
            }
            if (nowLeftScore.Length > 4)
            {
                checkBoxLeft5.Checked = nowUncertainFlagL[4];
            }
            if (nowLeftScore.Length > 5)
            {
                checkBoxLeft6.Checked = nowUncertainFlagL[5];
            }

            bool[] nowUncertainFlagR = Game.PlayerB.GetNowGroupUncertainFlag();
            if (nowRightScore.Length > 0)
            {
                checkBoxRight1.Checked = nowUncertainFlagR[0];
            }
            if (nowRightScore.Length > 1)
            {
                checkBoxRight2.Checked = nowUncertainFlagR[1];
            }
            if (nowRightScore.Length > 2)
            {
                checkBoxRight3.Checked = nowUncertainFlagR[2];
            }
            if (nowRightScore.Length > 3)
            {
                checkBoxRight4.Checked = nowUncertainFlagR[3];
            }
            if (nowRightScore.Length > 4)
            {
                checkBoxRight5.Checked = nowUncertainFlagR[4];
            }
            if (nowRightScore.Length > 5)
            {
                checkBoxRight6.Checked = nowUncertainFlagR[5];
            }


            //获取积分/总分
            if (checkBoxScreenSetShowInteg.Checked == true)
            {
                txtLeftTotal1.Text = Game.PlayerA.GetNowIntegralString();
                //txtLeftTotal2.Text = Game.PlayerA.GetTotalScoreString();    //积分模式下显示每局总分
                txtLeftTotal2.Text = Game.PlayerA.GetNowGroupScoreTotalString();

                txtRightTotal1.Text = Game.PlayerB.GetNowIntegralString();
                //txtRightTotal2.Text = Game.PlayerB.GetTotalScoreString();
                txtRightTotal2.Text = Game.PlayerB.GetNowGroupScoreTotalString();
            }
            else
            {
                //总分显示在第三行区域1
                txtLeftTotal1.Text = Game.PlayerA.GetTotalScoreString();
                txtLeftTotal2.Text = "";
                txtRightTotal1.Text = Game.PlayerB.GetTotalScoreString();
                txtRightTotal2.Text = "";
            }


            //2017年10月7日新增，同时自动发送以及更新下一个选手等内容
            if (Game.PlayerA.GetNowGroupOrder() > Game.PlayerB.GetNowGroupOrder())
            {
                //a 组多，需要清空b组内容
                txtRightScore1.Text = "";
                txtRightScore2.Text = "";
                txtRightScore3.Text = "";
                txtRightScore4.Text = "";
                txtRightScore5.Text = "";
                txtRightScore6.Text = "";

                checkBoxRight1.Checked = false;
                checkBoxRight2.Checked = false;
                checkBoxRight3.Checked = false;
                checkBoxRight4.Checked = false;
                checkBoxRight5.Checked = false;
                checkBoxRight6.Checked = false;

                if (radioBtnGameModInteg.Checked)
                {
                    txtRightTotal2.Text = "";
                }
            }
            else if (Game.PlayerA.GetNowGroupOrder() < Game.PlayerB.GetNowGroupOrder())
            {
                //清空a组
                txtLeftScore1.Text = "";
                txtLeftScore2.Text = "";
                txtLeftScore3.Text = "";
                txtLeftScore4.Text = "";
                txtLeftScore5.Text = "";
                txtLeftScore6.Text = "";

                checkBoxLeft1.Checked = false;
                checkBoxLeft2.Checked = false;
                checkBoxLeft3.Checked = false;
                checkBoxLeft4.Checked = false;
                checkBoxLeft5.Checked = false;
                checkBoxLeft6.Checked = false;

                if (radioBtnGameModInteg.Checked)
                {
                    txtLeftTotal2.Text = "";
                }
            }

            //自动发送
            if (checkBoxAuxScoAutoSend.Checked == true && checkBoxAuxScoOpen.Checked == true)
            {
                /*同时刷新了两个屏幕*/
                buttonScrLeftSend_Click(null, null);
                buttonScrRightSend_Click(null, null);
            }

            //修改选择团队/选手
            if (Game.GetNextShootPlayerNew() == Game.GamePlayer.PlayerA)
            {
                comboBoxAuxAddScore.SelectedIndex = 0;
            }
            else
            {
                comboBoxAuxAddScore.SelectedIndex = 1;
            }
            GloableVar.NextPlayer = Game.GetNextShootPlayerNew();


        }

        private void RefreshAuxInterface(Game.GamePlayer iplayer)
        {
            /*刷新辅助计分界面内容*/
            richBoxPlayerA.Text = Game.PlayerA.ShowScoreNew();
            labelATimes.Text = Game.PlayerA.GetTotShootNum().ToString();
            labelARound.Text = "第" + Game.PlayerA.GetNowGroupOrder().ToString() + "组";
            labelATotInteg.Text = Game.PlayerA.GetNowIntegralString();
            labelATotScor.Text = Game.PlayerA.GetTotalScoreString();

            richBoxPlayerB.Text = Game.PlayerB.ShowScoreNew();
            labelBTimes.Text = Game.PlayerB.GetTotShootNum().ToString();
            labelBRound.Text = "第" + Game.PlayerB.GetNowGroupOrder().ToString() + "组";
            labelBTotInteg.Text = Game.PlayerB.GetNowIntegralString();
            labelBTotScor.Text = Game.PlayerB.GetTotalScoreString();

            if (labelATotInteg.Text.Length == 0)
            {
                labelATotInteg.Text = "-";
            }
            if (labelBTotInteg.Text.Length == 0)
            {
                labelBTotInteg.Text = "-";
            }

            if (Game.GetResultOfGame() == Game.GamePlayer.PlayerA)
            {
                labelAGameResult.Text = "胜";
                labelBGameResult.Text = "负";
            }
            else if (Game.GetResultOfGame() == Game.GamePlayer.PlayerB)
            {
                labelAGameResult.Text = "负";
                labelBGameResult.Text = "胜";
            }
            else
            {
                labelAGameResult.Text = "--";
                labelBGameResult.Text = "--";
            }

            /*刷新屏幕输入模块内容*/
            txtLeftTitle.Text = txtBoxAuxTeamNameA.Text + " " + txtBoxAuxPlayerNameA.Text;
            txtRightTitle.Text = txtBoxAuxTeamNameB.Text + " " + txtBoxAuxPlayerNameB.Text;

            txtLeftScore1.Text = "";
            txtLeftScore2.Text = "";
            txtLeftScore3.Text = "";
            txtLeftScore4.Text = "";
            txtLeftScore5.Text = "";
            txtLeftScore6.Text = "";

            txtRightScore1.Text = "";
            txtRightScore2.Text = "";
            txtRightScore3.Text = "";
            txtRightScore4.Text = "";
            txtRightScore5.Text = "";
            txtRightScore6.Text = "";

            checkBoxLeft1.Checked = false;
            checkBoxLeft2.Checked = false;
            checkBoxLeft3.Checked = false;
            checkBoxLeft4.Checked = false;
            checkBoxLeft5.Checked = false;
            checkBoxLeft6.Checked = false;

            checkBoxRight1.Checked = false;
            checkBoxRight2.Checked = false;
            checkBoxRight3.Checked = false;
            checkBoxRight4.Checked = false;
            checkBoxRight5.Checked = false;
            checkBoxRight6.Checked = false;

            //获取数组
            int[] nowLeftScore = Game.PlayerA.GetNowGroupScoreArray();
            if (nowLeftScore.Length > 0)
            {
                txtLeftScore1.Text = nowLeftScore[0].ToString();
            }
            if (nowLeftScore.Length > 1)
            {
                txtLeftScore2.Text = nowLeftScore[1].ToString();
            }
            if (nowLeftScore.Length > 2)
            {
                txtLeftScore3.Text = nowLeftScore[2].ToString();
            }
            if (nowLeftScore.Length > 3)
            {
                txtLeftScore4.Text = nowLeftScore[3].ToString();
            }
            if (nowLeftScore.Length > 4)
            {
                txtLeftScore5.Text = nowLeftScore[4].ToString();
            }
            if (nowLeftScore.Length > 5)
            {
                txtLeftScore6.Text = nowLeftScore[5].ToString();
            }

            int[] nowRightScore = Game.PlayerB.GetNowGroupScoreArray();
            if (nowRightScore.Length > 0)
            {
                txtRightScore1.Text = nowRightScore[0].ToString();
            }
            if (nowRightScore.Length > 1)
            {
                txtRightScore2.Text = nowRightScore[1].ToString();
            }
            if (nowRightScore.Length > 2)
            {
                txtRightScore3.Text = nowRightScore[2].ToString();
            }
            if (nowRightScore.Length > 3)
            {
                txtRightScore4.Text = nowRightScore[3].ToString();
            }
            if (nowRightScore.Length > 4)
            {
                txtRightScore5.Text = nowRightScore[4].ToString();
            }
            if (nowRightScore.Length > 5)
            {
                txtRightScore6.Text = nowRightScore[5].ToString();
            }

            //获取争议分标记数组
            bool[] nowUncertainFlagL = Game.PlayerA.GetNowGroupUncertainFlag();
            if (nowLeftScore.Length > 0)
            {
                checkBoxLeft1.Checked = nowUncertainFlagL[0];
            }
            if (nowLeftScore.Length > 1)
            {
                checkBoxLeft2.Checked = nowUncertainFlagL[1];
            }
            if (nowLeftScore.Length > 2)
            {
                checkBoxLeft3.Checked = nowUncertainFlagL[2];
            }
            if (nowLeftScore.Length > 3)
            {
                checkBoxLeft4.Checked = nowUncertainFlagL[3];
            }
            if (nowLeftScore.Length > 4)
            {
                checkBoxLeft5.Checked = nowUncertainFlagL[4];
            }
            if (nowLeftScore.Length > 5)
            {
                checkBoxLeft6.Checked = nowUncertainFlagL[5];
            }

            bool[] nowUncertainFlagR = Game.PlayerB.GetNowGroupUncertainFlag();
            if (nowRightScore.Length > 0)
            {
                checkBoxRight1.Checked = nowUncertainFlagR[0];
            }
            if (nowRightScore.Length > 1)
            {
                checkBoxRight2.Checked = nowUncertainFlagR[1];
            }
            if (nowRightScore.Length > 2)
            {
                checkBoxRight3.Checked = nowUncertainFlagR[2];
            }
            if (nowRightScore.Length > 3)
            {
                checkBoxRight4.Checked = nowUncertainFlagR[3];
            }
            if (nowRightScore.Length > 4)
            {
                checkBoxRight5.Checked = nowUncertainFlagR[4];
            }
            if (nowRightScore.Length > 5)
            {
                checkBoxRight6.Checked = nowUncertainFlagR[5];
            }


            //获取积分/总分
            if (checkBoxScreenSetShowInteg.Checked == true)
            {
                txtLeftTotal1.Text = Game.PlayerA.GetNowIntegralString();
                //txtLeftTotal2.Text = Game.PlayerA.GetTotalScoreString();    //积分模式下显示每局总分
                txtLeftTotal2.Text = Game.PlayerA.GetNowGroupScoreTotalString();

                txtRightTotal1.Text = Game.PlayerB.GetNowIntegralString();
                //txtRightTotal2.Text = Game.PlayerB.GetTotalScoreString();
                txtRightTotal2.Text = Game.PlayerB.GetNowGroupScoreTotalString();
            }
            else
            {
                //总分显示在第三行区域1
                txtLeftTotal1.Text = Game.PlayerA.GetTotalScoreString();
                txtLeftTotal2.Text = "";
                txtRightTotal1.Text = Game.PlayerB.GetTotalScoreString();
                txtRightTotal2.Text = "";
            }


            //2017年10月7日新增，同时自动发送以及更新下一个选手等内容
            if (Game.PlayerA.GetNowGroupOrder() > Game.PlayerB.GetNowGroupOrder())
            {
                //a 组多，需要清空b组内容
                txtRightScore1.Text = "";
                txtRightScore2.Text = "";
                txtRightScore3.Text = "";
                txtRightScore4.Text = "";
                txtRightScore5.Text = "";
                txtRightScore6.Text = "";

                checkBoxRight1.Checked = false;
                checkBoxRight2.Checked = false;
                checkBoxRight3.Checked = false;
                checkBoxRight4.Checked = false;
                checkBoxRight5.Checked = false;
                checkBoxRight6.Checked = false;

                if (radioBtnGameModInteg.Checked)
                {
                    txtRightTotal2.Text = "";
                }
            }
            else if (Game.PlayerA.GetNowGroupOrder() < Game.PlayerB.GetNowGroupOrder())
            {
                //清空a组
                txtLeftScore1.Text = "";
                txtLeftScore2.Text = "";
                txtLeftScore3.Text = "";
                txtLeftScore4.Text = "";
                txtLeftScore5.Text = "";
                txtLeftScore6.Text = "";

                checkBoxLeft1.Checked = false;
                checkBoxLeft2.Checked = false;
                checkBoxLeft3.Checked = false;
                checkBoxLeft4.Checked = false;
                checkBoxLeft5.Checked = false;
                checkBoxLeft6.Checked = false;

                if (radioBtnGameModInteg.Checked)
                {
                    txtLeftTotal2.Text = "";
                }
            }

            //自动发送
            if (checkBoxAuxScoAutoSend.Checked == true && checkBoxAuxScoOpen.Checked == true)
            {

                if (iplayer == Game.GamePlayer.PlayerA &&
                    iplayer == Game.GetNextShootPlayerNew())
                {
                    buttonScrLeftSend_Click(null, null);
                }
                else if (iplayer == Game.GamePlayer.PlayerB &&
                    iplayer == Game.GetNextShootPlayerNew())
                {
                    buttonScrRightSend_Click(null, null);
                }
                else
                {
                    buttonScrLeftSend_Click(null, null);
                    buttonScrRightSend_Click(null, null);
                }
            }

            //修改选择团队/选手
            if (Game.GetNextShootPlayerNew() == Game.GamePlayer.PlayerA)
            {
                comboBoxAuxAddScore.SelectedIndex = 0;
            }
            else
            {
                comboBoxAuxAddScore.SelectedIndex = 1;
            }
            GloableVar.NextPlayer = Game.GetNextShootPlayerNew();


        }

        private void btnAddScore_Click(object sender, EventArgs e)
        {
            int scoreCurr;
            Game.GamePlayer iplayer = Game.GamePlayer.None;

            bool err = int.TryParse(txtBoxAuxAddScoreScore.Text, out scoreCurr);
            if (err == false)
            {
                MessageBox.Show("输入有误！请输入范围在0~10之内的数字。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool uncertainFlag = checkBoxAuxUncertainScore.Checked;
            bool addition = checkBoxAuxAdditional.Checked;

            //判断队伍
            if (comboBoxAuxAddScore.SelectedIndex == 0)
            {
                Game.AddScore(Game.GamePlayer.PlayerA, scoreCurr, uncertainFlag, addition);
                iplayer = Game.GamePlayer.PlayerA;
            }
            else if (comboBoxAuxAddScore.SelectedIndex == 1)
            {
                Game.AddScore(Game.GamePlayer.PlayerB, scoreCurr, uncertainFlag, addition);
                iplayer = Game.GamePlayer.PlayerB;
            }
            else
            {
                MessageBox.Show("请选择队伍！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //复位添加框
            txtBoxAuxAddScoreScore.Text = "";
            checkBoxAuxUncertainScore.Checked = false;
            //checkBoxAdditional.Checked = false;   //附件局选择框不复位

            //刷新辅助计分显示的信息
            RefreshAuxInterface(iplayer);

            
        }

        private void btnAuxBackScore_Click(object sender, EventArgs e)
        {
            //Game.PlayerA.BackOneRecord();
            //Game.PlayerB.BackOneRecord();
            if(Error.ErrSuccess != Game.BackOneScore())
            {
                MessageBox.Show("计分已经为空！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定回退一条输入记录？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                    == DialogResult.Cancel)
            {
                return;
            }

            //刷新
            RefreshAuxInterface();
        }

        private void btnAuxClearAllScore_Click(object sender, EventArgs e)
        {
            /*弹窗提示*/
            if (sender != null)
            {
                if (MessageBox.Show("确定要清空辅助计分模块的所有内容？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                    == DialogResult.Cancel)
                {
                    return;
                }
            }

            //清除名字
            txtBoxAuxTeamNameA.Text = "";
            txtBoxAuxPlayerNameA.Text = "";
            txtBoxAuxTeamNameB.Text = "";
            txtBoxAuxPlayerNameB.Text = "";

            //清空分数
            Game.ClearAll();

            //刷新
            RefreshAuxInterface();

            checkBoxAuxAdditional.Checked = false;
            checkBoxAuxUncertainScore.Checked = false;
        }

        private void checkBoxScreenSetShowInteg_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBoxScreenSetShowInteg.Checked == true)
            {
                radioBtnGameModInteg.Checked = true;
            }
            else
            {
                radioBtnGameModTot.Checked = true;
            }

            //调用radio button clieck事件
            radioBtnGameScoreMod_CheckedChanged(null, null);
        }

        private void btnAuxSetPlayerName_Click(object sender, EventArgs e)
        {
            if(btnAuxSetPlayerName.Text == "确定")
            {
                //将文字更新到屏幕输入框
                string textLeft = txtBoxAuxTeamNameA.Text + " " + txtBoxAuxPlayerNameA.Text;
                if (Screen.LeftScore.MeasureTitleLenIsOk(textLeft) != Error.ErrSuccess)
                {
                    MessageBox.Show("输入文字超过屏幕显示范围，请删除部分字符重试！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string textRight = txtBoxAuxTeamNameB.Text + " " + txtBoxAuxPlayerNameB.Text;
                if (Screen.RightScore.MeasureTitleLenIsOk(textRight) != Error.ErrSuccess)
                {
                    MessageBox.Show("输入文字超过屏幕显示范围，请删除部分字符重试！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtLeftTitle.Text = textLeft;
                txtRightTitle.Text = textRight;

                //修改比赛的文字内容
                Game.SetTeamNameA(txtBoxAuxTeamNameA.Text);
                Game.SetTeamNameB(txtBoxAuxTeamNameB.Text);
                Game.SetPlayerNameA(txtBoxAuxPlayerNameA.Text);
                Game.SetPlayerNameB(txtBoxAuxPlayerNameB.Text);

                //修改button，disable输入框
                btnAuxSetPlayerName.Text = "修改";
                txtBoxAuxTeamNameA.Enabled = false;
                txtBoxAuxPlayerNameA.Enabled = false;
                txtBoxAuxTeamNameB.Enabled = false;
                txtBoxAuxPlayerNameB.Enabled = false;

                //自动发送
                if (checkBoxAuxScoAutoSend.Checked == true && checkBoxAuxScoOpen.Checked == true)
                {
                    buttonScrLeftSend_Click(null, null);
                    buttonScrRightSend_Click(null, null);
                }
            }
            else
            {
                //修改button，disable输入框
                btnAuxSetPlayerName.Text = "确定";
                txtBoxAuxTeamNameA.Enabled = true;
                txtBoxAuxPlayerNameA.Enabled = true;
                txtBoxAuxTeamNameB.Enabled = true;
                txtBoxAuxPlayerNameB.Enabled = true;
            }
            
            
        }

        private void btnChangeScore_Click(object sender, EventArgs e)
        {
            int scoreCurr;
            int groupNum;
            int order;

            if(txtBoxAuxChangeScoreScore.Text == "" ||
                txtBoxChangeScoreRound.Text== "" ||
                txtBoxAuxChangeScoreBranch.Text == "")
            {
                MessageBox.Show("输入有误！请输入正确数据。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要修改该分数？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                    == DialogResult.Cancel)
            {
                return;
            }

            bool err = int.TryParse(txtBoxAuxChangeScoreScore.Text, out scoreCurr);
            err = int.TryParse(txtBoxChangeScoreRound.Text,out groupNum);
            err = int.TryParse(txtBoxAuxChangeScoreBranch.Text,out order);

            Error result = Error.ErrSuccess;
            //判断队伍
            if (comboBoxAuxChangeScore.SelectedIndex == 0)
            {
                result = Game.ChangeScore(Game.GamePlayer.PlayerA, groupNum, order, scoreCurr);
            }
            else if (comboBoxAuxChangeScore.SelectedIndex == 1)
            {
                result = Game.ChangeScore(Game.GamePlayer.PlayerB, groupNum, order, scoreCurr);
            }
            else
            {
                MessageBox.Show("请选择队伍！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //判断返回结果，提示对应信息
            if(result != Error.ErrSuccess)
            {
                MessageBox.Show("输入有误！请重新输入", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //复位输入框
            txtBoxChangeScoreRound.Text = "";
            txtBoxAuxChangeScoreBranch.Text = "";
            txtBoxAuxChangeScoreScore.Text = "";

            //刷新辅助计分显示的信息
            RefreshAuxInterface();

            return;
        }

        private void btnDelScore_Click(object sender, EventArgs e)
        {
            int groupNum;
            int order;

            if (txtBoxChangeScoreRound.Text == "" ||
                txtBoxAuxChangeScoreBranch.Text == "")
            {
                MessageBox.Show("输入有误！请输入正确数据。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要删除该分数？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                    == DialogResult.Cancel)
            {
                return;
            }

            bool err = int.TryParse(txtBoxChangeScoreRound.Text, out groupNum);
            err = int.TryParse(txtBoxAuxChangeScoreBranch.Text, out order);

            Error result = Error.ErrSuccess;
            //判断队伍
            if (comboBoxAuxChangeScore.SelectedIndex == 0)
            {
                result = Game.DeleteScore(Game.GamePlayer.PlayerA, groupNum, order);
            }
            else if (comboBoxAuxChangeScore.SelectedIndex == 1)
            {
                result = Game.DeleteScore(Game.GamePlayer.PlayerB, groupNum, order);
            }
            else
            {
                MessageBox.Show("请选择队伍！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //判断返回结果，提示对应信息
            if (result != Error.ErrSuccess)
            {
                MessageBox.Show("输入有误！请重新输入", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //复位输入框
            txtBoxChangeScoreRound.Text = "";
            txtBoxAuxChangeScoreBranch.Text = "";

            //刷新辅助计分显示的信息
            RefreshAuxInterface();

            return;
        }

        private void checkBoxScreenPreview_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxScreenPreview.Checked == true)
            {
                Screen.LeftScore.OpenPreview();
                Screen.RightScore.OpenPreview();
                Screen.Wind.OpenPreview();
            }
            else
            {
                Screen.LeftScore.ClosePreview();
                Screen.RightScore.ClosePreview();
                Screen.Wind.ClosePreview();
            }
            Screen.AddSendQue(Screen.Select.LeftScore);
            Screen.AddSendQue(Screen.Select.RightScore);
            if (ScreenSendWorker.IsBusy == false)
            {
                ScreenSendWorker.RunWorkerAsync();
            }
        }

        private void 打开比赛文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //自动开启辅助计分
            if (checkBoxAuxScoOpen.Checked == true)
            {
                openGameFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                openGameFileDialog.Filter = "射箭计分数据文件|*.ladf|所有文件|*.*";
                if (openGameFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string pathTemp = openGameFileDialog.FileName;
                    //保存文件到指定目录
                    if (Error.ErrSuccess == Game.OpenFromFile(pathTemp))
                    {
                        //重新设置页面内容
                        switch (Game.GetGameType())
                        {
                            case Game.GameType.IndividualIntegral:
                                radioBtnGameModIndiv.Checked = true;
                                radioBtnGameModInteg.Checked = true;
                                break;
                            case Game.GameType.IndividualTotScore:
                                radioBtnGameModIndiv.Checked = true;
                                radioBtnGameModTot.Checked = true;
                                break;
                            case Game.GameType.TeamIntegral:
                                radioBtnGameModTeam.Checked = true;
                                radioBtnGameModInteg.Checked = true;
                                break;
                            case Game.GameType.TeamTotScore:
                                radioBtnGameModTeam.Checked = true;
                                radioBtnGameModTot.Checked = true;
                                break;
                            case Game.GameType.TeamMixIntegral:
                                radioBtnGameModMix.Checked = true;
                                radioBtnGameModInteg.Checked = true;
                                break;
                            case Game.GameType.TeamMixTotScore:
                                radioBtnGameModMix.Checked = true;
                                radioBtnGameModTot.Checked = true;
                                break;
                        }

                        //设置队名
                        txtBoxAuxTeamNameA.Text = Game.GetTeamNameA();
                        txtBoxAuxPlayerNameA.Text = Game.GetPlayerNameA();
                        txtBoxAuxTeamNameB.Text = Game.GetTeamNameB();
                        txtBoxAuxPlayerNameB.Text = Game.GetPlayerNameB();

                        btnAuxSetPlayerName.Text = "确定";
                        btnAuxSetPlayerName_Click(null, null);

                        //刷新分数内容
                        RefreshAuxInterface();

                        return;
                    }
                    else
                    {
                        MessageBox.Show("文件打开失败，请确保文件格式及内容正确且未被占用！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                MessageBox.Show("未开启辅助计分！请先开启辅助计分功能后，再次加载比赛文件！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void 保存当前比赛ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //开启辅助计分才能保存文件，否则不保存
            if (checkBoxAuxScoOpen.Checked == true)
            {
                saveGameFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                saveGameFileDialog.FileName = DateTime.Now.ToString("yyyy-MM-dd") + "_" + Game.GetTeamNameA() + "_" + Game.GetPlayerNameA() + 
                    "VS" + Game.GetTeamNameB() + "_" + Game.GetPlayerNameB();      //格式为 日期+队名+人名 vs...
                saveGameFileDialog.Filter = "射箭计分数据文件|*.ladf|所有文件|*.*";
                if (saveGameFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string pathTemp = saveGameFileDialog.FileName;
                    //保存文件到指定目录
                    if(Error.ErrSuccess == Game.SaveToFile(pathTemp))
                    {
                        MessageBox.Show("文件保存成功！\n" +"路径："+ pathTemp + ".", "保存成功");
                    }
                    else
                    {
                        MessageBox.Show("文件保存失败，请查看文件是否被占用！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("文件未保存！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                MessageBox.Show("未开启辅助计分工具，无法保存比赛！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
        }

        

        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 软件升级ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.kelsen.site");
        }

        private void 关于ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            AboutForm frm_about = new AboutForm();
            frm_about.Show();
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(Application.StartupPath + "\\help\\Instructions.docx"))
            {
                System.Diagnostics.Process.Start(Application.StartupPath + "\\help\\Instructions.docx");
                return;
            }
            if (File.Exists(Application.StartupPath + "\\help\\Instructions.pdf"))
            {
                System.Diagnostics.Process.Start(Application.StartupPath + "\\help\\Instructions.pdf");
                return;
            }
            else
            {
                System.Diagnostics.Process.Start("http://www.kelsen.site");
                return;
            }
        }

        private void 板卡设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Password frm = new Password();
            frm.Show();
        }

        private void checkBoxLockMode_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxLockMode.Checked == true)
            {
                //数据发送一次
                radioBtnGameModIndiv.Enabled = false;
                radioBtnGameModInteg.Enabled = false;
                radioBtnGameModTeam.Enabled = false;
                radioBtnGameModTot.Enabled = false;
                radioBtnGameModMix.Enabled = false;
            }
            else
            {
                radioBtnGameModIndiv.Enabled = true;
                radioBtnGameModInteg.Enabled = true;
                radioBtnGameModTeam.Enabled = true;
                radioBtnGameModTot.Enabled = true;
                radioBtnGameModMix.Enabled = true;
            }
        }

        private void checkBoxUseAutoCalcCore_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxUseAutoCalcCore.Checked == true)
            {
                /*弹窗提示*/
                if (sender != null)
                {
                    if (MessageBox.Show("开启屏幕自动计算功能将清空当前输入的所有内容。确定要清空当前所有内容并开启？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                        == DialogResult.Cancel)
                    {
                        return;
                    }
                }

                //清除名字
                txtBoxAuxTeamNameA.Text = "";
                txtBoxAuxPlayerNameA.Text = "";
                txtBoxAuxTeamNameB.Text = "";
                txtBoxAuxPlayerNameB.Text = "";

                //清空分数
                Game.ClearAll();

                //刷新
                RefreshAuxInterface();

                checkBoxAuxScoOpen.Checked = true;
                checkBoxAuxScoAutoSend.Checked = true;

                checkBoxAuxAdditionalScreenInputArea.Enabled = true;

            }
            else
            {
                checkBoxAuxAdditionalScreenInputArea.Enabled = false;
            }
        }

        private void txtScore_Enter(int playerIndex,int scoreIndex)
        {
            if (checkBoxUseAutoCalcCore.Checked == true)
            {
                comboBoxAuxAddScore.SelectedIndex = playerIndex;
            }
        }

        private void txtLeftScore1_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 0;
            GloableVar.ScoreIndex = 1;
        }

        private void txtLeftScore2_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 0;
            GloableVar.ScoreIndex = 2;
        }
        private void txtLeftScore3_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 0;
            GloableVar.ScoreIndex = 3;
        }
        private void txtLeftScore4_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 0;
            GloableVar.ScoreIndex = 4;
        }

        private void txtLeftScore5_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 0;
            GloableVar.ScoreIndex = 5;
        }

        private void txtLeftScore6_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 0;
            GloableVar.ScoreIndex = 6;
        }


        private void txtRightScore1_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 1;
            GloableVar.ScoreIndex = 1;
        }

        private void txtRightScore2_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 1;
            GloableVar.ScoreIndex = 2;
        }

        private void txtRightScore3_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 1;
            GloableVar.ScoreIndex = 3;
        }
        private void txtRightScore4_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 1;
            GloableVar.ScoreIndex = 4;
        }
        private void txtRightScore5_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 1;
            GloableVar.ScoreIndex = 5;
        }
        private void txtRightScore6_Enter(object sender, EventArgs e)
        {
            GloableVar.PlayerIndex = 1;
            GloableVar.ScoreIndex = 6;
        }
       

        



        public static class GloableVar
        {
            public static int PlayerIndex = 0;
            public static int ScoreIndex = 1;
            public static Game.GamePlayer NextPlayer = Game.GamePlayer.None;
            public static int CheckDoubleFlag = 0;
        }

        private void checkBoxAuxAdditionalScreenInputArea_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAuxAdditionalScreenInputArea.Checked)
            {
                checkBoxAuxAdditional.Checked = true;
            }
            else
            {
                checkBoxAuxAdditional.Checked = false;
            }
        }

        private void checkBoxAuxAdditional_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxAuxAdditional.Checked)
            {
                checkBoxAuxAdditionalScreenInputArea.Checked = true;
            }
            else
            {
                checkBoxAuxAdditionalScreenInputArea.Checked = false;
            }
        }
    }
}
