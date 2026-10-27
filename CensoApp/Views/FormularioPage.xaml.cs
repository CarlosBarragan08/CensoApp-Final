using CensoApp.Models;
using CensoApp.Services;
using System.Globalization;

namespace CensoApp.Views
{
    public partial class FormularioPage : ContentPage
    {
        private readonly List<FileResult> _archivosSeleccionados = new();
        private readonly List<string> _archivosAdjuntos = new();
        private readonly DatabaseService _dbService;
        private readonly Establecimiento? _establecimientoParaEditar;

        private readonly Dictionary<string, string> _divipolaDeptos = new()
        {
            { "SUCRE", "70" },
            { "BOLÍVAR", "13" },
            { "CÓRDOBA", "23" },
            { "ATLÁNTICO", "08" },
            { "MAGDALENA", "47" }
        };

        private readonly Dictionary<string, string> _divipolaMunicipios = new()
        {
            { "COROZAL", "215" },
            { "SINCELEJO", "001" },
            { "BUENAVISTA", "110" },
            { "CAIMITO", "124" },
            { "COLOSO", "204" },
            { "COVEÑAS", "221" },
            { "EL ROBLE", "230" },
            { "GALERAS", "233" },
            { "GUARANDA", "265" },
            { "LA UNIÓN", "400" },
            { "LOS PALMITOS", "418" },
            { "MAJAGUAL", "429" },
            { "MORROA", "473" },
            { "OVEJAS", "508" },
            { "PALMITO", "523" },
            { "SAMPUÉS", "670" },
            { "SAN BENITO ABAD", "678" },
            { "SAN JUAN DE BETULIA", "708" },
            { "SAN MARCOS", "713" },
            { "SAN ONOFRE", "742" },
            { "SAN PEDRO", "771" },
            { "SINCÉ", "717" },
            { "SUCRE", "773" },
            { "TOLÚ", "820" },
            { "TOLÚ VIEJO", "823" }
        };

        private readonly Dictionary<string, List<string>> _tiposEstablecimiento = new()
        {
            { "PREPARACIÓN ALIMENTOS", new() { "RESTAURANTE", "CAFETERÍA", "PANADERÍA Y/O PASTELERÍA", "JUGOS, FRUTERÍA Y/O HELADERÍA", "COMIDA RÁPIDA" } },
            { "COMEDORES", new() { "PROGRAMAS SOCIALES DEL ESTADO", "COMEDORES CARCELARIOS U PENITENCIARIOS (USPEC)" } },
            { "EXPENDIO ALIMENTOS", new() { "EXPENDIO", "PRODUCTOS DE LA PESCA" } },
            { "GRANDES SUPERFICIES", new() { "HIPERMERCADO / SUPERMERCADO" } },
            { "ENSAMBLE ALIMENTOS", new() { "ENSAMBLE MENÚ", "ENSAMBLE REFRIGERIO" } },
            { "ALMACENAMIENTO", new() { "ALMACENAMIENTO A TEMPERATURA AMBIENTE", "DEPÓSITO DE FRÍO" } },
            { "VENTA VÍA PÚBLICA", new() { "PUESTO FIJO O ESTACIONARIO", "PUESTO MÓVIL O AMBULANTE", "ESTACIONARIO CON PREPARACIÓN DE ALIMENTOS", "AMBULANTE CON PREPARACIÓN DE ALIMENTOS" } },
            { "EXPENDIO BEBIDAS ALCOHÓLICAS", new() { "EXPENDIO BEBIDAS ALCOHÓLICAS" } },
            { "PLAZAS DE MERCADO", new() { "PLAZA DE MERCADO", "CENTRAL DE ABASTO" } }
        };

        public FormularioPage(DatabaseService dbService, Establecimiento? establecimiento = null)
        {
            InitializeComponent();
            _dbService = dbService;
            _establecimientoParaEditar = establecimiento;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_establecimientoParaEditar != null)
            {
                CargarDatosEdicion();
            }
            else
            {
                CargarValoresPorDefecto();
            }
        }

