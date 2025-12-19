// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
	function applyTheme(theme) {
		if (theme === 'dark') {
			document.documentElement.classList.add('dark-theme');
		} else {
			document.documentElement.classList.remove('dark-theme');
		}
		var icon = document.getElementById('theme-icon');
		if (icon) icon.textContent = theme === 'dark' ? '🌙' : '☀️';
	}

	document.addEventListener('DOMContentLoaded', function () {
		var stored = localStorage.getItem('site-theme') || 'dark';
		applyTheme(stored);

		var btn = document.getElementById('theme-toggle');
		if (btn) {
			btn.addEventListener('click', function (e) {
				e.preventDefault();
				var isDark = document.documentElement.classList.contains('dark-theme');
				var next = isDark ? 'light' : 'dark';
				localStorage.setItem('site-theme', next);
				applyTheme(next);
			});
		}
	});
})();
