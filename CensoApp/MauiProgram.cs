using CensoApp.Services;
using CensoApp.Views;

namespace CensoApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            SQLitePCL.Batteries_V2.Init();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registro de Servicios y Vistas para Inyección de Dependencias
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<FormularioPage>();
            builder.Services.AddTransient<DetallePage>();
            builder.Services.AddTransient<NuevaVisitaPage>();

            return builder.Build();
        }
    }
}