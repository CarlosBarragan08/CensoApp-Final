using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace CensoApp.Models
{
    [Table("visitas")]
    public class Visita : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        public int Id { get; set; }

        [Column("establecimiento_id")]
        public int EstablecimientoId { get; set; }

        [Column("numero_visita")]
        public int NumeroVisita { get; set; } = 1;

        [Column("fecha_visita")]
        public string FechaVisita { get; set; } = string.Empty;

        [Column("porcentaje_cumplimiento")]
        public string PorcentajeCumplimiento { get; set; } = "0";

        [Column("concepto_sanitario")]
        public string ConceptoSanitario { get; set; } = "SIN VISITA RECIENTE";

        [Column("funcionarios_visita")]
        public string FuncionariosVisita { get; set; } = string.Empty;

        [Column("objetivo_visita")]
        public string ObjetivoVisita { get; set; } = "PROGRAMACION";

        [Column("denuncias")]
        public string Denuncias { get; set; } = "NO";

        [Column("eta")]
        public string Eta { get; set; } = "NO";

        [Column("tiene_medida_sanitaria")]
        public string TieneMedidaSanitaria { get; set; } = "NO";

        [Column("tipo_medida_sanitaria")]
        public string TipoMedidaSanitaria { get; set; } = string.Empty;

        [Column("observaciones")]
        public string Observaciones { get; set; } = string.Empty;

        [Column("documento_adjunto")]
        public string DocumentoAdjunto { get; set; } = string.Empty;
    }
}