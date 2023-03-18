#if UNITY_EDITOR && UNITASK_WEBREQUEST_SUPPORT

#define UseTelegram
//#define UseDiscord
#define UseGalaxyHomeMiniSpeaker

using System.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Networking;

namespace Plugins.BuildMessage.Editor
{
    public class UnityBuildMessage : UnityEditor.Editor, IPreprocessBuildWithReport,
        IPostprocessBuildWithReport
    {
        #region Message Info

        private string _buildStartMessage;
        private string _buildStartSpeakerMessage;

        private string _buildSuccessMessage;
        private string _buildSuccessSpeakerMessage;

        private string _buildFailMessage;
        private string _buildFailSpeakerMessage;

        private static string _lightmapStartMessage;
        private static string _lightmapStartSpeakerMessage;

        private static string _lightmapSuccessMessage;
        private static string _lightmapSuccessSpeakerMessage;
        
        private void Awake()
        {
            string projectName = GetProjectName();
            
            _buildStartMessage = $"유니티 {projectName} 프로젝트에서 빌드 시작했습니다.";
            _buildStartSpeakerMessage = $"유니티 프로젝트,{projectName}가 빌드 시작했습니다.";

            _buildSuccessMessage = $"유니티 {projectName} 프로젝트에서 빌드 완료했습니다.";
            _buildSuccessSpeakerMessage = $"유니티 프로젝트,{projectName}가 빌드 완료했습니다.";

            _buildFailMessage = $"유니티 {projectName} 프로젝트에서 빌드 실패했습니다.";
            _buildFailSpeakerMessage = $"유니티 프로젝트,{projectName}가 빌드 실패했습니다.";

            _lightmapStartMessage = $"유니티 {projectName} 프로젝트에서 라이트맵 굽기 시작되었습니다.";
            _lightmapStartSpeakerMessage = $"유니티 프로젝트,{projectName} 프로젝트에서 라이트맵 굽기 시작되었습니다.";

            _lightmapSuccessMessage = $"유니티 {projectName} 프로젝트에서 라이트맵 굽기 완료되었습니다.";
            _lightmapSuccessSpeakerMessage = $"유니티 프로젝트,{projectName} 프로젝트에서 라이트맵 굽기 완료되었습니다.";
        }

        #endregion

        #region Token

        #region Discord

        //챗봇 웹훅
        private const string DiscordHook = ""; //필수

        #endregion

        #region Telegram

        private const string TelegramToken = ""; //필수
        private const string TelegramID = ""; //필수

        #region Hide

        //텔레그램 ID 찾는 사이트 훅
        private const string TelegramUpdateLogHook =
            "https://api.telegram.org/bot[Token]/getUpdates";

        //텔레그램 메세지 전달 훅
        private const string TelegramSendHook =
            "https://api.telegram.org/bot[Token]/sendmessage?chat_id=[UserID]&text=[Message]";

        #endregion
        
        #endregion

        #region 스마트싱스 스피커

        private const string DeviceID = ""; //필수
        private const string SmartThingsToken = ""; //필수

        #region Hide

        //스마트싱스 스피커 훅
        private const string SmartThingsSpeakerHook =
            "https://api.smartthings.com/v1/devices/[DeviceID]/commands";

        #endregion
        
#endregion

        #endregion

        #region Test

        [MenuItem("Tools/Build Notification/Utility/Telegram/CheckID")]
        public static void OpenCheckIDSite()
        {
            string editTelegramHook = TelegramUpdateLogHook;
            editTelegramHook = editTelegramHook.Replace("[Token]", TelegramToken);
            Application.OpenURL(editTelegramHook);
        }

        [MenuItem("Tools/Build Notification/Test/Telegram/SendMessage")]
        public static void TestSendTelegramMessage()
        {
            string message = "유니티 " + GetProjectName() + " 프로젝트에서 빌드 완료했습니다.";

            //텔레그램
            SendTelegramMessage(message, true).Forget();
        }

        [MenuItem("Tools/Build Notification/Test/Discord/SendMessage")]
        public static void TestSendDiscordMessage()
        {
            string message = "유니티 " + GetProjectName() + " 프로젝트에서 빌드 완료했습니다.";

            //디스코드
            SendDiscordMessage(message, true).Forget();
        }

        [MenuItem("Tools/Build Notification/Test/Smartthings/SendMessage")]
        public static void TestSendSmartthingsMessage()
        {
            string message = "유니티 프로젝트," + GetProjectName() + "가 빌드 완료했습니다.";

            //디스코드
            SendSmartThingsMessage(message, true).Forget();
        }

        #endregion

        #region Core

        [UsedImplicitly]
        private static async UniTaskVoid SendDiscordMessage(string message, bool test = false)
        {
            if (!test)
            {
                int buildAlarmState = EditorPrefs.GetInt("BuildAlarmState", 0);

                if (buildAlarmState == 0)
                    return;
            }

            WWWForm form = new();
            form.AddField("content", message);

            UnityWebRequest request = UnityWebRequest.Post(DiscordHook, form);
            await request.SendWebRequest();
            request.Dispose();
        }

        [UsedImplicitly]
        private static async UniTaskVoid SendTelegramMessage(string message, bool test = false)
        {
            if (!test)
            {
                int buildAlarmState = EditorPrefs.GetInt("BuildAlarmState", 0);

                if (buildAlarmState == 0)
                    return;
            }

            string editTelegramHook = TelegramSendHook;
            editTelegramHook = editTelegramHook.Replace("[Token]", TelegramToken);
            editTelegramHook = editTelegramHook.Replace("[UserID]", TelegramID);
            editTelegramHook = editTelegramHook.Replace("[Message]", message);

            using UnityWebRequest request = UnityWebRequest.Get(editTelegramHook);
            await request.SendWebRequest();
            request.Dispose();
        }

        [UsedImplicitly]
        private static async UniTaskVoid SendSmartThingsMessage(string message, bool test = false)
        {
            if (!test)
            {
                int buildAlarmState = EditorPrefs.GetInt("BuildAlarmState", 0);

                if (buildAlarmState == 0)
                    return;
            }

            string editSmartthingsCommend = SmartThingsSpeakerHook;
            editSmartthingsCommend = editSmartthingsCommend.Replace("[DeviceID]", DeviceID);

            string requestBody =
                "{\"commands\": [{\"component\": \"main\",\"capability\": \"speechSynthesis\",\"command\": \"speak\",\"arguments\": [\"" +
                message + "\"]}] }";

            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);

            UnityWebRequest request = new(editSmartthingsCommend, "POST");
            request.SetRequestHeader("Authorization", "Bearer " + SmartThingsToken);
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            await request.SendWebRequest();
            request.Dispose();
        }

