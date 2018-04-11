//Copyright 2018, Davin Carten, All rights reserved

using emotitron.Utilities.BitUtilities;
using System.Runtime.InteropServices;
using emotitron.Network.Compression;

namespace emotitron.Network.NST
{
	[StructLayout(LayoutKind.Explicit)]
	public struct CompressedElement
	{
		[FieldOffset(0)]
		public uint x;
		[FieldOffset(4)]
		public uint y;
		[FieldOffset(8)]
		public uint z;

		[FieldOffset(0)]
		public float floatx;
		[FieldOffset(4)]
		public float floaty;
		[FieldOffset(8)]
		public float floatz;

		[FieldOffset(0)]
		public ulong quat;

		public readonly static CompressedElement zero;

		public CompressedElement(uint _x, uint _y, uint _z)
		{
			this = default(CompressedElement);
			x = _x;
			y = _y;
			z = _z;
		}

		public CompressedElement(ushort _x, ushort _y, ushort _z)
		{
			this = default(CompressedElement);
			x = _x;
			y = _y;
			z = _z;
		}

		public CompressedElement(float _x, float _y, float _z)
		{
			this = default(CompressedElement);
			floatx = _x;
			floaty = _y;
			floatz = _z;
		}

		public CompressedElement(ulong _quat)
		{
			this = default(CompressedElement);
			quat = _quat;
		}

		static CompressedElement()
		{
			zero = new CompressedElement() { x = 0, y = 0, z = 0 };
		}

		// Indexer
		public uint this[int index]
		{
			get
			{
				return (index == 0) ? x : (index == 1) ? y : z;
			}
			set
			{
				if (index == 0) x = value;
				else if (index == 1) y = value;
				else if (index == 2) z = value;
			}
		}

		public float GetFloat(int axis)
		{
			return (axis == 0) ? floatx : (axis == 1) ? floaty : floatz;
		}

		public uint GetUInt(int axis)
		{
			return (axis == 0) ? x : (axis == 1) ? y : z;
		}

		public static implicit operator ulong(CompressedElement val)
		{
			return val.quat;
		}

		public static implicit operator CompressedElement(ulong val)
		{
			return new CompressedElement(val);
		}

		/// <summary>
		/// Basic compare of the X, Y, Z, and W values. True if they all match.
		/// </summary>
		public static bool Compare(CompressedElement a, CompressedElement b)
		{
			return (a.x == b.x && a.y == b.y && a.z == b.z);
		}

		public static void Copy(CompressedElement source, CompressedElement target)
		{
			target.x = source.x;
			target.y = source.y;
			target.z = source.z;
		}

		/// <summary>
		/// Get the bit count of the highest bit that is different between two compressed positions. This is the min number of bits that must be sent.
		/// </summary>
		/// <returns></returns>
		public static int HighestDifferentBit(uint a, uint b)
		{
			int highestDiffBit = 0;

			for (int i = 0; i < 32; i++)
				if (i.CompareBit(a, b) == false)
					highestDiffBit = i;

			return highestDiffBit;
		}

		public static CompressedElement operator +(CompressedElement a, CompressedElement b)
		{
			return new CompressedElement((uint)((long)a.x + b.x), (uint)((long)a.y + b.y), (uint)((long)a.z + b.z));
		}

		public static CompressedElement operator -(CompressedElement a, CompressedElement b)
		{
			return new CompressedElement((uint)((long)a.x - b.x), (uint)((long)a.y - b.y), (uint)((long)a.z - b.z));
		}
		public static CompressedElement operator *(CompressedElement a, float b)
		{
			return new CompressedElement((uint)(a.x * b), (uint)(a.y * b), (uint)(a.z * b));
		}

		public static CompressedElement Extrapolate(CompressedElement curr, CompressedElement prev, int divisor = 2)
		{
			return new CompressedElement
				(
				(uint)(curr.x + (((long)curr.x - prev.x)) / divisor),
				(uint)(curr.y + (((long)curr.y - prev.y)) / divisor),
				(uint)(curr.z + (((long)curr.z - prev.z)) / divisor)
				);
		}
		/// <summary>
		/// It is preferable to use the overload that takes and int divisor value than a float, to avoid all float math.
		/// </summary>
		public static CompressedElement Extrapolate (CompressedElement curr, CompressedElement prev, float amount = .5f)
		{
			int divisor = (int)(1f / amount);
			return Extrapolate(curr, prev, divisor);
		}

