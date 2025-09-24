cd "/Users/joseluisperez/Documents/Personal/Desarrollos/KÜPA Logistic/POS/POS.API"
ASPNETCORE_URLS="http://localhost:5002;https://localhost:5003" ASPNETCORE_ENVIRONMENT=Development dotnet runDemo frontend estático para probar la API localmente.

Cómo usar:
1. Asegúrate de que la API esté corriendo en http://localhost:5000
2. Sirve la carpeta `frontend` con un servidor estático, por ejemplo:

   # con Python 3
   python3 -m http.server 8080

   # o con npx serve
   npx serve frontend

3. Abre http://localhost:8080 en tu navegador. La UI hará fetch a http://localhost:5000/api/cash/consolidated

Notas:
- Esta UI es sólo para desarrollo y demostración rápida. Para una UI completa, crea un proyecto Angular/React y migra los componentes.