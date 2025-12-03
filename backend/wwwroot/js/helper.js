function FormatDate(dateString) {
    return moment(dateString).format("DD/MM/YYYY");
}

function FormatDateTime(dateString) {
    return moment(dateString).format("DD/MM/YYYY HH:mm:ss");
}

function AlertResponse(message, type, button) {
    swal({
        title: "Thông báo",
        text: message,
        icon: type,
    });
}
