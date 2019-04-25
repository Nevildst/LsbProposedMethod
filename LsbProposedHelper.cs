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

                    int r = Embed2Bit(pixel.R, ref bitSeq, ref ibitSeq);
                    int g = Embed2Bit(pixel.G, ref bitSeq, ref ibitSeq);
                    int b = Embed2Bit(pixel.B, ref bitSeq, ref ibitSeq);

                    bitmap.SetPixel(i, j, Color.FromArgb(r, g, b));

                    if (ibitSeq == bitSeq.Length + 8)
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

        public static string ExtractText(Bitmap image)
        {
            string result = "", tmp = "";

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color pxl = image.GetPixel(i, j);

                    tmp += Get2BitRightMost(pxl.R) + Get2BitRightMost(pxl.G) + Get2BitRightMost(pxl.B);
                    if (tmp.Length >= 16)
                    {
                        string sub = tmp.Substring(0, 16);
                        if (sub == "0000000000000000")
                            return result;

                        char c1 = (char)Convert.ToByte(sub.Substring(0, 8), 2);
                        char c2 = (char)Convert.ToByte(sub.Substring(8, 8), 2);
                        result += $"{c1}{c2}";

                        tmp = tmp.Substring(16, tmp.Length - 16);
                    }
                }
            }

            return result;
        }

        private static int Embed2Bit(byte b, ref string msg, ref int ibitSeq)
        {
            string tmp = Convert.ToString(b, 2).PadLeft(8, '0');
            if (ibitSeq < msg.Length)
            {
                tmp = tmp.Substring(0, 6) + msg.Substring(ibitSeq, 2);
            }
            else
            {
                tmp = tmp.Substring(0, 6) + "00";
            }

            ibitSeq += 2;

            return Convert.ToByte(tmp, 2);
        }

        private static string Get2BitRightMost(byte b)
        {
            return Convert.ToString(b, 2).PadLeft(8, '0').Substring(6, 2);
        }

        private static string GetBitSequence(string encodeMessage)
        {
            string msg = "";
            foreach (char ch in encodeMessage)
                msg += Convert.ToString((int)ch, 2).PadLeft(8, '0');

            return msg;
        }
    }
}
