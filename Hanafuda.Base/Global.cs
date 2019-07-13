using System;
using System.Collections.Generic;
// ReSharper disable All
/*
 * Todo:
 *  - Dynamische Generierung der Card Skins
 */

namespace Hanafuda.Base
{
    public static class Global
    {
        public static Action NoAction = () => { };
        public static int MovingCards;
        public static int Turn = -1;
        public static List<Card> allCards = new List<Card>();
        public static List<Yaku> allYaku = new List<Yaku>();
        public static List<string> Spielverlauf = new List<string>();

        private static System.Diagnostics.Process process;
        private static readonly bool AllowLog = false;

        public static void Log(string output, bool allow = false)
        {
            if (AllowLog && allow)
            {
                process.StandardInput.WriteLine(output);
            }
        }
    }
}