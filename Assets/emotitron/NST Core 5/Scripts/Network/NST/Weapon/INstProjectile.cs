//Copyright 2018, Davin Carten, All rights reserved

namespace emotitron.Network.NST.Weapon
{
	/// <summary>
	/// Interface use by weapons to pass information to projectiles
	/// </summary>
	public interface INstProjectile
	{
		NetworkSyncTransform OwnerNst { set; }

	}
}


