using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using BepInEx;
using System.Reflection;
using Photon.Pun;
using UnityEngine;
using System.Linq;
using Utilla.Utils;
using GorillaNetworking;
using ExitGames.Client.Photon;

namespace Utilla
{
    public class UtillaNetworkController : MonoBehaviourPunCallbacks
    {
        public static Events events;

        Events.RoomJoinedArgs lastRoom;

		public GamemodeManager gameModeManager;
  
		public override void OnJoinedRoom()
		{
            // trigger events
            bool isPrivate = false;
            string gamemode = "";
            if (PhotonNetwork.CurrentRoom != null)
            {
                var currentRoom = PhotonNetwork.NetworkingClient.CurrentRoom;
                isPrivate = !currentRoom.IsVisible ||
                            currentRoom.CustomProperties.ContainsKey("Description"); // Room Browser rooms
				if (currentRoom.CustomProperties.TryGetValue("gameMode", out var gamemodeObject))
				{
					gamemode = "MODDED_" + (currentRoom.CustomProperties["gameMode"] as string);
                    currentRoom.CustomProperties["gameMode"] = gamemode;
                }
			}

			// TODO: Generate dynamically
			var prefix = "CUSTOM";
			GorillaComputer.instance.currentGameModeText.Value = "CURRENT MODE\n" + prefix;

			Events.RoomJoinedArgs args = new Events.RoomJoinedArgs
            {
                isPrivate = isPrivate,
                Gamemode = gamemode
            };
            events.TriggerRoomJoin(args);

            lastRoom = args;

			RoomUtils.ResetQueue();
        }

		public override void OnLeftRoom()
		{
            if (lastRoom != null)
			{
				events.TriggerRoomLeft(lastRoom);
				lastRoom = null;
			}

			GorillaComputer.instance.currentGameModeText.Value = "CURRENT MODE\n-NOT IN ROOM-";
		}

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
			if (!propertiesThatChanged.TryGetValue("gameMode", out var gameModeObject)) return;
			if (!(gameModeObject is string gameMode)) return;
			if (!gameMode.Contains(Models.Gamemode.GamemodePrefix))
			{
				gameMode = "MODDED_" + gameMode;
            }

			if (lastRoom.Gamemode.Contains(Models.Gamemode.GamemodePrefix) && !gameMode.Contains(Models.Gamemode.GamemodePrefix))
			{
				gameModeManager.OnRoomLeft(null, lastRoom);
			}
				
			lastRoom.Gamemode = gameMode;
			lastRoom.isPrivate = PhotonNetwork.CurrentRoom.IsVisible;

        }
    }
}
