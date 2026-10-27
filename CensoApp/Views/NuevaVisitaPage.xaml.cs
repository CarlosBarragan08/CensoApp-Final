using CensoApp.Models;
using CensoApp.Services;
using System.Globalization;

namespace CensoApp.Views
{
    public partial class NuevaVisitaPage : ContentPage
    {
        private readonly List<FileResult> _archivosSeleccionados = new();
        private readonly List<string> _archivosAdjuntos = new();
        private readonly DatabaseService _dbService;
        private readonly Establecimiento _establecimiento;
        private readonly int _numeroVisita;

        public NuevaVisitaPage(DatabaseService dbService, Establecimiento establecimiento, int numeroVisita)
        {
            InitializeComponent();
            _dbService = dbService;
            _establecimiento = establecimiento;
            _numeroVisita = numeroVisita;

            LblTituloVisita.Text = $"REGISTRAR VISITA #{_numeroVisita}";
            DpFechaVisita.Date = DateTime.Now;

            PckObjetivo.SelectedIndex = 0;
            PckDenuncias.SelectedIndex = 0;
            PckEta.SelectedIndex = 0;
        }

        private void OnObjetivoSelectedIndexChanged(object sender, EventArgs e)
        {
            string seleccionado = PckObjetivo.SelectedItem?.ToString() ?? "";
            SeccionOtroObjetivo.IsVisible = (seleccionado == "OTRO");
        }

        private void OnToggleMedidasClicked(object sender, EventArgs e)
        {
            SeccionDesplegableMedidas.IsVisible = !SeccionDesplegableMedidas.IsVisible;
        }

        private void OnMedidaCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox chk && chk == ChkNoAplica && e.Value)
            {
                ChkClausuraTotal.IsChecked = false;
                ChkClausuraParcial.IsChecked = false;
                ChkSuspensionParcial.IsChecked = false;
                ChkSuspensionTotal.IsChecked = false;
                ChkDecomiso.IsChecked = false;
                ChkDestruccion.IsChecked = false;
                ChkDesnaturalizacion.IsChecked = false;
                ChkCongelacionTemporal.IsChecked = false;
                ChkSuspensionTemporalVenta.IsChecked = false;
                ChkOtraMedida.IsChecked = false;
            }
            else if (sender is CheckBox chkOtra && chkOtra != ChkNoAplica && e.Value)
            {
                ChkNoAplica.IsChecked = false;
            }

