
function initResizablePanels() {
    interact('.resizable-x')
        .resizable({
            edges: { left: true, right: true, bottom: false, top: false },
            listeners: {
                move(event) {
                    let { x, y } = event.target.dataset

                    x = (parseFloat(x) || 0) + event.deltaRect.left
                    y = (parseFloat(y) || 0) + event.deltaRect.top

                    Object.assign(event.target.style, {
                        width: `${event.rect.width}px`,
                        height: `${event.rect.height}px`
                    })

                    Object.assign(event.target.dataset, { x, y })
                }
            },
            modifiers: [
                interact.modifiers.restrictEdges({
                    outer: 'parent'
                }),
                interact.modifiers.restrictSize({
                    min: { width: 200, height: 50 },
                    max: { width: 800, height: 600 }
                })
            ],
            inertia: true
        })

    interact('.resizable-y')
        .resizable({
            edges: { top: true, bottom: true, left: false, right: false },
            listeners: {
                move(event) {
                    let { x, y } = event.target.dataset

                    x = (parseFloat(x) || 0) + event.deltaRect.left
                    y = (parseFloat(y) || 0) + event.deltaRect.top

                    Object.assign(event.target.style, {
                        width: `${event.rect.width}px`,
                        height: `${event.rect.height}px`
                    })

                    Object.assign(event.target.dataset, { x, y })
                }
            },
            modifiers: [
                interact.modifiers.restrictEdges({
                    outer: 'parent'
                }),
                interact.modifiers.restrictSize({
                    min: { width: 50, height: 200 },
                    max: { width: 600, height: 800 }
                })
            ],
            inertia: true
        })
}

// Call the function when the document is ready
$(document).ready(function () {
    initResizablePanels();
});

var currentZoom = 1.0; // initialize a global variable to keep track of zoom level
var currentGridSize = 5; // initialize a global variable to keep track of grid size
var hoveredConnection = null;

