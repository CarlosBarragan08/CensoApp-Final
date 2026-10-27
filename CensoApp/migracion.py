import pandas as pd
import numpy as np
import re
from supabase import create_client

# ==============================================================================
# 1. CONFIGURACIÓN DE SUPABASE
# ==============================================================================
SUPABASE_URL = "https://cpyapbjrbxewpcibazuh.supabase.co"
SUPABASE_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImNweWFwYmpyYnhld3BjaWJhenVoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3OTA1NDA5NDgsImV4cCI6MjEwNjExNjk0OH0.zu6TFZXBLWYuKB3EGaw5DAbLwuH2MLI3h7r1D9rVMew"

supabase = create_client(SUPABASE_URL, SUPABASE_KEY)


# ==============================================================================
# 2. DICCIONARIOS DE HOMOLOGACIÓN Y CORRECCIÓN ORTOGRÁFICA
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

DICCIONARIO_RIESGO = {
    "ATO": "ALTO",
    "ALTO": "ALTO",
    "MEDIO": "MEDIO",
    "BAJO": "BAJO"
}

DICCIONARIO_ESTADO = {
    "ACTVO": "ACTIVO",
    "ACTIVO": "ACTIVO",
    "INACTIVO": "INACTIVO"
}

DICCIONARIO_AREA = {
    "URNABO": "URBANA",
    "URNANA": "URBANA",
    "URNANO": "URBANA",
    "UTBANA": "URBANA",
    "URBANO": "URBANA",
    "URBANA": "URBANA",
    "RURA": "RURAL",
    "RURAL": "RURAL"
}

DICCIONARIO_SUJETOS = {
    "PREPARACION DE LAIMENTOS": "PREPARACION DE ALIMENTOS",
    "ENSAMBLE DE ALIEMNTOS": "ENSAMBLE DE ALIMENTOS",
    "ENSAMBLE ALIMENTOS": "ENSAMBLE DE ALIMENTOS",
    "CARNE Y/O PRODUCTOS CARNICOS COMESTIBLES": "CARNES Y/O PRODUCTOS CARNICOS COMESTIBLES"
}

DICCIONARIO_ESTABLECIMIENTOS = {
    "ENSAMBLE PAUQUETES DE ALIMENTOS": "ENSAMBLE PAQUETES DE ALIMENTOS",
    "RESTAUANTE": "RESTAURANTE"
}


# ==============================================================================
# 3. FUNCIONES AUXILIARES DE LIMPIEZA
# ==============================================================================
def limpiar_texto(val):
    if val is None or pd.isna(val):
        return ""
    val_str = str(val).replace('\xa0', ' ').strip()
    val_str = re.sub(r'\s+', ' ', val_str)
    return val_str.upper()

def corregir_campo(val, diccionario, valor_defecto=""):
    texto = limpiar_texto(val)
    if not texto:
        return valor_defecto
    for clave, correcto in diccionario.items():
        if clave in texto:
            return correcto
    return texto if valor_defecto == "" else valor_defecto

def limpiar_porcentaje(val):
    """ Convierte 0.77 a 77, 0.785 a 78.5 y limpia casillas vacías (NaN) """
    if val is None or pd.isna(val):
        return ""
    
    val_str = str(val).replace('%', '').replace(',', '.').strip()
    if val_str.lower() in ['nan', 'none', '']:
        return ""
        
    try:
        num = float(val_str)
        if pd.isna(num):
            return ""
        # Si es un número decimal <= 1 (ej: 0.77 o 0.785), lo multiplicamos por 100
        if 0 < num <= 1.0:
            num = num * 100
        # Formato sin decimales innecesarios
        if num == int(num):
            return str(int(num))
        else:
            return str(round(num, 1))
    except ValueError:
        return ""


# ==============================================================================
# 4. LECTURA Y MAPPING EXACTO A LAS COLUMNAS DE SUPABASE
# ==============================================================================
archivo_excel = "CENSO ALIMENTOS Y BEBIDAS - ACTUALIZADO (1) - copia (1).xlsx"

print("Cargando y saneando matriz Excel de 2.900 registros...")
try:
    df = pd.read_excel(archivo_excel, sheet_name="CENSO")
except Exception as e:
    print(f"✖ Error al abrir el archivo de Excel: {e}")
    exit()

df.columns = df.columns.str.strip()

establecimientos_a_insertar = []

