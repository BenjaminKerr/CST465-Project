// Grover's Algorithm Visualization
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        const canvas = document.getElementById('groverCanvas');
        if (!canvas) return; // Exit if canvas not found

        const ctx = canvas.getContext('2d');
        const iterationInfo = document.getElementById('iterationInfo');
        const searchStats = document.getElementById('searchStats');
        const targetItemSpan = document.getElementById('targetItem');

        let N = 8;
        let targetIndex = Math.floor(Math.random() * N);
        let iteration = 0;
        let probabilities = [];
        let maxIterations = 0;
        let autoTimer = null;

        function initialize() {
            N = Math.max(4, parseInt(document.getElementById('dbSize').value) || 8);
            targetIndex = Math.floor(Math.random() * N);
            iteration = 0;
            maxIterations = Math.ceil(Math.PI / 4 * Math.sqrt(N));

            probabilities = new Array(N).fill(1 / N);

            targetItemSpan.textContent = targetIndex;
            updateStats();
            updateDisplay();
        }

        function groverIteration() {
            if (iteration >= maxIterations) {
                iterationInfo.textContent = `Search Complete! Found item ${targetIndex} in ${iteration} iterations (Classical: ~${Math.floor(N / 2)} checks)`;
                stopAuto();
                return;
            }

            iteration++;

            const avgAmplitude = probabilities.reduce((a, b) => a + Math.sqrt(b), 0) / N;

            probabilities = probabilities.map((p, i) => {
                let amplitude = Math.sqrt(p);

                if (i === targetIndex) {
                    amplitude = -amplitude;
                }

                amplitude = 2 * avgAmplitude - amplitude;

                return Math.max(0, amplitude * amplitude);
            });

            const sum = probabilities.reduce((a, b) => a + b, 0);
            probabilities = probabilities.map(p => p / sum);

            updateDisplay();
        }

        function getDisplayBins(maxBins = 128) {
            if (N <= maxBins) {
                return {
                    probs: probabilities.slice(),
                    labels: probabilities.map((_, i) => String(i)),
                    displayN: N
                };
            }

            const bins = maxBins;
            const binSize = Math.ceil(N / bins);
            const probs = new Array(bins).fill(0);
            const labels = new Array(bins).fill('');

            for (let i = 0; i < N; i++) {
                const b = Math.floor(i / binSize);
                if (b < bins) probs[b] += probabilities[i];
            }

            for (let b = 0; b < bins; b++) {
                const start = b * binSize;
                const end = Math.min(N - 1, (b + 1) * binSize - 1);
                labels[b] = `${start}-${end}`;
            }

            return { probs, labels, displayN: bins };
        }

        function updateDisplay() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);

            const { probs: displayProbs, labels, displayN } = getDisplayBins(128);

            const barWidth = Math.min(60, canvas.width / displayN - 6);
            const spacing = canvas.width / displayN;
            const maxHeight = canvas.height - 100;

            const maxProb = Math.max(...displayProbs);

            displayProbs.forEach((prob, i) => {
                const height = (prob / maxProb) * maxHeight;
                const x = i * spacing + (spacing - barWidth) / 2;
                const y = canvas.height - height - 40;

                let containsTarget = false;
                if (N <= displayN) {
                    containsTarget = (i === targetIndex);
                } else {
                    const parts = labels[i].split('-').map(Number);
                    containsTarget = (targetIndex >= parts[0] && targetIndex <= parts[1]);
                }

                ctx.fillStyle = containsTarget ? '#dc2626' : '#9333ea';
                ctx.fillRect(x, y, barWidth, height);

                ctx.fillStyle = '#111';
                ctx.font = displayN <= 64 ? '14px sans-serif' : '12px sans-serif';
                ctx.textAlign = 'center';
                ctx.fillText(labels[i], x + barWidth / 2, canvas.height - 18);

                const percent = (prob * 100).toFixed(2);
                ctx.fillText(`${percent}%`, x + barWidth / 2, y - 6);
            });

            if (iteration < maxIterations) {
                iterationInfo.textContent = `Iteration: ${iteration} / ${maxIterations} | Target Item: ${targetIndex} | Searching...`;
            } else if (iteration === maxIterations) {
                iterationInfo.textContent = `Search Complete! Found item ${targetIndex} in ${iteration} iterations (Classical: ~${Math.floor(N / 2)} checks)`;
            }

            updateStats();
        }

        function updateStats() {
            const classical = Math.floor(N / 2);
            const quantum = Math.ceil(Math.PI / 4 * Math.sqrt(N));
            const ratio = (classical / Math.max(1, quantum)).toFixed(2);
            searchStats.textContent = `N = ${N} — Classical ~${classical} checks, Quantum ~${quantum} iterations, Speedup ≈ ${ratio}×`;
        }

        function startAuto() {
            if (autoTimer) return;
            const interval = Math.max(50, parseInt(document.getElementById('autoInterval').value) || 300);
            autoTimer = setInterval(() => {
                groverIteration();
                if (iteration >= maxIterations) stopAuto();
            }, interval);
            document.getElementById('autoBtn').textContent = 'Stop Auto';
        }

        function stopAuto() {
            if (autoTimer) {
                clearInterval(autoTimer);
                autoTimer = null;
            }
            document.getElementById('autoBtn').textContent = 'Auto Run';
        }

        // Event Listeners
        document.getElementById('resetBtn').addEventListener('click', () => {
            initialize();
            stopAuto();
        });
        
        document.getElementById('stepBtn').addEventListener('click', groverIteration);
        
        document.getElementById('dbSize').addEventListener('change', initialize);
        
        document.getElementById('autoBtn').addEventListener('click', () => {
            if (autoTimer) stopAuto();
            else startAuto();
        });

        // Initialize on load
        initialize();
    });
})();