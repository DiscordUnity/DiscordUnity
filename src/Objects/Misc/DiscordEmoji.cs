using System.Collections;
using UnityEngine;

namespace DiscordUnity
{
    public class DiscordEmoji
    {
        /// <summary> The name of this emoji. </summary>
        public string name { get; internal set; }
        /// <summary> The texture of this emoji. </summary>
        public Texture2D emoji { get; internal set; }
        /// <summary> The state of this emoji's texture. </summary>
        public TextureState emojiState { get; internal set; }

        internal string ID;
        internal DiscordClient client;

        internal DiscordEmoji(DiscordClient parent, DiscordEmojiJSON e)
        {
            ID = e.id;
            name = e.name;
            client = parent;
            emojiState = TextureState.Unloaded;
        }

        public IEnumerator GetEmoji(DiscordTextureCallback callback)
        {
            if (emojiState == TextureState.Unloaded)
            {
                emojiState = TextureState.Loading;
                Debug.Log("https://cdn.discordapp.com/emojis/" + ID + ".png");
                WWW www = new WWW("https://cdn.discordapp.com/emojis/" + ID + ".png");
                yield return www;
                Texture2D result = www.texture;

                if (result == null)
                {
                    emojiState = TextureState.NoTexture;
                    yield break;
                }

                emoji = www.texture;
                emojiState = TextureState.Loaded;
                client.unityInvoker.Enqueue(() => callback(client, emoji, new DiscordError()));
            }
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(DiscordEmoji a, DiscordEmoji b)
        {
            if ((object)a == null && (object)b == null) return true;
            if ((object)a == null && (object)b != null) return false;
            if ((object)a != null && (object)b == null) return false;
            return a.ID == b.ID;
        }

        public static bool operator !=(DiscordEmoji a, DiscordEmoji b)
        {
            return !(a == b);
        }
    }
}