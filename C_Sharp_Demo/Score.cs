using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace C_Sharp_Demo
{
    /// <summary>
    /// 分数类
    /// </summary>
    public class Score
    {
        /// <summary>
        /// 分数操作枚举
        /// </summary>
        public enum ScoreOperation
        {
            None = 0,
            AddScore,       //添加
            ChangeScore,    //修改
            DeleteScore,    //删除
        }

        /// <summary>
        /// 分数数据类
        /// </summary>
        public class ScoreData
        {
            public int Order;   //从1开始
            public int Val;
            public bool UncertainFlag;
            public bool AdditionFlag;

            public ScoreData()
            {
                Val = 0;
                UncertainFlag = false;
                AdditionFlag = false;
            }

            public ScoreData(int val)
            {
                Val = val;
                UncertainFlag = false;
                AdditionFlag = false;
            }

            public ScoreData(int val, bool uncertain)
            {
                Val = val;
                UncertainFlag = uncertain;
                AdditionFlag = false;
            }

            public ScoreData(int val, bool uncertain, bool addition)
            {
                Val = val;
                UncertainFlag = uncertain;
                AdditionFlag = addition;
            }

            //求分组总分

            //求addition的总分



        }


        /// <summary>
        /// 操作步骤记录
        /// </summary>
        private class Record
        {
            public ScoreOperation Op;  //操作类型
            public int Order;      //分数序号
            public ScoreData OldVal;         //原分数值

            public Record()
            {

            }
            public Record(ScoreOperation op, int order, ScoreData oldVal)
            {
                Op = op;
                Order = order;
                OldVal = oldVal;
            }

            public Error AddToXmlDoc(XmlDocument xmlDoc, XmlNode parentNode, string className ,int index)
            {
                try
                {
                    XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, className, null);
                    XmlElement xe = (XmlElement)node;
                    xe.SetAttribute("id", index.ToString());
                    //录入分数
                    XmlPublicMethod xm = new XmlPublicMethod();
                    xm.CreateNode(xmlDoc, node, "Op", Op.ToString());
                    xm.CreateNode(xmlDoc, node, "Order", Order.ToString());

                    //OldVal
                    string xml = XmlUtil.Serializer(typeof(ScoreData), OldVal);
                    xm.CreateNode(xmlDoc, node, "OldVal", xml);

                    parentNode.AppendChild(node);
                }
                catch
                {
                    return Error.ErrFailed;
                }
                return Error.ErrSuccess;
            }

            public Error SetByString(XmlNode parentNode)
            {
                try
                {
                    XmlNode root = parentNode.SelectSingleNode("Op");
                    if(root.InnerText == "AddScore")
                    {
                        Op = ScoreOperation.AddScore;
                    }
                    else if (root.InnerText == "ChangeScore")
                    {
                        Op = ScoreOperation.ChangeScore;
                    }
                    else if (root.InnerText == "DeleteScore")
                    {
                        Op = ScoreOperation.DeleteScore;
                    }
                    else if (root.InnerText == "None")
                    {
                        Op = ScoreOperation.None;
                    }
                    else
                    {
                        return Error.ErrFailed;
                    }

                    root = parentNode.SelectSingleNode("Order");
                    if(int.TryParse(root.InnerText, out Order) != true)
                    {
                        return Error.ErrFailed;
                    }

                    root = parentNode.SelectSingleNode("OldVal");
                    ScoreData newOldVal = XmlUtil.Deserialize(typeof(ScoreData), root.InnerText) as ScoreData;
                    OldVal = newOldVal;
                }
                catch
                {
                    return Error.ErrFailed;
                }
                return Error.ErrSuccess;
            }

        }


        //标志量
        private int ShootNumOfEachGroup;       //每组中射击次数
        private bool IncludeIntegral;
        private const int MAX_SHOOT_NUM = 100;  //最大射击次数


        //private int MaxRounds;              //最多比赛局，不算附加局
        //private int MaxWinIntegral;         //获胜积分数

        //数据
        private List<ScoreData> Data = new List<ScoreData>();
        private List<int> Integral = new List<int>();       //计分列表
        private Stack<Record> History = new Stack<Record>();    //历史记录


        /// <summary>
        /// 分数类构造函数
        /// </summary>
        public Score()
        {
            //初始化为个人淘汰 积分制
            ShootNumOfEachGroup = 3;   //每组射击次数初始化设定为6
            IncludeIntegral = true;     //开启积分制

            //获胜规则
            //MaxRounds = 5;
            //MaxWinIntegral = 6;
        }

        /*****************private****************/
        /// <summary>
        /// 添加记录
        /// </summary>
        /// <param name="op">操作类型</param>
        /// <param name="order">分数序号</param>
        /// <returns></returns>
        private Error AddRecord(ScoreOperation op, int order, ScoreData oldScore)
        {
            Error result = Error.ErrSuccess;

            //判断是否能修改/删除的工作，在change score函数中完成
            switch (op)
            {
                case ScoreOperation.AddScore:
                    {
                        History.Push(new Record(op, order, oldScore));
                        break;
                    }
                case ScoreOperation.ChangeScore:
                case ScoreOperation.DeleteScore:
                    {
                        History.Push(new Record(op, order, oldScore));
                        break;
                    }
                default:
                    {
                        result = Error.ErrFailed;
                        break;
                    }
            }

            return result;
        }

        /// <summary>
        /// 恢复一条记录
        /// </summary>
        /// <returns></returns>
        private Error RecoverRecord()
        {
            Error result = Error.ErrSuccess;
            if (History.Count == 0)
            {
                return Error.ErrNotFound;
            }

            Record tmp = History.Pop();

            switch (tmp.Op)
            {
                case ScoreOperation.AddScore:
                    {
                        //删除当前数据
                        Data.RemoveAt(tmp.Order - 1);
                        break;
                    }
                case ScoreOperation.ChangeScore:
                    {
                        //将数据写入数组
                        Data[tmp.Order - 1] = tmp.OldVal;
                        break;
                    }
                case ScoreOperation.DeleteScore:
                    {
                        Data.Insert(tmp.Order - 1, tmp.OldVal);
                        break;
                    }
                default:
                    {
                        result = Error.ErrFailed;
                        break;
                    }
            }

            return result;
        }

        /// <summary>
        /// 清空历史记录
        /// </summary>
        /// <returns></returns>
        private Error ClearHistory()
        {
            History.Clear();
            return Error.ErrSuccess;
        }


        /// <summary>
        /// 插入分数
        /// </summary>
        /// <param name="order">序号，从1开始</param>
        /// <param name="score">分数</param>
        /// <returns></returns>
        private Error InsertScore(int order, ScoreData score)
        {
            if ((order - 1) > Data.Count)
            {
                return Error.ErrFailed;
            }

            Data.Insert(order - 1, score);
            return Error.ErrSuccess;
        }


        /// <summary>
        /// 清空
        /// </summary>
        public void ClearAll()
        {
            Data.Clear();
            Integral.Clear();   //review 此处可以删除，积分只在每次使用时重新计算
            History.Clear();

            return;
        }

        /// <summary>
        /// 回退操作
        /// </summary>
        /// <returns></returns>
        public Error BackOneRecord()
        {
            return RecoverRecord();
        }


        /// <summary>
        /// 获取分数
        /// </summary>
        /// <param name="order">分数序号，从1开始</param>
        /// <returns></returns>
        public ScoreData GetScore(int order)
        {
            return Data[order - 1];
        }

        /// <summary>
        /// 获取分数
        /// </summary>
        /// <param name="gNum">组序号，从1开始</param>
        /// <param name="orderIngroup">在组中的射击序号，从1开始</param>
        /// <returns></returns>
        public ScoreData GetScore(int gNum, int orderIngroup)
        {
            return GetScore((gNum - 1) * ShootNumOfEachGroup + orderIngroup);
        }

        /// <summary>
        /// 获取第一个附加局标志的射击序号，从1开始
        /// </summary>
        /// <returns></returns>
        public int GetAdditionFirstFlag()
        {
            if(Data.Count > 0)
            {
                for (int i = 0;i<Data.Count;i++)
                {
                    if(Data[i].AdditionFlag)
                    {
                        return i+1;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 判断当前是否为附加局
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public bool IsNowAdditionSet()
        {
            /* //以下不使用，取第一个flag为准
            if(Data.Count > 0)
            {
                if(Data[Data.Count-1].AdditionFlag == true)
                {
                    return true;
                }
            }
            return false;
            */

            if (GetAdditionFirstFlag() > 0)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 修改分数
        /// </summary>
        /// <param name="order">分数序号，从1开始</param>
        /// <param name="score">分数数值</param>
        /// <returns></returns>
        public Error ChangeScore(int order, ScoreData score)
        {
            //判断数据是否正确
            if (order > Data.Count || order < 1)
            {
                return Error.ErrNotFound;
            }

            if (score.Val > 10 || score.Val < 0)
            {
                return Error.ErrFailed;
            }

            ScoreData tmp = GetScore(order);

            //添加历史记录
            AddRecord(ScoreOperation.ChangeScore, order, new ScoreData(tmp.Val, tmp.UncertainFlag, tmp.AdditionFlag));

            //将数据写入数组
            tmp.Val = score.Val;
            tmp.UncertainFlag = false;

            //删除重新添加
            Data[order - 1] = tmp;

            return Error.ErrSuccess;
        }

        /// <summary>
        /// 修改分数
        /// </summary>
        /// <param name="gNum">组序号，从1开始</param>
        /// <param name="shootNum">在组中射击序号，从1开始</param>
        /// <param name="score"></param>
        /// <returns></returns>
        public Error ChangeScore(int gNum, int shootNum, ScoreData score)
        {
            return ChangeScore((gNum - 1) * ShootNumOfEachGroup + shootNum, score);
        }

        /// <summary>
        /// 添加分数
        /// </summary>
        /// <param name="score">分数值</param>
        /// <returns></returns>
        public Error AddAScore(ScoreData score)
        {
            if (Data.Count == MAX_SHOOT_NUM || (score.Val < 0 || score.Val > 10))
            {
                //数组已满或数据不正确
                return Error.ErrOverLimit;
            }

            //添加历史记录
            AddRecord(ScoreOperation.AddScore, Data.Count + 1, new ScoreData());

            //添加分数
            Data.Add(score);

            return Error.ErrSuccess;
        }


        /// <summary>
        /// 删除分数
        /// </summary>
        /// <param name="order">分数序号，从1开始</param>
        /// <returns></returns>
        public Error DeleteScore(int order)
        {
            //判断数据是否正确
            if (order > Data.Count || order < 1)
            {
                return Error.ErrNotFound;
            }

            ScoreData tmp = GetScore(order);
            //添加历史记录
            AddRecord(ScoreOperation.DeleteScore, order, new ScoreData(tmp.Val, tmp.UncertainFlag, tmp.AdditionFlag));

            //删除分数
            Data.RemoveAt(order - 1);

            return Error.ErrSuccess;
        }

        /// <summary>
        /// 删除分数
        /// </summary>
        /// <param name="gNum"></param>
        /// <param name="shootNum"></param>
        /// <returns></returns>
        public Error DeleteScore(int gNum, int shootNum)
        {
            return DeleteScore((gNum - 1) * ShootNumOfEachGroup + shootNum);
        }

        /// <summary>
        /// 获取当前分数数组内容
        /// </summary>
        /// <returns></returns>
        public int[] GetNowGroupScoreArray()
        {
            int gNum = Data.Count / ShootNumOfEachGroup;
            int last = Data.Count % ShootNumOfEachGroup;

            if (last == 0 && gNum > 0)
            {
                last = ShootNumOfEachGroup;
                gNum--;
            }

            int[] tmp = new int[last];

            for (int i = 0; i < last; i++)
            {
                tmp[i] = Data[gNum * ShootNumOfEachGroup + i].Val;
            }

            return tmp;
        }

        /// <summary>
        /// 获取正在进行组的总分
        /// </summary>
        /// <returns></returns>
        public string GetNowGroupScoreTotalString()
        {
            if (Data.Count > 0)
            {
                return GetNowGroupScoreArray().Sum().ToString();
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// 获取一组分数数组
        /// </summary>
        /// <param name="groupOrder">组序号，从1开始</param>
        /// <returns></returns>
        public int[] GetAGroupScoreArray(int groupOrder)
        {
            int gNum = Data.Count / ShootNumOfEachGroup;
            int remainder = Data.Count % ShootNumOfEachGroup;

            int ArrSize = ShootNumOfEachGroup;

            if (remainder != 0 && groupOrder > gNum)
            {
                ArrSize = remainder;
            }

            int[] tmp = new int[ArrSize];

            for (int i = 0; i < ArrSize; i++)
            {
                tmp[i] = Data[(groupOrder - 1) * ShootNumOfEachGroup + i].Val;
            }

            return tmp;
        }

        /// <summary>
        /// 获取一组分数的总分
        /// </summary>
        /// <param name="groupOrder"></param>
        /// <returns></returns>
        public int GetAGroupScoreTotal(int groupOrder)
        {
            return GetAGroupScoreArray(groupOrder).Sum();
        }

        /// <summary>
        /// 获取当前组的flag数组
        /// </summary>
        /// <returns></returns>
        public bool[] GetNowGroupUncertainFlag()
        {
            int gNum = Data.Count / ShootNumOfEachGroup;
            int last = Data.Count % ShootNumOfEachGroup;

            if (last == 0 && gNum > 0)
            {
                last = ShootNumOfEachGroup;
                gNum--;
            }

            bool[] tmp = new bool[last];

            for (int i = 0; i < last; i++)
            {
                tmp[i] = Data[gNum * ShootNumOfEachGroup + i].UncertainFlag;
            }

            return tmp;
        }

        /// <summary>
        /// 获取当前总射击次数
        /// </summary>
        /// <returns></returns>
        public int GetTotShootNum()
        {
            return Data.Count;
        }

        /// <summary>
        /// 获取当前进行局number
        /// </summary>
        /// <returns></returns>
        public int GetNowGroupOrder()
        {
            int num = Data.Count / ShootNumOfEachGroup;
            int remainder = Data.Count % ShootNumOfEachGroup;
            if (num == 0)
            {
                return 1;
            }
            if (remainder != 0)
            {
                num++;
            }
            return num;
        }

        /// <summary>
        /// 获取完整局数量
        /// </summary>
        /// <returns></returns>
        public int GetIntactGroupNum()
        {
            int gNumTmp;

            /*计算当前所在组*/
            if (Data.Count % ShootNumOfEachGroup == 0)
            {
                /*本组最后一个数据*/
                gNumTmp = Data.Count / ShootNumOfEachGroup;

            }
            else
            {
                gNumTmp = Data.Count / ShootNumOfEachGroup + 1;
            }

            return gNumTmp;
        }

        public bool IsLastOneOfGroup(int GroupNum)
        {
            /*计算当前所在组*/
            if (GroupNum< GetIntactGroupNum() ||
                (GroupNum == GetIntactGroupNum() && Data.Count%ShootNumOfEachGroup==0))
            {
                /*本组最后一个数据*/
                return true;

            }
            

            return false;
        }

        /// <summary>
        /// 判断当前组是否为附加局的组
        /// </summary>
        /// <param name="gNum">从1开始</param>
        /// <returns></returns>
        private bool IsAdditionGroup(int gNum)
        {
            if(GetAdditionFirstFlag() == 0 || gNum == 0)
            {
                return false;
            }

            //当前组的第一个射击序号大于等于第一个附加局标志
            if ((((gNum - 1) * ShootNumOfEachGroup) + 1) >= GetAdditionFirstFlag())
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// 获取当前总积分
        /// </summary>
        /// <returns></returns>
        public string GetNowIntegralString()
        {
            if (IncludeIntegral == true && Data.Count != 0)
            {
                return Integral.Sum().ToString();
            }
            else
            {
                return "";
            }
        }

        public int GetNowTotalIntegral()
        {
            return Integral.Sum();
        }

        /// <summary>
        /// 获取全部射击总分
        /// </summary>
        /// <returns></returns>
        public string GetTotalScoreString()
        {
            if (Data.Count > 0)
            {
                return Data.Sum(x => x.Val).ToString();
            }
            else
            {
                return "";
            }
        }

        public int GetNowTotalScore()
        {
            if (Data.Count > 0)
            {
                return Data.Sum(x => x.Val);
            }
            else
            {
                return 0;
            }
        }


        /// <summary>
        /// 设置是否包括积分
        /// </summary>
        /// <param name="val"></param>
        public void SetIncludeIntegral(bool val)
        {
            IncludeIntegral = val;
        }

        public bool GetIncludeIntegral()
        {
            return IncludeIntegral;
        }

        /// <summary>
        /// 设置每局射击次数
        /// </summary>
        /// <param name="num"></param>
        public void SetShootNumOfAGroup(int num)
        {
            ShootNumOfEachGroup = num;
        }

        public int  GetShootNumOfAGroup()
        {
            return ShootNumOfEachGroup;
        }

        /// <summary>
        /// 清空积分数组
        /// </summary>
        public void ClearAllIntegral()
        {
            Integral.Clear();
        }

        

        public Error SetIntegralNew(int shootNum, int integral)
        {
            if (shootNum > MAX_SHOOT_NUM)   //review1007 这里不应该是最大射击数量
            {
                return Error.ErrFailed;
            }
            if (shootNum > Integral.Count)
            {
                Integral.Add(integral);
            }
            else
            {
                Integral[shootNum - 1] = integral;
            }
            return Error.ErrSuccess;
        }

        

        /// <summary>
        /// 分数格式化函数
        /// </summary>
        /// <param name="score">分数值</param>
        /// <returns></returns>
        private string ScoreTrim(int score)
        {
            if (score < 10)
            {
                return " " + score.ToString() + " ";
            }
            else
            {
                return score.ToString();
            }
        }

        /// <summary>
        /// 输出分数显示信息
        /// </summary>
        /// <returns>分数信息文字内容</returns>
        [Obsolete]
        public string ShowScore()
        {
            int gNum = Data.Count / ShootNumOfEachGroup;
            int last = Data.Count % ShootNumOfEachGroup;
            string content = "";

            /*需要显示补射局的积分*/
            
            if (last > 0)
            {
                gNum++;
            }
            


            int i = 0;
            int j = 0;
            for (i = 0; i < gNum; i++)
            {
                int totalTmp = 0;   //当前组总分
                if (i > 0)
                {
                    content += "\n";
                }
                content += "第" + ScoreTrim(i + 1) + "组: ";

                int maxJ = ShootNumOfEachGroup;
                /*判断是否为最后一组*/
                if (i == gNum - 1)
                {
                    maxJ = last;
                }
                if (last == 0)
                {
                    maxJ = ShootNumOfEachGroup;
                }

                for (j = 0; j < maxJ; j++)
                {
                    totalTmp += Data[i * ShootNumOfEachGroup + j].Val;
                    content += ScoreTrim(Data[i * ShootNumOfEachGroup + j].Val);
                    if (j + 1 < ShootNumOfEachGroup)
                    {
                        content += ",";
                    }
                }
                if (IncludeIntegral == true)    //是否显示积分
                {
                    if (Integral.Count > i)
                        content += "   积分: " + Integral[i];
                    else
                        content += "   积分: 0";
                }
                else
                {
                    content += "   ";
                }
                content += "   本组总环: " + totalTmp;
            }

/*
            //余数为0 直接退出
            if (last == 0)
            {
                return content;
            }

            if (i > 0)
            {
                content += "\n";
            }
            content += "第" + ScoreTrim(i + 1) + "组: ";

            for (j = 0; j < last; j++)
            {
                //totalTmp += Data[i * ShootNumOfAGroup + j];
                content += ScoreTrim(Data[i * ShootNumOfEachGroup + j].Val);
                if (j + 1 < last)
                {
                    content += ",";
                }
            }
*/
            return content;
        }

        public string ShowScoreNew()
        {
            int gNum = Data.Count / ShootNumOfEachGroup;
            int last = Data.Count % ShootNumOfEachGroup;
            string content = "";

            /*需要显示补射局的积分*/
            if (last > 0)
            {
                gNum++;     //最后一组
            }

            int i = 0;
            int j = 0;
            for (i = 0; i < gNum; i++)
            {
                int totalTmp = 0;   //当前组总分
                int integralTmp = 0;    //当前组积分

                if (i > 0)
                {
                    content += "\n";
                }

                //如果为补射局，则对应显示补射局
                if(IsAdditionGroup(i+1))
                {
                    content += "补 射 局: ";
                }
                else
                {
                    content += "第" + ScoreTrim(i + 1) + "组: ";
                }

                int maxJ = ShootNumOfEachGroup;
                /*判断是否为最后一组*/
                if (i == gNum - 1)
                {
                    maxJ = last;
                }
                if (last == 0)
                {
                    maxJ = ShootNumOfEachGroup;
                }

                for (j = 0; j < maxJ; j++)
                {
                    totalTmp += Data[i * ShootNumOfEachGroup + j].Val;      //统计当前组的积分和总分
                    integralTmp += Integral[i * ShootNumOfEachGroup + j];
                    content += ScoreTrim(Data[i * ShootNumOfEachGroup + j].Val);
                    if (j + 1 < ShootNumOfEachGroup)
                    {
                        content += ",";
                    }
                }

                if (IncludeIntegral == true)    //是否显示积分
                {
                    content += "   积分: " + integralTmp;
                }
                else
                {
                    content += "   ";
                }
                content += "   本组总环: " + totalTmp;
            }
            return content;
        }

        /// <summary>
        /// 将比赛内容保存到xml文档中
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="parentNode"></param>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public Error AddScoreToXmlDoc(XmlDocument xmlDoc, XmlNode parentNode, string playerName)
        {
            try
            {
                XmlNode scoreNode = xmlDoc.CreateNode(XmlNodeType.Element, playerName, null);

                //录入分数
                XmlPublicMethod xm = new XmlPublicMethod();
                xm.CreateNode(xmlDoc, scoreNode, "ShootNumOfEachGroup", ShootNumOfEachGroup.ToString());
                xm.CreateNode(xmlDoc, scoreNode, "IncludeIntegral", IncludeIntegral.ToString());

                //Data 不是数组，也需要在类中添加
                string xml = XmlUtil.Serializer(typeof(List<ScoreData>), Data);
                xm.CreateNode(xmlDoc, scoreNode, "Data", xml);

                //Integral
                xml = XmlUtil.Serializer(typeof(List<int>), Integral);
                xm.CreateNode(xmlDoc, scoreNode, "Integral", xml);

                //History
                Stack<Record> h2Tmp = new Stack<Record>();
                foreach (Record tmp in History)
                {
                    h2Tmp.Push(tmp);
                }
                XmlNode historyNode = xmlDoc.CreateElement("History");
                int idx = 0;
                foreach (Record tmp in h2Tmp)
                {
                    if(tmp.AddToXmlDoc(xmlDoc, historyNode, "step", idx++) != Error.ErrSuccess)
                    {
                        return Error.ErrFailed;
                    }
                }

                scoreNode.AppendChild(historyNode);
                
                parentNode.AppendChild(scoreNode);
            }
            catch
            {
                return Error.ErrFailed;
            }
            return Error.ErrSuccess;
        }

        /// <summary>
        /// 将文本内容转换为内存结构体
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        public Error SetByString(XmlNode parentNode)
        {
            try
            {
                XmlNode root = parentNode.SelectSingleNode("ShootNumOfEachGroup");
                if (int.TryParse(root.InnerText, out ShootNumOfEachGroup) != true)
                {
                    return Error.ErrFailed;
                }

                root = parentNode.SelectSingleNode("IncludeIntegral");
                if (root.InnerText == "true")
                {
                    IncludeIntegral = true;
                }
                else
                {
                    IncludeIntegral = false;
                }

                root = parentNode.SelectSingleNode("Data");
                List<ScoreData> newData = XmlUtil.Deserialize(typeof(List<ScoreData>), root.InnerText) as List<ScoreData>;
                Data = newData;


                root = parentNode.SelectSingleNode("Integral");
                List<int>  newIntegral = XmlUtil.Deserialize(typeof(List<int>), root.InnerText) as List<int>;
                Integral = newIntegral;

                XmlNodeList nodeList = parentNode.SelectSingleNode("History").ChildNodes;
                //清空stack
                History.Clear();

                foreach (XmlNode xn in nodeList)
                {
                    //XmlElement xe = (XmlElement)xn; //将子节点转换为xmlelement类型
                    Record rc = new Record();
                    if(rc.SetByString(xn) != Error.ErrSuccess)
                    {
                        return Error.ErrFailed;
                    }

                    History.Push(rc);
                }

                
            }
            catch
            {
                return Error.ErrFailed;
            }
            return Error.ErrSuccess;
        }

    }

    /// <summary>
    /// 比赛类
    /// </summary>
    public static class Game
    {
        /// <summary>
        /// 比赛类型
        /// </summary>
        public enum GameType
        {
            IndividualIntegral = 0,     //个人积分
            IndividualTotScore,         //个人总分制
            TeamIntegral,       //团体积分制
            TeamTotScore,       //团体总分制
            TeamMixIntegral,    //团体混合积分制
            TeamMixTotScore,    //团体混合总分制
        }

        public enum GamePlayer
        {
            None = 0,
            PlayerA,
            PlayerB,
        }

        public static Score PlayerA = new Score();    //团队/选手A
        public static Score PlayerB = new Score();    //团队/选手B
        public static GameType Type = GameType.IndividualIntegral;  //默认为个人积分制 //review 需要保存到文件中
        private static Stack<GamePlayer> History = new Stack<GamePlayer>();    //分数记录栈
        private static string teamNameA = "";     //队名称
        private static string teamNameB = "";
        private static string playerNameA = "";     //运动员
        private static string playerNameB = "";

        private static GamePlayer FirstPlayer = GamePlayer.None;    //第一局裁判选定选手 //review 以下两组新增内容需要保存到文件中
        private static GamePlayer AdditionGroupFirstPlayer = GamePlayer.None;   //补射局选定首先发射的选手


        public static string openFilePath = "";     //打开的文件路径


        public static int Init()
        {
            return 0;
        }

        public static int SetGameType()
        {
            return 0;
        }

        public static GameType GetGameType()
        {
            if(PlayerA.GetShootNumOfAGroup() == 3)
            {
                if(PlayerA.GetIncludeIntegral() == true)
                {
                    return GameType.IndividualIntegral;
                }
                else
                {
                    return GameType.IndividualTotScore;
                }
            }
            else if (PlayerA.GetShootNumOfAGroup() == 6)
            {
                if (PlayerA.GetIncludeIntegral() == true)
                {
                    return GameType.TeamIntegral;
                }
                else
                {
                    return GameType.TeamTotScore;
                }
            }
            else
            {
                if (PlayerA.GetIncludeIntegral() == true)
                {
                    return GameType.TeamMixIntegral;
                }
                else
                {
                    return GameType.TeamMixTotScore;
                }
            }
            
        }

        public static void SetTeamNameA(string name)
        {
            teamNameA = name;
        }

        public static void SetTeamNameB(string name)
        {
            teamNameB = name;
        }

        public static void SetPlayerNameA(string name)
        {
            playerNameA = name;
        }

        public static void SetPlayerNameB(string name)
        {
            playerNameB = name;
        }

        public static string GetTeamNameA()
        {
            return teamNameA ;
        }

        public static string GetTeamNameB()
        {
            return teamNameB ;
        }

        public static string GetPlayerNameA()
        {
            return playerNameA ;
        }

        public static string GetPlayerNameB()
        {
            return playerNameB;
        }

        /// <summary>
        /// 添加分数
        /// </summary>
        /// <param name="player">团队/选手</param>
        /// <param name="score">分数</param>
        /// <param name="uncertain">争议分标记</param>
        /// <returns></returns>
        public static Error AddScore(GamePlayer player, int score, bool uncertain, bool addition)
        {
            /*记录第一支箭射击选手*/
            if (Game.PlayerA.GetTotShootNum() == 0 &&
                Game.PlayerB.GetTotShootNum() == 0)
            {
                FirstPlayer = player;
            }

            /*记录第一支附加局射击选手*/
            if(addition && PlayerA.GetAdditionFirstFlag() == 0 &&
                PlayerB.GetAdditionFirstFlag() == 0)
            {
                AdditionGroupFirstPlayer = player;
            }

            //添加分数，注意同时要入栈运动员
            if (player == GamePlayer.PlayerA)
            {
                /*添加分数*/
                Game.PlayerA.AddAScore(new Score.ScoreData(score, uncertain, addition));  //判断一下是否添加成功
                History.Push(GamePlayer.PlayerA);
            }
            else if (player == GamePlayer.PlayerB)
            {
                /*添加分数*/
                Game.PlayerB.AddAScore(new Score.ScoreData(score, uncertain, addition));
                History.Push(GamePlayer.PlayerB);
            }
            else
            {
                return Error.ErrFailed;
            }

            //添加成功重新计算积分
            CalcIntegralNew();

            return Error.ErrSuccess;
        }

        /// <summary>
        /// 修改分数
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gNum"></param>
        /// <param name="shootNum"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public static Error ChangeScore(GamePlayer player, int gNum, int shootNum, int score)
        {
            Error result = Error.ErrSuccess;
            //修改分数，注意同时要入栈运动员
            if (player == GamePlayer.PlayerA)
            {
                result = PlayerA.ChangeScore(gNum, shootNum, new Score.ScoreData(score));  //判断一下是否添加成功

                History.Push(GamePlayer.PlayerA);
            }
            else if (player == GamePlayer.PlayerB)
            {
                result = PlayerB.ChangeScore(gNum, shootNum, new Score.ScoreData(score));

                History.Push(GamePlayer.PlayerB);
            }
            else
            {
                return Error.ErrFailed;
            }

            //添加成功重新计算积分
            CalcIntegralNew();

            return Error.ErrSuccess;
        }


        /// <summary>
        /// 删除分数
        /// </summary>
        /// <param name="player"></param>
        /// <param name="gNum"></param>
        /// <param name="shootNum"></param>
        /// <returns></returns>
        public static Error DeleteScore(GamePlayer player, int gNum, int shootNum)
        {
            //删除分数，注意同时要入栈运动员
            if (player == GamePlayer.PlayerA)
            {
                PlayerA.DeleteScore(gNum, shootNum);  //判断一下是否添加成功

                History.Push(GamePlayer.PlayerA);
            }
            else if (player == GamePlayer.PlayerB)
            {
                PlayerB.DeleteScore(gNum, shootNum);

                History.Push(GamePlayer.PlayerB);
            }
            else
            {
                return Error.ErrFailed;
            }

            //添加成功重新计算积分
            CalcIntegralNew();

            return Error.ErrSuccess;
        }

        /// <summary>
        /// 回退一次分数
        /// </summary>
        /// <returns></returns>
        public static Error BackOneScore()
        {
            //从堆栈获取数据
            GamePlayer tmp = GamePlayer.None;
            if (History.Count == 0)
            {
                return Error.ErrFailed;
            }

            tmp = History.Pop();

            if (tmp == GamePlayer.PlayerA)
            {
                PlayerA.BackOneRecord();
            }
            else if (tmp == GamePlayer.PlayerB)
            {
                PlayerB.BackOneRecord();
            }

            //重新计算积分
            CalcIntegralNew();

            return Error.ErrSuccess;
        }


        /// <summary>
        /// 清空所有分数
        /// </summary>
        /// <returns></returns>
        public static Error ClearAll()
        {
            teamNameA = "";     //队名称
            teamNameB = "";
            playerNameA = "";     //运动员
            playerNameB = "";

            PlayerA.ClearAll();
            PlayerB.ClearAll();

            History.Clear();    //清空堆栈
            return Error.ErrSuccess;
        }

        
        /// <summary>
        /// 判断是否为本组最后一次射击
        /// </summary>
        /// <param name="shootNo"></param>
        /// <param name="firstFlagAdd"></param>
        /// <returns></returns>
        private static bool IsTheEndOfGroup(int shootNo,int firstFlagAdd)
        {
            int tmpNormalShootNumToCalc = 3;   //默认个人淘汰赛
            int tmpAdditionShootNumToCalc = 1;

            switch (GetGameType())
            {
                case GameType.IndividualIntegral:
                case GameType.IndividualTotScore:
                    tmpNormalShootNumToCalc = 3;
                    tmpAdditionShootNumToCalc = 1;
                    break;
                case GameType.TeamIntegral:
                case GameType.TeamTotScore:
                    tmpNormalShootNumToCalc = 6;
                    tmpAdditionShootNumToCalc = 3;
                    break;
                case GameType.TeamMixIntegral:
                case GameType.TeamMixTotScore:
                    tmpNormalShootNumToCalc = 4;
                    tmpAdditionShootNumToCalc = 2;
                    break;
            }

            /*判断当前是否为补射局*/
            if (firstFlagAdd != 0 && shootNo >= firstFlagAdd)
            {
                /*当前的射击为补射局*/
                if ((shootNo + 1 - firstFlagAdd) > 0 && (shootNo + 1 - firstFlagAdd) % tmpAdditionShootNumToCalc == 0)
                {
                    return true;
                }

            }
            else
            {
                /*普通射击局*/
                if (shootNo > 0 && shootNo % tmpNormalShootNumToCalc == 0)
                {
                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// 计算比赛积分
        /// </summary>
        /// <returns></returns>
        private static Error CalcIntegralNew()
        {
            //清空积分
            PlayerA.ClearAllIntegral();
            PlayerB.ClearAllIntegral();

            //循环所有分数，射击次数取最大值，以保证分数对应有积分值
            int tmpATotalGroupScore = 0;
            int tmpBTotalGroupScore = 0;
            int tmpShootNumTotal = PlayerA.GetTotShootNum() > PlayerB.GetTotShootNum() ? PlayerA.GetTotShootNum() : PlayerB.GetTotShootNum();
            int tmpFirstAdditionShootNo = PlayerA.GetAdditionFirstFlag() > PlayerB.GetAdditionFirstFlag() ? PlayerB.GetAdditionFirstFlag() : PlayerA.GetAdditionFirstFlag();
            for (int i = 1; i <= tmpShootNumTotal; i++)
            {
                /*根据普通局计算计分的射击数和附加局数来计算积分
                  积分数量同分数数量一致
                */
                try
                {
                    tmpATotalGroupScore += PlayerA.GetScore(i).Val;
                    tmpBTotalGroupScore += PlayerB.GetScore(i).Val;
                }
                catch
                {
                    PlayerA.SetIntegralNew(i, 0);
                    PlayerB.SetIntegralNew(i, 0);
                    continue;
                }
                

                /*判断当前是否为当前组最后一次射击*/
                if (IsTheEndOfGroup(i,tmpFirstAdditionShootNo))
                {
                    int great = 2;
                    int equ = 1;
                    int less = 0;

                    if (tmpFirstAdditionShootNo!= 0 && i >= tmpFirstAdditionShootNo)
                    {
                        //补射局
                        great = 1;
                        equ = 0;
                        less = 0;
                    }

                    //比较大小，写入积分
                    if (tmpATotalGroupScore > tmpBTotalGroupScore)
                    {
                        PlayerA.SetIntegralNew(i, great);
                        PlayerB.SetIntegralNew(i, less);
                    }
                    else if (tmpATotalGroupScore == tmpBTotalGroupScore)
                    {
                        PlayerA.SetIntegralNew(i, equ);
                        PlayerB.SetIntegralNew(i, equ);
                    }
                    else
                    {
                        PlayerA.SetIntegralNew(i, less);
                        PlayerB.SetIntegralNew(i, great);
                    }

                    //清零
                    tmpATotalGroupScore = 0;
                    tmpBTotalGroupScore = 0;
                }
                else
                {
                    PlayerA.SetIntegralNew(i, 0);
                    PlayerB.SetIntegralNew(i, 0);
                }

            }

            return Error.ErrSuccess;
        }

        public static GamePlayer GetNextShootPlayerNew()
        {
            int tmpNormalInterval = 1;          //普通局连续射击次数
            int tmpAdditionInterval = 1;        //附加局连续射击次数
            int nowInterval = 1;
            bool nowIsAdditionGroup = false;    //附加局标记



            //like  |  1st group  |  2nd group | addtion  |
            //      | A B A B A B | B A B A B A| C C' C C'|
            // A为第一次射击选择的组，C为附加局第一次射击选择的组
            // 第二局的B为根据积分计算出来的


            switch (GetGameType())
            {
                case GameType.IndividualIntegral:
                case GameType.IndividualTotScore:
                    tmpNormalInterval = 1;
                    tmpAdditionInterval = 1;
                    break;
                case GameType.TeamIntegral:
                case GameType.TeamTotScore:
                    tmpNormalInterval = 3;
                    tmpAdditionInterval = 1;
                    break;
                case GameType.TeamMixIntegral:
                case GameType.TeamMixTotScore:
                    tmpNormalInterval = 2;
                    tmpAdditionInterval = 1;
                    break;
            }


            /*1.双方都为0，则默认返回A*/
            if (0 == PlayerA.GetTotShootNum() &&
                PlayerA.GetTotShootNum() == PlayerB.GetTotShootNum())
            {
                return GamePlayer.PlayerA;
            }

            /*2.计算当前应当使用的射击间隔*/
            if (PlayerA.GetAdditionFirstFlag() == 0 &&
                PlayerB.GetAdditionFirstFlag() == 0)
            {
                //无附加局标记
                nowInterval = tmpNormalInterval;
            }
            else if (PlayerB.GetAdditionFirstFlag() == 0)
            {
                return GamePlayer.PlayerB;
            }
            else if (PlayerA.GetAdditionFirstFlag() == 0)
            {
                return GamePlayer.PlayerA;
            }
            else
            {
                //都不等于0，此时为附加局
                nowInterval = tmpAdditionInterval;

                nowIsAdditionGroup = true;
            }

            /*3.计算两队的差，根据间隔计算射击方*/
            int difference = PlayerA.GetTotShootNum() - PlayerB.GetTotShootNum();
            difference = difference >= 0 ? difference : -1 * difference;
            if (difference >= nowInterval)
            {
                //超出范围，返回射击次数少的一方
                if (PlayerA.GetTotShootNum() > PlayerB.GetTotShootNum())
                {
                    return GamePlayer.PlayerB;
                }
                else
                {
                    return GamePlayer.PlayerA;
                }
            }
            else if(difference < nowInterval && difference > 0)
            {
                //在间隔范围内，返回正在射击一方
                //由于补射局间隔为1，所以补射局不会出现这种情况

                if (PlayerA.GetTotShootNum() > PlayerB.GetTotShootNum())
                {
                    if (PlayerA.GetTotShootNum() % nowInterval == 0)
                    {
                        return GamePlayer.PlayerB;
                    }
                    else
                    {
                        return GamePlayer.PlayerA;
                    }
                }
                else
                {
                    if (PlayerB.GetTotShootNum() % nowInterval == 0)
                    {
                        return GamePlayer.PlayerA;
                    }
                    else
                    {
                        return GamePlayer.PlayerB;
                    }
                }
            }
            else
            {
                //双方射击次数相同，需要根据规则计算
                /*
                 * 分为三种情况：
                 * 1.普通局，不是最后一个；
                 * 2.普通局，最后一个；
                 * 3.附加局，最后一个（附加局间隔为1）；
                 */

                GamePlayer tagPlayer;       //当前组被标记的第一个人

                if (nowIsAdditionGroup)
                {
                    //附加局
                    tagPlayer = AdditionGroupFirstPlayer;
                }
                else
                {
                    //非附加局
                    tagPlayer = FirstPlayer;

                }

                
                if(GetGameType() == GameType.IndividualTotScore || GetGameType() == GameType.TeamTotScore ||
                    GetGameType() == GameType.TeamMixTotScore)
                {
                    //总分制根据总分情况自动选择下一次射击选手
                    if (PlayerA.GetNowTotalScore() > PlayerB.GetNowTotalScore())
                    {
                        //除第一局裁判指定外，每一局都是积分低的优先射击
                        return GamePlayer.PlayerB;
                    }
                    else if (PlayerA.GetNowTotalScore() < PlayerB.GetNowTotalScore())
                    {
                        return GamePlayer.PlayerA;
                    }
                    else
                    {
                        //相等的情况下由指定的第一个队发射
                        return tagPlayer;
                    }

                }
                else
                {
                    if (PlayerA.GetNowTotalIntegral() > PlayerB.GetNowTotalIntegral())
                    {
                        //除第一局裁判指定外，每一局都是积分低的优先射击
                        return GamePlayer.PlayerB;
                    }
                    else if (PlayerA.GetNowTotalIntegral() < PlayerB.GetNowTotalIntegral())
                    {
                        return GamePlayer.PlayerA;
                    }
                    else
                    {
                        //相等的情况下由指定的第一个队发射
                        return tagPlayer;
                    }
                }
                

            }
            
        }

        /// <summary>
        /// 获取比赛结果
        /// </summary>
        /// <returns></returns>
        public static GamePlayer GetResultOfGame()
        {
            return GamePlayer.None;
        }


        //比赛文件相关
        
        public static Error SaveToFile(string filePath)
        {
            //string xml = XmlUtil.Serializer(typeof(Game), Game);
            try
            {
                //创建xmldoc
                XmlDocument xmldoc = new XmlDocument();
                XmlNode node = xmldoc.CreateXmlDeclaration("1.0", "utf-8", "");
                xmldoc.AppendChild(node);

                XmlNode root = xmldoc.CreateElement("Game");
                xmldoc.AppendChild(root);


                /* 中间处理任务 */
                XmlPublicMethod xm = new XmlPublicMethod();
                //Type
                xm.CreateNode(xmldoc, root, "Type", Type.ToString());
                //playerNameA
                xm.CreateNode(xmldoc, root, "playerNameA", playerNameA);
                //playerNameB
                xm.CreateNode(xmldoc, root, "playerNameB", playerNameB);
                //teamNameA
                xm.CreateNode(xmldoc, root, "teamNameA", teamNameA);
                //teamNameB
                xm.CreateNode(xmldoc, root, "teamNameB", teamNameB);

                //FirstPlayer
                xm.CreateNode(xmldoc, root, "FirstPlayer", FirstPlayer.ToString());
                xm.CreateNode(xmldoc, root, "AdditionGroupFirstPlayer", AdditionGroupFirstPlayer.ToString());


                //History
                Stack<GamePlayer> h2Tmp = new Stack<GamePlayer>();
                foreach (GamePlayer tmp in History)
                {
                    h2Tmp.Push(tmp);
                }
                XmlNode historyNode = xmldoc.CreateElement("History");
                int idx = 0;
                foreach (GamePlayer tmp in h2Tmp)
                {
                    xm.CreateNode(xmldoc, historyNode, "step", tmp.ToString(),idx++);
                }
                root.AppendChild(historyNode);


                //playerA
                if ( Error.ErrSuccess != PlayerA.AddScoreToXmlDoc(xmldoc, root, "PlayerA"))
                {
                    return Error.ErrFailed;
                }

                //playerB
                if (Error.ErrSuccess != PlayerB.AddScoreToXmlDoc(xmldoc, root, "PlayerB"))
                {
                    return Error.ErrFailed;
                }


                //保存文件到指定路径
                xmldoc.Save(filePath);
            }
            catch
            {
                return Error.ErrFailed;
            }

            return Error.ErrSuccess;
        }

        public static Error OpenFromFile(string filePath)
        {
            XmlDocument xmldoc = new XmlDocument();
            try
            {

                xmldoc.Load(filePath);
            }
            catch
            {
                return Error.ErrFailed;
            }

            try
            { 

                XmlNode root = xmldoc.SelectSingleNode("Game");


                XmlNode eleNode = root.SelectSingleNode("Type");
                if (eleNode.InnerText == "IndividualIntegral")
                {
                    Type = GameType.IndividualIntegral;
                }
                else if (eleNode.InnerText == "IndividualTotScore")
                {
                    Type = GameType.IndividualTotScore;
                }
                else if (eleNode.InnerText == "TeamIntegral")
                {
                    Type = GameType.TeamIntegral;
                }
                else if (eleNode.InnerText == "TeamMixIntegral")
                {
                    Type = GameType.TeamMixIntegral;
                }
                else if (eleNode.InnerText == "TeamMixTotScore")
                {
                    Type = GameType.TeamMixTotScore;
                }
                else
                {
                    return Error.ErrFailed;
                }

                //注意字符串复制一份新的
                eleNode = root.SelectSingleNode("playerNameA");
                playerNameA = string.Copy(eleNode.InnerText);

                eleNode = root.SelectSingleNode("playerNameB");
                playerNameB = string.Copy(eleNode.InnerText);

                eleNode = root.SelectSingleNode("teamNameA");
                teamNameA = string.Copy(eleNode.InnerText);

                eleNode = root.SelectSingleNode("teamNameB");
                teamNameB = string.Copy(eleNode.InnerText);

                //FirstPlayer
                eleNode = root.SelectSingleNode("FirstPlayer");
                if (eleNode.InnerText == "PlayerA")
                {
                    FirstPlayer = GamePlayer.PlayerA;
                }
                else if (eleNode.InnerText == "PlayerB")
                {
                    FirstPlayer = GamePlayer.PlayerB;
                }
                else
                {
                    FirstPlayer = GamePlayer.None;
                }

                //AdditionGroupFirstPlayer
                eleNode = root.SelectSingleNode("AdditionGroupFirstPlayer");
                if (eleNode.InnerText == "PlayerA")
                {
                    AdditionGroupFirstPlayer = GamePlayer.PlayerA;
                }
                else if (eleNode.InnerText == "PlayerB")
                {
                    AdditionGroupFirstPlayer = GamePlayer.PlayerB;
                }
                else
                {
                    AdditionGroupFirstPlayer = GamePlayer.None;
                }



                //History
                XmlNodeList nodeList = root.SelectSingleNode("History").ChildNodes;
                History.Clear();
                foreach (XmlNode xn in nodeList)
                {
                    XmlElement xe = (XmlElement)xn; //将子节点转换为xmlelement类型
                    if (xe.InnerText == "PlayerA")
                    {
                        History.Push(GamePlayer.PlayerA);
                    }
                    else if (xe.InnerText == "PlayerB")
                    {
                        History.Push(GamePlayer.PlayerB);
                    }
                    else
                    {
                        return Error.ErrFailed;
                    }
                }


                eleNode = root.SelectSingleNode("PlayerA");
                if(PlayerA.SetByString(eleNode) != Error.ErrSuccess)
                {
                    return Error.ErrFailed;
                }

                eleNode = root.SelectSingleNode("PlayerB");
                if (PlayerB.SetByString(eleNode) != Error.ErrSuccess)
                {
                    return Error.ErrFailed;
                }

            }
            catch
            {
                return Error.ErrFailed;
            }
            return Error.ErrSuccess;
        }

    }

    /// <summary>
    /// xml 公共方法
    /// </summary>
    public class XmlPublicMethod
    {
        public Error CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            try
            {
                XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
                node.InnerText = value;
                parentNode.AppendChild(node);
            }
            catch
            {
                return Error.ErrFailed;
            }
            return Error.ErrSuccess;
        }

        

        public Error CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value,int attributeId)
        {
            try
            {
                XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
                node.InnerText = value;
                XmlElement xe = (XmlElement)node;
                xe.SetAttribute("id", attributeId.ToString());
                parentNode.AppendChild(node);
            }
            catch
            {
                return Error.ErrFailed;
            }
            return Error.ErrSuccess;
        }


    }

    /// <summary>
    /// Xml序列化与反序列化
    /// </summary>
    public class XmlUtil
    {
#region 反序列化
        /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="type">类型</param>
            /// <param name="xml">XML字符串</param>
            /// <returns></returns>
        public static object Deserialize(Type type, string xml)
        {
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(type);
                    return xmldes.Deserialize(sr);
                }
            }
            catch (Exception e)
            {

                return null;
            }
        }
        /// <summary>
            /// 反序列化
            /// </summary>
            /// <param name="type"></param>
            /// <param name="xml"></param>
            /// <returns></returns>
        public static object Deserialize(Type type, Stream stream)
        {
            XmlSerializer xmldes = new XmlSerializer(type);
            return xmldes.Deserialize(stream);
        }
#endregion

#region 序列化
        /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="type">类型</param>
            /// <param name="obj">对象</param>
            /// <returns></returns>
        public static string Serializer(Type type, object obj)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(type);
            try
            {
                //序列化对象
                xml.Serialize(Stream, obj);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();

            return str;
        }

#endregion
    }
}
