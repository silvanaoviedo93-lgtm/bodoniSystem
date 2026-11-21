using System;
using System.IO;

namespace Bodoni
{
    public class WorkState
    {
        public int Version { get; set; }
        public Draft Draft { get; set; }
        public string RequestId { get; set; }
        public string ServerId { get; set; }
        public bool Pending { get; set; }
        public IssuedNote Issued { get; set; }
        public WorkState() { Version = 1; }
    }
    public sealed class IssueWorkflow
    {
        readonly string path;
        public WorkState State { get; private set; }
        public bool Locked { get { return State.Pending || State.Issued != null; } }
        public string Number { get { return State.Issued == null ? "PRUEBA" : State.Issued.DisplayNumber; } }
        public IssueWorkflow(string file, Draft fallback)
        {
            path = file;
            if (File.Exists(path))
            {
                State = NetworkJson.Read<WorkState>(path);
                if (State == null || State.Version != 1 || State.Draft == null || State.Draft.Version != 1 || State.Draft.Lines == null || State.Draft.Lines.Count > 10) throw new InvalidDataException("No se pudo recuperar el trámite actual. Se conserva el archivo para no duplicar una emisión pendiente.");
                if (State.Pending || State.Issued != null)
                {
                    Guid request, server;
                    if (!Guid.TryParse(State.RequestId, out request) || !Guid.TryParse(State.ServerId, out server)) throw new InvalidDataException("El trámite de emisión no tiene identificadores válidos.");
                    if (State.Issued != null) ValidateReceipt(State.Issued);
                }
            }
            else State = new WorkState { Draft = NetworkJson.Copy(fallback) };
        }
        void Persist(WorkState candidate) { NetworkJson.Save(path, candidate); State = candidate; }
        public void Edit(Draft draft)
        {
            if (Locked) return;
            WorkState candidate = NetworkJson.Copy(State); candidate.Draft = NetworkJson.Copy(draft); Persist(candidate);
        }
        public void New(Draft draft)
        {
            if (State.Pending) throw new InvalidOperationException("Primero recupera la emisión pendiente. Se conservará su solicitud para no duplicar números.");
            Persist(new WorkState { Draft = NetworkJson.Copy(draft) });
        }
        public void Prepare(Draft draft, string serverId)
        {
            if (State.Issued != null) return;
            if (State.Pending)
            {
                if (State.ServerId != serverId) throw new InvalidOperationException("La emisión pendiente pertenece a otro principal. Recupera la conexión original.");
                return;
            }
            Draft normalized = NoteRules.Normalize(draft);
            Persist(new WorkState { Draft = normalized, RequestId = Guid.NewGuid().ToString("D"), ServerId = serverId, Pending = true });
        }
        // Only after an authenticated, definitive authorization rejection.
        // The principal checks for an existing receipt before it can reject.
        internal void ReleaseRejectedAuthorization()
        {
            if (!State.Pending || State.Issued != null) return;
            Persist(new WorkState { Draft = NetworkJson.Copy(State.Draft) });
        }
        void ValidateReceipt(IssuedNote receipt)
        {
            if (receipt.RequestId != State.RequestId || receipt.ServerId != State.ServerId || receipt.Series != "001-001-000" || receipt.Number < 1 || receipt.Number > 999999 || receipt.Fingerprint != NoteRules.Fingerprint(State.Draft) || receipt.Fingerprint != NoteRules.Fingerprint(receipt.Note) || receipt.Total != Calculation.Total(Calculation.Lines(receipt.Note)))
                throw new InvalidDataException("La nota recibida no coincide con la solicitud pendiente. Se mantendrá para su recuperación.");
        }
        public void Complete(IssuedNote receipt)
        {
            if (receipt == null) throw new InvalidDataException("El principal no devolvió la nota emitida.");
            ValidateReceipt(receipt);
            WorkState candidate = NetworkJson.Copy(State); candidate.Issued = NetworkJson.Copy(receipt); candidate.Pending = false; candidate.Draft = NetworkJson.Copy(receipt.Note); Persist(candidate);
        }
        public void OpenIssued(IssuedNote receipt)
        {
            if (State.Pending) throw new InvalidOperationException("Recupera primero la emisión pendiente.");
            WorkState candidate = new WorkState { Draft = NetworkJson.Copy(receipt.Note), RequestId = receipt.RequestId, ServerId = receipt.ServerId, Issued = NetworkJson.Copy(receipt) };
            WorkState previous = State; State = candidate;
            try { ValidateReceipt(receipt); Persist(candidate); } catch { State = previous; throw; }
        }
    }
}