window.jsPlumbInterop = {
    instance: null, // define instance here
    nodes: null,

    initJsPlumb: function (refrence, startzoom) {
        this.instance = jsPlumb.getInstance({
            DragOptions: { cursor: 'pointer', zIndex: 2000 },
            PaintStyle: { stroke: '#666', zIndex: 9999 }, // Adjust zIndex here
            EndpointHoverStyle: { fill: '#216477' },
            HoverPaintStyle: { stroke: '#216477' },
            EndpointStyle: { width: 1, height: 1, stroke: '#666' },
            Endpoint: "Blank",
            Anchors: ["Right", "Left"],
        });

        nodes = document.querySelectorAll(".node");

        nodes.forEach(function (node) {
            node.addEventListener('mousedown', function (event) {
                event.stopPropagation();
            });
        });

        this.instance.draggable(nodes, {
            //borde ändra till detta som inställnign typ
            grid: [5, 5], 
            start: function (params) {
                var zoom = currentZoom;
                var left = parseFloat(params.el.style.left);
                var top = parseFloat(params.el.style.top);
                startConnections = jsPlumbInterop.instance.getConnections({ source: params.el });

                //params.el.style.left = (left * zoom) + 'px';
                //params.el.style.top = (top * zoom) + 'px';
            },

            drag: throttle(function (params) {
                var zoom = currentZoom;
                var left = parseFloat(params.pos[0]) / zoom;
                var top = parseFloat(params.pos[1]) / zoom;

                //params.el.style.left = left + 'px';
                //params.el.style.top = top + 'px';

                // get all child elements with class Connection
                var connections = params.el.getElementsByClassName('Connection');

                // get the canvas' bounding rectangle
                var canvasElement = document.getElementById("canvas");
                var canvasRect = canvasElement.getBoundingClientRect();

                // variable to track if any endpoint position was updated
                var endpointsUpdated = false;

                // update the jsPlumb instance about each connection's position changes
                for (var i = 0; i < connections.length; i++) {
                    var connectionId = connections[i].id;

                    // also move the corresponding -endpoint elements
                    var endpointId = connectionId + '-endpoint';
                    var baseElement = connections[i];
                    var endpointElement = document.getElementById(endpointId);

                    if (endpointElement) {
                        var rect = baseElement.getBoundingClientRect();

                        // adjust the coordinates to be relative to the canvas
                        var baseLeft = ((rect.left - canvasRect.left) / currentZoom) + baseElement.offsetWidth / 2;
                        var baseTop = ((rect.top - canvasRect.top) / currentZoom) + baseElement.offsetHeight / 2;

                        endpointElement.style.left = baseLeft + 'px';
                        endpointElement.style.top = baseTop + 'px';

                        endpointsUpdated = true;
                    }
                }

                // only repaint everything if any endpoint position was updated
                if (endpointsUpdated) {
                    requestAnimationFrame(function () {
                        jsPlumbInterop.instance.repaintEverything();
                    });

                }
            }, 8),

            stop: function (params) {
                var zoom = currentZoom;
                var newX = Math.round(parseFloat(params.pos[0]));
                var newY = Math.round(parseFloat(params.pos[1]));
                var nodeId = params.el.id;

                updateNodeLocation(refrence, nodeId, newX, newY);

                associatedConnections = jsPlumbInterop.instance.getConnections({ element: params.el });
                var repaintNeeded = false;

                //Iterate over each connection
                associatedConnections.forEach(function (connection) {
                    var sourceTop = connection.source.offsetTop;
                    var targetTop = connection.target.offsetTop;
                    var sourceLeft = connection.source.offsetLeft;
                    var targetLeft = connection.target.offsetLeft;

                    // Calculate vertical and overall distances
                    var verticalDistance = Math.abs(sourceTop - targetTop);
                    var horizontalDistance = Math.abs(sourceLeft - targetLeft);
                    var overallDistance = Math.sqrt(Math.pow(horizontalDistance, 2) + Math.pow(verticalDistance, 2));

                    // Determine the connector type based on the vertical and overall distances
                    var connectorType = ["Bezier", { curviness: 100 }];
                    if (verticalDistance < 10 || overallDistance < 50) {
                        connectorType = "Straight";
                    }

                    // Check if the connector type is different from the current one
                    if (connection.getConnector().type !== connectorType[0]) {
                        // Set the connector type for the connection
                        connection.setConnector(connectorType);
                        repaintNeeded = true;
                    }
                });

                // Repaint everything only if needed
                if (repaintNeeded) {
                    jsPlumbInterop.instance.repaintEverything();
                }
                
            }
        });

        this.instance.bind("connection", function (info) {
            // Ensure the connection has a canvas element
            if (info.connection.canvas) {
                // Add mouseover and mouseout event listeners to the connection's canvas
                info.connection.canvas.addEventListener("mouseover", function () {
                    hoveredConnection = info.connection;
                    //console.log("Mouse over connection with source:", hoveredConnection.sourceId, "and target:", hoveredConnection.targetId);
                });

                info.connection.canvas.addEventListener("mouseout", function () {
                    hoveredConnection = null;
                });
            }
        });
    },

    //----------------------------remove   probably
    clearConnections: function () {
        try {
            if (this.instance) {
                var connections = this.instance.getAllConnections();
                for (var i = 0; i < connections.length; i++) {
                    this.instance.detach(connections[i]);
                }
            }
        } catch (error) {
            console.error("Failed to clear connections:", error);
        }
    },

    removeNodeConnections: function (sourceId, targetId) {
        try {
            // Get the jsPlumb instance
            var instance = this.instance;

            if (!instance) {
                console.error("jsPlumb instance is not initialized.");
                return;
            }

            // Retrieve the connections between the given source and target
            var connections = instance.getConnections({
                source: sourceId,
                target: targetId
            });

            // Loop over connections and detach them
            connections.forEach(function (connection) {
                instance.detach(connection);
            });

        } catch (error) {
            console.error("Failed to remove single connection:", error);
        }
    },
    removeConnection: function (sourceId, targetId) {
        try {
            // Get the jsPlumb instance
            var instance = this.instance;

            if (!instance) {
                console.error("jsPlumb instance is not initialized.");
                return;
            }

            // Define the connection query parameters
            var queryParams = {};
            if (sourceId) queryParams.source = sourceId;
            if (targetId) queryParams.target = targetId;

            // Retrieve the connections based on the provided parameters
            var connections = instance.getConnections(queryParams);

            // Loop over connections and detach them
            connections.forEach(function (connection) {
                instance.detach(connection);
            });

        } catch (error) {
            console.error("Failed to remove connections:", error);
        }
    },

    //----------------------------

    clearAllConnections: function () {
        try {
            if (this.instance) {
                var connectionLayer = document.getElementById('connection-layer');
                this.instance.empty(connectionLayer);
            }

        } catch (error) {
            console.error("Failed to clear all connections:", error);
        }
    },

    connectInputs: function (sourceId, targetId, color) {
        // Use attribute selectors to select elements by ID, even when the ID starts with a digit
        var source = document.querySelector('[id="' + sourceId + '"].Connection');
        var target = document.querySelector('[id="' + targetId + '"].Connection');

        if (source && target) {
            var connectionLayer = document.getElementById('connection-layer');

            // Get the canvas's offset and zoom level
            var canvas = document.getElementById("canvas");
            var canvasRect = canvas.getBoundingClientRect();
            var offsetX = canvasRect.left;
            var offsetY = canvasRect.top;
            var zoom = currentZoom;

            // Adjust the position of the endpoints considering the canvas offset and zoom level
            var sourceEndpoint = document.createElement('div');
            sourceEndpoint.id = sourceId + '-endpoint';
            sourceEndpoint.style.position = 'absolute';
            sourceEndpoint.style.left = (((source.getBoundingClientRect().left - offsetX) / zoom) + source.offsetWidth / 2) + 'px';
            sourceEndpoint.style.top = (((source.getBoundingClientRect().top - offsetY) / zoom) + source.offsetHeight / 2) + 'px';
            sourceEndpoint.style.width = '0px';
            sourceEndpoint.style.height = '0px';
            sourceEndpoint.style.background = color;
            sourceEndpoint.style.borderRadius = '0%';
            connectionLayer.appendChild(sourceEndpoint);

            var targetEndpoint = document.createElement('div');
            targetEndpoint.id = targetId + '-endpoint';
            targetEndpoint.style.position = 'absolute';
            targetEndpoint.style.left = (((target.getBoundingClientRect().left - offsetX) / zoom) + target.offsetWidth / 2) + 'px';
            targetEndpoint.style.top = (((target.getBoundingClientRect().top - offsetY) / zoom) + target.offsetHeight / 2) + 'px';
            targetEndpoint.style.width = '0px';
            targetEndpoint.style.height = '0px';
            targetEndpoint.style.background = color;
            targetEndpoint.style.borderRadius = '0%';
            connectionLayer.appendChild(targetEndpoint);

            this.instance.addEndpoint(sourceEndpoint.id, { anchor: 'Right' });
            this.instance.addEndpoint(targetEndpoint.id, { anchor: 'Left' });

            //console.log('Source Position and Dimensions:', source.getBoundingClientRect());
            //console.log('Target Position and Dimensions:', target.getBoundingClientRect());
            //console.log(source);

            if (color == "#ff0000") {
                //console.log('Source Endpoint Position:', sourceEndpoint.style.left, sourceEndpoint.style.top);
                //console.log('Target Endpoint Position:', targetEndpoint.style.left, targetEndpoint.style.top);
            }

            var sourceTop = (source.getBoundingClientRect().top - offsetY) / zoom;
            var targetTop = (target.getBoundingClientRect().top - offsetY) / zoom;

            var connectorType = ["Bezier", { curviness: 100 }];

            if (Math.abs(sourceTop - targetTop) < 5) { // 5 is a threshold you can adjust
                connectorType = "Straight";
            //    console.log("connector straight");
            }

            // Check for existing connections between the same source and target
            var existingConnections = this.instance.getConnections({
                target: targetEndpoint.id
            });

            // Remove any existing connections
            existingConnections.forEach(function (connection) {
                this.instance.deleteConnection(connection);
            }.bind(this));

            this.instance.connect({
                source: sourceEndpoint.id,
                target: targetEndpoint.id,
                paintStyle: { stroke: color, strokeWidth: 5 },
                connector: connectorType
            });

        } else {
            console.error('Unable to connect', sourceId, 'to', targetId);
            if (!source) console.error('Source does not exist');
            if (!target) console.error('Target does not exist');
        }
    },

    setGridSize: function (refrence, gridSize, startzoom) {
        currentGridSize = gridSize;  // Update the grid size
        this.instance.reset();  // Reset the current jsPlumb instance
        this.initJsPlumb(refrence, startzoom); 
    },

    addZoom: function (zoomValue) {
        if (zoomValue != null) {
            currentZoom = zoomValue;

            var canvas = document.getElementById("canvas");

            if (!canvas) {
                console.error("Canvas element not found");
                return;
            }

            var transformOrigin = [0, 0];
            var el = canvas;

            var p = ["webkit", "moz", "ms", "o"],
                s = "scale(" + zoomValue + ")",
                oString = (transformOrigin[0] * 100) + "% " + (transformOrigin[1] * 100) + "%";

            for (var i = 0; i < p.length; i++) {
                el.style[p[i] + "Transform"] = s;
                el.style[p[i] + "TransformOrigin"] = oString;
            }
            if (zoomValue != null && this.instance != null) {
                this.instance.setZoom(zoomValue)
            }

            el.style["transform"] = s;
            el.style["transformOrigin"] = oString;
        }
    },

    addNode: function (nodeId) {
        var newNode = document.getElementById(nodeId);

        // Check if the node exists
        if (newNode) {
            // Make the new node draggable
            this.instance.draggable(newNode, {
                grid: [currentGridSize, currentGridSize],
                start: function (params) {
                    var zoom = currentZoom;
                    var left = parseFloat(params.el.style.left);
                    var top = parseFloat(params.el.style.top);

                    //params.el.style.left = (left * zoom) + 'px';
                    //params.el.style.top = (top * zoom) + 'px';
                },
                drag: throttle(function (params) {
                    var zoom = currentZoom;
                    var left = parseFloat(params.pos[0]) / zoom;
                    var top = parseFloat(params.pos[1]) / zoom;

                    //params.el.style.left = left + 'px';
                    //params.el.style.top = top + 'px';

                    // get all child elements with class Connection
                    var connections = params.el.getElementsByClassName('Connection');

                    // get the canvas' bounding rectangle
                    var canvasElement = document.getElementById("canvas");
                    var canvasRect = canvasElement.getBoundingClientRect();

                    // variable to track if any endpoint position was updated
                    var endpointsUpdated = false;

                    // update the jsPlumb instance about each connection's position changes
                    for (var i = 0; i < connections.length; i++) {
                        var connectionId = connections[i].id;

                        // also move the corresponding -endpoint elements
                        var endpointId = connectionId + '-endpoint';
                        var baseElement = connections[i];
                        var endpointElement = document.getElementById(endpointId);

                        if (endpointElement) {
                            var rect = baseElement.getBoundingClientRect();

                            // adjust the coordinates to be relative to the canvas
                            var baseLeft = ((rect.left - canvasRect.left) / currentZoom) + baseElement.offsetWidth / 2;
                            var baseTop = ((rect.top - canvasRect.top) / currentZoom) + baseElement.offsetHeight / 2;

                            endpointElement.style.left = baseLeft + 'px';
                            endpointElement.style.top = baseTop + 'px';

                            endpointsUpdated = true;
                        }
                    }

                    // only repaint everything if any endpoint position was updated
                    if (endpointsUpdated) {
                        requestAnimationFrame(function () {
                            //var associatedConnections = jsPlumbInterop.instance.getConnections({ element: params.el });

                            //// Iterate over each connection
                            //associatedConnections.forEach(function (connection) {
                            //    var sourceTop = connection.source.offsetTop;
                            //    var targetTop = connection.target.offsetTop;
                            //    var sourceLeft = connection.source.offsetLeft;
                            //    var targetLeft = connection.target.offsetLeft;

                            //    // Calculate vertical and overall distances
                            //    var verticalDistance = Math.abs(sourceTop - targetTop);
                            //    var horizontalDistance = Math.abs(sourceLeft - targetLeft);
                            //    var overallDistance = Math.sqrt(Math.pow(horizontalDistance, 2) + Math.pow(verticalDistance, 2));

                            //    // Determine the connector type based on the vertical and overall distances
                            //    var connectorType = ["Bezier", { curviness: 100 }];
                            //    if (verticalDistance < 10 || overallDistance < 50) {
                            //        connectorType = "Straight";
                            //    }

                            //    // Set the connector type for the connection
                            //    connection.setConnector(connectorType);
                            //});

                            // Since the connector type might have changed, request a repaint
                            jsPlumbInterop.instance.repaintEverything();
                        });
                    }
                }, 8),

                stop: function (params) {
                    var zoom = currentZoom;
                    var newX = Math.round(parseFloat(params.pos[0]));
                    var newY = Math.round(parseFloat(params.pos[1]));
                    var nodeId = params.el.id;

                    updateNodeLocation(refrence, nodeId, newX, newY);

                    associatedConnections = jsPlumbInterop.instance.getConnections({ element: params.el });
                    var repaintNeeded = false;

                    //Iterate over each connection
                    associatedConnections.forEach(function (connection) {
                        var sourceTop = connection.source.offsetTop;
                        var targetTop = connection.target.offsetTop;
                        var sourceLeft = connection.source.offsetLeft;
                        var targetLeft = connection.target.offsetLeft;

                        // Calculate vertical and overall distances
                        var verticalDistance = Math.abs(sourceTop - targetTop);
                        var horizontalDistance = Math.abs(sourceLeft - targetLeft);
                        var overallDistance = Math.sqrt(Math.pow(horizontalDistance, 2) + Math.pow(verticalDistance, 2));

                        // Determine the connector type based on the vertical and overall distances
                        var connectorType = ["Bezier", { curviness: 100 }];
                        if (verticalDistance < 10 || overallDistance < 50) {
                            connectorType = "Straight";
                        }

                        // Check if the connector type is different from the current one
                        if (connection.getConnector().type !== connectorType[0]) {
                            // Set the connector type for the connection
                            connection.setConnector(connectorType);
                            repaintNeeded = true;
                        }
                    });

                    // Repaint everything only if needed
                    if (repaintNeeded) {
                        jsPlumbInterop.instance.repaintEverything();
                    }
                }

            });

            // Add the new node to the node list
            nodes = document.querySelectorAll(".node");

            // Apply event listeners to the new node
            newNode.addEventListener('mousedown', function (event) {
                event.stopPropagation();
            });
        } else {
            console.error('Node does not exist with id:' + nodeId);
        }
    },
};

