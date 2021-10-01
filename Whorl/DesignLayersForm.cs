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
    public partial class DesignLayersForm : Form
    {
        private WhorlDesign design;
        private List<Pattern> _selectedPatterns = new List<Pattern>();
        public List<Pattern> SelectedPatterns
        {
            get { return _selectedPatterns; }
            set
            {
                if (value == null)
                    throw new Exception("SelectedPatterns cannot be set to null.");
                _selectedPatterns = value;
                SetSelectedCheckBoxes();
            }
        }

        public DesignLayersForm()
        {
            InitializeComponent();
            dgvDesignLayers.AutoGenerateColumns = false;
            dgvDesignLayers.DataSource = designLayerBindingSource;
            colColorBlendType.ValueType = typeof(ColorBlendTypes);
            colColorBlendType.DataSource = Enum.GetValues(typeof(ColorBlendTypes));
        }

        public void SetDesign(WhorlDesign whorlDesign)
        {
            this.design = whorlDesign;
            BindDataGridView();
            SetSelectedCheckBoxes();
            SetDefaultCheckBoxes();
        }

        private void BindDataGridView()
        {
            this.designLayerBindingSource.Clear();
            foreach (DesignLayer designLayer in design.DesignLayerList.DesignLayers)
            {
                this.designLayerBindingSource.Add(designLayer);
            }
        }

        private void SetSelectedCheckBoxes()
        {
            HashSet<DesignLayer> patternLayers = new HashSet<DesignLayer>(
                                 SelectedPatterns.Select(ptn => ptn.DesignLayer));
            foreach (DataGridViewRow dgvRow in dgvDesignLayers.Rows)
            {
                DesignLayer designLayer = dgvRow.DataBoundItem as DesignLayer;
                bool selected = designLayer != null && patternLayers.Contains(designLayer);
                dgvRow.Cells[colIsSelected.Index].Value = selected;
            }
        }

        private void SetDefaultCheckBoxes()
        {
            DesignLayer defaultLayer = design?.DefaultDesignLayer;
            foreach (DataGridViewRow dgvRow in dgvDesignLayers.Rows)
            {
                DesignLayer designLayer = dgvRow.DataBoundItem as DesignLayer;
                bool selected = designLayer != null && designLayer == defaultLayer;
                dgvRow.Cells[colIsDefaultLayer.Index].Value = selected;
            }
        }

        private void dgvDesignLayers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex != colDeleteLayer.Index || e.RowIndex < 0)
                    return;
                var dgvRow = dgvDesignLayers.Rows[e.RowIndex];
                var designLayer = dgvRow.DataBoundItem as DesignLayer;
                if (designLayer == null)
                    return;
                if (design != null)
                {
                    design.DesignLayerList.RemoveDesignLayer(designLayer);
                    this.designLayerBindingSource.Remove(designLayer);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void dgvDesignLayers_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && 
                        dgvDesignLayers.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
                    dgvDesignLayers.EndEdit();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private bool ignoreCellValueChanged = false;

        private void dgvDesignLayers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (ignoreCellValueChanged)
                return;
            try
            {
                ignoreCellValueChanged = true;
                if (e.RowIndex < 0)
                    return;
                var cell = dgvDesignLayers.CurrentCell;
                var dgvRow = dgvDesignLayers.Rows[e.RowIndex];
                var designLayer = dgvRow.DataBoundItem as DesignLayer;
                if (e.ColumnIndex == colZOrder.Index && cell.Value is int && 
                    designLayer != null && design != null)
                {
                    int zOrder = (int)cell.Value;
                    design.DesignLayerList.ReorderLayer(designLayer, zOrder);
                    designLayerBindingSource.ResetBindings(false);
                }
                else if (cell.Value is bool && designLayer != null)
                {
                    bool cellValue = (bool)cell.Value;
                    if (e.ColumnIndex == colIsSelected.Index)
                    {
                        if (design != null)
                            design.Initializing = true;
                        try
                        {
                            foreach (Pattern pattern in SelectedPatterns)
                            {
                                pattern.DesignLayer = cellValue ? designLayer : null;
                            }
                            SetSelectedCheckBoxes();
                        }
                        finally
                        {
                            if (design != null)
                            {
                                design.Initializing = false;
                                design.DesignLayerList.RaiseLayerChangedEvent(
                                        designLayer, whorlDesignChanged: true);
                            }
                        }
                    }
                    else if (e.ColumnIndex == colIsDefaultLayer.Index)
                    {
                        if (design != null)
                            design.DefaultDesignLayer = designLayer;
                        SetDefaultCheckBoxes();
                    }
                    else if (e.ColumnIndex == colLayerVisible.Index)
                        designLayer.Visible = cellValue;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
            finally
            {
                ignoreCellValueChanged = false;
            }
        }

        private void btnAddLayer_Click(object sender, EventArgs e)
        {
            try
            {
                if (design != null)
                {
                    DesignLayer designLayer = new DesignLayer(design.DesignLayerList);
                    design.DesignLayerList.AddDesignLayer(designLayer);
                    this.designLayerBindingSource.Add(designLayer);
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void DesignLayersForm_Activated(object sender, EventArgs e)
        {
            try
            {
                SetSelectedCheckBoxes();
                SetDefaultCheckBoxes();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
