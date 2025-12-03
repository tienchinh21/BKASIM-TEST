let table;
let timeoutDebounce = 500;

const languageTable = {
    //url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/vi.json',
    emptyTable: "Chưa có dữ liệu",
    info: "Hiển thị _START_ đến _END_ của _TOTAL_ mục",
    infoEmpty: "Hiển thị 0 đến 0 của 0 mục",
    lengthMenu: "Hiển thị _MENU_ mục",
    loadingRecords: "Đang tải...",
    processing: "Đang xử lý...",
    paginate: {
        // first: "Đầu",
        // last: "Cuối",
        // next: "Tiếp",
        // previous: "Trước"
    }
}

const bookingStatusMapping = {
    0: `<div class="badge badge-warning">Chờ xác nhận</div>`, // Pending
    1: `<div class="badge badge-primary">Đã xác nhận</div>`, // Confirmed
    2: `<div class="badge badge-success">Đã check-in</div>`, // CheckIn
    3: `<div class="badge badge-info">Đang diễn ra</div>`, // InProgress
    4: `<div class="badge badge-success">Đã hoàn thành</div>`, // Completed
    5: `<div class="badge badge-danger">Đã hủy</div>`, // Canceled
    6: `<div class="badge badge-danger">Từ chối</div>`, // Rejected
};

const paymentStatusMapping = {
    0: `<div class="badge badge-warning">Chưa thanh toán</div>`, // Unpaid
    1: `<div class="badge badge-success">Đã thanh toán</div>`, // Paid
    2: `<div class="badge badge-danger">Thanh toán thất bại</div>`, // Failed
    3: `<div class="badge badge-info">Đã hoàn tiền</div>`, // Refunded
};

const deliveryStatusMapping = {
    0: `<div class="badge badge-warning">Đang chờ</div>`,
    1: `<div class="badge badge-warning">Đang chuẩn bị</div>`,
    2: `<div class="badge badge-success">Đã xác nhận</div>`,
    3: `<div class="badge badge-primary">Đã vận chuyển</div>`,
    4: `<div class="badge badge-danger">Đã hủy</div>`,
};

const orderStatusMapping = {
    0: `<div class="badge badge-warning">Chờ xác nhận</div>`, // Pending
    1: `<div class="badge badge-primary">Đã xác nhận</div>`, // Confirmed
    2: `<div class="badge badge-success">Đã hoàn thành</div>`, // Completed
    3: `<div class="badge badge-danger">Đã hủy</div>`, // Cancelled
};

const requestStatusMapping = {
    1: `<div class="badge badge-warning">Đang chờ duyệt</div>`,
    2: `<div class="badge badge-danger">Đã từ chối</div>`,
    3: `<div class="badge badge-success">Đã xác nhận</div>`,
    4: `<div class="badge badge-danger">Đã hủy</div>`,
    5: `<div class="badge badge-success">Đã hoàn thành</div>`,
};

// appointmentStatusMapping đã được định nghĩa trong mappingStatus.js, không khai báo lại ở đây

const search = debounce(function () {
    console.log("searching...");
    table.ajax.reload(null, false);
}, timeoutDebounce);

$(function () {
    $(".wrapper-menu").on("click", () => {
        $("body").toggleClass("sidebar-main");

        //if ($("body").hasClass("sidebar-main")) {
        //	$(".iq-menu-bt-sidebar").css("transform", "translateX(-45px)");
        //} else {
        //	$(".iq-menu-bt-sidebar").css("transform", "");
        //}
    });

    $.ajaxSetup({
        beforeSend: function (xhr) {
            const token = GetToken();
            xhr.setRequestHeader("Authorization", `Bearer ${token}`);
        },
    });

    $("#start").on("change", function () {
        // console.log($(this))
    });

    $("#end").on("change", function () {
        // console.log($(this))
    });
});

