using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Bodoni
{
    static class Program
    {
        internal static bool PracticeMode;
        internal static void PreparePractice()
        {
            Directory.CreateDirectory(Storage.Root);
            if (File.Exists(NetworkConfig.ConfigPath)) return;
            if (File.Exists(NetworkConfig.LedgerPath) || File.Exists(NetworkConfig.LedgerPath + ".inicializado")) throw new InvalidDataException("Falta la conexión del entorno de práctica existente. Recupera sus archivos; no se reiniciará.");
            NetworkJson.Save(NetworkConfig.ConfigPath, NetworkConfig.Principal(4121, 47832));
        }
        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--self-test") return Verification.Run();
            if (args.Length > 0 && args[0] == "--network-test")
            {
                Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
                return NetworkVerification.Run();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0 && args[0] == "--ui-check") return CheckVisibleUi();
            PracticeMode = args.Length > 0 && args[0] == "--practica";
            if (PracticeMode) Storage.Root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datos_practica");
            try
            {
                Directory.CreateDirectory(Storage.Root);
                if (!PracticeMode) Storage.ImportLegacyPrintingAndDraft();
                using (FileStream lease = new FileStream(Path.Combine(Storage.Root, "cliente-abierto.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    if (PracticeMode) PreparePractice();
                    Application.Run(new MainForm());
                }
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo iniciar el prototipo.\n\n" + ex.Message, "Bodoni", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }
        static int CheckVisibleUi()
        {
            string output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entregables");
            Directory.CreateDirectory(output);
            string originalRoot = Storage.Root;
            string temporary = Path.Combine(output, "ui-check-" + Guid.NewGuid().ToString("N"));
            Storage.Root = temporary;
            int exitCode = 0;
            try
            {
                using (MainForm form = new MainForm())
                using (Timer timer = new Timer { Interval = 700 })
                {
                    timer.Tick += delegate
                    {
                        timer.Stop();
                        try
                        {
                            form.VerifyUi(Path.Combine(output, "prototipo-pantalla.png"));
                            using (AuthorizationForm authorization = new AuthorizationForm(PrintAuthorization.OriginalTemplate(), new List<PrintAuthorization>(), true))
                            {
                                authorization.Show(); Application.DoEvents();
                                using (Bitmap capture = new Bitmap(authorization.Width, authorization.Height))
                                {
                                    authorization.DrawToBitmap(capture, new Rectangle(0, 0, capture.Width, capture.Height));
                                    capture.Save(Path.Combine(output, "pie-imprenta-pantalla.png"));
                                }
                                authorization.Close();
                            }
                            File.WriteAllText(Path.Combine(output, "REVISION_PANTALLA.txt"), "OK - Ventana visible, captura de controles, edición de detalle y validación de cliente.\r\n", Encoding.UTF8);
                        }
                        catch (Exception ex)
                        {
                            File.WriteAllText(Path.Combine(output, "REVISION_PANTALLA.txt"), ex.ToString(), Encoding.UTF8);
                            exitCode = 1;
                        }
                        form.Close();
                    };
                    form.Shown += delegate { timer.Start(); };
                    Application.Run(form);
                }
            }
            catch (Exception ex) { File.WriteAllText(Path.Combine(output, "REVISION_PANTALLA.txt"), ex.ToString(), Encoding.UTF8); exitCode = 1; }
            finally
            {
                Storage.Root = originalRoot;
                if (Directory.Exists(temporary))
                {
                    foreach (string file in Directory.GetFiles(temporary)) File.Delete(file);
                    Directory.Delete(temporary, false);
                }
            }
            return exitCode;
        }
    }

    static class Verification
    {
        static readonly List<string> log = new List<string>();
        static void Check(bool condition, string name)
        {
            if (!condition) throw new Exception("FALLÓ: " + name);
            log.Add("OK - " + name);
        }
        static void Reject(Action action, string name)
        {
            bool rejected = false;
            try { action(); } catch (ArgumentException) { rejected = true; }
            Check(rejected, name);
        }
        public static int Run()
        {
            string output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entregables");
            Directory.CreateDirectory(output);
            string temporary = Path.Combine(output, "verificacion-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporary);
            int exitCode = 0;
            try
            {
                Draft sample = Draft.Sample(); sample.Date = "2026-11-12";
                Profile profile = new Profile();
                List<CalculatedLine> lines = Calculation.Lines(sample);
                Check(Calculation.Total(lines) == 50m, "100 × 0,25 + 2 × 12,50 = 50,00");
                Check(Calculation.Number("0,25", "Precio") == Calculation.Number("0.25", "Precio"), "Coma y punto decimal equivalentes");
                Reject(delegate { Calculation.Number("1,000.50", "Precio"); }, "Separadores de miles ambiguos rechazados");
                Reject(delegate { Calculation.Number("NaN", "Precio"); }, "Valores no numéricos rechazados");
                Draft rounding = Draft.Sample(); rounding.Lines.Clear(); rounding.Lines.Add(new DraftLine { Quantity = "1", Description = "Redondeo", UnitPrice = "0.005" });
                Check(Calculation.Total(Calculation.Lines(rounding)) == .01m, "Redondeo decimal de 0,005 a 0,01");
                rounding.Lines[0].Quantity = "0"; Reject(delegate { Calculation.Lines(rounding); }, "Cantidad cero rechazada");
                rounding.Lines[0].Quantity = "1"; rounding.Lines[0].UnitPrice = "-1"; Reject(delegate { Calculation.Lines(rounding); }, "Precio negativo rechazado");
                rounding.Lines[0].UnitPrice = "0"; Check(Calculation.Total(Calculation.Lines(rounding)) == 0, "Precio cero permitido");
                rounding.Lines[0].Quantity = "1.2345"; Reject(delegate { Calculation.Lines(rounding); }, "Exceso de decimales en cantidad rechazado");
                List<TextItem> items = Layout.Build(sample, profile, true);
                Check(items.Exists(delegate(TextItem item) { return item.Text == "PRUEBA"; }), "Salida marcada PRUEBA, sin número real");
                Check(!items.Exists(delegate(TextItem item) { return item.Text == "004122"; }), "No se presenta el próximo número real como emitido");
                byte[] pdf = Pdf.Create(items); string raw = Encoding.ASCII.GetString(pdf);
                Check(raw.Contains("/MediaBox [0 0 566.92913 411.02362]"), "Página PDF de 200 × 145 mm");
                Check(raw.Contains("(50.00) Tj") && raw.Contains("(PRUEBA) Tj"), "PDF contiene total y marca de prueba");
                Check(!raw.Contains("/Subtype /Image") && !raw.Contains("NOTA DE VENTA"), "PDF sin imagen de fondo ni encabezado preimpreso");
                Check(raw.Contains("/FontFile2 7 0 R") && raw.Contains("/FontFile2 10 0 R"), "Fuentes Arial y Times incrustadas en el PDF");
                Check(Fonts.Body.Measure("José Muñoz, impresión (A) \\", 8) > 0, "Caracteres españoles y literales especiales admitidos");
                Reject(delegate { Fonts.Body.Measure("😀", 8); }, "Caracteres no representables rechazados antes de exportar");
                Reject(delegate { Fonts.Body.Measure("Texto\nOtra línea", 8); }, "Saltos de línea rechazados para no invadir otras casillas");
                Draft longDescription = Draft.Sample(); longDescription.Lines[0].Description = new string('W', 200);
                Reject(delegate { Layout.Build(longDescription, profile, true); }, "Descripción que excede la casilla rechazada");
                Draft invalidCustomer = Draft.Sample(); invalidCustomer.Customer = "";
                Reject(delegate { Layout.Build(invalidCustomer, profile, true); }, "Cliente vacío rechazado para PDF");
                Draft invalidDate = Draft.Sample(); invalidDate.Date = "2026-02-30";
                Reject(delegate { Layout.Build(invalidDate, profile, true); }, "Fecha imposible rechazada");
                Draft tooMany = Draft.Sample(); while (tooMany.Lines.Count < 11) tooMany.Lines.Add(new DraftLine { Quantity = "1", Description = "Más filas", UnitPrice = "1" });
                Reject(delegate { Layout.Build(tooMany, profile, true); }, "Más de diez filas rechazadas");
                Draft ten = Draft.Sample(); while (ten.Lines.Count < 10) ten.Lines.Add(new DraftLine { Quantity = "1", Description = "Fila " + (ten.Lines.Count + 1), UnitPrice = "1" });
                Check(Layout.Build(ten, profile, true).Exists(delegate(TextItem item) { return item.Label == "Descripción, fila 10"; }), "Décima fila disponible sin invadir el total");
                Profile moved = new Profile { OffsetX = 1.25m, OffsetY = -0.5m };
                List<TextItem> shifted = Layout.Build(sample, moved, true);
                Check(Math.Abs(shifted[0].X - items[0].X - 1.25) < .00001 && Math.Abs(shifted[0].Baseline - items[0].Baseline + .5) < .00001, "Desplazamientos aplicados en milímetros");
                Profile noSeries = new Profile { PrintSeries = false };
                Check(!Layout.Build(sample, noSeries, true).Exists(delegate(TextItem item) { return item.Label == "Serie"; }), "Serie se puede ocultar si ya está preimpresa");
                Check(!profile.PrintSeries, "La serie preimpresa no se duplica en la configuración inicial");
                Check(Layout.Build(sample, new Profile { PrintSeries = true }, true).Exists(delegate(TextItem item) { return item.Label == "Serie"; }), "Se conserva la opción de imprimir serie para otros formularios");
                TextItem customerItem = items.Find(delegate(TextItem item) { return item.Label == "Cliente"; });
                TextItem identificationItem = items.Find(delegate(TextItem item) { return item.Label == "Identificación"; });
                TextItem lastRowItem = Layout.Build(ten, profile, true).Find(delegate(TextItem item) { return item.Label == "Descripción, fila 10"; });
                Check(customerItem.Baseline < 38 && identificationItem.Baseline < 53 && lastRowItem.Baseline < 122.4, "Cliente, identificación y décima fila dentro de las casillas de la plantilla original");
                using (Bitmap original = OriginalTemplate.Load())
                {
                    Check(original.Width == 2247 && original.Height == 1534, "Imagen original completa incrustada dentro del ejecutable");
                    byte[] completePdf = Pdf.Create(items, original);
                    string completeRaw = Encoding.ASCII.GetString(completePdf);
                    Check(completeRaw.Contains("/Subtype /Image") && completeRaw.Contains("/Template Do"), "PDF completo incluye la plantilla como fondo real del documento");
                    Check(completeRaw.Contains("/Width 2247 /Height 1534") && completeRaw.Contains("/Filter /DCTDecode"), "La plantilla se incrusta a su resolución completa");
                    Check(completeRaw.Contains("/MediaBox [0 0 566.92913 411.02362]") && completeRaw.Contains("(50.00) Tj") && completeRaw.Contains("(PRUEBA) Tj"), "PDF completo conserva medidas, total y marca de prueba");
                    Check(completeRaw.Contains("/Info 12 0 R") && raw.Contains("/Info 11 0 R"), "Referencias del PDF válidas con y sin objeto de imagen");
                    string duplicateSeries = Encoding.ASCII.GetString(Pdf.Create(Layout.Build(sample, new Profile { PrintSeries = true }, true), original));
                    Check(!duplicateSeries.Contains("(001-001-000) Tj"), "PDF completo no duplica la serie que ya aparece en la plantilla");
                    Check(completeRaw.IndexOf("/Template Do", StringComparison.Ordinal) < completeRaw.IndexOf("(PRUEBA) Tj", StringComparison.Ordinal), "La plantilla se dibuja detrás del texto y no lo tapa");
                    Storage.WriteBytes(Path.Combine(output, "Bodoni-PRUEBA-completa.pdf"), completePdf);
                    Pdf.Save(Path.Combine(output, "Bodoni-PRUEBA-completa-diez-filas.pdf"), ten, profile, original);
                    using (Bitmap bitmap = new Bitmap(1600, 1160))
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        Layout.Draw(graphics, new RectangleF(0, 0, 1600, 1160), items, original, false);
                        bitmap.Save(Path.Combine(output, "plantilla-con-texto-prueba.png"), System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                string draftPath = Path.Combine(temporary, "ejemplo.bodoni.json");
                Storage.WriteJson(draftPath, sample); Draft recovered = Storage.ReadJson<Draft>(draftPath);
                Check(recovered.Customer == sample.Customer && Calculation.Total(Calculation.Lines(recovered)) == 50m, "Borrador conserva cliente y detalle al guardarse y recuperarse");
                sample.Customer = "José Muñoz"; Storage.WriteJson(draftPath, sample);
                Check(Storage.ReadJson<Draft>(draftPath).Customer == "José Muñoz", "Reemplazo de borrador y caracteres españoles conservados");
                Settings config = new Settings(); config.Profiles.Add(moved); config.SelectedProfile = 1;
                string configPath = Path.Combine(temporary, "perfiles.json"); Storage.WriteJson(configPath, config);
                Settings saved = Storage.ReadJson<Settings>(configPath); saved.Validate();
                Check(saved.SelectedProfile == 1 && saved.Profiles[1].OffsetX == 1.25m, "Perfil y selección persisten al recuperarse");
                saved.Profiles[1].PrintSeries = true; saved.UpgradeTemplate();
                Check(!saved.Profiles[1].PrintSeries && saved.Profiles[1].OffsetX == 1.25m, "Perfiles anteriores evitan duplicar serie y conservan la calibración");
                saved.Profiles[1].PrintSeries = true; saved.UpgradeTemplate();
                Check(saved.Profiles[1].PrintSeries, "Una selección posterior de imprimir serie se respeta sin repetir la migración");
                Pdf.Save(Path.Combine(output, "Bodoni-PRUEBA-alineacion.pdf"), recovered, profile);
                Pdf.Save(Path.Combine(output, "Bodoni-PRUEBA-diez-filas.pdf"), ten, profile);
                Storage.WriteJson(Path.Combine(output, "ejemplo-50-dolares.bodoni.json"), recovered);
                using (Bitmap bitmap = new Bitmap(1200, 870))
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    Layout.Draw(graphics, new RectangleF(0, 0, 1200, 870), Layout.Build(recovered, profile, true), null, false);
                    bitmap.Save(Path.Combine(output, "vista-datos-prueba.png"), System.Drawing.Imaging.ImageFormat.Png);
                }
                Check(true, "PDF de muestra, borrador y vista de datos generados sin tocar datos_prototipo");
                // Isolate profile/template paths during the UI test as well.
                string originalRoot = Storage.Root;
                Storage.Root = temporary;
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    using (MainForm form = new MainForm()) { form.VerifyStartup("CLIENTE DE PRUEBA", 50m); form.VerifyPdfModes(temporary); form.VerifyUi(null); }
                    Check(true, "Controles de pantalla: total inicial, edición de celda y validación de cliente sin abrir ventanas");
                    Check(true, "Las dos opciones de exportación de la pantalla generan el PDF correspondiente");
                    Draft session = Draft.Sample(); session.Customer = "CLIENTE RECUPERADO"; session.Lines[0].Quantity = "200";
                    Storage.WriteJson(Storage.SessionPath, session);
                    using (MainForm form = new MainForm()) form.VerifyStartup("CLIENTE RECUPERADO", 75m);
                    Check(true, "Al abrir se recuperan automáticamente plantilla, texto y total de la última sesión");
                }
                finally { Storage.Root = originalRoot; }
            }
            catch (Exception ex) { log.Add("ERROR - " + ex); exitCode = 1; }
            finally
            {
                // Only remove the files created by this invocation, not a user directory.
                foreach (string file in Directory.GetFiles(temporary)) File.Delete(file);
                Directory.Delete(temporary, false);
                log.Add(exitCode == 0 ? "RESULTADO: TODAS LAS COMPROBACIONES CORRECTAS" : "RESULTADO: REVISAR ERRORES");
                File.WriteAllLines(Path.Combine(output, "VERIFICACION.txt"), log, new UTF8Encoding(false));
            }
            return exitCode;
        }
    }
}
