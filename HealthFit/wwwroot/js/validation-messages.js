// Override jQuery validation messages to Vietnamese
$(document).ready(function() {
    // Override default messages
    $.extend($.validator.messages, {
        required: "Trường này là bắt buộc.",
        remote: "Vui lòng sửa trường này.",
        email: "Vui lòng nhập địa chỉ email hợp lệ.",
        url: "Vui lòng nhập URL hợp lệ.",
        date: "Vui lòng nhập ngày hợp lệ.",
        dateISO: "Vui lòng nhập ngày hợp lệ (ISO).",
        number: "Vui lòng nhập số hợp lệ.",
        digits: "Vui lòng chỉ nhập chữ số.",
        equalTo: "Vui lòng nhập lại giá trị này.",
        maxlength: $.validator.format("Vui lòng nhập không quá {0} ký tự."),
        minlength: $.validator.format("Vui lòng nhập ít nhất {0} ký tự."),
        rangelength: $.validator.format("Vui lòng nhập giá trị từ {0} đến {1} ký tự."),
        range: $.validator.format("Vui lòng nhập giá trị từ {0} đến {1}."),
        max: $.validator.format("Vui lòng nhập giá trị nhỏ hơn hoặc bằng {0}."),
        min: $.validator.format("Vui lòng nhập giá trị lớn hơn hoặc bằng {0}."),
        step: $.validator.format("Vui lòng nhập bội số của {0}.")
    });

    // Override step validation message for price fields
    $.validator.addMethod("step", function(value, element, param) {
        var type = $(element).attr("type"),
            errorMessage = "Thuộc tính Step không được hỗ trợ cho input type " + type + ".",
            supportedTypes = ["text", "number", "range"],
            re = new RegExp("\\b" + type + "\\b"),
            notSupported = type && !re.test(supportedTypes.join()),
            decimalPlaces = function(num) {
                var match = ("" + num).match(/(?:\.(\d+))?$/);
                if (!match) {
                    return 0;
                }
                return match[1] ? match[1].length : 0;
            },
            toInt = function(num) {
                return Math.round(num * Math.pow(10, decimals));
            },
            valid = true,
            decimals;

        if (notSupported) {
            throw new Error(errorMessage);
        }

        decimals = decimalPlaces(param);

        if (decimalPlaces(value) > decimals || toInt(value) % toInt(param) !== 0) {
            valid = false;
        }

        return this.optional(element) || valid;
    }, function(param, element) {
        // Custom message for step validation
        if ($(element).attr("name") === "Price" || $(element).attr("id") === "Price") {
            return "Vui lòng nhập giá là bội số của 1000.";
        }
        return $.validator.format("Vui lòng nhập bội số của {0}.", param);
    });
}); 