// Theme toggle and visualization save functionality

(function () {
	// Theme Management
	function applyTheme(theme) {
		if (theme === 'dark') {
			document.documentElement.classList.add('dark-theme');
		} else {
			document.documentElement.classList.remove('dark-theme');
		}
		const icon = document.getElementById('theme-icon');
		if (icon) icon.textContent = theme === 'dark' ? '🌙' : '☀️';
	}

	// Initialize on DOM ready
	document.addEventListener('DOMContentLoaded', function () {
		// Apply saved theme
		const stored = localStorage.getItem('site-theme') || 'dark';
		applyTheme(stored);

		// Theme toggle button
		const themeBtn = document.getElementById('theme-toggle');
		if (themeBtn) {
			themeBtn.addEventListener('click', function (e) {
				e.preventDefault();
				const isDark = document.documentElement.classList.contains('dark-theme');
				const next = isDark ? 'light' : 'dark';
				localStorage.setItem('site-theme', next);
				applyTheme(next);
			});
		}

		// Visualization Save Logic
		const saveBtn = document.getElementById('btnSaveViz');
		if (saveBtn) {
			saveBtn.addEventListener('click', async function () {
				const nameInput = document.getElementById('vizName');
				const descInput = document.getElementById('vizDesc');
				const statusDiv = document.getElementById('saveStatus');

				// Validation
				if (!nameInput || !nameInput.value.trim()) {
					if (statusDiv) {
						statusDiv.innerHTML = '<span class="text-danger">Please enter a name.</span>';
					}
					return;
				}

				// Prepare payload
				const currentBitSize = parseInt(document.getElementById('dbSize')?.value || 8);
				const payload = {
					name: nameInput.value.trim(),
					bitSize: currentBitSize,
					description: descInput?.value?.trim() || null
				};

				// Show loading state
				if (statusDiv) {
					statusDiv.innerHTML = '<span class="text-info">Saving...</span>';
				}

				try {
					// Get anti-forgery token
					const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
					const token = tokenInput ? tokenInput.value : '';

					// Use the correct API endpoint from VisualizationsApiController
					const response = await fetch('/api/visualizations', {
						method: 'POST',
						headers: {
							'Content-Type': 'application/json',
							'RequestVerificationToken': token
						},
						body: JSON.stringify(payload)
					});

					if (response.ok) {
						const result = await response.json();
						if (statusDiv) {
							statusDiv.innerHTML = `<span class="text-success">✓ Saved! <a href="/Visualizations/Download/${result.id}">Download Report</a></span>`;
						}
						// Clear inputs
						nameInput.value = '';
						if (descInput) descInput.value = '';
					} else {
						const errorText = await response.text();
						console.error('Save error:', response.status, errorText);
						if (statusDiv) {
							statusDiv.innerHTML = '<span class="text-danger">Error saving. Please try again.</span>';
						}
					}
				} catch (error) {
					console.error('Network error:', error);
					if (statusDiv) {
						statusDiv.innerHTML = '<span class="text-danger">Network error. Please check your connection.</span>';
					}
				}
			});
		}
	});
})();