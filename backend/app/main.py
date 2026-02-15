from fastapi import FastAPI # Importamos la biblioteca FastAPI

app = FastAPI() # Inicio de la app

@app.get("/health") #Endpoint salud
async def health():
    return {"status": "awake"}