function debounce(func, wait) {
    let timeout;
    return function (...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}

function GetToken() {
    const encodedToken = localStorage.getItem("_tk");
    const token = atob(encodedToken);
    return token;
}

function getQueryParam(name) {
    const url = new URL(window.location.href);
    const paramValue = url.searchParams.get(name);

    // Xóa param khỏi URL
    url.searchParams.delete(name);

    // Thay thế URL hiện tại (không reload trang)
    window.history.replaceState({}, document.title, url.toString());

    return paramValue;
}

function InitValidator() {
    $(".modal-footer button[type=submit]").attr("disabled", true);

    $("form input[required][type=text], form select[required], form textarea[required]").each(function () {
        $(this).on("focusin", function () {
            $(this).parent().removeClass("has-error has-danger");
            $(this).next(".help-block.with-errors").text("");
        });

        $(this).on("focusout", function () {
            const inputValue = $(this).val();
            const nextElement = $(this).next(".help-block.with-errors");
            const errorMessage = $(this).data("error-message") ?? "Vui lòng nhập trường này";

            if (inputValue === "") {
                nextElement.text(errorMessage);
                $(".modal-footer button[type=submit]").attr("disabled", true);
                $(this).parent().addClass("has-error has-danger");
            } else {
                nextElement.html("");
                $("button[type=submit]").removeAttr("disabled");
                $(this).parent().removeClass("has-error has-danger");
            }
        });
    });
}

function ValidateFormFromTo(title, message) {
    const fromVal = new Date($("form input[data-from=true]").val());
    const toVal = new Date($("form input[data-to=true]").val());

    if (fromVal < toVal) {
        return true;
    }
    $.alert({
        title: title,
        content: message,
    });
    return false;
}

function validateEmail(email) {
    if (!email) return false;

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function preventInvalidChars(event) {
    const regex = /^[a-zA-Z0-9]*$/;
    const char = String.fromCharCode(event.which);

    if (!regex.test(char)) {
        event.preventDefault();
    }
}

function ClearForm() {
    $("form").trigger("reset");
}

function NoSupportFeature() {
    // Use SweetAlert2 if available, otherwise fallback to SweetAlert 1.x
    if (typeof Swal !== 'undefined' && Swal.fire) {
        Swal.fire({
            title: "Thông báo",
            text: "Chưa hỗ trợ tính năng này!",
            icon: "info",
            confirmButtonText: "Đã hiểu"
        });
    } else if (typeof swal !== 'undefined') {
        // Fallback to SweetAlert 1.x
        swal({
            title: "Thông báo",
            text: "Chưa hỗ trợ tính năng này!",
            icon: "info",
            buttons: {
                confirm: "Đã hiểu",
            },
        });
    } else {
        // Final fallback to native alert
        alert("Chưa hỗ trợ tính năng này!");
    }
}

function FormatDate(dateString) {
    return moment(dateString).format("DD/MM/YYYY");
}

function FormatDateTime(dateString) {
    return moment(dateString).format("DD/MM/YYYY HH:mm:ss");
}

function AlertResponse(message, type, button) {
    // Use SweetAlert2 if available, otherwise fallback to SweetAlert 1.x
    if (typeof Swal !== 'undefined' && Swal.fire) {
        Swal.fire({
            title: "Thông báo",
            text: message,
            icon: type || 'info',
            confirmButtonText: button || 'OK'
        });
    } else if (typeof swal !== 'undefined') {
        // Fallback to SweetAlert 1.x
        swal({
            title: "Thông báo",
            text: message,
            icon: type,
        });
    } else {
        // Final fallback to native alert
        alert(message);
    }
}

function DeleteItem(url, customTable) {
    // Use SweetAlert2 if available, otherwise fallback to SweetAlert 1.x
    if (typeof Swal !== 'undefined' && Swal.fire) {
        Swal.fire({
            title: "Cảnh báo",
            text: "Dữ liệu sẽ không được phục hồi với thao tác này!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Xác nhận",
            cancelButtonText: "Hủy"
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: url,
                    method: "DELETE",
                    success: function (response) {
                        if (response.code === 0) {
                            Swal.fire({
                                title: "Thành công",
                                text: response.message ?? "Xóa thành công!",
                                icon: "success",
                                timer: 2000,
                                showConfirmButton: false
                            });
                            if (customTable) {
                                customTable.ajax.reload(null, false);
                            } else if (typeof table !== 'undefined' && table) {
                                table.ajax.reload(null, false);
                            }
                        } else {
                            Swal.fire({
                                title: "Lỗi",
                                text: response.message ?? "Đã xảy ra lỗi!",
                                icon: "error"
                            });
                        }
                    },
                    error: function (error) {
                        Swal.fire({
                            title: "Lỗi",
                            text: "Lỗi khi thực hiện yêu cầu!",
                            icon: "error"
                        });
                    },
                });
            }
        });
    } else if (typeof swal !== 'undefined') {
        // Fallback to SweetAlert 1.x
        swal({
            title: "Cảnh báo",
            text: "Dữ liệu sẽ không được phục hồi với thao tác này!",
            icon: "warning",
            buttons: {
                cancel: {
                    text: "Hủy",
                    value: null,
                    visible: true,
                    className: "",
                    closeModal: true,
                },
                confirm: {
                    text: "Xác nhận",
                    value: true,
                    visible: true,
                    className: "",
                    closeModal: true,
                },
            },
        }).then((result) => {
            if (result) {
                $.ajax({
                    url: url,
                    method: "DELETE",
                    success: function (response) {
                        if (response.code === 0) {
                            AlertResponse(response.message ?? "Thành công!", "success");
                            if (customTable) {
                                customTable.ajax.reload(null, false);
                            } else if (typeof table !== 'undefined' && table) {
                                table.ajax.reload(null, false);
                            }
                        } else {
                            AlertResponse(response.message ?? "Đã xảy ra lỗi!", "error");
                        }
                    },
                    error: function (error) {
                        AlertResponse("Lỗi khi thực hiện yêu cầu!", "error");
                    },
                });
            }
        });
    } else {
        // Final fallback to native confirm
        if (confirm("Dữ liệu sẽ không được phục hồi với thao tác này! Bạn có chắc chắn muốn xóa?")) {
            $.ajax({
                url: url,
                method: "DELETE",
                success: function (response) {
                    if (response.code === 0) {
                        alert(response.message ?? "Xóa thành công!");
                        if (customTable) {
                            customTable.ajax.reload(null, false);
                        } else if (typeof table !== 'undefined' && table) {
                            table.ajax.reload(null, false);
                        }
                    } else {
                        alert(response.message ?? "Đã xảy ra lỗi!");
                    }
                },
                error: function (error) {
                    alert("Lỗi khi thực hiện yêu cầu!");
                },
            });
        }
    }
}

