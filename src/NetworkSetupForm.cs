using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Bodoni
{
    public sealed class NetworkSetupForm : Form
    {
        readonly ComboBox role = new ComboBox();
        readonly TextBox host = new TextBox(), key = new TextBox(), serverId = new TextBox();
        readonly NumericUpDown port = new NumericUpDown(), last = new NumericUpDown();
        readonly Label instructions = new Label();
        NetworkConfig principal, client;
        public NetworkConfig Result { get; private set; }
        public NetworkSetupForm(NetworkConfig current)
        {
            Text = "Red y consecutivo compartido"; Size = new Size(650, 545); MinimumSize = Size; MaximumSize = Size;
            StartPosition = FormStartPosition.CenterParent; Font = new Font("Segoe UI", 9); MaximizeBox = false; MinimizeBox = false;
            principal = current != null && current.Role == "Principal" ? NetworkJson.Copy(current) : NetworkConfig.Principal(4121, 47831);
            client = current != null && current.Role == "Cliente" ? NetworkJson.Copy(current) : new NetworkConfig();
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 9, Padding = new Padding(16) };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170)); panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            int[] heights = { 36, 85, 38, 38, 38, 38, 38, 40, 45 };
            foreach (int height in heights) panel.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            Controls.Add(panel);
            role.DropDownStyle = ComboBoxStyle.DropDownList; role.Items.Add("Otro computador es el principal"); role.Items.Add("Este computador es el principal");
            Add(panel, "Función de este equipo", role, 0);
            instructions.Dock = DockStyle.Fill; instructions.ForeColor = Color.FromArgb(48, 70, 88); instructions.Padding = new Padding(0, 7, 0, 0);
            panel.Controls.Add(instructions, 0, 1); panel.SetColumnSpan(instructions, 2);
            Add(panel, "Nombre/IP del principal", host, 2);
            port.Minimum = 1024; port.Maximum = 65535; Add(panel, "Puerto", port, 3);
            key.ReadOnly = true; key.UseSystemPasswordChar = true; Add(panel, "Clave de conexión", key, 4);
            serverId.ReadOnly = true; Add(panel, "Identificación del principal", serverId, 5);
            last.Minimum = 0; last.Maximum = 999998; last.ThousandsSeparator = true; Add(panel, "Último número utilizado", last, 6);
            FlowLayoutPanel transfer = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            Button import = new Button { Text = "Importar conexión del principal", AutoSize = true, Height = 30 };
            import.Click += Import; transfer.Controls.Add(import);
            panel.Controls.Add(transfer, 0, 7); panel.SetColumnSpan(transfer, 2);
            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            Button save = new Button { Text = "Guardar y conectar", AutoSize = true, Height = 32 };
            save.Click += delegate
            {
                try
                {
                    bool isPrincipal = role.SelectedIndex == 1;
                    NetworkConfig config = NetworkJson.Copy(isPrincipal ? principal : client);
                    config.Role = isPrincipal ? "Principal" : "Cliente";
                    config.Host = host.Text.Trim(); config.Port = (int)port.Value; config.InitialLast = (int)last.Value;
                    config.Validate(); Result = config; DialogResult = DialogResult.OK; Close();
                }
                catch (Exception ex) { MessageBox.Show(this, ex.Message, "Revisa la conexión", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            };
            actions.Controls.Add(save); actions.Controls.Add(new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true, Height = 32 });
            panel.Controls.Add(actions, 0, 8); panel.SetColumnSpan(actions, 2);
            role.SelectedIndexChanged += delegate { ApplyRole(); };
            role.SelectedIndex = current != null && current.Role == "Principal" ? 1 : 0;
        }
        static void Add(TableLayoutPanel panel, string text, Control control, int row)
        {
            panel.Controls.Add(new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            control.Dock = DockStyle.Fill; control.Margin = new Padding(3, 6, 3, 4); panel.Controls.Add(control, 1, row);
        }
        void ApplyRole()
        {
            bool isPrincipal = role.SelectedIndex == 1;
            NetworkConfig config = isPrincipal ? principal : client;
            host.Text = isPrincipal ? Environment.MachineName : config.Host;
            port.Value = config.Port; key.Text = config.Key ?? ""; serverId.Text = config.ServerId ?? ""; last.Value = config.InitialLast;
            last.Enabled = isPrincipal && !config.Initialized && !File.Exists(NetworkConfig.LedgerPath) && !File.Exists(NetworkConfig.LedgerPath + ".inicializado");
            instructions.Text = isPrincipal
                ? "Este equipo guardará las notas y asignará los números a todos. Debe permanecer encendido y con Bodoni abierto. Antes de la primera instalación ingresa el último número real utilizado. 004121 es solo la referencia de pruebas. Un registro existente conserva su contador."
                : "Importa el archivo de conexión exportado en el principal. Si es necesario, cambia su nombre por su dirección IP. Este equipo no asignará números sin conexión al principal.";
        }
        void Import(object sender, EventArgs args)
        {
            using (OpenFileDialog dialog = new OpenFileDialog { Title = "Conexión exportada por el principal", Filter = "Conexión Bodoni|*.bodoni-red.json" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    if (new FileInfo(dialog.FileName).Length > 16000) throw new ArgumentException("El archivo de conexión no es válido.");
                    NetworkConfig imported = NetworkJson.Read<NetworkConfig>(dialog.FileName);
                    if (imported == null || imported.Role != "Cliente") throw new ArgumentException("Este archivo no es una conexión para equipos secundarios.");
                    imported.Validate(); client = imported;
                    role.SelectedIndex = 0; ApplyRole();
                }
                catch (Exception ex) { MessageBox.Show(this, ex.Message, "No se pudo importar", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
        }
    }

    public sealed class NoteHistoryForm : Form
    {
        readonly DataGridView grid = new DataGridView();
        public string SelectedRequest { get; private set; }
        public NoteHistoryForm(List<NoteSummary> notes)
        {
            Text = "Notas emitidas en el principal · Últimas 200"; Width = 900; Height = 500; StartPosition = FormStartPosition.CenterParent; Font = new Font("Segoe UI", 9);
            grid.Dock = DockStyle.Fill; grid.ReadOnly = true; grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false; grid.RowHeadersVisible = false;
            grid.AutoGenerateColumns = false; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = false;
            string[] properties = { "Number", "Customer", "Date", "Total", "Computer" };
            string[] headers = { "Número", "Cliente", "Fecha", "Total US$", "Equipo" };
            for (int i = 0; i < properties.Length; i++) grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = properties[i], HeaderText = headers[i], FillWeight = i == 1 ? 180 : 100 });
            grid.DataSource = notes; Controls.Add(grid);
            Button open = new Button { Text = "Abrir para consultar o reimprimir", Dock = DockStyle.Bottom, Height = 42 };
            open.Click += delegate { SelectNote(); }; grid.CellDoubleClick += delegate { SelectNote(); }; Controls.Add(open);
        }
        void SelectNote()
        {
            if (grid.CurrentRow == null) return;
            NoteSummary note = grid.CurrentRow.DataBoundItem as NoteSummary;
            if (note == null) return;
            SelectedRequest = note.RequestId; DialogResult = DialogResult.OK; Close();
        }
    }
}
