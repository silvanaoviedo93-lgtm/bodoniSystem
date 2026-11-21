using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Bodoni
{
    public class PreviewPanel : Control
    {
        public List<TextItem> Items = new List<TextItem>();
        public Image Template;
        public bool Guides = true;
        public PreviewPanel() { DoubleBuffered = true; BackColor = Color.FromArgb(228, 234, 239); Dock = DockStyle.Fill; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            float width = Math.Min(ClientSize.Width - 32, (ClientSize.Height - 52) * 200f / 145f);
            if (width <= 0) return;
            float height = width * 145f / 200;
            RectangleF page = new RectangleF((ClientSize.Width - width) / 2, (ClientSize.Height - height) / 2 + 9, width, height);
            using (Brush shadow = new SolidBrush(Color.FromArgb(35, 50, 70))) e.Graphics.FillRectangle(shadow, page.X + 3, page.Y + 3, page.Width, page.Height);
            Bodoni.Layout.Draw(e.Graphics, page, Items, Template, Guides);
            e.Graphics.DrawRectangle(Pens.Silver, page.X, page.Y, page.Width, page.Height);
            string label = Template == null ? "Sin fondo · Carga la imagen original de tu plantilla" : "Plantilla original · Exporta el PDF completo o solo los datos";
            TextRenderer.DrawText(e.Graphics, label, Font, new Rectangle(8, 4, Width - 16, 28), Color.FromArgb(65, 80, 96), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && Template != null) Template.Dispose();
            base.Dispose(disposing);
        }
    }

    public sealed partial class MainForm : Form
    {
        readonly TextBox customer = new TextBox(), identification = new TextBox(), address = new TextBox(), phone = new TextBox(), email = new TextBox();
        readonly DateTimePicker date = new DateTimePicker();
        readonly DataGridView grid = new DataGridView();
        readonly PreviewPanel preview = new PreviewPanel();
        readonly Label total = new Label(), validation = new Label(), footer = new Label();
        readonly ComboBox profiles = new ComboBox();
        readonly NumericUpDown offsetX = new NumericUpDown(), offsetY = new NumericUpDown(), fontScale = new NumericUpDown();
        readonly CheckBox printSeries = new CheckBox(), guides = new CheckBox();
        Settings settings = new Settings();
        bool loading, dirty, ready, refreshing;
        string draftPath;
        readonly Timer sessionTimer = new Timer { Interval = 600 };

        static readonly Color Blue = Color.FromArgb(0, 145, 198), Ink = Color.FromArgb(28, 48, 65);
        static Label TextLabel(string text)
        {
            return new Label { Text = text, AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Ink };
        }
        static Button Button(string text, EventHandler click)
        {
            Button button = new Button { Text = text, AutoSize = true, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Ink, Margin = new Padding(0, 4, 8, 4), Padding = new Padding(7, 2, 7, 2) };
            button.FlatAppearance.BorderColor = Color.FromArgb(193, 208, 219); button.Click += click; return button;
        }
        static FlowLayoutPanel Flow()
        {
            return new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true, BackColor = Color.White, Padding = new Padding(5, 0, 0, 0) };
        }

        public MainForm()
        {
            Text = Program.PracticeMode ? "Bodoni · PRÁCTICA · Datos separados de producción" : "Bodoni · Notas de venta en red";
            Font = new Font("Segoe UI", 9);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1120, 740); Size = new Size(1380, 900);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(242, 246, 249);
            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(16) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            Controls.Add(root);
            Panel header = new Panel { Dock = DockStyle.Fill, BackColor = Ink };
            header.Controls.Add(new Label { Text = Program.PracticeMode ? "BODONI   /   PRÁCTICA" : "BODONI   /   Nota de venta", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.White, Padding = new Padding(14, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft });
            root.Controls.Add(header, 0, 0);
            networkNotice = TextLabel("BORRADOR · Los PDF de prueba no consumen numeración. Usa Emitir para asignar el número desde el principal.");
            networkNotice.BackColor = Color.FromArgb(255, 242, 210); networkNotice.Padding = new Padding(10, 0, 0, 0); root.Controls.Add(networkNotice, 0, 1);
            root.Controls.Add(BuildNetworkBar(), 0, 2);
            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterWidth = 12, FixedPanel = FixedPanel.None };
            root.Controls.Add(split, 0, 3);
            split.Panel1.Controls.Add(BuildEditor()); split.Panel2.Controls.Add(BuildPreview());
            footer.Dock = DockStyle.Fill; footer.ForeColor = Color.FromArgb(84, 100, 116); footer.TextAlign = ContentAlignment.MiddleLeft;
            root.Controls.Add(footer, 0, 4);
            LoadSettings(); LoadTemplate(); ApplyProfile();
            RestoreSession();
            RestoreWork();
            ready = true; RefreshPreview(); UpdateWorkControls();
            sessionTimer.Tick += delegate { sessionTimer.Stop(); if (grid.IsCurrentCellInEditMode) sessionTimer.Start(); else SaveSession(); };
            Shown += delegate
            {
                split.SplitterDistance = Math.Max(540, split.Width / 2);
                LoadNetwork();
                footer.Text = "PDF: 200 × 145 mm · Emite una sola vez; regenerar o reimprimir conserva el número.";
            };
            FormClosing += OnClosing;
        }

        Control BuildEditor()
        {
            TableLayoutPanel left = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.White, ColumnCount = 1, RowCount = 7, Padding = new Padding(10) };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 172));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 57));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            FlowLayoutPanel actions = Flow();
            actions.Controls.Add(Button("Nueva", delegate { BeginDraft(new Draft()); }));
            actions.Controls.Add(Button("Ejemplo $50", delegate { BeginDraft(Draft.Sample()); }));
            actions.Controls.Add(Button("Abrir borrador", OpenDraft));
            actions.Controls.Add(Button("Guardar borrador", SaveDraft));
            left.Controls.Add(actions, 0, 0);
            Label title = TextLabel("01  Datos del cliente"); title.Font = new Font(Font, FontStyle.Bold); left.Controls.Add(title, 0, 1);
            TableLayoutPanel fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 5 };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 5; i++) fields.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            AddField(fields, "Cliente *", customer, 0, 0); fields.SetColumnSpan(customer, 3);
            AddField(fields, "C.I. / RUC", identification, 0, 1);
            date.Format = DateTimePickerFormat.Custom; date.CustomFormat = "dd/MM/yyyy"; date.Dock = DockStyle.Fill;
            AddField(fields, "Fecha", date, 2, 1); date.ValueChanged += Changed;
            AddField(fields, "Dirección", address, 0, 2); fields.SetColumnSpan(address, 3);
            AddField(fields, "Teléfono", phone, 0, 3); fields.SetColumnSpan(phone, 3);
            AddField(fields, "Correo", email, 0, 4); fields.SetColumnSpan(email, 3);
            left.Controls.Add(fields, 0, 2);
            FlowLayoutPanel detail = Flow();
            detail.Controls.Add(new Label { Text = "02  Detalle manual", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(0, 8, 12, 0) });
            detail.Controls.Add(Button("+ Fila", delegate { if (!CanEditWork()) return; if (grid.Rows.Count < 10) { grid.Rows.Add(); Changed(null, EventArgs.Empty); } else ShowInfo("La plantilla tiene diez filas disponibles."); }));
            detail.Controls.Add(Button("− Fila", delegate { if (!CanEditWork()) return; if (grid.CurrentRow != null) { grid.Rows.Remove(grid.CurrentRow); Changed(null, EventArgs.Empty); } }));
            left.Controls.Add(detail, 0, 3);
            grid.Dock = DockStyle.Fill; grid.BackgroundColor = Color.White; grid.BorderStyle = BorderStyle.FixedSingle;
            grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false; grid.AllowUserToResizeRows = false;
            grid.RowHeadersVisible = false; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.CellSelect; grid.MultiSelect = false;
            grid.EnableHeadersVisualStyles = false; grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Blue; grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold); grid.RowTemplate.Height = 30;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(243, 248, 251);
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Cant.", FillWeight = 16 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Descripción", FillWeight = 49 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "P. unitario", FillWeight = 18 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Importe", FillWeight = 18, ReadOnly = true });
            foreach (DataGridViewColumn column in grid.Columns) column.SortMode = DataGridViewColumnSortMode.NotSortable;
            grid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grid.CellEndEdit += Changed;
            grid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e) { e.ThrowException = false; validation.Text = "Revisa el valor ingresado en el detalle."; };
            left.Controls.Add(grid, 0, 4);
            total.Dock = DockStyle.Fill; total.TextAlign = ContentAlignment.MiddleRight; total.Font = new Font("Segoe UI", 22, FontStyle.Bold); total.ForeColor = Ink;
            left.Controls.Add(total, 0, 5);
            validation.Dock = DockStyle.Fill; validation.ForeColor = Color.FromArgb(150, 82, 12); validation.Padding = new Padding(3, 3, 0, 0);
            left.Controls.Add(validation, 0, 6);
            foreach (TextBox input in new TextBox[] { customer, identification, address, phone, email }) { input.TextChanged += Changed; input.MaxLength = 300; }
            return left;
        }
        static void AddField(TableLayoutPanel panel, string label, Control input, int col, int row)
        {
            panel.Controls.Add(TextLabel(label), col, row); input.Dock = DockStyle.Fill; input.Margin = new Padding(3, 5, 5, 3); panel.Controls.Add(input, col + 1, row);
        }
        Control BuildPreview()
        {
            TableLayoutPanel right = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.White, ColumnCount = 1, RowCount = 6, Padding = new Padding(10) };
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); right.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); right.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 92)); right.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
            Label title = TextLabel("03  Vista previa sobre tu plantilla · 20 × 14,5 cm"); title.Font = new Font(Font, FontStyle.Bold); right.Controls.Add(title, 0, 0);
            FlowLayoutPanel templateActions = Flow();
            templateActions.Controls.Add(Button("Cambiar plantilla", ImportTemplate));
            guides.Text = "Guías de campos"; guides.AutoSize = true; guides.Checked = false; preview.Guides = false; guides.Padding = new Padding(0, 8, 0, 0);
            guides.CheckedChanged += delegate { preview.Guides = guides.Checked; preview.Invalidate(); }; templateActions.Controls.Add(guides);
            right.Controls.Add(templateActions, 0, 1); right.Controls.Add(preview, 0, 2);
            FlowLayoutPanel profileRow = Flow();
            profileRow.Controls.Add(new Label { Text = "Impresora", AutoSize = true, Padding = new Padding(0, 10, 5, 0) });
            profiles.DropDownStyle = ComboBoxStyle.DropDownList; profiles.DisplayMember = "Name"; profiles.Width = 190; profiles.Margin = new Padding(2, 7, 8, 2);
            profiles.SelectedIndexChanged += delegate { if (!loading) { settings.SelectedProfile = profiles.SelectedIndex; ApplyProfile(); PersistSettings(); RefreshPreview(); } };
            profileRow.Controls.Add(profiles); profileRow.Controls.Add(Button("Nuevo perfil", AddProfile)); right.Controls.Add(profileRow, 0, 3);
            TableLayoutPanel calibration = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 3 };
            for (int i = 0; i < 6; i++) calibration.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666f));
            calibration.RowStyles.Add(new RowStyle(SizeType.Absolute, 25)); calibration.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); calibration.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            calibration.Controls.Add(TextLabel("Horizontal (mm)"), 0, 0); calibration.SetColumnSpan(calibration.GetControlFromPosition(0, 0), 2);
            calibration.Controls.Add(TextLabel("Vertical (mm)"), 2, 0); calibration.SetColumnSpan(calibration.GetControlFromPosition(2, 0), 2);
            calibration.Controls.Add(TextLabel("Letra (%)"), 4, 0); calibration.SetColumnSpan(calibration.GetControlFromPosition(4, 0), 2);
            ConfigureNumber(offsetX, -10, 10, .25m, 2); ConfigureNumber(offsetY, -10, 10, .25m, 2); ConfigureNumber(fontScale, 80, 120, 1, 0);
            calibration.Controls.Add(offsetX, 0, 1); calibration.SetColumnSpan(offsetX, 2);
            calibration.Controls.Add(offsetY, 2, 1); calibration.SetColumnSpan(offsetY, 2);
            calibration.Controls.Add(fontScale, 4, 1); calibration.SetColumnSpan(fontScale, 2);
            printSeries.Text = "Imprimir serie en PDF solo datos (si falta en el papel)"; printSeries.AutoSize = true; printSeries.Dock = DockStyle.Fill; printSeries.CheckedChanged += CalibrationChanged;
            calibration.Controls.Add(printSeries, 0, 2); calibration.SetColumnSpan(printSeries, 6);
            right.Controls.Add(calibration, 0, 4);
            FlowLayoutPanel exportRow = Flow();
            Button export = Button("PDF con plantilla", delegate { ExportPdf(true); }); export.BackColor = Blue; export.ForeColor = Color.White;
            exportRow.Controls.Add(export);
            exportRow.Controls.Add(Button("PDF solo datos", delegate { ExportPdf(false); }));
            exportRow.Controls.Add(Button("Cómo imprimir", delegate { ShowInfo("PDF CON PLANTILLA\nIncluye logo, textos, líneas y datos. Para papel blanco.\n\nPDF SOLO DATOS\nPara el formulario que ya tiene el diseño impreso.\n\nPapel de 200 × 145 mm, Tamaño real o 100 %, sin ajustar a página. Las guías no se imprimen.\n\nUn borrador muestra PRUEBA. Después de Emitir, ambos PDF conservan el número asignado por el principal. Reimprimir no consume otro número."); }));
            right.Controls.Add(exportRow, 0, 5); return right;
        }
        void ConfigureNumber(NumericUpDown input, decimal min, decimal max, decimal increment, int decimals)
        {
            input.Minimum = min; input.Maximum = max; input.Increment = increment; input.DecimalPlaces = decimals; input.Dock = DockStyle.Fill; input.ValueChanged += CalibrationChanged;
        }
        Profile ActiveProfile { get { return settings.Profiles[Math.Max(0, profiles.SelectedIndex)]; } }
        void LoadSettings()
        {
            try { if (File.Exists(Storage.ConfigPath)) { settings = Storage.ReadJson<Settings>(Storage.ConfigPath); if (settings == null) throw new InvalidDataException("Archivo vacío."); settings.Validate(); } }
            catch (Exception ex) { settings = new Settings(); ShowInfo("No se pudieron cargar los perfiles. Se usarán valores iniciales.\n\n" + ex.Message); }
            settings.UpgradeTemplate();
            loading = true; profiles.Items.Clear(); foreach (Profile profile in settings.Profiles) profiles.Items.Add(profile);
            profiles.SelectedIndex = settings.SelectedProfile; loading = false;
        }
        void ApplyProfile()
        {
            loading = true; Profile p = ActiveProfile; offsetX.Value = p.OffsetX; offsetY.Value = p.OffsetY; fontScale.Value = p.FontScale * 100; printSeries.Checked = p.PrintSeries; loading = false;
        }
        void CalibrationChanged(object sender, EventArgs args)
        {
            if (loading || !ready) return;
            Profile p = ActiveProfile; p.OffsetX = offsetX.Value; p.OffsetY = offsetY.Value; p.FontScale = fontScale.Value / 100; p.PrintSeries = printSeries.Checked;
            PersistSettings(); RefreshPreview();
        }
        void PersistSettings()
        {
            try { settings.Validate(); Storage.WriteJson(Storage.ConfigPath, settings); }
            catch (Exception ex) { footer.Text = "No se guardó el perfil: " + ex.Message; }
        }
        void AddProfile(object sender, EventArgs args)
        {
            using (Form dialog = new Form { Text = "Nuevo perfil de impresora", Width = 370, Height = 160, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, StartPosition = FormStartPosition.CenterParent })
            {
                TextBox name = new TextBox { Left = 15, Top = 20, Width = 320, MaxLength = 50 };
                Button accept = new Button { Text = "Crear", Left = 235, Top = 65, Width = 100, DialogResult = DialogResult.OK };
                dialog.Controls.Add(name); dialog.Controls.Add(accept); dialog.AcceptButton = accept;
                if (dialog.ShowDialog(this) != DialogResult.OK || String.IsNullOrWhiteSpace(name.Text)) return;
                foreach (Profile p in settings.Profiles) if (String.Equals(p.Name, name.Text.Trim(), StringComparison.OrdinalIgnoreCase)) { ShowInfo("Ya existe un perfil con ese nombre."); return; }
                Profile profile = new Profile { Name = name.Text.Trim() }; settings.Profiles.Add(profile); profiles.Items.Add(profile); profiles.SelectedIndex = profiles.Items.Count - 1;
            }
        }
        void LoadTemplate()
        {
            Bitmap loaded = null;
            try
            {
                if (File.Exists(Storage.TemplatePath))
                    using (Image source = Image.FromFile(Storage.TemplatePath)) loaded = new Bitmap(source);
            }
            catch (Exception) { footer.Text = "Se usará la plantilla original incluida: no se pudo leer la copia personalizada."; }
            if (loaded == null) loaded = OriginalTemplate.Load();
            if (preview.Template != null) preview.Template.Dispose();
            preview.Template = loaded;
        }
        void RestoreSession()
        {
            if (File.Exists(Storage.SessionPath))
            {
                try { SetDraft(Storage.ReadJson<Draft>(Storage.SessionPath)); dirty = false; return; }
                catch (Exception)
                {
                    // Preserve an unreadable user session before any later autosave.
                    string backup = Storage.SessionPath + ".recuperacion-" + Guid.NewGuid().ToString("N") + ".json";
                    File.Copy(Storage.SessionPath, backup);
                    footer.Text = "El borrador anterior no se pudo recuperar. Se conservó una copia para revisión.";
                }
            }
            SetDraft(Draft.Sample()); dirty = false;
        }
        void SaveSession()
        {
            try { Draft draft = ReadDraft(); if (workflow != null) workflow.Edit(draft); Storage.WriteJson(Storage.SessionPath, draft); }
            catch (Exception ex) { footer.Text = "No se guardó la recuperación automática: " + ex.Message; }
        }
        void ImportTemplate(object sender, EventArgs args)
        {
            using (OpenFileDialog dialog = new OpenFileDialog { Title = "Selecciona la imagen original completa de tu nota de venta", Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    using (Image source = Image.FromFile(dialog.FileName))
                    {
                        if (source.Width > 12000 || source.Height > 12000) throw new ArgumentException("La imagen es demasiado grande. Usa una copia de hasta 12.000 píxeles por lado.");
                        using (Bitmap image = new Bitmap(source))
                        using (MemoryStream stream = new MemoryStream())
                        { image.Save(stream, System.Drawing.Imaging.ImageFormat.Png); Storage.WriteBytes(Storage.TemplatePath, stream.ToArray()); }
                    }
                    LoadTemplate(); preview.Invalidate(); footer.Text = "Plantilla guardada. PDF con plantilla incluye el fondo; PDF solo datos sirve para papel preimpreso.";
                }
                catch (Exception ex) { ShowInfo("No se pudo cargar la plantilla.\n\n" + ex.Message); }
            }
        }
        Draft ReadDraft()
        {
            if (!refreshing) grid.EndEdit();
            Draft draft = new Draft { Customer = customer.Text, Identification = identification.Text, Address = address.Text, Phone = phone.Text, Email = email.Text, Date = date.Value.ToString("yyyy-MM-dd") };
            foreach (DataGridViewRow row in grid.Rows)
                draft.Lines.Add(new DraftLine { Quantity = Convert.ToString(row.Cells[0].Value), Description = Convert.ToString(row.Cells[1].Value), UnitPrice = Convert.ToString(row.Cells[2].Value) });
            return draft;
        }
        void SetDraft(Draft draft)
        {
            if (draft == null || draft.Version != 1 || draft.Lines == null || draft.Lines.Count > 10) throw new ArgumentException("Este archivo no corresponde a un borrador compatible de hasta diez filas.");
            DateTime savedDate;
            if (!DateTime.TryParseExact(draft.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out savedDate) || savedDate < date.MinDate || savedDate > date.MaxDate)
                throw new ArgumentException("El borrador contiene una fecha inválida.");
            foreach (DraftLine line in draft.Lines) if (line == null) throw new ArgumentException("El borrador contiene una fila inválida.");
            loading = true;
            customer.Text = draft.Customer ?? ""; identification.Text = draft.Identification ?? ""; address.Text = draft.Address ?? ""; phone.Text = draft.Phone ?? ""; email.Text = draft.Email ?? ""; date.Value = savedDate;
            grid.Rows.Clear(); foreach (DraftLine line in draft.Lines) grid.Rows.Add(line.Quantity, line.Description, line.UnitPrice, "");
            if (grid.Rows.Count == 0) grid.Rows.Add();
            loading = false; RefreshPreview();
        }
        void Changed(object sender, EventArgs args)
        {
            if (loading || !ready || (workflow != null && workflow.Locked)) return;
            dirty = true; RefreshPreview(); sessionTimer.Stop(); sessionTimer.Start();
        }
        void RefreshPreview()
        {
            if (loading || refreshing) return;
            refreshing = true;
            try
            {
            Draft draft = ReadDraft();
            foreach (DataGridViewRow row in grid.Rows)
            {
                row.Cells[3].Value = "";
                try
                {
                    Draft single = new Draft(); single.Lines.Add(draft.Lines[row.Index]);
                    List<CalculatedLine> one = Calculation.Lines(single); row.Cells[3].Value = Calculation.Money(one[0].Amount);
                }
                catch (ArgumentException) { }
            }
            try
            {
                List<CalculatedLine> lines = Calculation.Lines(draft);
                total.Text = "TOTAL  US$ " + Calculation.Money(Calculation.Total(lines));
                preview.Items = Bodoni.Layout.Build(draft, ActiveProfile, false, WorkNumber);
                try { Bodoni.Layout.Build(draft, ActiveProfile, true, WorkNumber); validation.Text = WorkNumber == "PRUEBA" ? "Borrador: los PDF muestran PRUEBA. Emite para obtener el número compartido." : "Nota emitida " + WorkNumber + ". Ambos PDF conservan este número al reimprimir."; validation.ForeColor = Color.FromArgb(35, 110, 80); }
                catch (ArgumentException ex) { validation.Text = ex.Message; validation.ForeColor = Color.FromArgb(160, 75, 12); }
            }
            catch (Exception ex)
            {
                preview.Items = new List<TextItem>(); total.Text = "TOTAL  US$ —"; validation.ForeColor = Color.FromArgb(160, 75, 12); validation.Text = ex.Message;
            }
            preview.Invalidate();
            }
            finally { refreshing = false; }
        }
        bool ConfirmDiscard()
        {
            grid.EndEdit();
            if (!dirty) return true;
            DialogResult choice = MessageBox.Show(this, "¿Deseas guardar los cambios del borrador antes de continuar?", "Borrador modificado", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (choice == DialogResult.Cancel) return false;
            if (choice == DialogResult.Yes) return SaveCurrentDraft();
            return true;
        }
        void OpenDraft(object sender, EventArgs args)
        {
            if (!CanLeaveWork()) return;
            if (!ConfirmDiscard()) return;
            using (OpenFileDialog dialog = new OpenFileDialog { Title = "Abrir borrador de prueba", Filter = "Borrador Bodoni|*.bodoni.json", InitialDirectory = Storage.Root })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try { Draft draft = Storage.ReadJson<Draft>(dialog.FileName); SetDraft(draft); workflow.New(draft); UpdateWorkControls(); draftPath = dialog.FileName; dirty = false; SaveSession(); footer.Text = "Borrador recuperado: " + Path.GetFileName(draftPath); }
                catch (Exception ex) { ShowInfo("No se pudo abrir el borrador.\n\n" + ex.Message); }
            }
        }
        void SaveDraft(object sender, EventArgs args) { SaveCurrentDraft(); }
        bool SaveCurrentDraft()
        {
            if (workflow != null && workflow.Locked) { ShowInfo("La nota emitida se conserva en el principal. Usa los botones PDF para reimprimirla, o Nueva para crear otra."); return false; }
            try
            {
                Draft draft = ReadDraft();
                using (SaveFileDialog dialog = new SaveFileDialog { Title = "Guardar borrador de prueba", Filter = "Borrador Bodoni|*.bodoni.json", DefaultExt = "bodoni.json", FileName = draftPath == null ? "nota-prueba.bodoni.json" : Path.GetFileName(draftPath), InitialDirectory = draftPath == null ? Storage.Root : Path.GetDirectoryName(draftPath), OverwritePrompt = true })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return false;
                    Storage.WriteJson(dialog.FileName, draft); draftPath = dialog.FileName; dirty = false; footer.Text = "Borrador guardado sin consumir numeración: " + Path.GetFileName(draftPath); return true;
                }
            }
            catch (Exception ex) { ShowInfo("No se pudo guardar el borrador.\n\n" + ex.Message); return false; }
        }
        async void ExportPdf(bool includeTemplate)
        {
            if (networkBusy) return;
            try
            {
                if (workflow != null && workflow.State.Pending) throw new InvalidOperationException("Primero recupera la emisión pendiente para saber qué número le asignó el principal.");
                if (workflow != null && workflow.State.Issued != null)
                {
                    if (networkConfig == null || networkConfig.ServerId != workflow.State.Issued.ServerId) throw new InvalidOperationException("Conecta con el principal original para comprobar que la nota sigue vigente.");
                    networkBusy = true; UpdateWorkControls();
                    NetworkConfig config = NetworkJson.Copy(networkConfig); string requestId = workflow.State.Issued.RequestId;
                    IssuedNote current = await System.Threading.Tasks.Task.Factory.StartNew(delegate { return new NumberClient(config).Find(requestId); });
                    if (IsDisposed) return;
                    if (current == null) throw new InvalidOperationException("No se encontró la nota en el principal.");
                    workflow.OpenIssued(current);
                    if (current.Cancelled) throw new InvalidOperationException("Esta nota está anulada y se conserva para consulta. No se genera un PDF válido de una nota anulada.");
                }
                Draft draft = ReadDraft(); Bodoni.Layout.Build(draft, ActiveProfile, true, WorkNumber);
                using (SaveFileDialog dialog = new SaveFileDialog { Title = includeTemplate ? "Guardar PDF completo con plantilla" : "Guardar PDF SOLO DATOS para papel preimpreso", Filter = "Documento PDF|*.pdf", DefaultExt = "pdf", FileName = "Bodoni-" + (Program.PracticeMode ? "PRACTICA-" : "") + WorkNumber + (includeTemplate ? "-completa.pdf" : "-solo-datos.pdf"), InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entregables"), OverwritePrompt = true })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    SavePdfForMode(dialog.FileName, draft, includeTemplate);
                    footer.Text = includeTemplate ? "PDF completo guardado: incluye plantilla y datos. Para imprimirlo, usa papel blanco al 100 %." : "PDF solo datos guardado: imprime al 100 % sobre el papel preimpreso. No se consumió numeración.";
                    if (MessageBox.Show(this, "PDF " + WorkNumber + " generado. ¿Deseas abrirlo?", "PDF listo", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex) { ShowInfo("No se pudo generar o abrir el PDF.\n\n" + ex.Message); }
            finally { networkBusy = false; if (!IsDisposed) UpdateWorkControls(); }
        }
        void SavePdfForMode(string path, Draft draft, bool includeTemplate)
        {
            if (workflow != null && workflow.State.Issued != null) Pdf.SaveIssued(path, workflow.State.Issued, ActiveProfile, includeTemplate ? preview.Template : null);
            else if (includeTemplate) Pdf.Save(path, draft, ActiveProfile, preview.Template);
            else Pdf.Save(path, draft, ActiveProfile);
        }
        void OnClosing(object sender, FormClosingEventArgs args)
        {
            if (!ConfirmDiscard()) { args.Cancel = true; return; }
            sessionTimer.Stop(); SaveSession(); PersistSettings();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { sessionTimer.Dispose(); networkTimer.Dispose(); if (numberServer != null) numberServer.Dispose(); }
            base.Dispose(disposing);
        }
        // Offline UI smoke test: exercise the real controls and render them
        // without opening a window or touching the user's draft/profile files.
        internal void VerifyUi(string imagePath)
        {
            if (preview.Template == null) throw new Exception("La plantilla original no se carga automáticamente.");
            SetDraft(Draft.Sample());
            PerformLayout();
            TableLayoutPanel root = (TableLayoutPanel)Controls[0];
            SplitContainer split = (SplitContainer)root.GetControlFromPosition(0, 3);
            split.SplitterDistance = Math.Max(540, split.Width / 2);
            networkStatus.Parent.PerformLayout();
            if (networkStatus.Width < 200 || networkStatus.Height < 18) throw new Exception("El estado de conexión y numeración no tiene espacio visible en la pantalla.");
            if (profiles.SelectedIndex < 0 || String.IsNullOrWhiteSpace(profiles.Text)) throw new Exception("El perfil de impresora no está seleccionado en la pantalla.");
            if (!total.Text.EndsWith("50.00")) throw new Exception("La pantalla no muestra el total esperado.");
            if (imagePath != null)
            {
                if (!Visible) throw new Exception("La captura requiere que la ventana esté visible.");
                using (Bitmap bitmap = new Bitmap(Width, Height))
                {
                    DrawToBitmap(bitmap, new Rectangle(Point.Empty, Size));
                    bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            grid.CurrentCell = grid.Rows[0].Cells[0];
            if (!grid.BeginEdit(false)) throw new Exception("No se pudo iniciar la edición del detalle.");
            ((TextBox)grid.EditingControl).Text = "101";
            grid.EndEdit();
            if (!total.Text.EndsWith("50.25")) throw new Exception("La edición del detalle no actualiza el total.");
            customer.Text = "";
            if (!validation.Text.Contains("cliente")) throw new Exception("No se valida el cliente desde la pantalla.");
            dirty = false;
        }
        internal void VerifyStartup(string expectedCustomer, decimal expectedTotal)
        {
            if (preview.Template == null || preview.Template.Width != 2247 || preview.Template.Height != 1534)
                throw new Exception("No se cargó la plantilla original incluida.");
            if (customer.Text != expectedCustomer || !total.Text.EndsWith(Calculation.Money(expectedTotal)))
                throw new Exception("No se recuperaron automáticamente los datos esperados al abrir.");
        }
        internal void VerifyPdfModes(string directory)
        {
            Draft draft = ReadDraft();
            string completePath = Path.Combine(directory, "ui-con-plantilla.pdf");
            string dataPath = Path.Combine(directory, "ui-solo-datos.pdf");
            SavePdfForMode(completePath, draft, true);
            SavePdfForMode(dataPath, draft, false);
            string complete = System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(completePath));
            string data = System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(dataPath));
            if (!complete.Contains("/Subtype /Image") || data.Contains("/Subtype /Image"))
                throw new Exception("Las opciones de PDF no incluyen o excluyen correctamente la plantilla.");
        }
        internal void VerifyIssuedUi(IssuedNote note, string directory)
        {
            workflow.OpenIssued(note); SetDraft(note.Note); UpdateWorkControls();
            if (!customer.ReadOnly || !identification.ReadOnly || date.Enabled || !grid.ReadOnly || WorkNumber != note.DisplayNumber)
                throw new Exception("La pantalla permite modificar una nota emitida o no muestra su número.");
            string file = Path.Combine(directory, "nota-ui-numerada.pdf"); SavePdfForMode(file, ReadDraft(), true);
            string pdf = System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(file));
            if (!pdf.Contains("(" + note.DisplayNumber + ") Tj") || pdf.Contains("(PRUEBA) Tj")) throw new Exception("El PDF de la pantalla no conserva el número emitido.");
        }
        void ShowInfo(string message) { MessageBox.Show(this, message, "Bodoni", MessageBoxButtons.OK, MessageBoxIcon.Information); }
    }
}