// Variable to store the current hovered connection
var hoveredConnection = null;

// Function to add a mouseover listener to a connection

function throttle(func, limit) {
    let lastFunc;
    let lastRan;
    return function () {
        const context = this;
        const args = arguments;
        if (!lastRan) {
            func.apply(context, args);
            lastRan = Date.now();
        } else {
            clearTimeout(lastFunc);
            lastFunc = setTimeout(function () {
                if ((Date.now() - lastRan) >= limit) {
                    func.apply(context, args);
                    lastRan = Date.now();
                }
            }, limit - (Date.now() - lastRan));
        }
    };
}

function resetEndpoints(id, currentZoom) {
    element = document.getElementById(id);

    // get all child elements with class Connection
    var connections = element.getElementsByClassName('Connection');

    // get the canvas' bounding rectangle
    var canvasElement = document.getElementById("canvas");
    var canvasRect = canvasElement.getBoundingClientRect();

    // variable to track if any endpoint position was updated
    var endpointsUpdated = false;

    // update the jsPlumb instance about each connection's position changes
    for (var i = 0; i < connections.length; i++) {
        var connectionId = connections[i].id;

        // also move the corresponding -endpoint elements
        var endpointId = connectionId + '-endpoint';
        var baseElement = connections[i];
        var endpointElement = document.getElementById(endpointId);

        if (endpointElement) {
            var rect = baseElement.getBoundingClientRect();

            // adjust the coordinates to be relative to the canvas
            var baseLeft = ((rect.left - canvasRect.left) / currentZoom) + baseElement.offsetWidth / 2;
            var baseTop = ((rect.top - canvasRect.top) / currentZoom) + baseElement.offsetHeight / 2;

            endpointElement.style.left = baseLeft + 'px';
            endpointElement.style.top = baseTop + 'px';

            endpointsUpdated = true;
        }
    }

    if (endpointsUpdated) {
        requestAnimationFrame(function () {
            jsPlumbInterop.instance.repaintEverything();
        });
    }

    return endpointsUpdated;
}

