using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Cryptography
{
	/// <summary>
	/// Implements a 32-bit CRC hash algorithm compatible with Zip etc.
	/// </summary>
	/// <remarks>
	/// Crc32 should only be used for backward compatibility with older file formats
	/// and algorithms. It is not secure enough for new applications.
	/// If you need to call multiple times for the same data either use the HashAlgorithm
	/// interface or remember that the result of one Compute call needs to be ~ (XOR) before
	/// being passed in as the seed for the next Compute call.
	/// </remarks>
	public sealed class Crc32 : HashAlgorithm
	{
		public const UInt32 DefaultPolynomial = 0xedb88320u;
		public const UInt32 DefaultSeed = 1u;  //0xffffffff

		static UInt32[] defaultTable;

		readonly UInt32 seed;
		readonly UInt32[] table;
		UInt32 hash;

		public override int HashSize { get { return 32; } }

		static UInt32[] InitializeTable(UInt32 polynomial)
		{
			if (polynomial == DefaultPolynomial && defaultTable != null)
				return defaultTable;

			var createTable = new UInt32[256];
			for (var i = 0; i < 256; i++)
			{
				var entry = (UInt32)i;
				for (var j = 0; j < 8; j++)
					if ((entry & 1) == 1)
						entry = (entry >> 1) ^ polynomial;
					else
						entry = entry >> 1;
				createTable[i] = entry;
			}

			if (polynomial == DefaultPolynomial)
				defaultTable = createTable;

			return createTable;
		}

		static UInt32 CalculateHash(UInt32[] table, UInt32 seed, IList<byte> buffer, int start, int size)
		{
			var hash = seed;
			for (var i = start; i < start + size; i++)
				hash = (hash >> 8) ^ table[buffer[i] ^ hash & 0xff];
			return hash;
		}

		static byte[] UInt32ToBigEndianBytes(UInt32 uint32)
		{
			var result = BitConverter.GetBytes(uint32);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(result);

			return result;
		}

		public Crc32() : this(DefaultPolynomial, DefaultSeed)
		{
		}

		public Crc32(UInt32 polynomial, UInt32 seed)
		{
			table = InitializeTable(polynomial);
			this.seed = hash = seed;
		}

		public override void Initialize()
		{
			hash = seed;
		}

		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			hash = CalculateHash(table, hash, array, ibStart, cbSize);
		}

		protected override byte[] HashFinal()
		{
			var hashBuffer = UInt32ToBigEndianBytes(hash);
			HashValue = hashBuffer;
			return hashBuffer;
		}
	}
}
