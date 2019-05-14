using System;
using System.Collections.Generic;

namespace Steganography
{
    public class InforAnalysisEmbed
    {
        private static readonly InforAnalysisEmbed _instance = new InforAnalysisEmbed();
        
        private InforAnalysisEmbed()
        {

        }
        static InforAnalysisEmbed()
        {

        }

        public static InforAnalysisEmbed Instance => _instance;

        private Dictionary<TypeElementPixel, ResultAnalysis> Result { set; get; } = new Dictionary<TypeElementPixel, ResultAnalysis> {
            { TypeElementPixel.Red, new ResultAnalysis() },
            { TypeElementPixel.Green, new ResultAnalysis()},
            { TypeElementPixel.Blue, new ResultAnalysis()}
        };

        public void IncreaseValue(TypeElementPixel type, bool isIdentical = false)
        {
            if (isIdentical)
            {
                Result[type].Identical += 1;
            }
            else
            {
                Result[type].NoIdentical += 1;
            }
        }

        public void ResetResult()
        {
            Result  = new Dictionary<TypeElementPixel, ResultAnalysis> {
                { TypeElementPixel.Red, new ResultAnalysis() },
                { TypeElementPixel.Green, new ResultAnalysis()},
                { TypeElementPixel.Blue, new ResultAnalysis()}
            };
        }
    }

    public enum TypeElementPixel
    {
        Red, Green, Blue
    }

    public class ResultAnalysis
    {
        public double Identical { get; set; }
        public double NoIdentical { get; set; }
        public int RatioIDT => (Identical == 0) ? 0 : (int)Math.Round(Identical / (Identical + NoIdentical));
        public int RatioNoIDT => (NoIdentical == 0) ? 0 : (int)Math.Round(NoIdentical / (Identical + NoIdentical));
        public int NetRatio => RatioIDT + RatioNoIDT;
    }
}