function logParentNodeName(sourceId) {
    var sourceElement = document.getElementById(sourceId);
    if (sourceElement) {
        var parentElement = sourceElement.parentElement;
        if (parentElement) {
            var grandparentElement = parentElement.parentElement;
            if (grandparentElement) {
                var greatGrandparentElement = grandparentElement.parentElement;
                if (greatGrandparentElement) {
                    var theOne = greatGrandparentElement.parentElement;
                    if (theOne) {
                        console.log(theOne.id);
                    } else {
                        console.log('the one could not be found');
                    }
                } else {
                    console.log("Great-grandparent element not found.");
                }
            } else {
                console.log("Grandparent element not found.");
            }
        } else {
            console.log("Parent element not found.");
        }
    } else {
        console.log("Source element not found.");
    }
}

window.updateNodeLocation = (refrence, nodeId, newX, newY) => {
    newX = Math.round(newX);
    newY = Math.round(newY);
    console.log(`The node was moved to X: ${newX}, Y: ${newY}`);

    if (newX < -2147483648 || newX > 2147483647 || newY < -2147483648 || newY > 2147483647) {
        console.error('Coordinates out of range for 32-bit integer');
        return;
    }

    refrence.invokeMethodAsync('UpdateNodeLocation', nodeId, newX, newY)
        .catch(err => {
            console.error(err);
            console.log(newX);
            console.log(newY);
        });
};

