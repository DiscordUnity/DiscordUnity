using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiscordUnity
{
    internal static class Utils
    {
        #region Internal Methods
        public static Color GetColorFromInt(uint color)
        {
            return new Color((byte)(color >> 16) / 255f, (byte)(color >> 8) / 255f, (byte)(color >> 0) / 255f, (byte)(color >> 24) / 255f);
        }

        public static uint GetIntFromColor(Color color)
        {
            return (uint)(((byte)(color.a * 255f) << 24) | ((byte)(color.r * 255f) << 16) | ((byte)(color.g * 255f) << 8) | ((byte)(color.b * 255f) << 0));
        }
        
        public static byte[] FloatsToBytes(float[] floats)
        {
            byte[] bytes = new byte[floats.Length * 2];

            for (int x = 0; x < floats.Length; x++)
            {
                short sample = (short)(floats[x] * 32768f);
                byte[] bits = BitConverter.GetBytes(sample);
                bytes[x * 2] = bits[0];
                bytes[x * 2 + 1] = bits[1];
            }

            return bytes;
        }

        public static float[] BytesToFloats(byte[] bytes)
        {
            float[] floats = new float[bytes.Length / 2];

            for (int x = 0; x < bytes.Length / 2; x++)
            {
                short sample = BitConverter.ToInt16(bytes, x * 2);
                floats[x] = sample / 32768f;
            }

            return floats;
        }

        public static string[] GetRoleIDs(DiscordRole[] roles)
        {
            List<string> roleIDs = new List<string>();

            foreach (DiscordRole role in roles)
            {
                roleIDs.Add(role.ID);
            }

            return roleIDs.ToArray();
        }

        public static uint GetPermissions(DiscordPermission[] permissions)
        {
            uint raw = 0;

            foreach (DiscordPermission permission in permissions)
            {
                if (((raw >> (byte)permission) & 1) == 0)
                {
                    raw |= (uint)(1 << (byte)permission);
                }
            }

            return raw;
        }

        public static DiscordPermission[] GetPermissions(uint raw)
        {
            if (raw == 0) return new DiscordPermission[0];

            List<DiscordPermission> perm = new List<DiscordPermission>();
            var allPermissions = Enum.GetValues(typeof(DiscordPermission));

            foreach (DiscordPermission permission in allPermissions)
            {
                if (((raw >> (byte)permission) & 1) != 0)
                {
                    perm.Add(permission);
                }
            }
            return perm.ToArray();
        }
        #endregion
    }
}