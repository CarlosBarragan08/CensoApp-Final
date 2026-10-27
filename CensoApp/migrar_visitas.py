import pandas as pd
import numpy as np
import re
from supabase import create_client

# ==============================================================================
# 1. CONFIGURACIÓN DE SUPABASE
# ==============================================================================
SUPABASE_URL = "https://cpyapbjrbxewpcibazuh.supabase.co"
SUPABASE_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImNweWFwYmpyYnhld3BjaWJhenVoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3OTA1NDA5NDgsImV4cCI6MjEwNjExNjk0OH0.zu6TFZXBLWYuKB3EGaw5DAbLwuH2MLI3h7r1D9rVMew"  # Tu service_role key

supabase = create_client(SUPABASE_URL, SUPABASE_KEY)


# ==============================================================================
# 2. DICCIONARIOS DE HOMOLOGACIÓN
# ==============================================================================
DICCIONARIO_CONCEPTO = {
    "FABORABLE CO REQUERIMIENTOS": "FAVORABLE CON REQUERIMIENTO",
    "FABORABLE CON REQUERIMIENTOS": "FAVORABLE CON REQUERIMIENTO",
    "FFABORABLE CON REQUERIMIENTOS": "FAVORABLE CON REQUERIMIENTO",
    "FAVPRABLE CON REQUERIMIENTOS": "FAVORABLE CON REQUERIMIENTO",
    "FAVOTABLE CON REQUERIMIENTO": "FAVORABLE CON REQUERIMIENTO",
    "FAVORABLE CON RQUERIMIENTOS": "FAVORABLE CON REQUERIMIENTO",
    "FAVORABLE CON REQUERIMEINTOS": "FAVORABLE CON REQUERIMIENTO",
    "FAVORABLE CON REQUERIMIENTOS": "FAVORABLE CON REQUERIMIENTO",
    "FAVORABLE CON REQUERIMIENTO": "FAVORABLE CON REQUERIMIENTO",
    "FABORABLE CON REQUERIMIENTO": "FAVORABLE CON REQUERIMIENTO",
    "FABORABLE": "FAVORABLE",
    "FAVORABLE": "FAVORABLE",
    "DESFAVORABLE": "DESFAVORABLE"
}


# ==============================================================================
# 3. FUNCIONES DE LIMPIEZA Y FORMATEO
# ==============================================================================
def obtener_valor_escalar(val):
    if isinstance(val, pd.Series):
        val_limpio = val.dropna()
        return val_limpio.iloc[0] if not val_limpio.empty else None
    return val

def extraer_nit_raiz(val):
    val = obtener_valor_escalar(val)
    if val is None or pd.isna(val):
        return ""
    val_str = str(val).split('-')[0].strip()
    if val_str.upper() in ['NONE', 'NAN', '']:
        return ""
    digitos = re.sub(r'\D', '', val_str)
    if len(digitos) == 10 and digitos.startswith(('8', '9')):
        return digitos[:9]
    return digitos

def limpiar_texto(val):
    val = obtener_valor_escalar(val)
    if val is None or pd.isna(val):
        return ""
    val_str = str(val).replace('\xa0', ' ').strip()
    val_str = re.sub(r'\s+', ' ', val_str)
    return val_str.upper()

def corregir_concepto(val_texto):
    if not val_texto:
        return "SIN CONCEPTO"
    for clave, correcto in DICCIONARIO_CONCEPTO.items():
        if clave in val_texto:
            return correcto
    return val_texto

def limpiar_porcentaje(val_texto):
    if not val_texto:
        return ""
    val_str = str(val_texto).replace('%', '').replace(',', '.').strip()
    if val_str.lower() in ['nan', 'none', '']:
        return ""
    try:
        num = float(val_str)
        if pd.isna(num):
            return ""
        if 0 < num <= 1.0:
            num = num * 100
        return str(int(num)) if num == int(num) else str(round(num, 1))
    except ValueError:
        return ""

def formatear_fecha(val_texto):
    if not val_texto:
        return ""
    val_str = str(val_texto).strip()
    if not val_str or val_str.lower() in ['nat', 'nan', 'none']:
        return ""
    if "00:00:00" in val_str:
        val_str = val_str.split(" ")[0]
    try:
        dt = pd.to_datetime(val_str, dayfirst=True, errors='coerce')
        if pd.notna(dt):
            return dt.strftime('%Y-%m-%d')
    except Exception:
        pass
    return val_str

def generar_claves(nit_raw, nombre_raw, inscripcion_raw=""):
    nit_raiz = extraer_nit_raiz(nit_raw)
    nombre = limpiar_texto(nombre_raw)
    inscripcion = limpiar_texto(inscripcion_raw)
    
    claves = []
    if nit_raiz and nombre:
        claves.append(f"{nit_raiz}_{nombre}")
    if inscripcion:
        claves.append(f"INSC_{inscripcion}")
    if nombre:
        claves.append(f"NOM_{nombre}")
    return claves


# ==============================================================================
# 4. MAPA DE ESTABLECIMIENTOS EN SUPABASE
# ==============================================================================
print("Consultando establecimientos en Supabase...")

mapa_establecimientos = {}
offset = 0
limit = 1000

while True:
    res = supabase.table("establecimientos").select("id, nit_o_cedula, nombre_comercial, numero_inscripcion").range(offset, offset + limit - 1).execute()
    if not res.data:
        break
    for item in res.data:
        est_id = item.get("id")
        nit = item.get("nit_o_cedula")
        nombre = item.get("nombre_comercial")
        insc = item.get("numero_inscripcion")
        
        if est_id:
            for c in generar_claves(nit, nombre, insc):
                mapa_establecimientos[c] = est_id

    if len(res.data) < limit:
        break
    offset += limit

