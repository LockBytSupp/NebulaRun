/*
Unrolled XChaCha8
*/

using System;
using System.Security.Cryptography;

namespace NebulaRun
{
    public static class XChaCha8
    {
        private static void QuarterRound(uint[] state, int a, int b, int c, int d)
        {
            state[a] += state[b]; state[d] ^= state[a]; state[d] = Rol(state[d], 16);
            state[c] += state[d]; state[b] ^= state[c]; state[b] = Rol(state[b], 12);
            state[a] += state[b]; state[d] ^= state[a]; state[d] = Rol(state[d], 8);
            state[c] += state[d]; state[b] ^= state[c]; state[b] = Rol(state[b], 7);
        }

        private static uint Rol(uint x, int y)
        {
            return (x << y) | (x >> (32 - y));
        }

        private static void ChaChaBlock(uint[] input, uint[] output, int rounds)
        {
            output[0] = input[0];
            output[1] = input[1];
            output[2] = input[2];
            output[3] = input[3];
            output[4] = input[4];
            output[5] = input[5];
            output[6] = input[6];
            output[7] = input[7];
            output[8] = input[8];
            output[9] = input[9];
            output[10] = input[10];
            output[11] = input[11];
            output[12] = input[12];
            output[13] = input[13];
            output[14] = input[14];
            output[15] = input[15];

            for (int r = 0; r < rounds; r += 2)
            {
                QuarterRound(output, 0, 4, 8, 12);
                QuarterRound(output, 1, 5, 9, 13);
                QuarterRound(output, 2, 6, 10, 14);
                QuarterRound(output, 3, 7, 11, 15);

                QuarterRound(output, 0, 5, 10, 15);
                QuarterRound(output, 1, 6, 11, 12);
                QuarterRound(output, 2, 7, 8, 13);
                QuarterRound(output, 3, 4, 9, 14);
            }

            output[0] += input[0];
            output[1] += input[1];
            output[2] += input[2];
            output[3] += input[3];
            output[4] += input[4];
            output[5] += input[5];
            output[6] += input[6];
            output[7] += input[7];
            output[8] += input[8];
            output[9] += input[9];
            output[10] += input[10];
            output[11] += input[11];
            output[12] += input[12];
            output[13] += input[13];
            output[14] += input[14];
            output[15] += input[15];
        }

        private static void ToBytes(uint[] input, byte[] output)
        {
            byte[] b0 = BitConverter.GetBytes(input[0]);
            byte[] b1 = BitConverter.GetBytes(input[1]);
            byte[] b2 = BitConverter.GetBytes(input[2]);
            byte[] b3 = BitConverter.GetBytes(input[3]);
            byte[] b4 = BitConverter.GetBytes(input[4]);
            byte[] b5 = BitConverter.GetBytes(input[5]);
            byte[] b6 = BitConverter.GetBytes(input[6]);
            byte[] b7 = BitConverter.GetBytes(input[7]);
            byte[] b8 = BitConverter.GetBytes(input[8]);
            byte[] b9 = BitConverter.GetBytes(input[9]);
            byte[] b10 = BitConverter.GetBytes(input[10]);
            byte[] b11 = BitConverter.GetBytes(input[11]);
            byte[] b12 = BitConverter.GetBytes(input[12]);
            byte[] b13 = BitConverter.GetBytes(input[13]);
            byte[] b14 = BitConverter.GetBytes(input[14]);
            byte[] b15 = BitConverter.GetBytes(input[15]);

            Buffer.BlockCopy(b0, 0, output, 0, 4);
            Buffer.BlockCopy(b1, 0, output, 4, 4);
            Buffer.BlockCopy(b2, 0, output, 8, 4);
            Buffer.BlockCopy(b3, 0, output, 12, 4);
            Buffer.BlockCopy(b4, 0, output, 16, 4);
            Buffer.BlockCopy(b5, 0, output, 20, 4);
            Buffer.BlockCopy(b6, 0, output, 24, 4);
            Buffer.BlockCopy(b7, 0, output, 28, 4);
            Buffer.BlockCopy(b8, 0, output, 32, 4);
            Buffer.BlockCopy(b9, 0, output, 36, 4);
            Buffer.BlockCopy(b10, 0, output, 40, 4);
            Buffer.BlockCopy(b11, 0, output, 44, 4);
            Buffer.BlockCopy(b12, 0, output, 48, 4);
            Buffer.BlockCopy(b13, 0, output, 52, 4);
            Buffer.BlockCopy(b14, 0, output, 56, 4);
            Buffer.BlockCopy(b15, 0, output, 60, 4);
        }

