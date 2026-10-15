using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhoDunBit.Networking
{
    public class OnlineManager : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_InputField _joinCodeInput;

        private bool _onlineReady;
        private bool _connecting;
        private ISession _session;

        private async void Start()
        {
            try
            {
                InitializationOptions options = new();

                string profile = Application.isEditor ? "editor" : "build";

                string[] arguments = Environment.GetCommandLineArgs();
                int nameIndex = Array.IndexOf(arguments, "-name");

                if (Application.isEditor && nameIndex >= 0 && nameIndex + 1 < arguments.Length)
                {
                    profile = arguments[nameIndex + 1];
                }

                string profileHash = Hash128.Compute(profile).ToString();

                profile = "test_" + profileHash.Substring(0, 20);

                options.SetProfile(profile);

                await UnityServices.InitializeAsync(options);

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                _onlineReady = true;

                Debug.Log($"Online ready. Player ID: " + AuthenticationService.Instance.PlayerId, this);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        public async void Host()
        {
            if (!_onlineReady)
            {
                Debug.Log("Cannot host session. Waiting for online initialization to finish.", this);
                return;
            }

            if (_connecting || _session != null)
            {
                return;
            }

            _connecting = true;

            try
            {
                // Keep this object alive when Main Menu unloads.
                DontDestroyOnLoad(gameObject);

                // Gameplay contains the NetworkManager we need.
                await SceneManager.LoadSceneAsync("Gameplay");

                SessionOptions options = new SessionOptions
                {
                    MaxPlayers = 8,
                    IsPrivate = true
                }.WithRelayNetwork();

                _session = await MultiplayerService.Instance.CreateSessionAsync(options);

                Debug.Log($"Hosting session. Join code: {_session.Code}", this);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                _connecting = false;
            }
        }

        public async void Join()
        {
            if (!_onlineReady)
            {
                Debug.Log("Cannot join session. Waiting for online initialization to finish.", this);
                return;
            }

            if (_connecting || _session != null)
                return;

            string joinCode = _joinCodeInput.text.Trim().ToUpperInvariant();

            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.Log("Cannot join session. Join code null or empty.", this);
                return;
            }

            _connecting = true;

            try
            {
                DontDestroyOnLoad(gameObject);

                await SceneManager.LoadSceneAsync("Gameplay");

                _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);

                Debug.Log($"Joined game: {_session.Id}", this);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not join game: {exception.Message}", this);

                await ReturnToMenuAfterFailureAsync();
            }
            finally
            {
                _connecting = false;
            }
        }

        private async Task ReturnToMenuAfterFailureAsync()
        {
            NetworkManager networkManager = NetworkManager.Singleton;

            if (networkManager != null)
            {
                networkManager.Shutdown();

                // Shutdown can take several frames.
                while (networkManager != null && networkManager.ShutdownInProgress)
                {
                    await Awaitable.NextFrameAsync();
                }

                if (networkManager != null)
                {
                    Destroy(networkManager.gameObject);
                }
            }

            // The menu will create a fresh OnlineManager.
            Destroy(gameObject);

            // Allow the old objects to finish being destroyed.
            await Awaitable.NextFrameAsync();

            await SceneManager.LoadSceneAsync("Main Menu");
        }
    }
}
