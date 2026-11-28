using System;

namespace NebulaRun
{
	public static class Poly1305
	{
		public static void Mac(byte[] key, byte[] input, int inputLen, byte[] macOut)
		{
			if (key == null || key.Length < 32)
				throw new ArgumentException("Poly1305 requires a 32-byte key", nameof(key));
			if (macOut == null || macOut.Length < 16)
				throw new ArgumentException("Poly1305 output must be 16 bytes", nameof(macOut));

			// Clamp r (bytes 0-15 of key)
			byte[] r = new byte[16];
			Buffer.BlockCopy(key, 0, r, 0, 16);
			r[0] &= 0x0f;
			for (int i = 1; i < 15; i++) r[i] &= 0xfc;
			r[15] &= 0x0f;

			// Extract 5 x 26-bit limbs from clamped r
			uint r0 = (uint)(r[0] | (r[1] << 8) | (r[2] << 16) | ((r[3] & 0x03) << 24));
			uint r1 = (uint)((r[3] >> 2) | (r[4] << 6) | (r[5] << 14) | ((r[6] & 0x0f) << 22));
			uint r2 = (uint)((r[6] >> 4) | (r[7] << 4) | (r[8] << 12) | ((r[9] & 0x3f) << 20));
			uint r3 = (uint)((r[9] >> 6) | (r[10] << 2) | (r[11] << 10) | (r[12] << 18));
			uint r4 = (uint)(r[13] | (r[14] << 8) | (r[15] << 16));

			// s (bytes 16-31 of key)
			uint s0 = Load32(key, 16);
			uint s1 = Load32(key, 20);
			uint s2 = Load32(key, 24);
			uint s3 = Load32(key, 28);

			// h as 5 x 26-bit limbs
			uint h0 = 0, h1 = 0, h2 = 0, h3 = 0, h4 = 0;
			uint carry;

			int offset = 0;
			int remaining = inputLen;

			while (remaining >= 16)
			{
				// Add 16-byte input block to h (as 5 x 26-bit limbs)
				uint m0 = (uint)(input[offset] | (input[offset + 1] << 8) | (input[offset + 2] << 16) | ((input[offset + 3] & 0x03) << 24));
				uint m1 = (uint)((input[offset + 3] >> 2) | (input[offset + 4] << 6) | (input[offset + 5] << 14) | ((input[offset + 6] & 0x0f) << 22));
				uint m2 = (uint)((input[offset + 6] >> 4) | (input[offset + 7] << 4) | (input[offset + 8] << 12) | ((input[offset + 9] & 0x3f) << 20));
				uint m3 = (uint)((input[offset + 9] >> 6) | (input[offset + 10] << 2) | (input[offset + 11] << 10) | (input[offset + 12] << 18));
				uint m4 = (uint)(input[offset + 13] | (input[offset + 14] << 8) | (input[offset + 15] << 16));

				h0 += m0;
				h1 += m1;
				h2 += m2;
				h3 += m3;
				h4 += m4;

				// Carry propagate (26-bit limbs)
				carry = h0 >> 26; h0 &= 0x3ffffff;
				h1 += carry; carry = h1 >> 26; h1 &= 0x3ffffff;
				h2 += carry; carry = h2 >> 26; h2 &= 0x3ffffff;
				h3 += carry; carry = h3 >> 26; h3 &= 0x3ffffff;
				h4 += carry; carry = h4 >> 26; h4 &= 0x3ffffff;
				h0 += carry * 5;

				// Multiply h = h * r mod (2^130 - 5)
				Mul(h0, h1, h2, h3, h4, r0, r1, r2, r3, r4, out h0, out h1, out h2, out h3, out h4);

				offset += 16;
				remaining -= 16;
			}

			if (remaining > 0)
			{
				byte[] block = new byte[16];
				Buffer.BlockCopy(input, offset, block, 0, remaining);
				block[remaining] = 0x01;

				uint m0 = (uint)(block[0] | (block[1] << 8) | (block[2] << 16) | ((block[3] & 0x03) << 24));
				uint m1 = (uint)((block[3] >> 2) | (block[4] << 6) | (block[5] << 14) | ((block[6] & 0x0f) << 22));
				uint m2 = (uint)((block[6] >> 4) | (block[7] << 4) | (block[8] << 12) | ((block[9] & 0x3f) << 20));
				uint m3 = (uint)((block[9] >> 6) | (block[10] << 2) | (block[11] << 10) | (block[12] << 18));
				uint m4 = (uint)(block[13] | (block[14] << 8) | (block[15] << 16));

				h0 += m0;
				h1 += m1;
				h2 += m2;
				h3 += m3;
				h4 += m4;

				carry = h0 >> 26; h0 &= 0x3ffffff;
				h1 += carry; carry = h1 >> 26; h1 &= 0x3ffffff;
				h2 += carry; carry = h2 >> 26; h2 &= 0x3ffffff;
				h3 += carry; carry = h3 >> 26; h3 &= 0x3ffffff;
				h4 += carry; carry = h4 >> 26; h4 &= 0x3ffffff;
				h0 += carry * 5;

				Mul(h0, h1, h2, h3, h4, r0, r1, r2, r3, r4, out h0, out h1, out h2, out h3, out h4);
			}

			// Final reduction (fully reduce h mod p)
			carry = h0 >> 26; h0 &= 0x3ffffff;
			h1 += carry; carry = h1 >> 26; h1 &= 0x3ffffff;
			h2 += carry; carry = h2 >> 26; h2 &= 0x3ffffff;
			h3 += carry; carry = h3 >> 26; h3 &= 0x3ffffff;
			h4 += carry; carry = h4 >> 26; h4 &= 0x3ffffff;
			h0 += carry * 5;

			carry = h0 >> 26; h0 &= 0x3ffffff;
			h1 += carry; carry = h1 >> 26; h1 &= 0x3ffffff;
			h2 += carry; carry = h2 >> 26; h2 &= 0x3ffffff;
			h3 += carry; carry = h3 >> 26; h3 &= 0x3ffffff;
			h4 += carry; carry = h4 >> 26; h4 &= 0x3ffffff;
			h0 += carry * 5;

			// Add s
			h0 += s0;
			h1 += s1;
			h2 += s2;
			// s3 is not used since h is only 5 limbs (130 bits) and s4 = 0

			carry = h0 >> 26; h0 &= 0x3ffffff;
			h1 += carry; carry = h1 >> 26; h1 &= 0x3ffffff;
			h2 += carry; carry = h2 >> 26; h2 &= 0x3ffffff;
			h3 += carry; carry = h3 >> 26; h3 &= 0x3ffffff;
			h4 += carry;

			// Serialize little-endian (128 bits)
			macOut[0] = (byte)h0;
			macOut[1] = (byte)(h0 >> 8);
			macOut[2] = (byte)(h0 >> 16);
			macOut[3] = (byte)((h0 >> 24) | (h1 << 6));
			macOut[4] = (byte)(h1 >> 2);
			macOut[5] = (byte)(h1 >> 10);
			macOut[6] = (byte)(h1 >> 18);
			macOut[7] = (byte)((h1 >> 26) | (h2 << 4));
			macOut[8] = (byte)(h2 >> 4);
			macOut[9] = (byte)(h2 >> 12);
			macOut[10] = (byte)(h2 >> 20);
			macOut[11] = (byte)((h2 >> 28) | (h3 << 2));
			macOut[12] = (byte)(h3 >> 6);
			macOut[13] = (byte)(h3 >> 14);
			macOut[14] = (byte)(h3 >> 22);
			macOut[15] = (byte)h4;
		}

