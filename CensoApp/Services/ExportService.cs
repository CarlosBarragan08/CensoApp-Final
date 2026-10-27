using ClosedXML.Excel;
using CensoApp.Models;
using System.Globalization;

namespace CensoApp.Services
{
    public static class ExportService
    {
        public static async Task<string> ExportarAExcelAsync(List<Establecimiento> establecimientos, List<Visita> visitas)
        {
            return await Task.Run(() =>
            {
                using (var workbook = new XLWorkbook())
                {
                    // PESTAÑA 1: DIRECTORIO DE ESTABLECIMIENTOS
                    var sheetEst = workbook.Worksheets.Add("Establecimientos");
                    sheetEst.ShowGridLines = true;

                    sheetEst.Cell("A1").Value = "DIRECTORIO GENERAL DE CENSO IVC";
                    sheetEst.Cell("A1").Style.Font.Bold = true;
                    sheetEst.Cell("A1").Style.Font.FontSize = 16;
                    sheetEst.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#1E293B");

                    sheetEst.Cell("A2").Value = $"Secretaría de Salud - Reporte generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
                    sheetEst.Cell("A2").Style.Font.Italic = true;
                    sheetEst.Cell("A2").Style.Font.FontSize = 10;
                    sheetEst.Cell("A2").Style.Font.FontColor = XLColor.FromHtml("#64748B");

                    string[] headersEst = {
                        "N° Inscripción", "Tipo Documento", "N° Documento", "Nombre Comercial", "Razón Social",
                        "Representante Legal", "Departamento", "Cod. Dpto", "Municipio", "Cod. Mpio",
                        "Área", "Dirección", "Teléfono", "Correo Electrónico", "Tipo Sujeto",
                        "Tipo Establecimiento", "Estado", "Aut. Dec. 1500", "Res. Autorización",
                        "Concepto Sanitario", "% Cumplimiento", "Nivel Riesgo"
                    };

                    for (int col = 0; col < headersEst.Length; col++)
                    {
                        var celda = sheetEst.Cell(4, col + 1);
                        celda.Value = headersEst[col];
                        celda.Style.Font.Bold = true;
                        celda.Style.Font.FontColor = XLColor.White;
                        celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
                        celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                    sheetEst.Row(4).Height = 28;

                    int filaEst = 5;
                    foreach (var item in establecimientos)
                    {
                        sheetEst.Cell(filaEst, 1).Value = item.NumeroInscripcion;
                        sheetEst.Cell(filaEst, 2).Value = item.TipoDocumento;
                        sheetEst.Cell(filaEst, 3).Value = item.NitOCedula;
                        sheetEst.Cell(filaEst, 4).Value = item.NombreComercial;
                        sheetEst.Cell(filaEst, 5).Value = item.RazonSocial;
                        sheetEst.Cell(filaEst, 6).Value = item.RepresentanteLegal;
                        sheetEst.Cell(filaEst, 7).Value = item.Departamento;
                        sheetEst.Cell(filaEst, 8).Value = item.CodigoDivipolaDpto;
                        sheetEst.Cell(filaEst, 9).Value = item.Municipio;
                        sheetEst.Cell(filaEst, 10).Value = item.CodigoDivipolaMunicipio;
                        sheetEst.Cell(filaEst, 11).Value = item.Area;
                        sheetEst.Cell(filaEst, 12).Value = item.Direccion;
                        sheetEst.Cell(filaEst, 13).Value = item.Telefono;
                        sheetEst.Cell(filaEst, 14).Value = item.CorreoElectronico;
                        sheetEst.Cell(filaEst, 15).Value = item.TipoSujeto;
                        sheetEst.Cell(filaEst, 16).Value = item.TipoEstablecimiento;
                        sheetEst.Cell(filaEst, 17).Value = item.Estado;
                        sheetEst.Cell(filaEst, 18).Value = item.AutorizacionSanitariaDecreto1500;
                        sheetEst.Cell(filaEst, 19).Value = item.NumeroFechaResolucionAutorizacion;

                        FormatearCeldaConcepto(sheetEst.Cell(filaEst, 20), item.ConceptoSanitario);
                        FormatearCeldaPorcentaje(sheetEst.Cell(filaEst, 21), item.PorcentajeCumplimiento);
                        FormatearCeldaRiesgo(sheetEst.Cell(filaEst, 22), item.TipoRiesgo);

                        int[] colsCentradas = { 1, 2, 6, 7, 8, 9, 10, 12, 16, 17, 19, 21 };
                        foreach (int c in colsCentradas)
                            sheetEst.Cell(filaEst, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        if (filaEst % 2 == 0)
                        {
                            for (int c = 1; c <= 21; c++)
                                if (c != 19 && c != 21) sheetEst.Cell(filaEst, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
                        }
                        sheetEst.Row(filaEst).Height = 22;
                        filaEst++;
                    }

                    var rangoEst = sheetEst.Range(4, 1, Math.Max(filaEst - 1, 4), 21);
                    AplicarBordesTabla(rangoEst);
                    sheetEst.Columns().AdjustToContents();

                    // PESTAÑA 2: HISTORIAL DE VISITAS
                    var sheetVis = workbook.Worksheets.Add("Historial de Visitas");
                    sheetVis.ShowGridLines = true;

                    sheetVis.Cell("A1").Value = "HISTORIAL COMPLETO DE VISITAS DE INSPECCIÓN (IVC)";
                    sheetVis.Cell("A1").Style.Font.Bold = true;
                    sheetVis.Cell("A1").Style.Font.FontSize = 16;
                    sheetVis.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#1E293B");

                    sheetVis.Cell("A2").Value = $"Total de visitas registradas en sistema: {visitas.Count}";
                    sheetVis.Cell("A2").Style.Font.Italic = true;
                    sheetVis.Cell("A2").Style.Font.FontSize = 10;
                    sheetVis.Cell("A2").Style.Font.FontColor = XLColor.FromHtml("#64748B");

                    string[] headersVis = {
                        "Establecimiento", "N° Visita", "Fecha Visita",
                        "Concepto Sanitario", "% Cumplimiento", "Funcionarios", "Objetivo",
                        "Denuncias", "ETA", "Tiene Medida", "Tipo Medida", "Observaciones"
                    };

                    for (int col = 0; col < headersVis.Length; col++)
                    {
                        var celda = sheetVis.Cell(4, col + 1);
                        celda.Value = headersVis[col];
                        celda.Style.Font.Bold = true;
                        celda.Style.Font.FontColor = XLColor.White;
                        celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
                        celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                    sheetVis.Row(4).Height = 28;

                    int filaVis = 5;
                    foreach (var v in visitas)
                    {
                        var estabRelacionado = establecimientos.FirstOrDefault(e => e.Id == v.EstablecimientoId);

                        sheetVis.Cell(filaVis, 1).Value = estabRelacionado?.NombreComercial ?? "N/A";
                        sheetVis.Cell(filaVis, 2).Value = v.NumeroVisita;
                        sheetVis.Cell(filaVis, 3).Value = v.FechaVisita;

                        FormatearCeldaConcepto(sheetVis.Cell(filaVis, 4), v.ConceptoSanitario);
                        FormatearCeldaPorcentaje(sheetVis.Cell(filaVis, 5), v.PorcentajeCumplimiento);

                        sheetVis.Cell(filaVis, 6).Value = v.FuncionariosVisita;
                        sheetVis.Cell(filaVis, 7).Value = v.ObjetivoVisita;
                        sheetVis.Cell(filaVis, 8).Value = v.Denuncias;
                        sheetVis.Cell(filaVis, 9).Value = v.Eta;
                        sheetVis.Cell(filaVis, 10).Value = v.TieneMedidaSanitaria;
                        sheetVis.Cell(filaVis, 11).Value = v.TipoMedidaSanitaria;
                        sheetVis.Cell(filaVis, 12).Value = v.Observaciones;

                        int[] colsCentradasVis = { 2, 3, 4, 7, 8, 9, 10 };
                        foreach (int c in colsCentradasVis)
                            sheetVis.Cell(filaVis, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        if (filaVis % 2 == 0)
                        {
                            for (int c = 1; c <= 12; c++)
                                if (c != 4) sheetVis.Cell(filaVis, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
                        }
                        sheetVis.Row(filaVis).Height = 22;
                        filaVis++;
                    }

                    var rangoVis = sheetVis.Range(4, 1, Math.Max(filaVis - 1, 4), 12);
                    AplicarBordesTabla(rangoVis);
                    sheetVis.Columns().AdjustToContents();

                    string nombreArchivo = $"Censo_IVC_Completo_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    string rutaCompleta = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);
                    workbook.SaveAs(rutaCompleta);
                    return rutaCompleta;
                }
            });
        }

        private static void FormatearCeldaConcepto(IXLCell celda, string? conceptoRaw)
        {
            celda.Value = conceptoRaw;
            string concepto = conceptoRaw?.ToUpper() ?? "";
            if (concepto.Contains("FAVORABLE") && !concepto.Contains("REQUERIMIENTOS"))
            {
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCFCE7");
                celda.Style.Font.FontColor = XLColor.FromHtml("#15803D");
                celda.Style.Font.Bold = true;
            }
            else if (concepto.Contains("DESFAVORABLE"))
            {
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
                celda.Style.Font.FontColor = XLColor.FromHtml("#B91C1C");
                celda.Style.Font.Bold = true;
            }
            else
            {
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
                celda.Style.Font.FontColor = XLColor.FromHtml("#475569");
            }
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private static void FormatearCeldaPorcentaje(IXLCell celda, string? pctRaw)
        {
            // Reemplaza comas por puntos para asegurar que parsee bien los decimales
            string pctTexto = pctRaw?.ToString().Replace("%", "").Replace(",", ".").Trim() ?? "0";

            if (double.TryParse(pctTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out double pctVal))
            {
                celda.Value = pctVal > 1 ? pctVal / 100.0 : pctVal;
            }
            else
            {
                celda.Value = 0;
            }

            // Se cambia de "0%" a "0.0%" (o "0.#%" para mostrar decimales solo cuando existan)
            celda.Style.NumberFormat.Format = "0.0%";
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private static void FormatearCeldaRiesgo(IXLCell celda, string? riesgoRaw)
        {
            celda.Value = riesgoRaw;
            string riesgo = riesgoRaw?.ToUpper() ?? "";
            if (riesgo == "ALTO")
            {
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
                celda.Style.Font.FontColor = XLColor.FromHtml("#991818");
                celda.Style.Font.Bold = true;
            }
            else if (riesgo == "MEDIO")
            {
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
                celda.Style.Font.FontColor = XLColor.FromHtml("#92400E");
                celda.Style.Font.Bold = true;
            }
            else if (riesgo == "BAJO")
            {
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCFCE7");
                celda.Style.Font.FontColor = XLColor.FromHtml("#166534");
                celda.Style.Font.Bold = true;
            }
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private static void AplicarBordesTabla(IXLRange rango)
        {
            rango.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rango.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            rango.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
            rango.Style.Border.InsideBorderColor = XLColor.FromHtml("#E2E8F0");
        }
    }
}