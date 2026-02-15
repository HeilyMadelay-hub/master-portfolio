import { useState } from 'react'
import './App.css'
import axios from "axios";

function App() {
    const [mensaje, setMensaje] = useState("");
    const [resultado, setResultado] = useState(null);

    const traducir = async () => {
        const res = await axios.post("http://localhost:8000/traducir", { mensaje });
        setResultado(res.data);
    };

    return (
        <div>
            <input value={mensaje} onChange={e => setMensaje(e.target.value)} />
            <button onClick={traducir}>Traducir</button>
            <pre>{resultado && JSON.stringify(resultado, null, 2)}</pre>
        </div>
    );
}

export default App;