        private void CargarValoresPorDefecto()
        {
            if (PckTipoDocumento.SelectedIndex < 0) PckTipoDocumento.SelectedIndex = 0;
            if (PckTipoSujeto.SelectedIndex < 0) PckTipoSujeto.SelectedIndex = 0;
            if (PckEstado.SelectedIndex < 0) PckEstado.SelectedIndex = 0;
            if (PckMunicipio.SelectedIndex < 0) PckMunicipio.SelectedIndex = 0;
            if (PckDepartamento.SelectedIndex < 0) PckDepartamento.SelectedIndex = 0;
            if (PckArea.SelectedIndex < 0) PckArea.SelectedIndex = 0;
            if (PckActividadPrincipal.SelectedIndex < 0) PckActividadPrincipal.SelectedIndex = 0;
            if (PckRiesgo.SelectedIndex < 0) PckRiesgo.SelectedIndex = 0;
            if (PckDec1500.SelectedIndex < 0) PckDec1500.SelectedIndex = 0;
            if (PckObjetivoInicial.SelectedIndex < 0) PckObjetivoInicial.SelectedIndex = 0;
            if (PckDenunciasInicial.SelectedIndex < 0) PckDenunciasInicial.SelectedIndex = 0;
            if (PckEtaInicial.SelectedIndex < 0) PckEtaInicial.SelectedIndex = 0;

            ActualizarTipoEstablecimiento();
            ActualizarDivipolaMunicipio();
            ActualizarDivipolaDepartamento();
        }

        private void CargarDatosEdicion()
        {
            Title = "Editar Establecimiento";

            TxtNombreComercial.Text = _establecimientoParaEditar!.NombreComercial;
            TxtRazonSocial.Text = _establecimientoParaEditar.RazonSocial;
            PckTipoDocumento.SelectedItem = _establecimientoParaEditar.TipoDocumento;
            TxtNumeroDocumento.Text = _establecimientoParaEditar.NitOCedula;
            TxtInscripcion.Text = _establecimientoParaEditar.NumeroInscripcion;
            TxtRepresentante.Text = _establecimientoParaEditar.RepresentanteLegal;
            PckTipoSujeto.SelectedItem = _establecimientoParaEditar.TipoSujeto;
            PckEstado.SelectedItem = _establecimientoParaEditar.Estado;

            TxtDireccion.Text = _establecimientoParaEditar.Direccion;
            PckMunicipio.SelectedItem = _establecimientoParaEditar.Municipio;
            TxtDivipolaMuni.Text = _establecimientoParaEditar.CodigoDivipolaMunicipio;
            PckDepartamento.SelectedItem = _establecimientoParaEditar.Departamento;
            TxtDivipolaDpto.Text = _establecimientoParaEditar.CodigoDivipolaDpto;
            PckArea.SelectedItem = _establecimientoParaEditar.Area;
            TxtTelefono.Text = _establecimientoParaEditar.Telefono;
            TxtCorreo.Text = _establecimientoParaEditar.CorreoElectronico;

            if (!string.IsNullOrEmpty(_establecimientoParaEditar.ActividadPrincipal))
            {
                PckActividadPrincipal.SelectedItem = _establecimientoParaEditar.ActividadPrincipal;
            }

            ActualizarTipoEstablecimiento();

            if (!string.IsNullOrEmpty(_establecimientoParaEditar.TipoEstablecimiento))
            {
                PckTipoEstablecimiento.SelectedItem = _establecimientoParaEditar.TipoEstablecimiento;
            }

            PckRiesgo.SelectedItem = _establecimientoParaEditar.TipoRiesgo;
            PckDec1500.SelectedItem = _establecimientoParaEditar.AutorizacionSanitariaDecreto1500;
            TxtResAutorizacion.Text = _establecimientoParaEditar.NumeroFechaResolucionAutorizacion;

            TxtPorcentajeInicial.Text = _establecimientoParaEditar.PorcentajeCumplimiento;
        }