function ConfirmAlert(title, text, icon, url, method, data, customTable, onSuccess) {
    // Use SweetAlert2 if available, otherwise fallback to SweetAlert 1.x
    if (typeof Swal !== 'undefined' && Swal.fire) {
        Swal.fire({
            title: title || "Xác nhận",
            text: text || "Bạn có chắc chắn muốn thực hiện thao tác này?",
            icon: icon || "question",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Xác nhận",
            cancelButtonText: "Hủy"
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: url,
                    method: method || "POST",
                    data: data,
                    contentType: "application/json",
                    success: function (response) {
                        if (response.code === 0 || response.success) {
                            Swal.fire({
                                title: "Thành công",
                                text: response.message ?? "Thành công!",
                                icon: "success",
                                timer: 2000,
                                showConfirmButton: false
                            });
                            if (customTable) {
                                customTable.ajax.reload(null, false);
                            } else if (typeof table !== 'undefined' && table) {
                                table.ajax.reload(null, false);
                            }

                            if (typeof onSuccess === "function") {
                                onSuccess(response); // thành công thì callback
                            }
                        } else {
                            Swal.fire({
                                title: "Lỗi",
                                text: response.message ?? "Đã xảy ra lỗi!",
                                icon: "error"
                            });
                        }
                    },
                    error: function (error) {
                        Swal.fire({
                            title: "Lỗi",
                            text: "Lỗi khi thực hiện yêu cầu!",
                            icon: "error"
                        });
                    },
                });
            }
        });
    } else if (typeof swal !== 'undefined') {
        // Fallback to SweetAlert 1.x
        swal({
            title: title,
            text: text,
            icon: icon,
            buttons: {
                cancel: {
                    text: "Hủy",
                    value: null,
                    visible: true,
                    className: "",
                    closeModal: true,
                },
                confirm: {
                    text: "Xác nhận",
                    value: true,
                    visible: true,
                    className: "",
                    closeModal: true,
                },
            },
        }).then((result) => {
            if (result) {
                $.ajax({
                    url: url,
                    method: method || "POST",
                    data: data,
                    contentType: "application/json",
                    success: function (response) {
                        if (response.code === 0 || response.success) {
                            AlertResponse(response.message ?? "Thành công!", "success");
                            if (customTable) {
                                customTable.ajax.reload(null, false);
                            } else if (typeof table !== 'undefined' && table) {
                                table.ajax.reload(null, false);
                            }

                            if (typeof onSuccess === "function") {
                                onSuccess(response); // thành công thì callback
                            }
                        } else {
                            AlertResponse(response.message ?? "Đã xảy ra lỗi!", "error");
                        }
                    },
                    error: function (error) {
                        AlertResponse("Lỗi khi thực hiện yêu cầu!", "error");
                    },
                });
            }
        });
    } else {
        // Final fallback to native confirm
        if (confirm(text || "Bạn có chắc chắn muốn thực hiện thao tác này?")) {
            $.ajax({
                url: url,
                method: method || "POST",
                data: data,
                contentType: "application/json",
                success: function (response) {
                    if (response.code === 0 || response.success) {
                        alert(response.message ?? "Thành công!");
                        if (customTable) {
                            customTable.ajax.reload(null, false);
                        } else if (typeof table !== 'undefined' && table) {
                            table.ajax.reload(null, false);
                        }

                        if (typeof onSuccess === "function") {
                            onSuccess(response);
                        }
                    } else {
                        alert(response.message ?? "Đã xảy ra lỗi!");
                    }
                },
                error: function (error) {
                    alert("Lỗi khi thực hiện yêu cầu!");
                },
            });
        }
    }
}