		private static void Mul(uint h0, uint h1, uint h2, uint h3, uint h4,
								uint r0, uint r1, uint r2, uint r3, uint r4,
								out uint out0, out uint out1, out uint out2, out uint out3, out uint out4)
		{
			// 5x5 schoolbook multiplication mod p (2^130 - 5)
			// Each limb is 26 bits, products fit in ulong
			ulong t0 = (ulong)h0 * r0 + 5 * ((ulong)h1 * r4 + (ulong)h2 * r3 + (ulong)h3 * r2 + (ulong)h4 * r1);
			ulong t1 = (ulong)h0 * r1 + (ulong)h1 * r0 + 5 * ((ulong)h2 * r4 + (ulong)h3 * r3 + (ulong)h4 * r2);
			ulong t2 = (ulong)h0 * r2 + (ulong)h1 * r1 + (ulong)h2 * r0 + 5 * ((ulong)h3 * r4 + (ulong)h4 * r3);
			ulong t3 = (ulong)h0 * r3 + (ulong)h1 * r2 + (ulong)h2 * r1 + (ulong)h3 * r0 + 5 * ((ulong)h4 * r4);
			ulong t4 = (ulong)h0 * r4 + (ulong)h1 * r3 + (ulong)h2 * r2 + (ulong)h3 * r1 + (ulong)h4 * r0;

			out0 = (uint)(t0 & 0x3ffffff);
			ulong carry = t0 >> 26;
			t1 += carry;
			out1 = (uint)(t1 & 0x3ffffff);
			carry = t1 >> 26;
			t2 += carry;
			out2 = (uint)(t2 & 0x3ffffff);
			carry = t2 >> 26;
			t3 += carry;
			out3 = (uint)(t3 & 0x3ffffff);
			carry = t3 >> 26;
			t4 += carry;
			out4 = (uint)(t4 & 0x3ffffff);
			carry = t4 >> 26;
			out0 += (uint)(carry * 5);
		}

		private static uint Load32(byte[] data, int offset)
		{
			return (uint)data[offset] | ((uint)data[offset + 1] << 8) | ((uint)data[offset + 2] << 16) | ((uint)data[offset + 3] << 24);
		}
	}
}
