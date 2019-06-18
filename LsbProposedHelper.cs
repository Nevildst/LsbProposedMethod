using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Steganography
{
    public class LsbProposedHelper
    {
        public static List<int> Vector { get; set; } = new List<int>();

        public static Bitmap EmbedText(string encodeMessage, Bitmap bitmap)
        {
            if (bitmap == null || encodeMessage == "")
            {
                return bitmap;
            }

            if (encodeMessage.Length * 8 > bitmap.Height * bitmap.Width * 6)
            {
                return bitmap;
            }

            string bitSeq = GetBitSequence(encodeMessage);

            int ibitSeq = 0;
            bool isDone = false;
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color pixel = bitmap.GetPixel(i, j);

                    int r = Embed2Bit(pixel.R, TypeElementPixel.Red, ref bitSeq, ref ibitSeq);
                    int g = Embed2Bit(pixel.G, TypeElementPixel.Green, ref bitSeq, ref ibitSeq);
                    int b = Embed2Bit(pixel.B, TypeElementPixel.Blue, ref bitSeq, ref ibitSeq);

                    bitmap.SetPixel(i, j, Color.FromArgb(r, g, b));

                    if (ibitSeq == bitSeq.Length)
                    {
                        isDone = true;
                        break;
                    }
                }

                if (isDone)
                    break;
            }

            return bitmap;
        }

        public static string ExtractText(Bitmap image, List<int> vector)
        {
            string result = "", tmp = "";

            if (vector.Count == 0)
            {
                return string.Empty;
            }

            int ivector = 0, lVector = vector.Count;
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color pxl = image.GetPixel(i, j);

                    if (ivector >= lVector)
                        return result;

                    int ihiddenInR = (ivector + 1 > lVector) ? 4 : vector[ivector++];
                    int ihiddenInG = (ivector + 1 > lVector) ? 4 : vector[ivector++];
                    int ihiddenInB = (ivector + 1 > lVector) ? 4 : vector[ivector++];

                    tmp += Get2BitHidden(pxl.R, ihiddenInR)
                         + Get2BitHidden(pxl.G, ihiddenInG)
                         + Get2BitHidden(pxl.B, ihiddenInB);

                    if (tmp.Length >= 8)
                    {
                        string sub = tmp.Substring(0, 8);

                        char c1 = (char)Convert.ToByte(sub.Substring(0, 8), 2);
                        result += $"{c1}";

                        tmp = tmp.Substring(8, tmp.Length - 8);
                    }
                }
            }

            return result;
        }

        private static int Embed2Bit(byte b, TypeElementPixel type, ref string msg, ref int ibitSeq)
        {
            string tmp = Convert.ToString(b, 2).PadLeft(8, '0');
            var subString = msg.Substring(ibitSeq, 2);

            if (tmp.Substring(0, 2) == subString)
            {
                Vector.Add(1);
                InforAnalysisEmbed.Instance.IncreaseValue(type, true);
            }
            else if (tmp.Substring(2, 2) == subString)
            {
                Vector.Add(2);
                InforAnalysisEmbed.Instance.IncreaseValue(type, true);
            }
            else if (tmp.Substring(4, 2) == subString)
            {
                Vector.Add(3);
                InforAnalysisEmbed.Instance.IncreaseValue(type, true);
            }
            else
            {
                if (tmp.Substring(6, 2) == subString)
                {
                    InforAnalysisEmbed.Instance.IncreaseValue(type, true);
                }
                else
                {
                    InforAnalysisEmbed.Instance.IncreaseValue(type);
                }

                tmp = tmp.Substring(0, 6) + subString;
                Vector.Add(4);
            }


            ibitSeq += 2;

            return Convert.ToByte(tmp, 2);
        }

        private static string Get2BitHidden(byte b, int ipixelElement)
        {
            string result = "";
            switch (ipixelElement)
            {
                case 1:
                    result = Convert.ToString(b, 2).PadLeft(8, '0').Substring(0, 2);
                    break;
                case 2:
                    result = Convert.ToString(b, 2).PadLeft(8, '0').Substring(2, 2);
                    break;
                case 3:
                    result = Convert.ToString(b, 2).PadLeft(8, '0').Substring(4, 2);
                    break;
                case 4:
                    result = Convert.ToString(b, 2).PadLeft(8, '0').Substring(6, 2);
                    break;
            }
            return result;
        }

        private static string GetBitSequence(string encodeMessage)
        {
            string msg = "";
            foreach (char ch in encodeMessage)
                msg += Convert.ToString((int)ch, 2).PadLeft(8, '0');

            return msg;
        }

        public static void ResetVector()
        {
            Vector = new List<int>();
        }
    }
}