window.createInfiniteCanvas = function (refrence, sx, sy) {
    var canvas = document.getElementById("canvas");
    var viewport = document.getElementById("whiteboard");
    var isDown = false;
    var startX;
    var startY;

    canvas.style.left = sx + 'px';
    canvas.style.top = sy + 'px';
    viewport.addEventListener('mousedown', function (e) {
        isDown = true;
        startX = e.pageX - canvas.offsetLeft;
        startY = e.pageY - canvas.offsetTop;
    });

    viewport.addEventListener('mouseup', function (event) {
        //console.log('Mouseup event triggered');
        //console.log('Element pressed:', event.target);

        if (event.target.id == "whiteboard") {
            refrence.invokeMethodAsync('DeselectBlock')
        }

        isDown = false;
        var leftValue = parseFloat(canvas.style.left);
        var topValue = parseFloat(canvas.style.top);

        if (isNaN(leftValue) || isNaN(topValue)) {
            console.error("Cannot convert to number.");
            return;
        }

        refrence.invokeMethodAsync('UpdateCameraPosition', leftValue, topValue)
            .catch(err => {
                console.error(err);
                console.log('Left:', canvas.style.left);
                console.log('Top:', canvas.style.top);
                console.log('Event Target:', event.target); // Logging the target element in case of an error
            });
    });


    viewport.addEventListener('mousemove', function (e) {
        if (!isDown) return;
        var x = e.pageX;
        var y = e.pageY;
        canvas.style.left = (x - startX) + 'px';
        canvas.style.top = (y - startY) + 'px';

        // Set the background position of the whiteboard
        viewport.style.backgroundPositionX = canvas.style.left;
        viewport.style.backgroundPositionY = canvas.style.top;
    });
}