        #endregion

        #region Build

        public int callbackOrder => 0;

        /// <summary>
        /// 빌드 시작시
        /// </summary>
        /// <param name="report"></param>
        public void OnPreprocessBuild(BuildReport report)
        {
#if UseTelegram
            SendTelegramMessage(_buildStartMessage).Forget();
#endif

#if UseDiscord
            SendDiscordMessage(_buildStartMessage).Forget();
#endif

#if UseGalaxyHomeMiniSpeaker
            SendSmartThingsMessage(_buildStartSpeakerMessage).Forget();
#endif
            Application.logMessageReceived += OnBuildError;
        }

        /// <summary>
        /// 빌드 성공 또는 완료시
        /// </summary>
        /// <param name="report"></param>
        public void OnPostprocessBuild(BuildReport report)
        {
#if UseTelegram
            SendTelegramMessage(_buildSuccessMessage).Forget();
#endif

#if UseDiscord
            SendDiscordMessage(_buildSuccessMessage).Forget();
#endif

#if UseGalaxyHomeMiniSpeaker
            SendSmartThingsMessage(_buildSuccessSpeakerMessage).Forget();
#endif
            Application.logMessageReceived -= OnBuildError;
        }

        /// <summary>
        /// 빌드 실패시
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="stacktrace"></param>
        /// <param name="type"></param>
        private void OnBuildError(string condition, string stacktrace, LogType type)
        {
            if (type == LogType.Error)
            {
#if UseTelegram
                SendTelegramMessage(_buildFailMessage).Forget();
#endif

#if UseDiscord
                SendDiscordMessage(_buildFailMessage).Forget();
#endif

#if UseGalaxyHomeMiniSpeaker
                SendSmartThingsMessage(_buildFailSpeakerMessage).Forget();
#endif
                Application.logMessageReceived -= OnBuildError;
            }
        }

        #endregion

        #region LightMap Bake

        private static void LightBakeStart()
        {
#if UseTelegram
            SendTelegramMessage(_lightmapStartMessage).Forget();
#endif

#if UseDiscord
            SendDiscordMessage(_lightmapStartMessage).Forget();
#endif

#if UseGalaxyHomeMiniSpeaker

            SendSmartThingsMessage(_lightmapStartSpeakerMessage).Forget();
#endif
        }

        private static void LightBakeComplete()
        {
#if UseTelegram
            SendTelegramMessage(_lightmapSuccessMessage).Forget();
#endif

#if UseDiscord
            SendDiscordMessage(_lightmapSuccessMessage).Forget();
#endif

#if UseGalaxyHomeMiniSpeaker
            SendSmartThingsMessage(_lightmapSuccessSpeakerMessage).Forget();
#endif
        }

        #region Option

        [MenuItem("Tools/Build Notification/Alarm ON-OFF/Build/Build 알림 ON", false, 0)]
        public static void BuildMessageOn()
        {
            EditorPrefs.SetInt("BuildAlarmState", 1);
            Debug.Log("빌드 알림 : ON");
        }

        [MenuItem("Tools/Build Notification/Alarm ON-OFF/Build/Build 알림 OFF", false, 0)]
        public static void BuildMessageOff()
        {
            EditorPrefs.SetInt("BuildAlarmState", 0);
            Debug.Log("빌드 알림 : OFF");
        }

        [MenuItem("Tools/Build Notification/Alarm ON-OFF/LightMap/Lightmap Bake 알림 ON", false, 0)]
        public static void LightBakeMessageOn()
        {
            Lightmapping.bakeCompleted -= LightBakeComplete;
            Lightmapping.bakeStarted -= LightBakeStart;

            Lightmapping.bakeCompleted += LightBakeComplete;
            Lightmapping.bakeStarted += LightBakeStart;
            Debug.Log("라이트맵 굽기알림 : ON");
        }

        [MenuItem("Tools/Build Notification/Alarm ON-OFF/LightMap/Lightmap Bake 알림 OFF", false, 0)]
        public static void LightBakeMessageOff()
        {
            Lightmapping.bakeCompleted -= LightBakeComplete;
            Lightmapping.bakeStarted -= LightBakeStart;
            Debug.Log("라이트맵 굽기알림 : OFF");
        }

        #endregion

        #endregion

        #region Utility

        private static string GetProjectName()
        {
            // string[] s = Application.dataPath.Split('/');
            // string projectName = s[^2];
            return Application.productName;
        }

        #endregion
    }
}
#endif