            SeccionOtraMedida.IsVisible = ChkOtraMedida.IsChecked;
            ActualizarTextoResumenMedidas();
        }

        private void ActualizarTextoResumenMedidas()
        {
            var seleccionadas = new List<string>();

            if (ChkClausuraTotal.IsChecked) seleccionadas.Add("CLAUSURA TEMPORAL TOTAL");
            if (ChkClausuraParcial.IsChecked) seleccionadas.Add("CLAUSURA TEMPORAL PARCIAL");
            if (ChkSuspensionParcial.IsChecked) seleccionadas.Add("SUSPENSIÓN PARCIAL DE TRABAJOS O SERVICIOS");
            if (ChkSuspensionTotal.IsChecked) seleccionadas.Add("SUSPENSIÓN TOTAL DE TRABAJOS O SERVICIOS");
            if (ChkDecomiso.IsChecked) seleccionadas.Add("DECOMISO DE OBJETOS Y PRODUCTOS");
            if (ChkDestruccion.IsChecked) seleccionadas.Add("DESTRUCCIÓN ARTICULOS O PRODUCTOS");
            if (ChkDesnaturalizacion.IsChecked) seleccionadas.Add("DESNATURALIZACIÓN ARTICULOS O PRODUCTOS");
            if (ChkCongelacionTemporal.IsChecked) seleccionadas.Add("CONGELACIÓN TEMPORAL VENTA/EMPLEO");
            if (ChkSuspensionTemporalVenta.IsChecked) seleccionadas.Add("SUSPENSIÓN TEMPORAL VENTA/EMPLEO");
            if (ChkOtraMedida.IsChecked) seleccionadas.Add("OTRA MEDIDA");

            if (seleccionadas.Count == 0 || ChkNoAplica.IsChecked)
            {
                LblMedidasSeleccionadas.Text = "NO APLICA";
            }
            else
            {
                LblMedidasSeleccionadas.Text = string.Join(", ", seleccionadas);
            }
        }

        private void OnPorcentajeTextChanged(object sender, TextChangedEventArgs e)
        {
            string textoLimpio = e.NewTextValue?.Replace(',', '.') ?? "";

            if (double.TryParse(textoLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out double pct))
            {
                if (pct < 0 || pct > 100)
                {
                    LblConceptoAuto.Text = "PORCENTAJE INVÁLIDO (0 - 100)";
                    LblConceptoAuto.TextColor = Color.FromArgb("#64748B");
                }
                else if (pct < 60)
                {
                    LblConceptoAuto.Text = "DESFAVORABLE";
                    LblConceptoAuto.TextColor = Color.FromArgb("#EF4444");
                }
                else if (pct >= 60 && pct < 90)
                {
                    LblConceptoAuto.Text = "FAVORABLE CON REQUERIMIENTOS";
                    LblConceptoAuto.TextColor = Color.FromArgb("#F59E0B");
                }
                else
                {
                    LblConceptoAuto.Text = "FAVORABLE";
                    LblConceptoAuto.TextColor = Color.FromArgb("#22C55E");
                }
            }
            else
            {
                LblConceptoAuto.Text = "INGRESE UN PORCENTAJE VÁLIDO";
                LblConceptoAuto.TextColor = Color.FromArgb("#64748B");
            }
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            string textoLimpio = TxtPorcentaje.Text?.Replace(',', '.') ?? "";

            if (!double.TryParse(textoLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out double pct) || pct < 0 || pct > 100)
            {
                await DisplayAlert("Atención", "Por favor ingrese un porcentaje válido entre 0 y 100.", "OK");
                return;
            }

            DateTime fechaSeleccionada = DpFechaVisita.Date ?? DateTime.Now;
            string pctFormateado = pct.ToString("0.##", CultureInfo.InvariantCulture);

            string motivoSeleccionado = PckObjetivo.SelectedItem?.ToString() ?? "PROGRAMACIÓN";
            if (motivoSeleccionado == "OTRO" && !string.IsNullOrWhiteSpace(TxtOtroObjetivo.Text))
            {
                motivoSeleccionado = $"OTRO: {TxtOtroObjetivo.Text.Trim()}";
            }

            var medidasSeleccionadasBD = new List<string>();

            if (ChkClausuraTotal.IsChecked) medidasSeleccionadasBD.Add("CLAUSURA TEMPORAL TOTAL");
            if (ChkClausuraParcial.IsChecked) medidasSeleccionadasBD.Add("CLAUSURA TEMPORAL PARCIAL");
            if (ChkSuspensionParcial.IsChecked) medidasSeleccionadasBD.Add("SUSPENSIÓN PARCIAL DE TRABAJOS O SERVICIOS");
            if (ChkSuspensionTotal.IsChecked) medidasSeleccionadasBD.Add("SUSPENSIÓN TOTAL DE TRABAJOS O SERVICIOS");
            if (ChkDecomiso.IsChecked) medidasSeleccionadasBD.Add("DECOMISO");
            if (ChkDestruccion.IsChecked) medidasSeleccionadasBD.Add("DESTRUCCIÓN");
            if (ChkDesnaturalizacion.IsChecked) medidasSeleccionadasBD.Add("DESNATURALIZACIÓN");
            if (ChkCongelacionTemporal.IsChecked)
                medidasSeleccionadasBD.Add("CONGELACIÓN TEMPORAL DE LA VENTA O EMPLEO DE PRODUCTOS Y OBJETOS");

            if (ChkSuspensionTemporalVenta.IsChecked)
                medidasSeleccionadasBD.Add("SUSPENSIÓN TEMPORAL DE LA VENTA O EMPLEO DE PRODUCTOS Y OBJETOS");

            if (ChkOtraMedida.IsChecked)
            {
                string textoOtra = !string.IsNullOrWhiteSpace(TxtOtraMedida.Text)
                    ? $"OTRA MEDIDA: {TxtOtraMedida.Text.Trim()}"
                    : "OTRA MEDIDA";
                medidasSeleccionadasBD.Add(textoOtra);
            }

            bool tieneMedidas = medidasSeleccionadasBD.Count > 0;
            string tieneMedidaSanitaria = tieneMedidas ? "SI" : "NO";
            string tipoMedidaFinal = tieneMedidas ? string.Join(", ", medidasSeleccionadasBD) : "NO APLICA";

            string urlsAdjuntas = "";

            if (_archivosSeleccionados != null && _archivosSeleccionados.Count > 0)
            {
                var listaUrls = new List<string>();
                foreach (var archivo in _archivosSeleccionados)
                {
                    // Sube el archivo físico al bucket 'documentos'
                    string urlPublica = await _dbService.SubirArchivoAsync(archivo);
                    listaUrls.Add(urlPublica);
                }
                urlsAdjuntas = string.Join(";", listaUrls);
            }

            var nuevaVisita = new Visita
            {
                EstablecimientoId = _establecimiento.Id,
                NumeroVisita = _numeroVisita,
                FechaVisita = fechaSeleccionada.ToString("dd/MM/yyyy"),
                PorcentajeCumplimiento = pctFormateado,
                ConceptoSanitario = LblConceptoAuto.Text,
                FuncionariosVisita = TxtFuncionarios.Text?.Trim() ?? "N/A",
                ObjetivoVisita = motivoSeleccionado,
                Denuncias = PckDenuncias.SelectedItem?.ToString() ?? "NO",
                Eta = PckEta.SelectedItem?.ToString() ?? "NO",
                TieneMedidaSanitaria = tieneMedidaSanitaria,
                TipoMedidaSanitaria = tipoMedidaFinal,
                Observaciones = TxtObservaciones.Text?.Trim() ?? "",
                DocumentoAdjunto = urlsAdjuntas,
            };

            await _dbService.GuardarVisitaAsync(nuevaVisita);

            _establecimiento.PorcentajeCumplimiento = pctFormateado;
            _establecimiento.ConceptoSanitario = LblConceptoAuto.Text;
            await _dbService.GuardarEstablecimientoAsync(_establecimiento);

            await DisplayAlert("Éxito", $"Visita #{_numeroVisita} registrada correctamente.", "OK");
            await Navigation.PopAsync();
        }

        private async void OnSeleccionarDocumentoClicked(object sender, EventArgs e)
        {
            try
            {
                var resultados = await FilePicker.Default.PickMultipleAsync(new PickOptions
                {
                    PickerTitle = "Seleccione los documentos (Acta IVC / Inscripción)"
                });

                if (resultados != null && resultados.Any())
                {
                    _archivosSeleccionados.AddRange(resultados);
                    TxtDocumentoAdjunto.Text = string.Join(", ", _archivosSeleccionados.Select(a => a.FileName));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudieron cargar los archivos: {ex.Message}", "OK");
            }
        }
    }
}