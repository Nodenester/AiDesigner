window.takeScreenshot = async (dotNetReference) => {
    let div = document.getElementById('whiteboard-content');

    html2canvas(div).then((canvas) => {
        let resizedCanvas = document.createElement('canvas');
        let ctx = resizedCanvas.getContext('2d');

        // Set the desired dimensions for the resized image
        resizedCanvas.width = 600;  // Example width
        resizedCanvas.height = 400; // Example height

        ctx.drawImage(canvas, 0, 0, 600, 400);

        let dataUrl = resizedCanvas.toDataURL();
        dotNetReference.invokeMethodAsync('ReceiveScreenshot', dataUrl);
    });
}