        public static void DeriveSubkey(byte[] key, byte[] nonce16, byte[] subkeyOut)
        {
            uint[] state = new uint[16];
            state[0] = 0x61707865;
            state[1] = 0x3320646e;
            state[2] = 0x79622d32;
            state[3] = 0x6b206574;

            state[4] = BitConverter.ToUInt32(key, 0);
            state[5] = BitConverter.ToUInt32(key, 4);
            state[6] = BitConverter.ToUInt32(key, 8);
            state[7] = BitConverter.ToUInt32(key, 12);
            state[8] = BitConverter.ToUInt32(key, 16);
            state[9] = BitConverter.ToUInt32(key, 20);
            state[10] = BitConverter.ToUInt32(key, 24);
            state[11] = BitConverter.ToUInt32(key, 28);

            state[12] = BitConverter.ToUInt32(nonce16, 0);
            state[13] = BitConverter.ToUInt32(nonce16, 4);
            state[14] = BitConverter.ToUInt32(nonce16, 8);
            state[15] = BitConverter.ToUInt32(nonce16, 12);

            uint[] working = new uint[16];
            ChaChaBlock(state, working, 20);

            byte[] b0 = BitConverter.GetBytes(working[0]);
            byte[] b1 = BitConverter.GetBytes(working[1]);
            byte[] b2 = BitConverter.GetBytes(working[2]);
            byte[] b3 = BitConverter.GetBytes(working[3]);
            byte[] b12 = BitConverter.GetBytes(working[12]);
            byte[] b13 = BitConverter.GetBytes(working[13]);
            byte[] b14 = BitConverter.GetBytes(working[14]);
            byte[] b15 = BitConverter.GetBytes(working[15]);

            Buffer.BlockCopy(b0, 0, subkeyOut, 0, 4);
            Buffer.BlockCopy(b1, 0, subkeyOut, 4, 4);
            Buffer.BlockCopy(b2, 0, subkeyOut, 8, 4);
            Buffer.BlockCopy(b3, 0, subkeyOut, 12, 4);
            Buffer.BlockCopy(b12, 0, subkeyOut, 16, 4);
            Buffer.BlockCopy(b13, 0, subkeyOut, 20, 4);
            Buffer.BlockCopy(b14, 0, subkeyOut, 24, 4);
            Buffer.BlockCopy(b15, 0, subkeyOut, 28, 4);
        }

