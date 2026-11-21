using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bodoni
{
    public sealed partial class MainForm
    {
        IssueWorkflow workflow;
        NetworkConfig networkConfig;
        NumberServer numberServer;
        Label networkNotice, networkStatus;
        Button issueButton;
        bool networkBusy, polling, connected;
        int connectionGeneration;
        readonly Timer networkTimer = new Timer { Interval = 4000 };
        string WorkNumber { get { return workflow == null ? "PRUEBA" : workflow.Number; } }
        string WorkPath { get { return Path.Combine(Storage.Root, "tramite-actual.json"); } }

        Control BuildNetworkBar()
        {
            TableLayoutPanel bar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.White };
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bar.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            bar.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            bar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            FlowLayoutPanel actions = Flow();
            actions.Controls.Add(Button("Red y numeración", ConfigureNetwork));
            issueButton = Button("Emitir y asignar número", IssueCurrent); issueButton.BackColor = Blue; issueButton.ForeColor = Color.White; issueButton.Enabled = false;
            actions.Controls.Add(issueButton); actions.Controls.Add(Button("Historial", ShowHistory)); actions.Controls.Add(Button("Exportar conexión", ExportConnection));
            actions.Controls.Add(Button("Última numeración", ShowNumbering));
            actions.Controls.Add(Button("Anular última nota", CancelLastNote));
            actions.Controls.Add(Button("Pie de imprenta", ConfigureAuthorization));
            bar.Controls.Add(actions, 0, 0);
            networkStatus = TextLabel("Sin principal configurado · Los borradores no consumen números."); networkStatus.MinimumSize = new Size(260, 30); networkStatus.Padding = new Padding(8, 0, 0, 0);
            bar.Controls.Add(networkStatus, 0, 1);
            authorizationNotice = TextLabel("Conecta con el principal para comprobar la autorización del pie de imprenta.");
            authorizationNotice.Padding = new Padding(8, 0, 0, 0); bar.Controls.Add(authorizationNotice, 0, 2); return bar;
        }
        void RestoreWork()
        {
            workflow = new IssueWorkflow(WorkPath, ReadDraft());
            SetDraft(workflow.State.Draft); dirty = false;
        }
        bool CanEditWork()
        {
            if (workflow != null && workflow.Locked) { ShowInfo("Este documento está emitido o pendiente de recuperación. Usa Nueva para otra nota, o recupera la emisión pendiente."); return false; }
            return true;
        }
        bool CanLeaveWork()
        {
            if (networkBusy) { ShowInfo("Espera a que termine la consulta al principal."); return false; }
            if (workflow != null && workflow.State.Pending) { ShowInfo("Recupera primero la emisión pendiente. La misma solicitud puede tener ya un número en el principal."); return false; }
            return true;
        }
        void BeginDraft(Draft draft)
        {
            if (!CanLeaveWork() || !ConfirmDiscard()) return;
            try { workflow.New(draft); SetDraft(draft); draftPath = null; dirty = false; SaveSession(); UpdateWorkControls(); }
            catch (Exception ex) { ShowInfo(ex.Message); }
        }
        void UpdateWorkControls()
        {
            bool locked = workflow != null && workflow.Locked;
            foreach (TextBox input in new TextBox[] { customer, identification, address, phone, email }) input.ReadOnly = locked;
            date.Enabled = !locked; grid.ReadOnly = locked;
            if (!locked) grid.Columns[3].ReadOnly = true;
            bool pending = workflow != null && workflow.State.Pending;
            bool issued = workflow != null && workflow.State.Issued != null;
            issueButton.Text = pending ? "Recuperar emisión" : issued ? "Nota " + WorkNumber + " emitida" : "Emitir y asignar número";
            issueButton.Enabled = !networkBusy && !issued && networkConfig != null && (connected || pending);
            networkNotice.Text = pending ? "EMISIÓN PENDIENTE · Se conservará la misma solicitud al reconectar. No crees otra nota para reemplazarla."
                : issued ? "NOTA EMITIDA " + WorkNumber + " · Conserva su número al generar otro PDF o reimprimir. Usa Nueva para la siguiente venta."
                : "BORRADOR · Los PDF de prueba no consumen numeración. Usa Emitir para asignar el número desde el principal.";
            networkNotice.BackColor = issued ? Color.FromArgb(221, 243, 230) : Color.FromArgb(255, 242, 210);
            if (issued && workflow.State.Issued.Cancelled) networkNotice.Text = "NOTA ANULADA " + WorkNumber + " · " + workflow.State.Issued.CancellationReason + " · Se conserva en el historial.";
            if (Program.PracticeMode) networkNotice.Text = "PRÁCTICA · " + networkNotice.Text;
            UpdateAuthorizationControls();
        }
        void LoadNetwork()
        {
            networkTimer.Tick += delegate { PollNetwork(); };
            if (!File.Exists(NetworkConfig.ConfigPath)) return;
            try { ApplyNetwork(NetworkJson.Read<NetworkConfig>(NetworkConfig.ConfigPath)); }
            catch (Exception ex) { networkStatus.Text = "No se inició la conexión: " + ex.Message; connected = false; UpdateWorkControls(); }
        }
        void ApplyNetwork(NetworkConfig config)
        {
            config.Validate(); connectionGeneration++; connected = false; authorizationStatus = null;
            if (numberServer != null) { numberServer.Dispose(); numberServer = null; }
            networkConfig = NetworkJson.Copy(config);
            if (networkConfig.Role == "Principal")
            {
                numberServer = new NumberServer(networkConfig, NetworkConfig.LedgerPath, Program.PracticeMode ? IPAddress.Loopback : IPAddress.Any, networkConfig.Port);
                if (!networkConfig.Initialized)
                {
                    networkConfig.Initialized = true;
                    try { NetworkJson.Save(NetworkConfig.ConfigPath, networkConfig); }
                    catch { numberServer.Dispose(); numberServer = null; throw; }
                }
            }
            networkStatus.Text = "Conectando con " + (networkConfig.Role == "Principal" ? "este principal" : networkConfig.Host) + "…";
            UpdateWorkControls(); networkTimer.Start(); PollNetwork();
        }
        async void PollNetwork()
        {
            if (polling || networkBusy || networkConfig == null || IsDisposed) return;
            polling = true; int generation = connectionGeneration; NetworkConfig config = NetworkJson.Copy(networkConfig);
            try
            {
                CentralStatus status = await Task.Factory.StartNew(delegate { return new NumberClient(config).Status(); });
                if (IsDisposed || generation != connectionGeneration) return;
                connected = true;
                networkStatus.Text = (config.Role == "Principal" ? "PRINCIPAL" : config.Host) + " · Último " + status.LastNumber.ToString("D6") + " · Próximo " + (status.NextNumber > 999999 ? "agotado" : status.NextNumber.ToString("D6"));
                networkStatus.ForeColor = Color.FromArgb(30, 105, 70);
                AcceptAuthorizationStatus(status, true);
            }
            catch (Exception)
            {
                if (IsDisposed || generation != connectionGeneration) return;
                connected = false; authorizationStatus = null; networkStatus.Text = "Sin conexión al principal · No se asignarán números localmente."; networkStatus.ForeColor = Color.Firebrick;
            }
            finally { polling = false; if (!IsDisposed) UpdateWorkControls(); }
        }
        void ConfigureNetwork(object sender, EventArgs args)
        {
            if (networkBusy) return;
            if (Program.PracticeMode) { ShowInfo("Modo de práctica: principal local en este computador, último inicial 004121, puerto 47832. Los datos se guardan en datos_practica. Para configurar principal o secundario de producción, cierra esta ventana y abre Bodoni.Red.exe normalmente."); return; }
            using (NetworkSetupForm dialog = new NetworkSetupForm(networkConfig))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    if (workflow.State.Pending && workflow.State.ServerId != dialog.Result.ServerId) throw new InvalidOperationException("Hay una emisión pendiente en otro principal. Recupera primero esa conexión.");
                    NetworkJson.Save(NetworkConfig.ConfigPath, dialog.Result); ApplyNetwork(dialog.Result);
                }
                catch (Exception ex) { connected = false; UpdateWorkControls(); ShowInfo("No se pudo iniciar la conexión. La configuración se conserva para reintentar.\n\n" + ex.Message); }
            }
        }
        void ExportConnection(object sender, EventArgs args)
        {
            if (Program.PracticeMode) { ShowInfo("La práctica está aislada en este computador. Exporta la conexión desde el principal de producción cuando lo configures en la oficina."); return; }
            if (networkConfig == null || networkConfig.Role != "Principal" || numberServer == null) { ShowInfo("Exporta la conexión desde el equipo configurado como principal y con el servicio iniciado."); return; }
            using (SaveFileDialog dialog = new SaveFileDialog { Title = "Guardar conexión para los otros equipos de Bodoni", Filter = "Conexión Bodoni|*.bodoni-red.json", FileName = "Conexion-Bodoni.bodoni-red.json", DefaultExt = "bodoni-red.json", OverwritePrompt = true })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try { NetworkJson.Save(dialog.FileName, networkConfig.ForClient(String.IsNullOrWhiteSpace(networkConfig.Host) || networkConfig.Host == "127.0.0.1" ? Environment.MachineName : networkConfig.Host)); ShowInfo("Conexión exportada. Copia este archivo a los otros equipos e impórtalo en Red y numeración. Si el nombre del principal no responde, usa su dirección IP en esos equipos."); }
                catch (Exception ex) { ShowInfo("No se pudo exportar la conexión.\n\n" + ex.Message); }
            }
        }
        async void ShowNumbering(object sender, EventArgs args)
        {
            if (networkBusy) return;
            if (networkConfig == null) { ShowInfo("Configura primero el principal en Red y numeración."); return; }
            networkBusy = true; UpdateWorkControls();
            NetworkConfig config = NetworkJson.Copy(networkConfig);
            try
            {
                CentralStatus status = await Task.Factory.StartNew(delegate { return new NumberClient(config).Status(); });
                if (IsDisposed) return;
                ShowInfo((Program.PracticeMode ? "PRÁCTICA · Datos separados de producción\n\n" : "") + "Principal: " + config.Host + "\nSerie: " + status.Series + "\nÚltima numeración utilizada: " + status.LastNumber.ToString("D6") + "\nSiguiente al emitir: " + (status.NextNumber > 999999 ? "agotado" : status.NextNumber.ToString("D6")) + "\nNotas registradas: " + status.Count + (status.NextNumber == status.LastNumber ? "\nLa última nota está anulada; el siguiente documento reemplazará ese número." : "") + "\n\n" + status.AuthorizationNotice + "\n\nConsultar no consume ni reserva números.");
            }
            catch (Exception ex) { if (!IsDisposed) ShowInfo("No se pudo consultar la numeración actual.\n\n" + ex.Message); }
            finally { networkBusy = false; if (!IsDisposed) UpdateWorkControls(); }
        }
        async void CancelLastNote(object sender, EventArgs args)
        {
            if (networkBusy || !CanLeaveWork()) return;
            if (networkConfig == null || workflow.State.Issued == null) { ShowInfo("Abre la última nota emitida desde Historial antes de anularla."); return; }
            string reason;
            using (Form dialog = new Form { Text = "Anular y reemplazar última nota " + WorkNumber, Width = 540, Height = 230, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false })
            {
                Label label = new Label { Text = "Se conservará la original anulada. La siguiente emisión reutilizará su número. Solo se permite si no existe una nota posterior.\n\nMotivo de la anulación:", Left = 16, Top = 16, Width = 490, Height = 70 };
                TextBox input = new TextBox { Left = 16, Top = 88, Width = 490, MaxLength = 300 };
                Button accept = new Button { Text = "Confirmar anulación", Left = 195, Top = 130, Width = 165, DialogResult = DialogResult.OK };
                Button cancel = new Button { Text = "Cancelar", Left = 370, Top = 130, Width = 135, DialogResult = DialogResult.Cancel };
                dialog.Controls.AddRange(new Control[] { label, input, accept, cancel }); dialog.CancelButton = cancel;
                if (dialog.ShowDialog(this) != DialogResult.OK) return; reason = input.Text.Trim();
            }
            NetworkConfig config = NetworkJson.Copy(networkConfig); string requestId = workflow.State.Issued.RequestId;
            if (workflow.State.Issued.ServerId != config.ServerId) { ShowInfo("Esta nota pertenece a otro principal. Recupera su conexión original."); return; }
            networkBusy = true; UpdateWorkControls();
            try
            {
                IssuedNote note = await Task.Factory.StartNew(delegate { return new NumberClient(config).CancelLast(requestId, reason); });
                if (IsDisposed) return;
                workflow.OpenIssued(note); UpdateWorkControls();
                ShowInfo("Nota " + note.DisplayNumber + " anulada y conservada en Historial. Pulsa Nueva para preparar su reemplazo. El principal comprobará el número disponible al emitir.");
            }
            catch (Exception ex) { if (!IsDisposed) ShowInfo("No se pudo confirmar la anulación. Consulta la nota en Historial antes de continuar; puedes reintentar sobre la misma nota.\n\n" + ex.Message); }
            finally { networkBusy = false; if (!IsDisposed) { UpdateWorkControls(); PollNetwork(); } }
        }
        async void IssueCurrent(object sender, EventArgs args)
        {
            if (networkBusy || networkConfig == null || workflow.State.Issued != null) return;
            try
            {
                if (!workflow.State.Pending)
                {
                    string authorizationBlock = CurrentAuthorizationBlock();
                    if (authorizationBlock != null) throw new AuthorizationException(authorizationBlock);
                    Draft draft = ReadDraft(); Bodoni.Layout.Build(draft, ActiveProfile, true); NoteRules.Normalize(draft);
                    if (MessageBox.Show(this, "Se guardará esta nota en el principal y se le asignará el siguiente número real. Después podrás generar sus PDF o reimprimir con ese mismo número.\n\n¿Emitir la nota de venta?", "Emitir nota de venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                    workflow.Prepare(draft, networkConfig.ServerId);
                }
                else workflow.Prepare(workflow.State.Draft, networkConfig.ServerId);
                dirty = false; networkBusy = true; UpdateWorkControls();
                NetworkConfig config = NetworkJson.Copy(networkConfig); WorkState request = NetworkJson.Copy(workflow.State);
                IssuedNote note = await Task.Factory.StartNew(delegate { return new NumberClient(config).Issue(request.RequestId, request.Draft); });
                // The saved request survives even if this window closes before
                // the response. Recovery always uses the same identifier.
                if (IsDisposed) return;
                workflow.Complete(note); SetDraft(note.Note); dirty = false; SaveSession();
                footer.Text = "Nota " + note.DisplayNumber + " guardada en el principal. Genera el PDF completo o solo datos; reimprimir no cambia su número.";
            }
            catch (AuthorizationException ex)
            {
                if (IsDisposed) return;
                try { workflow.ReleaseRejectedAuthorization(); dirty = false; SaveSession(); }
                catch (Exception saveError) { ShowInfo("El rechazo se recibió, pero no se pudo guardar el borrador. Se conserva la solicitud para recuperar.\n\n" + saveError.Message); }
                ShowInfo(ex.Message);
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                ShowInfo("No se completó la confirmación de emisión.\n\n" + ex.Message + (workflow.State.Pending ? "\n\nLa solicitud está guardada. Pulsa Recuperar emisión al reconectar; se utilizará el mismo número si el principal ya la guardó." : ""));
            }
            finally { networkBusy = false; if (!IsDisposed) { UpdateWorkControls(); RefreshPreview(); PollNetwork(); } }
        }
        async void ShowHistory(object sender, EventArgs args)
        {
            if (!CanLeaveWork() || !ConfirmDiscard()) return;
            if (networkConfig == null) { ShowInfo("Configura primero la conexión con el principal."); return; }
            networkBusy = true; UpdateWorkControls();
            try
            {
                NetworkConfig config = NetworkJson.Copy(networkConfig);
                System.Collections.Generic.List<NoteSummary> notes = await Task.Factory.StartNew(delegate { return new NumberClient(config).Recent(); });
                if (IsDisposed) return;
                using (NoteHistoryForm dialog = new NoteHistoryForm(notes))
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    IssuedNote note = await Task.Factory.StartNew(delegate { return new NumberClient(config).Find(dialog.SelectedRequest); });
                    if (IsDisposed) return;
                    if (note == null) throw new InvalidDataException("El principal no encontró esa nota.");
                    workflow.OpenIssued(note); SetDraft(note.Note); dirty = false; UpdateWorkControls();
                }
            }
            catch (Exception ex) { if (!IsDisposed) ShowInfo("No se pudo consultar el historial.\n\n" + ex.Message); }
            finally { networkBusy = false; if (!IsDisposed) UpdateWorkControls(); }
        }
    }
}
