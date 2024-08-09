using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace MoveWindow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ListOpenWindows();

            listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
            RestoreWindowsPositionAndSize();
        }
        
        private bool IsSystemWindow(string title)
        {
            // Lista de t�tulos de janelas conhecidas do sistema
            var excludedTitles = new HashSet<string>
            {
                "Experiência de Entrada do Windows", "Configurações", "NVIDIA GeForce Overlay",
                "ShellExperienceHost", "SearchUI", "StartMenuExperienceHost", "Electron" //, "Form1"
            };

            return excludedTitles.Contains(title);
        }

        private void ListOpenWindows()
        {
            // Lista para armazenar t�tulos das janelas
            List<string> windowTitles = new List<string>();

            // Delegate para processar cada janela
            WindowHelper.EnumWindowsProc enumProc = (hWnd, lParam) =>
            {
                if (WindowHelper.IsWindowVisible(hWnd) && WindowHelper.IsTopLevelWindow(hWnd))
                {
                    uint processId;
                    WindowHelper.GetWindowThreadProcessId(hWnd, out processId);
                    var process = Process.GetProcessById((int)processId);

                    // Verifica se a janela tem um t�tulo e est� associada a um processo do usu�rio
                    StringBuilder windowText = new StringBuilder(256);
                    WindowHelper.GetWindowText(hWnd, windowText, windowText.Capacity);
                    string title = windowText.ToString();

                    if (!string.IsNullOrEmpty(title) && !IsSystemWindow(title))
                    {
                        windowTitles.Add(title);
                    }
                }
                return true; // Continua enumerando
            };

            // Enumera todas as janelas
            WindowHelper.EnumWindows(enumProc, IntPtr.Zero);

            // Adiciona os t�tulos ao ListBox
            listBox1.Items.AddRange(windowTitles.ToArray());
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                string selectedWindowTitle = listBox1.SelectedItem.ToString();
                IntPtr hWnd = WindowHelper.FindWindow(null, selectedWindowTitle);

                if (hWnd != IntPtr.Zero)
                {
                    WindowHelper.RECT rect;
                    if (WindowHelper.GetWindowRect(hWnd, out rect))
                    {
                        int x = rect.Left;
                        int y = rect.Top;
                        int width = rect.Right - rect.Left;
                        int height = rect.Bottom - rect.Top;

                        numericUpDown1.Value = x;
                        numericUpDown2.Value = y;
                        numericUpDown3.Value = width;
                        numericUpDown4.Value = height;
                    }
                }
            }
        }

        private void numericDown(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedWindowTitle = listBox1.SelectedItem?.ToString();
                if (selectedWindowTitle != null)
                {
                    IntPtr hWnd = WindowHelper.FindWindow(null, selectedWindowTitle);

                    if (hWnd != IntPtr.Zero)
                    {
                        int x = (int)numericUpDown1.Value;
                        int y = (int)numericUpDown2.Value;

                        WindowHelper.SetWindowPos(hWnd, WindowHelper.HWND_TOP, x, y, 0, 0,
                            WindowHelper.SWP_NOSIZE | WindowHelper.SWP_NOZORDER | WindowHelper.SWP_NOACTIVATE);
                    }
                }
            }
            else
            {

                MessageBoxEx.Show(this, "Selecione uma janela na lista primeiro.");

            }
        }

        private void numericDown2(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedWindowTitle = listBox1.SelectedItem.ToString();
                IntPtr hWnd = WindowHelper.FindWindow(null, selectedWindowTitle);

                if (hWnd != IntPtr.Zero)
                {
                    WindowHelper.RECT rect;
                    if (WindowHelper.GetWindowRect(hWnd, out rect))
                    {
                        int x = rect.Left;
                        int y = rect.Top;
                        int width = (int)numericUpDown3.Value;
                        int height = (int)numericUpDown4.Value;

                        WindowHelper.SetWindowPos(hWnd, WindowHelper.HWND_TOP, x, y, width, height,
                            WindowHelper.SWP_NOZORDER | WindowHelper.SWP_NOACTIVATE);
                    }
                }
            }
            else
            {
                MessageBoxEx.Show(this, "Selecione uma janela na lista primeiro.");
            }
        }

        private void SaveWindowPositionAndSize()
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedWindowTitle = listBox1.SelectedItem.ToString();
                IntPtr hWnd = WindowHelper.FindWindow(null, selectedWindowTitle);

                if (hWnd != IntPtr.Zero)
                {
                    WindowHelper.RECT rect;
                    if (WindowHelper.GetWindowRect(hWnd, out rect))
                    {
                        int x = rect.Left;
                        int y = rect.Top;
                        int width = rect.Right - rect.Left;
                        int height = rect.Bottom - rect.Top;
                        WindowHelper.SaveWindowPositionAndSize(selectedWindowTitle, x, y, width, height);
                        MessageBoxEx.Show(this, "Posição e tamanho da janela foram salvo!");
                    }
                }
            }
            else
            {
                MessageBoxEx.Show(this, "Selecione uma janela na lista primeiro.");
            }
        }

        private void RestoreWindowsPositionAndSize()
        {
            var savedWindows = WindowHelper.LoadAllRegistryValues();
            foreach (var windowTitle in savedWindows.Keys)
            {
                var windowData = savedWindows[windowTitle];
                if (windowData.ContainsKey("X") && windowData.ContainsKey("Y") && windowData.ContainsKey("Width") && windowData.ContainsKey("Height"))
                {
                    IntPtr hWnd = WindowHelper.FindWindow(null, windowTitle);
                    if (hWnd != IntPtr.Zero)
                    {
                        int x = int.Parse(windowData["X"]);
                        int y = int.Parse(windowData["Y"]);
                        int width = int.Parse(windowData["Width"]);
                        int height = int.Parse(windowData["Height"]);

                        WindowHelper.SetWindowPos(hWnd, WindowHelper.HWND_TOP, x, y, width, height,
                            WindowHelper.SWP_NOZORDER | WindowHelper.SWP_NOACTIVATE);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveWindowPositionAndSize();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            ListOpenWindows();
        }
    }
}
