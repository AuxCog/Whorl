using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public class DrawDesign
    {
        public static void DrawPatterns(Graphics g, IRenderCaller caller, IEnumerable<Pattern> patterns, Size imgSize,
                                        bool computeRandom = false, float textureScale = 0, bool checkTilePattern = true, 
                                        bool enableCache = true, bool draftMode = false)
        {
            foreach (Pattern pattern in patterns)
            {
                if (caller != null && caller.CancelRender)
                    break;
                if (checkTilePattern && pattern.PatternTileInfo.TilePattern)
                {
                    pattern.DrawTiledPatterns(g, caller, imgSize, computeRandom, draftMode, textureScale, enableCache: enableCache);
                }
                else
                {
                    DrawPattern(pattern, g, caller, textureScale, computeRandom, enableCache: enableCache, draftMode: draftMode);
                }
            }
        }

        public static void RedrawPatterns(Bitmap bitmap, WhorlDesign design, IRenderCaller caller, float textureScale,
                                          bool excludeSelected = false,
                                          bool computeRandom = false,
                                          IEnumerable<Pattern> overridePatterns = null, 
                                          bool enableCache = true, bool draftMode = false)
        {
            var patterns = overridePatterns ?? design.EnabledPatterns;
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                DrawPatterns(g, caller,
                             patterns.Where(
                             ptn => !(excludeSelected && ptn.Selected)
                                 && (ptn.DesignLayer == null || ptn.DesignLayer.Visible)),
                             bitmap.Size,
                             computeRandom, textureScale, enableCache: enableCache, draftMode: draftMode);
            }
        }

        public static void ApplyTextureScale(Pattern pattern, float textureScale)
        {
            var pathPattern = pattern as PathPattern;
            if (pathPattern != null && pathPattern.PathRibbon != null)
                pattern = pathPattern.PathRibbon;
            foreach (var patternLayer in pattern.PatternLayers.PatternLayers)
            {
                if (patternLayer.FillInfo is TextureFillInfo)
                {
                    if (patternLayer.FillInfo.FillBrush == null)
                        patternLayer.FillInfo.CreateFillBrush();
                    patternLayer.FillInfo.ApplyTransforms(textureScale);
                }
            }
        }

        /// <summary>
        /// Draw pattern in normal drawing mode (not Quality).
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="g"></param>
        public static void DrawPattern(Pattern pattern, Graphics g, IRenderCaller caller, float textureScale = 0,
                                       bool computeRandom = false, bool enableCache = true, bool draftMode = false)
        {
            if (textureScale != 0)
                ApplyTextureScale(pattern, textureScale);
            pattern.DrawFilled(g, caller, computeRandom, draftMode, textureScale: textureScale, enableCache: enableCache);
            //if (textureScale != 0)
            //    ApplyTextureScale(pattern, textureScale: 1F);
        }

        private static void DrawLayerPatterns(Bitmap bitmap, WhorlDesign design, IRenderCaller caller, 
                                              float textureScale, bool computeRandom,
                                              DesignLayer designLayer,
                                              IEnumerable<Pattern> overridePatterns = null, bool enableCache = true, 
                                              bool draftMode = false)
        {
            IEnumerable<Pattern> patterns = overridePatterns ?? design.EnabledPatterns;
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                DrawPatterns(g, caller, patterns.Where(ptn => ptn.DesignLayer == designLayer),
                             bitmap.Size, computeRandom, textureScale, enableCache: enableCache, draftMode: draftMode);
            }
        }

        private static Bitmap CreateLayerBitmap(WhorlDesign design, DesignLayer designLayer, IRenderCaller caller,
                                                Size bitmapSize, float textureScale,
                                                bool computeRandom,
                                                IEnumerable<Pattern> overridePatterns = null, bool enableCache = true, 
                                                bool draftMode = false)
        {
            Bitmap bitmap = BitmapTools.CreateFormattedBitmap(bitmapSize);
            DrawLayerPatterns(bitmap, design, caller, textureScale, computeRandom, designLayer,
                              overridePatterns, enableCache: enableCache, draftMode: draftMode);
            return bitmap;
        }

        private static void ExtractPixels(Bitmap bitmap, int[] colorArray, IntColor[] pixels)
        {
            BitmapTools.CopyBitmapToColorArray(bitmap, colorArray);
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new IntColor(Color.FromArgb(colorArray[i]));
        }

        private static int GetColorValue(int iColor, int iNewColor, float strength,
                                         ColorBlendTypes blendType, float blendStrength)
        {
            int retColor;
            switch (blendType)
            {
                case ColorBlendTypes.Add:
                    retColor = iColor + (int)Math.Round(blendStrength * strength * iNewColor);
                    break;
                case ColorBlendTypes.Average:
                    retColor = iColor + (int)Math.Round(0.5 * strength * (iNewColor - iColor));
                    break;
                case ColorBlendTypes.Contrast:
                    retColor = iColor + (int)Math.Round(blendStrength * strength * (iNewColor - iColor));
                    break;
                case ColorBlendTypes.Subtract:
                //case ColorBlendTypes.Difference:
                    retColor = iColor - (int)Math.Round(blendStrength * strength * iNewColor);
                    //if (blendType == ColorBlendTypes.Difference)
                    //    retColor = -retColor;
                    break;
                case ColorBlendTypes.Multiply:
                    retColor = (int)Math.Round((1 - strength) * iColor + strength * 
                                               iColor * iNewColor / 255D);
                    break;
                case ColorBlendTypes.And:
                    retColor = iColor & (int)Math.Round(
                        strength * iNewColor + 255D * (1 - strength));
                    break;
                case ColorBlendTypes.Or:
                    retColor = iColor | (int)Math.Round(strength * iNewColor);
                    break;
                case ColorBlendTypes.XOr:
                    retColor = iColor ^ (int)Math.Round(strength * iNewColor);
                    break;
                case ColorBlendTypes.None:
                default:
                    retColor = iColor + (int)Math.Round(strength * (iNewColor - iColor));
                    break;
            }
            if (retColor > 255)
                retColor = 510 - retColor;
            return retColor;  // < 0 ? 0 : retColor > 255 ? 255 : retColor;
        }

        private static IntColor BlendColor(IntColor pixel, IntColor newPixel,
                                           ColorBlendTypes blendType, float blendStrength)
        {
            if (newPixel.A == 0)
                return pixel;   //newPixel is transparent.
            float strength = (float)newPixel.A / 255F;
            return new IntColor(pixel.A,
                                GetColorValue(pixel.R, newPixel.R, strength, blendType, blendStrength),
                                GetColorValue(pixel.G, newPixel.G, strength, blendType, blendStrength),
                                GetColorValue(pixel.B, newPixel.B, strength, blendType, blendStrength));
        }

        //public static async Task DrawDesignLayersAsync(WhorlDesign design, Bitmap baseBitmap, IRenderCaller caller,
        //                         float textureScale = 0, bool computeRandom = false,
        //                         IEnumerable<Pattern> overridePatterns = null, bool enableCache = true,
        //                         bool draftMode = false)
        //{
        //    var t = Task.Run(() => DrawDesignLayers(design, baseBitmap, caller, textureScale, computeRandom,
        //                                          overridePatterns, enableCache, draftMode));
        //    t.Wait();
        //}

        public static void DrawDesignLayers(WhorlDesign design, Bitmap baseBitmap, IRenderCaller caller,
                                            float textureScale = 0, bool computeRandom = false,
                                            IEnumerable<Pattern> overridePatterns = null, bool enableCache = true, 
                                            bool draftMode = false)
        {
            DrawLayerPatterns(baseBitmap, design, caller, textureScale, 
                              computeRandom, designLayer: null, overridePatterns: overridePatterns, 
                              enableCache: enableCache, draftMode: draftMode);
            if (!design.EnabledPatterns.Any(ptn => ptn.DesignLayer != null))
                return;
            int steps = 10 * design.DesignLayerList.DesignLayers.Count();
            if (caller != null)
                caller.RenderCallback(steps, initial: true);
            //progressBar.Maximum = 10 * design.DesignLayerList.DesignLayers.Count();
            //progressBar.Value = 0;
            IntColor[] basePixels = new IntColor[baseBitmap.Width * baseBitmap.Height];
            IntColor[] layerPixels = new IntColor[basePixels.Length];
            int[] colorArray = new int[basePixels.Length];
            int progressBarSteps = basePixels.Length / 10;
            int progressBarCurStep = 0;
            ExtractPixels(baseBitmap, colorArray, basePixels);
            foreach (DesignLayer designLayer in design.DesignLayerList.DesignLayers)
            {
                if (!designLayer.Visible)
                    continue;
                if (caller != null && caller.CancelRender)
                    return;
                using (Bitmap layerBitmap = CreateLayerBitmap(design, designLayer, caller,
                                            baseBitmap.Size, textureScale, 
                                            computeRandom, overridePatterns, enableCache: enableCache, draftMode: draftMode))
                {
                    ExtractPixels(layerBitmap, colorArray, layerPixels);
                    ColorBlendTypes blendType = designLayer.ColorBlendType;
                    float blendStrength = designLayer.BlendStrength;
                    for (int i = 0; i < basePixels.Length; i++)
                    {
                        if (caller != null && caller.CancelRender)
                            return;
                        basePixels[i] = BlendColor(basePixels[i], layerPixels[i], 
                                                   blendType, blendStrength);
                        if (++progressBarCurStep % progressBarSteps == 0)
                        {
                            if (caller != null)
                                caller.RenderCallback(progressBarCurStep / progressBarSteps);
                            //progressBar.Increment(1);
                            //progressBarCurStep = 0;
                        }
                    }
                }
            }
            for (int i = 0; i < basePixels.Length; i++)
                colorArray[i] = basePixels[i].GetColor().ToArgb();
            BitmapTools.CopyColorArrayToBitmap(baseBitmap, colorArray);
            if (caller != null)
                caller.RenderCallback(steps);
            //progressBar.Value = progressBar.Maximum;
        }
    }
}
