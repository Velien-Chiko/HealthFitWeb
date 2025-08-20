document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.toggle-password').forEach(function (element) {
        element.addEventListener('click', function () {
            var input = document.querySelector(this.getAttribute('toggle'));
            if (input.type === "password") {
                input.type = "text";
                this.innerHTML = '<i class="fa fa-eye-slash"></i>';
            } else {
                input.type = "password";
                this.innerHTML = '<i class="fa fa-eye"></i>';
            }
        });
    });
});
