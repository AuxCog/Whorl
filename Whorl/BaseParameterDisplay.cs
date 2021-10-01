using ParserEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
    public class ParameterChangedEventArgs : EventArgs
    {
        public bool RefreshDisplay { get; }

        public ParameterChangedEventArgs(bool refreshDisplay)
        {
            RefreshDisplay = refreshDisplay;
        }
    }

    public class ParameterActionEventArgs : EventArgs
    {
        public ParameterActions ParameterAction { get; }
        public BaseParameterDisplay BaseParameterDisplay { get; }

        public ParameterActionEventArgs(ParameterActions parameterAction, BaseParameterDisplay baseParameterDisplay)
        {
            ParameterAction = parameterAction;
            BaseParameterDisplay = baseParameterDisplay;
        }
    }

    public enum ParameterActions
    {
        //None,
        Incrementer,
        CopyParameters,
        PasteParameters
    }

    public class ParameterDisplaysContainer
    {
        public class ParameterDisplays
        {
            public ParameterDisplay ParameterDisplay { get; }
            public CSharpParameterDisplay CSharpParameterDisplay { get; }
            public BaseParameterDisplay BaseParameterDisplay { get; private set; }
            public bool IsCSharp { get; private set; }
            private ContextMenu contextMenu { get; set; }
            //private Control selectedControl { get; set; }

            public ParameterDisplays(Panel pnlParameters,
                                     BaseParameterDisplay.ParamChanged paramChanged,
                                     BaseParameterDisplay.ActionSelectedDelegate actionSelected,
                                     int controlWidth = BaseParameterDisplay.defaultControlWidth,
                                     bool singleColumn = false)
            {
                this.ParameterDisplay = new ParameterDisplay(pnlParameters, paramChanged, 
                                        actionSelected, controlWidth);
                this.CSharpParameterDisplay = new CSharpParameterDisplay(pnlParameters, paramChanged, 
                                        actionSelected, singleColumn, controlWidth);
                AddContextMenu(pnlParameters);
                pnlParameters.MouseDown += new MouseEventHandler(PnlParameters_MouseDown);
            }

            public void AfterControlsAdded(Panel pnlParameters)
            {
                foreach (Control control in pnlParameters.Controls)
                {
                    if (IsParameterLabel(control))
                        control.MouseDown += new MouseEventHandler(PnlParameters_MouseDown);
                }
            }

            private bool IsParameterLabel(Control control)
            {
                return control is Label;
            }

            private void PnlParameters_MouseDown(object sender, MouseEventArgs e)
            {
                try
                {
                    if (e.Button != MouseButtons.Right)
                        return;
                    Panel panel;
                    Label label = sender as Label;
                    Point location;
                    if (label == null)
                    {
                        panel = (Panel)sender;
                        location = e.Location;
                        label = (from Control ctl in panel.Controls select ctl)
                                    .Where(ctl => IsParameterLabel(ctl))
                                    .OrderBy(ctl => Tools.DistanceSquared(e.Location, ctl.Location))
                                    .FirstOrDefault() as Label;
                    }
                    else
                    {
                        panel = (Panel)label.Parent;
                        location = new Point(e.X + label.Location.X, e.Y + label.Location.Y);
                    }
                    BaseParameterDisplay.SelectedLabel = label;
                    foreach (MenuItem menuItem in contextMenu.MenuItems)
                        menuItem.Tag = BaseParameterDisplay;
                    contextMenu.Show(panel, location);
                }
                catch (Exception ex)
                {
                    Tools.HandleException(ex);
                }
            }

            public void SetIsCSharp(bool isCSharp)
            {
                IsCSharp = isCSharp;
                BaseParameterDisplay = GetBaseParameterDisplay(isCSharp);
            }

            private BaseParameterDisplay GetBaseParameterDisplay(bool isCSharp)
            {
                if (isCSharp)
                    return CSharpParameterDisplay;
                else
                    return ParameterDisplay;
            }

            private MenuItem GetMenuItem(ParameterActions parameterAction)
            {
                switch (parameterAction)
                {
                    case ParameterActions.CopyParameters:
                        return new MenuItem("Copy Parameters", CopyParameters_Clicked);
                    case ParameterActions.PasteParameters:
                        return new MenuItem("Paste Parameters", PasteParameters_Clicked);
                    case ParameterActions.Incrementer:
                    default:
                        return new MenuItem("Incrementer", Incrementer_Clicked);
                }
            }

            private void AddContextMenu(Panel pnlParameters)
            {
                var mnuItems = new List<MenuItem>();
                foreach (ParameterActions parameterAction in Enum.GetValues(typeof(ParameterActions)))
                {
                    var mnuItem = GetMenuItem(parameterAction);
                    mnuItem.Tag = this.BaseParameterDisplay;
                    mnuItems.Add(mnuItem);
                }
                contextMenu = new ContextMenu(mnuItems.ToArray());
                //if (pnlParameters.ContextMenu != null)
                //    pnlParameters.ContextMenu.Dispose();
                //pnlParameters.ContextMenu = contextMenu;
            }

            private static BaseParameterDisplay GetBaseParameterDisplay(object sender)
            {
                MenuItem menuItem = (MenuItem)sender;
                return menuItem.Tag as BaseParameterDisplay;
                //var paramDisplays = menuItem.Tag as ParameterDisplaysContainer.ParameterDisplays;
                //return paramDisplays?.BaseParameterDisplay;
            }

            private static void CopyParameters_Clicked(object sender, EventArgs e)
            {
                BaseParameterDisplay baseParameterDisplay = GetBaseParameterDisplay(sender);
                if (baseParameterDisplay != null)
                    baseParameterDisplay.CopyParameters();
            }

            private static void PasteParameters_Clicked(object sender, EventArgs e)
            {
                BaseParameterDisplay baseParameterDisplay = GetBaseParameterDisplay(sender);
                if (baseParameterDisplay != null)
                    baseParameterDisplay.PasteParameters();
            }

            private static void Incrementer_Clicked(object sender, EventArgs e)
            {
                BaseParameterDisplay baseParameterDisplay = GetBaseParameterDisplay(sender);
                if (baseParameterDisplay != null)
                {
                    if (baseParameterDisplay.SelectedLabel != null)
                    {
                        if (MessageBox.Show(baseParameterDisplay.SelectedLabel.Text, "Selection", MessageBoxButtons.OKCancel)
                                            == DialogResult.Cancel)
                            return;
                    }
                    baseParameterDisplay.Incrementer();
                }
            }
        }

        private Dictionary<Panel, ParameterDisplays> dictDisplays { get; } =
            new Dictionary<Panel, ParameterDisplays>();

        public ParameterDisplaysContainer(IEnumerable<Panel> parameterPanels, 
                                          BaseParameterDisplay.ParamChanged paramChanged,
                                          BaseParameterDisplay.ActionSelectedDelegate actionSelected,
                                          bool singleColumn = false)
        {
            foreach (Panel panel in parameterPanels)
            {
                dictDisplays.Add(panel, new ParameterDisplays(panel, paramChanged, actionSelected,
                                 singleColumn: singleColumn));
            }
        }

        public ParameterDisplays GetParameterDisplays(Panel pnl, bool throwException = false)
        {
            if (!dictDisplays.TryGetValue(pnl, out ParameterDisplays parameterDisplays))
            {
                if (throwException)
                    throw new Exception($"Parameters panel {pnl.Name} was not initialized.");
                else
                    parameterDisplays = null;
            }
            return parameterDisplays;
        }

        public void AddParametersControls(Panel pnlParameters, FormulaSettings formulaSettings,
                                          Pattern sourcesPattern = null)
        {
            var parameterDisplays = GetParameterDisplays(pnlParameters, throwException: true);
            parameterDisplays.SetIsCSharp(formulaSettings.IsCSharpFormula);
            parameterDisplays.BaseParameterDisplay.AddAllParametersControls(formulaSettings, sourcesPattern);
            parameterDisplays.AfterControlsAdded(pnlParameters);
        }

        public void RefreshComboBoxes(Panel pnlParameters)
        {
            var parameterDisplays = GetParameterDisplays(pnlParameters, throwException: true);
            if (parameterDisplays.BaseParameterDisplay != null)
                parameterDisplays.BaseParameterDisplay.RefreshComboBoxes();
        }
    }

    public abstract class BaseParameterDisplay
    {
        public static FormulaSettings CopiedFormulaSettings { get; private set; }

        protected class ParameterActionItem
        {
            public ParameterActions ParameterAction { get; }
            public string Text { get; }

            public ParameterActionItem(ParameterActions parameterAction, string text)
            {
                ParameterAction = parameterAction;
                Text = text;
            }

            private static string GetText(ParameterActions parameterAction)
            {
                switch (parameterAction)
                {
                    //case ParameterActions.None:
                    //    return string.Empty;
                    case ParameterActions.CopyParameters:
                        return "Copy Parameters";
                    case ParameterActions.PasteParameters:
                        return "Paste Parameters";
                    default:
                        return parameterAction.ToString();
                }
            }

            public static List<ParameterActionItem> GetItems()
            {
                return (from ParameterActions a in Enum.GetValues(typeof(ParameterActions))
                        select new ParameterActionItem(a, GetText(a))).ToList();
            }

            public override string ToString()
            {
                return Text;
            }
        }
        public const int defaultControlWidth = 85;
        protected const int 
                  critTopMargin = 2,
                  critLeftMargin = 3,
                  critRowHeight = 25;
        protected int critControlWidth { get; }
        protected int critColumnWidth { get; }
        protected int critColumnCount { get; private set; }
        protected bool handleEvents { get; set; }
        public FormulaSettings FormulaSettings { get; protected set; }
        public Panel ParametersPanel { get; }

        public delegate bool delEditTransformInfluenceLink(string parameterName);
        public delEditTransformInfluenceLink FnEditInfluenceLink { get; set; }


        public delegate void ParamChanged(object sender, ParameterChangedEventArgs e);
        public event ParamChanged ParameterChanged;

        public delegate void ActionSelectedDelegate(object sender, ParameterActionEventArgs e);
        public event ActionSelectedDelegate ActionSelected;
        public bool SingleColumn { get; }

        public Label SelectedLabel { get; set; }

        public static void ClearParametersControls(Panel pnlParameters)
        {
            foreach (Control ctl in pnlParameters.Controls)
            {
                ctl.Dispose();
            }
            pnlParameters.Controls.Clear();
        }

        public BaseParameterDisplay(Panel pnlParameters, ParamChanged paramChangedFn, 
                                    ActionSelectedDelegate actionSelectedFn = null, 
                                    int controlWidth = defaultControlWidth,
                                    bool singleColumn = false)
        {
            SingleColumn = singleColumn;
            critControlWidth = controlWidth;
            critColumnWidth = controlWidth + critLeftMargin;
            ParametersPanel = pnlParameters;
            ParameterChanged += paramChangedFn;
            if (actionSelectedFn != null)
                ActionSelected += actionSelectedFn;
            if (singleColumn)
                critColumnCount = 2;
            else
            {
                critColumnCount = (pnlParameters.ClientSize.Width - critLeftMargin) / critColumnWidth;
                if (critColumnCount == 0)
                    throw new Exception($"Parameters panel {pnlParameters.Name} is not wide enough to hold controls.");
            }
        }

        public virtual void SetFormulaSettings(FormulaSettings formulaSettings)
        {
            FormulaSettings = formulaSettings;
            
        }

        public virtual void AddAllParametersControls(FormulaSettings formulaSettings,
                                                     Pattern sourcesPattern = null)
        {
            
            SetFormulaSettings(formulaSettings);
            ClearParametersControls(ParametersPanel);
        }

        public abstract void RefreshComboBoxes();

        protected void GetControlPosition(int parameterIndex, out int left, out int top)
        {
            int rowIndex, colIndex;
            rowIndex = parameterIndex / critColumnCount;
            colIndex = parameterIndex % critColumnCount;
            left = critLeftMargin + colIndex * critColumnWidth;
            top = critTopMargin + rowIndex * critRowHeight;
        }

        protected LinkLabel CreateInfluenceLinkLabel(string parameterName, ref int txtWidth)
        {
            var lnkInfluence = new LinkLabel()
            {
                AutoSize = false,
                Width = 15,
                Tag = parameterName
            };
            SetInfluenceLinkLabelText(lnkInfluence);
            lnkInfluence.Click += LnkInfluence_Click;
            txtWidth -= lnkInfluence.Width;
            return lnkInfluence;
        }

        public void UpdateInfluenceLinkLabelsText()
        {
            foreach (Control control in ParametersPanel.Controls)
            {
                var linkLabel = control as LinkLabel;
                if (linkLabel == null || !(linkLabel.Tag is Parameter))
                    continue;
                SetInfluenceLinkLabelText(linkLabel);
            }
        }

        private void SetInfluenceLinkLabelText(LinkLabel linkLabel)
        {
            string parameterName = (string)linkLabel.Tag;
            bool haveInfluenceLinks = false;
            var parentCollection = FormulaSettings?.InfluenceLinkParentCollection;
            if (parentCollection != null &&
                parentCollection.InfluenceLinkParentsByParameterName.TryGetValue(
                                 parameterName, out var linkParent))
            {
                haveInfluenceLinks = linkParent.InfluenceLinks.Any();
            }
            linkLabel.Text = haveInfluenceLinks ? "i*" : "i";
        }

        private void LnkInfluence_Click(object sender, EventArgs e)
        {
            try
            {
                if (FnEditInfluenceLink == null) return;
                var lnkInfluence = (LinkLabel)sender;
                string parameterName = (string)lnkInfluence.Tag;
                if (FnEditInfluenceLink(parameterName))
                {
                    UpdateInfluenceLinkLabelsText();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        //private void ParametersPanel_MouseDown(object sender, MouseEventArgs e)
        //{
        //    try
        //    {
        //        var panel = (Panel)sender;
        //        if (e.Button == MouseButtons.Right && panel.ContextMenu != null)
        //        {
        //            panel.ContextMenu.Show(panel, e.Location);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Tools.HandleException(ex);
        //    }
        //}

        public void AddActionComboBox(ref int parameterIndex)
        {
            //GetControlPosition(parameterIndex, out int left, out int top);
            //AddLabel("Action", 1, top, ref left, align: false);
            //var comboBox = new ComboBox();
            //comboBox.AutoSize = true;
            //comboBox.DataSource = ParameterActionItem.GetItems();
            //comboBox.Top = top;
            //comboBox.Left = left;
            //comboBox.SelectedIndexChanged += ActionComboBoxSelectedIndexChanged;
            //ParametersPanel.Controls.Add(comboBox);
            //parameterIndex += 2;
        }


        public void CopyParameters()
        {
            if (FormulaSettings != null && FormulaSettings.IsValid)
                CopiedFormulaSettings = new FormulaSettings(FormulaSettings, parser: FormulaSettings.Parser);  //Create copy.
        }

        public void PasteParameters()
        {
            if (FormulaSettings != null)
            {
                if (FormulaSettings.PasteParameters(CopiedFormulaSettings))  //Shows messages if invalid.
                {
                    OnParameterChanged(this);  //Render pattern.
                    AddAllParametersControls(FormulaSettings);
                }
            }
        }

        public void Incrementer()
        {
            if (FormulaSettings != null)
                ActionSelected?.Invoke(this, new ParameterActionEventArgs(ParameterActions.Incrementer, this));
        }

        //private void ActionComboBoxSelectedIndexChanged(object sender, EventArgs e)
        //{
        //    MenuItem menuItem = sender as MenuItem;
        //    ParameterActions action = (ParameterActions)menuItem.Tag;
        //    //var comboBox = (ComboBox)sender;
        //    //ParameterActionItem parameterActionItem = comboBox.SelectedItem as ParameterActionItem;
        //    //if (parameterActionItem != null && parameterActionItem.ParameterAction != ParameterActions.None)
        //    //{
        //    //    handleEvents = false;
        //    //    comboBox.SelectedIndex = 0;
        //    //    handleEvents = true;
        //    //    ParameterActions action = parameterActionItem.ParameterAction;
        //        switch (action)
        //        {
        //            case ParameterActions.CopyParameters:
        //                if (formulaSettings != null && formulaSettings.IsValid)
        //                    CopiedFormulaSettings = new FormulaSettings(formulaSettings, parser: formulaSettings.Parser);  //Create copy.
        //                break;
        //            case ParameterActions.PasteParameters:
        //                if (formulaSettings != null)
        //                {
        //                    if (formulaSettings.PasteParameters(CopiedFormulaSettings))  //Shows messages if invalid.
        //                    {
        //                        OnParameterChanged(this);  //Render pattern.
        //                        AddAllParametersControls(formulaSettings);
        //                    }
        //                }
        //                break;
        //            case ParameterActions.Incrementer:
        //                ActionSelected?.Invoke(menuItem, new ParameterActionEventArgs(action, formulaSettings));
        //                break;
        //        }
        //    //}
        //}

        protected void OnParameterChanged(object sender, bool refreshDisplay = true)
        {
            if (handleEvents)
                ParameterChanged?.Invoke(sender, new ParameterChangedEventArgs(refreshDisplay));
        }

        public static string GetParameterLabel(BaseParameter baseParameter)
        {
            return baseParameter.GetLabel();
        }

        public static string GetCSharpParameterLabel(PropertyInfo propertyInfo, int index = -1)
        {
            var labelAttr = propertyInfo.GetCustomAttribute<ParameterLabelAttribute>();
            string labelText = labelAttr != null ? labelAttr.Label : propertyInfo.Name;
            if (index != -1)
            {
                labelText += $"[{index + 1}]";
            }
            return labelText;
        }

        protected void AddLabel(string labelText, int labelSpan, int top, ref int left, bool align = true)
        {
            var lbl = new Label();
            lbl.AutoSize = false;
            lbl.Text = labelText;
            int textWidth = TextRenderer.MeasureText(lbl.Text, lbl.Font).Width;
            int width;
            if (SingleColumn)
                width = textWidth;
            else
            {
                int maxWidth = labelSpan * critControlWidth - critLeftMargin;
                width = Math.Min(maxWidth, textWidth);
            }
            lbl.Width = width;
            lbl.Top = top;
            if (align && !SingleColumn)
            {
                left += labelSpan * critControlWidth + (labelSpan - 1) * critLeftMargin;
                lbl.Left = left - width - critLeftMargin;
            }
            else
            {
                lbl.Left = left;
                left += textWidth + critLeftMargin;
            }
            ParametersPanel.Controls.Add(lbl);
        }
    }
}
