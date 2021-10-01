using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public class ColorGradientFormComponent: IColor
    {
        public static ColorNodeList CopiedColorNodes { get; private set; }

        const int nodeSize = 6;

        public EventHandler GradientChanged;
        public MouseEventHandler ShowContextMenu;
        private ColorNodeList _colorNodeList = new ColorNodeList();
        public ColorNodeList ColorNodes
        {
            get { return _colorNodeList; }
            set
            {
                if (value == null)
                    throw new Exception("ColorNodes cannot be set to null.");
                if (value.Count < 2)
                    return;
                _colorNodeList = value;
                RefreshGradient(raiseEvent: false);
            }
        }
        private ColorNode selectedColorNode { get; set; }
        private System.Windows.Forms.PictureBox picGradient;
        private LinearGradientBrush linearGradientBrush { get; set; }
        private Point mouseDownPoint { get; set; }
        private bool mouseDragging { get; set; }
        private ColorTransparencyForm colorTransparencyForm { get; set; }
        private PaintEventHandler currentPaintHandler { get; set; }
        public bool IsGradientLocked { get; }
        private Rectangle clientRectangle { get; }
        private Form owner { get; }
        public Form frmMain { get; set; }
        public Form frmParent { get; set; }


        public Color TransparencyColor
        {
            get
            {
                if (selectedColorNode != null)
                    return selectedColorNode.Color;
                else
                    return Color.Black;
            }
            set
            {
                if (selectedColorNode != null && selectedColorNode.Color != value)
                {
                    selectedColorNode.Color = value;
                    RefreshGradient();
                }
            }
        }

        public ColorGradientFormComponent(Size picSize)
        {
            clientRectangle = new Rectangle(new Point(0, 0), picSize);
            IsGradientLocked = true;
            ColorNodes.AddDefaultNodes();
            InitializeGradient();
        }

        public ColorGradientFormComponent(PictureBox picGradient, Form owner, bool lockGradient = false)
        {
            try
            {
                this.picGradient = picGradient;
                this.owner = owner;
                clientRectangle = picGradient.ClientRectangle;
                IsGradientLocked = lockGradient;
                currentPaintHandler = new System.Windows.Forms.PaintEventHandler(this.picGradient_Paint);
                this.picGradient.Paint += currentPaintHandler;
                if (!lockGradient)
                {
                    this.picGradient.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picGradient_MouseDown);
                    this.picGradient.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picGradient_MouseMove);
                    this.picGradient.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picGradient_MouseUp);
                }
                ColorNodes.AddDefaultNodes();
                InitializeGradient();
                PaintGradient();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void SetPaintHandler(PaintEventHandler handler = null)
        {
            if (picGradient == null)
                return;
            if (currentPaintHandler != null)
                picGradient.Paint -= currentPaintHandler;
            if (handler == null)
                handler = picGradient_Paint;
            picGradient.Paint += handler;
            currentPaintHandler = handler;
        }

        public void InitializeGradient()
        {
            if (linearGradientBrush != null)
                linearGradientBrush.Dispose();
            linearGradientBrush = new LinearGradientBrush(clientRectangle, Color.Black, Color.White, angle: 0);
            var colorBlend = new ColorBlend();
            SetColorBlend(colorBlend);
            linearGradientBrush.InterpolationColors = colorBlend;
        }

        private IEnumerable<ColorNode> SetColorBlend(ColorBlend colorBlend)
        {
            var colorNodes = ColorNodes.GetColorBlendNodes();
            colorBlend.Colors = colorNodes.Select(cn => cn.Color).ToArray();
            colorBlend.Positions = colorNodes.Select(cn => cn.Position).ToArray();
            return colorNodes;
        }

        private void RefreshGradient(bool raiseEvent = true)
        {
            ColorBlend colorBlend = new ColorBlend();
            var colorNodes = SetColorBlend(colorBlend);
            try
            {
                linearGradientBrush.InterpolationColors = colorBlend;
            }
            catch (Exception ex)
            {
                throw new Exception($"Positions = {string.Join(",", colorNodes.Select(cn => cn.Position))}", ex);
            }
            PaintGradient();
            if (raiseEvent)
            {
                RaiseChangedEvent();
            }
        }

        private void RaiseChangedEvent()
        {
            GradientChanged?.Invoke(ColorNodes, EventArgs.Empty);
        }

        public void PaintGradient()
        {
            if (picGradient != null)
                picGradient.Refresh();
        }

        private int GetNodeX(ColorNode node)
        {
            return (int)(node.Position * (clientRectangle.Width - nodeSize));
        }

        private float GetNodePosition(int x)
        {
            return Math.Max(0F, Math.Min(1F, (float)x / (clientRectangle.Width - nodeSize)));
        }

        private ColorNode FindNearestNode(int x)
        {
            return ColorNodes.FindNearestNode(GetNodePosition(x));
        }

        private void DrawNode(Graphics g, ColorNode node)
        {
            Color fillColor = node.Selected ? Color.Aqua : Color.White;
            Color borderColor = Color.Black;
            int x = GetNodeX(node);
            Rectangle rectangle = new Rectangle(new Point(x, 0), new Size(nodeSize, nodeSize));
            using (var brush = new SolidBrush(fillColor))
            {
                g.FillRectangle(brush, rectangle);
            }
            using (var pen = new Pen(borderColor))
            {
                g.DrawRectangle(pen, rectangle);
            }
        }

        private bool SetNodeSelected(ColorNode node)
        {
            if (node == selectedColorNode)
                return false;
            foreach (ColorNode n in ColorNodes.ColorNodes)
            {
                n.Selected = (n == node);
            }
            selectedColorNode = node;
            return true;
        }

        public Image GetGradientImage()
        {
            var bitmap = new Bitmap(clientRectangle.Width, clientRectangle.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(linearGradientBrush, clientRectangle);
            }
            return bitmap;
        }

        private void picGradient_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.FillRectangle(linearGradientBrush, clientRectangle);
                foreach (ColorNode node in ColorNodes.ColorNodes)
                {
                    DrawNode(e.Graphics, node);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool PointIsAtNode(ColorNode node, Point p)
        {
            if (node == null)
                return false;
            Point nodeP = new Point(GetNodeX(node), 0);
            return p.Y <= nodeSize && p.X >= nodeP.X && p.X - nodeP.X <= nodeSize;
        }

        private void SetSelectedNodePosition(int x, bool raiseEvent)
        {
            if (selectedColorNode != null)
            {
                float position = GetNodePosition(x);
                var node2 = FindNearestNode(x);
                if (node2 != null && node2 != selectedColorNode && node2.Position == position)
                    return;
                selectedColorNode.Position = position;
                ColorNodes.SortNodes();
                RefreshGradient(raiseEvent);
            }
        }

        private void picGradient_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                ColorNode node = FindNearestNode(e.X);
                if (node == null)
                    return;
                mouseDownPoint = e.Location;
                bool nodeIsValid = PointIsAtNode(node, e.Location);
                if (e.Button == MouseButtons.Left)
                {
                    if (SetNodeSelected(node))
                        PaintGradient();
                    mouseDragging = nodeIsValid;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    ShowContextMenu?.Invoke(this, e);
                    //deleteColorToolStripMenuItem.Enabled = nodeIsValid && ColorNodes.Count > ColorNodeList.MinCount;
                    //gradientContextMenu.Show(picGradient, e.Location);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void picGradient_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!(mouseDragging && e.Button == MouseButtons.Left))
                    return;
                SetSelectedNodePosition(e.X, raiseEvent: false);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void picGradient_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (mouseDragging)
                {
                    mouseDragging = false;
                    if (e.Button == MouseButtons.Left)
                    {
                        SetSelectedNodePosition(e.X, raiseEvent: true);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void SetNodeColor(ColorNode node, Color color)
        {
            int alpha = node.Color.A;
            node.Color = Color.FromArgb(alpha, color);
        }

        public void pickColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ColorNode node = FindNearestNode(mouseDownPoint.X);
                if (node == null)
                    return;
                var dlg = new ColorDialog();
                dlg.Color = node.Color;
                if (dlg.ShowDialog(owner) == DialogResult.OK)
                {
                    SetNodeColor(node, dlg.Color);
                    RefreshGradient();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void Redisplay()
        {
            if (frmMain != null)
                frmMain.BringToFront();
            if (frmParent != null)
                frmParent.BringToFront();
        }

        public void chooseColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ColorNode node = FindNearestNode(mouseDownPoint.X);
                if (node == null)
                    return;
                MainForm.SelectColorForm.Initialize();
                if (MainForm.SelectColorForm.ShowDialog(owner) == DialogResult.OK)
                {
                    SetNodeColor(node, (Color)MainForm.SelectColorForm.SelectedColor);
                    RefreshGradient();
                }
                Redisplay();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void AddColor(Color color)
        {
            float position = GetNodePosition(mouseDownPoint.X);
            var node = new ColorNode(color, position);
            ColorNodes.AddNode(node);
            ColorNodes.SortNodes();
            RefreshGradient();
        }

        private Color GetColorAtX(int x)
        {
            float position = GetNodePosition(x);
            return ColorNodes.GetColorAtPosition(position);
        }

        public void addColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new ColorDialog();
                dlg.Color = GetColorAtX(mouseDownPoint.X);
                if (dlg.ShowDialog(owner) == DialogResult.OK)
                {
                    AddColor(dlg.Color);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void chooseAddedColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MainForm.SelectColorForm.Initialize();
                if (MainForm.SelectColorForm.ShowDialog(owner) == DialogResult.OK)
                {
                    AddColor((Color)MainForm.SelectColorForm.SelectedColor);
                }
                Redisplay();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void deleteColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorNode node = FindNearestNode(mouseDownPoint.X);
            if (node == null || !PointIsAtNode(node, mouseDownPoint))
                return;
            if (ColorNodes.RemoveNode(node))
                RefreshGradient();
        }

        public void editTransparencyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var node = FindNearestNode(mouseDownPoint.X);
                if (node == null)
                    return;
                SetNodeSelected(node);
                if (colorTransparencyForm == null || colorTransparencyForm.IsDisposed)
                    colorTransparencyForm = new ColorTransparencyForm();
                colorTransparencyForm.Initialize(this);
                Tools.DisplayForm(colorTransparencyForm, owner);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void addColorToChoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Color newColor = GetColorAtX(mouseDownPoint.X);
                if (!MainForm.SelectColorForm.PatternChoices.AddColor(newColor))
                    MessageBox.Show("This color has already been added to the color choices.", "Message");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void copyGradientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                CopiedColorNodes = ColorNodes.GetCopy();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void pasteGradientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (CopiedColorNodes != null)
                {
                    ColorNodes = CopiedColorNodes.GetCopy();
                    RaiseChangedEvent();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void addGradientToChoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!MainForm.SelectPaletteForm.PatternChoices.AddPalette(ColorNodes.GetCopy()))
                    MessageBox.Show("This palette has already been added to choices.");
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        public void selectGradientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MainForm.SelectPaletteForm.Initialize();
                if (MainForm.SelectPaletteForm.ShowDialog(owner) == DialogResult.OK)
                {
                    ColorNodes = MainForm.SelectPaletteForm.SelectedPalette.GetCopy();
                    RaiseChangedEvent();
                }
                Redisplay();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

    }
}
