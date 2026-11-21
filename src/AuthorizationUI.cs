using System;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bodoni
{
    public sealed partial class MainForm
    {
        CentralStatus authorizationStatus;
        Label authorizationNotice;
        string shownAuthorizationAlert;

        string CurrentAuthorizationBlock()
        {
            if (authorizationStatus == null) return "Conecta con el principal para comprobar la autorización del pie de imprenta.";
            DateTime today;
            if (!DateTime.TryParseExact(authorizationStatus.ServerDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out today)) return "No se pudo comprobar la fecha del principal.";
            return AuthorizationRules.BlockReason(authorizationStatus.Authorization, today, date.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
        void UpdateAuthorizationControls()
        {
            if (authorizationNotice == null || workflow == null) return;
            string block = CurrentAuthorizationBlock();
            bool pending = workflow.State.Pending, issued = workflow.State.Issued != null;
            if (!pending && !issued && block != null) issueButton.Enabled = false;
            authorizationNotice.Text = block ?? authorizationStatus.AuthorizationNotice;
            bool warning = block != null || authorizationStatus == null || authorizationStatus.AuthorizationNotice.IndexOf("caduca", StringComparison.OrdinalIgnoreCase) >= 0;
            authorizationNotice.ForeColor = block != null ? Color.Firebrick : Color.FromArgb(112, 76, 0);
            authorizationNotice.BackColor = warning ? Color.FromArgb(255, 242, 210) : Color.FromArgb(235, 246, 239);
        }
        void AcceptAuthorizationStatus(CentralStatus status, bool alert)
        {
            authorizationStatus = status; UpdateWorkControls();
            DateTime today, end;
            if (!alert || status == null || status.Authorization == null || !DateTime.TryParse(status.ServerDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out today) || !DateTime.TryParse(status.Authorization.ExpiresOn, CultureInfo.InvariantCulture, DateTimeStyles.None, out end)) return;
            string key = status.ServerId + ":" + status.Authorization.RevisionId + ":" + status.ServerDate;
            if (((end.Date - today.Date).Days <= 14) && shownAuthorizationAlert != key)
            {
                shownAuthorizationAlert = key; ShowInfo(status.AuthorizationNotice);
            }
        }
        async void ConfigureAuthorization(object sender, EventArgs args)
        {
            if (networkBusy) return;
            if (networkConfig == null) { ShowInfo("Configura el principal en Red y numeración. Para practicar, abre Abrir_Practica.cmd."); return; }
            NetworkConfig config = NetworkJson.Copy(networkConfig); networkBusy = true; UpdateWorkControls();
            try
            {
                CentralStatus status = await Task.Factory.StartNew(delegate { return new NumberClient(config).Status(); });
                System.Collections.Generic.List<PrintAuthorization> history = await Task.Factory.StartNew(delegate { return new NumberClient(config).AuthorizationHistory(); });
                if (IsDisposed) return;
                AcceptAuthorizationStatus(status, false);
                using (AuthorizationForm dialog = new AuthorizationForm(status.Authorization, history, config.Role == "Principal"))
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    if (workflow.State.Pending) { ShowInfo("Recupera primero la emisión pendiente antes de cambiar el pie de imprenta."); return; }
                    PrintAuthorization value = dialog.Result;
                    CentralStatus updated = await Task.Factory.StartNew(delegate { return new NumberClient(config).UpdateAuthorization(value); });
                    if (IsDisposed) return;
                    AcceptAuthorizationStatus(updated, false);
                    ShowInfo("Pie de imprenta guardado en el principal. Se conserva el anterior y no cambia el consecutivo.\n\nSi cambió el formulario físico, carga su nueva imagen con Cambiar plantilla en cada equipo.\n\n" + updated.AuthorizationNotice);
                }
            }
            catch (Exception ex) { if (!IsDisposed) ShowInfo("No se pudo confirmar la actualización del pie de imprenta. Vuelve a abrir esta pantalla para consultar lo guardado antes de reintentar.\n\n" + ex.Message); }
            finally { networkBusy = false; if (!IsDisposed) { UpdateWorkControls(); PollNetwork(); } }
        }
        internal void VerifyAuthorizationUi(NetworkConfig config)
        {
            networkConfig = NetworkJson.Copy(config); connected = true;
            date.Value = new DateTime(2026, 11, 20);
            authorizationStatus = new CentralStatus { Authorization = PrintAuthorization.OriginalTemplate(), ServerDate = "2026-11-20", AuthorizationNotice = "Vigente" };
            UpdateWorkControls(); if (!issueButton.Enabled) throw new Exception("No se habilitó la emisión vigente.");
            date.Value = new DateTime(2027, 3, 10); UpdateWorkControls();
            if (issueButton.Enabled) throw new Exception("Se habilitó una fecha posterior a la caducidad.");
            date.Value = new DateTime(2026, 11, 20); authorizationStatus.ServerDate = "2027-03-10"; UpdateWorkControls();
            if (issueButton.Enabled || authorizationNotice.Text.IndexOf("caducada") < 0) throw new Exception("Falta el bloqueo y aviso de autorización caducada.");
            workflow.Prepare(ReadDraft(), config.ServerId); UpdateWorkControls();
            if (!issueButton.Enabled || issueButton.Text != "Recuperar emisión") throw new Exception("La caducidad bloqueó la recuperación de una solicitud previa.");
        }
    }
}
