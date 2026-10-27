using Supabase;
using CensoApp.Models;

namespace CensoApp.Services
{
    public class DatabaseService
    {
        private Client? _supabase;
        private bool _isInitialized = false;

        private const string SupabaseUrl = "https://cpyapbjrbxewpcibazuh.supabase.co";

        // IMPORTANTE: Recuerda pegar tu clave 'anon' 'public' completa aquí
        private const string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImNweWFwYmpyYnhld3BjaWJhenVoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3OTA1NDA5NDgsImV4cCI6MjEwNjExNjk0OH0.zu6TFZXBLWYuKB3EGaw5DAbLwuH2MLI3h7r1D9rVMew";

        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized && _supabase != null) return;

            var options = new SupabaseOptions
            {
                // ÚNICO CAMBIO: Desactivado para evitar la conexión obligatoria de WebSockets.
                // Permite que la app abra instantáneamente mediante HTTPS sin ser bloqueada por el Firewall.
                AutoConnectRealtime = false
            };

            _supabase = new Client(SupabaseUrl, SupabaseKey, options);
            await _supabase.InitializeAsync();
            _isInitialized = true;
        }

        public async Task GuardarEstablecimientoAsync(Establecimiento est)
        {
            await EnsureInitializedAsync();

            if (est.Id == 0)
            {
                var response = await _supabase!
                    .From<Establecimiento>()
                    .Insert(est);

                var nuevo = response.Models.FirstOrDefault();
                if (nuevo != null)
                {
                    est.Id = nuevo.Id;
                }
            }
            else
            {
                await _supabase!
                    .From<Establecimiento>()
                    .Update(est);
            }
        }

        public async Task GuardarVisitaAsync(Visita visita)
        {
            await EnsureInitializedAsync();

            if (visita.Id == 0)
            {
                await _supabase!
                    .From<Visita>()
                    .Insert(visita);
            }
            else
            {
                await _supabase!
                    .From<Visita>()
                    .Update(visita);
            }
        }

        public async Task<List<Establecimiento>> ObtenerEstablecimientosAsync()
        {
            await EnsureInitializedAsync();

            var response = await _supabase!
                .From<Establecimiento>()
                .Get();

            return response.Models ?? new List<Establecimiento>();
        }

        public async Task<List<Visita>> ObtenerTodasLasVisitasAsync()
        {
            await EnsureInitializedAsync();

            var response = await _supabase!
                .From<Visita>()
                .Get();

            return response.Models ?? new List<Visita>();
        }

        public async Task<List<Visita>> ObtenerVisitasPorEstablecimientoAsync(int establecimientoId)
        {
            await EnsureInitializedAsync();

            var response = await _supabase!
                .From<Visita>()
                .Where(v => v.EstablecimientoId == establecimientoId)
                .Get();

            return response.Models ?? new List<Visita>();
        }

        public async Task EliminarEstablecimientoAsync(Establecimiento est)
        {
            await EnsureInitializedAsync();

            await _supabase!
                .From<Establecimiento>()
                .Where(e => e.Id == est.Id)
                .Delete();
        }

        public async Task EliminarVisitaAsync(Visita visita)
        {
            await EnsureInitializedAsync();

            await _supabase!
                .From<Visita>()
                .Where(v => v.Id == visita.Id)
                .Delete();
        }

        public async Task<string> SubirArchivoAsync(FileResult archivo)
        {
            await EnsureInitializedAsync();

            // Genera un nombre único con fecha y hora para evitar sobrescribir archivos con el mismo nombre
            string extension = Path.GetExtension(archivo.FileName);
            string nombreLimpio = Path.GetFileNameWithoutExtension(archivo.FileName).Replace(" ", "_");
            string nombreUnico = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 5)}_{nombreLimpio}{extension}";

            // Lee los bytes del archivo seleccionado
            using var stream = await archivo.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] bytes = ms.ToArray();

            // Sube el archivo a la carpeta 'documentos' en Supabase Storage
            await _supabase!.Storage
                .From("documentos")
                .Upload(bytes, nombreUnico);

            // Retorna la URL pública directa para abrir o descargar el archivo
            return _supabase.Storage
                .From("documentos")
                .GetPublicUrl(nombreUnico);
        }
    }
}