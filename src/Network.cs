using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace Bodoni
{
    public static class NetworkJson
    {
        public static string Serialize(object value) { return new JavaScriptSerializer { MaxJsonLength = 64 * 1024 * 1024 }.Serialize(value); }
        public static T Parse<T>(string value) { return new JavaScriptSerializer { MaxJsonLength = 64 * 1024 * 1024 }.Deserialize<T>(value); }
        public static T Copy<T>(T value) { return Parse<T>(Serialize(value)); }
        public static string Hash(string value)
        {
            using (SHA256 hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "");
        }
        public static void Save(string path, object value) { Storage.WriteBytes(path, new UTF8Encoding(false).GetBytes(Serialize(value))); }
        public static T Read<T>(string path) { return Parse<T>(File.ReadAllText(path, Encoding.UTF8)); }
    }

    public class NetworkConfig
    {
        public string Role { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Key { get; set; }
        public string ServerId { get; set; }
        public int InitialLast { get; set; }
        public bool Initialized { get; set; }
        public NetworkConfig() { Role = "Cliente"; Host = ""; Port = 47831; InitialLast = 4121; }
        public void Validate()
        {
            Guid id;
            if (Role != "Principal" && Role != "Cliente") throw new ArgumentException("Selecciona Principal o Cliente.");
            if (Port < 1024 || Port > 65535) throw new ArgumentException("El puerto debe estar entre 1024 y 65535.");
            if (String.IsNullOrWhiteSpace(Key) || Key.Length < 32 || Key.Length > 128) throw new ArgumentException("Importa la conexión del principal o utiliza su clave completa.");
            if (!Guid.TryParse(ServerId, out id)) throw new ArgumentException("La conexión no tiene una identificación válida del principal.");
            if (Role == "Cliente" && (String.IsNullOrWhiteSpace(Host) || Host.Length > 255)) throw new ArgumentException("Indica el nombre o la dirección IP del principal.");
            if (InitialLast < 0 || InitialLast > 999998) throw new ArgumentException("El último número debe estar entre cero y 999998.");
        }
        public static NetworkConfig Principal(int last, int port)
        {
            byte[] bytes = new byte[32]; using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            return new NetworkConfig { Role = "Principal", Host = "127.0.0.1", Port = port, InitialLast = last, Key = BitConverter.ToString(bytes).Replace("-", ""), ServerId = Guid.NewGuid().ToString("D") };
        }
        public NetworkConfig ForClient(string host)
        {
            NetworkConfig config = NetworkJson.Copy(this); config.Role = "Cliente"; config.Host = host.Trim(); return config;
        }
        public static string ConfigPath { get { return Path.Combine(Storage.Root, "conexion-red.json"); } }
        public static string LedgerPath { get { return Path.Combine(Storage.Root, "principal", "registro-central.json"); } }
    }

    public static class NoteRules
    {
        static string Text(string value, string label)
        {
            string text = (value ?? "").Trim();
            if (text.Length > 300) throw new ArgumentException(label + ": máximo 300 caracteres.");
            Fonts.Body.Measure(text, 8); return text;
        }
        public static Draft Normalize(Draft draft)
        {
            if (draft == null || draft.Version != 1 || draft.Lines == null || draft.Lines.Count > 10) throw new ArgumentException("El documento no es válido o tiene más de diez filas.");
            DateTime date;
            if (!DateTime.TryParseExact(draft.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) throw new ArgumentException("La fecha de la nota no es válida.");
            Draft normalized = new Draft { Customer = Text(draft.Customer, "Cliente"), Identification = Text(draft.Identification, "Identificación"), Address = Text(draft.Address, "Dirección"), Phone = Text(draft.Phone, "Teléfono"), Email = Text(draft.Email, "Correo"), Date = draft.Date };
            if (normalized.Customer.Length == 0) throw new ArgumentException("Ingresa el nombre del cliente antes de emitir.");
            foreach (CalculatedLine line in Calculation.Lines(draft)) normalized.Lines.Add(new DraftLine { Quantity = line.Quantity.ToString("0.###", CultureInfo.InvariantCulture), UnitPrice = line.Price.ToString("0.00##", CultureInfo.InvariantCulture), Description = Text(line.Description, "Descripción") });
            return normalized;
        }
        public static string Fingerprint(Draft draft) { return NetworkJson.Hash(NetworkJson.Serialize(Normalize(draft))); }
    }

    public class IssuedNote
    {
        public PrintAuthorization Authorization { get; set; }
        public string RequestId { get; set; }
        public string ServerId { get; set; }
        public string Series { get; set; }
        public int Number { get; set; }
        public string IssuedUtc { get; set; }
        public string Computer { get; set; }
        public string Fingerprint { get; set; }
        public Draft Note { get; set; }
        public decimal Total { get; set; }
        public bool Cancelled { get; set; }
        public string CancellationReason { get; set; }
        public string CancelledUtc { get; set; }
        public string DisplayNumber { get { return Number.ToString("D6", CultureInfo.InvariantCulture); } }
    }
    public class NoteSummary
    {
        public string State { get; set; }
        public string RequestId { get; set; }
        public string Number { get; set; }
        public string Customer { get; set; }
        public string Date { get; set; }
        public decimal Total { get; set; }
        public string Computer { get; set; }
    }
    public class CentralState
    {
        public PrintAuthorization Authorization { get; set; }
        public List<PrintAuthorization> AuthorizationHistory { get; set; }
        public int Version { get; set; }
        public string ServerId { get; set; }
        public string Series { get; set; }
        public int InitialLast { get; set; }
        public int LastNumber { get; set; }
        public List<IssuedNote> Notes { get; set; }
        public List<IssuedNote> CancelledHistory { get; set; }
    }
    public class LedgerEnvelope
    {
        public string Payload { get; set; }
        public string Checksum { get; set; }
    }
    public class CentralStatus
    {
        public PrintAuthorization Authorization { get; set; }
        public string ServerDate { get; set; }
        public string AuthorizationNotice { get; set; }
        public string ServerId { get; set; }
        public string Series { get; set; }
        public int LastNumber { get; set; }
        public int NextNumber { get; set; }
        public int Count { get; set; }
    }

    public sealed class CentralLedger : IDisposable
    {
        readonly object gate = new object();
        readonly string path;
        FileStream owner;
        CentralState state;
        bool disposed;
        internal Action BeforeCommit;
        public CentralLedger(string file, NetworkConfig config)
        {
            config.Validate(); path = Path.GetFullPath(file);
            if (path.StartsWith(@"\\", StringComparison.Ordinal)) throw new ArgumentException("El registro central debe estar en un disco local del principal, no en una carpeta compartida.");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            try
            {
                owner = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                string marker = path + ".inicializado";
                if (File.Exists(path))
                {
                    LedgerEnvelope envelope = NetworkJson.Read<LedgerEnvelope>(path);
                    if (envelope == null || String.IsNullOrEmpty(envelope.Payload) || envelope.Checksum != NetworkJson.Hash(envelope.Payload)) throw new InvalidDataException("El registro central está dañado. No se reiniciará el contador: recupera el registro antes de emitir.");
                    state = NetworkJson.Parse<CentralState>(envelope.Payload); ValidateState(config.ServerId);
                    if (state.Version < 3)
                    {
                        CentralState upgraded = NetworkJson.Copy(state); upgraded.Version = 3;
                        if (state.Version < 3) { upgraded.Authorization = PrintAuthorization.OriginalTemplate(); upgraded.AuthorizationHistory = new List<PrintAuthorization>(); }
                        Commit(upgraded); state = upgraded;
                    }
                }
                else
                {
                    if (config.Initialized || File.Exists(marker)) throw new InvalidDataException("Falta el registro de un principal que ya fue inicializado. No se reiniciará el consecutivo: recupera sus datos antes de emitir.");
                    state = new CentralState { Version = 3, Authorization = PrintAuthorization.OriginalTemplate(), AuthorizationHistory = new List<PrintAuthorization>(), ServerId = config.ServerId, Series = "001-001-000", InitialLast = config.InitialLast, LastNumber = config.InitialLast, Notes = new List<IssuedNote>() };
                    Commit(state);
                }
                if (!File.Exists(marker)) Storage.WriteBytes(marker, Encoding.ASCII.GetBytes(config.ServerId));
            }
            catch { if (owner != null) owner.Dispose(); throw; }
        }
        void ValidateState(string expectedId)
        {
            if (state == null || state.Version < 1 || state.Version > 3 || state.ServerId != expectedId || state.Series != "001-001-000" || state.Notes == null || state.InitialLast < 0 || state.LastNumber > 999999 || state.LastNumber != state.InitialLast + state.Notes.Count)
                throw new InvalidDataException("El registro central no coincide con esta conexión o su secuencia no es válida. No se reiniciará.");
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < state.Notes.Count; i++)
            {
                IssuedNote note = state.Notes[i]; Guid id;
                if (note == null || !Guid.TryParse(note.RequestId, out id) || !ids.Add(note.RequestId) || note.ServerId != state.ServerId || note.Series != state.Series || note.Number != state.InitialLast + i + 1 || note.Fingerprint != NoteRules.Fingerprint(note.Note) || note.Total != Calculation.Total(Calculation.Lines(note.Note)))
                    throw new InvalidDataException("El registro contiene una nota incoherente. No se reiniciará el contador.");
                if (note.Cancelled && (i != state.Notes.Count - 1 || String.IsNullOrWhiteSpace(note.CancellationReason) || String.IsNullOrWhiteSpace(note.CancelledUtc))) throw new InvalidDataException("La anulación del registro no es válida.");
            }
            if (state.CancelledHistory == null) state.CancelledHistory = new List<IssuedNote>();
            foreach (IssuedNote note in state.CancelledHistory)
                if (note == null || !note.Cancelled || String.IsNullOrWhiteSpace(note.CancellationReason) || String.IsNullOrWhiteSpace(note.CancelledUtc) || !ids.Add(note.RequestId) || note.ServerId != state.ServerId || note.Series != state.Series || note.Number <= state.InitialLast || note.Number > state.LastNumber || note.Fingerprint != NoteRules.Fingerprint(note.Note) || note.Total != Calculation.Total(Calculation.Lines(note.Note))) throw new InvalidDataException("El historial de anulaciones no es válido.");
            if (state.Version >= 3)
            {
                if (state.Authorization == null || state.AuthorizationHistory == null) throw new InvalidDataException("Faltan los datos de autorización del registro. No se reemplazarán por valores iniciales.");
                state.Authorization.Validate();
                foreach (PrintAuthorization authorization in state.AuthorizationHistory) { if (authorization == null) throw new InvalidDataException("El historial de autorizaciones está dañado."); authorization.Validate(); }
            }
        }
        void Commit(CentralState candidate)
        {
            if (BeforeCommit != null) BeforeCommit();
            string payload = NetworkJson.Serialize(candidate);
            NetworkJson.Save(path, new LedgerEnvelope { Payload = payload, Checksum = NetworkJson.Hash(payload) });
        }
        void CheckOpen() { if (disposed) throw new ObjectDisposedException("Registro central"); }
        public CentralStatus Status()
        {
            lock (gate) { CheckOpen(); DateTime today = AuthorizationRules.CurrentDate().Date; int next = ReusableLast ? state.LastNumber : state.LastNumber + 1; return new CentralStatus { Authorization = NetworkJson.Copy(state.Authorization), ServerDate = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), AuthorizationNotice = AuthorizationRules.Notice(state.Authorization, today), ServerId = state.ServerId, Series = state.Series, LastNumber = state.LastNumber, NextNumber = next, Count = state.Notes.Count }; }
        }
        public List<PrintAuthorization> GetAuthorizationHistory()
        {
            lock (gate) { CheckOpen(); return NetworkJson.Copy(state.AuthorizationHistory); }
        }
        public PrintAuthorization UpdateAuthorization(PrintAuthorization value)
        {
            if (value == null) throw new ArgumentException("Falta el pie de imprenta actualizado.");
            value = NetworkJson.Copy(value); value.Validate();
            lock (gate)
            {
                CheckOpen();
                if (state.Authorization.SameContent(value)) return NetworkJson.Copy(state.Authorization);
                if (state.Authorization.RevisionId != value.RevisionId) throw new ArgumentException("La autorización cambió en otra ventana. Vuelve a abrir Pie de imprenta antes de guardar.");
                CentralState candidate = NetworkJson.Copy(state); candidate.Version = 3;
                candidate.AuthorizationHistory.Add(NetworkJson.Copy(state.Authorization));
                value.RevisionId = Guid.NewGuid().ToString("D"); value.SavedUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture); candidate.Authorization = value;
                Commit(candidate); state = candidate; return NetworkJson.Copy(value);
            }
        }
        bool ReusableLast { get { return state.Notes.Count > 0 && state.Notes[state.Notes.Count - 1].Cancelled; } }
        IssuedNote Lookup(string requestId)
        {
            IssuedNote note = state.Notes.Find(delegate(IssuedNote item) { return item.RequestId == requestId; });
            return note ?? (state.CancelledHistory == null ? null : state.CancelledHistory.Find(delegate(IssuedNote item) { return item.RequestId == requestId; }));
        }
        public IssuedNote CancelLast(string requestId, string reason)
        {
            reason = (reason ?? "").Trim();
            if (reason.Length < 3 || reason.Length > 300) throw new ArgumentException("Indica un motivo de anulación de 3 a 300 caracteres.");
            lock (gate)
            {
                CheckOpen(); IssuedNote original = Lookup(requestId);
                if (original == null) throw new ArgumentException("No se encontró la nota.");
                if (original.Cancelled) return NetworkJson.Copy(original);
                if (state.Notes.Count == 0 || state.Notes[state.Notes.Count - 1].RequestId != requestId) throw new ArgumentException("Solo se puede anular para reemplazar la última nota. Ya existe una nota posterior.");
                CentralState candidate = NetworkJson.Copy(state); candidate.Version = 3;
                IssuedNote cancelled = candidate.Notes[candidate.Notes.Count - 1];
                cancelled.Cancelled = true; cancelled.CancellationReason = reason; cancelled.CancelledUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                Commit(candidate); state = candidate; return NetworkJson.Copy(cancelled);
            }
        }
        public IssuedNote Find(string requestId)
        {
            lock (gate)
            {
                CheckOpen(); IssuedNote found = Lookup(requestId);
                return found == null ? null : NetworkJson.Copy(found);
            }
        }
        public IssuedNote Issue(string requestId, Draft draft, string computer)
        {
            Guid id; if (!Guid.TryParse(requestId, out id)) throw new ArgumentException("La solicitud de emisión no tiene una identificación válida.");
            Draft normalized = NoteRules.Normalize(draft); string fingerprint = NoteRules.Fingerprint(normalized);
            lock (gate)
            {
                CheckOpen();
                IssuedNote existing = Lookup(requestId);
                if (existing != null)
                {
                    if (existing.Fingerprint != fingerprint) throw new ArgumentException("La solicitud ya corresponde a otra nota. Recupera su emisión original.");
                    return NetworkJson.Copy(existing);
                }
                string authorizationBlock = AuthorizationRules.BlockReason(state.Authorization, AuthorizationRules.CurrentDate(), normalized.Date);
                if (authorizationBlock != null) throw new AuthorizationException(authorizationBlock);
                if (state.LastNumber >= 999999 && !ReusableLast) throw new ArgumentException("Se agotó el formato de seis dígitos. No se reiniciará la numeración.");
                IssuedNote issued = new IssuedNote { RequestId = requestId, ServerId = state.ServerId, Series = state.Series, Number = ReusableLast ? state.LastNumber : state.LastNumber + 1, IssuedUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), Computer = (computer ?? "").Substring(0, Math.Min(80, (computer ?? "").Length)), Fingerprint = fingerprint, Note = normalized, Total = Calculation.Total(Calculation.Lines(normalized)) };
                issued.Authorization = NetworkJson.Copy(state.Authorization);
                CentralState candidate = NetworkJson.Copy(state); candidate.Version = 3; candidate.LastNumber = issued.Number;
                if (candidate.CancelledHistory == null) candidate.CancelledHistory = new List<IssuedNote>();
                if (ReusableLast) { candidate.CancelledHistory.Add(candidate.Notes[candidate.Notes.Count - 1]); candidate.Notes.RemoveAt(candidate.Notes.Count - 1); }
                candidate.Notes.Add(issued);
                // Document and counter are one durable atomic replacement. A
                // failed write must not advance the in-memory counter either.
                Commit(candidate); state = candidate; return NetworkJson.Copy(issued);
            }
        }
        public List<NoteSummary> Recent()
        {
            lock (gate)
            {
                CheckOpen(); List<NoteSummary> list = new List<NoteSummary>();
                List<IssuedNote> history = new List<IssuedNote>(state.Notes);
                if (state.CancelledHistory != null) history.AddRange(state.CancelledHistory);
                history.Sort(delegate(IssuedNote a, IssuedNote b) { int order = b.Number.CompareTo(a.Number); return order != 0 ? order : String.CompareOrdinal(b.IssuedUtc, a.IssuedUtc); });
                for (int i = 0; i < history.Count && list.Count < 200; i++)
                {
                    IssuedNote note = history[i]; list.Add(new NoteSummary { State = note.Cancelled ? "ANULADA: " + note.CancellationReason : "Emitida", RequestId = note.RequestId, Number = note.DisplayNumber, Customer = note.Note.Customer, Date = note.Note.Date, Total = note.Total, Computer = note.Computer });
                }
                return list;
            }
        }
        public void Dispose() { lock (gate) { disposed = true; if (owner != null) { owner.Dispose(); owner = null; } } }
    }

    public class NetworkRequest
    {
        public PrintAuthorization Authorization { get; set; }
        public string Reason { get; set; }
        public int Version { get; set; }
        public string Key { get; set; }
        public string ServerId { get; set; }
        public string Operation { get; set; }
        public string RequestId { get; set; }
        public string Computer { get; set; }
        public Draft Note { get; set; }
    }
    public class NetworkReply
    {
        public bool AuthorizationRejected { get; set; }
        public List<PrintAuthorization> AuthorizationHistory { get; set; }
        public bool Ok { get; set; }
        public string Error { get; set; }
        public string ServerId { get; set; }
        public CentralStatus Status { get; set; }
        public IssuedNote Issued { get; set; }
        public List<NoteSummary> Notes { get; set; }
    }
    static class NetworkFrame
    {
        const int MaxSize = 2 * 1024 * 1024;
        static void ReadExactly(Stream stream, byte[] buffer)
        {
            int at = 0;
            while (at < buffer.Length) { int read = stream.Read(buffer, at, buffer.Length - at); if (read == 0) throw new IOException("La conexión se cerró antes de recibir la respuesta completa."); at += read; }
        }
        public static T Read<T>(NetworkStream stream)
        {
            byte[] header = new byte[4]; ReadExactly(stream, header);
            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(header, 0));
            if (length <= 0 || length > MaxSize) throw new InvalidDataException("El mensaje de red excede el tamaño permitido.");
            byte[] bytes = new byte[length]; ReadExactly(stream, bytes);
            return NetworkJson.Parse<T>(new UTF8Encoding(false, true).GetString(bytes));
        }
        public static void Write(NetworkStream stream, object value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(NetworkJson.Serialize(value));
            if (bytes.Length > MaxSize) throw new InvalidDataException("El mensaje de red excede el tamaño permitido.");
            byte[] header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(bytes.Length));
            stream.Write(header, 0, header.Length); stream.Write(bytes, 0, bytes.Length); stream.Flush();
        }
    }

    public sealed class NumberServer : IDisposable
    {
        internal static Exception LastUnexpectedFailure;
        readonly NetworkConfig config;
        readonly CentralLedger ledger;
        readonly TcpListener listener;
        readonly Thread acceptThread;
        readonly Semaphore slots = new Semaphore(12, 12);
        volatile bool stopping;
        public int Port { get { return ((IPEndPoint)listener.LocalEndpoint).Port; } }
        public NumberServer(NetworkConfig configuration, string ledgerPath, IPAddress address, int port)
        {
            config = NetworkJson.Copy(configuration); ledger = new CentralLedger(ledgerPath, config);
            listener = new TcpListener(address, port);
            try { listener.Start(16); }
            catch { ledger.Dispose(); throw; }
            acceptThread = new Thread(Accept) { IsBackground = true, Name = "Bodoni principal" }; acceptThread.Start();
        }
        void Accept()
        {
            while (!stopping)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    if (!slots.WaitOne(0)) { client.Close(); continue; }
                    ThreadPool.QueueUserWorkItem(delegate { try { Handle(client); } finally { slots.Release(); } });
                }
                catch (SocketException) { if (stopping) break; }
                catch (ObjectDisposedException) { break; }
            }
        }
        static bool SameKey(string expected, string supplied)
        {
            if (supplied == null || expected.Length != supplied.Length) return false;
            int different = 0; for (int i = 0; i < expected.Length; i++) different |= expected[i] ^ supplied[i]; return different == 0;
        }
        void Handle(TcpClient client)
        {
            using (client)
            {
                try
                {
                    client.ReceiveTimeout = 5000; client.SendTimeout = 5000;
                    using (NetworkStream stream = client.GetStream())
                    {
                        NetworkRequest request = NetworkFrame.Read<NetworkRequest>(stream);
                        NetworkReply reply = new NetworkReply { ServerId = config.ServerId };
                        try
                        {
                            if (request == null || request.Version != 3 || !SameKey(config.Key, request.Key) || request.ServerId != config.ServerId) throw new ArgumentException("Revisa la conexión y utiliza la misma versión actualizada de Bodoni en todos los equipos.");
                            switch (request.Operation)
                            {
                                case "status": reply.Status = ledger.Status(); break;
                                case "issue": reply.Issued = ledger.Issue(request.RequestId, request.Note, request.Computer); reply.Status = ledger.Status(); break;
                                case "find": reply.Issued = ledger.Find(request.RequestId); break;
                                case "recent": reply.Notes = ledger.Recent(); break;
                                case "cancel-last": reply.Issued = ledger.CancelLast(request.RequestId, request.Reason); reply.Status = ledger.Status(); break;
                                case "authorization-history": reply.AuthorizationHistory = ledger.GetAuthorizationHistory(); break;
                                case "set-authorization":
                                    if (!IPAddress.IsLoopback(((IPEndPoint)client.Client.RemoteEndPoint).Address)) throw new ArgumentException("Actualiza el pie de imprenta desde el computador principal.");
                                    ledger.UpdateAuthorization(request.Authorization); reply.Status = ledger.Status(); break;
                                default: throw new ArgumentException("La operación solicitada no existe.");
                            }
                            reply.Ok = true;
                        }
                        catch (Exception ex) { if (!(ex is ArgumentException)) LastUnexpectedFailure = ex; reply.AuthorizationRejected = ex is AuthorizationException; reply.Error = ex is ArgumentException ? ex.Message : "El principal no pudo guardar o leer el registro. No se generará otro número por este intento."; }
                        NetworkFrame.Write(stream, reply);
                    }
                }
                catch (Exception) { /* The client preserves its request ID and can recover it after a disconnect. */ }
            }
        }
        public void Dispose() { stopping = true; listener.Stop(); if (Thread.CurrentThread != acceptThread) acceptThread.Join(2000); ledger.Dispose(); }
    }

    public class NumberClient
    {
        readonly NetworkConfig config;
        public NumberClient(NetworkConfig configuration) { config = NetworkJson.Copy(configuration); config.Validate(); }
        public NetworkReply Call(string operation, string requestId, Draft note, string reason = null, PrintAuthorization authorization = null)
        {
            using (TcpClient client = new TcpClient())
            {
                IAsyncResult connection = client.BeginConnect(config.Role == "Principal" ? "127.0.0.1" : config.Host, config.Port, null, null);
                using (WaitHandle wait = connection.AsyncWaitHandle)
                { if (!wait.WaitOne(3000)) throw new IOException("No se pudo conectar con el principal. Comprueba que esté encendido y conectado a la red."); client.EndConnect(connection); }
                client.ReceiveTimeout = 6000; client.SendTimeout = 6000;
                using (NetworkStream stream = client.GetStream())
                {
                    NetworkFrame.Write(stream, new NetworkRequest { Version = 3, Key = config.Key, ServerId = config.ServerId, Operation = operation, RequestId = requestId, Note = note, Reason = reason, Authorization = authorization, Computer = Environment.MachineName });
                    NetworkReply reply = NetworkFrame.Read<NetworkReply>(stream);
                    if (reply == null || reply.ServerId != config.ServerId) throw new IOException("La respuesta no corresponde al principal configurado.");
                    if (!reply.Ok) { if (operation == "issue" && reply.AuthorizationRejected) throw new AuthorizationException(reply.Error); throw new InvalidOperationException(reply.Error ?? "El principal rechazó la solicitud."); }
                    return reply;
                }
            }
        }
        public CentralStatus Status() { return Call("status", null, null).Status; }
        public CentralStatus UpdateAuthorization(PrintAuthorization value) { return Call("set-authorization", null, null, null, value).Status; }
        public List<PrintAuthorization> AuthorizationHistory() { return Call("authorization-history", null, null).AuthorizationHistory; }
        public IssuedNote CancelLast(string requestId, string reason) { return Call("cancel-last", requestId, null, reason).Issued; }
        public IssuedNote Issue(string requestId, Draft note) { return Call("issue", requestId, note).Issued; }
        public IssuedNote Find(string requestId) { return Call("find", requestId, null).Issued; }
        public List<NoteSummary> Recent() { return Call("recent", null, null).Notes; }
    }
}
