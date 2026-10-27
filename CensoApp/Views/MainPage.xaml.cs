using CensoApp.Models;
using CensoApp.Services;

namespace CensoApp.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private List<Establecimiento> _listaCompleta = new();

        public MainPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            InicializarFiltros();
        }

        public MainPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            InicializarFiltros();
        }

        private void InicializarFiltros()
        {
            if (PickerFiltroConcepto != null) PickerFiltroConcepto.SelectedIndex = 0;
            if (PickerFiltroRiesgo != null) PickerFiltroRiesgo.SelectedIndex = 0;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarEstablecimientos();
        }

        private async Task CargarEstablecimientos()
        {
            try
            {
                _listaCompleta = await _dbService.ObtenerEstablecimientosAsync();
                CargarPickerMunicipios();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error al conectar con Supabase", ex.Message, "OK");
            }
        }

        private void CargarPickerMunicipios()
        {
            string seleccionPrevia = PickerFiltroMunicipio.SelectedItem?.ToString() ?? "TODOS";
            var listaMunicipios = new List<string> { "TODOS" };

            if (_listaCompleta != null && _listaCompleta.Count > 0)
            {
                var municipiosRegistrados = _listaCompleta
                    .Where(e => !string.IsNullOrWhiteSpace(e.Municipio))
                    .Select(e => e.Municipio.Trim().ToUpper())
                    .Distinct()
                    .OrderBy(m => m);

                listaMunicipios.AddRange(municipiosRegistrados);
            }

            PickerFiltroMunicipio.ItemsSource = null;
            PickerFiltroMunicipio.ItemsSource = listaMunicipios;

            if (listaMunicipios.Contains(seleccionPrevia))
                PickerFiltroMunicipio.SelectedItem = seleccionPrevia;
            else
                PickerFiltroMunicipio.SelectedIndex = 0;
        }

        private void OnFiltrosChanged(object sender, EventArgs e) => AplicarFiltros();

        private void AplicarFiltros()
        {
            if (_listaCompleta == null) return;
            var resultado = _listaCompleta.AsEnumerable();

            if (PickerFiltroMunicipio.SelectedIndex > 0)
            {
                string mpio = PickerFiltroMunicipio.SelectedItem?.ToString() ?? "";
                if (!mpio.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                {
                    resultado = resultado.Where(e => e.Municipio != null &&
                        e.Municipio.Equals(mpio, StringComparison.OrdinalIgnoreCase));
                }
            }

            string texto = TxtBuscar.Text?.Trim().ToLower() ?? string.Empty;
            if (!string.IsNullOrEmpty(texto))
            {
                resultado = resultado.Where(e =>
                    (!string.IsNullOrEmpty(e.NombreComercial) && e.NombreComercial.ToLower().Contains(texto)) ||
                    (!string.IsNullOrEmpty(e.RazonSocial) && e.RazonSocial.ToLower().Contains(texto)) ||
                    (!string.IsNullOrEmpty(e.NitOCedula) && e.NitOCedula.ToLower().Contains(texto)) ||
                    (!string.IsNullOrEmpty(e.RepresentanteLegal) && e.RepresentanteLegal.ToLower().Contains(texto))
                );
            }

            if (PickerFiltroConcepto.SelectedIndex > 0)
            {
                string concepto = PickerFiltroConcepto.SelectedItem?.ToString() ?? "";
                if (!concepto.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                {
                    string conceptoLimpio = concepto.TrimEnd('S', 's');

                    resultado = resultado.Where(e => e.ConceptoSanitario != null &&
                        e.ConceptoSanitario.TrimEnd('S', 's').Equals(conceptoLimpio, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (PickerFiltroRiesgo.SelectedIndex > 0)
            {
                string riesgo = PickerFiltroRiesgo.SelectedItem?.ToString() ?? "";
                if (!riesgo.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                {
                    resultado = resultado.Where(e => e.TipoRiesgo != null &&
                        e.TipoRiesgo.Equals(riesgo, StringComparison.OrdinalIgnoreCase));
                }
            }

            var listaFiltrada = resultado.ToList();
            ListaEstablecimientos.ItemsSource = listaFiltrada;
            LblTotalRegistros.Text = $"Mostrando {listaFiltrada.Count} de {_listaCompleta.Count} registros";
        }

        private async void OnExportarClicked(object sender, EventArgs e)
        {
            if (_listaCompleta == null || _listaCompleta.Count == 0)
            {
                await DisplayAlert("Sin datos", "No hay registros guardados en la base de datos para exportar.", "OK");
                return;
            }

            try
            {
                var listaVisitas = await _dbService.ObtenerTodasLasVisitasAsync();
                string rutaArchivo = await ExportService.ExportarAExcelAsync(_listaCompleta, listaVisitas);

#if WINDOWS
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(rutaArchivo)
                });
#else
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Exportar Base de Datos Censo IVC",
                    File = new ShareFile(rutaArchivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                });
#endif
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error de Exportación", $"Ocurrió un detalle al exportar: {ex.Message}", "OK");
            }
        }

        private async void OnEstablecimientoSeleccionado(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Establecimiento seleccionado)
            {
                // Deselecciona el ítem para que no quede marcado en gris al regresar
                ListaEstablecimientos.SelectedItem = null;

                // Redirige a la página de detalle enviando la base de datos y el ítem seleccionado
                await Navigation.PushAsync(new DetallePage(_dbService, seleccionado));
            }
        }

        private async void OnNuevoClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FormularioPage(_dbService));
        }
    }
}