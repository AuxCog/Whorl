using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public partial class frmColorGradient : Form
    {
        public frmColorGradient()
        {
            InitializeComponent();
            Component = new ColorGradientFormComponent(picGradient, this);
            Component.frmParent = this;
            Component.ShowContextMenu += showContextMenu;
            this.pickColorToolStripMenuItem.Click += new System.EventHandler(Component.pickColorToolStripMenuItem_Click);
            this.addColorToolStripMenuItem.Click += new System.EventHandler(Component.addColorToolStripMenuItem_Click);
            this.deleteColorToolStripMenuItem.Click += new System.EventHandler(Component.deleteColorToolStripMenuItem_Click);
            this.editTransparencyToolStripMenuItem.Click += new System.EventHandler(Component.editTransparencyToolStripMenuItem_Click);
            this.chooseColorToolStripMenuItem.Click += new System.EventHandler(Component.chooseColorToolStripMenuItem_Click);
            this.addColorToChoicesToolStripMenuItem.Click += new System.EventHandler(Component.addColorToChoicesToolStripMenuItem_Click);
            this.copyGradientToolStripMenuItem.Click += new System.EventHandler(Component.copyGradientToolStripMenuItem_Click);
            this.pasteGradientToolStripMenuItem.Click += new System.EventHandler(Component.pasteGradientToolStripMenuItem_Click);
            this.addPaletteToChoicesToolStripMenuItem.Click += 
                new EventHandler(Component.addGradientToChoicesToolStripMenuItem_Click);
            this.selectPaletteToolStripMenuItem.Click +=
                new EventHandler(Component.selectGradientToolStripMenuItem_Click);
            //try
            //{
            //    ColorNodes.AddNode(new ColorNode(Color.Black, 0));
            //    ColorNodes.AddNode(new ColorNode(Color.White, 1));
            //    InitializeGradient();
            //    PaintGradient();
            //}
            //catch (Exception ex)
            //{
            //    Tools.HandleException(ex);
            //}
        }

        private void showContextMenu(object sender, MouseEventArgs e)
        {
            gradientContextMenu.Show(picGradient, e.Location);
        }

        public ColorGradientFormComponent Component { get; }

        private void frmColorGradient_Resize(object sender, EventArgs e)
        {
            try
            {
                if (WindowState == FormWindowState.Minimized)
                    return;
                Component.InitializeGradient();
                Component.PaintGradient();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void frmColorGradient_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Hide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //const int nodeSize = 6;

        //public EventHandler GradientChanged;
        //private ColorNodeList _colorNodeList = new ColorNodeList();
        //public ColorNodeList ColorNodes
        //{
        //    get { return _colorNodeList; }
        //    set
        //    {
        //        if (value == null)
        //            throw new Exception("ColorNodes cannot be set to null.");
        //        if (value.Count < 2)
        //            return;
        //        _colorNodeList = value;
        //        RefreshGradient(raiseEvent: false);
        //    }
        //}
        //private ColorNode selectedColorNode { get; set; }
        //private LinearGradientBrush linearGradientBrush { get; set; }
        //private Point mouseDownPoint { get; set; }
        //private bool mouseDragging { get; set; }

        //private void InitializeGradient()
        //{
        //    if (linearGradientBrush != null)
        //        linearGradientBrush.Dispose();
        //    linearGradientBrush = new LinearGradientBrush(picGradient.ClientRectangle, Color.Black, Color.White, angle: 0);
        //    var colorBlend = new ColorBlend();
        //    SetColorBlend(colorBlend);
        //    linearGradientBrush.InterpolationColors = colorBlend;
        //}

        ///// <summary>
        ///// Return color nodes for wrapping color gradient around from last node to first.
        ///// </summary>
        ///// <returns></returns>
        //private IEnumerable<ColorNode> GetColorBlendNodes()
        //{
        //    var firstNode = ColorNodes.ColorNodes.FirstOrDefault();
        //    var lastNode = ColorNodes.ColorNodes.LastOrDefault();
        //    if (firstNode == null)
        //        return ColorNodes.ColorNodes;
        //    Color boundColor;
        //    if ((firstNode.Position > 0 || lastNode.Position < 1) && firstNode.Position < lastNode.Position)
        //    {
        //        float factor = (1F - lastNode.Position) / (1F - lastNode.Position + firstNode.Position);
        //        boundColor = ColorGradient.FloatColor.InterpolateColor(lastNode.FloatColor, firstNode.FloatColor, factor);
        //        var colorNodes = new List<ColorNode>(ColorNodes.ColorNodes);
        //        if (firstNode.Position > 0)
        //        {
        //            colorNodes.Insert(0, new ColorNode(boundColor, 0));
        //        }
        //        if (lastNode.Position < 1)
        //        {
        //            colorNodes.Add(new ColorNode(boundColor, 1));
        //        }
        //        return colorNodes;
        //    }
        //    else
        //        return ColorNodes.ColorNodes;
        //}

        //private void SetColorBlend(ColorBlend colorBlend)
        //{
        //    var colorNodes = GetColorBlendNodes();
        //    colorBlend.Colors = colorNodes.Select(cn => cn.Color).ToArray();
        //    colorBlend.Positions = colorNodes.Select(cn => cn.Position).ToArray();
        //}

        //private void RefreshGradient(bool raiseEvent = true)
        //{
        //    ColorBlend colorBlend = new ColorBlend();
        //    SetColorBlend(colorBlend);
        //    linearGradientBrush.InterpolationColors = colorBlend;
        //    PaintGradient();
        //    if (raiseEvent)
        //    {
        //        GradientChanged?.Invoke(ColorNodes, EventArgs.Empty);
        //    }
        //}

        //private void PaintGradient()
        //{
        //    picGradient.Refresh();
        //}

        //private int GetNodeX(ColorNode node)
        //{
        //    return (int)(node.Position * (picGradient.ClientRectangle.Width - nodeSize));
        //}

        //private float GetNodePosition(int x)
        //{
        //    return Math.Min(1F, (float)x / (picGradient.ClientRectangle.Width - nodeSize));
        //}

        //private ColorNode FindNearestNode(int x)
        //{
        //    return ColorNodes.FindNearestNode(GetNodePosition(x));
        //}

        //private void DrawNode(Graphics g, ColorNode node)
        //{
        //    Color fillColor = node.Selected ? Color.Aqua : Color.White;
        //    Color borderColor = Color.Black;
        //    int x = GetNodeX(node);
        //    Rectangle rectangle = new Rectangle(new Point(x, 0), new Size(nodeSize, nodeSize));
        //    using (var brush = new SolidBrush(fillColor))
        //    {
        //        g.FillRectangle(brush, rectangle);
        //    }
        //    using (var pen = new Pen(borderColor))
        //    {
        //        g.DrawRectangle(pen, rectangle);
        //    }
        //}

        //private bool SetNodeSelected(ColorNode node)
        //{
        //    if (node == selectedColorNode)
        //        return false;
        //    foreach (ColorNode n in ColorNodes.ColorNodes)
        //    {
        //        n.Selected = (n == node);
        //    }
        //    selectedColorNode = node;
        //    return true;
        //}

        //private void picGradient_Paint(object sender, PaintEventArgs e)
        //{
        //    try
        //    {
        //        e.Graphics.FillRectangle(linearGradientBrush, picGradient.ClientRectangle);
        //        foreach (ColorNode node in ColorNodes.ColorNodes)
        //        {
        //            DrawNode(e.Graphics, node);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private bool PointIsAtNode(ColorNode node, Point p)
        //{
        //    if (node == null)
        //        return false;
        //    Point nodeP = new Point(GetNodeX(node), 0);
        //    return p.Y <= nodeSize && p.X >= nodeP.X && p.X - nodeP.X <= nodeSize;
        //}

        //private void SetSelectedNodePosition(int x, bool raiseEvent)
        //{
        //    if (selectedColorNode != null)
        //    {
        //        selectedColorNode.Position = GetNodePosition(x);
        //        ColorNodes.SortNodes();
        //        RefreshGradient(raiseEvent);
        //    }
        //}

        //private void picGradient_MouseDown(object sender, MouseEventArgs e)
        //{
        //    try
        //    {
        //        ColorNode node = FindNearestNode(e.X);
        //        if (node == null)
        //            return;
        //        mouseDownPoint = e.Location;
        //        bool nodeIsValid = PointIsAtNode(node, e.Location);
        //        if (e.Button == MouseButtons.Left)
        //        {
        //            if (SetNodeSelected(node))
        //                PaintGradient();
        //            mouseDragging = nodeIsValid;
        //        }
        //        else if (e.Button == MouseButtons.Right)
        //        {
        //            deleteColorToolStripMenuItem.Enabled = nodeIsValid && ColorNodes.Count > ColorNodeList.MinCount;
        //            gradientContextMenu.Show(picGradient, e.Location);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void picGradient_MouseMove(object sender, MouseEventArgs e)
        //{
        //    try
        //    {
        //        if (!(mouseDragging && e.Button == MouseButtons.Left))
        //            return;
        //        SetSelectedNodePosition(e.X, raiseEvent: false);
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void picGradient_MouseUp(object sender, MouseEventArgs e)
        //{
        //    try
        //    {
        //        if (mouseDragging)
        //        {
        //            mouseDragging = false;
        //            if (e.Button == MouseButtons.Left)
        //            {
        //                SetSelectedNodePosition(e.X, raiseEvent: true);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void pickColorToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        ColorNode node = FindNearestNode(mouseDownPoint.X);
        //        if (node == null)
        //            return;
        //        var dlg = new ColorDialog();
        //        dlg.Color = node.Color;
        //        if (dlg.ShowDialog() == DialogResult.OK)
        //        {
        //            node.Color = dlg.Color;
        //            RefreshGradient();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void chooseColorToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        ColorNode node = FindNearestNode(mouseDownPoint.X);
        //        if (node == null)
        //            return;
        //        MainForm.SelectColorForm.Initialize();
        //        if (MainForm.SelectColorForm.ShowDialog() == DialogResult.OK)
        //        {
        //            node.Color = (Color)MainForm.SelectColorForm.SelectedColor;
        //            RefreshGradient();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}


        //private void addColorToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var dlg = new ColorDialog();
        //        if (dlg.ShowDialog() == DialogResult.Cancel)
        //            return;
        //        float position = GetNodePosition(mouseDownPoint.X);
        //        var node = new ColorNode(dlg.Color, position);
        //        ColorNodes.AddNode(node);
        //        ColorNodes.SortNodes();
        //        RefreshGradient();
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        //private void deleteColorToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    ColorNode node = FindNearestNode(mouseDownPoint.X);
        //    if (node == null)
        //        return;
        //    if (ColorNodes.RemoveNode(node))
        //        RefreshGradient();
        //}

    }
}
