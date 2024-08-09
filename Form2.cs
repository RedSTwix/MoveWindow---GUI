using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MoveWindow
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            LoadRegistryValuesGrid();

            this.Load += new EventHandler(Form2_Load);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // Carrega os valores do registro
            //LoadRegistryValues();

            // Limpa a seleção do DataGridView após carregar os dados
            dataGridView1.ClearSelection();
        }

        private void LoadRegistryValuesGrid()
        {
            // Limpa todas as linhas do DataGridView
            dataGridView1.Rows.Clear();

            // Obtém os valores do registro
            var registryValues = WindowHelper.LoadAllRegistryValues();

            if (registryValues.Count == 0)
            {
                // Exibe uma mensagem se nenhuma chave for encontrada no registro (opcional)
                // MessageBox.Show("Nenhuma chave encontrada no registro.");
                return;
            }

            foreach (var subKey in registryValues)
            {
                var subKeyTitle = subKey.Key; // Título da subchave

                // Verifica se há pelo menos quatro pares de key e value para exibir
                if (subKey.Value.Count < 4)
                    continue;

                // Obtém os pares de key e value
                var firstPair = new KeyValuePair<string, string>(subKey.Value.ElementAt(0).Key, subKey.Value.ElementAt(0).Value);
                var secondPair = new KeyValuePair<string, string>(subKey.Value.ElementAt(1).Key, subKey.Value.ElementAt(1).Value);
                var thirdPair = new KeyValuePair<string, string>(subKey.Value.ElementAt(2).Key, subKey.Value.ElementAt(2).Value);
                var fourPair = new KeyValuePair<string, string>(subKey.Value.ElementAt(3).Key, subKey.Value.ElementAt(3).Value);

                // Adiciona uma nova linha ao DataGridView
                dataGridView1.Rows.Add(subKeyTitle, firstPair.Value, secondPair.Value, thirdPair.Value, fourPair.Value);
            }
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Obtém o título da subchave da linha selecionada
                var subKeyTitle = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();

                // Confirmar com o usuário antes de excluir
                var confirmResult = MessageBoxEx.Show(this, "Deseja excluir '" + subKeyTitle + "'?",
                                            "Confirmação de exclusão",
                                            MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    // Excluir a subchave
                    try
                    {
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MoveWindow", true))
                        {
                            if (key != null)
                            {
                                key.DeleteSubKey(subKeyTitle);
                                //MessageBox.Show(this, "Subchave excluída com sucesso.");
                                LoadRegistryValuesGrid(); // Recarregar as chaves após a exclusão
                                dataGridView1.ClearSelection(); // Limpar a seleção após recarregar
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBoxEx.Show(this, "Erro ao excluir a subchave: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBoxEx.Show(this, "Selecione uma subchave para excluir.");
            }
        }
    }
}
