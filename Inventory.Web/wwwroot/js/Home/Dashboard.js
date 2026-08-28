document.addEventListener("DOMContentLoaded", function () {
    const config = window.DashboardConfig || {
        topProducts: { labels: [], quantities: [], datasetLabel: 'Quantity' },
        categories: { labels: [], counts: [], emptyLabel: 'No Data' }
    };

    // Helper: Determine Theme Colors
    function getThemeColors() {
        const isDark = document.documentElement.getAttribute('data-bs-theme') === 'dark' ||
            document.body.getAttribute('data-bs-theme') === 'dark' ||
            localStorage.getItem('theme') === 'dark';

        return {
            textColor: isDark ? '#94a3b8' : '#64748b',
            gridColor: isDark ? 'rgba(255, 255, 255, 0.08)' : 'rgba(0, 0, 0, 0.05)',
            cardBg: isDark ? '#1e293b' : '#ffffff',
            primaryColor: '#4f46e5',
            palette: [
                '#4f46e5', // Indigo
                '#06b6d4', // Cyan
                '#10b981', // Emerald
                '#f59e0b', // Amber
                '#ec4899', // Pink
                '#8b5cf6'  // Purple
            ]
        };
    }

    const theme = getThemeColors();

    // -------------------------------------------------------------
    // 1. Stock Levels Bar Chart
    // -------------------------------------------------------------
    const stockCanvas = document.getElementById('stockBarChart');
    if (stockCanvas) {
        const topLabels = config.topProducts.labels && config.topProducts.labels.length > 0
            ? config.topProducts.labels
            : [config.categories.emptyLabel || 'No Products'];

        const topQuantities = config.topProducts.quantities && config.topProducts.quantities.length > 0
            ? config.topProducts.quantities
            : [0];

        new Chart(stockCanvas.getContext('2d'), {
            type: 'bar',
            data: {
                labels: topLabels,
                datasets: [{
                    label: config.topProducts.datasetLabel || 'Quantity',
                    data: topQuantities,
                    backgroundColor: 'rgba(79, 70, 229, 0.85)',
                    hoverBackgroundColor: '#4338ca',
                    borderColor: '#4f46e5',
                    borderWidth: 1,
                    borderRadius: 6,
                    maxBarThickness: 45
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        padding: 10,
                        cornerRadius: 8,
                        callbacks: {
                            label: function (context) {
                                return ` ${context.dataset.label}: ${context.parsed.y} units`;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: theme.gridColor,
                            drawBorder: false
                        },
                        ticks: {
                            color: theme.textColor,
                            precision: 0
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: theme.textColor,
                            maxRotation: 25,
                            minRotation: 0
                        }
                    }
                }
            }
        });
    }

    // -------------------------------------------------------------
    // 2. Category Distribution Doughnut Chart
    // -------------------------------------------------------------
    const catCanvas = document.getElementById('categoryPieChart');
    if (catCanvas) {
        const catLabels = config.categories.labels && config.categories.labels.length > 0
            ? config.categories.labels
            : [config.categories.emptyLabel || 'No Categories'];

        const catCounts = config.categories.counts && config.categories.counts.length > 0
            ? config.categories.counts
            : [1];

        const catColors = config.categories.counts && config.categories.counts.length > 0
            ? theme.palette
            : ['#94a3b8'];

        new Chart(catCanvas.getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: catLabels,
                datasets: [{
                    data: catCounts,
                    backgroundColor: catColors,
                    borderWidth: 2,
                    borderColor: theme.cardBg,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            color: theme.textColor,
                            boxWidth: 12,
                            padding: 14,
                            usePointStyle: true
                        }
                    },
                    tooltip: {
                        padding: 10,
                        cornerRadius: 8,
                        callbacks: {
                            label: function (context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const value = context.parsed;
                                const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                                return ` ${context.label}: ${value} (${percentage}%)`;
                            }
                        }
                    }
                },
                cutout: '68%'
            }
        });
    }
});