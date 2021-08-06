using UnityEngine;
using System.Collections;
#if UNITY_ANDROID || UNITY_EDITOR
using Ximmerse.RhinoX.Internal;
#endif
using System.Threading.Tasks;
using System.IO;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;
using UnityEngine.SceneManagement;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// RhinoX device authentication interface. 
    /// How to use :
    /// 1. Put a RXDeviceAuthentication component at any game object in your scene.
    /// 2. Input your AppID, developer Id, developer Key.
    /// 3. Call GetSdkAuthorizationAvailableSeconds() to get your app's remain time.
    /// </summary>
    public class RXDeviceAuthentication : MonoBehaviour
#if UNITY_ANDROID || UNITY_EDITOR
        , I_Authentication
#endif
    {

        [SerializeField]
        string m_AppID = "10";

        [SerializeField]
        string m_DeveloperID = "";

        [SerializeField]
        string m_DeveloperKey = "";

        /// <summary>
        /// Gets or sets the config app identifier.
        /// </summary>
        /// <value>The config app identifier.</value>
        public string ConfigAppID { get { return m_AppID; } set { m_AppID = value; } }

        /// <summary>
        /// Gets or sets the config developer identifier.
        /// </summary>
        /// <value>The config developer identifier.</value>
        public string ConfigDeveloperID { get { return m_DeveloperID; } set { m_DeveloperID = value; } }

        /// <summary>
        /// Gets or sets the config developer key.
        /// </summary>
        /// <value>The config developer key.</value>
        public string ConfigDeveloperKey { get { return m_DeveloperKey; } set { m_DeveloperKey = value; } }

        /// <summary>
        /// If true, use the authentication file.
        /// </summary>
        public bool UseAuthenticationFile = false;

        public string AuthFilePath = "/sdcard/ximmerse/AuthInfo.json";

        /// <summary>
        /// if true, the application's package ID will be applied.
        /// </summary>
        public bool UseApplicationIDAsAppID = true;

        public struct AuthInfo
        {
            public string appID;
            public string developerID;
            public string developerKey;
        }

        static RXDeviceAuthentication instance;

        public static RXDeviceAuthentication Instance
        {
            get
            {
                if (!instance)
                    instance = FindObjectOfType<RXDeviceAuthentication>();
                return instance;
            }
        }

        /// <summary>
        /// Is the rhinoX authroized to start current game ?
        /// </summary>
        public static bool IsAuthorized
        {
            get;
            private set;
        }

        /// <summary>
        /// Event : fired when authentication error status is changed.
        /// </summary>
        public static event System.Action OnAuthenticationErrorEvent, OnAuthenticationSuccessEvent;

        /// <summary>
        /// Is authentication ends in error ?
        /// </summary>
        public static bool IsAuthenticationEndsInError
        {
            get; private set;
        }

        /// <summary>
        /// The remain time.
        /// </summary>
        public static int RemainTime
        {
            get; private set;
        }

        bool NeedsReload = false;

        bool RequestQuitApp = false;

        /// <summary>
        /// If event is dirty , means authentication event happens
        /// </summary>
        static bool EventDirty = false;

        private void Awake()
        {
            #if UNITY_ANDROID
            instance = this;
            IsAuthorized = false;

            if (UseAuthenticationFile && !string.IsNullOrEmpty(AuthFilePath))
            {
                try
                {
                    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageRead);
                    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
                    string txt = File.ReadAllText(AuthFilePath);
                    var authInfo = JsonUtility.FromJson<AuthInfo>(txt);
                    this.m_AppID = authInfo.appID;
                    this.m_DeveloperID = authInfo.developerID;
                    this.m_DeveloperKey = authInfo.developerKey;
                    Debug.LogFormat("Read auth-info json, appID:{0}, developerID:{1}, developerKey:{2}", m_AppID, m_DeveloperID, m_DeveloperKey);
                }
                catch (System.Exception exc)
                {
                    Debug.LogFormat("Error reading auth-info json file: {0}", AuthFilePath);
                    Debug.LogException(exc);
                }
            }

            if (UseApplicationIDAsAppID)
            {
                this.m_AppID = Application.identifier;
            }

            //Applies authenication interface:
            typeof(RhinoXSystem)
            .GetProperty("authenticationImpl", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            .SetMethod.Invoke(null, new object[] { this });
#endif
        }

        IEnumerator Start()
        {
#if UNITY_ANDROID
            IsAuthorized = false;
            while (ARCamera.Instance == null || !ARCamera.Instance.IsARBegan)
            {
                yield return null;
            }

            yield return new WaitForSeconds(1);

            XDevicePlugin.OpCenterMessagesDelegates rxOpMsgAction = new XDevicePlugin.OpCenterMessagesDelegates();
            rxOpMsgAction.OnRequestBeginMission = (string strArg) =>
            {
                //For the first time:
                if (IsAuthorized == false)
                {
                    Debug.LogFormat("On 1st request begin mission, parameter: {0}", strArg);
                    int remain_time_sec = XDevicePlugin.OnMissionBegin();

                    Debug.LogFormat("On mission begin , remain time: {0}", remain_time_sec);
                    RemainTime = remain_time_sec;

                    if (remain_time_sec <= 0)
                    {
                        IsAuthenticationEndsInError = true;
                        IsAuthorized = false;
                        EventDirty = true;
                    }
                    else
                    {
                        IsAuthenticationEndsInError = false;
                        IsAuthorized = true;
                        EventDirty = true;
                    }
                }
                //第二次启动:
                else
                {
                    Debug.LogFormat("On 2nd request begin mission, parameter: {0}", strArg);
                    int remain_time_sec = XDevicePlugin.OnMissionBegin();
                    RemainTime = remain_time_sec;
                    Debug.LogFormat("On mission begin , remain time: {0}", remain_time_sec);
                    //启动错误:
                    if (remain_time_sec <= 0)
                    {
                        IsAuthenticationEndsInError = true;
                        IsAuthorized = false;
                        EventDirty = true;
                    }
                    else
                    {
                        IsAuthenticationEndsInError = false;
                        IsAuthorized = true;
                        NeedsReload = true;
                        EventDirty = true;
                    }
                }
            };
            rxOpMsgAction.OnRequestEndMission = (string strArg) =>
            {
                Debug.LogFormat("On request end mission, parameter: {0}", strArg);
            };
            rxOpMsgAction.OnRequestExitApp = (string strArg) =>
            {
                Debug.LogFormat("On request quit app, parameter: {0}", strArg);
                RequestQuitApp = true;
            };
            XDevicePlugin.SetOpCenterMessageDelegates(rxOpMsgAction);
#else
            yield return null;
#endif
        }

        void Update()
        {
            //中控请求关闭应用
            if (RequestQuitApp)
            {
                RequestQuitApp = false;
                Invoke("QuitApp", 0);
            }
            if (EventDirty)
            {
                EventDirty = false;
                if (IsAuthenticationEndsInError)
                {
                    OnAuthenticationErrorEvent?.Invoke();
                }
                else if (IsAuthorized)
                {
                    OnAuthenticationSuccessEvent?.Invoke();
                }
            }
            //if(IsAuthenticationEndsInError)
            //{
            //    Debug.LogError("[RxAuthentication Error] - Authentication ends in error.");
            //}

            if (NeedsReload)
            {
                NeedsReload = false;
                Debug.Log("Reload on restart mission");
                SceneManager.LoadScene(0);//重新读取第一个level.
            }

        }

        /// <summary>
        /// End mission is called when the game is over.
        /// </summary>
        public static void EndMission()
        {
#if UNITY_ANDROID
            if (IsAuthorized && Application.platform == RuntimePlatform.Android)
            {
                var now = System.DateTime.Now;
                Debug.LogFormat("End Mission at time : {0}", now.ToString());
                int second = XDevicePlugin.OnMissionEnd();
                var ts = System.DateTime.Now - now;
                Debug.LogFormat("End Mission Result : {0} sec, total time elapse : {1}", second, ts.TotalSeconds.ToString("F2"));
            }
#endif
        }

        /// <summary>
        /// 退出应用.
        /// </summary>
        void QuitApp()
        {
            Application.Quit();
        }


//#if UNITY_EDITOR

        [ContextMenu("Simulate Error")]
        public static void SimAuthError()
        {
            EventDirty = true;
            IsAuthenticationEndsInError = true;
            IsAuthorized = false;
            Debug.Log("Simulate error");
        }

        [ContextMenu("Simulate Auth Success")]
        public static void SimAuthSuccess()
        {
            EventDirty = true;
            IsAuthenticationEndsInError = false;
            IsAuthorized = true;
            Debug.Log("Simulate success");
        }
//#endif

    }
}