using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Bodoni
{
    public sealed class AuthorizationForm : Form
    {
        public PrintAuthorization Result { get; private set; }
        public AuthorizationForm(PrintAuthorization current, List<PrintAuthorization> history, bool editable)
        {
            Text = "Pie de imprenta y autorización"; Size = new Size(760, 660); MinimumSize = Size;
            StartPosition = FormStartPosition.CenterParent; Font = new Font("Segoe UI", 9);
            TableLayoutPanel panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(16) };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190)); panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            int[] heights = { 68, 38, 38, 38, 115, 40, 170, 45 };
            foreach (int height in heights) panel.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            Controls.Add(panel);
            Label explanation = new Label { Dock = DockStyle.Fill, Text = "Copia los datos de la autorización y del pie del nuevo formulario. El número AUT. S.R.I. de la nota es distinto de la autorización de la imprenta. Guardar estos datos no modifica la imagen de la plantilla ni reinicia la numeración." };
            panel.Controls.Add(explanation, 0, 0); panel.SetColumnSpan(explanation, 2);
            TextBox number = new TextBox { Text = current.Number, MaxLength = 40, ReadOnly = !editable };
            DateTimePicker start = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Value = DateTime.ParseExact(current.StartsOn, "yyyy-MM-dd", CultureInfo.InvariantCulture), Enabled = editable };
            DateTimePicker expiry = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Value = DateTime.ParseExact(current.ExpiresOn, "yyyy-MM-dd", CultureInfo.InvariantCulture), Enabled = editable };
            TextBox footer = new TextBox { Text = current.Footer, Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 1500, ReadOnly = !editable };
            footer.TextChanged += delegate { DateTime first, last; if (editable && PrintAuthorization.TryReadFooterDates(footer.Text, out first, out last) && first >= start.MinDate && first <= start.MaxDate && last >= expiry.MinDate && last <= expiry.MaxDate) { start.Value = first; expiry.Value = last; } };
            Add(panel, "AUT. S.R.I. de la nota", number, 1); Add(panel, "Emisión del pie de imprenta", start, 2); Add(panel, "Caduca según el pie", expiry, 3); Add(panel, "Pie de imprenta completo", footer, 4);
            Label reminder = new Label { Dock = DockStyle.Fill, Text = editable ? "Después de renovar, usa Cambiar plantilla para cargar la imagen del nuevo formulario en cada equipo." : "La actualización se realiza en el computador principal. Aquí puedes consultar la vigencia y el historial." };
            panel.Controls.Add(reminder, 0, 5); panel.SetColumnSpan(reminder, 2);
            StringBuilder old = new StringBuilder();
            if (history != null) for (int i = history.Count - 1; i >= 0; i--)
            {
                PrintAuthorization item = history[i]; old.AppendLine("AUT. " + item.Number + " · " + item.StartsOn + " a " + item.ExpiresOn + " · Registrado: " + item.SavedUtc); old.AppendLine(item.Footer); old.AppendLine();
            }
            Add(panel, "Historial anterior", new TextBox { ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, Text = old.Length == 0 ? "Sin cambios anteriores." : old.ToString() }, 6);
            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            if (editable)
            {
                Button save = new Button { Text = "Guardar pie de imprenta", AutoSize = true, Height = 32 };
                save.Click += delegate {
                    try {
                        PrintAuthorization value = new PrintAuthorization { Number = number.Text.Trim(), StartsOn = start.Value.ToString("yyyy-MM-dd"), ExpiresOn = expiry.Value.ToString("yyyy-MM-dd"), Footer = footer.Text.Trim(), RevisionId = current.RevisionId };
                        value.Validate(); Result = value; DialogResult = DialogResult.OK; Close();
                    } catch (Exception ex) { MessageBox.Show(this, ex.Message, "Revisa el pie de imprenta", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                };
                actions.Controls.Add(save);
            }
            Button close = new Button { Text = "Cerrar", AutoSize = true, Height = 32, DialogResult = DialogResult.Cancel }; actions.Controls.Add(close); CancelButton = close;
            panel.Controls.Add(actions, 0, 7); panel.SetColumnSpan(actions, 2);
        }
        static void Add(TableLayoutPanel panel, string text, Control control, int row)
        {
            panel.Controls.Add(new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            control.Dock = DockStyle.Fill; control.Margin = new Padding(3, 6, 3, 4); panel.Controls.Add(control, 1, row);
        }
    }
}
