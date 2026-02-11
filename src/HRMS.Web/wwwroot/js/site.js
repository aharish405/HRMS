// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Sidebar Toggle Logic
document.addEventListener("DOMContentLoaded", function () {
    var sidebarToggle = document.getElementById("sidebarToggleTop");
    if (sidebarToggle) {
        sidebarToggle.addEventListener("click", function (e) {
            e.preventDefault();
            document.body.classList.toggle("sidebar-toggled");
            document.querySelector(".sidebar").classList.toggle("toggled");
            document.getElementById("wrapper").classList.toggle("toggled");
        });
    }
});