		/// <summary>
		/// Test changes between two compressed Vector3 elements and return the ideal BitCullingLevel for that change.
		/// </summary>
		public static BitCullingLevel GetGuessableBitCullLevel(CompressedElement oldComp, CompressedElement newComp, FloatRange[] fr, BitCullingLevel maxCullLvl)
		{
			for (BitCullingLevel lvl = maxCullLvl; lvl > 0; lvl--)
			{
				// Starting guess is the new lower bits using the previous upperbits
				if (Compare(newComp, (oldComp.ZeroLowerBits(lvl) | newComp.ZeroUpperBits(lvl))))
					return lvl;
			}
			return BitCullingLevel.NoCulling;
		}

		/// <summary>
		/// Return the smallest bit culling level that will be able to communicate the changes between two compressed elements.
		/// </summary>
		public static BitCullingLevel FindBestBitCullLevel(CompressedElement prev, CompressedElement next, FloatRange[] ar, BitCullingLevel maxCulling)
		{

			if (maxCulling == BitCullingLevel.NoCulling || !TestMatchingUpper(prev, next, ar, BitCullingLevel.DropThird))
				return BitCullingLevel.NoCulling;

			if (maxCulling == BitCullingLevel.DropThird || !TestMatchingUpper(prev, next, ar, BitCullingLevel.DropHalf))
				return BitCullingLevel.DropThird;

			if (maxCulling == BitCullingLevel.DropHalf || prev != next)
				return BitCullingLevel.DropHalf;

			// both values are the same
			return BitCullingLevel.DropAll;
		}

		private static bool TestMatchingUpper(uint a, uint b, int lowerbits)
		{
			return (((a >> lowerbits) << lowerbits) == ((b >> lowerbits) << lowerbits));
		}

		public static bool TestMatchingUpper(CompressedElement prevPos, CompressedElement b, FloatRange[] ar, BitCullingLevel bcl)
		{
			return
				(
				TestMatchingUpper(prevPos.x, b.x, ar[0].BitsAtCullLevel(bcl)) &&
				TestMatchingUpper(prevPos.y, b.y, ar[1].BitsAtCullLevel(bcl)) &&
				TestMatchingUpper(prevPos.z, b.z, ar[2].BitsAtCullLevel(bcl))
				);
		}

		public override string ToString()
		{
			return "[" + quat + "]" + " x:" + x + " y:" + y + " z:" + z;
		}
	}

	public static class CompressedElementExt
	{
		public static System.UInt32[] reusableInts = new System.UInt32[3];

		public static uint[] GetChangeAmount(CompressedElement a, CompressedElement b)
		{
			for (int i = 0; i < 3; i++)
				reusableInts[i] = (System.UInt32)System.Math.Abs(a[i] - b[0]);

			return reusableInts;
		}

		/// <summary>
		/// Alternative to OverwriteUpperBits that attempts to guess the upperbits by seeing if each axis of the new position would be
		/// closer to the old one if the upper bit is incremented by one, two, three etc. Stops trying when it fails to get a better result.
		/// </summary>
		/// <param name="oldcpos">Last best position test against.</param>
		/// <returns>Returns a corrected CompressPos</returns>
		public static CompressedElement GuessUpperBits(this CompressedElement newcpos, CompressedElement oldcpos, FloatRange[] axesranges, BitCullingLevel bcl)
		{
			return new CompressedElement(
				axesranges[0].GuessUpperBits(newcpos[0], oldcpos[0], bcl),
				axesranges[1].GuessUpperBits(newcpos[1], oldcpos[1], bcl),
				axesranges[2].GuessUpperBits(newcpos[2], oldcpos[2], bcl)
				);
		}

		/// <summary>
		/// Replace the upperbits of the first compressed element with the upper bits of the second, using BitCullingLevel as the separation point.
		/// </summary>
		public static CompressedElement OverwriteUpperBits(this CompressedElement low, CompressedElement up, FloatRange[] ranges, BitCullingLevel bcl)
		{
			return new CompressedElement(
				ranges[0].OverwriteUpperBits(low.x, up.x, bcl),
				ranges[1].OverwriteUpperBits(low.y, up.y, bcl),
				ranges[2].OverwriteUpperBits(low.z, up.z, bcl)
				);
		}

		public static CompressedElement ZeroLowerBits(this CompressedElement fullpos, FloatRange[] ranges, BitCullingLevel bcl)
		{
			return new CompressedElement(
				ranges[0].ZeroLowerBits(fullpos.x, bcl),
				ranges[1].ZeroLowerBits(fullpos.y, bcl),
				ranges[2].ZeroLowerBits(fullpos.z, bcl)
				);
		}

		public static CompressedElement ZeroUpperBits(this CompressedElement fullpos, FloatRange[] ranges, BitCullingLevel bcl)
		{
			return new CompressedElement(
				ranges[0].ZeroUpperBits(fullpos.x, bcl),
				ranges[1].ZeroUpperBits(fullpos.y, bcl),
				ranges[2].ZeroUpperBits(fullpos.z, bcl)
				);
		}

	}
}