window.updateCameraPos = function (newX, newY, refrence) {
    var canvas = document.getElementById("canvas");
    var viewport = document.getElementById("whiteboard");

    if (canvas && viewport) {
        canvas.style.left = newX + 'px';
        canvas.style.top = newY + 'px';
        viewport.style.backgroundPositionX = newX + 'px';
        viewport.style.backgroundPositionY = newY + 'px';

        if (refrence) {
            refrence.invokeMethodAsync('UpdateCameraPosition', newX, newY)
                .catch(err => console.error(err));
        }
    } else {
        console.error("Canvas or viewport element is null");
    }
};

window.initializeContextMenu = function () {
    // Get the 'whiteBoard' element
    var whiteBoard = document.getElementById('whiteboard');

    whiteBoard.addEventListener('contextmenu', function (event) {
        // prevent the default context menu from showing
        event.preventDefault();

        // translate the event's page coordinates to be relative to the whiteBoard div's top-left corner
        var rect = whiteBoard.getBoundingClientRect();
        var x = event.clientX - rect.left;
        var y = event.clientY - rect.top;

        showContextMenu(x, y);
    });

    document.addEventListener('click', function (event) {
        var contextMenu = document.getElementById("contextMenu");

        // if the clicked element is not the context menu and not a descendant of the context menu
        if (!contextMenu.contains(event.target)) {
            contextMenu.style.display = "none";
            inputContextMenu.style.display = "none";
        }
    });

    window.showContextMenu = function (refrence, pageX, pageY) {
        whiteBoard = document.getElementById('whiteboard');
        var contextMenu = document.getElementById("contextMenu");
        var inputContextMenu = document.getElementById("inputContextMenu");
        var canvas = document.getElementById("canvas");

        // test for mini context menu
        var zoomLevel = ((window.outerWidth)
            / window.innerWidth);

        var elementUnderCursor = document.elementFromPoint(pageX, pageY);

        // Calculate the canvas' position
        var rectCanvas = canvas.getBoundingClientRect();
        var canvasX = rectCanvas.left;
        var canvasY = rectCanvas.top;

        // Adjust the coordinates relative to the canvas considering the zoom level
        var relativeX = (pageX - canvasX - 50) / currentZoom;
        var relativeY = (pageY - canvasY) / currentZoom;

        if (elementUnderCursor.textContent.includes("\u200B") && elementUnderCursor.classList.contains('small-font')) {
            inputContextMenu.style.display = "block";
            inputContextMenu.style.left = `${pageX}px`;
            inputContextMenu.style.top = `${pageY}px`;
            hideContextMenu();
            return; // Exit the function without showing the context menu
        }
        //----------------

        contextMenu.style.display = "block";
        hideInputContextMenu();
        var width = contextMenu.offsetWidth;
        var height = contextMenu.offsetHeight;

        var rectWhiteBoard = whiteBoard.getBoundingClientRect();
        var windowWidth = rectWhiteBoard.width;
        var windowHeight = rectWhiteBoard.height;

        if (pageX + (width * 1) > windowWidth + rectWhiteBoard.left) {
            pageX -= width;
        }
        if (pageY + (height * 1) > windowHeight + rectWhiteBoard.top) {
            pageY -= height;
        }

        contextMenu.style.left = `${pageX}px`;
        contextMenu.style.top = `${pageY}px`;

        if (refrence) {
            refrence.invokeMethodAsync('SetBlockSpawn', relativeX, relativeY)
                .then(result => console.log("Async method result: ", result))
                .catch(err => console.error("Async method error: ", err));
        }
    };

    window.hideContextMenu = function () {
        var contextMenu = document.getElementById("contextMenu");
        contextMenu.style.display = "none";
    }

    window.hideInputContextMenu = function () {
        var contextMenu = document.getElementById("inputContextMenu");
        contextMenu.style.display = "none";
    }

    window.contextMenuFunctions = {
        showSubMenu: function (subMenuId) {
            var subMenu = document.getElementById(subMenuId);
            if (subMenu.classList.contains('hidden')) {
                subMenu.classList.remove('hidden');
            }
        },
        hideSubMenu: function (subMenuId) {
            var subMenu = document.getElementById(subMenuId);
            if (!subMenu.classList.contains('hidden')) {
                subMenu.classList.add('hidden');
            }
        }
    };

    window.toggleSubMenu = function (subMenuId) {
        var subMenu = document.getElementById(subMenuId);
        if (subMenu.classList.contains('hidden')) {
            subMenu.classList.remove('hidden');
        } else {
            subMenu.classList.add('hidden');
        }
    };

}

