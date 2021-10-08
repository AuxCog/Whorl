using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public partial class SelectPatternForm : Form
    {
        public enum TargetTypes
        {
            Pattern,
            Color,
            Palette
        }

        private enum FilterPatternTypes
        {
            Pattern,
            Ribbon,
            Path,
            Text
        }

        private class PatternListInfo
        {
            public PatternList PatternList { get; }

            public PatternListInfo(PatternList patternList)
            {
                PatternList = patternList;
            }

            public override string ToString()
            {
                return PatternList == null ? string.Empty : PatternList.PatternListName;
            }
        }

        public const int ThumbnailImageWidth = 100;
        private const int pagesPerRange = 20;
        private int pictureBoxMargin { get; } = 10;
        private int pictureBoxWidth { get; } = ThumbnailImageWidth;
        private int pictureBoxHeight { get; } = ThumbnailImageWidth;
        public int BoxesAcross { get; private set; }
        public int BoxesDown { get; private set; }

        private PictureBox[] pictureBoxes;
        private TextBox[] nameTextBoxes;
        private PictureBox selectedPictureBox = null;
        private int currentPageIndex = 0;
        private ColorGradientFormComponent paletteTool { get; }

        public SelectPatternForm(TargetTypes targetType, PatternGroupList patternChoices)
        {
            InitializeComponent();
            this.TargetType = targetType;
            this.PatternChoices = patternChoices;  //List of patterns and colors.
            pnlPatternFilters.Parent = this;
            cboPatternType.DataSource = Enum.GetValues(typeof(FilterPatternTypes));
            cboBasicOutlineType.DataSource = Enum.GetValues(typeof(BasicOutlineTypes));
            cboRibbonType.DataSource = Enum.GetValues(typeof(RibbonDrawingModes));
            cboHasRandom.DataSource = BooleanItem.GetYesNoItems();
            cboUsesSection.DataSource = BooleanItem.GetYesNoItems();
            cboUsesRecursion.DataSource = BooleanItem.GetYesNoItems();
            cboRibbonUsesFormula.DataSource = BooleanItem.GetYesNoItems();
            //Checked properties may have been set true by events:
            chkPatternType.Checked = false;
            chkBasicOutlineType.Checked = false;
            chkRibbonType.Checked = false;
            pnlPatternDashboard.Visible = TargetType == TargetTypes.Pattern;
            if (targetType != TargetTypes.Pattern)
            {
                this.removeItemToolStripMenuItem.Text = $"Remove {targetType}";
                this.editPatternToolStripMenuItem.Visible = false;
                if (targetType == TargetTypes.Color)
                     setPatternFromDefaultToolStripMenuItem.Visible = false;
                else if (targetType == TargetTypes.Palette)
                    setPatternFromDefaultToolStripMenuItem.Text = "Paste Palette";
                this.pictureBoxMargin = 3;
                this.pictureBoxHeight = 30;
                this.pictureBoxWidth = targetType == TargetTypes.Palette ? 100 : 30;
                if (targetType == TargetTypes.Palette)
                {
                    paletteTool = new ColorGradientFormComponent(new Size(pictureBoxWidth, pictureBoxHeight));
                }
            }
            ResizeForm();
            ConfigureForm();
        }

        public WhorlDesign Design { get; set; }

        /// <summary>
        /// Contains lists of patterns and colors to choose from.
        /// </summary>
        public PatternGroupList PatternChoices { get; }

        public PatternList SelectedPatternGroup { get; private set; }

        public Color? SelectedColor { get; private set; }

        public ColorNodeList SelectedPalette { get; private set; }

        public TargetTypes TargetType { get; }

        private List<PatternList> FilteredPatternLists { get; set; }

        public void Initialize()
        {
            try
            {
                if (TargetType == TargetTypes.Pattern)
                {
                    PopulatedNamedPatternCombobox();
                    PopulateTransformNameComboBox();
                }
                Init();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void Init()
        {
            PatternList selPatternGroup = SelectedPatternGroup;
            selectedPictureBox = null;
            switch (TargetType)
            {
                case TargetTypes.Pattern:
                    SelectedPatternGroup = null;
                    SetFilteredPatternLists();
                    Size picSize = new Size(pictureBoxWidth, pictureBoxWidth);
                    foreach (PatternList patternGroup in FilteredPatternLists)
                    {
                        patternGroup.SetForPreview(picSize);
                    }
                    break;
                case TargetTypes.Color:
                    SelectedColor = null;
                    break;
                case TargetTypes.Palette:
                    SelectedPalette = null;
                    break;
            }
            int itemCount = GetItemCount();
            PopulatePageRangeComboBox(itemCount);
            if (TargetType == TargetTypes.Pattern && selPatternGroup != null)
            {
                if (GoToPatternList(selPatternGroup, allowClearFilter: false))
                    return;
            }
            int maxPageIndex = Math.Max(0, (itemCount - 1) / pictureBoxes.Length);
            currentPageIndex = Math.Min(currentPageIndex, maxPageIndex);
            DisplayItems(currentPageIndex);
            AddPageLabels(itemCount);
        }

        private bool PatternIsOfType(Pattern pattern, FilterPatternTypes filterPatternType)
        {
            switch (filterPatternType)
            {
                case FilterPatternTypes.Pattern:
                    return pattern.GetType() == typeof(Pattern);
                case FilterPatternTypes.Ribbon:
                    return pattern.GetRibbon() != null;
                case FilterPatternTypes.Path:
                    return pattern.GetType() == typeof(PathPattern);
                case FilterPatternTypes.Text:
                    return pattern.GetType() == typeof(StringPattern);
                default:
                    return false;
            }
        }

        private bool PatternListMatchesFilters(PatternList patternList, 
                                               bool onlyNamedPatterns,
                                               FilterPatternTypes? filterPatternType,
                                               RibbonDrawingModes? ribbonType,
                                               BasicOutlineTypes? basicOutlineType,
                                               bool? hasRandom,
                                               string transformName,
                                               bool? usesSection,
                                               bool? ribbonUsesFormula,
                                               bool? usesRecursion)
        {
            if (onlyNamedPatterns)
            {
                if (patternList.PatternListName == null)
                    return false;
            }
            IEnumerable<Pattern> patterns = patternList.Patterns;
            if (filterPatternType != null)
            {
                patterns = patterns.Where(ptn => PatternIsOfType(ptn, (FilterPatternTypes)filterPatternType));
            }
            if (usesSection != null)
            {
                patterns = patterns.Where(ptn => ptn.UsesSection == usesSection);
            }
            if (ribbonType != null)
            {
                patterns = patterns.Where(ptn => ptn is Ribbon && ((Ribbon)ptn).DrawingMode == ribbonType);
            }
            if (ribbonUsesFormula != null)
            {
                patterns = patterns.Where(ptn => ptn is Ribbon && ((Ribbon)ptn).UsesFormula == ribbonUsesFormula);
            }
            if (basicOutlineType != null)
            {
                patterns = patterns.Where(ptn => ptn.BasicOutlines.Exists(bo => bo.BasicOutlineType == basicOutlineType));
            }
            if (hasRandom != null)
            {
                patterns = patterns.Where(ptn => ptn.HasRandomElements == hasRandom);
            }
            if (usesRecursion != null)
            {
                patterns = patterns.Where(ptn => ptn.Recursion.IsRecursive);
            }
            if (transformName != null)
            {
                patterns = patterns.Where(ptn => ptn.Transforms.Exists(t => t.TransformName == transformName));
            }
            return patterns.Any();
        }

        private bool? GetBoolValue(ComboBox cbo)
        {
            var booleanItem = cbo.SelectedItem as BooleanItem;
            return booleanItem?.Value;
        }

        private void SetFilteredPatternLists()
        {
            if (chkFilterPatterns.Checked)
            {
                FilterPatternTypes? patternType = null;
                if (chkPatternType.Checked && cboPatternType.SelectedItem is FilterPatternTypes)
                    patternType = (FilterPatternTypes)cboPatternType.SelectedItem;
                BasicOutlineTypes? basicOutlineType = null;
                if (chkBasicOutlineType.Checked && cboBasicOutlineType.SelectedItem is BasicOutlineTypes)
                    basicOutlineType = (BasicOutlineTypes)cboBasicOutlineType.SelectedItem;
                string transformName = null;
                if (chkTransform.Checked)
                    transformName = (string)cboTransformName.SelectedItem;
                RibbonDrawingModes? ribbonType = null;
                if (chkRibbonType.Checked && cboRibbonType.SelectedItem is RibbonDrawingModes)
                {
                    ribbonType = (RibbonDrawingModes)cboRibbonType.SelectedItem;
                }
                FilteredPatternLists = PatternChoices.PatternGroups.FindAll(pg => 
                            PatternListMatchesFilters(
                                       pg, chkOnlyNamedPatterns.Checked, patternType, ribbonType,
                                       basicOutlineType, GetBoolValue(cboHasRandom), transformName, 
                                       GetBoolValue(cboUsesSection), GetBoolValue(cboRibbonUsesFormula),
                                       GetBoolValue(cboUsesRecursion)));
            }
            else
            {
                FilteredPatternLists = PatternChoices.PatternGroups;
            }
        }

        private void PopulatedNamedPatternCombobox()
        {
            List<PatternListInfo> items = PatternChoices.PatternGroups.Where(pg => pg.PatternListName != null)
                                          .OrderBy(pg => pg.PatternListName)
                                          .Select(pg => new PatternListInfo(pg)).ToList();
            items.Insert(0, new PatternListInfo(null));
            try
            {
                cboPatternName.SelectedIndexChanged -= cboPatternName_SelectedIndexChanged;
                cboPatternName.DataSource = items;
            }
            finally
            {
                cboPatternName.SelectedIndexChanged += cboPatternName_SelectedIndexChanged;
            }
        }

        private void PopulateTransformNameComboBox()
        {
            bool hasFilter = chkTransform.Checked;
            string selName = cboTransformName.SelectedItem as string;
            cboTransformName.DataSource = PatternChoices.FormulaEntryList.GetEntries(FormulaTypes.Transform)
                    .Select(fe => fe.FormulaName)
                    .Where(fn => !string.IsNullOrEmpty(fn)).Distinct().OrderBy(fn => fn).ToList();
            if (selName != null && cboTransformName.Items.Contains(selName))
                cboTransformName.SelectedItem = selName;
            chkTransform.Checked = hasFilter;
        }

        private void DisplayItems(int pageIndex)
        {
            if (TargetType == TargetTypes.Pattern)
                DisplayPatterns(pageIndex);
            else if (TargetType == TargetTypes.Color)
                DisplayColors(pageIndex);
            else if (TargetType == TargetTypes.Palette)
                DisplayPalettes(pageIndex);
        }

        private void DisplayPatterns(int pageIndex)
        {
            int patternInd = pageIndex * pictureBoxes.Length;
            for (int picInd = 0; picInd < pictureBoxes.Length; picInd++)
            {
                PictureBox pic = pictureBoxes[picInd];
                PatternList patternGroup;
                if (patternInd < FilteredPatternLists.Count)
                {
                    patternGroup = FilteredPatternLists[patternInd++];
                    pic.BackColor = patternGroup.GetPreviewBackgroundColor();
                }
                else
                {
                    patternGroup = null;
                    pic.BackColor = Color.White;
                }
                pic.BorderStyle = BorderStyle.None;
                pic.Tag = patternGroup;
                TextBox txt = nameTextBoxes[picInd];
                txt.Text = patternGroup == null ? string.Empty : patternGroup.PatternListName;
                txt.Tag = patternGroup;
                txt.BackColor = Color.White;
                txt.Enabled = patternGroup != null;
                //DisplayPattern(pic, patternGroup);
                DisplayPatternAsync(pic, patternGroup).ContinueWith(
                    Tools.AsyncTaskFailed, TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        //private void DisplayPattern(PictureBox pic, PatternList patternGroup)
        //{
        //    if (patternGroup != null)
        //    {
        //        patternGroup.CheckCreateThumbnailImage();
        //        pic.Image = patternGroup.ThumbnailImage;
        //    }
        //    else
        //    {
        //        pic.Image = null;
        //    }
        //    if (pic.Image == null)
        //        //Display patternGroup if not null, else blank picturebox:
        //        pic.Invalidate();
        //}

        private async Task DisplayPatternAsync(PictureBox pic, PatternList patternGroup)
        {
            if (patternGroup != null)
            {
                await patternGroup.CheckCreateThumbnailImageAsync();
                pic.Image = patternGroup.ThumbnailImage;
            }
            else
                pic.Image = null;
            if (pic.Image == null)
                //Display patternGroup if not null, else blank picturebox:
                pic.Invalidate();
        }

        private void DisplayColors(int pageIndex)
        {
            int colorInd = pageIndex * pictureBoxes.Length;
            foreach (PictureBox pic in pictureBoxes)
            {
                if (colorInd < PatternChoices.ColorChoices.Count)
                {
                    pic.Tag = colorInd;
                    pic.BackColor = PatternChoices.ColorChoices[colorInd++];
                }
                else
                {
                    pic.Tag = -1;
                    pic.BackColor = Color.Empty;
                }
                pic.BorderStyle = BorderStyle.None;
            }
        }

        private void ShowPalette(PictureBox pictureBox, ColorNodeList palette)
        {
            paletteTool.ColorNodes = palette;
            pictureBox.Image?.Dispose();
            pictureBox.Image = paletteTool.GetGradientImage();
            pictureBox.Refresh();
        }

        private void DisplayPalettes(int pageIndex)
        {
            int paletteInd = pageIndex * pictureBoxes.Length;
            foreach (PictureBox pic in pictureBoxes)
            {
                if (paletteInd < PatternChoices.PaletteChoices.Count)
                {
                    pic.Tag = paletteInd;
                    var palette = PatternChoices.PaletteChoices[paletteInd++];
                    ShowPalette(pic, palette);
                }
                else
                {
                    pic.Tag = -1;
                    pic.Image?.Dispose();
                    pic.Image = null;
                    pic.Refresh();
                }
                pic.BorderStyle = BorderStyle.None;
            }
        }


        private int GetPageCount(int itemCount)
        {
            return 1 + (itemCount - 1) / pictureBoxes.Length;
        }

        private void PopulatePageRangeComboBox(int itemCount)
        {
            try
            {
                cboPageRange.SelectedIndexChanged -= cboPageRange_SelectedIndexChanged;
                List<string> pageRanges = new List<string>();
                int pageCount = GetPageCount(itemCount);
                for (int startPage = 1; startPage <= pageCount; startPage += pagesPerRange)
                {
                    int endPage = Math.Min(pageCount, startPage + pagesPerRange - 1);
                    pageRanges.Add($"{startPage}-{endPage}");
                }
                cboPageRange.DataSource = pageRanges;
                int ind = currentPageIndex / pagesPerRange;
                cboPageRange.SelectedIndex = Math.Min(cboPageRange.Items.Count - 1, ind);
            }
            finally
            {
                cboPageRange.SelectedIndexChanged += cboPageRange_SelectedIndexChanged;
            }
        }

        private void cboPageRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                AddPageLabels(GetItemCount());
                SetCurrentPageIndex(Math.Max(0, cboPageRange.SelectedIndex * pagesPerRange));
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void AddPageLabels(int itemCount)
        {
            pnlPages.Controls.Clear();

            int startPage = 1 + pagesPerRange * Math.Max(cboPageRange.SelectedIndex, 0);
            int endPage = Math.Min(startPage + pagesPerRange - 1, GetPageCount(itemCount));
            for (int page = startPage; page <= endPage; page++)
            {
                LinkLabel lbl = new LinkLabel();
                lbl.AutoSize = true;
                lbl.Text = page.ToString();
                int pageIndex = page - 1;
                lbl.Tag = pageIndex;
                lbl.Click += pageLabel_Clicked;
                if (currentPageIndex == pageIndex)
                    lbl.Enabled = false;
                pnlPages.Controls.Add(lbl);
            }
        }

        private void SetCurrentPageIndex(int pageIndex, bool setPageRange = false)
        {
            if (setPageRange)
            {
                int pageRangeIndex = pageIndex / pagesPerRange;
                try
                {
                    cboPageRange.SelectedIndexChanged -= cboPageRange_SelectedIndexChanged;
                    cboPageRange.SelectedIndex = pageRangeIndex;
                    AddPageLabels(GetItemCount());
                }
                finally
                {
                    cboPageRange.SelectedIndexChanged += cboPageRange_SelectedIndexChanged;
                }
            }
            if (currentPageIndex != pageIndex)
            {
                currentPageIndex = pageIndex;
                selectedPictureBox = null;
                if (TargetType == TargetTypes.Pattern)
                    SelectedPatternGroup = null;
                else
                    SelectedColor = null;
            }
            foreach (Control ctl in pnlPages.Controls)
            {
                LinkLabel pageLink = ctl as LinkLabel;
                if (pageLink != null)
                {
                    ctl.Enabled = (int)pageLink.Tag != currentPageIndex;
                }
            }
            DisplayItems(currentPageIndex);
        }

        private void pageLabel_Clicked(object sender, EventArgs e)
        {
            try
            {
                SetCurrentPageIndex((int)((LinkLabel)sender).Tag);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private int GetItemCount()
        {
            switch (TargetType)
            {
                case TargetTypes.Pattern:
                    return FilteredPatternLists.Count;
                case TargetTypes.Color:
                    return PatternChoices.ColorChoices.Count;
                case TargetTypes.Palette:
                    return PatternChoices.PaletteChoices.Count;
                default:
                    throw new Exception("TargetType not implemented.");
            }
        }

        /// <summary>
        /// Create pictureboxes to hold patterns.
        /// </summary>
        private void ConfigureForm()
        {
            int totalWidth = (pictureBoxWidth + pictureBoxMargin);
            int totalHeight;
            if (TargetType == TargetTypes.Color)
                totalHeight = totalWidth;
            else if (TargetType == TargetTypes.Pattern)
                totalHeight = totalWidth + 20;
            else //if (TargetType == TargetTypes.Palette)
                totalHeight = pictureBoxHeight + pictureBoxMargin;
            BoxesAcross = (pnlPatterns.Width - pictureBoxMargin) / totalWidth;
            BoxesDown = (pnlPatterns.Height - pictureBoxMargin) / totalHeight;
            pictureBoxes = new PictureBox[BoxesAcross * BoxesDown];
            if (TargetType == TargetTypes.Pattern)
                nameTextBoxes = new TextBox[pictureBoxes.Length];
            int picInd = 0;
            int y = pictureBoxMargin;
            for (int yInd = 0; yInd < BoxesDown; yInd++)
            {
                int x = pictureBoxMargin;
                for (int xInd = 0; xInd < BoxesAcross; xInd++)
                {
                    PictureBox pic = new PictureBox();
                    pic.Width = pictureBoxWidth;
                    pic.Height = pictureBoxHeight;
                    pic.Top = y;
                    pic.Left = x;
                    pic.BackColor = Color.White;
                    pic.Paint += pictureBox_Paint;
                    pic.MouseDown += pictureBox_MouseDown;
                    pnlPatterns.Controls.Add(pic);
                    pictureBoxes[picInd] = pic;
                    if (TargetType == TargetTypes.Pattern)
                    {
                        TextBox txt = new TextBox();
                        txt.Width = pictureBoxWidth;
                        txt.Height = 16;
                        txt.Top = y + 1 + pictureBoxWidth;
                        txt.Left = x;
                        txt.Font = new Font(this.Font.FontFamily, 8);
                        txt.AcceptsReturn = true;
                        txt.KeyDown += txtNameTextBox_KeyDown;
                        pnlPatterns.Controls.Add(txt);
                        nameTextBoxes[picInd] = txt;
                    }
                    x += totalWidth;
                    picInd++;
                }
                y += totalHeight;
            }
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                PictureBox pic = (PictureBox)sender;
                if (pic.Image != null)
                    return;
                PatternList patternGroup = pic.Tag as PatternList;
                if (patternGroup != null)
                {
                    patternGroup.DrawFilled(e.Graphics, null, pic.Size, computeRandom: false, 
                                            checkTilePattern: false, enableCache: false);
                }
            }
            catch { }
        }

        private void SelectItem(PictureBox pic)
        {
            bool picSelected;
            if (TargetType == TargetTypes.Pattern)
            {
                PatternList patternGroup = pic.Tag as PatternList;
                picSelected = patternGroup != null;
                if (picSelected)
                {
                    SelectedPatternGroup = patternGroup;
                    int picInd = Array.IndexOf(pictureBoxes, pic);
                    for (int i = 0; i < nameTextBoxes.Length; i++)
                    {
                        nameTextBoxes[i].BackColor = i == picInd ? Color.PaleTurquoise : Color.White;
                    }
                }
            }
            else
            {
                int index = (int)pic.Tag;
                picSelected = index >= 0;
                if (picSelected)
                {
                    if (TargetType == TargetTypes.Color)
                        SelectedColor = pic.BackColor;
                    else if (TargetType == TargetTypes.Palette)
                        SelectedPalette = PatternChoices.PaletteChoices[index];
                }
            }
            if (picSelected)
            {
                if (selectedPictureBox != null)
                    selectedPictureBox.BorderStyle = BorderStyle.None;
                selectedPictureBox = pic;
                selectedPictureBox.BorderStyle = BorderStyle.Fixed3D;
            }
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                PictureBox pic = (PictureBox)sender;
                SelectItem(pic);
                if (e.Button == MouseButtons.Right)
                {
                    contextMenuStrip1.Show(pic, new Point(e.X, e.Y));
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void txtNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode != Keys.Enter)
                    return;
                e.Handled = true;
                TextBox txt = (TextBox)sender;
                PatternList patternList = txt.Tag as PatternList;
                if (patternList == null)
                    return;
                if (PatternChoices.SetPatternName(patternList, txt.Text))
                {
                    txt.Text = patternList.PatternListName;
                    PopulatedNamedPatternCombobox();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void removeItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedPictureBox != null)
                {
                    if (TargetType == TargetTypes.Pattern)
                    {
                        PatternChoices.RemovePatternGroup(SelectedPatternGroup);
                        FilteredPatternLists.Remove(SelectedPatternGroup);
                        if (SelectedPatternGroup.PatternListName != null)
                        {
                            PopulatedNamedPatternCombobox();
                        }
                        SelectedPatternGroup = null;
                        DisplayPatterns(currentPageIndex);
                    }
                    else 
                    {
                        int index = (int)selectedPictureBox.Tag;
                        if (index >= 0)
                        {
                            if (TargetType == TargetTypes.Color)
                            {
                                PatternChoices.RemoveColorAt(index);
                                SelectedColor = null;
                                DisplayColors(currentPageIndex);
                            }
                            else if (TargetType == TargetTypes.Palette)
                            {
                                PatternChoices.RemovePaletteAt(index);
                                SelectedPalette = null;
                                DisplayPalettes(currentPageIndex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetSelectedPatternGroup(PatternList patternGroup, int index)
        {
            string oldPatternName;
            if (patternGroup.PatternListName != null)
                oldPatternName = null;
            else
                oldPatternName = SelectedPatternGroup?.PatternListName;
            SelectedPatternGroup = patternGroup;
            SelectedPatternGroup.SetForPreview(new Size(pictureBoxWidth, pictureBoxWidth));
            PatternChoices.SetPatternGroup(SelectedPatternGroup, index);
            selectedPictureBox.Tag = SelectedPatternGroup;
            //SelectedPatternGroup.CheckCreateThumbnailImage();
            Task.Run(async () =>
            {
                await SelectedPatternGroup.CheckCreateThumbnailImageAsync();
                selectedPictureBox.Image = SelectedPatternGroup.ThumbnailImage;  //Could be null.
            });
            if (selectedPictureBox.Image == null)
                selectedPictureBox.Invalidate();
            if (oldPatternName != null && SelectedPatternGroup.PatternListName != oldPatternName)
                SelectedPatternGroup.PatternListName = oldPatternName;
            if (SelectedPatternGroup.PatternListName != null)
                PopulatedNamedPatternCombobox();
        }

        private PatternForm patternForm = null;

        private void editPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedPictureBox != null && SelectedPatternGroup != null)
                {
                    if (patternForm == null || patternForm.IsDisposed)
                        patternForm = new PatternForm();
                    int ind = PatternChoices.PatternGroups.IndexOf(SelectedPatternGroup);
                    if (ind == -1)
                        throw new Exception("Pattern to edit not found in choices.");
                    patternForm.Initialize(SelectedPatternGroup, this, Design);
                    if (patternForm.ShowDialog() == DialogResult.OK)
                    {
                        SetSelectedPatternGroup(patternForm.EditedPatternGroup, ind);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void setPatternFromDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedPictureBox == null)
                    return;
                if (TargetType == TargetTypes.Pattern)
                {
                    if (SelectedPatternGroup != null)
                    {
                        if (Design == null || Design.DefaultPatternGroup == null)
                            return;
                        int ind = PatternChoices.PatternGroups.IndexOf(SelectedPatternGroup);
                        if (ind == -1)
                            throw new Exception("Pattern to set not found in choices.");
                        SetSelectedPatternGroup(
                            Design.DefaultPatternGroup.GetCopy(copySharedPatternID: false), ind);
                    }
                }
                else if (TargetType == TargetTypes.Palette && 
                         ColorGradientFormComponent.CopiedColorNodes != null)
                {
                    int index = (int)selectedPictureBox.Tag;
                    if (index >= 0)
                    {
                        var palette = ColorGradientFormComponent.CopiedColorNodes.GetCopy();
                        PatternChoices.SetPalette(palette, index);
                        ShowPalette(selectedPictureBox, palette);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                string message = null;
                if (TargetType == TargetTypes.Pattern && SelectedPatternGroup == null)
                {
                    message = "Please click on a pattern to select it.";
                }
                else if (TargetType == TargetTypes.Color && SelectedColor == null)
                {
                    message = "Please click on a color to select it.";
                }
                else if (TargetType == TargetTypes.Palette && SelectedPalette == null)
                {
                    message = "Please click on a palette to select it.";
                }
                if (message != null)
                    MessageBox.Show(message, "Message");
                else
                {
                    this.DialogResult = DialogResult.OK;
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.DialogResult = DialogResult.Cancel;
                this.Hide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ClearFilters()
        {
            chkFilterPatterns.Checked = false;  //Event calls Init()
        }

        private bool GoToPatternList(PatternList patternList, bool allowClearFilter = true)
        {
            int index = FilteredPatternLists.IndexOf(patternList);
            if (index == -1 && allowClearFilter)
            {
                ClearFilters();
                index = FilteredPatternLists.IndexOf(patternList);
            }
            if (index == -1)
                return false;
            int pageIndex = index / pictureBoxes.Length;
            SetCurrentPageIndex(pageIndex, setPageRange: true);
            int picIndex = index % pictureBoxes.Length;
            SelectItem(pictureBoxes[picIndex]);
            return true;
        }

        private void cboPatternName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var patternListInfo = (PatternListInfo)cboPatternName.SelectedItem;
                if (patternListInfo?.PatternList != null)
                    GoToPatternList(patternListInfo.PatternList);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void ResizeForm()
        {
            if (pnlPatternFilters.Visible)
                pnlBottom.Top = pnlPatternFilters.Bottom + 2;
            else
                pnlBottom.Top = pnlPatternFilters.Top;
            this.ClientSize = new Size(ClientSize.Width, pnlBottom.Bottom + 2);
        }

        private void btnFilterPatterns_Click(object sender, EventArgs e)
        {
            pnlPatternFilters.Show();
            pnlPatternFilters.BringToFront();
            ResizeForm();
            btnFilterPatterns.Enabled = false;
        }

        private void btnCloseFilters_Click(object sender, EventArgs e)
        {
            pnlPatternFilters.Hide();
            ResizeForm();
            btnFilterPatterns.Enabled = true;
        }

        private void btnApplyFilters_Click(object sender, EventArgs e)
        {
            try
            {
                if (!chkFilterPatterns.Checked)
                    chkFilterPatterns.Checked = true;  //Event calls Init()
                else
                    Init();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void chkFilterPatterns_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                Init();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void cboPatternType_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkPatternType.Checked = true;
            OnChangePatternType();
        }

        private void OnChangePatternType()
        {
            if (cboPatternType.SelectedItem is FilterPatternTypes)
            {
                bool showRibbonType = (FilterPatternTypes)cboPatternType.SelectedItem == FilterPatternTypes.Ribbon;
                pnlRibbonFilters.Visible = showRibbonType;
                if (showRibbonType)
                    pnlPatternFilters.Height = pnlRibbonFilters.Bottom + 5;
                else
                    pnlPatternFilters.Height = pnlRibbonFilters.Top - 1;
            }
        }

        private void cboBasicOutlineType_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkBasicOutlineType.Checked = true;
        }

        private void cboTransformName_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkTransform.Checked = true;
        }

        private void cboRibbonType_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkRibbonType.Checked = true;
        }
    }
}
