# 1. Obtiene el token de acceso de una cuenta de VetLink para probar la API en Swagger o Postman.
# 2. Uso: .\scripts\obtener-token.ps1   (pide correo y contraseña; la contraseña no se muestra)
# 3. El token dura una hora y queda copiado en el portapapeles.

param(
    [string]$SupabaseUrl = $env:SUPABASE_URL,
    [string]$ClavePublica = $env:SUPABASE_PUBLISHABLE_KEY
)

if (-not $SupabaseUrl) { $SupabaseUrl = Read-Host "URL de Supabase (https://xxxx.supabase.co)" }
if (-not $ClavePublica) { $ClavePublica = Read-Host "Clave pública de Supabase (publishable key)" }
$correo = Read-Host "Correo de tu cuenta VetLink"
$segura = Read-Host "Contraseña" -AsSecureString
$contrasena = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($segura))

try {
    $respuesta = Invoke-RestMethod -Method Post `
        -Uri "$($SupabaseUrl.TrimEnd('/'))/auth/v1/token?grant_type=password" `
        -Headers @{ apikey = $ClavePublica } `
        -ContentType "application/json" `
        -Body (@{ email = $correo; password = $contrasena } | ConvertTo-Json)
    Set-Clipboard -Value $respuesta.access_token
    Write-Host "Token copiado al portapapeles. Vence en $([int]($respuesta.expires_in / 60)) minutos." -ForegroundColor Green
    Write-Host "En Swagger: botón Authorize > pegar el token > Authorize."
}
catch {
    Write-Host "No se pudo iniciar sesión: revisá el correo, la contraseña y que la cuenta esté confirmada." -ForegroundColor Red
}
finally {
    $contrasena = $null
}
