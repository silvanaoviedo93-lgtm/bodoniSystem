using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Bodoni
{
    public sealed class AuthorizationException : ArgumentException
    {
        public AuthorizationException(string message) : base(message) { }
    }

    public class PrintAuthorization
    {
        public string Number { get; set; }
        public string StartsOn { get; set; }
        public string ExpiresOn { get; set; }
        public string Footer { get; set; }
        public string RevisionId { get; set; }
        public string SavedUtc { get; set; }

        public static PrintAuthorization OriginalTemplate()
        {
            return new PrintAuthorization {
                Number = "1133313444", StartsOn = "2026-03-09", ExpiresOn = "2027-03-09",
                Footer = "Eugenio Girón Gorky Juan - R.U.C. 0902893130001 Aut. 13485 * 2B. (100x2) # 4026-4225 * Emisión: 9/Marzo/2026 * Caduca: 9/Marzo/2027",
                RevisionId = "plantilla-original-2026", SavedUtc = "2026-03-09T00:00:00Z"
            };
        }
        public void Validate()
        {
            DateTime start, end;
            if (String.IsNullOrWhiteSpace(Number) || Number.Length > 40) throw new ArgumentException("Ingresa el número de autorización de la nota (AUT. S.R.I.).");
            foreach (char c in Number) if (c < '0' || c > '9') throw new ArgumentException("El número de autorización debe contener solo dígitos.");
            if (!DateTime.TryParseExact(StartsOn, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out start) ||
                !DateTime.TryParseExact(ExpiresOn, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out end) || end < start)
                throw new ArgumentException("Revisa las fechas de emisión y caducidad del pie de imprenta.");
            if (String.IsNullOrWhiteSpace(Footer) || Footer.Length > 1500) throw new ArgumentException("Ingresa el pie de imprenta completo (máximo 1500 caracteres).");
            DateTime footerStart, footerEnd;
            if (!TryReadFooterDates(Footer, out footerStart, out footerEnd)) throw new ArgumentException("El pie debe incluir Emisión: día/mes/año y Caduca: día/mes/año. Puedes escribir el mes con número o en español.");
            if (footerStart != start || footerEnd != end) throw new ArgumentException("Las fechas ingresadas no coinciden con Emisión y Caduca del pie de imprenta.");
        }
        public static bool TryReadFooterDates(string footer, out DateTime start, out DateTime end)
        {
            start = end = DateTime.MinValue;
            Match first = Regex.Match(footer ?? "", @"Emisi[oó]n\s*:\s*(\d{1,2}[/.-][\p{L}\d]+[/.-]\d{4})", RegexOptions.IgnoreCase);
            Match last = Regex.Match(footer ?? "", @"Caduca\s*:\s*(\d{1,2}[/.-][\p{L}\d]+[/.-]\d{4})", RegexOptions.IgnoreCase);
            string[] formats = { "d/M/yyyy", "d/MMMM/yyyy", "d/MMM/yyyy" };
            CultureInfo spanish = CultureInfo.GetCultureInfo("es-EC");
            return first.Success && last.Success && DateTime.TryParseExact(first.Groups[1].Value.Replace('-', '/').Replace('.', '/'), formats, spanish, DateTimeStyles.AllowWhiteSpaces, out start) && DateTime.TryParseExact(last.Groups[1].Value.Replace('-', '/').Replace('.', '/'), formats, spanish, DateTimeStyles.AllowWhiteSpaces, out end);
        }
        public bool SameContent(PrintAuthorization other)
        {
            return other != null && Number == other.Number && StartsOn == other.StartsOn && ExpiresOn == other.ExpiresOn && Footer == other.Footer;
        }
    }

    public static class AuthorizationRules
    {
        // Mainland Ecuador's date; independent of the secondary computer's clock.
        internal static Func<DateTime> CurrentDate = delegate { return DateTime.UtcNow.AddHours(-5).Date; };
        public static string BlockReason(PrintAuthorization authorization, DateTime today, string noteDate)
        {
            if (authorization == null) return "Falta registrar la autorización y el pie de imprenta. Emisión bloqueada.";
            try { authorization.Validate(); } catch (ArgumentException ex) { return "Autorización no válida. " + ex.Message; }
            DateTime start = DateTime.ParseExact(authorization.StartsOn, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime end = DateTime.ParseExact(authorization.ExpiresOn, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (today.Date > end) return "La autorización " + authorization.Number + " está caducada desde el " + end.ToString("dd/MM/yyyy") + ". No se permite emitir notas de venta. Actualiza el pie de imprenta en el principal.";
            if (today.Date < start) return "La autorización todavía no está vigente. Emisión bloqueada hasta el " + start.ToString("dd/MM/yyyy") + ".";
            DateTime date;
            if (!DateTime.TryParseExact(noteDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) return "La fecha de emisión de la nota no es válida.";
            if (date < start || date > end) return "La fecha de la nota debe estar entre " + start.ToString("dd/MM/yyyy") + " y " + end.ToString("dd/MM/yyyy") + ", según el pie de imprenta. Emisión bloqueada.";
            return null;
        }
        public static string Notice(PrintAuthorization authorization, DateTime today)
        {
            string block = BlockReason(authorization, today, today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            if (block != null) return block;
            DateTime end = DateTime.ParseExact(authorization.ExpiresOn, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            int days = (end - today.Date).Days;
            return days <= 14 ? (days == 0 ? "La autorización caduca HOY" : "La autorización caduca en " + days + " día(s)") + " (" + end.ToString("dd/MM/yyyy") + "). Gestiona su renovación."
                : "Autorización " + authorization.Number + " vigente hasta " + end.ToString("dd/MM/yyyy") + ".";
        }
    }
}
