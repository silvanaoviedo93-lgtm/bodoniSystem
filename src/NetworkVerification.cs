using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bodoni
{
    static class NetworkVerification
    {
        static readonly List<string> log = new List<string>();
        static void Check(bool value, string message) { if (!value) throw new Exception("FALLÓ: " + message); log.Add("OK - " + message); }
        static void Reject(Action action, string message)
        {
            bool rejected = false; try { action(); } catch { rejected = true; }
            Check(rejected, message);
        }
        static Draft Sample(string name)
        {
            Draft draft = Draft.Sample(); draft.Customer = "SIMULACIÓN DE RED · " + name; draft.Date = "2026-11-12"; return draft;
        }
        public static int Run()
        {
            string output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entregables"); Directory.CreateDirectory(output);
            string temp = Path.Combine(output, "red-test-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(temp);
            string originalRoot = Storage.Root; int result = 0;
            try
            {
                NetworkConfig config = NetworkConfig.Principal(4121, 47831);
                using (CentralLedger diagnostic = new CentralLedger(Path.Combine(temp, "diagnostico", "registro.json"), config))
                {
                    string diagnosticId = Guid.NewGuid().ToString("D"); diagnostic.Issue(diagnosticId, Sample("ANULACIÓN LOCAL"), "TEST"); diagnostic.CancelLast(diagnosticId, "Error de cliente");
                    Check(diagnostic.Find(diagnosticId).Cancelled, "Anulación local conserva la original sin depender del cliente TCP");
                }
                string ledgerPath = Path.Combine(temp, "principal", "registro.json");
                NetworkConfig clientConfig;
                int last;
                using (NumberServer server = new NumberServer(config, ledgerPath, IPAddress.Loopback, 0))
                {
                    clientConfig = config.ForClient("127.0.0.1"); clientConfig.Port = server.Port;
                    NumberClient client = new NumberClient(clientConfig);
                    Check(client.Status().LastNumber == 4121 && client.Status().NextNumber == 4122, "Último confirmado 004121; siguiente 004122 sin consumir al consultar");
                    Reject(delegate { using (CentralLedger duplicate = new CentralLedger(ledgerPath, config)) { } }, "Un segundo proceso no puede abrir el mismo registro central");
                    NetworkConfig badKey = NetworkJson.Copy(clientConfig); badKey.Key = new string('X', 64);
                    Reject(delegate { new NumberClient(badKey).Status(); }, "Una clave incorrecta no permite consultar ni emitir");
                    NetworkConfig wrongServer = NetworkJson.Copy(clientConfig); wrongServer.ServerId = Guid.NewGuid().ToString("D");
                    Reject(delegate { new NumberClient(wrongServer).Status(); }, "No se acepta una respuesta de otro principal");
                    Draft invalid = Sample("INVÁLIDA"); invalid.Customer = "";
                    Reject(delegate { client.Issue(Guid.NewGuid().ToString("D"), invalid); }, "Documento inválido rechazado sin consumir numeración");
                    Check(client.Status().LastNumber == 4121, "Errores de acceso y validación no cambian el contador");

                    ConcurrentBag<IssuedNote> issued = new ConcurrentBag<IssuedNote>();
                    Task[] computers = new Task[3];
                    for (int computer = 0; computer < computers.Length; computer++)
                    {
                        int number = computer + 1;
                        computers[computer] = Task.Factory.StartNew(delegate
                        {
                            NumberClient workstation = new NumberClient(clientConfig);
                            for (int note = 0; note < 10; note++) issued.Add(workstation.Issue(Guid.NewGuid().ToString("D"), Sample("EQUIPO " + number + ", " + note)));
                        });
                    }
                    Task.WaitAll(computers);
                    int[] numbers = issued.Select(delegate(IssuedNote note) { return note.Number; }).OrderBy(delegate(int number) { return number; }).ToArray();
                    Check(numbers.Length == 30 && numbers.Distinct().Count() == 30 && numbers[0] == 4122 && numbers[29] == 4151, "Tres clientes TCP emiten 30 notas concurrentes, únicas y consecutivas 004122–004151");
                    for (int i = 0; i < numbers.Length; i++) Check(numbers[i] == 4122 + i, "Secuencia sin duplicados, posición " + (i + 1));
                    Check(client.Status().Count == 30 && client.Recent().Count == 30, "Historial y contador son compartidos por los tres clientes");

                    string repeatId = Guid.NewGuid().ToString("D"); Draft repeatedDraft = Sample("DOBLE CLIC");
                    Task<IssuedNote>[] repeated = new Task<IssuedNote>[3];
                    for (int i = 0; i < 3; i++) repeated[i] = Task.Factory.StartNew(delegate { return new NumberClient(clientConfig).Issue(repeatId, repeatedDraft); });
                    Task.WaitAll(repeated);
                    Check(repeated.All(delegate(Task<IssuedNote> task) { return task.Result.Number == 4152; }) && client.Status().LastNumber == 4152, "Doble clic y reintentos simultáneos devuelven la misma nota 004152");
                    Draft changed = Sample("CONTENIDO CAMBIADO");
                    Reject(delegate { client.Issue(repeatId, changed); }, "La misma solicitud con otro contenido se rechaza");
                    Check(client.Status().LastNumber == 4152, "Conflicto de solicitud no consume otro número");

                    string workPath = Path.Combine(temp, "cliente", "tramite.json");
                    IssueWorkflow workflow = new IssueWorkflow(workPath, Sample("RESPUESTA PERDIDA"));
                    workflow.Prepare(workflow.State.Draft, config.ServerId);
                    string savedRequest = workflow.State.RequestId;
                    IssuedNote lost = client.Issue(savedRequest, workflow.State.Draft); // Deliberately do not save the response.
                    IssueWorkflow reopened = new IssueWorkflow(workPath, Sample("NO DEBE REEMPLAZAR"));
                    Check(reopened.State.Pending && reopened.State.RequestId == savedRequest, "Reabrir el programa conserva la solicitud cuyo resultado se perdió");
                    reopened.Complete(client.Issue(reopened.State.RequestId, reopened.State.Draft));
                    Check(reopened.State.Issued.Number == lost.Number && client.Status().LastNumber == 4153, "Recuperar la respuesta perdida conserva 004153 sin emitir otra nota");
                    Draft altered = Sample("NO EDITAR EMITIDA"); reopened.Edit(altered);
                    Check(reopened.State.Issued.Note.Customer == lost.Note.Customer, "La nota emitida no se modifica al guardar otro texto como borrador");
                    Reject(delegate { workflow.New(Sample("NUEVA")); }, "No se puede abandonar una solicitud pendiente para crear otra emisión");
                    Reject(delegate { workflow.Prepare(workflow.State.Draft, Guid.NewGuid().ToString("D")); }, "Una emisión pendiente no puede trasladarse a otro principal");

                    using (Bitmap template = OriginalTemplate.Load())
                    {
                        Pdf.SaveIssued(Path.Combine(output, "SIMULACION-RED-nota-numerada.pdf"), reopened.State.Issued, new Profile(), template);
                        string numbered = Encoding.ASCII.GetString(File.ReadAllBytes(Path.Combine(output, "SIMULACION-RED-nota-numerada.pdf")));
                        Check(numbered.Contains("(" + lost.DisplayNumber + ") Tj") && !numbered.Contains("(PRUEBA) Tj"), "El PDF de una nota emitida muestra su número central, no PRUEBA");
                        Reject(delegate { Pdf.SaveIssued(temp, reopened.State.Issued, new Profile(), template); }, "Un fallo al guardar el PDF se comunica sin emitir otra nota");
                        Pdf.SaveIssued(Path.Combine(temp, "reimpresion.pdf"), reopened.State.Issued, new Profile(), null);
                        Check(client.Status().LastNumber == 4153, "Fallo de PDF, regeneración y cambio de modalidad no consumen números");
                    }
                    Check(client.Find(savedRequest).Number == lost.Number && client.Find(Guid.NewGuid().ToString("D")) == null, "Se recupera una nota específica; una inexistente no cambia el contador");
                    last = client.Status().LastNumber;
                }

                IssueWorkflow offline = new IssueWorkflow(Path.Combine(temp, "cliente-offline", "tramite.json"), Sample("SIN RED"));
                offline.Prepare(offline.State.Draft, config.ServerId);
                Reject(delegate { new NumberClient(clientConfig).Issue(offline.State.RequestId, offline.State.Draft); }, "Sin principal no se emite ni se asignan números locales");
                Check(offline.State.Pending && offline.State.Issued == null, "La emisión sin respuesta permanece recuperable");
                NetworkConfig restart = NetworkJson.Copy(config); restart.InitialLast = 1;
                using (NumberServer server = new NumberServer(restart, ledgerPath, IPAddress.Loopback, 0))
                {
                    clientConfig.Port = server.Port; NumberClient client = new NumberClient(clientConfig);
                    Check(client.Status().LastNumber == last, "Reiniciar el principal conserva su contador aunque cambie el valor inicial de configuración");
                    offline.Complete(client.Issue(offline.State.RequestId, offline.State.Draft));
                    Check(offline.State.Issued.Number == last + 1, "Al reconectar se completa la solicitud guardada con el siguiente número central");
                    client.Issue(offline.State.RequestId, offline.State.Draft);
                    Check(client.Status().LastNumber == last + 1, "Reintentar después de reconectar sigue sin duplicar numeración");

                    Storage.Root = Path.Combine(temp, "interfaz"); Directory.CreateDirectory(Storage.Root);
                    using (MainForm form = new MainForm()) form.VerifyIssuedUi(offline.State.Issued, temp);
                    Check(true, "Interfaz bloquea la edición de una nota emitida y genera PDF con su mismo número");
                    Storage.Root = originalRoot;
                }

                string replacementPath = Path.Combine(temp, "reemplazos", "registro.json");
                string originalRequest = Guid.NewGuid().ToString("D");
                string replacementRequest = Guid.NewGuid().ToString("D");
                Draft originalDraft = Sample("ORIGINAL ANULADA");
                using (NumberServer server = new NumberServer(config, replacementPath, IPAddress.Loopback, 0))
                {
                    NetworkConfig replacementConfig = config.ForClient("127.0.0.1"); replacementConfig.Port = server.Port;
                    NumberClient client = new NumberClient(replacementConfig);
                    client.Issue(originalRequest, originalDraft);
                    Reject(delegate { client.CancelLast(originalRequest, ""); }, "Anulación exige un motivo y no modifica la secuencia");
                    IssuedNote cancelled = client.CancelLast(originalRequest, "Error de cliente");
                    Check(cancelled.Cancelled && cancelled.Number == 4122 && cancelled.CancellationReason == "Error de cliente", "Anulación conserva nota, motivo y número");
                    Check(client.Status().LastNumber == 4122 && client.Status().NextNumber == 4122, "Última numeración y número disponible reflejan el reemplazo pendiente");
                    Check(client.CancelLast(originalRequest, "Reintento de anulación").CancelledUtc == cancelled.CancelledUtc, "Reintentar anulación conserva el registro original");
                    Check(client.Issue(originalRequest, originalDraft).Cancelled, "Reintentar la emisión original devuelve la anulada, sin resucitarla");
                    Reject(delegate { Pdf.SaveIssued(Path.Combine(temp, "anulada.pdf"), cancelled, new Profile(), null); }, "PDF válido bloqueado para una nota anulada");
                    IssuedNote replacement = client.Issue(replacementRequest, Sample("REEMPLAZO"));
                    Check(replacement.Number == 4122 && !replacement.Cancelled, "Nueva solicitud reemplaza solo el último número anulado");
                    Check(client.Find(originalRequest).Cancelled && client.Find(replacementRequest).Note.Customer != originalDraft.Customer, "Original anulada y reemplazo permanecen consultables por separado");
                    Check(client.Recent().Count == 2 && client.Recent().Exists(delegate(NoteSummary note) { return note.State.StartsWith("ANULADA"); }), "Historial muestra ambas versiones y motivo de anulación");
                    Check(client.Issue(replacementRequest, Sample("REEMPLAZO")).Number == 4122 && client.Status().NextNumber == 4123, "Reintentar reemplazo no duplica ni consume otro número");
                    client.Issue(Guid.NewGuid().ToString("D"), Sample("POSTERIOR"));
                    Reject(delegate { client.CancelLast(replacementRequest, "Nota anterior"); }, "No se puede anular para reutilizar una nota que tiene otra posterior");
                    Check(client.CancelLast(originalRequest, "Reintento antiguo").Cancelled && client.Status().NextNumber == 4124, "Reintentar anulación antigua no libera el número de un reemplazo vigente");
                }
                using (CentralLedger reopened = new CentralLedger(replacementPath, config))
                {
                    Check(reopened.Find(originalRequest).Cancelled && reopened.Find(replacementRequest).Number == 4122 && reopened.Status().NextNumber == 4124, "Reinicio conserva historial de reemplazos y secuencia");
                    IssuedNote latest = reopened.Find(reopened.Recent()[0].RequestId);
                    reopened.BeforeCommit = delegate { throw new IOException("Fallo al guardar anulación"); };
                    Reject(delegate { reopened.CancelLast(latest.RequestId, "Fallo de guardado"); }, "Fallo durable de anulación no confirma el cambio");
                    Check(!reopened.Find(latest.RequestId).Cancelled && reopened.Status().NextNumber == 4124, "Fallo de anulación conserva nota vigente y contador");
                    reopened.BeforeCommit = null; reopened.CancelLast(latest.RequestId, "Corregir última");
                }
                using (CentralLedger reopened = new CentralLedger(replacementPath, config))
                {
                    Check(reopened.Status().NextNumber == 4123, "Reinicio con anulación pendiente conserva el número a reemplazar");
                    ConcurrentBag<IssuedNote> replacements = new ConcurrentBag<IssuedNote>();
                    Parallel.For(0, 2, delegate(int index) { replacements.Add(reopened.Issue(Guid.NewGuid().ToString("D"), Sample("CONCURRENTE " + index), "TEST")); });
                    Check(replacements.Select(delegate(IssuedNote note) { return note.Number; }).OrderBy(delegate(int number) { return number; }).SequenceEqual(new int[] { 4123, 4124 }), "Dos reemplazos concurrentes no asignan el mismo número vigente");
                }
                Storage.Root = Path.Combine(temp, "practica");
                Program.PreparePractice(); NetworkConfig practice = NetworkJson.Read<NetworkConfig>(NetworkConfig.ConfigPath);
                Check(practice.Role == "Principal" && practice.InitialLast == 4121 && practice.Port == 47832, "Práctica configura principal local con último 004121 y puerto separado");
                Program.PreparePractice(); Check(NetworkJson.Read<NetworkConfig>(NetworkConfig.ConfigPath).ServerId == practice.ServerId, "Reabrir práctica conserva la identidad del principal");
                Program.PracticeMode = true;
                try { Check(Layout.Build(Sample("PDF PRÁCTICA"), new Profile(), true, "004122").Exists(delegate(TextItem item) { return item.Text == "PRUEBA 004122"; }), "Vista previa y PDF de práctica identifican el número como PRUEBA"); }
                finally { Program.PracticeMode = false; }
                Storage.Root = originalRoot;

                string failurePath = Path.Combine(temp, "fallo-escritura", "registro.json");
                using (CentralLedger ledger = new CentralLedger(failurePath, config))
                {
                    ledger.BeforeCommit = delegate { throw new IOException("Fallo de escritura simulado"); };
                    Reject(delegate { ledger.Issue(Guid.NewGuid().ToString("D"), Sample("FALLO"), "TEST"); }, "Fallo de guardado central no confirma la emisión");
                    Check(ledger.Status().LastNumber == 4121 && ledger.Status().Count == 0, "Un fallo de escritura no adelanta el contador en memoria ni guarda una nota parcial");
                    ledger.BeforeCommit = null;
                    Check(ledger.Issue(Guid.NewGuid().ToString("D"), Sample("GUARDADO"), "TEST").Number == 4122, "Después del fallo el siguiente número sigue siendo 004122");
                }
                File.WriteAllText(failurePath, "registro dañado a propósito", Encoding.UTF8);
                Reject(delegate { using (CentralLedger broken = new CentralLedger(failurePath, config)) { } }, "Un registro dañado bloquea la emisión y nunca reinicia el contador automáticamente");
                Check(File.ReadAllText(failurePath, Encoding.UTF8) == "registro dañado a propósito", "El registro dañado se conserva para recuperación");
                File.Delete(failurePath);
                Reject(delegate { using (CentralLedger deleted = new CentralLedger(failurePath, config)) { } }, "Si se borra el registro, su marca de inicialización impide reiniciar el contador");
                NetworkConfig initialized = NetworkJson.Copy(config); initialized.Initialized = true;
                Reject(delegate { using (CentralLedger missing = new CentralLedger(Path.Combine(temp, "sin-datos", "registro.json"), initialized)) { } }, "La configuración de un principal existente también impide arrancar con datos vacíos");
                log.Add("ALCANCE: pruebas en TCP local con tres clientes; instalación y prueba entre computadoras físicas pendientes.");
            }
            catch (Exception ex) { result = 1; log.Add("ERROR - " + ex); }
            finally
            {
                Storage.Root = originalRoot;
                // Guard the absolute target before removing this test's tree.
                string allowed = Path.GetFullPath(output) + Path.DirectorySeparatorChar;
                if (Path.GetFullPath(temp).StartsWith(allowed, StringComparison.OrdinalIgnoreCase) && Path.GetFileName(temp).StartsWith("red-test-", StringComparison.Ordinal)) DeleteTestTree(temp);
                log.Add(result == 0 ? "RESULTADO: TODAS LAS COMPROBACIONES DE RED CORRECTAS" : "RESULTADO: REVISAR ERRORES DE RED");
                File.WriteAllLines(Path.Combine(output, "VERIFICACION_RED.txt"), log, new UTF8Encoding(false));
            }
            return result;
        }
        static void DeleteTestTree(string directory)
        {
            foreach (string file in Directory.GetFiles(directory)) File.Delete(file);
            foreach (string child in Directory.GetDirectories(directory)) DeleteTestTree(child);
            Directory.Delete(directory, false);
        }
    }
}
