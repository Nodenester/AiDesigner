window.takeScreenshot = async (dotNetReference) => {
    let div = document.getElementById('whiteboard-content');

    html2canvas(div).then((canvas) => {
        let resizedCanvas = document.createElement('canvas');
        let ctx = resizedCanvas.getContext('2d');

        // Set the desired dimensions for the resized image
        resizedCanvas.width = 300;  // Example width
        resizedCanvas.height = 200; // Example height

        ctx.drawImage(canvas, 0, 0, 300, 200);

        let dataUrl = resizedCanvas.toDataURL();
        dotNetReference.invokeMethodAsync('ReceiveScreenshot', dataUrl);
    });
}

