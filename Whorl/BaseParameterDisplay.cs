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

    public abstract class BaseParameterDisplay
    {
        public static CopyPasteInfo CopiedFormulaSettingsInfo { get; } = new CopyPasteInfo();

        //protected class ParameterActionItem
        //{
        //    public ParameterActions ParameterAction { get; }
        //    public string Text { get; }

        //    public ParameterActionItem(ParameterActions parameterAction, string text)
        //    {
        //        ParameterAction = parameterAction;
        //        Text = text;
        //    }

        //    private static string GetText(ParameterActions parameterAction)
        //    {
        //        switch (parameterAction)
        //        {
        //            //case ParameterActions.None:
        //            //    return string.Empty;
        //            case ParameterActions.CopyParameters:
        //                return "Copy Parameters";
        //            case ParameterActions.PasteParameters:
        //                return "Paste Parameters";
        //            default:
        //                return parameterAction.ToString();
        //        }
        //    }

        //    public static List<ParameterActionItem> GetItems()
        //    {
        //        return (from ParameterActions a in Enum.GetValues(typeof(ParameterActions))
        //                select new ParameterActionItem(a, GetText(a))).ToList();
        //    }

        //    public override string ToString()
        //    {
        //        return Text;
        //    }
        //}
        public const int defaultControlWidth = 85;
        protected const int 
                  critTopMargin = 2,
                  critLeftMargin = 3,
                  critRowHeight = 25;
        protected int critControlWidth { get; }
        protected int critColumnWidth { get; }
        protected int critColumnCount { get; private set; }
        protected bool handleEvents { get; set; }
        public FormulaSettings FormulaSettings { get; private set; }
        public Panel ParametersPanel { get; }

        public delegate bool delEditTransformInfluenceLink(string parameterKey);
        public delEditTransformInfluenceLink FnEditInfluenceLink { get; set; }


        public delegate void ParamChanged(object sender, ParameterChangedEventArgs e);
        public event ParamChanged ParameterChanged;

        public ParamChanged ParamChangedFn { get; }

        public delegate void ActionSelectedDelegate(object sender, ParameterActionEventArgs e);
        public event ActionSelectedDelegate ActionSelected;
        public bool SingleColumn { get; }
        public bool UpdateParametersObject { get; set; } = true;
        public Label SelectedLabel { get; set; }

        public static void ClearParametersControls(Panel pnlParameters)
        {
            foreach (Control ctl in pnlParameters.Controls)
            {
                ctl.Dispose();
            }
            pnlParameters.Controls.Clear();
        }

        public BaseParameterDisplay(Panel pnlParameters, ParamChanged paramChangedFn = null, 
                                    ActionSelectedDelegate actionSelectedFn = null, 
                                    int controlWidth = defaultControlWidth,
                                    bool singleColumn = false)
        {
            SingleColumn = singleColumn;
            critControlWidth = controlWidth;
            critColumnWidth = controlWidth + critLeftMargin;
            ParametersPanel = pnlParameters;
            ParamChangedFn = paramChangedFn;
            if (paramChangedFn != null)
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

        protected LinkLabel CreateInfluenceLinkLabel(string parameterKey, ref int txtWidth)
        {
            var lnkInfluence = new LinkLabel()
            {
                AutoSize = false,
                Width = 15,
                Tag = parameterKey
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
            string parameterKey = (string)linkLabel.Tag;
            bool haveInfluenceLinks = false;
            var parentCollection = FormulaSettings?.InfluenceLinkParentCollection;
            if (parentCollection != null)
            {
                var linkParent = parentCollection.GetLinkParent(parameterKey);
                if (linkParent != null)
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
                string parameterKey = (string)lnkInfluence.Tag;
                if (FnEditInfluenceLink(parameterKey))
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
            {
                var copiedFormulaSettings = new FormulaSettings(FormulaSettings, parser: FormulaSettings.Parser);  //Create copy.
                Tools.SetCopyForPaste(CopiedFormulaSettingsInfo, copiedFormulaSettings, null);
            }
        }

        public void PasteParameters()
        {
            if (FormulaSettings != null)
            {
                try
                {
                    handleEvents = false;
                    FormulaSettings copiedFormulaSettings = Tools.GetCopyForPaste<FormulaSettings>(CopiedFormulaSettingsInfo, out bool cancelled);
                    if (cancelled) return;
                    if (FormulaSettings.PasteParameters(copiedFormulaSettings))  //Shows messages if invalid.
                    {
                        OnParameterChanged(this, alwaysHandle: true);  //Render pattern.
                        AddAllParametersControls(FormulaSettings);
                    }
                }
                finally
                {
                    handleEvents = true;
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

        protected void OnParameterChanged(object sender, bool refreshDisplay = true, bool alwaysHandle = false)
        {
            if (handleEvents || alwaysHandle)
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
