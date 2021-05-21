using System;
using System.Windows.Forms;

namespace HKFaceSearch
{
    public partial class MainForm : Form
    {
        private bool initializedSDK = false;//海康设备初始化成功标志
        private uint iLastErr = 0;//错误消息代码
        private int[] m_lPlayBackHandle = { -1, -1 };//回放状态
        private int supperHeadLoginStatus = -1;//超脑登录状态
        private int temperatureLoginStatus = -1;//测温相机登录状态
        private int m_lAlarmHandle = -1;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                initializedSDK = CHCNetSDK.NET_DVR_Init();
                if (initializedSDK == false)
                {
                    MessageBox.Show("海康设备初始化失败！");
                    return;
                }
                else
                {
                    //保存SDK日志 To save the SDK log
                    CHCNetSDK.NET_DVR_SetLogToFile(3, "D:\\HKSDK\\", true);

                    //超脑登录
                    HKDeviceLogin(ref supperHeadLoginStatus, "IP", "USERNAME", "PASSWORD", "PORT");
                    //测温摄像头登录
                    HKDeviceLogin(ref temperatureLoginStatus, "IP", "USERNAME", "PASSWORD", "PORT");

                    if (supperHeadLoginStatus >= 0 && temperatureLoginStatus >= 0)
                    {
                        flp_RealtimePhotos.Controls.Clear();

                        //布防
                        AlarmLoad();
                        AlarmStart(ref m_lAlarmHandle, temperatureLoginStatus);
                    }
                    else
                    {
                        MessageBox.Show("海康设备尚未登录，无法预览！");
                        return;
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}