        public void OnDepartamentoSelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarDivipolaDepartamento();
        }

        public void OnMunicipioSelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarDivipolaMunicipio();
        }

        public void OnActividadPrincipalSelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarTipoEstablecimiento();
        }

        private void ActualizarDivipolaDepartamento()
        {
            if (PckDepartamento.SelectedItem != null)
            {
                string depto = PckDepartamento.SelectedItem.ToString() ?? string.Empty;
                TxtDivipolaDpto.Text = _divipolaDeptos.TryGetValue(depto, out string? codigo) ? codigo : string.Empty;
            }
        }

        private void ActualizarDivipolaMunicipio()
        {
            if (PckMunicipio.SelectedItem != null)
            {
                string municipio = PckMunicipio.SelectedItem.ToString() ?? string.Empty;
                TxtDivipolaMuni.Text = _divipolaMunicipios.TryGetValue(municipio, out string? codigo) ? codigo : string.Empty;
            }
        }

        private void ActualizarTipoEstablecimiento()
        {
            if (PckActividadPrincipal.SelectedItem != null)
            {
                string actividad = PckActividadPrincipal.SelectedItem.ToString() ?? string.Empty;
                if (_tiposEstablecimiento.TryGetValue(actividad, out var opciones))
                {
                    PckTipoEstablecimiento.ItemsSource = opciones;
                    if (opciones.Count > 0 && PckTipoEstablecimiento.SelectedIndex < 0)
                    {
                        PckTipoEstablecimiento.SelectedIndex = 0;
                    }
                }
            }
        }

        private void OnPorcentajeInicialTextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = e.NewTextValue;

            if (string.IsNullOrWhiteSpace(texto))
            {
                LblConceptoInicialAuto.Text = "PENDIENTE";
                return;
            }

            texto = texto.Replace(',', '.');

            if (double.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out double porcentaje))
            {
                if (porcentaje >= 0 && porcentaje <= 100)
                {
                    if (porcentaje >= 90)
                        LblConceptoInicialAuto.Text = "FAVORABLE";
                    else if (porcentaje >= 60)
                        LblConceptoInicialAuto.Text = "FAVORABLE CON REQUERIMIENTO";
                    else
                        LblConceptoInicialAuto.Text = "DESFAVORABLE";
                }
                else
                {
                    LblConceptoInicialAuto.Text = "PORCENTAJE FUERA DE RANGO (0-100)";
                }
            }
            else
            {
                LblConceptoInicialAuto.Text = "PORCENTAJE INVÁLIDO";
            }
        }

        public void OnObjetivoInicialSelectedIndexChanged(object sender, EventArgs e)
        {
            if (PckObjetivoInicial.SelectedItem != null)
            {
                string seleccion = PckObjetivoInicial.SelectedItem.ToString() ?? string.Empty;
                SeccionOtroObjetivoInicial.IsVisible = string.Equals(seleccion, "OTRO", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void OnToggleMedidasInicialClicked(object sender, EventArgs e)
        {
            SeccionDesplegableMedidasInicial.IsVisible = !SeccionDesplegableMedidasInicial.IsVisible;
        }

        private void OnMedidaInicialCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox chk && chk == ChkNoAplicaInicial && e.Value)
            {
                ChkClausuraTotalInicial.IsChecked = false;
                ChkClausuraParcialInicial.IsChecked = false;
                ChkSuspensionParcialInicial.IsChecked = false;
                ChkSuspensionTotalInicial.IsChecked = false;
                ChkDecomisoInicial.IsChecked = false;
                ChkDestruccionInicial.IsChecked = false;
                ChkDesnaturalizacionInicial.IsChecked = false;
                ChkOtraMedidaInicial.IsChecked = false;
                ChkCongelacionTemporalInicial.IsChecked = false;
                ChkSuspensionTemporalVentaInicial.IsChecked = false;
            }
            else if (sender is CheckBox chkOtra && chkOtra != ChkNoAplicaInicial && e.Value)
            {
                ChkNoAplicaInicial.IsChecked = false;
            }

            SeccionOtraMedidaInicial.IsVisible = ChkOtraMedidaInicial.IsChecked;
            ActualizarTextoResumenMedidasInicial();
        }

        private void ActualizarTextoResumenMedidasInicial()
        {
            var seleccionadas = new List<string>();

            if (ChkClausuraTotalInicial.IsChecked) seleccionadas.Add("CLAUSURA TEMPORAL TOTAL");
            if (ChkClausuraParcialInicial.IsChecked) seleccionadas.Add("CLAUSURA TEMPORAL PARCIAL");
            if (ChkSuspensionParcialInicial.IsChecked) seleccionadas.Add("SUSPENSIÓN PARCIAL");
            if (ChkSuspensionTotalInicial.IsChecked) seleccionadas.Add("SUSPENSIÓN TOTAL");
            if (ChkDecomisoInicial.IsChecked) seleccionadas.Add("DECOMISO");
            if (ChkDestruccionInicial.IsChecked) seleccionadas.Add("DESTRUCCIÓN");
            if (ChkDesnaturalizacionInicial.IsChecked) seleccionadas.Add("DESNATURALIZACIÓN");
            if (ChkCongelacionTemporalInicial.IsChecked) seleccionadas.Add("CONGELACIÓN TEMPORAL VENTA/EMPLEO");
            if (ChkSuspensionTemporalVentaInicial.IsChecked) seleccionadas.Add("SUSPENSIÓN TEMPORAL VENTA/EMPLEO");
            if (ChkOtraMedidaInicial.IsChecked) seleccionadas.Add("OTRA MEDIDA");

            if (seleccionadas.Count == 0 || ChkNoAplicaInicial.IsChecked)
            {
                LblMedidasInicialSeleccionadas.Text = "NO APLICA";
            }
            else
            {
                LblMedidasInicialSeleccionadas.Text = string.Join(", ", seleccionadas);
            }
        }

        public async void OnGuardarClicked(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(TxtNombreComercial.Text))
            {
                await DisplayAlert("Validación", "El Nombre Comercial es obligatorio.", "OK");
                return;
            }

            if (sender is Button btnGuardar) btnGuardar.IsEnabled = false;

            try
            {
                var est = _establecimientoParaEditar ?? new Establecimiento();

                // DATOS GENERALES
                est.NombreComercial = TxtNombreComercial.Text ?? "";
                est.RazonSocial = TxtRazonSocial.Text ?? "";
                est.TipoDocumento = PckTipoDocumento.SelectedItem?.ToString() ?? "";
                est.NitOCedula = TxtNumeroDocumento.Text ?? "";
                est.NumeroInscripcion = TxtInscripcion.Text ?? "";
                est.RepresentanteLegal = TxtRepresentante.Text ?? "";
                est.TipoSujeto = PckTipoSujeto.SelectedItem?.ToString() ?? "";
                est.Estado = PckEstado.SelectedItem?.ToString() ?? "";

                // UBICACIÓN Y CONTACTO
                est.Direccion = TxtDireccion.Text ?? "";
                est.Municipio = PckMunicipio.SelectedItem?.ToString() ?? "";
                est.CodigoDivipolaMunicipio = TxtDivipolaMuni.Text ?? "";
                est.Departamento = PckDepartamento.SelectedItem?.ToString() ?? "";
                est.CodigoDivipolaDpto = TxtDivipolaDpto.Text ?? "";
                est.Area = PckArea.SelectedItem?.ToString() ?? "";
                est.Telefono = TxtTelefono.Text ?? "";
                est.CorreoElectronico = TxtCorreo.Text ?? "";

                // CLASIFICACIÓN
                est.ActividadPrincipal = PckActividadPrincipal.SelectedItem?.ToString() ?? "";
                est.TipoEstablecimiento = PckTipoEstablecimiento.SelectedItem?.ToString() ?? "";
                est.TipoRiesgo = PckRiesgo.SelectedItem?.ToString() ?? "";
                est.AutorizacionSanitariaDecreto1500 = PckDec1500.SelectedItem?.ToString() ?? "";
                est.NumeroFechaResolucionAutorizacion = TxtResAutorizacion.Text ?? "";

                est.PorcentajeCumplimiento = TxtPorcentajeInicial.Text ?? "0";
                est.ConceptoSanitario = LblConceptoInicialAuto.Text ?? "PENDIENTE";

                // 1. GUARDAR EN SUPABASE
                await _dbService.GuardarEstablecimientoAsync(est);

                // 2. CREAR Y GUARDAR LA PRIMERA VISITA (solo si es registro nuevo)
                if (_establecimientoParaEditar == null)
                {
                    var medidasSeleccionadasBD = new List<string>();

                    if (ChkClausuraTotalInicial.IsChecked) medidasSeleccionadasBD.Add("CLAUSURA TEMPORAL TOTAL");
                    if (ChkClausuraParcialInicial.IsChecked) medidasSeleccionadasBD.Add("CLAUSURA TEMPORAL PARCIAL");
                    if (ChkSuspensionParcialInicial.IsChecked) medidasSeleccionadasBD.Add("SUSPENSIÓN PARCIAL DE TRABAJOS O SERVICIOS");
                    if (ChkSuspensionTotalInicial.IsChecked) medidasSeleccionadasBD.Add("SUSPENSIÓN TOTAL DE TRABAJOS O SERVICIOS");
                    if (ChkDecomisoInicial.IsChecked) medidasSeleccionadasBD.Add("DECOMISO DE OBJETOS Y PRODUCTOS");
                    if (ChkDestruccionInicial.IsChecked) medidasSeleccionadasBD.Add("DESTRUCCIÓN DE ARTICULOS O PRODUCTOS");
                    if (ChkDesnaturalizacionInicial.IsChecked) medidasSeleccionadasBD.Add("DESNATURALIZACIÓN DE ARTICULOS O PRODUCTOS");
                    if (ChkCongelacionTemporalInicial.IsChecked)
                        medidasSeleccionadasBD.Add("CONGELACIÓN TEMPORAL DE LA VENTA O EMPLEO DE PRODUCTOS Y OBJETOS");

                    if (ChkSuspensionTemporalVentaInicial.IsChecked)
                        medidasSeleccionadasBD.Add("SUSPENSIÓN TEMPORAL DE LA VENTA O EMPLEO DE PRODUCTOS Y OBJETOS");

                    if (ChkOtraMedidaInicial.IsChecked)
                    {
                        string textoOtra = !string.IsNullOrWhiteSpace(TxtOtraMedidaInicial.Text)
                            ? $"OTRA MEDIDA: {TxtOtraMedidaInicial.Text.Trim()}"
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

                    // 2. Crear el objeto Visita con las URLs generadas
                    var primeraVisita = new Visita
                    {
                        EstablecimientoId = est.Id,
                        NumeroVisita = 1,
                        FechaVisita = $"{DpFechaVisitaInicial.Date:dd/MM/yyyy}",
                        PorcentajeCumplimiento = TxtPorcentajeInicial.Text ?? "0",
                        ConceptoSanitario = LblConceptoInicialAuto.Text ?? "PENDIENTE",
                        FuncionariosVisita = TxtFuncionariosInicial.Text ?? "",
                        ObjetivoVisita = PckObjetivoInicial.SelectedItem?.ToString() ?? "",
                        Denuncias = PckDenunciasInicial.SelectedItem?.ToString() ?? "NO",
                        Eta = PckEtaInicial.SelectedItem?.ToString() ?? "NO",
                        TieneMedidaSanitaria = "NO",
                        TipoMedidaSanitaria = "NO APLICA",
                        DocumentoAdjunto = urlsAdjuntas, // <-- Se guardan las URLs reales
                        Observaciones = TxtObservacionesInicial.Text ?? ""
                    };

                    await _dbService.GuardarVisitaAsync(primeraVisita);
                }

                await DisplayAlert("Éxito", "Establecimiento guardado correctamente en Supabase.", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error al Guardar", ex.Message, "OK");
            }
            finally
            {
                if (sender is Button btnVolver) btnVolver.IsEnabled = true;
            }
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
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