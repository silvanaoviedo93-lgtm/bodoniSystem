using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace Bodoni
{
    public class DraftLine
    {
        public string Quantity { get; set; }
        public string Description { get; set; }
        public string UnitPrice { get; set; }
    }

    public class Draft
    {
        public int Version { get; set; }
        public string Customer { get; set; }
        public string Identification { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Date { get; set; }
        public List<DraftLine> Lines { get; set; }
        public Draft()
        {
            Version = 1;
            Date = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            Lines = new List<DraftLine>();
        }
        public static Draft Sample()
        {
            Draft d = new Draft();
            d.Customer = "CLIENTE DE PRUEBA";
            d.Identification = "0000000000";
            d.Address = "DIRECCIÓN DE PRUEBA";
            d.Phone = "0000000000";
            d.Email = "prueba@example.com";
            d.Lines.Add(new DraftLine { Quantity = "100", Description = "Trabajo A", UnitPrice = "0,25" });
            d.Lines.Add(new DraftLine { Quantity = "2", Description = "Trabajo B", UnitPrice = "12,50" });
            return d;
        }
    }

    public class Profile
    {
        public string Name { get; set; }
        public decimal OffsetX { get; set; }
        public decimal OffsetY { get; set; }
        public decimal FontScale { get; set; }
        public bool PrintSeries { get; set; }
        // The supplied Bodoni form already includes the printed series.
        public Profile() { Name = "Epson L3250"; FontScale = 1; PrintSeries = false; }
        public override string ToString() { return Name; }
    }

    public class Settings
    {
        public List<Profile> Profiles { get; set; }
        public int SelectedProfile { get; set; }
        public int TemplateVersion { get; set; }
        public Settings() { Profiles = new List<Profile> { new Profile() }; }
        public void UpgradeTemplate()
        {
            if (TemplateVersion >= 1) return;
            // Older profiles defaulted to printing a series over the already
            // printed series. Migrate once, preserving all physical offsets.
            foreach (Profile profile in Profiles) profile.PrintSeries = false;
            TemplateVersion = 1;
        }
        public void Validate()
        {
            if (Profiles == null || Profiles.Count == 0) throw new InvalidDataException("No hay perfiles de impresión.");
            if (SelectedProfile < 0 || SelectedProfile >= Profiles.Count) SelectedProfile = 0;
            foreach (Profile p in Profiles)
            {
                if (p == null || String.IsNullOrWhiteSpace(p.Name) || Math.Abs(p.OffsetX) > 10 || Math.Abs(p.OffsetY) > 10 || p.FontScale < .8m || p.FontScale > 1.2m)
                    throw new InvalidDataException("El perfil de impresión no es válido.");
            }
        }
    }

    public static class Storage
    {
        public static string Root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datos_bodoni");
        public static void ImportLegacyPrintingAndDraft()
        {
            string legacy = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datos_prototipo");
            Directory.CreateDirectory(Root);
            foreach (string name in new string[] { "perfiles.json", "plantilla.png", "ultima-sesion.bodoni.json" })
            {
                string source = Path.Combine(legacy, name), target = Path.Combine(Root, name);
                if (File.Exists(source) && !File.Exists(target)) File.Copy(source, target);
            }
        }
        public static string ConfigPath { get { return Path.Combine(Root, "perfiles.json"); } }
        public static string TemplatePath { get { return Path.Combine(Root, "plantilla.png"); } }
        public static string SessionPath { get { return Path.Combine(Root, "ultima-sesion.bodoni.json"); } }
        public static void WriteJson(string path, object value)
        {
            WriteBytes(path, new UTF8Encoding(false).GetBytes(new JavaScriptSerializer().Serialize(value)));
        }
        public static T ReadJson<T>(string path)
        {
            if (new FileInfo(path).Length > 2 * 1024 * 1024) throw new InvalidDataException("El archivo es demasiado grande.");
            return new JavaScriptSerializer().Deserialize<T>(File.ReadAllText(path, Encoding.UTF8));
        }
        public static void WriteBytes(string path, byte[] content)
        {
            string full = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            string temp = full + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (FileStream file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    file.Write(content, 0, content.Length);
                    file.Flush(true);
                }
                if (File.Exists(full)) File.Replace(temp, full, null);
                else File.Move(temp, full);
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
    }

    public static class OriginalTemplate
    {
        public static Bitmap Load()
        {
            using (Stream stream = typeof(OriginalTemplate).Assembly.GetManifestResourceStream("Bodoni.Plantilla.jpg"))
            {
                if (stream == null) throw new InvalidDataException("No se encontró la plantilla original dentro del programa. Vuelve a compilar incluyendo el recurso de Bodoni.");
                using (Image image = Image.FromStream(stream)) return new Bitmap(image);
            }
        }
    }

    public class CalculatedLine
    {
        public decimal Quantity, Price, Amount;
        public string Description;
    }

    public static class Calculation
    {
        public static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        public static decimal Number(string input, string field)
        {
            string s = (input ?? "").Trim();
            decimal number;
            if (s.Contains(",") && s.Contains(".")) throw new ArgumentException(field + ": utiliza coma o punto decimal, sin separadores de miles.");
            s = s.Replace(',', '.');
            if (!Decimal.TryParse(s, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, Invariant, out number))
                throw new ArgumentException(field + ": ingresa un número válido, sin separadores de miles.");
            return number;
        }
        public static List<CalculatedLine> Lines(Draft draft)
        {
            if (draft == null || draft.Lines == null) throw new ArgumentException("No se encontraron los datos de la nota.");
            List<CalculatedLine> result = new List<CalculatedLine>();
            foreach (DraftLine raw in draft.Lines)
            {
                if (raw == null) throw new ArgumentException("Hay una fila inválida.");
                if (String.IsNullOrWhiteSpace(raw.Quantity) && String.IsNullOrWhiteSpace(raw.Description) && String.IsNullOrWhiteSpace(raw.UnitPrice)) continue;
                int row = result.Count + 1;
                decimal quantity = Number(raw.Quantity, "Cantidad de la fila " + row);
                decimal price = Number(raw.UnitPrice, "Precio de la fila " + row);
                if (quantity <= 0 || quantity > 1000000m) throw new ArgumentException("Fila " + row + ": la cantidad debe ser mayor que cero y no superar 1.000.000.");
                if (price < 0 || price > 1000000000m) throw new ArgumentException("Fila " + row + ": el precio debe estar entre cero y 1.000.000.000.");
                if (Decimal.Round(quantity, 3) != quantity) throw new ArgumentException("Fila " + row + ": utiliza hasta tres decimales en cantidad.");
                if (Decimal.Round(price, 4) != price) throw new ArgumentException("Fila " + row + ": utiliza hasta cuatro decimales en precio.");
                string description = (raw.Description ?? "").Trim();
                if (description.Length == 0) throw new ArgumentException("Fila " + row + ": falta la descripción.");
                result.Add(new CalculatedLine { Quantity = quantity, Price = price, Description = description, Amount = Decimal.Round(quantity * price, 2, MidpointRounding.AwayFromZero) });
            }
            if (result.Count == 0) throw new ArgumentException("Agrega al menos un concepto para generar el PDF.");
            if (result.Count > 10) throw new ArgumentException("El formulario tiene espacio para diez filas. Reduce el detalle antes de generar el PDF.");
            return result;
        }
        public static decimal Total(List<CalculatedLine> lines)
        {
            decimal total = 0;
            foreach (CalculatedLine line in lines) total += line.Amount;
            return total;
        }
        public static string Money(decimal amount) { return amount.ToString("0.00", Invariant); }
    }

    // Read the actual embedded font advances so PDF positioning and preview use
    // the same metrics. Only WinAnsi (Spanish/Western European) input is allowed.
    public sealed class FontFace
    {
        public readonly byte[] Bytes;
        public readonly string PdfName, Family;
        public readonly int[] Widths = new int[256];
        public readonly int[] Bounds = new int[4];
        public readonly int Ascent, Descent;
        public static readonly Encoding WinAnsi = Encoding.GetEncoding(1252, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
        int U16(int at) { return (Bytes[at] << 8) | Bytes[at + 1]; }
        int S16(int at) { return (short)U16(at); }
        int U32(int at) { return (Bytes[at] << 24) | (Bytes[at + 1] << 16) | (Bytes[at + 2] << 8) | Bytes[at + 3]; }
        public FontFace(string path, string pdfName, string family)
        {
            Bytes = File.ReadAllBytes(path); PdfName = pdfName; Family = family;
            Dictionary<string, int> tables = new Dictionary<string, int>();
            int count = U16(4);
            for (int i = 0; i < count; i++)
            {
                int at = 12 + 16 * i;
                tables[Encoding.ASCII.GetString(Bytes, at, 4)] = U32(at + 8);
            }
            int head = tables["head"], hhea = tables["hhea"], hmtx = tables["hmtx"], cmap = tables["cmap"];
            double units = U16(head + 18);
            for (int i = 0; i < 4; i++) Bounds[i] = (int)Math.Round(S16(head + 36 + i * 2) * 1000 / units);
            Ascent = (int)Math.Round(S16(hhea + 4) * 1000 / units);
            Descent = (int)Math.Round(S16(hhea + 6) * 1000 / units);
            int metrics = U16(hhea + 34), format4 = -1;
            for (int i = 0; i < U16(cmap + 2); i++)
            {
                int at = cmap + 4 + 8 * i;
                int subtable = cmap + U32(at + 4);
                if (U16(at) == 3 && U16(at + 2) == 1 && U16(subtable) == 4) format4 = subtable;
            }
            if (format4 < 0) throw new InvalidDataException("La fuente no tiene el mapa de caracteres necesario.");
            int segments = U16(format4 + 6) / 2;
            int ends = format4 + 14, starts = ends + segments * 2 + 2, deltas = starts + segments * 2, ranges = deltas + segments * 2;
            for (int code = 32; code <= 255; code++)
            {
                char unicode = WinAnsi.GetString(new byte[] { (byte)code })[0];
                int glyph = 0;
                for (int j = 0; j < segments; j++)
                {
                    if (unicode < U16(starts + j * 2) || unicode > U16(ends + j * 2)) continue;
                    int range = U16(ranges + j * 2), delta = S16(deltas + j * 2);
                    if (range == 0) glyph = (unicode + delta) & 65535;
                    else
                    {
                        int offset = ranges + j * 2 + range + (unicode - U16(starts + j * 2)) * 2;
                        glyph = U16(offset);
                        if (glyph != 0) glyph = (glyph + delta) & 65535;
                    }
                    break;
                }
                Widths[code] = (int)Math.Round(U16(hmtx + Math.Min(glyph, metrics - 1) * 4) * 1000 / units);
            }
        }
        public double Measure(string text, double size)
        {
            foreach (char character in text)
                if (Char.IsControl(character)) throw new ArgumentException("El texto contiene saltos de línea o caracteres de control que no caben en una casilla.");
            byte[] codes;
            try { codes = WinAnsi.GetBytes(text); }
            catch (EncoderFallbackException) { throw new ArgumentException("Utiliza caracteres de texto en español o latinos; no se admiten emojis en este formato."); }
            double width = 0;
            foreach (byte code in codes)
            {
                if (code < 32 || code == 127) throw new ArgumentException("El texto contiene saltos de línea o caracteres de control que no caben en una casilla.");
                width += Widths[code];
            }
            return width * size / 1000;
        }
    }

    public static class Fonts
    {
        public static readonly FontFace Body = new FontFace(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"), "ArialMT", "Arial");
        public static readonly FontFace Number = new FontFace(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "times.ttf"), "TimesNewRomanPSMT", "Times New Roman");
    }

    public class TextItem
    {
        public string Text, Label;
        public double X, Baseline, Width, Size;
        public bool Right, Center, Red;
        public FontFace Face;
        public double StartX { get { double width = Face.Measure(Text, Size) / Layout.PointsPerMm; return X + (Right ? Width - width : Center ? (Width - width) / 2 : 0); } }
    }

    public static class Layout
    {
        public const double PageWidth = 200, PageHeight = 145, PointsPerMm = 72 / 25.4;
        // Positions match the supplied original, 2247 x 1534 pixels, mapped to
        // 200 x 145 mm. Physical printer calibration remains pending.
        public static List<TextItem> Build(Draft draft, Profile profile, bool strict)
        {
            return Build(draft, profile, strict, "PRUEBA");
        }
        public static List<TextItem> Build(Draft draft, Profile profile, bool strict, string number)
        {
            int parsedNumber;
            if (number != "PRUEBA" && (number == null || number.Length != 6 || !Int32.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out parsedNumber) || parsedNumber < 1))
                throw new ArgumentException("El número debe provenir de una nota emitida por el principal.");
            List<TextItem> result = new List<TextItem>();
            if (draft == null) throw new ArgumentException("No hay datos para mostrar.");
            if (strict && String.IsNullOrWhiteSpace(draft.Customer)) throw new ArgumentException("Ingresa el nombre del cliente.");
            DateTime date;
            if (!DateTime.TryParseExact(draft.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) throw new ArgumentException("La fecha de emisión no es válida.");
            if (profile.PrintSeries) Add(result, "001-001-000", "Serie", 156.5, 9.0, 42.2, 8.5, true, false, false, profile);
            Add(result, number, number == "PRUEBA" ? "Número de prueba" : "Número de nota", 156.5, 15.7, 42.2, 18, true, false, true, profile);
            Add(result, draft.Customer, "Cliente", 13.8, 37.2, 125, 8, false, false, false, profile);
            Add(result, date.ToString("dd/MM/yyyy"), "Fecha", 164.1, 37.2, 34.5, 8, false, false, false, profile);
            Add(result, draft.Address, "Dirección", 16.8, 44.3, 131, 8, false, false, false, profile);
            Add(result, draft.Phone, "Teléfono", 164.1, 44.3, 34.5, 8, false, false, false, profile);
            Add(result, draft.Identification, "Identificación", 20, 51.4, 91, 8, false, false, false, profile);
            Add(result, draft.Email, "Correo", 122.5, 51.4, 76.1, 8, false, false, false, profile);
            List<CalculatedLine> lines = Calculation.Lines(draft);
            for (int i = 0; i < lines.Count; i++)
            {
                CalculatedLine line = lines[i]; double y = 64.7 + 6.18 * i;
                Add(result, line.Quantity.ToString("0.###", Calculation.Invariant), "Cantidad, fila " + (i + 1), 1.4, y, 10.8, 8, false, true, false, profile);
                Add(result, line.Description, "Descripción, fila " + (i + 1), 15, y, 131.5, 8, false, false, false, profile);
                Add(result, line.Price.ToString("0.00##", Calculation.Invariant), "Precio, fila " + (i + 1), 148.7, y, 21, 8, false, true, false, profile);
                Add(result, Calculation.Money(line.Amount), "Importe, fila " + (i + 1), 172.2, y, 26, 8, false, true, false, profile);
            }
            Add(result, Calculation.Money(Calculation.Total(lines)), "Total", 172.2, 131, 26, 11, false, true, false, profile);
            foreach (TextItem item in result)
            {
                double width = item.Face.Measure(item.Text, item.Size) / PointsPerMm;
                if (strict && width > item.Width + .01) throw new ArgumentException(item.Label + ": el texto excede su espacio en la plantilla. Acórtalo o ajusta el tamaño de letra.");
                if (strict && (item.StartX < 0 || item.StartX + width > PageWidth || item.Baseline > PageHeight || item.Baseline - item.Size / PointsPerMm < 0))
                    throw new ArgumentException(item.Label + ": el ajuste coloca el texto fuera de la hoja.");
            }
            return result;
        }
        static void Add(List<TextItem> list, string text, string label, double x, double baseline, double width, double size, bool center, bool right, bool red, Profile profile)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0) return;
            list.Add(new TextItem { Text = text, Label = label, X = x + (double)profile.OffsetX, Baseline = baseline + (double)profile.OffsetY, Width = width, Size = size * (double)profile.FontScale, Center = center, Right = right, Red = red, Face = red ? Fonts.Number : Fonts.Body });
        }
        public static void Draw(Graphics graphics, RectangleF page, List<TextItem> items, Image template, bool guides)
        {
            GraphicsState state = graphics.Save();
            graphics.SetClip(page);
            graphics.FillRectangle(Brushes.White, page);
            if (template != null) graphics.DrawImage(template, page);
            double scale = page.Width / PageWidth;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            using (StringFormat format = (StringFormat)StringFormat.GenericTypographic.Clone())
            using (Pen guide = new Pen(Color.FromArgb(100, 0, 149, 205), 1))
            {
                format.FormatFlags |= StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces;
                guide.DashStyle = DashStyle.Dot;
                foreach (TextItem item in items)
                {
                    // The original background already contains the series.
                    // The series checkbox controls the data-only output.
                    if (template != null && item.Label == "Serie") continue;
                    double sizePx = item.Size / PointsPerMm * scale;
                    using (Font font = new Font(item.Face.Family, (float)sizePx, FontStyle.Regular, GraphicsUnit.Pixel))
                    {
                        double ascent = (double)font.FontFamily.GetCellAscent(FontStyle.Regular) / font.FontFamily.GetEmHeight(FontStyle.Regular) * sizePx;
                        float x = page.Left + (float)(item.StartX * scale);
                        float baseline = page.Top + (float)(item.Baseline * scale);
                        graphics.DrawString(item.Text, font, item.Red ? Brushes.Firebrick : Brushes.Black, new PointF(x, baseline - (float)ascent), format);
                        if (guides) graphics.DrawRectangle(guide, page.Left + (float)(item.X * scale), baseline - (float)ascent, (float)(item.Width * scale), (float)(sizePx * 1.2));
                    }
                }
            }
            graphics.Restore(state);
        }
    }

    public static class Pdf
    {
        static string N(double value) { return value.ToString("0.#####", CultureInfo.InvariantCulture); }
        static byte[] A(string text) { return Encoding.ASCII.GetBytes(text); }
        static byte[] Stream(byte[] bytes, string extra)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                byte[] header = A("<< /Length " + bytes.Length + " " + extra + " >>\nstream\n");
                stream.Write(header, 0, header.Length); stream.Write(bytes, 0, bytes.Length);
                byte[] end = A("\nendstream"); stream.Write(end, 0, end.Length); return stream.ToArray();
            }
        }
        static string Literal(string text)
        {
            StringBuilder escaped = new StringBuilder();
            foreach (byte value in FontFace.WinAnsi.GetBytes(text))
            {
                if (value == '(' || value == ')' || value == '\\') escaped.Append('\\').Append((char)value);
                else if (value >= 32 && value < 127) escaped.Append((char)value);
                else escaped.Append('\\').Append(Convert.ToString(value, 8).PadLeft(3, '0'));
            }
            return "(" + escaped + ")";
        }
        static void AddFont(List<byte[]> objects, FontFace font, int descriptorId, int fileId)
        {
            StringBuilder widths = new StringBuilder();
            for (int i = 32; i <= 255; i++) widths.Append(font.Widths[i]).Append(' ');
            objects.Add(A("<< /Type /Font /Subtype /TrueType /BaseFont /" + font.PdfName + " /FirstChar 32 /LastChar 255 /Widths [" + widths + "] /Encoding /WinAnsiEncoding /FontDescriptor " + descriptorId + " 0 R >>"));
            objects.Add(A("<< /Type /FontDescriptor /FontName /" + font.PdfName + " /Flags 32 /FontBBox [" + String.Join(" ", font.Bounds) + "] /ItalicAngle 0 /Ascent " + font.Ascent + " /Descent " + font.Descent + " /CapHeight " + font.Ascent + " /StemV 80 /FontFile2 " + fileId + " 0 R >>"));
            objects.Add(Stream(font.Bytes, "/Length1 " + font.Bytes.Length));
        }
        public static byte[] Create(List<TextItem> items)
        {
            return Create(items, null);
        }
        static byte[] ImageObject(Image template)
        {
            // Encode the same background shown in the preview. Flatten alpha
            // onto white when a user-supplied PNG is used instead of the JPEG.
            using (Bitmap bitmap = new Bitmap(template.Width, template.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (MemoryStream jpeg = new MemoryStream())
            {
                graphics.Clear(Color.White);
                graphics.DrawImage(template, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                System.Drawing.Imaging.ImageCodecInfo codec = null;
                foreach (System.Drawing.Imaging.ImageCodecInfo candidate in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
                    if (candidate.MimeType == "image/jpeg") codec = candidate;
                if (codec == null) throw new InvalidOperationException("No se encontró el codificador de imágenes para incluir la plantilla.");
                using (System.Drawing.Imaging.EncoderParameters parameters = new System.Drawing.Imaging.EncoderParameters(1))
                {
                    parameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L);
                    bitmap.Save(jpeg, codec, parameters);
                }
                return Stream(jpeg.ToArray(), "/Type /XObject /Subtype /Image /Width " + bitmap.Width + " /Height " + bitmap.Height + " /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode");
            }
        }
        public static byte[] Create(List<TextItem> items, Image template)
        {
            StringBuilder content = new StringBuilder();
            if (template != null)
                content.Append("q ").Append(N(Layout.PageWidth * Layout.PointsPerMm)).Append(" 0 0 ")
                    .Append(N(Layout.PageHeight * Layout.PointsPerMm)).Append(" 0 0 cm /Template Do Q\n");
            foreach (TextItem item in items)
            {
                if (template != null && item.Label == "Serie") continue;
                content.Append(item.Red ? "0.8 0.08 0.08 rg\n" : "0 0 0 rg\n");
                content.Append("BT /").Append(item.Red ? "F2" : "F1").Append(' ').Append(N(item.Size)).Append(" Tf 1 0 0 1 ")
                    .Append(N(item.StartX * Layout.PointsPerMm)).Append(' ').Append(N((Layout.PageHeight - item.Baseline) * Layout.PointsPerMm)).Append(" Tm ")
                    .Append(Literal(item.Text)).Append(" Tj ET\n");
            }
            List<byte[]> objects = new List<byte[]>();
            objects.Add(A("<< /Type /Catalog /Pages 2 0 R >>"));
            objects.Add(A("<< /Type /Pages /Kids [3 0 R] /Count 1 >>"));
            objects.Add(A("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " + N(Layout.PageWidth * Layout.PointsPerMm) + " " + N(Layout.PageHeight * Layout.PointsPerMm) + "] /Resources << /Font << /F1 5 0 R /F2 8 0 R >> " + (template == null ? "" : "/XObject << /Template 11 0 R >> ") + ">> /Contents 4 0 R >>"));
            objects.Add(Stream(A(content.ToString()), ""));
            AddFont(objects, Fonts.Body, 6, 7);
            AddFont(objects, Fonts.Number, 9, 10);
            if (template != null) objects.Add(ImageObject(template));
            int infoId = objects.Count + 1;
            string documentNumber = "PRUEBA";
            foreach (TextItem item in items) if (item.Label == "Número de nota") documentNumber = item.Text;
            objects.Add(A("<< /Title (Bodoni - " + (template == null ? "SOLO DATOS" : "CON PLANTILLA") + " - " + documentNumber + ") /Creator (Bodoni) >>"));
            using (MemoryStream output = new MemoryStream())
            {
                byte[] header = A("%PDF-1.4\n% Bodoni - datos de prueba\n"); output.Write(header, 0, header.Length);
                List<long> offsets = new List<long> { 0 };
                for (int i = 0; i < objects.Count; i++)
                {
                    offsets.Add(output.Position); byte[] prefix = A((i + 1) + " 0 obj\n"); output.Write(prefix, 0, prefix.Length);
                    output.Write(objects[i], 0, objects[i].Length); byte[] suffix = A("\nendobj\n"); output.Write(suffix, 0, suffix.Length);
                }
                long xref = output.Position;
                StringBuilder trailer = new StringBuilder("xref\n0 " + (objects.Count + 1) + "\n0000000000 65535 f \n");
                for (int i = 1; i < offsets.Count; i++) trailer.Append(offsets[i].ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
                trailer.Append("trailer\n<< /Size ").Append(objects.Count + 1).Append(" /Root 1 0 R /Info ").Append(infoId).Append(" 0 R >>\nstartxref\n").Append(xref).Append("\n%%EOF\n");
                byte[] tail = A(trailer.ToString()); output.Write(tail, 0, tail.Length); return output.ToArray();
            }
        }
        public static void Save(string path, Draft draft, Profile profile)
        {
            Storage.WriteBytes(path, Create(Layout.Build(draft, profile, true)));
        }
        public static void Save(string path, Draft draft, Profile profile, Image template)
        {
            if (template == null) throw new ArgumentException("No hay una plantilla cargada para generar el PDF completo.");
            Storage.WriteBytes(path, Create(Layout.Build(draft, profile, true), template));
        }
        public static void SaveIssued(string path, IssuedNote note, Profile profile, Image template)
        {
            if (note == null || note.Fingerprint != NoteRules.Fingerprint(note.Note) || note.Number < 1 || note.Number > 999999 || note.Series != "001-001-000")
                throw new ArgumentException("No se encontró una nota emitida válida para generar el PDF.");
            Storage.WriteBytes(path, Create(Layout.Build(note.Note, profile, true, note.DisplayNumber), template));
        }
    }
}