// Rút gọn văn bản
function truncateText(text, maxLength) {
    if (!text) return "";
    return text.length > maxLength ? text.substring(0, maxLength) + "..." : text;
}

// Chuỗi số dạng có dấu chấm ngăn cách về số
function parseCurrency(value) {
    return Number(value.replace(/[^\d]/g, ""));
}

// loading
const opts = {
    lines: 13, // The number of lines to draw
    length: 38, // The length of each line
    width: 17, // The line thickness
    radius: 45, // The radius of the inner circle
    scale: 1, // Scales overall size of the spinner
    corners: 1, // Corner roundness (0..1)
    speed: 1, // Rounds per second
    rotate: 0, // The rotation offset
    animation: "spinner-line-fade-quick", // The CSS animation name for the lines
    direction: 1, // 1: clockwise, -1: counterclockwise
    color: "#ffffff", // CSS color or array of colors
    fadeColor: "transparent", // CSS color or array of colors
    top: "50%", // Top position relative to parent
    left: "50%", // Left position relative to parent
    shadow: "0 0 1px transparent", // Box-shadow for the lines
    zIndex: 2000000000, // The z-index (defaults to 2e9)
    className: "spinner", // The CSS class to assign to the spinner
    position: "absolute", // Element positioning
};
const target = document.getElementById("foo");
const spinner = new Spinner(opts).spin(target);

const removeVietnameseAndSpace = (str) => {
    return str
        .normalize("NFD") // tách dấu khỏi chữ
        .replace(/[\u0300-\u036f]/g, "") // xóa dấu
        .replace(/đ/g, "d")
        .replace(/Đ/g, "D") // xử lý đ/Đ
        .replace(/\s+/g, "") // xóa khoảng trắng
        .toLowerCase(); // tùy chọn: viết thường toàn bộ
};

// Đảm bảo hàm validateReviewPoint có thể được gọi từ bất kỳ đâu
window.validateReviewPoint = function (input, min, max) {
    const start = input.selectionStart;
    const end = input.selectionEnd;

    let value = input.value;

    // Chỉ cho số và dấu chấm
    value = value.replace(/[^0-9.]/g, "");

    // Không cho nhiều dấu chấm
    value = value.replace(/(\..*)\./g, "$1");

    // Loại bỏ 0 đầu nếu không phải 0.x
    if (!value.startsWith("0.") && value !== "0") {
        value = value.replace(/^0+(?=\d)/, "");
    }

    // Parse thành số
    let num = parseFloat(value);

    // Nếu không hợp lệ (NaN), giữ nguyên giá trị text (ví dụ đang nhập `.5`)
    if (!isNaN(num)) {
        // Giới hạn giá trị trong khoảng [min, max]
        if (num < min) num = min;
        if (num > max) num = max;

        // Làm tròn tới 1 số thập phân
        value = num.toString().match(/\./) ? num.toFixed(1) : num.toString();
    }

    // Chỉ update nếu khác để tránh nhảy con trỏ
    if (input.value !== value) {
        input.value = value;
        input.setSelectionRange(start, end);
    }
}
function ValidateImageRatio(event) {
    const file = event.target.files[0];

    if (!file) return;

    const reader = new FileReader();
    reader.readAsDataURL(file);

    reader.onload = function (e) {
        const img = new Image();
        img.src = e.target.result;

        img.onload = function () {
            const width = img.width;
            const height = img.height;
            const ratio = width / height;
            const isValid = Math.abs(ratio - 16 / 9) < 0.01;

            if (!isValid) {
                AlertResponse(`Ảnh không đúng tỉ lệ 16:9. Kích thước: ${width}x${height} (tỉ lệ: ${ratio.toFixed(2)})`, "warning");
                event.target.value = ""; // reset input
                return;
            }
        };

        img.onerror = function () {
            console.error("Không thể đọc ảnh.");
            event.target.value = "";
        };
    };

    reader.onerror = function () {
        console.error("Lỗi khi đọc file.");
        event.target.value = "";
    };
}