for index, row in df.iterrows():
    nit_cedula = limpiar_texto(row.get('NIT O CEDULA'))
    if not nit_cedula or nit_cedula == "NONE":
        continue

    nombre_comercial = limpiar_texto(row.get('NOMBRE COMERCIAL')) or "SIN NOMBRE"
    razon_social = limpiar_texto(row.get('RAZON SOCIAL')) or nombre_comercial
    municipio = limpiar_texto(row.get('MUNICIPIO'))
    departamento = limpiar_texto(row.get('DEPARTAMENTO')) or "SUCRE"
    
    tipo_sujeto_raw = limpiar_texto(row.get('TIPOS DE SUJETOS'))
    tipo_sujeto = DICCIONARIO_SUJETOS.get(tipo_sujeto_raw, tipo_sujeto_raw)
    
    tipo_est_raw = limpiar_texto(row.get('TIPOS DE ESTABLECIMIENTOS'))
    tipo_establecimiento = DICCIONARIO_ESTABLECIMIENTOS.get(tipo_est_raw, tipo_est_raw)
    
    riesgo = corregir_campo(row.get('TIPO DE RIESGO'), DICCIONARIO_RIESGO, "MEDIO")
    concepto = corregir_campo(row.get('CONCEPTO SANITARIO EMITIDO'), DICCIONARIO_CONCEPTO, valor_defecto="PENDIENTE")
    estado = corregir_campo(row.get('ESTADO'), DICCIONARIO_ESTADO, "ACTIVO")
    area = corregir_campo(row.get('AREA'), DICCIONARIO_AREA, "URBANA")

    decreto_raw = limpiar_texto(row.get('CUENTA CON AUTORIZACION SANITARIA DEFINITIVA DECRETO 1500 Y SUS REGLAMENTARIOS'))
    decreto_1500 = "SÍ" if "SI" in decreto_raw else "NO"

    # Procesar porcentaje con la función corregida
    pct_limpio = limpiar_porcentaje(row.get('PORCENTAJE DE CUMPLIMENTO'))

    establecimiento = {
        "nit_o_cedula": nit_cedula,
        "nombre_comercial": nombre_comercial,
        "razon_social": razon_social,
        "tipo_sujeto": tipo_sujeto,
        "numero_inscripcion": limpiar_texto(row.get('INSCRIPCION')),
        "estado": estado,
        "representante_legal": limpiar_texto(row.get('REPRESENTANTE LEGAL O PROPIETARIO')),
        "direccion": limpiar_texto(row.get('DIRECCION')),
        "municipio": municipio,
        "codigo_divipola_municipio": limpiar_texto(row.get('CODIGO DIVIPOLA MUNICIPIO')),
        "departamento": departamento,
        "codigo_divipola_dpto": limpiar_texto(row.get('CODIGO DIVIPOLA DPTO')),
        "area": area,
        "telefono": limpiar_texto(row.get('TELEFONO')),
        "correo_electronico": limpiar_texto(row.get('CORREO ELECTRÓNICO')).lower(),
        "tipo_establecimiento": tipo_establecimiento,
        "tipo_riesgo": riesgo,
        "concepto_sanitario": concepto,
        "porcentaje_cumplimiento": pct_limpio,
        "autorizacion_sanitaria_decreto_1500": decreto_1500,
        "numero_fecha_resolucion_autorizacion": limpiar_texto(row.get('NÚMERO Y FECHA DE LA RESOLUCION DE AUTORIZACION SANITARIA')),
        "actividad_principal": tipo_sujeto,
        "tipo_documento": "NIT" if len(nit_cedula) > 9 else "CÉDULA DE CIUDADANÍA"
    }
    
    establecimientos_a_insertar.append(establecimiento)


# ==============================================================================
# 5. RE-INSERCIÓN A SUPABASE
# ==============================================================================
print("Limpiando registros antiguos e insertando los datos corregidos...")

try:
    # Elimina los datos cargados anteriormente para no duplicar
    supabase.table("establecimientos").delete().neq("id", -1).execute()
except Exception as e:
    print(f"Nota sobre limpieza: {e}")

lote_size = 500
total_registros = len(establecimientos_a_insertar)

for i in range(0, total_registros, lote_size):
    lote = establecimientos_a_insertar[i:i + lote_size]
    try:
        supabase.table("establecimientos").insert(lote).execute()
        print(f"✔ Lote {i//lote_size + 1} ({len(lote)} registros) actualizado con éxito.")
    except Exception as e:
        print(f"✖ Error en el lote {i//lote_size + 1}: {e}")

print("\n¡Porcentajes y registros corregidos exitosamente!")