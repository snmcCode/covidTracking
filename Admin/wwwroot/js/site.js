// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
function check() {
    var checkbox = document.getElementById("verifyCheckbox");
    var radioDiv = document.getElementById("hideShow");
    var verifyNow = document.getElementById("verifyNow");
    var verifyLater = document.getElementById("verifyLater");
    if (checkbox.checked == true) {
        radioDiv.hidden = false;
    } else {
        radioDiv.hidden = true;
        verifyNow.checked = false;
        verifyLater.checked = false;
    }
}