print(f"✔ Mapa generado con {len(mapa_establecimientos)} claves únicas.")


# ==============================================================================
# 5. BLOQUES HORIZONTALES EN ORDEN CRONOLÓGICO (DE MÁS ANTIGUO A MÁS RECIENTE)
# ==============================================================================
BLOQUES_CRONOLOGICOS = [
    {
        "nombre_bloque": "ULTIMA VISITA",
        "col_fecha": 53,
        "col_pct": 54,
        "col_concepto": 55,
        "col_func": None,
        "col_obj": 56
    },
    {
        "nombre_bloque": "ULTIMA ANTEPENULTIMA VISITA",
        "col_fecha": 40,
        "col_pct": 41,
        "col_concepto": 42,
        "col_func": None,
        "col_obj": 43
    },
    {
        "nombre_bloque": "PENULTIMA VISITA",
        "col_fecha": 34,
        "col_pct": 35,
        "col_concepto": 36,
        "col_func": None,
        "col_obj": 37
    },
    {
        "nombre_bloque": "VISITA ANTEPENULTIMA",
        "col_fecha": 28,
        "col_pct": 29,
        "col_concepto": 30,
        "col_func": None,
        "col_obj": 31
    },
    {
        "nombre_bloque": "VISITA ANTERIOR",
        "col_fecha": 21,
        "col_pct": 22,
        "col_concepto": 23,
        "col_func": 24,
        "col_obj": 25
    },
    {
        "nombre_bloque": "VISITA ACTUAL",
        "col_fecha": 16,
        "col_pct": 17,
        "col_concepto": 18,
        "col_func": 19,
        "col_obj": 20
    }
]


# ==============================================================================
# 6. EXTRACCIÓN Y HERENCIA DE FUNCIONARIOS
# ==============================================================================
archivo_excel = "CENSO ALIMENTOS Y BEBIDAS - ACTUALIZADO (1) - copia (1).xlsx"

print("Cargando matriz Excel para procesar historial ordenado...")
df = pd.read_excel(archivo_excel, sheet_name="CENSO")

visitas_a_insertar = []

for index, row in df.iterrows():
    nit_raw = row.iloc[8]      # Columna I
    nombre_raw = row.iloc[5]   # Columna F
    insc_raw = row.iloc[6]     # Columna G
    
    claves_busqueda = generar_claves(nit_raw, nombre_raw, insc_raw)
    est_id = None
    for c in claves_busqueda:
        if c in mapa_establecimientos:
            est_id = mapa_establecimientos[c]
            break
            
    if not est_id:
        continue

    # Funcionario principal de la fila (Columna 19) para usar como herencia
    func_principal = limpiar_texto(row.iloc[19]) if len(row) > 19 else ""
    obs_fila = limpiar_texto(row.iloc[52]) if len(row) > 52 else ""

    numero_visita_contador = 0

    # Recorrer bloques desde el más antiguo hasta la Visita Actual
    for bloque in BLOQUES_CRONOLOGICOS:
        fecha = formatear_fecha(limpiar_texto(row.iloc[bloque["col_fecha"]])) if len(row) > bloque["col_fecha"] else ""
        pct = limpiar_porcentaje(limpiar_texto(row.iloc[bloque["col_pct"]])) if len(row) > bloque["col_pct"] else ""
        concepto_raw = limpiar_texto(row.iloc[bloque["col_concepto"]]) if len(row) > bloque["col_concepto"] else ""
        concepto = corregir_concepto(concepto_raw)
        
        # Evaluar funcionario específico o heredar el funcionario principal
        func_especifico = ""
        if bloque["col_func"] is not None and len(row) > bloque["col_func"]:
            func_especifico = limpiar_texto(row.iloc[bloque["col_func"]])
            
        func_final = func_especifico if func_especifico else func_principal
            
        obj = "PROGRAMACIÓN"
        if bloque["col_obj"] is not None and len(row) > bloque["col_obj"]:
            val_obj = limpiar_texto(row.iloc[bloque["col_obj"]])
            if val_obj:
                obj = val_obj

        # Guardar solo bloques con datos válidos de inspección
        if fecha or pct or (concepto and concepto != "SIN CONCEPTO"):
            numero_visita_contador += 1
            
            visita = {
                "establecimiento_id": est_id,
                "numero_visita": numero_visita_contador,
                "fecha_visita": fecha,
                "porcentaje_cumplimiento": pct,
                "concepto_sanitario": concepto,
                "funcionarios_visita": func_final,
                "objetivo_visita": obj,
                "denuncias": "NO",
                "eta": "NO",
                "tiene_medida_sanitaria": "NO",
                "tipo_medida_sanitaria": "",
                "observaciones": obs_fila
            }
            
            visitas_a_insertar.append(visita)


# ==============================================================================
# 7. REGISTRO EN SUPABASE
# ==============================================================================
print(f"Insertando {len(visitas_a_insertar)} visitas ordenadas cronológicamente...")

lote_size = 500
total_visitas = len(visitas_a_insertar)

for i in range(0, total_visitas, lote_size):
    lote = visitas_a_insertar[i:i + lote_size]
    try:
        supabase.table("visitas").insert(lote).execute()
        print(f"✔ Lote {i//lote_size + 1} ({len(lote)} visitas) registrado con éxito.")
    except Exception as e:
        print(f"✖ Error en el lote {i//lote_size + 1}: {e}")

print("\n¡Migración cronológica y unificada finalizada exitosamente!")