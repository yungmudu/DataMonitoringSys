// Chart.js functionality for the engineering dashboard

let lineChart, barChart, pieChart, scatterChart;

// Global error handler for JavaScript errors
window.addEventListener('error', function(event) {
    console.error('JavaScript error:', event.error);
    // Don't show error UI for chart-related errors
    if (event.error && event.error.message && event.error.message.includes('Chart')) {
        event.preventDefault();
    }
});

// Global unhandled promise rejection handler
window.addEventListener('unhandledrejection', function(event) {
    console.error('Unhandled promise rejection:', event.reason);
    event.preventDefault();
});

window.initializeCharts = function() {
    try {
        // Initialize Line Chart
        const lineCtx = document.getElementById('lineChart');
        if (lineCtx) {
        lineChart = new Chart(lineCtx, {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Value',
                    data: [],
                    borderColor: 'rgb(59, 130, 246)',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    tension: 0.1,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: 'Time'
                        }
                    },
                    y: {
                        title: {
                            display: true,
                            text: 'Value'
                        }
                    }
                },
                plugins: {
                    title: {
                        display: true,
                        text: 'Data Trends Over Time'
                    },
                    legend: {
                        display: true
                    }
                }
            }
        });
    }

    // Initialize Bar Chart
    const barCtx = document.getElementById('barChart');
    if (barCtx) {
        barChart = new Chart(barCtx, {
            type: 'bar',
            data: {
                labels: [],
                datasets: [{
                    label: 'Average Value',
                    data: [],
                    backgroundColor: [
                        'rgba(59, 130, 246, 0.8)',
                        'rgba(16, 185, 129, 0.8)',
                        'rgba(245, 158, 11, 0.8)',
                        'rgba(239, 68, 68, 0.8)',
                        'rgba(139, 92, 246, 0.8)',
                        'rgba(236, 72, 153, 0.8)'
                    ],
                    borderColor: [
                        'rgb(59, 130, 246)',
                        'rgb(16, 185, 129)',
                        'rgb(245, 158, 11)',
                        'rgb(239, 68, 68)',
                        'rgb(139, 92, 246)',
                        'rgb(236, 72, 153)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: 'Parameters'
                        }
                    },
                    y: {
                        title: {
                            display: true,
                            text: 'Average Value'
                        },
                        beginAtZero: true
                    }
                },
                plugins: {
                    title: {
                        display: true,
                        text: 'Average Values by Parameter'
                    },
                    legend: {
                        display: false
                    }
                }
            }
        });
    }

    // Initialize Pie Chart
    const pieCtx = document.getElementById('pieChart');
    if (pieCtx) {
        pieChart = new Chart(pieCtx, {
            type: 'pie',
            data: {
                labels: [],
                datasets: [{
                    data: [],
                    backgroundColor: [
                        'rgba(59, 130, 246, 0.8)',
                        'rgba(16, 185, 129, 0.8)',
                        'rgba(245, 158, 11, 0.8)',
                        'rgba(239, 68, 68, 0.8)',
                        'rgba(139, 92, 246, 0.8)',
                        'rgba(236, 72, 153, 0.8)',
                        'rgba(107, 114, 128, 0.8)',
                        'rgba(34, 197, 94, 0.8)'
                    ],
                    borderColor: [
                        'rgb(59, 130, 246)',
                        'rgb(16, 185, 129)',
                        'rgb(245, 158, 11)',
                        'rgb(239, 68, 68)',
                        'rgb(139, 92, 246)',
                        'rgb(236, 72, 153)',
                        'rgb(107, 114, 128)',
                        'rgb(34, 197, 94)'
                    ],
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Data Points by Engineering Unit'
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }

    // Initialize Scatter Chart
    const scatterCtx = document.getElementById('scatterChart');
    if (scatterCtx) {
        scatterChart = new Chart(scatterCtx, {
            type: 'scatter',
            data: {
                datasets: [{
                    label: 'Data Points',
                    data: [],
                    backgroundColor: 'rgba(59, 130, 246, 0.6)',
                    borderColor: 'rgb(59, 130, 246)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: 'Data Point Index'
                        }
                    },
                    y: {
                        title: {
                            display: true,
                            text: 'Value'
                        }
                    }
                },
                plugins: {
                    title: {
                        display: true,
                        text: 'Value Distribution'
                    },
                    legend: {
                        display: true
                    }
                }
            }
        });
    }
    } catch (error) {
        console.error('Error initializing charts:', error);
    }
};

window.updateCharts = function(lineData, barData, pieData, scatterData) {
    try {
        // Update Line Chart
        if (lineChart && lineData) {
            lineChart.data.labels = lineData.map(d => d.x);
            lineChart.data.datasets[0].data = lineData.map(d => d.y);
            lineChart.update();
        }

        // Update Bar Chart
        if (barChart && barData) {
            barChart.data.labels = barData.map(d => d.label);
            barChart.data.datasets[0].data = barData.map(d => d.value);
            barChart.update();
        }

        // Update Pie Chart
        if (pieChart && pieData) {
            pieChart.data.labels = pieData.map(d => d.label);
            pieChart.data.datasets[0].data = pieData.map(d => d.value);
            pieChart.update();
        }

        // Update Scatter Chart
        if (scatterChart && scatterData) {
            scatterChart.data.datasets[0].data = scatterData.map(d => ({ x: d.x, y: d.y }));
            scatterChart.update();
        }
    } catch (error) {
        console.error('Error updating charts:', error);
    }
};

window.destroyCharts = function() {
    if (lineChart) {
        lineChart.destroy();
        lineChart = null;
    }
    if (barChart) {
        barChart.destroy();
        barChart = null;
    }
    if (pieChart) {
        pieChart.destroy();
        pieChart = null;
    }
    if (scatterChart) {
        scatterChart.destroy();
        scatterChart = null;
    }
};

// Export chart as image
window.exportChart = function(chartId, filename) {
    const canvas = document.getElementById(chartId);
    if (canvas) {
        const url = canvas.toDataURL('image/png');
        const link = document.createElement('a');
        link.download = filename || 'chart.png';
        link.href = url;
        link.click();
    }
};

// Utility function to generate random colors
window.generateColors = function(count) {
    const colors = [];
    for (let i = 0; i < count; i++) {
        const hue = (i * 137.508) % 360; // Golden angle approximation
        colors.push(`hsl(${hue}, 70%, 60%)`);
    }
    return colors;
};

// File download function
window.downloadFile = function(filename, contentType, base64Data) {
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: contentType });

    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};
