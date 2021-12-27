using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whorl
{
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
                                     BaseParameterDisplay.ActionSelectedDelegate actionSelected = null,
                                     int controlWidth = BaseParameterDisplay.defaultControlWidth,
                                     bool singleColumn = false)
            {
                this.ParameterDisplay = new ParameterDisplay(pnlParameters, paramChanged,
                                        actionSelected, controlWidth);
                this.CSharpParameterDisplay = new CSharpParameterDisplay(pnlParameters, paramChanged,
                                        actionSelected, singleColumn, controlWidth);
                AddContextMenu();
                pnlParameters.MouseDown += new MouseEventHandler(PnlParameters_MouseDown);
            }

            public void AddParametersControls(Panel pnlParameters, FormulaSettings formulaSettings, Pattern sourcesPattern = null)
            {
                SetIsCSharp(formulaSettings.IsCSharpFormula);
                BaseParameterDisplay.AddAllParametersControls(formulaSettings, sourcesPattern);
                if (contextMenu != null)
                {
                    foreach (Control control in pnlParameters.Controls)
                    {
                        if (IsParameterLabel(control))
                            control.MouseDown += new MouseEventHandler(PnlParameters_MouseDown);
                    }
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
                        label = panel.Controls.Cast<Control>()
                                     .Select(ctl => ctl as Label)
                                     .Where(lbl => lbl != null)
                                     .OrderBy(lbl => Tools.DistanceSquared(e.Location, lbl.Location))
                                     .FirstOrDefault();
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

            private void AddContextMenu()
            {
                contextMenu = new ContextMenu();
                foreach (ParameterActions parameterAction in Enum.GetValues(typeof(ParameterActions)))
                {
                    var mnuItem = GetMenuItem(parameterAction);
                    //mnuItem.Tag = this.BaseParameterDisplay;
                    contextMenu.MenuItems.Add(mnuItem);
                }
            }

            private static BaseParameterDisplay GetBaseParameterDisplay(object sender)
            {
                MenuItem menuItem = (MenuItem)sender;
                return menuItem.Tag as BaseParameterDisplay;
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
                                          BaseParameterDisplay.ActionSelectedDelegate actionSelected = null,
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
            parameterDisplays.AddParametersControls(pnlParameters, formulaSettings, sourcesPattern);
        }

        public void RefreshComboBoxes(Panel pnlParameters)
        {
            var parameterDisplays = GetParameterDisplays(pnlParameters, throwException: true);
            if (parameterDisplays.BaseParameterDisplay != null)
                parameterDisplays.BaseParameterDisplay.RefreshComboBoxes();
        }
    }

}
