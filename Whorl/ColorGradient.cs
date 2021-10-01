using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Whorl
{
    public class ColorGradient
    {
        public class FloatColor
        {
            public float Alpha { get; set; }
            public float Red { get; set; }
            public float Green { get; set; }
            public float Blue { get; set; }

            public FloatColor()
            {
            }

            public FloatColor(Color color)
            {
                SetFromColor(color);
            }

            public void SetFromColor(Color color)
            {
                Alpha = color.A;
                Red = color.R;
                Green = color.G;
                Blue = color.B;
            }

            public static Color GetColor(FloatColor clr)
            {
                return Color.FromArgb(ClipColor(clr.Alpha), ClipColor(clr.Red), ClipColor(clr.Green), ClipColor(clr.Blue));
            }

            private static float InterpolateColor(float argb1, float argb2, float factor)
            {
                return argb1 + factor * (argb2 - argb1);
            }

            public static FloatColor InterpolateFloatColor(FloatColor color1, FloatColor color2, float factor)
            {
                FloatColor clr = new FloatColor();
                clr.Alpha = InterpolateColor(color1.Alpha, color2.Alpha, factor);
                clr.Red = InterpolateColor(color1.Red, color2.Red, factor);
                clr.Green = InterpolateColor(color1.Green, color2.Green, factor);
                clr.Blue = InterpolateColor(color1.Blue, color2.Blue, factor);
                return clr;
            }

            public static Color InterpolateColor(FloatColor color1, FloatColor color2, float factor)
            {
                FloatColor clr = InterpolateFloatColor(color1, color2, factor);
                return GetColor(clr);
            }
        }

        private class ColorPosition
        {
            public FloatColor Color { get; set; }
            public int Position { get; set; }

            public ColorPosition(Color color, int position)
            {
                Color = new FloatColor();
                Color.SetFromColor(color);
                Position = position;
            }
        }

        //public Color Color1 { get; set; }
        //public Color Color2 { get; set; }
        public int Steps { get; private set; }
        public bool CycleColors { get; private set; }

        private List<ColorPosition> colorPositions { get; set; }
        //private FloatColor CurColor { get; set; }
        //private FloatColor ColorIncrement { get; set; }
        private int currentStep { get; set; }

        public void Initialize(int steps, List<Color> colors, List<float> positions, bool cycleColors = false)
        {
            if (steps <= 0)
                throw new Exception("ColorGradient Steps must be a positive integer.");
            if (colors.Count != positions.Count)
                throw new Exception("ColorGradient colors and positions lists must be same length.");
            if (colors.Count < 2)
                throw new Exception("ColorGradient colors list must have length at least 2.");
            if (positions.First() != 0F)
                throw new Exception("ColorGradient first position must be 0.");
            if (positions.Last() != 1F)
                throw new Exception("ColorGradient last position must be 1.");
            Steps = steps;
            if (Steps == 1)
                cycleColors = false;
            CycleColors = cycleColors;
            int steps1 = CycleColors ? steps / 2 : steps;
            colorPositions = Enumerable.Range(0, colors.Count).Select(
                             i => new ColorPosition(colors[i], (int)Math.Round(positions[i] * steps1)))
                             .OrderBy(cp => cp.Position).ToList();
            currentStep = 0;
        }

        public void Initialize(int steps, Color color1, Color color2, bool cycleColors = false)
        {
            Initialize(steps, new List<Color>() { color1, color2 }, new List<float>() { 0F, 1F }, cycleColors);
            //if (Steps <= 0)
            //    throw new Exception("ColorGradient Steps must be a positive integer.");
            //CurColor = new FloatColor();
            //CurColor.SetFromColor(Color1);
            //ColorIncrement = new FloatColor();
            //float fSteps;
            //if (Steps == 1)
            //    CycleColors = false;
            //if (CycleColors)
            //    fSteps = (float)(Steps / 2);
            //else
            //    fSteps = (float)Steps;
            //ColorIncrement.Alpha = (float)(Color2.A - Color1.A) / fSteps;
            //ColorIncrement.Red = (float)(Color2.R - Color1.R) / fSteps;
            //ColorIncrement.Green = (float)(Color2.G - Color1.G) / fSteps;
            //ColorIncrement.Blue = (float)(Color2.B - Color1.B) / fSteps;
            //CurStep = 0;
        }

        public static int ClipColor(int colorValue)
        {
            return Math.Max(0, Math.Min(255, colorValue));
        }

        public static int ClipColor(float colorValue)
        {
            return ClipColor((int)Math.Round(colorValue));
        }

        private Color GetColorAtStep(int step)
        {
            //Find ColorPosition with position > step:
            int ind = colorPositions.FindIndex(cp => cp.Position > step);
            if (ind == -1)
                ind = colorPositions.Count - 1;
            if (ind == 0)
                throw new Exception("Invalid colorPositions list.");
            ColorPosition clrPos1 = colorPositions[ind - 1];
            ColorPosition clrPos2 = colorPositions[ind];
            int diff = clrPos2.Position - clrPos1.Position;
            if (diff == 0)
                return FloatColor.GetColor(clrPos1.Color);
            else
            {
                float factor = (float)(step - clrPos1.Position) / diff;
                return FloatColor.InterpolateColor(clrPos1.Color, clrPos2.Color, factor);
            }
        }

        public Color GetCurrentColor()
        {
            int step1;
            if (CycleColors && currentStep > Steps / 2)
                step1 = Steps - currentStep;
            else
                step1 = currentStep;
            if (++currentStep > Steps)
                currentStep = 0;
            return GetColorAtStep(step1);
            //Color color = Color.FromArgb(
            //                ClipColor(CurColor.Alpha), ClipColor(CurColor.Red), 
            //                ClipColor(CurColor.Green), ClipColor(CurColor.Blue));
            //if (CycleColors && currentStep == Steps / 2 && currentStep > 0)
            //{
            //    float fSteps = (Steps - currentStep);
            //    if (fSteps != 0)
            //    {
            //        ColorIncrement.Alpha = ((float)Color1.A - CurColor.Alpha) / fSteps;
            //        ColorIncrement.Red = ((float)Color1.R - CurColor.Red) / fSteps;
            //        ColorIncrement.Green = ((float)Color1.G - CurColor.Green) / fSteps;
            //        ColorIncrement.Blue = ((float)Color1.B - CurColor.Blue) / fSteps;
            //    }
            //}
            //CurColor.Alpha += ColorIncrement.Alpha;
            //CurColor.Red += ColorIncrement.Red;
            //CurColor.Green += ColorIncrement.Green;
            //CurColor.Blue += ColorIncrement.Blue;
        }
    }
}
