using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CensoApp.Models
{
    [Table("establecimientos")]
    public class Establecimiento : BaseModel
    {
        [PrimaryKey("id", false)] // <-- 'false' es indispensable para que C# NO envíe 'id: 0' al insertar
        public int Id { get; set; }

        [Column("nombre_comercial")]
        public string NombreComercial { get; set; } = string.Empty;

        [Column("razon_social")]
        public string RazonSocial { get; set; } = string.Empty;

        [Column("tipo_sujeto")]
        public string TipoSujeto { get; set; } = "C.C.";

        [Column("nit_o_cedula")]
        public string NitOCedula { get; set; } = string.Empty;

        [Column("numero_inscripcion")]
        public string NumeroInscripcion { get; set; } = string.Empty;

        [Column("estado")]
        public string Estado { get; set; } = "ACTIVO";

        [Column("representante_legal")]
        public string RepresentanteLegal { get; set; } = string.Empty;

        [Column("direccion")]
        public string Direccion { get; set; } = string.Empty;

        [Column("municipio")]
        public string Municipio { get; set; } = string.Empty;

        [Column("codigo_divipola_municipio")]
        public string CodigoDivipolaMunicipio { get; set; } = string.Empty;

        [Column("departamento")]
        public string Departamento { get; set; } = string.Empty;

        [Column("codigo_divipola_dpto")]
        public string CodigoDivipolaDpto { get; set; } = string.Empty;

        [Column("area")]
        public string Area { get; set; } = "URBANA";

        [Column("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [Column("correo_electronico")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Column("tipo_establecimiento")]
        public string TipoEstablecimiento { get; set; } = string.Empty;

        [Column("tipo_riesgo")]
        public string TipoRiesgo { get; set; } = "SIN CLASIFICAR";

        [Column("concepto_sanitario")]
        public string ConceptoSanitario { get; set; } = "SIN VISITA RECIENTE";

        [Column("porcentaje_cumplimiento")]
        public string PorcentajeCumplimiento { get; set; } = "0";

        [Column("autorizacion_sanitaria_decreto_1500")]
        public string AutorizacionSanitariaDecreto1500 { get; set; } = "NO APLICA";

        [Column("numero_fecha_resolucion_autorizacion")]
        public string NumeroFechaResolucionAutorizacion { get; set; } = string.Empty;

        [Column("actividad_principal")]
        public string? ActividadPrincipal { get; set; }

        [Column("tipo_documento")]
        public string? TipoDocumento { get; set; }
    }
}