/**
 * Handle image preview with aspect ratio validation
 * @param {HTMLElement} inputElement - Input element containing files
 * @param {number} aspectRatio - Desired aspect ratio (width/height), default 16/9
 * @param {string} carouselPreviewSelector - Selector for carousel container
 * @param {string} previewContainerSelector - Selector for thumbnails container
 * @param {Object} options - Additional options
 * @returns {Promise<Array>} Array of valid files
 */
function handleImagePreview(inputElement, aspectRatio = { width: 16, height: 9 }, carouselPreviewSelector, previewContainerSelector, options = {}) {
    // Default options
    const config = {
        tolerance: 0.01,
        minWidth: 0,
        minHeight: 0,
        defaultImagePath: "/images/no-image-2.jpg",
        isSingleImage: false,
        ...options,
    };

    return new Promise(async (resolve) => {
        const carouselPreview = carouselPreviewSelector ? document.querySelector(carouselPreviewSelector) : null;
        const previewContainer = document.querySelector(previewContainerSelector);

        if (!inputElement || !inputElement.files || inputElement.files.length === 0) {
            // Clear previews when no files
            if (config.isSingleImage) {
                previewContainer.innerHTML = `<img style="object-fit:contain; height:150px;" src="${config.defaultImagePath}" />`;
            } else {
                showDefaultImages(carouselPreview, previewContainer, config.defaultImagePath);
            }
            resolve([]);
            return;
        }

        const files = Array.from(inputElement.files);
        const validFiles = [];

        // Validate each file
        for (let i = 0; i < files.length; i++) {
            const file = files[i];

            // Check if file is an image
            if (!file.type.startsWith("image/")) {
                AlertResponse(`File "${file.name}" không phải ảnh. Chỉ được upload ảnh!`, "warning");
                continue;
            }

            // Validate image dimensions
            let isValidSize = { valid: true };
            if (aspectRatio.width > 0 && aspectRatio.height > 0) {
                const ratioValue = aspectRatio.width / aspectRatio.height;
                isValidSize = await checkImageDimensions(file, ratioValue, config.tolerance);
            }
            if (!isValidSize.valid) {
                const expectedRatio = `${aspectRatio.width}:${aspectRatio.height}`;

                AlertResponse(
                    `Ảnh "${file.name}" không đúng tỷ lệ ${expectedRatio}!\n` +
                    `Kích thước hiện tại: ${isValidSize.actualWidth} x ${isValidSize.actualHeight}.\n` +
                    `Vui lòng chọn ảnh có tỷ lệ ${expectedRatio}.`,
                    "warning"
                );
                continue;
            }

            validFiles.push(file);
        }

        // Add this code to update the input's files with only valid files
        if (validFiles.length !== files.length) {
            const dt = new DataTransfer();
            validFiles.forEach((file) => dt.items.add(file));
            inputElement.files = dt.files;
        }

        // If no valid files, reset input
        if (validFiles.length === 0) {
            inputElement.value = "";
            resolve([]);
            return;
        }

        // Clear existing previews
        if (config.isSingleImage) {
            previewContainer.innerHTML = "";
        } else if (carouselPreview) {
            // Clear carousel if it contains default images
            const firstImage = carouselPreview.querySelector("img");
            if (carouselPreview.children.length > 0 && firstImage && firstImage.src.includes("/images/default/")) {
                carouselPreview.innerHTML = "";
            }
        }

        // Generate previews
        if (config.isSingleImage) {
            // Single image preview
            createSingleImagePreview(validFiles[0], previewContainer, inputElement);
        } else {
            // Multi-image preview
            //previewContainer.innerHTML = '';
            validFiles.forEach((file, index) => {
                createImagePreview(file, index, carouselPreview, previewContainer, inputElement);
            });
        }

        resolve(validFiles);
    });
}

/**
 * Check image dimensions and aspect ratio
 */
function checkImageDimensions(image, desiredRatio, tolerance = 0.01) {
    return new Promise((resolve) => {
        const reader = new FileReader();
        reader.readAsDataURL(image);

        reader.onload = function (e) {
            const img = new Image();
            img.src = e.target.result;

            img.onload = function () {
                const imageHeight = this.height;
                const imageWidth = this.width;
                const actualRatio = (imageWidth / imageHeight).toFixed(2);
                const isValid = Math.abs(imageWidth / imageHeight - desiredRatio) < tolerance;

                resolve({
                    valid: isValid,
                    actualWidth: imageWidth,
                    actualHeight: imageHeight,
                    actualRatio: actualRatio,
                });
            };
        };
    });
}

/**
 * Create preview for a single image
 */
