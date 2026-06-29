// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function showDeleteModal(formId, message) {

    document.getElementById("deleteMessage").textContent = message;

    document.getElementById("confirmDeleteBtn").onclick = function () {
        document.getElementById(formId).submit();
    };

    var modal = new bootstrap.Modal(
        document.getElementById("deleteConfirmModal")
    );

    modal.show();
}