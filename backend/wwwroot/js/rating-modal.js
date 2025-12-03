let ratingModalInitialized = false;

function ViewRating(productId) {
    $.ajax({
        url: "/Product/Ratings",
        type: "GET",
        data: { productId },
        success: function (html) {
            $("#modal-rating-content").html(html);
            $("#btnLoadMore").data("product", productId);
            $("#ratingModal").modal("show");
        },
        error: function () {
            alert("Không thể tải đánh giá sản phẩm.");
        }
    });
}

function loadMoreRatings(productId, page) {
    $.ajax({
        url: "/Product/Ratings/Page",
        type: "GET",
        data: { productId, page },
        //beforeSend: function () {
        //    $("#btnLoadMore").prop("disabled", true);
        //    $("#btnLoadMore").hide();
        //},
        success: function (html) {
            if (html.trim() === '') {
                $("#loadMoreContainer").hide();
            } else {
                $("#ratingList").append(html);
                const nextPage = parseInt($("#btnLoadMore").data("page")) + 1;
                $("#btnLoadMore").data("page", nextPage).prop("disabled", false).text("Xem thêm");
                $("#btnLoadMore").removeClass("d-none");
                $("#loadingSpinner").addClass("d-none");
                renderStars();
            }
        },
        error: function () {
            $("#btnLoadMore").prop("disabled", false).text("Thử lại");
            alert("Không thể tải thêm đánh giá. Vui lòng thử lại.");
        }
    });
}

function renderStars() {
    $(".star-container").each(function () {
        const star = parseFloat($(this).data("star"));
        const fullStars = Math.floor(star);
        const halfStar = (star - fullStars) >= 0.5;
        const emptyStars = 5 - fullStars - (halfStar ? 1 : 0);

        let starsHtml = "";

        for (let i = 0; i < fullStars; i++) {
            starsHtml += '<i class="ri-star-fill text-warning"></i>';
        }
        if (halfStar) {
            starsHtml += '<i class="ri-star-half-line text-warning"></i>';
        }
        for (let i = 0; i < emptyStars; i++) {
            starsHtml += '<i class="ri-star-line text-warning"></i>';
        }

        $(this).html(starsHtml);
    });
}

function initRatingModalHandlers() {
    if (ratingModalInitialized) return;
    ratingModalInitialized = true;

    $(document).on('shown.bs.modal', '#ratingModal', function () {
        const productId = $("#btnLoadMore").data("product");
        $("#ratingList").empty();
        $("#btnLoadMore").data("page", 1).show();
        loadMoreRatings(productId, 1);
    });

    $(document).on('click', '#btnLoadMore', function () {
        const productId = $(this).data("product");
        const page = $(this).data("page");
        loadMoreRatings(productId, page);
    });

    $(document).on('hidden.bs.modal', '#ratingModal', function () {
        $("#ratingList").empty();
        $("#btnLoadMore").data("product", "").data("page", 1);
    });
}

// Gọi khi trang load xong
$(document).ready(function () {
    initRatingModalHandlers();
});

$(document).on("change", ".toggle-show", function () {
    const id = $(this).data("id");
    const isShow = $(this).is(":checked");

    $.ajax({
        url: "/Product/Review/ToggleShow",
        type: "POST",
        data: { id: id, isShow: isShow },
        success: function (res) {
            if (res.code === 0) {
                AlertResponse(res.message, "success");
            }
            else {
                AlertResponse(res.message, "warning");
            }
        },
        error: function () {
            AlertResponse("Lỗi hệ thống, vui lòng thử lại.", "error");
        }
    });
});
