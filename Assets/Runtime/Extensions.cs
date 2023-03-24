using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomExtensions
{
    public static class DebugExtensions
    {

        // https://gamedev.stackexchange.com/questions/176292/can-i-show-logs-in-unity-with-different-colors-or-different-icon-signs



        // GREY 
        public static string Grey(this string text) => ColorText(text, Color.grey);
        public static string DarkGrey(this string text) => ColorText(text, "A9A9A9");

        // WHITE

        public static string White(this string text) => ColorText(text, Color.white);
        public static string Black(this string text) => ColorText(text, Color.black);

        // RED
        public static string Red(this string text) => ColorText(text, Color.red);
        public static string DarkRed(this string text) => ColorText(text, "8B0000");

        // ORANGE
        public static string Orange(this string text) => ColorText(text, "FFA500");
        public static string DarkOrange(this string text) => ColorText(text, "FF8C00");

        // YELLOW
        public static string Yellow1(this string text) => ColorText(text, "FFFFCC");
        public static string Yellow2(this string text) => ColorText(text, "FFFF99");
        public static string Yellow3(this string text) => ColorText(text, "FFFF66");
        public static string Yellow4(this string text) => ColorText(text, "FFFF00");

        // GREEN
        public static string Green(this string text) => ColorText(text, Color.green);
        public static string DarkGreen(this string text) => ColorText(text, "006400");
        public static string LightGreen(this string text) => ColorText(text, "90EE90");

        // BLUE
        public static string Blue(this string text) => ColorText(text, Color.blue);
        public static string LightBlue(this string text) => ColorText(text, "ADD8E6");

        // CYAN
        public static string Cyan(this string text) => ColorText(text, Color.cyan);
        public static string Aqua(this string text) => ColorText(text, "00FFFF");
        public static string Magenta(this string text) => ColorText(text, Color.magenta);

        // PURPLE
        public static string Purple(this string text) => ColorText(text, "800080");
        public static string MediumPurple(this string text) => ColorText(text, "9370DB");


        // PASTELS
        public static string Violet(this string text) => ColorText(text, "EE82EE");
        public static string Plum(this string text) => ColorText(text, "DDA0DD");

        public static string Peach1(this string text) => ColorText(text, "ffdab9");
        public static string Peach2(this string text) => ColorText(text, "ffc999");
        public static string Peach3(this string text) => ColorText(text, "ffad66");
        public static string Peach4(this string text) => ColorText(text, "ff9233");

        public static string LightPink(this string text) => ColorText(text, "FFB6C1");
        public static string Pink(this string text) => ColorText(text, "ffc0cb");
        public static string Pink1(this string text) => ColorText(text, "ff99aa");
        public static string Pink2(this string text) => ColorText(text, "ff6680");
        public static string Pink3(this string text) => ColorText(text, "ff3355");

        public static string CornflowerBlue(this string text) => ColorText(text, "6495ED");
        public static string Lavender(this string text) => ColorText(text, "E6E6FA");
        public static string Salmon(this string text) => ColorText(text, "FA8072");
        public static string Olive(this string text) => ColorText(text, "808000");
        public static string Gold(this string text) => ColorText(text, "FFD700");




        // https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html
        public static string ColorText(this string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }
        public static string ColorText(this string text, string color)
        {
            return $"<color=#{color}>{text}</color>";
        }




    }
}