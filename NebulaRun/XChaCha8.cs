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
        for (int i = 0; i < 16; i++)
        {
            output[i] = input[i];
        }

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

        for (int i = 0; i < 16; i++)
        {
            output[i] += input[i];
        }
    }

    private static void ToBytes(uint[] input, byte[] output)
    {
        for (int i = 0; i < 16; i++)
        {
            byte[] bytes = BitConverter.GetBytes(input[i]);
            Buffer.BlockCopy(bytes, 0, output, i * 4, 4);
        }
    }

    public static void DeriveSubkey(byte[] key, byte[] nonce16, byte[] subkeyOut)
    {
        uint[] state = new uint[16];
        state[0] = 0x61707865;
        state[1] = 0x3320646e;
        state[2] = 0x79622d32;
        state[3] = 0x6b206574;

        for (int i = 0; i < 8; i++)
        {
            state[4 + i] = BitConverter.ToUInt32(key, i * 4);
        }
        for (int i = 0; i < 4; i++)
        {
            state[12 + i] = BitConverter.ToUInt32(nonce16, i * 4);
        }

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

        for (int i = 0; i < 8; i++)
        {
            state[4 + i] = BitConverter.ToUInt32(subkey, i * 4);
        }

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
            for (int i = 0; i < chunk; i++)
            {
                output[offset + i] = (byte)(input[offset + i] ^ streamBlock[i]);
            }
            offset += chunk;
        }
    }
}
}