function createSingleImagePreview(file, container, inputElement) {
    const reader = new FileReader();
    reader.readAsDataURL(file);

    reader.onload = function (e) {
        // Create elements
        const previewItem = document.createElement("div");
        previewItem.className = "image-preview position-relative card";
        previewItem.style.height = "170px";
        previewItem.style.maxWidth = "250px";

        const removeBtn = document.createElement("span");
        removeBtn.className = "btn-preview-remove";
        removeBtn.textContent = "x";

        const img = document.createElement("img");
        img.style.objectFit = "contain";
        img.style.height = "150px";
        img.src = e.target.result;
        img.className = "card-img-top";
        img.alt = "Hình ảnh";

        // Add event listener
        removeBtn.addEventListener("click", function () {
            previewItem.remove();
            inputElement.value = "";
        });

        // Append elements
        previewItem.appendChild(removeBtn);
        previewItem.appendChild(img);
        container.innerHTML = "";
        container.appendChild(previewItem);
    };
}

/**
 * Create preview for an image in carousel and thumbnail container
 */
function createImagePreview(file, index, carouselPreview, previewContainer, inputElement) {
    const reader = new FileReader();
    const uniqueId = `preview-image-${Date.now()}-${index}`;

    reader.onload = function (e) {
        // Create carousel item
        if (carouselPreview) {
            const carouselItem = document.createElement("div");
            carouselItem.className = `carousel-item ${carouselPreview.children.length === 0 ? "active" : ""}`;
            carouselItem.dataset.id = uniqueId;

            const carouselImg = document.createElement("img");
            carouselImg.src = e.target.result;
            carouselImg.className = "d-block w-100";
            carouselImg.alt = "Hình ảnh";
            carouselImg.style.height = "170px";
            carouselImg.style.objectFit = "cover";

            carouselItem.appendChild(carouselImg);
            carouselPreview.appendChild(carouselItem);
        }

        // Create thumbnail
        const uploadPreview = document.createElement("div");
        uploadPreview.className = "image-preview mx-2 card position-relative";
        uploadPreview.style.maxWidth = "250px";
        uploadPreview.style.height = "170px";
        uploadPreview.dataset.id = uniqueId;

        const thumbnailImg = document.createElement("img");
        thumbnailImg.src = e.target.result;
        thumbnailImg.className = "card-img-top h-100";
        thumbnailImg.alt = "Hình ảnh";
        thumbnailImg.style.objectFit = "contain";

        const removeBtn = document.createElement("span");
        removeBtn.className = "btn-preview-remove";
        removeBtn.textContent = "x";

        removeBtn.addEventListener("click", function () {
            const targetId = uploadPreview.dataset.id;

            if (carouselPreview) {
                const carouselItem = carouselPreview.querySelector(`.carousel-item[data-id="${targetId}"]`);

                // If removing active carousel item, activate another one
                if (carouselItem.classList.contains("active")) {
                    const nextItem = carouselItem.nextElementSibling;
                    const prevItem = carouselItem.previousElementSibling;

                    if (nextItem) {
                        nextItem.classList.add("active");
                    } else if (prevItem) {
                        prevItem.classList.add("active");
                    }
                }

                carouselItem.remove();
            }

            uploadPreview.remove();

            // Remove file from input
            removeFileFromInput(inputElement, index);

            // Show default images if no images left
            if (carouselPreview && carouselPreview.children.length === 0) {
                showDefaultImages(carouselPreview, previewContainer);
            }
        });

        uploadPreview.appendChild(thumbnailImg);
        uploadPreview.appendChild(removeBtn);
        previewContainer.appendChild(uploadPreview);
    };

    reader.readAsDataURL(file);
}

/**
 * Remove a file from the input element
 */
function removeFileFromInput(inputElement, indexToRemove) {
    const dt = new DataTransfer();
    Array.from(inputElement.files)
        .filter((_, idx) => idx !== indexToRemove)
        .forEach((file) => dt.items.add(file));

    inputElement.files = dt.files;
}

/**
 * Show default images in preview containers
 */
function showDefaultImages(carouselPreview, previewContainer, defaultImagePath = "/images/no-image-2.jpg") {
    if (carouselPreview) {
        carouselPreview.innerHTML = `
            <div class="carousel-item active">
                <img src="${defaultImagePath}" class="d-block w-100" alt="Default Image" style="height:170px; object-fit:contain;" />
            </div>
        `;
    }

    if (previewContainer) {
        previewContainer.innerHTML = `
            <div class="image-preview mx-2 card position-relative" style="max-width: 250px; height: 170px;">
                <img src="${defaultImagePath}" class="card-img-top h-100" alt="Default Image" style="object-fit:contain" />
            </div>
        `;
    }
}

