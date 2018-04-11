﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace emotitron.Network.NST
{

	/// <summary>
	/// If this seems like this should all be in MasterNetAdapter, it should... only since it is common code between all network libraries and is not hotpath
	/// I have moved it here so it isn't replicated and a pain to make changes to.
	/// </summary>
	public class MasterNetCommon
	{
		public static void RegisterCallbackInterfaces(Component obj)
		{
			AddCallback<INetEvents>(MasterNetAdapter.iNetEvents, obj);
			AddCallback<IOnConnect>(MasterNetAdapter.iOnConnect, obj);
			AddCallback<IOnStartLocalPlayer>(MasterNetAdapter.iOnStartLocalPlayer, obj);
			AddCallback<IOnNetworkDestroy>(MasterNetAdapter.iOnNetworkDestroy, obj);
			AddCallback<IOnJoinRoom>(MasterNetAdapter.iOnJoinRoom, obj);
			AddCallback<IOnJoinRoomFailed>(MasterNetAdapter.iOnJoinRoomFailed, obj);
		}

		public static void UnregisterCallbackInterfaces(Component obj)
		{
			RemoveCallback<INetEvents>(MasterNetAdapter.iNetEvents, obj);
			RemoveCallback<IOnConnect>(MasterNetAdapter.iOnConnect, obj);
			RemoveCallback<IOnStartLocalPlayer>(MasterNetAdapter.iOnStartLocalPlayer, obj);
			RemoveCallback<IOnNetworkDestroy>(MasterNetAdapter.iOnNetworkDestroy, obj);
			RemoveCallback<IOnJoinRoom>(MasterNetAdapter.iOnJoinRoom, obj);
			RemoveCallback<IOnJoinRoomFailed>(MasterNetAdapter.iOnJoinRoomFailed, obj);
		}


		private static void AddCallback<T>(List<Component> list, Component obj)
		{
			if (obj is T && !list.Contains(obj))
				list.Add(obj);
		}

		private static void RemoveCallback<T>(List<Component> list, Component obj)
		{
			if (obj is T && list.Contains(obj))
				list.Remove(obj);
		}
	}
}
