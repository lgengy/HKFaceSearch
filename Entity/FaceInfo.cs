using System;
using System.Collections.Generic;
using System.Linq;

namespace HKFaceSearch.Entity
{
    class FaceInfo
    {
        private string _snapPicURL;
        private string _snapTime;
        private string _facePicURL;
        private string _glass;
        private string _smile;
        private string _similarity;
        private string _channelID;
        private string _sex;
        private string _ageGroup;
        private string _mask;
        private string _faceExpression;

        public string snapPicURL { get => _snapPicURL; set => _snapPicURL = value; }
        public string snapTime { get => _snapTime; set => _snapTime = value; }
        public string facePicURL { get => _facePicURL; set => _facePicURL = value; }
        public string glass { get => _glass; set => _glass = value; }
        public string smile { get => _smile; set => _smile = value; }
        public string similarity { get => _similarity; set => _similarity = value; }
        public string channelID { get => _channelID; set => _channelID = value; }
        public string sex { get => _sex; set => _sex = value; }
        public string ageGroup { get => _ageGroup; set => _ageGroup = value; }
        public string mask { get => _mask; set => _mask = value; }
        public string faceExpression { get => _faceExpression; set => _faceExpression = value; }

        public static DateTime? FormatSnapTime(string snapTime)
        {
            DateTime? dateTime = null;
            try
            {
                if (!string.IsNullOrEmpty(snapTime))
                {
                    dateTime = DateTime.Parse(snapTime);
                }
            }
            catch (Exception)
            {

            }
            return dateTime;
        }

        /// <summary>
        /// 通道分组后根据时间升序排序
        /// </summary>
        /// <param name="listFaceInfo"></param>
        /// <returns></returns>
        public static void SortFaceInfoBySnapTime(ref List<FaceInfo> listFaceInfo)
        {
            List<FaceInfo> faceInfos = new List<FaceInfo>();
            
            foreach(IGrouping<string, FaceInfo> group in listFaceInfo.GroupBy(_ => _.channelID))
            {
                foreach(FaceInfo info in group.OrderBy(_ => FormatSnapTime(_.snapTime)))
                {
                    faceInfos.Add(info);
                }
            }

            listFaceInfo = faceInfos;
        }
    }
}
