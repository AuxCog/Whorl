using ParserEngine;
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
    public partial class NamedRandomSeedForm : Form
    {
        public NamedRandomSeedForm()
        {
            InitializeComponent();
        }

        private RandomGenerator seededRandom;
        private WhorlDesign design;

        public void Initialize(RandomGenerator seededRandom, WhorlDesign design)
        {
            this.seededRandom = seededRandom;
            this.design = design;
            PopulateNamedSeedComboBox();
        }

        private void PopulateNamedSeedComboBox()
        {
            var namedSeed = cboNamedSeed.SelectedItem as NamedRandomSeed;
            cboNamedSeed.DataSource = design.AnimationSeeds.Values.ToList();
            if (namedSeed != null)
            {
                //Make sure namedSeed belongs to current design:
                namedSeed = design.GetNamedRandomSeed(namedSeed.Name);
                if (cboNamedSeed.Items.Contains(namedSeed))
                    cboNamedSeed.SelectedItem = namedSeed;
            }
        }

        private void cboNamedSeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var namedSeed = cboNamedSeed.SelectedItem as NamedRandomSeed;
                if (namedSeed != null)
                    this.txtSeedName.Text = namedSeed.Name;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnSetSeed_Click(object sender, EventArgs e)
        {
            try
            {
                var namedSeed = cboNamedSeed.SelectedItem as NamedRandomSeed;
                if (namedSeed != null)
                    seededRandom.ReseedRandom(namedSeed.Seed);
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnAddSeed_Click(object sender, EventArgs e)
        {
            try
            {
                NamedRandomSeed namedSeed = design.AddNamedRandomSeed(seededRandom);
                PopulateNamedSeedComboBox();
                cboNamedSeed.SelectedItem = namedSeed;
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnRenameSeed_Click(object sender, EventArgs e)
        {
            try
            {
                string name = txtSeedName.Text;
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Please enter a nonblank Seed Name.");
                    return;
                }
                var namedSeed = cboNamedSeed.SelectedItem as NamedRandomSeed;
                if (namedSeed != null && namedSeed.Name != name)
                {
                    if (design.AnimationSeeds.ContainsKey(name))
                    {
                        MessageBox.Show($"The name {name} is a duplicate.");
                        return;
                    }
                    design.AnimationSeeds.Remove(namedSeed.Name);
                    namedSeed.Name = name;
                    design.AnimationSeeds.Add(namedSeed.Name, namedSeed);
                    PopulateNamedSeedComboBox();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                this.Hide();
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnSetSeedNew_Click(object sender, EventArgs e)
        {
            try
            {
                var namedSeed = cboNamedSeed.SelectedItem as NamedRandomSeed;
                if (namedSeed != null)
                {
                    seededRandom.ReseedRandom();
                    namedSeed.Seed = seededRandom.RandomSeed;
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }

        private void btnDeleteSeed_Click(object sender, EventArgs e)
        {
            try
            {
                var namedSeed = cboNamedSeed.SelectedItem as NamedRandomSeed;
                if (namedSeed != null)
                {
                    if (MessageBox.Show($"Delete seed {namedSeed.Name}?", "Confirm",
                                        MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        return;
                    }
                    design.AnimationSeeds.Remove(namedSeed.Name);
                    PopulateNamedSeedComboBox();
                }
            }
            catch (Exception ex)
            {
                Tools.HandleException(ex);
            }
        }
    }
}
