using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

using GameData;
using System.IO;
using System.Linq;
using System;
using Google;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEditor;
using TMPro;
using System.Threading.Tasks;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public string userId;
    public UserData userData;

    public readonly int[] favorMax = { 0, 50, 100, 150, 200, 250, 300, 400 };

    public List<Character> characterSotred;

    // firebase
    DatabaseReference dbref;
    Firebase.FirebaseApp app;

    private string webClientId = "369182402609-nic6rvbt5vttsa83rmoeaujohhd76j3t.apps.googleusercontent.com";
    Firebase.Auth.FirebaseAuth auth;
    GoogleSignInConfiguration configuration;

    // json
    public string path;    // 읽기/쓰기가 가능한 로컬저장영역

    private void Awake()
    {
        // 싱글톤
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        userData = new UserData();
        path = Application.persistentDataPath + "/";

        Debug.Log(path);
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };

        CheckFirebaseDependencies();
        Debug.Log(path);
        // FirebaseInit();
        loadData();
        CharacterSort();

    }

    void CheckFirebaseDependencies()
    {
        Debug.Log(path);
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
           {
               if (task.IsCompleted)
               {
                   if (task.Result == DependencyStatus.Available)
                   {
                       auth = FirebaseAuth.DefaultInstance;
                       Debug.Log("Firebase 초기화 성공!");
                   }

                   else
                       Debug.LogError("Firebase dependency 문제 발생: " + "Could not resolve all Firebase dependencies: " + task.Result.ToString());

               }
               else
                   Debug.LogError("Dependency check was not completed. Error : " + "Firebase 초기화 실패: " + task.Exception?.Message);

           });
    }

    public void SignInWithGoogle() { OnSignIn(); }
    public void SignOutFromGoogle() { OnSignOut(); }

    private void OnSignIn()
    {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,  // Firebase에서 제공하는 Web Client ID
            RequestIdToken = true,
            RequestEmail = true,    
            UseGameSignIn = false
        };

        Debug.Log("Calling SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Sign-in Failed");
                foreach (Exception e in task.Exception.InnerExceptions)
                    Debug.LogError("Exception: " + e.Message + " StackTrace: " + e.StackTrace);

            }
            else if (task.IsCanceled)
                Debug.Log("Sign-in Canceled");

            else
                Debug.Log("Sign-in Success: " + task.Result.DisplayName);

        });
        // GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void OnSignOut()
    {
        Debug.Log("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        Debug.Log("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.Log("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    Debug.Log("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            Debug.Log("Canceled");
        }
        else
        {
            Debug.Log("Welcome: " + task.Result.DisplayName + "!");
            Debug.Log("Email = " + task.Result.Email);
            Debug.Log("Google ID Token = " + task.Result.IdToken);
            Debug.Log("Email = " + task.Result.Email);
            SignInWithGoogleOnFirebase(task.Result.IdToken);
        }
    }

    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            AggregateException ex = task.Exception;
            if (ex != null)
            {
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                    Debug.Log("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);
            }
            else
            {
                Debug.Log("Sign In Successful.");
            }
        });
    }

    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        Debug.Log("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        Debug.Log("Calling Games SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }




    // 걍 json
    public void loadData()
    {
        // userData 로드 
        string userJson = File.ReadAllText(path + "userData.json");
        userData = JsonUtility.FromJson<UserData>(userJson);

        // systemData 로드
        // string systemJson = File.ReadAllText(path + "systemData.json");
        // systemData = JsonUtility.FromJson<SystemData>(systemJson);
    }

    public void saveData()
    {
        File.WriteAllText(path + "userData.json", JsonUtility.ToJson(userData, true));
        // File.WriteAllText(path + "systemData.json",JsonUtility.ToJson(systemData, true));
    }

    public void CharacterSort()
    {
        characterSotred = userData.characters
                            .OrderBy(c => c.locked) // Locked == 0인 요소가 앞에 오도록
                            .ThenBy(c => c.locked == 0 ? DateTime.Parse(c.unlockDate) : DateTime.MaxValue) // Locked == 0인 경우 Date로 정렬
                            .ToList();
    }

    // 파이어베이스 초기화 
    // public void FirebaseInit()
    // {
    //     Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
    //     {
    //         var dependencyStatus = task.Result;
    //         if (dependencyStatus == Firebase.DependencyStatus.Available)
    //         {
    //             // Create and hold a reference to your FirebaseApp,
    //             // where app is a Firebase.FirebaseApp property of your application class.
    //             app = Firebase.FirebaseApp.DefaultInstance;
    //             auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

    //             // app 내가 임의로 선언함..

    //             // Set a flag here to indicate whether Firebase is ready to use by your app.
    //         }
    //         else
    //         {
    //             UnityEngine.Debug.LogError(System.String.Format(
    //               "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
    //             // Firebase Unity SDK is not safe to use here.
    //         }
    //     });


    // }


    // public void GetGoogleToken(string googleIdToken, string googleAccessToken)
    // {
    //     Firebase.Auth.Credential credential =
    //         Firebase.Auth.GoogleAuthProvider.GetCredential(googleIdToken, googleAccessToken);

    //     auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
    //     {
    //         if (task.IsCanceled)
    //         {
    //             Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
    //             return;
    //         }
    //         if (task.IsFaulted)
    //         {
    //             Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
    //             return;
    //         }

    //         Firebase.Auth.AuthResult result = task.Result;
    //         Debug.LogFormat("User signed in successfully: {0} ({1})",
    //             result.User.DisplayName, result.User.UserId);
    //     });
    // }

    // public void SignInAccount()
    // {
    //     Firebase.Auth.FirebaseUser user = auth.CurrentUser;
    //     if (user != null)
    //     {
    //         string name = user.DisplayName;
    //         string email = user.Email;
    //         System.Uri photo_url = user.PhotoUrl;
    //         // The user's Id, unique to the Firebase project.
    //         // Do NOT use this value to authenticate with your backend server, if you
    //         // have one; use User.TokenAsync() instead.
    //         string uid = user.UserId;
    //     }
    // }

    // public void SignOut()
    // {
    //     auth.SignOut();
    // }

    // public void InitializeUserData()
    // {

    // }

}
