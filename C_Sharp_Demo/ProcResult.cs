using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_Sharp_Demo
{
    public enum Error
    {
        ErrSuccess = 0, //成功
        ErrFailed,      //通用错误
        ErrNotConnect,  //屏幕未连接
        ErrNotFound,
        ErrBusy,
        ErrOverLimit,
    }

    public class ProcResult
    {
        public Screen.Select ScreenSelect;
        public ErrType ErrorNum;
        

        public string StatText;
        public Color TextColor;

        public enum ErrType
        {
            ErrSuccess = 0, //成功
            ErrFailed,      //通用错误
            ErrNotConnect,  //屏幕未连接
            ErrNotFound,
            ErrBusy,

        }

        /// <summary>
        /// 屏幕内容
        /// </summary>
        /// <param name="select">所选屏幕</param>
        /// <param name="screen">屏幕bmp内容</param>
        public ProcResult(Screen.Select select)
        {
            ScreenSelect = select;
            ErrorNum = ErrType.ErrSuccess;
            StatText = "正常";
            TextColor = Color.Green;
        }

        /// <summary>
        /// 屏幕内容及操作错误码
        /// </summary>
        /// <param name="select">所选屏幕</param>
        /// <param name="screen">屏幕bmp内容</param>
        /// <param name="errNo">错误码</param>
        public ProcResult(Screen.Select select, ErrType errNo)
        {
            ScreenSelect = select;
            ErrorNum = errNo;
            SetErrText(ErrorNum);
        }

        public ProcResult(ErrType errNo)
        {
            ScreenSelect = Screen.Select.Err;
            ErrorNum = errNo;
            SetErrText(ErrorNum);
        }


        private void SetErrText(ProcResult.ErrType err)
        {
            switch (err)
            {
                case ProcResult.ErrType.ErrSuccess:
                    StatText = "正常";
                    TextColor = Color.Green;
                    break;
                case ProcResult.ErrType.ErrNotConnect:
                    StatText = "未连接";
                    TextColor = Color.Red;
                    break;
                case ProcResult.ErrType.ErrFailed:
                    StatText = "发送失败";
                    TextColor = Color.Orange;
                    break;
                case ProcResult.ErrType.ErrBusy:
                    StatText = "忙碌";
                    TextColor = Color.Yellow;
                    break;
                default:
                    StatText = "发送失败";
                    TextColor = Color.Orange;
                    break;

            }
            
        }

        
    }
}