window.addClickListener = (dotNetObjRef) => {
    document.addEventListener('click', function (event) {
        // Check if clicked inside dropdown or button
        if (event.target.closest('.dropdown-list') || event.target.closest('.your-button-class')) {
            return;
        }

        dotNetObjRef.invokeMethodAsync('CheckAndResetJustToggled')
            .then(wasJustToggled => {
                if (!wasJustToggled) {
                    dotNetObjRef.invokeMethodAsync('HideDropdown');
                }
            });
    });
};

window.registerGlobalKeyPress = (dotNetObject) => {
    document.addEventListener('keydown', function (event) {
        dotNetObject.invokeMethodAsync('OnGlobalKeyPress', event.key, event.code);
        if (event.key === 'Delete') {
            if (hoveredConnection) {
                // Assuming hoveredConnection has properties like sourceId and targetId
                var sourceId = hoveredConnection.sourceId;
                var targetId = hoveredConnection.targetId;
                //console.log("Source ID:", sourceId, "Target ID:", targetId);

                dotNetObject.invokeMethodAsync('OnRemoveConnection', sourceId);
            }
        }
    });
};

function getCsrfToken() {
    const csrfCookie = document.cookie.split('; ').find(row => row.startsWith('X-CSRF-TOKEN='));
    return csrfCookie ? csrfCookie.split('=')[1] : null;
}


function submitLogoutForm(logoutUrl, csrfToken) {
    const form = document.createElement('form');
    form.action = logoutUrl;
    form.method = 'POST';

    const tokenInput = document.createElement('input');
    tokenInput.type = 'hidden';
    tokenInput.name = '__RequestVerificationToken';
    tokenInput.value = csrfToken;
    form.appendChild(tokenInput);

    document.body.appendChild(form);
    form.submit();
}