        public static void Crypt(byte[] key32, byte[] nonce24, byte[] input, byte[] output)
        {
            byte[] subkey = new byte[32];
            byte[] nonce16 = new byte[16];
            Buffer.BlockCopy(nonce24, 0, nonce16, 0, 16);
            DeriveSubkey(key32, nonce16, subkey);

            uint[] state = new uint[16];
            state[0] = 0x61707865;
            state[1] = 0x3320646e;
            state[2] = 0x79622d32;
            state[3] = 0x6b206574;

            state[4] = BitConverter.ToUInt32(subkey, 0);
            state[5] = BitConverter.ToUInt32(subkey, 4);
            state[6] = BitConverter.ToUInt32(subkey, 8);
            state[7] = BitConverter.ToUInt32(subkey, 12);
            state[8] = BitConverter.ToUInt32(subkey, 16);
            state[9] = BitConverter.ToUInt32(subkey, 20);
            state[10] = BitConverter.ToUInt32(subkey, 24);
            state[11] = BitConverter.ToUInt32(subkey, 28);

            state[12] = 0;
            state[13] = 0;
            state[14] = BitConverter.ToUInt32(nonce24, 16);
            state[15] = BitConverter.ToUInt32(nonce24, 20);

            uint[] block = new uint[16];
            byte[] streamBlock = new byte[64];

            int inputLen = input.Length;
            int offset = 0;
            uint counter = 0;

            while (offset < inputLen)
            {
                state[12] = counter++;
                ChaChaBlock(state, block, 8); // XChaCha8 (8 rounds)
                ToBytes(block, streamBlock);

                int chunk = Math.Min(64, inputLen - offset);
                output[offset] = (byte)(input[offset] ^ streamBlock[0]);
                if (chunk > 1) output[offset + 1] = (byte)(input[offset + 1] ^ streamBlock[1]);
                if (chunk > 2) output[offset + 2] = (byte)(input[offset + 2] ^ streamBlock[2]);
                if (chunk > 3) output[offset + 3] = (byte)(input[offset + 3] ^ streamBlock[3]);
                if (chunk > 4) output[offset + 4] = (byte)(input[offset + 4] ^ streamBlock[4]);
                if (chunk > 5) output[offset + 5] = (byte)(input[offset + 5] ^ streamBlock[5]);
                if (chunk > 6) output[offset + 6] = (byte)(input[offset + 6] ^ streamBlock[6]);
                if (chunk > 7) output[offset + 7] = (byte)(input[offset + 7] ^ streamBlock[7]);
                if (chunk > 8) output[offset + 8] = (byte)(input[offset + 8] ^ streamBlock[8]);
                if (chunk > 9) output[offset + 9] = (byte)(input[offset + 9] ^ streamBlock[9]);
                if (chunk > 10) output[offset + 10] = (byte)(input[offset + 10] ^ streamBlock[10]);
                if (chunk > 11) output[offset + 11] = (byte)(input[offset + 11] ^ streamBlock[11]);
                if (chunk > 12) output[offset + 12] = (byte)(input[offset + 12] ^ streamBlock[12]);
                if (chunk > 13) output[offset + 13] = (byte)(input[offset + 13] ^ streamBlock[13]);
                if (chunk > 14) output[offset + 14] = (byte)(input[offset + 14] ^ streamBlock[14]);
                if (chunk > 15) output[offset + 15] = (byte)(input[offset + 15] ^ streamBlock[15]);
                if (chunk > 16) output[offset + 16] = (byte)(input[offset + 16] ^ streamBlock[16]);
                if (chunk > 17) output[offset + 17] = (byte)(input[offset + 17] ^ streamBlock[17]);
                if (chunk > 18) output[offset + 18] = (byte)(input[offset + 18] ^ streamBlock[18]);
                if (chunk > 19) output[offset + 19] = (byte)(input[offset + 19] ^ streamBlock[19]);
                if (chunk > 20) output[offset + 20] = (byte)(input[offset + 20] ^ streamBlock[20]);
                if (chunk > 21) output[offset + 21] = (byte)(input[offset + 21] ^ streamBlock[21]);
                if (chunk > 22) output[offset + 22] = (byte)(input[offset + 22] ^ streamBlock[22]);
                if (chunk > 23) output[offset + 23] = (byte)(input[offset + 23] ^ streamBlock[23]);
                if (chunk > 24) output[offset + 24] = (byte)(input[offset + 24] ^ streamBlock[24]);
                if (chunk > 25) output[offset + 25] = (byte)(input[offset + 25] ^ streamBlock[25]);
                if (chunk > 26) output[offset + 26] = (byte)(input[offset + 26] ^ streamBlock[26]);
                if (chunk > 27) output[offset + 27] = (byte)(input[offset + 27] ^ streamBlock[27]);
                if (chunk > 28) output[offset + 28] = (byte)(input[offset + 28] ^ streamBlock[28]);
                if (chunk > 29) output[offset + 29] = (byte)(input[offset + 29] ^ streamBlock[29]);
                if (chunk > 30) output[offset + 30] = (byte)(input[offset + 30] ^ streamBlock[30]);
                if (chunk > 31) output[offset + 31] = (byte)(input[offset + 31] ^ streamBlock[31]);
                if (chunk > 32) output[offset + 32] = (byte)(input[offset + 32] ^ streamBlock[32]);
                if (chunk > 33) output[offset + 33] = (byte)(input[offset + 33] ^ streamBlock[33]);
                if (chunk > 34) output[offset + 34] = (byte)(input[offset + 34] ^ streamBlock[34]);
                if (chunk > 35) output[offset + 35] = (byte)(input[offset + 35] ^ streamBlock[35]);
                if (chunk > 36) output[offset + 36] = (byte)(input[offset + 36] ^ streamBlock[36]);
                if (chunk > 37) output[offset + 37] = (byte)(input[offset + 37] ^ streamBlock[37]);
                if (chunk > 38) output[offset + 38] = (byte)(input[offset + 38] ^ streamBlock[38]);
                if (chunk > 39) output[offset + 39] = (byte)(input[offset + 39] ^ streamBlock[39]);
                if (chunk > 40) output[offset + 40] = (byte)(input[offset + 40] ^ streamBlock[40]);
                if (chunk > 41) output[offset + 41] = (byte)(input[offset + 41] ^ streamBlock[41]);
                if (chunk > 42) output[offset + 42] = (byte)(input[offset + 42] ^ streamBlock[42]);
                if (chunk > 43) output[offset + 43] = (byte)(input[offset + 43] ^ streamBlock[43]);
                if (chunk > 44) output[offset + 44] = (byte)(input[offset + 44] ^ streamBlock[44]);
                if (chunk > 45) output[offset + 45] = (byte)(input[offset + 45] ^ streamBlock[45]);
                if (chunk > 46) output[offset + 46] = (byte)(input[offset + 46] ^ streamBlock[46]);
                if (chunk > 47) output[offset + 47] = (byte)(input[offset + 47] ^ streamBlock[47]);
                if (chunk > 48) output[offset + 48] = (byte)(input[offset + 48] ^ streamBlock[48]);
                if (chunk > 49) output[offset + 49] = (byte)(input[offset + 49] ^ streamBlock[49]);
                if (chunk > 50) output[offset + 50] = (byte)(input[offset + 50] ^ streamBlock[50]);
                if (chunk > 51) output[offset + 51] = (byte)(input[offset + 51] ^ streamBlock[51]);
                if (chunk > 52) output[offset + 52] = (byte)(input[offset + 52] ^ streamBlock[52]);
                if (chunk > 53) output[offset + 53] = (byte)(input[offset + 53] ^ streamBlock[53]);
                if (chunk > 54) output[offset + 54] = (byte)(input[offset + 54] ^ streamBlock[54]);
                if (chunk > 55) output[offset + 55] = (byte)(input[offset + 55] ^ streamBlock[55]);
                if (chunk > 56) output[offset + 56] = (byte)(input[offset + 56] ^ streamBlock[56]);
                if (chunk > 57) output[offset + 57] = (byte)(input[offset + 57] ^ streamBlock[57]);
                if (chunk > 58) output[offset + 58] = (byte)(input[offset + 58] ^ streamBlock[58]);
                if (chunk > 59) output[offset + 59] = (byte)(input[offset + 59] ^ streamBlock[59]);
                if (chunk > 60) output[offset + 60] = (byte)(input[offset + 60] ^ streamBlock[60]);
                if (chunk > 61) output[offset + 61] = (byte)(input[offset + 61] ^ streamBlock[61]);
                if (chunk > 62) output[offset + 62] = (byte)(input[offset + 62] ^ streamBlock[62]);
                if (chunk > 63) output[offset + 63] = (byte)(input[offset + 63] ^ streamBlock[63]);

                offset += chunk;
            }
        }
    }
}
