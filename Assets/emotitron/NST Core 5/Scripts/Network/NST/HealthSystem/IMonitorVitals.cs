﻿//Copyright 2018, Davin Carten, All rights reserved

namespace emotitron.Network.NST.HealthSystem
{
	/// <summary>
	/// Callback interface for the healthsystem.
	/// </summary>
	public interface IMonitorVitals
	{
		void OnVitalsChange(IVitals vitals);
	}
}

