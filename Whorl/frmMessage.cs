using ParserEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Whorl
{
    public partial class frmMessage : Form, IXml
    {
        private enum VariableNames
        {
            zStart,
            Iter,
            MaxIter,
            Fractal
        }

        static frmMessage()
        {
            ExpressionParser.DeclareExternType(typeof(FractalInfo));
        }

        public frmMessage()
        {
            InitializeComponent();
            tokenizer = new Tokenizer();
            parameterDisplay = new ParameterDisplay(pnlParameters, onParameterChanged, null);
            //csharpParameterDisplay = new CSharpParameterDisplay(pnlParameters, onCSharpParameterChanged, null);
            zStartIdent = formulaSettings.Parser.DeclareVariable(VariableNames.zStart.ToString(), typeof(Complex), 
                          new Complex(0, 0), isGlobal: true);
            iterIdent = formulaSettings.Parser.DeclareVariable(VariableNames.Iter.ToString(), typeof(double), 0D, isGlobal: true);
            maxIterIdent = formulaSettings.Parser.DeclareVariable(VariableNames.MaxIter.ToString(), typeof(double), 0D, isGlobal: true);
            var fractalInfoIdent = formulaSettings.Parser.DeclareVariable(VariableNames.Fractal.ToString(), 
                                   typeof(FractalInfo), fractalInfo, isGlobal: true, isReadOnly: true);
            foreach (TextBox txt in GetFractalTextBoxes())
            {
                txt.KeyPress += FractalTextBox_KeyPress;
                //txt.Leave += FractalTextBox_Leave;
            }
            string[] mthdNames = typeof(OutlineMethods).GetMethods().Select(mi => mi.Name).ToArray();
            cboOutlineMethod.DataSource = mthdNames;
            cboOutlineMethod.SelectedItem = mthdNames.FirstOrDefault();
        }

        //private void onCSharpParameterChanged(object sender, ParameterChangedEventArgs e)
        //{
        //    if (e.RefreshDisplay)
        //        RenderCompiled();
        //}

        Dictionary<string, string> TextBoxTexts { get; } = new Dictionary<string, string>();

        private void SaveTextBoxText(TextBox txt)
        {
            TextBoxTexts[txt.Name] = txt.Text;
        }

        //private void FractalTextBox_Leave(object sender, EventArgs e)
        //{
        //    TextBox txt = (TextBox)sender;
        //}

        private async void FractalTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r')
                return;
            e.Handled = true;
            TextBox txt = (TextBox)sender;
            if (TextBoxTexts.TryGetValue(txt.Name, out string prevText))
            {
                if (txt.Text == prevText)
                    return;
            }
            SaveTextBoxText(txt);
            if ((txt.Tag as string) == "Render")
                RenderFractal();
            else
                await Evaluate();
        }

        private const string filePath = @"C:\WhorlFilesDev\TestFormula.xml";
        private const string fileFolder = @"C:\WhorlFilesDev\Fractals";

        private ParameterDisplay parameterDisplay { get; }
        private ValidIdentifier zStartIdent { get; }
        private ValidIdentifier iterIdent { get; }
        private ValidIdentifier maxIterIdent { get; }
        private FractalInfo fractalInfo { get; } = new FractalInfo();
        private FormulaSettings formulaSettings { get; } = new FormulaSettings(FormulaTypes.Unknown);

        private bool forExpressionMessages { get; set; }
        private bool cancelEval;
        //private EventHandler evalCompleted;

        public void DisplayMessages()
        {
            forExpressionMessages = true;
            txtMessages.Text = DebugMessages.GetMessages();
        }

        public void DisplayMessages(string messages)
        {
            forExpressionMessages = false;
            txtMessages.Text = messages;
        }

        private void btnClearMessages_Click(object sender, EventArgs e)
        {
            txtMessages.Clear();
            if (forExpressionMessages)
                DebugMessages.ClearMessages();
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            try
            {
                codeTokens = tokenizer.TokenizeExpression(txtFormula.Text);
                var tokens = new List<Token>();
                int tokInd = 0;
                bool? isType = TryParseType(tokens, ref tokInd, false);
                txtMessages.Text = $"isType = {isType}; tokens = {string.Join(" ", tokens)}; next token = {GetToken(tokInd)}";
                //if (formulaSettings.Parse(txtFormula.Text) == ParseStatusValues.Success)
                //{
                //    parameterDisplay.AddAllParametersControls(formulaSettings);
                //    if (testOptimizationToolStripMenuItem.Checked)
                //    {
                //        txtMessages.AppendText(formulaSettings.Parser.ParenthesizeStatements());
                //    }
                //}
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private Tokenizer tokenizer { get; }
        private List<Token> codeTokens { get; set; }
        private static readonly Token nullToken = new Token(string.Empty, Token.TokenTypes.EndOfStream, 0);

        private Token GetToken(int tokenIndex)
        {
            if (tokenIndex >= codeTokens.Count)
            {
                return nullToken;
            }
            return codeTokens[tokenIndex];
        }

        private bool? TryParseType(List<Token> tokens, ref int tokInd,
                                   bool forGeneric)
        {
            bool? isType = null;
            while (true)
            {
                Token tok = GetToken(tokInd);
                if (tok.TokenType != Token.TokenTypes.Identifier)
                {
                    isType = false;
                    break;
                }
                tokens.Add(tok);
                tok = GetToken(++tokInd);
                if (tok.Text == ".")
                {
                    isType = true;
                    tokInd++;
                }
                else if (tok.Text == "<")
                {
                    tokens.Add(tok);
                    isType = true;
                    tokInd++;
                    while (true)
                    {
                        if (TryParseType(tokens, ref tokInd, forGeneric: true) == false)
                        {
                            isType = false;
                            break;
                        }
                        tok = GetToken(tokInd);
                        if (tok.Text == ",")
                            tokens.Add(tok);
                        else
                        {
                            if (tok.Text == ">")
                            {
                                tokens.Add(tok);
                                tokInd++;
                            }
                            else
                                isType = false;
                            break;
                        }
                    }
                    break;
                }
                else
                    break;
                tokens.Add(tok);
            }
            return isType;
        }


        private ColorNodeList colorNodeList = null;
        int colorCount = 255;
        float colorFactor = 1F;

        private void RenderFractal()
        {
            if (fractalInfo.IterArray == null)
                return;
            if (!float.TryParse(txtColorFactor.Text, out colorFactor))
                colorFactor = 1;
            if (useColorGradientToolStripMenuItem.Checked && frmColorGradient != null && !frmColorGradient.IsDisposed)
            {
                if (!int.TryParse(txtColorCount.Text, out colorCount))
                    colorCount = 255;
                colorNodeList = frmColorGradient.Component.ColorNodes;
            }
            else
            {
                colorNodeList = null;
            }
            int[] colorArray = fractalInfo.IterArray.Select(i => GetFractalColor(i).ToArgb()).ToArray();
            Bitmap bmp = picImage.Image as Bitmap;
            if (bmp != null)
                bmp.Dispose();
            bmp = BitmapTools.CreateFormattedBitmap(imgSize);
            BitmapTools.CopyColorArrayToBitmap(bmp, colorArray);
            picImage.Image = bmp;
        }

        private Color GetFractalColor(int iter)
        {
            iter = (int)(colorFactor * (maxIter - iter));
            if (colorNodeList != null)
            {
                iter = iter % (2 * colorCount + 1);
                if (iter > colorCount)
                    iter = 2 * colorCount - iter;
                float position = (float)iter / colorCount;
                return colorNodeList.GetColorAtPosition(position);
            }
            else
            {
                int g = iter & 0xFF;
                int b = (iter >> 8) & 0xFF;
                int r = (iter >> 16) & 0xFF;
                return Color.FromArgb(r, g, b);
            }
        }

        //private int[] iterArray;
        private Size imgSize;
        private int maxIter;

        private void InitFractal()
        {
            if (!int.TryParse(txtMaxIter.Text, out maxIter))
                maxIter = 255;
            maxIterIdent.SetCurrentValue((double)maxIter);
            if (!int.TryParse(txtImageWidth.Text, out int imgWidth))
                imgWidth = 100;
            if (!double.TryParse(txtZoom.Text, out double zoom))
                zoom = 1;
            if (!double.TryParse(txtZoomX.Text, out double zoomX))
                zoomX = 1;
            if (!double.TryParse(txtZoomY.Text, out double zoomY))
                zoomY = 1;
            if (!double.TryParse(txtOffsetX.Text, out double offsetX))
                offsetX = 0;
            if (!double.TryParse(txtOffsetY.Text, out double offsetY))
                offsetY = 0;
            if (!int.TryParse(txtDraftSize.Text, out int draftSize))
                draftSize = 1;
            fractalInfo.DraftSize = draftSize;
            double extent = 1D;
            zoom = Math.Max(0.00001, zoom);
            zoomX = zoom * Math.Max(0.00001, zoomX);
            zoomY = zoom * Math.Max(0.00001, zoomY);
            fractalInfo.zIncX = 2D * extent / (zoomX * imgWidth);
            fractalInfo.zIncY = 2D * extent / (zoomY * imgWidth);
            offsetX /= zoomX;
            offsetY /= zoomY;
            fractalInfo.imStart = -extent / zoomY + offsetY;
            fractalInfo.reStart = -extent / zoomX + offsetX;
            imgSize = ParseImageSize();
            fractalInfo.ImgSize = imgSize;
            fractalInfo.IterIndex = 0;
            formulaSettings.InitializeGlobals();
        }

        private Size ParseImageSize()
        {
            if (!int.TryParse(txtImageWidth.Text, out int imgWidth))
                imgWidth = 100;
            var imgSize = new Size(imgWidth, imgWidth);
            picImage.ClientSize = imgSize;
            return imgSize;
        }

        private Stopwatch stopwatch { get; } = new Stopwatch();

        private bool isRunning;

        private async void onParameterChanged(object sender, EventArgs e)
        {
            await Evaluate();
        }

        private async Task Evaluate()
        {
            try
            {
                if (!isFractalToolStripMenuItem.Checked)
                {
                    formulaSettings.EvalFormula();
                    DisplayMessages();
                    return;
                }
                if (isRunning)
                {
                    cancelEval = true;
                    while (isRunning)
                    {
                        Application.DoEvents();
                    }
                    lblStatus.Text = "Restarting run...";
                    //return;
                }
                else
                {
                    lblStatus.Text = "Running...";
                }
                lblStatus.Refresh();
                InitFractal();
                cancelEval = false;

                stopwatch.Restart();
                progressBar1.Maximum = fractalInfo.IterArray.Length;

                isRunning = true;
                await EvalFractalAsync();

                //EvalFractal();
                //Task task = new Task(() => EvalFractal());
                //task.Start();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private async void btnEval_Click(object sender, EventArgs e)
        {
            await Evaluate();
        }

        private void SetProgressBar(int value)
        {
            if (InvokeRequired)
            {
                Invoke((Action<int>)SetProgressBar, value);
                return;
            }
            progressBar1.Value = value;
        }

        private void OnEvalCompleted()
        {
            if (InvokeRequired)
            {
                Invoke((Action)OnEvalCompleted);
                return;
            }
            progressBar1.Value = progressBar1.Maximum;
            stopwatch.Stop();
            lblStatus.Text =
      $"Elapsed time: {stopwatch.ElapsedMilliseconds} milliseconds. Iter Total = {fractalInfo.IterArray.Sum()}; Min = {fractalInfo.IterArray.Min()}; Max = {fractalInfo.IterArray.Max()}";
            RenderFractal();
            DisplayMessages();
            foreach (TextBox txt in GetFractalTextBoxes())
            {
                SaveTextBoxText(txt);
            }
            isRunning = false;
        }

        private async Task EvalFractalAsync()
        {
            await (Task.Factory.StartNew(() =>
            {
                try
                {
                    int iter = 0;
                    double im = fractalInfo.imStart;
                    for (int y = 0; y < imgSize.Height && !cancelEval; y++)
                    {
                        double re = fractalInfo.reStart;
                        for (int x = 0; x < imgSize.Width && !cancelEval; x++)
                        {
                            if (y % fractalInfo.DraftSize == 0 & x % fractalInfo.DraftSize == 0)
                            {
                                var zStart = new Complex(re, im);
                                zStartIdent.SetCurrentValue(zStart);
                                iterIdent.SetCurrentValue(0D);
                                formulaSettings.EvalFormula();
                                iter = Convert.ToInt32(iterIdent.CurrentValue);
                                int maxJ = Math.Min(fractalInfo.DraftSize, imgSize.Height - y);
                                int maxI = Math.Min(fractalInfo.DraftSize, imgSize.Width - x);
                                for (int j = 0; j < maxJ; j++)
                                {
                                    for (int i = 0; i < maxI; i++)
                                    {
                                        int ii = fractalInfo.IterIndex + j * imgSize.Width + i;
                                        if (ii < fractalInfo.IterArray.Length)
                                            fractalInfo.IterArray[ii] = iter;
                                    }
                                }
                            }
                            re += fractalInfo.zIncX;
                            fractalInfo.IterIndex++;
                        }
                        im += fractalInfo.zIncY;
                        if (y % 20 == 0)
                        {
                            RenderFractal();
                            SetProgressBar(fractalInfo.IterIndex);
                            //Application.DoEvents();
                        }
                    }
                    OnEvalCompleted();
                }
                catch
                { }
            }));
        }

        //private void EvalFractal()
        //{
        //    int iter = 0;
        //    double im = fractalInfo.imStart;
        //    for (int y = 0; y < imgSize.Height && !cancelEval; y++)
        //    {
        //        double re = fractalInfo.reStart;
        //        for (int x = 0; x < imgSize.Width && !cancelEval; x++)
        //        {
        //            if (y % fractalInfo.DraftSize == 0 & x % fractalInfo.DraftSize == 0)
        //            {
        //                var zStart = new Complex(re, im);
        //                zStartIdent.SetCurrentValue(zStart);
        //                iterIdent.SetCurrentValue(0D);
        //                formulaSettings.EvalStatements();
        //                iter = Convert.ToInt32(iterIdent.CurrentValue);
        //                int maxJ = Math.Min(fractalInfo.DraftSize, imgSize.Height - y);
        //                int maxI = Math.Min(fractalInfo.DraftSize, imgSize.Width - x);
        //                for (int j = 0; j < maxJ; j++)
        //                {
        //                    for (int i = 0; i < maxI; i++)
        //                    {
        //                        int ii = fractalInfo.IterIndex + j * imgSize.Width + i;
        //                        if (ii < fractalInfo.IterArray.Length)
        //                            fractalInfo.IterArray[ii] = iter;
        //                    }
        //                }
        //            }
        //            re += fractalInfo.zIncX;
        //            fractalInfo.IterIndex++;
        //        }
        //        im += fractalInfo.zIncY;
        //        if (y % 20 == 0)
        //        {
        //            RenderFractal();
        //            SetProgressBar(fractalInfo.IterIndex);
        //            //Application.DoEvents();
        //        }
        //    }
        //    OnEvalCompleted();
        //}

        private void btnRenderFractal_Click(object sender, EventArgs e)
        {
            try
            {
                RenderFractal();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private void btnSaveFormula_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Tools.WriteToXml(filePath, formulaSettings, nameof(FormulaSettings));
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        private string savedFileName;

        private void saveFormulaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new SaveFileDialog();
                dlg.InitialDirectory = fileFolder;
                dlg.Filter = "Xml File|*.xml";
                if (savedFileName != null)
                {
                    dlg.FileName = Path.GetFileName(savedFileName);
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    XmlTools.WriteToXml(dlg.FileName, this);
                    savedFileName = dlg.FileName;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void openFormulaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog();
                dlg.InitialDirectory = fileFolder;
                dlg.Filter = "Xml File|*.xml";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Tools.ReadFromXml(dlg.FileName, this);
                    txtFormula.Text = formulaSettings.Formula;
                    parameterDisplay.AddAllParametersControls(formulaSettings);
                    savedFileName = dlg.FileName;
                    isFractalToolStripMenuItem.Checked = true;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private IEnumerable<TextBox> GetFractalTextBoxes()
        {
            foreach (Control ctl in this.Controls)
            {
                TextBox txt = ctl as TextBox;
                if (txt == null || txt == txtMessages || txt == txtFormula)
                    continue;
                yield return txt;
            }
        }

        public XmlNode ToXml(XmlNode parentNode, XmlTools xmlTools, string xmlNodeName = null)
        {
            if (xmlNodeName == null)
            {
                xmlNodeName = "FractalForm";
            }
            XmlNode node = xmlTools.CreateXmlNode(xmlNodeName);
            var childNode = xmlTools.CreateXmlNode("FormSettings");
            foreach (TextBox txt in GetFractalTextBoxes())
            {
                xmlTools.AppendChildNode(childNode, txt.Name, txt.Text);
            }
            node.AppendChild(childNode);
            formulaSettings.ToXml(node, xmlTools, "FormulaSettings");
            if (colorNodeList != null)
            {
                colorNodeList.ToXml(node, xmlTools, "ColorNodes");
            }
            return xmlTools.AppendToParent(parentNode, node);
        }

        public void FromXml(XmlNode node)
        {
            if (node.Name == "FormulaSettings")
            {   //Legacy code.
                formulaSettings.FromXml(node);
                return;
            }
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "FormSettings")
                {
                    foreach (XmlNode subNode in childNode.ChildNodes)
                    {
                        SetControlFromXml(subNode);
                    }
                }
                if (childNode.Name == "FormulaSettings")
                {
                    formulaSettings.FromXml(childNode);
                }
                else if (childNode.Name == "ColorNodes")
                {
                    colorNodeList = new ColorNodeList();
                    colorNodeList.FromXml(childNode);
                    CreateColorGradientForm();
                    frmColorGradient.Component.ColorNodes = colorNodeList;
                    useColorGradientToolStripMenuItem.Checked = true;
                }
                else
                {   //Legacy code.
                    SetControlFromXml(childNode);
                }
            }
        }

        private void SetControlFromXml(XmlNode xmlNode)
        {
            var txt = this.Controls.Find(xmlNode.Name, true).FirstOrDefault() as TextBox;
            if (txt != null)
            {
                txt.Text = xmlNode.Attributes["Value"].Value;
                SaveTextBoxText(txt);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cancelEval = true;
        }

        private frmColorGradient frmColorGradient;

        private void CreateColorGradientForm()
        {
            if (frmColorGradient == null || frmColorGradient.IsDisposed)
            {
                frmColorGradient = new frmColorGradient();
                frmColorGradient.Component.GradientChanged += ColorGradientChanged;
            }
        }

        private void colorGradientFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                CreateColorGradientForm();
                Tools.DisplayForm(frmColorGradient, this);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ColorGradientChanged(object sender, EventArgs e)
        {
            try
            {
                RenderFractal();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private PixelRenderInfo InitRuntime(int width)
        //{
        //    //string methodName = (string)cboOutlineMethod.SelectedItem;
        //    var info = new PixelRenderInfo();
        //    int halfW = width / 2;
        //    info.Center = new PointF(halfW, halfW);
        //    info.MaxPosition = halfW;
        //    info.ScaleFactor = 1.0;
        //    cSharpFormulaSettings.EvalInstance.SetInfoObject(info);
        //    //var methodInfo = typeof(OutlineMethods).GetMethod(methodName);
        //    //var fn = (Func<double, double, double>)Delegate.CreateDelegate(typeof(Func<double, double, double>), methodInfo);
        //    //var propInfo = CSharpCompiledInfo.EvalClassType.GetProperty("OutlineFunction");
        //    //propInfo.SetValue(CSharpCompiledInfo.EvalInstance, fn);
        //    return info;
        //}

        //private void EvalCSharpCompiledCode()
        //{
        //    const int width = 200;
        //    PixelRenderInfo info = InitRuntime(width);
        //    float result = 0;
        //    DateTime start = DateTime.Now;
        //    for (info.Y = 0; info.Y < width; info.Y++)
        //    {
        //        for (info.X = 0; info.X < width; info.X++)
        //        {
        //            cSharpFormulaSettings.EvalStatements();
        //            result = info.Position;
        //        }
        //    }
        //    var milliseconds = (DateTime.Now - start).TotalMilliseconds;
        //    txtMessages.Text = $"Compile was successful. Result = {result}; Elapsed Milliseconds = {milliseconds}.";
        //}

        //private FormulaSettings cSharpFormulaSettings { get; } = new FormulaSettings(FormulaTypes.PixelRender);
        //private CSharpParameterDisplay csharpParameterDisplay;

        //private void BtnCompile_Click(object sender, EventArgs e)
        //{

        //}

        //private void RenderCompiled()
        //{
        //    try
        //    {
        //        if (!cSharpFormulaSettings.IsValid || cSharpFormulaSettings.EvalInstance == null)
        //        {
        //            MessageBox.Show("Code did not compile successfully.");
        //            return;
        //        }
        //        DateTime start = DateTime.Now;
        //        Size imgSize = ParseImageSize();
        //        PixelRenderInfo info = InitRuntime(imgSize.Width);
        //        int[] colorArray = new int[imgSize.Width * imgSize.Height];
        //        CreateColorGradientForm();
        //        ColorNodeList colorNodeList = frmColorGradient.Component.ColorNodes;
        //        float x, y;
        //        y = 0;
        //        int iColor = 0;
        //        for (int iY = 0; iY < imgSize.Height; iY++)
        //        {
        //            x = 0;
        //            info.Y = y;
        //            for (int iX = 0; iX < imgSize.Width; iX++)
        //            {
        //                info.X = x;
        //                cSharpFormulaSettings.EvalStatements();
        //                float position = info.Position;
        //                colorArray[iColor++] = colorNodeList.GetColorAtPosition(position).ToArgb();
        //                x++;
        //            }
        //            y++;
        //        }
        //        Bitmap bmp = picImage.Image as Bitmap;
        //        if (bmp != null)
        //            bmp.Dispose();
        //        bmp = BitmapTools.CreateFormattedBitmap(imgSize);
        //        BitmapTools.CopyColorArrayToBitmap(bmp, colorArray);
        //        picImage.Image = bmp;
        //        var milliseconds = (DateTime.Now - start).TotalMilliseconds;
        //        txtMessages.Text = $"Rendered in {milliseconds} milliseconds.";
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void BtnRenderCompiled_Click(object sender, EventArgs e)
        //{
        //    RenderCompiled();
        //}
    }
}
