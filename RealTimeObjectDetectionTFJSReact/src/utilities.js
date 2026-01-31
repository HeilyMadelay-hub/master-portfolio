// Colores consistentes para cada clase de objeto
const classColors = {
    person: '#FF6B6B',
    car: '#4ECDC4',
    truck: '#45B7D1',
    bus: '#FFA07A',
    motorcycle: '#98D8C8',
    bicycle: '#F7DC6F',
    dog: '#BB8FCE',
    cat: '#85C1E2',
    bottle: '#52BE80',
    cup: '#F8B739',
    chair: '#EC7063',
    couch: '#AF7AC5',
    'dining table': '#5DADE2',
    laptop: '#48C9B0',
    mouse: '#F5B041',
    keyboard: '#EB984E',
    'cell phone': '#DC7633',
    book: '#CB4335',
    clock: '#884EA0',
    vase: '#2471A3',
    // Color por defecto para objetos no listados
    default: '#FFFFFF'
};

// Función para obtener color por clase
const getColorForClass = (className) => {
    return classColors[className] || classColors.default;
};

export const drawRect = (detections, ctx) => {
    // Loop through each prediction
    detections.forEach(prediction => {
        // Extract boxes, classes and confidence
        const [x, y, width, height] = prediction['bbox'];
        const text = prediction['class'];
        const confidence = (prediction['score'] * 100).toFixed(1); // Convertir a porcentaje

        // Get consistent color for this class
        const color = getColorForClass(text);

        // Set styling
        ctx.strokeStyle = color;
        ctx.lineWidth = 3; // Hacer las líneas más gruesas
        ctx.font = 'bold 18px Arial';

        // Draw rectangle
        ctx.beginPath();
        ctx.rect(x, y, width, height);
        ctx.stroke();

        // Draw label background
        const label = `${text} ${confidence}%`;
        const textWidth = ctx.measureText(label).width;
        const textHeight = 24;

        ctx.fillStyle = color;
        ctx.fillRect(x, y - textHeight, textWidth + 10, textHeight);

        // Draw label text
        ctx.fillStyle = '#000000'; // Texto negro para mejor contraste
        ctx.fillText(label, x + 5, y - 7);
    });
};

// Nueva función para calcular y mostrar FPS
let lastTime = performance.now();
let fps = 0;
let frameCount = 0;

export const drawFPS = (ctx, canvasWidth, canvasHeight) => {
    const currentTime = performance.now();
    frameCount++;

    // Actualizar FPS cada segundo
    if (currentTime - lastTime >= 1000) {
        fps = frameCount;
        frameCount = 0;
        lastTime = currentTime;
    }

    // Dibujar FPS en la esquina superior izquierda
    ctx.font = 'bold 20px Arial';
    ctx.fillStyle = '#00FF00'; // Verde brillante
    ctx.strokeStyle = '#000000';
    ctx.lineWidth = 3;

    // Paréntesis correctos
    ctx.strokeText(`FPS: ${fps}`, 10, 30);
    ctx.fillText(`FPS: ${fps}`, 10, 30);
};

// Nueva función para mostrar contador de objetos
export const drawObjectCount = (detections, ctx, canvasWidth) => {
    const count = detections.length;
    const text = `Objects: ${count}`;

    ctx.font = 'bold 20px Arial';
    const textWidth = ctx.measureText(text).width;

    ctx.fillStyle = '#FFD700'; // Dorado
    ctx.strokeStyle = '#000000';
    ctx.lineWidth = 3;

    // Ya estaba correcto aquí
    ctx.strokeText(text, canvasWidth - textWidth - 10, 30);
    ctx.fillText(text, canvasWidth - textWidth - 10, 30);
};