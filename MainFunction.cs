using HKFaceSearch.Entity;
using HKFaceSearch.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using HKFaceSearch.sdk;

namespace HKFaceSearch
{
    public partial class MainForm
    {
        private CHCNetSDK.LOGINRESULTCALLBACK LoginCallBack = null;
        private CHCNetSDK.NET_DVR_DEVICEINFO_V40 DeviceInfo;

        private CHCNetSDK.EXCEPYIONCALLBACK m_fExceptionCB = null;
        private CHCNetSDK.MSGCallBack_V31 m_falarmData_V31 = null;

        /// <summary>
        /// 设备登录
        /// </summary>
        /// <param name="loginUserID"></param>
        /// <param name="ip"></param>
        /// <param name="name"></param>
        /// <param name="pwd"></param>
        /// <param name="port"></param>
        private void HKDeviceLogin(ref Int32 loginUserID, string ip, string name, string pwd, string port)
        {
            try
            {
                if (loginUserID < 0)
                {
                    CHCNetSDK.NET_DVR_USER_LOGIN_INFO struGateLogInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO();

                    //设备IP地址或者域名
                    byte[] byIP = Encoding.Default.GetBytes(ip);
                    struGateLogInfo.sDeviceAddress = new byte[129];
                    byIP.CopyTo(struGateLogInfo.sDeviceAddress, 0);

                    //设备用户名
                    byte[] byUserName = Encoding.Default.GetBytes(name);
                    struGateLogInfo.sUserName = new byte[64];
                    byUserName.CopyTo(struGateLogInfo.sUserName, 0);

                    //设备密码
                    byte[] byPassword = Encoding.Default.GetBytes(pwd);
                    struGateLogInfo.sPassword = new byte[64];
                    byPassword.CopyTo(struGateLogInfo.sPassword, 0);

                    struGateLogInfo.wPort = ushort.Parse(port);//设备服务端口号

                    if (LoginCallBack == null)
                    {
                        LoginCallBack = new CHCNetSDK.LOGINRESULTCALLBACK(CBLoginCallBack);//注册回调函数                    
                    }
                    struGateLogInfo.cbLoginResult = LoginCallBack;
                    struGateLogInfo.bUseAsynLogin = false; //是否异步登录：0- 否，1- 是 

                    DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();
                    //登录设备 Login the device
                    loginUserID = CHCNetSDK.NET_DVR_Login_V40(ref struGateLogInfo, ref DeviceInfo);
                    if (loginUserID < 0)
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        MessageBox.Show(ip + " NET_DVR_Login_V40 failed, error code= " + iLastErr);
                        return;
                    }
                    else
                    {
                        //登录成功
                    }
                }
                else
                {
                    
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 登录回调函数
        /// </summary>
        /// <param name="lUserID"></param>
        /// <param name="dwResult"></param>
        /// <param name="lpDeviceInfo"></param>
        /// <param name="pUser"></param>
        private void CBLoginCallBack(int lUserID, int dwResult, IntPtr lpDeviceInfo, IntPtr pUser)
        {
            string strLoginCallBack = "登录设备，lUserID：" + lUserID + "，dwResult：" + dwResult;

            if (dwResult == 0)
            {
                uint iErrCode = CHCNetSDK.NET_DVR_GetLastError();
                strLoginCallBack = strLoginCallBack + "，错误号:" + iErrCode;
                MessageBox.Show(strLoginCallBack);
            }
            else
            {
                
            }
        }

        /// <summary>
        /// 布防初始化
        /// </summary>
        private void AlarmLoad()
        {
            CHCNetSDK.NET_DVR_LOCAL_GENERAL_CFG struLocalCfg = new CHCNetSDK.NET_DVR_LOCAL_GENERAL_CFG();
            struLocalCfg.byAlarmJsonPictureSeparate = 1;//控制JSON透传报警数据和图片是否分离，0-不分离(COMM_VCA_ALARM返回)，1-分离（分离后走COMM_ISAPI_ALARM回调返回）

            int nSize = Marshal.SizeOf(struLocalCfg);
            IntPtr ptrLocalCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(struLocalCfg, ptrLocalCfg, false);

            if (!CHCNetSDK.NET_DVR_SetSDKLocalCfg(17, ptrLocalCfg))  //NET_DVR_LOCAL_CFG_TYPE_GENERAL
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                MessageBox.Show("NET_DVR_SetSDKLocalCfg failed, error code= " + iLastErr);
            }
            Marshal.FreeHGlobal(ptrLocalCfg);

            //设置异常消息回调函数
            if (m_fExceptionCB == null)
            {
                m_fExceptionCB = new CHCNetSDK.EXCEPYIONCALLBACK(CBException);
            }
            CHCNetSDK.NET_DVR_SetExceptionCallBack_V30(0, IntPtr.Zero, m_fExceptionCB, IntPtr.Zero);


            //设置报警回调函数
            if (m_falarmData_V31 == null)
            {
                m_falarmData_V31 = new CHCNetSDK.MSGCallBack_V31(MsgCallback_V31);
            }
            CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(m_falarmData_V31, IntPtr.Zero);
        }

        /// <summary>
        /// 布防异常信息回调函数
        /// </summary>
        /// <param name="dwType"></param>
        /// <param name="lUserID"></param>
        /// <param name="lHandle"></param>
        /// <param name="pUser"></param>
        private void CBException(uint dwType, int lUserID, int lHandle, IntPtr pUser)
        {
            MessageBox.Show("异常消息回调，信息类型：0x" + Convert.ToString(dwType, 16) + ", lUserID:" + lUserID + ", lHandle:" + lHandle);
        }

        /// <summary>
        /// 布防正常信息回调函数
        /// </summary>
        /// <param name="lCommand"></param>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        /// <returns></returns>
        public bool MsgCallback_V31(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            AlarmMessageHandle(lCommand, ref pAlarmer, pAlarmInfo, dwBufLen, pUser);

            return true; //回调函数需要有返回，表示正常接收到数据
        }

        /// <summary>
        /// 通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
        /// </summary>
        /// <param name="lCommand"></param>
        /// <param name="pAlarmer"></param>
        /// <param name="pAlarmInfo"></param>
        /// <param name="dwBufLen"></param>
        /// <param name="pUser"></param>
        public void AlarmMessageHandle(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //这里删除了用不到的报警类型，详细类型请查阅官方文档和demo
            switch (lCommand)
            {
                case CHCNetSDK.COMM_UPLOAD_FACESNAP_RESULT://人脸抓拍结果信息
                    ProcessCommAlarm_FaceSnap(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 布防开始
        /// </summary>
        private void AlarmStart(ref Int32 handle, Int32 userID)
        {
            CHCNetSDK.NET_DVR_SETUPALARM_PARAM struAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();
            struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
            struAlarmParam.byLevel = 1; //0- 一级布防,1- 二级布防
            struAlarmParam.byAlarmInfoType = 1;//智能交通设备有效，新报警信息类型
            struAlarmParam.byFaceAlarmDetection = 1;//1-人脸侦测

            handle = CHCNetSDK.NET_DVR_SetupAlarmChan_V41(userID, ref struAlarmParam);
            if (handle < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                MessageBox.Show("布防失败，错误号：" + iLastErr);
            }
        }

        private void ProcessCommAlarm_FaceSnap(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            try
            {
                //报警设备IP地址
                string strIP = Encoding.UTF8.GetString(pAlarmer.sDeviceIP).TrimEnd('\0');

                CHCNetSDK.NET_VCA_FACESNAP_RESULT struFaceSnapInfo = new CHCNetSDK.NET_VCA_FACESNAP_RESULT();
                CHCNetSDK.NET_VCA_FACESNAP_ADDINFO struFaceSnapAddInfo = new CHCNetSDK.NET_VCA_FACESNAP_ADDINFO();
                uint dwSize = (uint)Marshal.SizeOf(struFaceSnapInfo);
                struFaceSnapInfo = (CHCNetSDK.NET_VCA_FACESNAP_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_VCA_FACESNAP_RESULT));

                //报警时间：年月日时分秒
                string strTimeYear = ((struFaceSnapInfo.dwAbsTime >> 26) + 2000).ToString();
                string strTimeMonth = ((struFaceSnapInfo.dwAbsTime >> 22) & 15).ToString("d2");
                string strTimeDay = ((struFaceSnapInfo.dwAbsTime >> 17) & 31).ToString("d2");
                string strTimeHour = ((struFaceSnapInfo.dwAbsTime >> 12) & 31).ToString("d2");
                string strTimeMinute = ((struFaceSnapInfo.dwAbsTime >> 6) & 63).ToString("d2");
                string strTimeSecond = ((struFaceSnapInfo.dwAbsTime >> 0) & 63).ToString("d2");
                string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;

                struFaceSnapAddInfo = (CHCNetSDK.NET_VCA_FACESNAP_ADDINFO)Marshal.PtrToStructure(struFaceSnapInfo.pAddInfoBuffer, typeof(CHCNetSDK.NET_VCA_FACESNAP_ADDINFO));

                //保存人脸图片数据
                if ((struFaceSnapInfo.dwFacePicLen != 0) && (struFaceSnapInfo.pBuffer1 != IntPtr.Zero))
                {
                    int iLen = (int)struFaceSnapInfo.dwFacePicLen;
                    byte[] by = new byte[iLen];
                    Marshal.Copy(struFaceSnapInfo.pBuffer1, by, 0, iLen);

                    MemoryStream ms = new MemoryStream(by);

                    BeginInvoke(new Action(() =>
                    {
                        Button btn_Face = new Button();
                        btn_Face.Size = new Size(142, 192);
                        btn_Face.Text = (struFaceSnapAddInfo.fFaceTemperature).ToString("f2");
                        btn_Face.ForeColor = struFaceSnapAddInfo.byIsAbnomalTemperature == 1 ? Color.Red : Color.Green;
                        btn_Face.TextAlign = ContentAlignment.TopCenter;
                        btn_Face.Tag = strTime;
                        btn_Face.BackgroundImageLayout = ImageLayout.Stretch;
                        btn_Face.BackgroundImage = Image.FromStream(ms);
                        btn_Face.Click += new EventHandler(Btn_FaceClick);

                        if (flp_RealtimePhotos.Controls.Count == 18)
                        {
                            flp_RealtimePhotos.Controls[0].Dispose();
                            flp_RealtimePhotos.Controls.RemoveAt(0);
                            flp_RealtimePhotos.Controls.Add(btn_Face);
                        }
                        else
                            flp_RealtimePhotos.Controls.Add(btn_Face);

                        ms.Close();
                    }));
                }

                GC.Collect();
            }
            catch (Exception)
            {

            }
        }

        private void Btn_FaceClick(object sender, EventArgs e)
        {
            lbl_XMachineFront_time.Text = "------";
            lbl_XMachineBack_Time.Text = "------";
            pb_SearchVideoFront.Image = null;
            pb_SearchVideoBack.Image = null;

            Button clicked = (Button)sender;
            DateTime pictureTime = DateTime.Parse(clicked.Tag.ToString());
            string modeData;

            pb_SecurityGate.Image = clicked.BackgroundImage;
            lbl_SecurrityGate_Time.Text = pictureTime.ToString("yyyy-MM-dd HH:mm:ss");
            modeData = GetModeDataFromImage(pb_SecurityGate.Image);

            //显示关联照片
            if (!string.IsNullOrEmpty(modeData))
            {
                string timeFront = "", timeBack = "";
                string xml = GetPhotoFromPhotoXMLCommand(pictureTime.AddSeconds(-60*10), pictureTime.AddSeconds(60*10), modeData, Utils.GetRandomString(15));
                Bitmap bmXMachineFront = GetPhotoFromPhoto(xml, "3", lbl_SecurrityGate_Time.Text, ref timeFront);
                Bitmap bmXMachineBack = GetPhotoFromPhoto(xml, "4", lbl_SecurrityGate_Time.Text, ref timeBack);
                pb_XMachineFront.Image = bmXMachineFront;
                if(!string.IsNullOrEmpty(timeFront))lbl_XMachineFront_time.Text = DateTime.Parse(timeFront).ToString("yyyy-MM-dd HH:mm:ss");
                pb_XMachineBack.Image = bmXMachineBack;
                if(!string.IsNullOrEmpty(timeBack)) lbl_XMachineBack_Time.Text = DateTime.Parse(timeBack).ToString("yyyy-MM-dd HH:mm:ss");
            }

            //显示前后摄像头回放视频,如果已在回放，先停止回放
            for(int i = 0; i < m_lPlayBackHandle.Length; i ++)
            {
                if (m_lPlayBackHandle[i] >= 0)
                {
                    if (!CHCNetSDK.NET_DVR_StopPlayBack(m_lPlayBackHandle[i]))
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        MessageBox.Show("NET_DVR_StopPlayBack failed, error code= " + iLastErr);
                        return;
                    }

                    m_lPlayBackHandle[i] = -1;
                }
            }
            //通过是否显示时间来判断是否搜索到关联图片
            if (!lbl_XMachineFront_time.Text.Contains("---"))
            {
                StartPlayBack(35, pb_SearchVideoFront, DateTime.Parse(lbl_XMachineFront_time.Text).AddSeconds(-10), DateTime.Parse(lbl_XMachineFront_time.Text).AddSeconds(5), 0);
                RenderPrivateDataClose(m_lPlayBackHandle[0]);
            }
            if (!lbl_XMachineBack_Time.Text.Contains("---"))
            {
                StartPlayBack(36, pb_SearchVideoBack, DateTime.Parse(lbl_XMachineBack_Time.Text).AddSeconds(-5), DateTime.Parse(lbl_XMachineBack_Time.Text).AddSeconds(10), 1);
                RenderPrivateDataClose(m_lPlayBackHandle[1]);
            }
        }

        private string GetModeDataFromImage(Image image)
        {
            string outXml = XMLCommand("POST /ISAPI/Intelligent/analysisImage/face", Utils.ChangeImageToByteArray(image), true);
            if (!string.IsNullOrEmpty(outXml))
            {
                return Utils.ReadXMLString("FaceContrastTargetsList/FaceContrastTarget", "modeData", "", outXml.Replace(" version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\"",""));
            }
            else
                return "";
        }

        private string GetPhotoFromPhotoXMLCommand(DateTime startTime, DateTime endTime, string modeData, string searchID)
        {
            string requestUrl = "POST /ISAPI/Intelligent/FDLib/FCSearch";
            string inputXML = "<FCSearchDescription version=\"2.0\" xmlns=\"http://www.std-cgi.org/ver20/XMLSchema\">" +
                "<searchID>" + searchID + "</searchID><searchResultPosition>0</searchResultPosition><maxResults>50</maxResults>" +
                "<snapStartTime>" + startTime.ToString("yyyy-MM-ddTHH:mm:ssZ") + "</snapStartTime><snapEndTime>" + endTime.ToString("yyyy-MM-ddTHH:mm:ssZ") + "</snapEndTime><eventType>unlimit</eventType>" +
                "<FaceModeList><FaceMode><ModeInfo><similarity>70</similarity><modeData>" + modeData + "</modeData></ModeInfo></FaceMode></FaceModeList></FCSearchDescription>";

            return XMLCommand(requestUrl, inputXML, false);
        }

        private Bitmap GetPhotoFromPhoto(string xml, string channelID, string baseTime, ref string snapTime)
        {
            Bitmap returnValue = null;

            if (Convert.ToInt32(Utils.ReadXMLString("FCSearchResult", "numOfMatches", "0", xml.Replace(" version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\"", ""))) == 0) MessageBox.Show("没有匹配到照片");
            else
            {
                List<FaceInfo> listFaceInfo = new List<FaceInfo>();
                Utils.ReadXMLString("FCSearchResult/MatchList/MatchElement", listFaceInfo, xml.Replace(" version=\"2.0\" xmlns=\"http://www.isapi.org/ver20/XMLSchema\"", ""));
                FaceInfo.SortFaceInfoBySnapTime(ref listFaceInfo);
                if (listFaceInfo.Count > 0)
                {
                    int count = listFaceInfo.Where(_ => _.channelID.Equals(channelID)).ToList().Count();
                    if (count != 0)
                    {
                        HttpWebRequest request = null;
                        //前摄像头关联最新的照片
                        if (channelID.Equals("3"))
                        {
                            snapTime = listFaceInfo[listFaceInfo.FindIndex(_ => _.channelID.Equals(channelID)) + count - 1].snapTime;
                            request = (HttpWebRequest)WebRequest.Create(listFaceInfo[listFaceInfo.FindIndex(_ => _.channelID.Equals(channelID)) + count - 1].snapPicURL);
                        }
                        //后摄像头关联最旧的照片
                        else
                        {
                            int startIndex = listFaceInfo.FindIndex(_ => _.channelID.Equals(channelID));
                            for (int i = startIndex; i < count + startIndex; i++)
                            {
                                if (DateTime.Parse(listFaceInfo[i].snapTime) >= DateTime.Parse(baseTime))
                                {
                                    snapTime = listFaceInfo[i].snapTime;
                                    request = (HttpWebRequest)WebRequest.Create(listFaceInfo[i].snapPicURL);
                                    break;
                                }
                            }
                        }

                        if (request != null)
                        {
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            if (response.GetResponseStream() != null)
                            {
                                returnValue = new Bitmap(response.GetResponseStream());
                            }
                        }   
                    }
                }
            }

            return returnValue;
        }

        private string XMLCommand(string strRequestUrl, object strInputXParam, bool isBinary)
        {
            string outXml = "";
            CHCNetSDK.NET_DVR_XML_CONFIG_INPUT pInputXml = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
            int nInSize = Marshal.SizeOf(pInputXml);
            pInputXml.dwSize = (uint)nInSize;

            uint dwRequestUrlLen = (uint)strRequestUrl.Length;
            pInputXml.lpRequestUrl = Marshal.StringToHGlobalAnsi(strRequestUrl);
            pInputXml.dwRequestUrlLen = dwRequestUrlLen;

            byte[] byInputParam;
            if (strInputXParam != null)
                if (isBinary)
                    byInputParam = (byte[])strInputXParam;
                else
                    byInputParam = Encoding.UTF8.GetBytes(strInputXParam.ToString());
            else
                byInputParam = null;

            if (byInputParam != null)
            {
                int iXMLInputLen = byInputParam.Length;

                pInputXml.lpInBuffer = Marshal.AllocHGlobal(iXMLInputLen);
                Marshal.Copy(byInputParam, 0, pInputXml.lpInBuffer, iXMLInputLen);
                pInputXml.dwInBufferSize = (uint)byInputParam.Length;

                CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT pOutputXml = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();
                pOutputXml.dwSize = (uint)Marshal.SizeOf(pOutputXml);
                pOutputXml.lpOutBuffer = Marshal.AllocHGlobal(3 * 1024 * 1024);
                pOutputXml.dwOutBufferSize = 3 * 1024 * 1024;
                pOutputXml.lpStatusBuffer = Marshal.AllocHGlobal(4096 * 4);
                pOutputXml.dwStatusSize = 4096 * 4;

                if (!CHCNetSDK.NET_DVR_STDXMLConfig(supperHeadLoginStatus, ref pInputXml, ref pOutputXml))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    MessageBox.Show("NET_DVR_STDXMLConfig failed, error code= " + iLastErr);
                    return "";
                }

                string strOutputParam = Marshal.PtrToStringAnsi(pOutputXml.lpOutBuffer);

                outXml = Encoding.UTF8.GetString(Encoding.Default.GetBytes(strOutputParam));

                Marshal.FreeHGlobal(pOutputXml.lpOutBuffer);
                Marshal.FreeHGlobal(pOutputXml.lpStatusBuffer);
            }

            Marshal.FreeHGlobal(pInputXml.lpRequestUrl);

            return outXml;
        }

        private int StartPlayBack(uint channelID, PictureBox VideoPlayWnd, DateTime dateTimeStart, DateTime dateTimeEnd, int index)
        {
            int playResult = -1;

            CHCNetSDK.NET_DVR_VOD_PARA struVodPara = new CHCNetSDK.NET_DVR_VOD_PARA();

            struVodPara.dwSize = (uint)Marshal.SizeOf(struVodPara);
            struVodPara.struIDInfo.dwChannel = channelID; //通道号 Channel number  
            struVodPara.hWnd = VideoPlayWnd.Handle;//回放窗口句柄

            //设置回放的开始时间 Set the starting time to search video files
            struVodPara.struBeginTime.dwYear = (uint)dateTimeStart.Year;
            struVodPara.struBeginTime.dwMonth = (uint)dateTimeStart.Month;
            struVodPara.struBeginTime.dwDay = (uint)dateTimeStart.Day;
            struVodPara.struBeginTime.dwHour = (uint)dateTimeStart.Hour;
            struVodPara.struBeginTime.dwMinute = (uint)dateTimeStart.Minute;
            struVodPara.struBeginTime.dwSecond = (uint)dateTimeStart.Second;

            //设置回放的结束时间 Set the stopping time to search video files
            struVodPara.struEndTime.dwYear = (uint)dateTimeEnd.Year;
            struVodPara.struEndTime.dwMonth = (uint)dateTimeEnd.Month;
            struVodPara.struEndTime.dwDay = (uint)dateTimeEnd.Day;
            struVodPara.struEndTime.dwHour = (uint)dateTimeEnd.Hour;
            struVodPara.struEndTime.dwMinute = (uint)dateTimeEnd.Minute;
            struVodPara.struEndTime.dwSecond = (uint)dateTimeEnd.Second;

            //按时间回放 Playback by time
            m_lPlayBackHandle[index] = CHCNetSDK.NET_DVR_PlayBackByTime_V40(supperHeadLoginStatus, ref struVodPara);
            if (m_lPlayBackHandle[index] < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                MessageBox.Show("NET_DVR_PlayBackByTime_V40 failed, error code= " + iLastErr);
            }

            uint iOutValue = 0;
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayBackHandle[index], CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                MessageBox.Show("NET_DVR_PLAYSTART failed, error code= " + iLastErr); //回放控制失败，输出错误号
            }

            playResult = 0;

            return playResult;
        }

        /// <summary>
        /// 去掉回放界面对移动物体的标记和框
        /// </summary>
        /// <param name="m_lRealHandle"></param>
        private void RenderPrivateDataClose(int m_lRealHandle)
        {
            int iPort = CHCNetSDK.NET_DVR_GetPlayBackPlayerIndex(m_lRealHandle);
            if (iPort > -1)
            {
                if (!PlayCtrl.PlayM4_RenderPrivateData(iPort, 0x1 + 0x2, false))
                {
                    iLastErr = PlayCtrl.PlayM4_GetLastError(iPort);
                    MessageBox.Show("PlayM4_RenderPrivateData failed, error code= " + iLastErr);
                }
            }
        }
    }
}