$('#changePasswordForm').on('submit', function (e) {
    e.preventDefault();

    const form = document.getElementById('changePasswordForm');
    const formData = new FormData(form);

    $.ajax({
        url: 'api/helpers/changepassword', // Hoặc đổi đúng URL backend nếu khác
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.succeeded) {
                AlertResponse('Đổi mật khẩu thành công!', 'success');
                $('#changePasswordForm')[0].reset();
                setTimeout(() => {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('changePasswordModal'));
                    modal.hide();
                }, 1000);
            } else {
                AlertResponse('Đổi mật khẩu thất bại. Vui lòng kiểm tra lại!', 'danger');
            }
        },
        error: function () {
            AlertResponse('Lỗi kết nối đến server!', 'danger');
        }
    });
});

/**
 * Lấy tên file từ header API xuất dữ liệu
 */
function getFileNameFromDisposition(disposition) {
    if (!disposition) return "export.xlsx";
    // Ưu tiên filename*
    let filenameStarMatch = disposition.match(/filename\*=UTF-8''([^;]+)/i);
    if (filenameStarMatch && filenameStarMatch[1]) {
        return decodeURIComponent(filenameStarMatch[1]);
    }
    // Sau đó đến filename
    let filenameMatch = disposition.match(/filename="?([^\";]+)"?/i);
    if (filenameMatch && filenameMatch[1]) {
        return filenameMatch[1];
    }
    return "export.xlsx";
}

/**
 * Lấy tọa độ từ link google maps
 */
function extractLatLong() {
    const link = document.getElementById("googleMapsLink").value;
    console.log("=== EXTRACT LAT LONG DEBUG ===");
    console.log("Input link:", link);

    if (!link) {
        console.log("No link provided");
        return;
    }

    // Cải thiện regex để hỗ trợ nhiều định dạng URL Google Maps
    const patterns = [
        /@(-?\d+\.\d+),(-?\d+\.\d+)/, // Format: @10.123,106.456
        /@(-?\d+\.\d+),(-?\d+\.\d+),/, // Format: @10.123,106.456, (with comma after)
        /maps\/.*\/(-?\d+\.\d+),(-?\d+\.\d+)/, // Format: maps/place/10.123,106.456
        /q=(-?\d+\.\d+),(-?\d+\.\d+)/, // Format: q=10.123,106.456
        /place\/(-?\d+\.\d+),(-?\d+\.\d+)/, // Format: place/10.123,106.456
        /@(-?\d+\.\d+),(-?\d+\.\d+),?\d*z/ // Format: @10.123,106.456,19z or @10.123,106.456,19.55z
    ];

    let match = null;
    for (let i = 0; i < patterns.length; i++) {
        const pattern = patterns[i];
        console.log(`Testing pattern ${i + 1}:`, pattern);
        match = link.match(pattern);
        if (match) {
            console.log(`Pattern ${i + 1} matched:`, match);
            break;
        }
    }

    if (match) {
        const lat = match[1];
        const lng = match[2];
        console.log("Extracted coordinates - Lat:", lat, "Lng:", lng);
        document.getElementById("latitude").value = lat;
        document.getElementById("longitude").value = lng;
        $("#error-latitude").text("");
        $("#error-longitude").text("");
        console.log("Successfully set latitude and longitude values");
    } else {
        console.log("No pattern matched the link");
        $("#error-latitude").text("Link không hợp lệ. Vui lòng dán một link Google Maps hợp lệ.");
        AlertResponse("Link không hợp lệ. Vui lòng dán một link Google Maps hợp lệ.", "warning");
    }
}

/**
 * Bộ đếm ký tự x/n
 */
function updateInputCounter($input, $counter) {
    const max = parseInt($input.attr('maxlength')) || '';
    const current = $input.val().length;
    $counter.text(`${current}/${max}`);
}

/**
 * Hiển thị thông báo thành công
 * @param {string} title Tiêu đề modal
 * @param {string} text Nội dung modal
 * @param {Object} [options] Các tuỳ chọn bổ sung cho swal
 */
function showSuccess(title, text, options = {}) {
    swal(Object.assign({
        title: title,
        text: text,
        icon: 'success',               // các icon built-in: "warning", "error", "success", "info" :contentReference[oaicite:0]{index=0}
        button: true                   // hiển thị nút OK (có thể là string hoặc ButtonOptions object) :contentReference[oaicite:1]{index=1}
    }, options));
}

/**
 * Hiển thị thông báo lỗi
 */
function showError(title, text, options = {}) {
    swal(Object.assign({
        title: title,
        text: text,
        icon: 'error',
        button: true
    }, options));
}

/**
 * Hiển thị thông báo cảnh báo
 */
