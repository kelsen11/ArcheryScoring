using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C_Sharp_Demo
{   
    /// <summary>
    /// 仰邦动态调用接口
    /// </summary>
    public static class YBLedDynaAreaInterf
    {
        /*以下是动态调用接口，详细内容请参考LedDynamicArea使用说明（版本  15.09.01）*/

        /*-------------------------------------------------------------------------------
        过程名:    Initialize
        初始化动态库；该函数不与显示屏通讯。
        参数:
        返回值            :详见返回状态代码定义。
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        private static extern int Initialize(); //初始化动态库    

        /*-------------------------------------------------------------------------------
        过程名:    Uninitialize
        释放动态库；该函数不与显示屏通讯。
        参数:
        返回值            :详见返回状态代码定义。
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        private static extern int Uninitialize(); //释放动态库    

        /*-------------------------------------------------------------------------------
            过程名:    AddScreen_Dynamic
            向动态库中添加显示屏信息；该函数不与显示屏通讯。
            参数:
            nControlType:显示屏的控制器型号，目前该动态区域动态库只支持BX-5E1、BX-5E2、BX-5E3等BX-5E系列控制器。
            nScreenNo：显示屏屏号；该参数与LedshowTW 2013软件中"设置屏参"模块的"屏号"参数一致。
            nSendMode：通讯模式；目前动态库中支持0:串口通讯；2：网络通讯(只支持固定IP模式)；5：保存到文件等三种通讯模式。
            nWidth：显示屏宽度；单位：像素。
            nHeight：显示屏高度；单位：像素。
            nScreenType：显示屏类型；1：单基色；2：双基色。
            nPixelMode：点阵类型，只有显示屏类型为双基色时有效；1：R+G；2：G+R。
            pCom：通讯串口，只有在串口通讯时该参数有效；例："COM1"
            nBaud：通讯串口波特率，只有在串口通讯时该参数有效；目前只支持9600、57600两种波特率。
            pSocketIP：控制器的IP地址；只有在网络通讯(固定IP模式)模式下该参数有效，例："192.168.0.199"
            nSocketPort：控制器的端口地址；只有在网络通讯(固定IP模式)模式下该参数有效，例：5005
            nServerMode     :0:服务器模式未启动；1：服务器模式启动。
            pBarcode        :设备条形码，用于服务器模式和中转服务器
            pNetworkID      :网络ID编号，用于服务器模式和中转服务器
            pServerIP       :中转服务器IP地址
            nServerPort     :中转服务器网络端口
            pServerAccessUser:中转服务器访问用户名
            pServerAccessPassword:中转服务器访问密码
            pCommandDataFile：保存到文件方式时，命令保存命令文件名称。只有在保存到文件模式下该参数有效，例："curCommandData.dat"
            返回值:    详见返回状态代码定义。
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int AddScreen_Dynamic(int nControlType, int nScreenNo, int nSendMode, int nWidth, int nHeight,
              int nScreenType, int nPixelMode, string pCom, int nBaud, string pSocketIP, int nSocketPort, int nStaticIpMode, int nServerMode,
              string pBarcode, string pNetworkID, string pServerIP, int nServerPort, string pServerAccessUser, string pServerAccessPassword,
              string pCommandDataFile);

        /*-------------------------------------------------------------------------------
          过程名:    QuerryServerDeviceList
          查询中转服务器设备的列表信息。
          该函数与显示屏进行通讯
          参数:      
            pTransitDeviceType :中转设备类型 BX-3GPRS，BX-3G
            pServerIP       :中转服务器IP地址
            nServerPort     :中转服务器网络端口
            pServerAccessUser:中转服务器访问用户名
            pServerAccessPassword:中转服务器访问密码
            pDeviceList       : 保存查询的设备列表信息
                将设备的信息用组成字符串, 比如：
                设备1：名称 条形码 状态 类型 网络ID
                设备2：名称 条形码 状态 类型 网络ID
                组成字符串为：'设备1名称,设备1条形码,设备1状态,设备1类型,设备1网络ID;设备2名称,设备2条形码,设备2状态,设备2类型,设备2网络ID'
                设备状态(Byte):  $10:设备未知
                       $11:设备在线
                       $12:设备不在线

                设备类型(Byte): $16:设备为2G
                      $17:设备为3G
            pDeviceCount      : 查询的设备个数

          返回值            :详见返回状态代码定义。
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int QuerryServerDeviceList(string pTransitDeviceType, string pServerIP, int nServerPort, string pServerAccessUser, string pServerAccessPassword, 
                                                        byte[] pDeviceList, ref int nDeviceCount);

        /*-------------------------------------------------------------------------------
          过程名:    AddScreenDynamicArea
          向动态库中指定显示屏添加动态区域；该函数不与显示屏通讯。
          参数:
            nScreenNo：显示屏屏号；该参数与AddScreen函数中的nScreenNo参数对应。
            nDYAreaID：动态区域编号；目前系统中最多5个动态区域；该值取值范围为0~4;
            nRunMode：动态区域运行模式：
                      0:动态区数据循环显示；
                      1:动态区数据显示完成后静止显示最后一页数据；
                      2:动态区数据循环显示，超过设定时间后数据仍未更新时不再显示；
                      3:动态区数据循环显示，超过设定时间后数据仍未更新时显示Logo信息,Logo 信息即为动态区域的最后一页信息
                      4:动态区数据顺序显示，显示完最后一页后就不再显示
            nTimeOut：动态区数据超时时间；单位：秒 
            nAllProRelate：节目关联标志；
                      1：所有节目都显示该动态区域；
                      0：在指定节目中显示该动态区域，如果动态区域要独立于节目显示，则下一个参数为空。
            pProRelateList：节目关联列表，用节目编号表示；节目编号间用","隔开,节目编号定义为LedshowTW 2013软件中"P***"中的"***"
            nPlayImmediately：动态区域是否立即播放0：该动态区域与异步节目一起播放；1：异步节目停止播放，仅播放该动态区域
            nAreaX：动态区域起始横坐标；单位：像素
            nAreaY：动态区域起始纵坐标；单位：像素
            nAreaWidth：动态区域宽度；单位：像素
            nAreaHeight：动态区域高度；单位：像素
            nAreaFMode：动态区域边框显示标志；0：纯色；1：花色；255：无边框
            nAreaFLine：动态区域边框类型, 纯色最大取值为FRAME_SINGLE_COLOR_COUNT；花色最大取值为：FRAME_MULI_COLOR_COUNT
            nAreaFColor：边框显示颜色；选择为纯色边框类型时该参数有效；
            nAreaFStunt：边框运行特技；
                      0：闪烁；1：顺时针转动；2：逆时针转动；3：闪烁加顺时针转动；
                      4:闪烁加逆时针转动；5：红绿交替闪烁；6：红绿交替转动；7：静止打出
            nAreaFRunSpeed：边框运行速度；
            nAreaFMoveStep：边框移动步长；该值取值范围：1~8；
          返回值:    详见返回状态代码定义。
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int AddScreenDynamicArea(int nScreenNo, int nDYAreaID, int nRunMode,
            int nTimeOut, int nAllProRelate, string pProRelateList, int nPlayImmediately,
            int nAreaX, int nAreaY, int nAreaWidth, int nAreaHeight, int nAreaFMode, int nAreaFLine, int nAreaFColor,
            int nAreaFStunt, int nAreaFRunSpeed, int nAreaFMoveStep);

        /*-------------------------------------------------------------------------------
          过程名: DeleteScreenDynamicArea
         * 删除动态库中指定显示屏指定的动态区域信息；该函数不与显示屏通讯。
        参数：
        nScreenNo	显示屏屏号；该参数与AddScreen_Dynamic函数中的nScreenNo参数对应。
        nDYAreaID	动态区域编号；该参数与AddScreenDynamicArea函数中的nDYAreaID参数对应*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int DeleteScreenDynamicArea(int nScreenNo, int nDYAreaID);

        /*-------------------------------------------------------------------------------
          过程名:    AddScreenDynamicAreaFile
          向动态库中指定显示屏的指定动态区域添加信息文件；该函数不与显示屏通讯。
          参数:
            nScreenNo：显示屏屏号；该参数与AddScreen函数中的nScreenNo参数对应。
            nDYAreaID：动态区域编号；该参数与AddScreenDynamicArea函数中的nDYAreaID参数对应
            pFileName：添加的信息文件名称；目前只支持txt(支持ANSI、UTF-8、Unicode等格式编码)、bmp的文件格式
            nShowSingle：文字信息是否单行显示；0：多行显示；1：单行显示；显示该参数只有szFileName为txt格式文档时才有效；
            pFontName：文字信息显示字体；该参数只有szFileName为txt格式文档时才有效；
            nFontSize：文字信息显示字体的字号；该参数只有szFileName为txt格式文档时才有效；
            nBold：文字信息是否粗体显示；0：正常；1：粗体显示；该参数只有szFileName为txt格式文档时才有效；
            nFontColor：文字信息显示颜色；该参数只有szFileName为txt格式文档时才有效；
            nStunt：动态区域信息运行特技；
                      00：随机显示 
                      01：静止显示
                      02：快速打出 
                      03：向左移动 
                      04：向左连移 
                      05：向上移动 
                      06：向上连移 
                      07：闪烁 
                      08：飘雪 
                      09：冒泡 
                      10：中间移出 
                      11：左右移入 
                      12：左右交叉移入 
                      13：上下交叉移入 
                      14：画卷闭合 
                      15：画卷打开 
                      16：向左拉伸 
                      17：向右拉伸 
                      18：向上拉伸 
                      19：向下拉伸 
                      20：向左镭射 
                      21：向右镭射 
                      22：向上镭射 
                      23：向下镭射 
                      24：左右交叉拉幕 
                      25：上下交叉拉幕 
                      26：分散左拉 
                      27：水平百页 
                      28：垂直百页 
                      29：向左拉幕 
                      30：向右拉幕 
                      31：向上拉幕 
                      32：向下拉幕 
                      33：左右闭合 
                      34：左右对开 
                      35：上下闭合 
                      36：上下对开 
                      37：向右移动 
                      38：向右连移 
                      39：向下移动 
                      40：向下连移
            nRunSpeed：动态区域信息运行速度
            nShowTime：动态区域信息显示时间；单位：10ms
          返回值:    详见返回状态代码定义。
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int AddScreenDynamicAreaFile(int nScreenNo, int nDYAreaID,
            string pFileName, int nShowSingle, int nAlignment, string pFontName, int nFontSize, int nBold, int nFontColor,
            int nStunt, int nRunSpeed, int nShowTime);

        /*-------------------------------------------------------------------------------
          过程名:    AddScreenDynamicAreaText
          向动态库中指定显示屏的指定动态区域添加信息文件；该函数不与显示屏通讯。
          参数:
            nScreenNo：显示屏屏号；该参数与AddScreen函数中的nScreenNo参数对应。
            nDYAreaID：动态区域编号；该参数与AddScreenDynamicArea函数中的nDYAreaID参数对应
            pText：添加的信息文件名称；目前只支持txt(支持ANSI、UTF-8、Unicode等格式编码)、bmp的文件格式
            nShowSingle：文字信息是否单行显示；0：多行显示；1：单行显示；显示该参数只有szFileName为txt格式文档时才有效；
            pFontName：文字信息显示字体；该参数只有szFileName为txt格式文档时才有效；
            nFontSize：文字信息显示字体的字号；该参数只有szFileName为txt格式文档时才有效；
            nBold：文字信息是否粗体显示；0：正常；1：粗体显示；该参数只有szFileName为txt格式文档时才有效；
            nFontColor：文字信息显示颜色；该参数只有szFileName为txt格式文档时才有效；
            nStunt：动态区域信息运行特技；
                      00：随机显示 
                      01：静止显示
                      02：快速打出 
                      03：向左移动 
                      04：向左连移 
                      05：向上移动 
                      06：向上连移 
                      07：闪烁 
                      08：飘雪 
                      09：冒泡 
                      10：中间移出 
                      11：左右移入 
                      12：左右交叉移入 
                      13：上下交叉移入 
                      14：画卷闭合 
                      15：画卷打开 
                      16：向左拉伸 
                      17：向右拉伸 
                      18：向上拉伸 
                      19：向下拉伸 
                      20：向左镭射 
                      21：向右镭射 
                      22：向上镭射 
                      23：向下镭射 
                      24：左右交叉拉幕 
                      25：上下交叉拉幕 
                      26：分散左拉 
                      27：水平百页 
                      28：垂直百页 
                      29：向左拉幕 
                      30：向右拉幕 
                      31：向上拉幕 
                      32：向下拉幕 
                      33：左右闭合 
                      34：左右对开 
                      35：上下闭合 
                      36：上下对开 
                      37：向右移动 
                      38：向右连移 
                      39：向下移动 
                      40：向下连移
            nRunSpeed：动态区域信息运行速度
            nShowTime：动态区域信息显示时间；单位：10ms
          返回值:    详见返回状态代码定义。
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int AddScreenDynamicAreaText(int nScreenNo, int nDYAreaID,
            string pText, int nShowSingle, int nAlignment, string pFontName, int nFontSize, int nBold, int nFontColor,
            int nStunt, int nRunSpeed, int nShowTime);

        /*-------------------------------------------------------------------------------
          过程名:    DeleteScreen
          删除动态库中指定显示屏的所有信息；该函数不与显示屏通讯。
          参数:
            nScreenNo：显示屏屏号；该参数与AddScreen函数中的nScreenNo参数对应。
          返回值:    详见返回状态代码定义
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int DeleteScreen_Dynamic(int nScreenNo);

        /*-------------------------------------------------------------------------------
          过程名:    DeleteScreenDynamicAreaFile
          删除动态库中指定显示屏指定的动态区域指定文件信息；该函数不与显示屏通讯。
          参数:
            nScreenNo：显示屏屏号；该参数与AddScreen函数中的nScreenNo参数对应。
            nDYAreaID：动态区域编号；该参数与AddScreenDynamicArea函数中的nDYAreaID参数对应
            nFileOrd：动态区域的指定文件的文件序号；该序号按照文件添加顺序，从0顺序递增，如删除中间的文件，后面的文件序号自动填充。
          返回值:    详见返回状态代码定义
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int DeleteScreenDynamicAreaFile(int nScreenNo, int nDYAreaID, int nFileOrd);

        /*-------------------------------------------------------------------------------
         过程名:    SendDynamicAreaInfoCommand
         发送动态库中指定显示屏指定的动态区域信息到显示屏；该函数与显示屏通讯。
         参数:
           nScreenNo：显示屏屏号；该参数与AddScreen函数中的nScreenNo参数对应。
         * nDelAllDYArea	动态区域编号列表；1：同时发送多个动态区域，0：发送单个动态区域
           pDYAreaIDList	动态区域编号；当nDelAllDYArea为1时，其值为""；当nDelAllDYArea为0时，该参数与AddScreenDynamicArea函数中的nDYAreaID参数对应，发送相应动态区域
          返回值:    详见返回状态代码定义
       -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int SendDynamicAreasInfoCommand(int nScreenNo, int nDelAllDYArea,string pDYAreaIDList);

        /*-------------------------------------------------------------------------------
          过程名:    SendDeleteDynamicAreasCommand
          删除动态库中指定显示屏指定的动态区域信息；同时向显示屏通讯删除指定的动态区域信息。该函数与显示屏通讯
          参数:
            nScreenNo：显示屏屏号；该参数与AddScreen函数中的nScreenNo参数对应。
            nDelAllDYArea	动态区域编号列表；1：同时删除多个动态区域，0：删除单个动态区域
            pDYAreaIDList	动态区域编号；当nDelAllDYArea为1时，其值为""；当nDelAllDYArea为0时，该参数与AddScreenDynamicArea函数中的nDYAreaID参数对应，删除相应动态区域
          返回值:    详见返回状态代码定义
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int SendDeleteDynamicAreasCommand(int nScreenNo, int nDelAllDYArea, string pDYAreaIDList); //删除指定动态区域的信息

        /*-------------------------------------------------------------------------------
          过程名:    StartServer
          启动服务器,用于网络模式下的服务器模式和GPRS通讯模式。
          参数:
            nSendMode       :与显示屏的通讯模式；
              0:串口模式、BX-5A2&RF、BX-5A4&RF等控制器为RF串口无线模式;
              1:GPRS模式
              2:网络模式
              4:WiFi模式
              5:ONBON服务器-GPRS
              6:ONBON服务器-3G
            pServerIP       :中转服务器IP地址
            nServerPort     :中转服务器网络端口
          返回值            :详见返回状态代码定义。
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int StartServer(int nSendMode, string pServerIP, int nServerPort);

        /*-------------------------------------------------------------------------------
          过程名:    StopServer
          关闭服务器,用于网络模式下的服务器模式和GPRS通讯模式。
          参数:
            nSendMode       :与显示屏的通讯模式；
              0:串口模式、BX-5A2&RF、BX-5A4&RF等控制器为RF串口无线模式;
              1:GPRS模式
              2:网络模式
              4:WiFi模式
              5:ONBON服务器-GPRS
              6:ONBON服务器-3G
          返回值            :详见返回状态代码定义。
        -------------------------------------------------------------------------------*/
        [DllImport("LedDynamicArea.dll")]
        public static extern int QuerryServerOnlineList(byte[] pDeviceList);
        [DllImport("LedDynamicArea.dll")]
        public static extern int StopServer(int nSendMode);


        //------------------------------------------------------------------------------
        // 板卡 注：动态库只支持下列板卡
        #region
        public const int CONTROLLER_BX_5E1 = 0x0154;
        public const int CONTROLLER_BX_5E2 = 0x0254;
        public const int CONTROLLER_BX_5E3 = 0x0354;
        public const int CONTROLLER_BX_5Q0P = 0x1056;
        public const int CONTROLLER_BX_5Q1P = 0x1156;
        public const int CONTROLLER_BX_5Q2P = 0x1256;
        public const int CONTROLLER_BX_6Q1 = 0x0166;
        public const int CONTROLLER_BX_6Q2 = 0x0266;
        public const int CONTROLLER_BX_6Q2L = 0x0466;
        public const int CONTROLLER_BX_6Q3 = 0x0366;
        public const int CONTROLLER_BX_6Q3L = 0x0566;

        public const int CONTROLLER_BX_5E1_INDEX = 0;
        public const int CONTROLLER_BX_5E2_INDEX = 1;
        public const int CONTROLLER_BX_5E3_INDEX = 2;
        public const int CONTROLLER_BX_5Q0P_INDEX = 3;
        public const int CONTROLLER_BX_5Q1P_INDEX = 4;
        public const int CONTROLLER_BX_5Q2P_INDEX = 5;
        public const int CONTROLLER_BX_6Q1_INDEX = 6;
        public const int CONTROLLER_BX_6Q2_INDEX = 7;
        public const int CONTROLLER_BX_6Q2L_INDEX = 8;
        public const int CONTROLLER_BX_6Q3_INDEX = 9;
        public const int CONTROLLER_BX_6Q3L_INDEX = 10;

        public const int FRAME_SINGLE_COLOR_COUNT = 23;    //纯色边框图片个数
        public const int FRAME_MULI_COLOR_COUNT = 47;      //花色边框图片个数

        //------------------------------------------------------------------------------
        // 通讯模式
        public const int SEND_MODE_SERIALPORT = 0;
        public const int SEND_MODE_NETWORK = 2;
        public const int SEND_MODE_Server_2G = 5;
        public const int SEND_MODE_Server_3G = 6;
        public const int SEND_MODE_SAVEFILE = 7;
        //------------------------------------------------------------------------------

        //------------------------------------------------------------------------------
        // 动态区域运行模式
        public const int RUN_MODE_CYCLE_SHOW = 0; //动态区数据循环显示；
        public const int RUN_MODE_SHOW_LAST_PAGE = 1; //动态区数据显示完成后静止显示最后一页数据；
        public const int RUN_MODE_SHOW_CYCLE_WAITOUT_NOSHOW = 2; //动态区数据循环显示，超过设定时间后数据仍未更新时不再显示；
        public const int RUN_MODE_SHOW_ORDERPLAYED_NOSHOW = 4; //动态区数据顺序显示，显示完最后一页后就不再显示
        //------------------------------------------------------------------------------

        //==============================================================================
        // 返回状态代码定义
        public const int RETURN_ERROR_NOFIND_DYNAMIC_AREA = 0xE1;      //没有找到有效的动态区域。
        public const int RETURN_ERROR_NOFIND_DYNAMIC_AREA_FILE_ORD = 0xE2; //在指定的动态区域没有找到指定的文件序号。
        public const int RETURN_ERROR_NOFIND_DYNAMIC_AREA_PAGE_ORD = 0xE3; //在指定的动态区域没有找到指定的页序号。
        public const int RETURN_ERROR_NOSUPPORT_FILETYPE = 0xE4;       //不支持该文件类型。
        public const int RETURN_ERROR_RA_SCREENNO = 0xF8;              //已经有该显示屏信息。如要重新设定请先DeleteScreen删除该显示屏再添加；
        public const int RETURN_ERROR_NOFIND_AREA = 0xFA;              //没有找到有效的显示区域；可以使用AddScreenProgramBmpTextArea添加区域信息。
        public const int RETURN_ERROR_NOFIND_SCREENNO = 0xFC;          //系统内没有查找到该显示屏；可以使用AddScreen函数添加显示屏
        public const int RETURN_ERROR_NOW_SENDING = 0xFD;              //系统内正在向该显示屏通讯，请稍后再通讯；
        public const int RETURN_ERROR_OTHER = 0xFF;                    //其它错误；
        public const int RETURN_ERROR_NOTCONNECT = 0xFE;               //未连接；   //手册未注明该参数
        public const int RETURN_NOERROR = 0;                           //没有错误
        //==============================================================================
        #endregion
        private enum InitState
        {
            InitNotDo = 0,
            InitDone,
            UninitDone,
        }
        private static InitState InitFlag = InitState.InitNotDo;

        public static int Init()
        {
            if (InitFlag == InitState.InitDone)
            {
                return RETURN_NOERROR;
            }
            InitFlag = InitState.InitDone;
            return Initialize();    //初始化动态库
        }

        public static int UnInit()
        {
            if (InitFlag == InitState.UninitDone)
            {
                return RETURN_NOERROR;
            }
            else if (InitFlag == InitState.InitDone)
            {
                InitFlag = InitState.UninitDone;
                return Uninitialize(); //释放动态库
            }

            return RETURN_ERROR_OTHER;
        }

        /// <summary>
        /// 获取错误内部错误信息
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="nResult"></param>
        /// <returns></returns>
        public static string GetErrorMessage(string fun, int nResult)
        {
            //string szResult;
            //DateTime dt = DateTime.Now;
            //szResult = "[" + dt.ToString() + "] " + szfunctionName + ":";
            /*后期增加log形式*/
            string message;

            switch (nResult)
            {
                case RETURN_ERROR_NOFIND_DYNAMIC_AREA:
                    message = "无有效的动态区！";
                    break;
                case RETURN_ERROR_NOFIND_DYNAMIC_AREA_FILE_ORD:
                    message = "未找到指定的文件序号！";
                    break;
                case RETURN_ERROR_NOFIND_DYNAMIC_AREA_PAGE_ORD:
                    message = "未找到指定的页序号！";
                    break;
                case RETURN_ERROR_NOSUPPORT_FILETYPE:
                    message = "动态库不支持该文件类型！";
                    break;
                case RETURN_ERROR_RA_SCREENNO:
                    message = "系统中已经有该显示屏信息！";
                    break;
                case RETURN_ERROR_NOFIND_AREA:
                    message = "系统中未找到有效的动态区域！";
                    break;
                case RETURN_ERROR_NOFIND_SCREENNO:
                    message = "系统中未找到该显示屏！";
                    break;
                case RETURN_ERROR_NOW_SENDING:
                    message = "通信中！";
                    break;
                case RETURN_ERROR_OTHER:
                    message = "其他错误！";
                    break;
                case RETURN_NOERROR:
                    message = "屏幕数据发送成功！";
                    break;
                case RETURN_ERROR_NOTCONNECT:
                    message = "屏幕未连接！";
                    break;
                default:
                    message = "其他错误（0x" + nResult.ToString("x") + "）！";
                    break;
            }

            return "[" + fun + "]" + message;
        }

        /// <summary>
        /// 将仰邦内部错误转换为本软件定义的标准错误
        /// </summary>
        /// <param name="YBDllErr"></param>
        /// <returns></returns>
        public static ProcResult.ErrType TransStandErr(int YBDllErr)
        {
            ProcResult.ErrType err = ProcResult.ErrType.ErrSuccess;
            switch (YBDllErr)
            {
                case RETURN_ERROR_NOFIND_DYNAMIC_AREA:
                case RETURN_ERROR_NOFIND_DYNAMIC_AREA_FILE_ORD:
                case RETURN_ERROR_NOFIND_DYNAMIC_AREA_PAGE_ORD:
                case RETURN_ERROR_NOSUPPORT_FILETYPE:
                case RETURN_ERROR_RA_SCREENNO:
                case RETURN_ERROR_NOFIND_AREA:
                case RETURN_ERROR_NOFIND_SCREENNO:
                    err = ProcResult.ErrType.ErrFailed;
                    break;
                case RETURN_ERROR_NOW_SENDING:
                    err = ProcResult.ErrType.ErrBusy;
                    break;
                case RETURN_ERROR_OTHER:
                    err = ProcResult.ErrType.ErrFailed;
                    break;
                case RETURN_NOERROR:
                    err = ProcResult.ErrType.ErrSuccess;
                    break;
                case RETURN_ERROR_NOTCONNECT:
                    err = ProcResult.ErrType.ErrNotConnect;
                    break;
                default:
                    err = ProcResult.ErrType.ErrFailed;
                    break;
            }
            return err;
        }

    }


    /// <summary>
    /// 上海仰邦 led板卡屏幕类
    /// </summary>
    public class YBScreen
    {
        protected int ScreenNum;            //屏幕序号，必须与实际安装设定的板卡序号一致
        protected int BoardId;                //板卡id，参考仰邦接口中板卡
        protected int BoardIdIndex;           //板卡index

        protected int ScreenWidth;          //屏幕宽度
        protected int ScreenHeight;          //屏幕高度


        protected string ScreenSocketIP;    //屏幕通讯固定Socket IP地址
        protected int ScreenSocketPort;     //屏幕通讯固定Socket端口号

        protected string ErrorMessage;      //上一次结束的错误信息

        protected string BmpBuffFile;       //动态屏幕显示缓存文件名

        protected int DisEffects;           //显示特效

        protected int DynaAreaID;           //动态区id

        private Bitmap BmpBuff;             //bmp 缓存

        private bool Opened;                //开关

        private bool State;             //当前屏幕状态

        private bool PreviewFlag;        //预览开关


        private object lockerBmp = new object();
        private object lockerSend = new object();


        /*其他固定参数*/ /*在目前板卡使用中是无效或固定参数*/
        private const int ScreenSendMode = YBLedDynaAreaInterf.SEND_MODE_NETWORK;//屏幕发送模式，参考仰邦接口中通讯模式
        private const string ScreenCOMM = "COM1";       //串口通讯中的串口
        private const int ScreenBaud = 57600;           //波特率
        private const int StaticIpMode = 0;             //0 TCP模式，1 UDP模式
        private const int ServerMode = 0;               //0 未启动，1 启动

        /*1：单基色 2：双基色 3：双基色 4：全彩色 只有5q系列支持 5：双基色灰度 */
        private const int ScreenType = 4;               //屏幕类型，参考样板卡动态区域dll使用说明

        private const int FileOrTxtAlignmentMode = 0;       //0 居左，1 居中，2 居右
        private const int DelAllDYAreaMode = 0;             //动态区域编号列表；1：同时发送多个动态区域，0：发送单个动态区域



        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <param name="id"></param>
        /// <param name="index"></param>
        /// <param name="width"></param>
        /// <param name="hight"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        protected YBScreen(int num, int id, int index, int width, int hight, string ip, int port)
        {
            ScreenNum = num;
            BoardId = id;
            BoardIdIndex = index;
            ScreenWidth = width;
            ScreenHeight = hight;
            ScreenSocketIP = ip;
            ScreenSocketPort = port;
            ErrorMessage = "";
            DisEffects = 1;             //1为精制显示，详细效果请参考接口文档
            DynaAreaID = 0;             //只支持0-4个动态区

            /*自动命名缓存图片名称*/
            BmpBuffFile = Application.StartupPath + "\\ScrDisTmp" + ScreenNum + ".bmp";

            /*根据长宽生成空白图片*/
            Bitmap bmp = new Bitmap(ScreenWidth, ScreenHeight);
            Graphics g = Graphics.FromImage(bmp);
            RectangleF rect = new RectangleF() { X = 0, Y = 0, Height = ScreenHeight, Width = ScreenWidth };
            g.FillRectangle(Brushes.Black, rect);

            BmpBuff = bmp;
            BmpBuff.Save(BmpBuffFile, ImageFormat.Bmp);

            /*开关*/
            Opened = false;
            PreviewFlag = false;
        }

        /// <summary>
        /// 配置文件板卡序号转换
        /// </summary>
        /// <param name="index">配置文件中的索引</param>
        /// <returns>dll使用的板卡控制器id</returns>
        private int BoardConfIdTrans(int index)
        {
            int id = BoardId;
            switch (index)
            {
                case YBLedDynaAreaInterf.CONTROLLER_BX_5E1_INDEX:
                    id = YBLedDynaAreaInterf.CONTROLLER_BX_5E1;
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_5E2_INDEX:
                    id = YBLedDynaAreaInterf.CONTROLLER_BX_5E2;
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_5E3_INDEX:
                    id = YBLedDynaAreaInterf.CONTROLLER_BX_5E3;
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_5Q0P_INDEX:
                    id = YBLedDynaAreaInterf.CONTROLLER_BX_5Q0P;
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P_INDEX:
                    id = YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P;
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_5Q2P_INDEX:
                    id = YBLedDynaAreaInterf.CONTROLLER_BX_5Q2P;
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_6Q1_INDEX:
                    id = YBLedDynaAreaInterf.CONTROLLER_BX_6Q1;
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_6Q2_INDEX:
                    id = YBLedDynaAreaInterf.CONTROLLER_BX_6Q2;
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_6Q2L_INDEX:
                    id = YBLedDynaAreaInterf.CONTROLLER_BX_6Q2L;
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_6Q3_INDEX:
                    id = YBLedDynaAreaInterf.CONTROLLER_BX_6Q3;
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_6Q3L_INDEX:
                    id = YBLedDynaAreaInterf.CONTROLLER_BX_6Q3L;
                    break;

                default:
                    break;
            }
            return id;
        }

        /// <summary>
        /// 板卡名称转换为配置文件id
        /// </summary>
        /// <param name="boardName">板卡名称</param>
        /// <returns>配置文件id字符串</returns>
        private string BoardNameTrans(string boardName)
        {
            string confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P_INDEX.ToString();
            switch (boardName)
            {
                case "BX-5E1":
                    confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_5E1_INDEX.ToString();
                    break;
                case "BX-5E2":
                    confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_5E2_INDEX.ToString();
                    break;
                case "BX-5E3":
                    confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_5E3_INDEX.ToString();
                    break;
                case "BX-5Q0+":
                    confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_5Q0P_INDEX.ToString();
                    break;
                case "BX-5Q1+":
                    confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P_INDEX.ToString();
                    break;
                case "BX-5Q2+":
                    confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_5Q2P_INDEX.ToString();
                    break;
                case "BX-6Q1":
                    confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_6Q1_INDEX.ToString();
                    break;
                case "BX-6Q2":
                    confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_6Q2_INDEX.ToString();
                    break;
                case "BX-6Q2L":
                    confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_6Q2L_INDEX.ToString();
                    break;
                case "BX-6Q3":
                    confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_6Q3_INDEX.ToString();
                    break;
                case "BX-6Q3L":
                    confIdStr = YBLedDynaAreaInterf.CONTROLLER_BX_6Q3L_INDEX.ToString();
                    break;
                default:
                    break;
            }
            return confIdStr;
        }



        /// <summary>
        /// 从配置文件读取板卡控制器类型
        /// </summary>
        private void InitBoardId()
        {
            //打开配置文件
            string confFile = Application.StartupPath + "\\screenConf.ini";
            if (!File.Exists(confFile))      //如果不存在，则创建
            {
                //新的写入方法
                File.WriteAllText(confFile, Resource1.default_conf, Encoding.Default);
            }

            using (StreamReader sr = File.OpenText(confFile))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.StartsWith("#") || s.StartsWith("["))
                        continue;
                    else
                    {
                        if (s.StartsWith("Board" + ScreenNum.ToString() + "Id="))
                        {
                            string[] arr = s.Split('=');
                            if (true == int.TryParse(arr[1], out BoardIdIndex))
                            {
                                BoardId = BoardConfIdTrans(BoardIdIndex);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 修改配置文件中board id
        /// 入参为板卡名称，仅支持：
        ///     "BX_5Q0+"
        ///     "BX_5Q1+"
        ///     "BX_5Q2+"
        /// </summary>
        /// <param name="boardName">板卡名称</param>
        public void ChangeBoardId(string boardName)
        {
            string confFile = Application.StartupPath + "\\screenConf.ini";
            string[] lineTemp;

            if (!File.Exists(confFile))      //如果不存在，则创建
            {
                //新的写入方法
                File.WriteAllText(confFile, Resource1.default_conf, System.Text.Encoding.Default);
            }

            lineTemp = File.ReadAllLines(confFile, System.Text.Encoding.Default);

            //MessageBox.Show(Convert.ToString(lineTemp.Length));
            int i;
            for (i = 0; i < lineTemp.Length; i++)
            {
                if (lineTemp[i].StartsWith("Board" + ScreenNum.ToString() + "Id="))
                {
                    lineTemp[i] = "Board" + ScreenNum.ToString() + "Id=" + BoardNameTrans(boardName);
                    //lineTemp[i] = "BoardId=" + boardName;
                }
            }

            File.WriteAllLines(confFile, lineTemp, System.Text.Encoding.Default);    //再写入文件
        }

        /// <summary>
        /// 获取板卡类型索引
        /// </summary>
        /// <returns>索引号</returns>
        private int GetBoardIndex()
        {
            return BoardIdIndex;
        }

        public string GetBoardNameStrFromCfg()
        {
            int idTmp = YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P_INDEX;
            //打开配置文件
            string confFile = Application.StartupPath + "\\screenConf.ini";
            if (!File.Exists(confFile))      //如果不存在，则创建
            {
                //新的写入方法
                File.WriteAllText(confFile, Resource1.default_conf, Encoding.Default);
            }

            using (StreamReader sr = File.OpenText(confFile))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.StartsWith("#") || s.StartsWith("["))
                        continue;
                    else
                    {
                        if (s.StartsWith("Board" + ScreenNum.ToString() + "Id="))
                        {
                            string[] arr = s.Split('=');
                            if (true != int.TryParse(arr[1], out idTmp))
                            {
                                return "BX-5Q1+";
                            }
                        }
                    }
                }
            }


            string name;
            switch(idTmp)
            {
                case YBLedDynaAreaInterf.CONTROLLER_BX_5E1_INDEX:
                    name = "BX-5E1";
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_5E2_INDEX:
                    name = "BX-5E2";
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_5E3_INDEX:
                    name = "BX-5E3";
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_5Q0P_INDEX:
                    name = "BX-5Q0+";
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P_INDEX:
                    name = "BX-5Q1+";
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_5Q2P_INDEX:
                    name = "BX-5Q2+";
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_6Q1_INDEX:
                    name = "BX-6Q1";
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_6Q2_INDEX:
                    name = "BX-6Q2";
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_6Q2L_INDEX:
                    name = "BX-6Q2L";
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_6Q3_INDEX:
                    name = "BX-6Q3";
                    break;
                case YBLedDynaAreaInterf.CONTROLLER_BX_6Q3L_INDEX:
                    name = "BX-6Q3L";
                    break;

                default:
                    name = "BX-5Q1+";
                    break;
            }
            return name;
        }

        public void Init()
        {
            //根据配置文件初始化board id
            InitBoardId();

            int err = YBLedDynaAreaInterf.Init();
            ErrorMessage = YBLedDynaAreaInterf.GetErrorMessage("Init dll", err);
            if (err != YBLedDynaAreaInterf.RETURN_NOERROR)
            {
                return;
            }

            //添加屏幕
            err = YBLedDynaAreaInterf.AddScreen_Dynamic(BoardId, ScreenNum, ScreenSendMode, ScreenWidth, ScreenHeight, ScreenType, 1,
                ScreenCOMM, ScreenBaud, ScreenSocketIP, ScreenSocketPort, StaticIpMode, ServerMode, null, null, null, 0, null, null,
                Application.StartupPath + "\\ScrStats" + ScreenNum + ".ini");
            ErrorMessage = YBLedDynaAreaInterf.GetErrorMessage("AddScreen_Dynamic", err);
            if (err != YBLedDynaAreaInterf.RETURN_NOERROR)
            {
                return;
            }

            //添加动态区     //注：每个屏幕只有一个动态区，每次更改内容通过删除新增文件实现
            err = YBLedDynaAreaInterf.AddScreenDynamicArea(ScreenNum, DynaAreaID, YBLedDynaAreaInterf.RUN_MODE_CYCLE_SHOW, 10000, 1, null, 1, 0, 0, ScreenWidth, ScreenHeight,
                255, 0, 255, 0, 0, 1);
            ErrorMessage = YBLedDynaAreaInterf.GetErrorMessage("AddScreenDynamicArea", err);
            if (err != YBLedDynaAreaInterf.RETURN_NOERROR)
            {
                return;
            }

            //添加动态区文件
            err = YBLedDynaAreaInterf.AddScreenDynamicAreaFile(ScreenNum, DynaAreaID, BmpBuffFile, 0, FileOrTxtAlignmentMode, "宋体", 12, 0, 0, DisEffects, 0, 100 * 3600);
            ErrorMessage = YBLedDynaAreaInterf.GetErrorMessage("AddScreenDynamicAreaFile", err);
            if (err != YBLedDynaAreaInterf.RETURN_NOERROR)
            {
                return;
            }

            Thread.Sleep(5);
        }

        public void UnInit()
        {
            //删除动态区等操作（略）

            //uninit
            int err = YBLedDynaAreaInterf.UnInit();
            ErrorMessage = YBLedDynaAreaInterf.GetErrorMessage("UnInit", err);
        }


        protected void SetBufWithLock(Bitmap bmp)
        {
            lock (lockerBmp)
            {
                BmpBuff = bmp;
            }
        }

        public void OpenPreview()
        {
            PreviewFlag = true;
        }

        public void ClosePreview()
        {
            PreviewFlag = false;
        }

        public Bitmap GetBmpBufWithLock()
        {
            Bitmap ret;

            if (Opened == true || PreviewFlag == true)
            {
                lock (lockerBmp)
                {
                    ret = BmpBuff;
                }
            }
            else
            {
                //绘制全灰色背景
                Bitmap bmp = new Bitmap(ScreenWidth, ScreenHeight);
                Graphics g = Graphics.FromImage(bmp);
                RectangleF rect = new RectangleF() { X = 0, Y = 0, Height = ScreenHeight, Width = ScreenWidth };
                g.FillRectangle(Brushes.DimGray, rect);
                ret = bmp;
            }
            return ret;
        }

        private void SaveBmpFileWithLock()
        {
            lock (lockerBmp)
            {
                BmpBuff.Save(BmpBuffFile, ImageFormat.Bmp);
                Thread.Sleep(20);
            }
        }

        public void ClearScreen()
        {
            /*根据长宽生成空白图片*/
            Bitmap bmp = new Bitmap(ScreenWidth, ScreenHeight);
            Graphics g = Graphics.FromImage(bmp);
            RectangleF rect = new RectangleF() { X = 0, Y = 0, Height = ScreenHeight, Width = ScreenWidth };
            g.FillRectangle(Brushes.Black, rect);

            SetBufWithLock(bmp);
        }

        /// <summary>
        /// 指定坐标显示指定大小图片文件
        /// </summary>
        /// <param name="img"></param>
        /// <param name="x">坐标x</param>
        /// <param name="y">坐标y</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        public void ShowImage(Image img, int x, int y, int width, int height)
        {
            //指定坐标显示指定大小图片文件
        }

        /// <summary>
        /// 获取屏幕开启状态
        /// </summary>
        /// <returns></returns>
        public bool GetOpenState()
        {
            return Opened;
        }

        /// <summary>
        /// 开启屏幕
        /// </summary>
        public void SetOpen()
        {
            Opened = true;
        }

        /// <summary>
        /// 关闭屏幕
        /// </summary>
        public void SetClose()
        {
            Opened = false;
        }

        public ProcResult.ErrType SendInfo()
        {
            int err = YBLedDynaAreaInterf.RETURN_NOERROR;
            //保存屏幕缓存文件
            SaveBmpFileWithLock();

            //发送屏幕信息
            if (Opened == true) //只有屏幕开启才对屏幕发送信息
            {
                lock (lockerSend) //dll调用互斥锁
                {
                    //更新动态链接库缓存，删除并重新添加文件
                    err = YBLedDynaAreaInterf.DeleteScreenDynamicAreaFile(ScreenNum, DynaAreaID, 0);    //注意文件序号是从0开始的
                    ErrorMessage = YBLedDynaAreaInterf.GetErrorMessage("DeleteScreenDynamicAreaFile", err);
                    if (err != YBLedDynaAreaInterf.RETURN_NOERROR)
                    {
                        return YBLedDynaAreaInterf.TransStandErr(err);
                    }
                    err = YBLedDynaAreaInterf.AddScreenDynamicAreaFile(ScreenNum, DynaAreaID, BmpBuffFile, 0, FileOrTxtAlignmentMode, "宋体", 12, 0, 0, DisEffects, 0, 100 * 3600);
                    ErrorMessage = YBLedDynaAreaInterf.GetErrorMessage("AddScreenDynamicAreaFile", err);
                    if (err != YBLedDynaAreaInterf.RETURN_NOERROR)
                    {
                        return YBLedDynaAreaInterf.TransStandErr(err);
                    }
                    Thread.Sleep(5);

                    err = YBLedDynaAreaInterf.SendDynamicAreasInfoCommand(ScreenNum, DelAllDYAreaMode, DynaAreaID.ToString());
                    ErrorMessage = YBLedDynaAreaInterf.GetErrorMessage("SendDynamicAreaInfoCommand", err);
                    if (err != YBLedDynaAreaInterf.RETURN_NOERROR)
                    {
                        return YBLedDynaAreaInterf.TransStandErr(err);
                    }

                    Thread.Sleep(5);
                }
            }
            else
            {
                return ProcResult.ErrType.ErrNotConnect;
            }

            return YBLedDynaAreaInterf.TransStandErr(err);
        }

        /// <summary>
        /// 检查屏幕是否连接
        /// 该检查只在屏幕关闭的情况下进行，使用时防止多个屏幕并行发送
        /// </summary>
        /// <returns>true 连接，false 未连接</returns>
        public bool CheckOn()
        {
            if (Opened == false)
            {
                lock (lockerSend) //dll调用互斥锁
                {
                    //添加黑色图片，不能影响buf中的内容

                    //发送并检查状态
                    int err = YBLedDynaAreaInterf.SendDynamicAreasInfoCommand(ScreenNum, DelAllDYAreaMode, DynaAreaID.ToString());
                    ErrorMessage = YBLedDynaAreaInterf.GetErrorMessage("SendDynamicAreaInfoCommand", err);
                    //区分不同的错误类型
                    if (err == YBLedDynaAreaInterf.RETURN_NOERROR)
                    {
                        //屏幕连接，则自动打开
                        Opened = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

    }

    /// <summary>
    /// 分数显示屏类
    /// </summary>
    public class ScoringScreen : YBScreen
    {
        public enum ColorStyle
        {
            Yellow = 0,
            Green,
            Mixture,
        }

        public enum Language
        {
            Chinese = 0,
            English,
        }

        public enum ScoreMark
        {
            NoMark = 0,
            UnderlineMark,
        }

        public enum TextMode
        {
            Intergral = 0,
            TotalScore,
        }

        /* 屏幕显示区域 */
        private string Title;       //标题
        private string ScoreStr;    //分数文字——“环数”、“SCORE”
        private string TotalArea1;      //积分文字区
        private string TotalArea2;      //总分文字区

        private int[] ScoreArray;        //分数（环数）数值
        private ScoreMark[] ScoreMarkArray;    //分数下划线标记
        private int TotalScore1;        //积分
        private int TotalScore2;        //总分


        /* 字体、画刷 */
        private Font TitleFont;
        private Font OtherTextFont;
        private Font UnderlinTextFont;

        private Brush TitleBrush;
        private Brush OtherTextBrush;
        private Brush ScoreBrush;

        /* 比赛模式 */
        private ColorStyle ScreenColor;
        private Language ScreenLanguage;
        private TextMode ScreenTextMode;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <param name="id">无用</param>
        /// <param name="index">无用</param>
        /// <param name="width"></param>
        /// <param name="hight"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public ScoringScreen(int num, int id, int index, int width, int hight, string ip, int port) : base(num, id, index, width, hight, ip, port)
        {
            TitleFont = new Font("宋体", 12, FontStyle.Bold);     //初始化默认为宋体
            OtherTextFont = new Font("宋体", 12, FontStyle.Bold);
            UnderlinTextFont = new Font("宋体", 12, FontStyle.Bold | FontStyle.Underline);

            GetConfColor();

            /*语言和模式设定，及文字内容初始化*/
            GetConfLanguage();
            SetTextMode(TextMode.Intergral);    /*默认为积分制*/
            MakeScreenText();//注意一定要生成文字

            Title = "";
            ScoreArray = new int[] { -1, -1, -1, -1, -1, -1 };
            ScoreMarkArray = new ScoreMark[] { ScoreMark.NoMark, ScoreMark.NoMark, ScoreMark.NoMark, ScoreMark.NoMark, ScoreMark.NoMark, ScoreMark.NoMark };
            TotalScore1 = -1;
            TotalScore2 = -1;
        }


        /*  Color Style  */

        private int SetColor(ColorStyle screenColor)
        {
            ScreenColor = screenColor;
            ScoreBrush = Brushes.White;     //分数使用白色显示
            int num;

            /*以下为颜色模板定义*/
            switch (screenColor)
            {
                case ColorStyle.Green:
                    TitleBrush = Brushes.Lime;
                    OtherTextBrush = Brushes.Lime;
                    num = 2;
                    break;
                case ColorStyle.Yellow:
                    TitleBrush = Brushes.Yellow;
                    OtherTextBrush = Brushes.Yellow;
                    num = 1;
                    break;
                case ColorStyle.Mixture:
                default:
                    TitleBrush = Brushes.Yellow;
                    OtherTextBrush = Brushes.Lime;
                    num = 3;
                    break;
            }
            return num;
        }

        private void GetConfColor()
        {
            //打开配置文件
            string confFile = Application.StartupPath + "\\screenConf.ini";
            if (!File.Exists(confFile))      //如果不存在，则创建
            {
                //新的写入方法
                File.WriteAllText(confFile, Resource1.default_conf, Encoding.Default);
            }

            using (StreamReader sr = File.OpenText(confFile))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.StartsWith("#") || s.StartsWith("["))
                        continue;
                    else
                    {
                        if (s.StartsWith("Color="))
                        {
                            if (s[6] == '1')
                            {
                                SetColor(ColorStyle.Yellow);
                            }
                            else if (s[6] == '2')
                            {
                                SetColor(ColorStyle.Green);
                            }

                            else if (s[6] == '3')
                            {
                                SetColor(ColorStyle.Mixture);
                            }
                            else
                            {
                                SetColor(ColorStyle.Mixture);
                            }
                        }
                    }
                }
            }
        }

        public ColorStyle GetScreenColor()
        {
            return ScreenColor;
        }

        /// <summary>
        /// 更改显示屏配色
        /// </summary>
        /// <param name="screenColor"></param>
        public void ChangeColor(ColorStyle screenColor)
        {
            int num = SetColor(screenColor);

            string confFile = Application.StartupPath + "\\screenConf.ini";
            string[] lineTemp;

            if (!File.Exists(confFile))      //如果不存在，则创建
            {
                //新的写入方法
                File.WriteAllText(confFile, Resource1.default_conf, System.Text.Encoding.Default);
            }

            lineTemp = File.ReadAllLines(confFile, System.Text.Encoding.Default);

            //MessageBox.Show(Convert.ToString(lineTemp.Length));
            int i;
            for (i = 0; i < lineTemp.Length; i++)
            {
                if (lineTemp[i].StartsWith("Color="))
                {
                    lineTemp[i] = "Color=" + num;
                }
            }

            File.WriteAllLines(confFile, lineTemp, System.Text.Encoding.Default);    //再写入文件
        }


        /*  Language  */
        private int SetLanguage(Language screenLanguage)
        {
            ScreenLanguage = screenLanguage;
            int num;

            switch(ScreenLanguage)
            {
                case Language.English:
                    num = 1;
                    break;
                case Language.Chinese:
                default:
                    num = 2;
                    break;
            }
            return num;
        }

        private void GetConfLanguage()
        {
            /* 从配置文件获取语言类型 */ /* 暂时打桩 */
            SetLanguage(Language.Chinese);
        }

        public void ChangeLanguage(Language screenLanguage)
        {
            SetLanguage(screenLanguage);
            MakeScreenText();
            /* 写入配置文件 */ /* 暂时打桩 */

        }


        /*  Text Mode  */

        /// <summary>
        /// 修改文字显示模式
        /// </summary>
        /// <param name="mode"></param>
        private void SetTextMode(TextMode mode)
        {
            ScreenTextMode = mode;
        }

        public void ChangeTextMode(TextMode mode)
        {
            SetTextMode(mode);
            MakeScreenText();

        }

        /// <summary>
        /// 生成屏幕固定文字
        /// 要在初始化语言和文本模式之后调用
        /// </summary>
        private void MakeScreenText()
        {
            string scoreTxt = "环数";
            string intergTxt = "积分";
            string totalTxt = "总环";

            switch(ScreenLanguage)
            {
                case Language.English:
                    scoreTxt = "SCORE";
                    intergTxt = "INTEG.";
                    totalTxt = "TOT.";
                    break;
                case Language.Chinese:
                default:
                    scoreTxt = "环数";
                    intergTxt = "积分";
                    totalTxt = "总环";
                    break;
            }

            switch(ScreenTextMode)
            {
                case TextMode.TotalScore:
                    ScoreStr = scoreTxt;
                    TotalArea1 = totalTxt;
                    TotalArea2 = "";    /*总分制中，区域2不显示内容*/
                    break;
                case TextMode.Intergral:
                default:
                    ScoreStr = scoreTxt;
                    TotalArea1 = intergTxt;
                    TotalArea2 = totalTxt;
                    break;

            }
        }

        public int SetCustomTxt(string line2Area1,string line3Area1,string line3Area2)
        {
            /*判断文字长度，超长不做修改*/
            if (line2Area1.Length > 10 || line3Area1.Length > 10 || line3Area2.Length > 10)
            {
                return -1;
            }

            ScoreStr = line2Area1;
            TotalArea1 = line3Area1;
            TotalArea2 = line3Area2;
            return 0;
        }

        public int RestoreTxt()
        {
            MakeScreenText();
            return 0;
        }

        public Error MeasureTitleLenIsOk(string title)
        {
            //TitleFont
            //Graphics g = new Graphics;
            Bitmap bmp = new Bitmap(ScreenWidth+100, ScreenHeight+100);
            Graphics g = Graphics.FromImage(bmp);
            SizeF siF = g.MeasureString(title, TitleFont);
            if (siF.Width > ScreenWidth || siF.Height > ScreenHeight)
            {
                return Error.ErrFailed;
            }
            return Error.ErrSuccess;
        }

        /// <summary>
        /// 将入参分数刷新到屏幕缓存image上
        /// </summary>
        /// <param name="title"></param>
        /// <param name="scoreArray"></param>
        /// <param name="markArray"></param>
        /// <param name="totalScore1"></param>
        /// <param name="totalScore2"></param>
        public void RefreshScore(string title, int[] scoreArray, ScoreMark[] markArray, int totalScore1, int totalScore2)
        {
            Bitmap bmp = new Bitmap(ScreenWidth, ScreenHeight);
            Graphics g = Graphics.FromImage(bmp);
            RectangleF backRect = new RectangleF() { X = 0, Y = 0, Height = ScreenHeight, Width = ScreenWidth + 3 };
            RectangleF titleRect = new RectangleF() { X = 0, Y = 0, Height = ScreenHeight / 3, Width = ScreenWidth + 3 };

            StringFormat alignRightFmt = new StringFormat(StringFormatFlags.NoClip);
            alignRightFmt.Alignment = StringAlignment.Far;
            alignRightFmt.LineAlignment = StringAlignment.Near;

            StringFormat alignCenterFmt = new StringFormat(StringFormatFlags.NoClip);
            alignCenterFmt.Alignment = StringAlignment.Center;
            alignCenterFmt.LineAlignment = StringAlignment.Near;

            PointF secondLine = new PointF(-3, 16);
            PointF thirdLineArea1 = new PointF(-3, 32);
            PointF thirdLineArea2 = new PointF(90 - 3, 32);

            //更新buffer
            Title = title;
            ScoreArray = scoreArray;
            ScoreMarkArray = markArray;
            TotalScore1 = totalScore1;
            TotalScore2 = totalScore2;


            //绘图
            g.FillRectangle(Brushes.Black, backRect);
            g.DrawString(Title, TitleFont, TitleBrush, titleRect, alignRightFmt);

            g.DrawString(ScoreStr, OtherTextFont, OtherTextBrush, secondLine);

            g.DrawString(TotalArea1, OtherTextFont, OtherTextBrush, thirdLineArea1);
            g.DrawString(TotalArea2, OtherTextFont, OtherTextBrush, thirdLineArea2);

            //第二行环数
            for (int i = 0; i < 6; i++)
            {
                PointF position;
                RectangleF rectf;
                Font scoreFont;

                /*如果没有分数，则不显示*/
                if (ScoreArray[i] < 0 || ScoreArray[i] > 10)    //出现一次就退出
                {
                    break;
                }

                /* 位置起始坐标（40,16），每个分数框宽20，文字居中 */ /* 根据中英文进行优化 */
                if (ScreenLanguage == Language.Chinese)
                {
                    position = new PointF(40 - 3, 16);
                    position.X += i * 20;
                    rectf = new RectangleF(position, new SizeF(20 + 6, 16));
                }
                else
                {
                    position = new PointF(40, 16);
                    position.X += i * 19;
                    rectf = new RectangleF(position, new SizeF(19 + 8, 16));
                }

                /*下划线标记*/
                if (0 == ScoreMarkArray[i])
                {
                    scoreFont = OtherTextFont;
                }
                else
                {
                    scoreFont = UnderlinTextFont;
                }
                g.DrawString(ScoreArray[i].ToString(), scoreFont, ScoreBrush, rectf, alignCenterFmt);
            }

            //第三行数据
            if (TotalScore1 >= 0)
            {
                g.DrawString(TotalScore1.ToString(), OtherTextFont, ScoreBrush, new RectangleF(thirdLineArea1, new SizeF(85 + 3, 16)), alignRightFmt);
            }

            if (TotalScore2 >= 0 && TotalArea2 != "")
            {
                g.DrawString(TotalScore2.ToString(), OtherTextFont, ScoreBrush, new RectangleF(thirdLineArea2, new SizeF(70 + 3, 16)), alignRightFmt);
            }

            //更新到YBScreen buff
            SetBufWithLock(bmp);
        }

        public void Welcome()
        {
            Bitmap bmp = new Bitmap(ScreenWidth, ScreenHeight);
            Graphics g = Graphics.FromImage(bmp);
            RectangleF rect = new RectangleF() { X = 0, Y = 0, Height = ScreenHeight, Width = ScreenWidth };

            StringFormat fmt = new StringFormat(StringFormatFlags.NoClip);
            fmt.Alignment = StringAlignment.Center;
            fmt.LineAlignment = StringAlignment.Near;

            Font font = new Font("宋体", 15, FontStyle.Bold);    //小三字号


            //绘图
            g.FillRectangle(Brushes.Black, rect);
            g.DrawString("XX体育", font, Brushes.Yellow, new RectangleF(new PointF(0, 0), new SizeF(160, 48 / 2)), fmt);
            g.DrawString("祝比赛圆满成功", font, Brushes.Yellow, new RectangleF(new PointF(0, 48 / 2), new SizeF(160, 48 / 2)), fmt);

            SetBufWithLock(bmp);
        }

        internal void Refresh(string speed, char direction)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 风速显示屏类
    /// </summary>
    public class WindScreen : YBScreen
    {

        private string Speed;
        private char Direction;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <param name="id">无用</param>
        /// <param name="index">无用</param>
        /// <param name="width"></param>
        /// <param name="hight"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public WindScreen(int num, int id, int index, int width, int hight, string ip, int port) :
            base(num, id, index, width, hight, ip, port)
        {
            Speed = "0.0m/s";
            Direction = 'A';
        }

        public void RefreshWind(string speed, char direction)
        {
            Bitmap bmp = new Bitmap(ScreenWidth, ScreenHeight);
            Graphics g = Graphics.FromImage(bmp);
            RectangleF rect = new RectangleF() { X = 0, Y = 0, Height = ScreenHeight, Width = ScreenWidth };


            StringFormat fmt = new StringFormat(StringFormatFlags.NoClip);
            fmt.Alignment = StringAlignment.Center;
            fmt.LineAlignment = StringAlignment.Near;

            Font speedFont = new Font("宋体", 12, FontStyle.Bold);

            Brush speedBrush = Brushes.Yellow;

            //更新缓存
            Speed = speed;
            Direction = direction;

            //绘图
            g.FillRectangle(Brushes.Black, rect);

            g.DrawString(speed, speedFont, speedBrush, rect, fmt);

            Image pic;
            switch (direction)
            {
                case 'A':
                    pic = Resource1.lednorth;
                    break;
                case 'B':
                    pic = Resource1.lednortheast;
                    break;
                case 'C':
                    pic = Resource1.ledeast;
                    break;
                case 'D':
                    pic = Resource1.ledsoutheast;
                    break;
                case 'E':
                    pic = Resource1.ledsouth;
                    break;
                case 'F':
                    pic = Resource1.ledsouthwest;
                    break;
                case 'G':
                    pic = Resource1.ledwest;
                    break;
                case 'H':
                    pic = Resource1.lednorthwest;
                    break;
                default:
                    pic = Resource1.lednorth;
                    break;
            }
            g.DrawImage(pic, 10, 16, 32, 32);

            Image pic2 = Resource1.info;
            g.DrawImage(pic2, 47, 28, 18, 18);

            //更新到YBScreen buff
            SetBufWithLock(bmp);
        }
    }

    /// <summary>
    /// 静态类，做全局变量使用
    /// </summary>
    public static class Screen
    {
        /*   射箭计分显示屏相关参数   */
        private const int SCREEN_NO1 = 1;   //左边计分屏幕
        private const int SCREEN_NO2 = 2;   //中间风速风向屏幕
        private const int SCREEN_NO3 = 3;   //右边计分屏幕
        private const int BIG_SCREEN_WIDTH = 160;
        private const int WIND_SCREEN_WIDTH = 64;
        private const int BIG_SCREEN_HEIGHT = 48;
        private const int WIND_SCREEN_HEIGHT = 48;

#if DEBUG
        //调试使用ip
        private const string SCREEN1_SOCKETIP = "192.168.1.191";
        private const string SCREEN2_SOCKETIP = "192.168.1.192";
        private const string SCREEN3_SOCKETIP = "192.168.1.193";
#else
        /* 发布版本ip *//*  发布版本请使用该组ip  */
        private const string SCREEN1_SOCKETIP = "192.168.1.101";
        private const string SCREEN2_SOCKETIP = "192.168.1.102";
        private const string SCREEN3_SOCKETIP = "192.168.1.103";
#endif

        private const int SCREEN_SOCKETPORT = 5005;


        /* 添加三块屏幕 */
        public static ScoringScreen LeftScore = new ScoringScreen(SCREEN_NO1, YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P, YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P_INDEX,
            BIG_SCREEN_WIDTH, BIG_SCREEN_HEIGHT, SCREEN1_SOCKETIP, SCREEN_SOCKETPORT);
        public static ScoringScreen RightScore = new ScoringScreen(SCREEN_NO3, YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P, YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P_INDEX,
            BIG_SCREEN_WIDTH, BIG_SCREEN_HEIGHT, SCREEN3_SOCKETIP, SCREEN_SOCKETPORT);

        public static WindScreen Wind = new WindScreen(SCREEN_NO2, YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P, YBLedDynaAreaInterf.CONTROLLER_BX_5Q1P_INDEX,
            WIND_SCREEN_WIDTH, WIND_SCREEN_HEIGHT, SCREEN2_SOCKETIP, SCREEN_SOCKETPORT);


        /// <summary>
        /// 屏幕初始化，需要在main form load时候使用
        /// </summary>
        public static void Init()
        {
            /*分别初始化3块屏幕*/   //注意：默认屏幕通信是关闭的
            LeftScore.Init();
            RightScore.Init();
            Wind.Init();

        }


        /// <summary>
        /// 屏幕选择枚举
        /// </summary>
        public enum Select
        {
            Err = -1,
            LeftScore = 0,
            RightScore,
            Wind,
        }

        /* 发送队列机制 */
        private static Queue<Select> SendQueue = new Queue<Select>();

        /// <summary>
        /// 加入发送队列
        /// </summary>
        /// <param name="screen"></param>
        public static void AddSendQue(Select screen)
        {
            SendQueue.Enqueue(screen);
        }

        /// <summary>
        /// 从发送队列取出
        /// </summary>
        /// <returns></returns>
        public static Select DeSendQue()
        {
            Select scr;
            try
            {
                scr = SendQueue.Dequeue();

            }
            catch
            {
                scr = Select.Err;
            }
            return scr;
        }

        /// <summary>
        /// 获取队列数量
        /// </summary>
        /// <returns></returns>
        public static int GetQueueCount()
        {
            return SendQueue.Count;
        }

        
        
    }
}
