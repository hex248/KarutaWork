using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;

namespace KarutaWorkBot
{
    public class Card
    {
        public string code;
        public int print;
        public string show;
        public string name;
        public SocketUser user;
        public string quality;

        public EmbedImage cardImage;
        public string cardImageUrl;
        public EmbedThumbnail charImage;
        public string charImageUrl;

        #region Work Info
        public int effort;
        public int baseValue = 0;
        public int purity;
        public int wellness;
        public int quickness;
        public int style;
        public int grabber;
        public int dropper;
        public int toughness;
        public int vanity;
        #endregion

        #region Efforts
        public int mint;
        public int excellent;
        public int good;
        public int poor;
        public int damaged;
        #endregion


        public Card(string code, int print, string show, string name)
        {
            this.code = code;
            this.print = print;
            this.show = show;
            this.name = Regex.Replace(name.Trim(' ').Trim('*'), @"\s+", " ");
        }
    }
}
