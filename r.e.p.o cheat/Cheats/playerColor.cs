﻿using System;
using System.Reflection;
using UnityEngine;
using Photon.Pun;

namespace r.e.p.o_cheat
{
    internal class playerColor
    {
        public static bool isRandomizing = false;
        private static float lastColorChangeTime = 0f;
        private static float changeInterval = 0.1f;

        private static Type colorControllerType;
        private static object colorControllerInstance;
        private static MethodInfo playerSetColorMethod;
        private static bool isInitialized = false;

        private static void Initialize()
        {
            if (isInitialized) return;

            colorControllerType = Type.GetType("PlayerAvatar, Assembly-CSharp");
            if (colorControllerType == null)
            {
                DLog.Log("colorControllerType (PlayerAvatar) not found.");
                return;
            }
            DLog.Log("colorControllerType (PlayerAvatar) found.");

            colorControllerInstance = null;

            if (PhotonNetwork.IsConnected)
            {
                var photonViews = UnityEngine.Object.FindObjectsOfType<PhotonView>();
                DLog.Log($"Found {photonViews.Length} PhotonViews in the scene.");
                foreach (var photonView in photonViews)
                {
                    if (photonView != null && photonView.IsMine)
                    {
                        var playerAvatar = photonView.gameObject.GetComponent(colorControllerType);
                        if (playerAvatar != null)
                        {
                            colorControllerInstance = playerAvatar;
                            DLog.Log($"Local PlayerAvatar found via PhotonView: {photonView.gameObject.name}, Owner: {photonView.Owner?.NickName}");
                            break;
                        }
                    }
                }
            }
            else
            {
                var playerAvatar = UnityEngine.Object.FindObjectOfType(colorControllerType);
                if (playerAvatar != null)
                {
                    colorControllerInstance = playerAvatar;
                    DLog.Log($"PlayerAvatar found in singleplayer via FindObjectOfType: {(playerAvatar as MonoBehaviour).gameObject.name}");
                }
                else
                {
                    GameObject localPlayer = DebugCheats.GetLocalPlayer();
                    if (localPlayer != null)
                    {
                        var playerAvatarComponent = localPlayer.GetComponent(colorControllerType);
                        if (playerAvatarComponent != null)
                        {
                            colorControllerInstance = playerAvatarComponent;
                            DLog.Log($"PlayerAvatar found in singleplayer via GetLocalPlayer: {localPlayer.name}");
                        }
                        else
                        {
                            DLog.Log("PlayerAvatar component not found in object returned by GetLocalPlayer.");
                        }
                    }
                    else
                    {
                        DLog.Log("No PlayerAvatar found in singleplayer via GetLocalPlayer.");
                    }
                }
            }

            if (colorControllerInstance == null)
            {
                DLog.Log("No local PlayerAvatar found for this client (multiplayer or singleplayer).");
                return;
            }

            playerSetColorMethod = colorControllerType.GetMethod("PlayerAvatarSetColor", BindingFlags.Public | BindingFlags.Instance);
            if (playerSetColorMethod == null)
            {
                DLog.Log("PlayerAvatarSetColor method not found in PlayerAvatar.");
                return;
            }

            isInitialized = true;
            DLog.Log("playerColor successfully initialized for local player.");
        }

        public static void colorRandomizer()
        {
            Initialize();

            if (!isInitialized || colorControllerInstance == null || playerSetColorMethod == null)
            {
                DLog.Log("Randomizer ignored: Initialization failure or missing instance/method.");
                return;
            }

            if (isRandomizing && Time.time - lastColorChangeTime >= changeInterval)
            {
                var colorIndex = new System.Random().Next(0, 30);
                try
                {
                    playerSetColorMethod.Invoke(colorControllerInstance, new object[] { colorIndex });
                    lastColorChangeTime = Time.time;
                    DLog.Log($"Local player color changed to index: {colorIndex}");
                }
                catch (Exception e)
                {
                    DLog.Log($"Error invoking PlayerAvatarSetColor: {e.Message}");
                }
            }
        }

        public static void Reset()
        {
            isInitialized = false;
            colorControllerType = null;
            colorControllerInstance = null;
            playerSetColorMethod = null;
            DLog.Log("playerColor reset.");
        }
    }
}