function showWarning(title, text, options = {}) {
    swal(Object.assign({
        title: title,
        text: text,
        icon: 'warning',
        button: true,
        dangerMode: true               // làm button confirm thành đỏ và focus vào nút cancel :contentReference[oaicite:2]{index=2}
    }, options));
}

/**
 * Hiển thị thông báo thông tin
 */
function showInfo(title, text, options = {}) {
    swal(Object.assign({
        title: title,
        text: text,
        icon: 'info',
        button: true
    }, options));
}

/**
 * Hiển thị toast (auto-close)
 * @param {string} text Nội dung toast
 * @param {"success"|"error"|"warning"|"info"} [icon='info']
 * @param {number} [timer=1500] Tự đóng sau bao nhiêu ms
 * @param {Object} [options] Tuỳ chọn bổ sung (ví dụ: position, className…)
 */
function showToast(text, icon = 'info', timer = 1500, options = {}) {
    swal(Object.assign({
        title: '',                     // bỏ title để chỉ còn nội dung ngắn
        text: text,
        icon: icon,
        button: false,                 // ẩn nút OK :contentReference[oaicite:3]{index=3}
        timer: timer,                  // tự đóng sau timer ms :contentReference[oaicite:4]{index=4}
        closeOnClickOutside: false,    // ngăn click ngoài đóng toast
        closeOnEsc: false
    }, options));
}

/**
 * Hiển thị confirm xoá với Promise API của SweetAlert v1.x
 * @param {string} title        Tiêu đề
 * @param {string} text         Nội dung
 * @param {Function} onConfirm  Hàm được gọi khi user xác nhận (OK)
 * @param {Function} [onCancel] Hàm được gọi khi user huỷ (Cancel)
 * @param {Object} [options]    Tuỳ options bổ sung (confirmButtonText, cancelButtonText, timer, v.v.)
 */
function showDeleteConfirm(title, text, onConfirm, onCancel = null, options = {}) {
    const cancelText = options.cancelButtonText || 'Hủy';
    const confirmText = options.confirmButtonText || 'Xóa';

    // nút Cancel / OK sẽ nằm chung trong mảng buttons: [cancelLabel, confirmLabel]
    swal(Object.assign({
        title: title,
        text: text,
        icon: 'warning',
        buttons: [cancelText, confirmText],
        dangerMode: true
    }, options))
        .then((willConfirm) => {
            if (willConfirm) {
                onConfirm();
            } else if (typeof onCancel === 'function') {
                onCancel();
            }
        });
}

// Hàm xử lý preview ảnh cho sản phẩm
window.ShowPreview = function (event) {
    const files = event.target.files;
    const previewContainer = document.getElementById('preview');

    if (!previewContainer) {
        console.error('Preview container not found');
        return;
    }

    // Xóa các ảnh preview cũ (chỉ giữ lại ảnh cũ có data-type="old")
    const oldImages = previewContainer.querySelectorAll('[data-type="old"]');
    previewContainer.innerHTML = '';
    oldImages.forEach(img => previewContainer.appendChild(img));

    // Thêm ảnh mới
    Array.from(files).forEach((file, index) => {
        if (file.type.startsWith('image/')) {
            const reader = new FileReader();
            reader.onload = function (e) {
                const imageId = `new_${Date.now()}_${index}`;
                const imageDiv = document.createElement('div');
                imageDiv.className = 'image-preview mx-2 card position-relative';
                imageDiv.style.cssText = 'max-width: 250px; height:170px';
                imageDiv.setAttribute('data-image-id', imageId);
                imageDiv.setAttribute('data-type', 'new');

                imageDiv.innerHTML = `
                    <img src="${e.target.result}" class="card-img-top h-100" alt="Preview" style="object-fit:contain" />
                    <span class="btn-preview-remove" data-image-id="${imageId}">x</span>
                    <div class="image-order-badge">#${previewContainer.children.length + 1}</div>
                `;

                previewContainer.appendChild(imageDiv);

                // Thêm event listener cho nút xóa
                const removeBtn = imageDiv.querySelector('.btn-preview-remove');
                removeBtn.addEventListener('click', function () {
                    imageDiv.remove();
                    updateImageOrder();
                });
            };
            reader.readAsDataURL(file);
        }
    });
};

// Hàm cập nhật thứ tự ảnh
function updateImageOrder() {
    const previewContainer = document.getElementById('preview');
    const images = previewContainer.querySelectorAll('.image-preview');

    images.forEach((img, index) => {
        const badge = img.querySelector('.image-order-badge');
        if (badge) {
            badge.textContent = `#${index + 1}`;
        }
    });
}