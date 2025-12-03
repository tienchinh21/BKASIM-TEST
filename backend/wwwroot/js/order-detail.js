/**
 * Module OrderDetail - Quản lý chi tiết đơn hàng trong modal
 */
const OrderDetail = (function () {
	// Private variables
	let orderDetails = [];
	let isChangeOrderDetails = false;
	let orderId = "";

	/**
	 * Add a product to the order - opens product detail modal
	 */
	function addProduct(productId) {
		// Set up product modal for 'update' context with our callback
		ProductSearchUtils.setupProductModal("update", processSelectedProduct);

		// Load the product details
		ProductSearchUtils.loadProductDetail(productId, "#product-detail-modal .modal-body", function (product) {
			window.currentProductDetail = product;
		});
		$("#product-detail-modal").modal("show");
	}

	/**
	 * Process the selected product from the modal
	 * @param {Object} productData - The selected product data
	 */
	function processSelectedProduct(productData) {
		console.log("Order details", orderDetails);
		console.log("Selected product", productData);

		// Find existing product with same variant
		const existingProductIndex = orderDetails.findIndex((item) => item.productId === productData.productId && JSON.stringify(item.propertyValueIds || []) === JSON.stringify(productData.propertyValueIds || []));

		// Update existing product or add new one
		if (existingProductIndex >= 0) {
			orderDetails[existingProductIndex].quantity += productData.quantity;
			orderDetails[existingProductIndex].note = productData.note || orderDetails[existingProductIndex].note;
		} else {
			orderDetails.push(productData);
		}

		isChangeOrderDetails = true;

		// Update UI
		renderOrderDetail();
		updateOrderTotals();

		AlertResponse("Sản phẩm đã được thêm vào đơn hàng", "success");
	}

	/**
	 * Tìm kiếm sản phẩm - delegate to ProductSearchUtils
	 */
	function searchProducts(query) {
		if (typeof ProductSearchUtils !== "undefined" && ProductSearchUtils.searchProducts) {
			ProductSearchUtils.searchProducts(query, "#order-detail__search__result");
		}
	}

	/**
	 * Xóa sản phẩm khỏi đơn hàng
	 */
	function removeProduct(productId) {
		try {
			if (confirm("Bạn có chắc chắn muốn xóa sản phẩm này khỏi đơn hàng?")) {
				isChangeOrderDetails = true;

				const product = orderDetails.find((item) => item.productId === productId);
				orderDetails = orderDetails.filter((item) => item.productId !== productId);

				renderOrderDetail();
				updateOrderTotals();

				if (product) {
					AlertResponse(`Đã xóa sản phẩm "${product.productName}"`, "success");
				}
			}
		} catch (err) {
			console.error(err);
			isChangeOrderDetails = false;
			AlertResponse("Đã xảy ra lỗi khi xóa sản phẩm!", "error");
		}
	}

	/**
	 * Hiển thị UI để sửa số lượng sản phẩm
	 */
	function editQuantity(span, productId) {
		const originalValue = $(span).text().trim();

		const inputGroup = document.createElement("div");
		inputGroup.className = "input-group input-group-sm";
		inputGroup.style.width = "120px";

		// Minus button
		const minusBtn = document.createElement("button");
		minusBtn.className = "btn btn-outline-secondary";
		minusBtn.type = "button";
		minusBtn.innerHTML = "<i class='ri-subtract-line'></i>";
		minusBtn.onclick = function () {
			const currentVal = parseInt(input.value) || 1;
			if (currentVal > 1) {
				input.value = currentVal - 1;
			}
		};

		// Input
		const input = document.createElement("input");
		input.type = "number";
		input.className = "form-control text-center";
		input.value = originalValue;
		input.min = "1";

		// Plus button
		const plusBtn = document.createElement("button");
		plusBtn.className = "btn btn-outline-secondary";
		plusBtn.type = "button";
		plusBtn.innerHTML = "<i class='ri-add-line'></i>";
		plusBtn.onclick = function () {
			const currentVal = parseInt(input.value) || 1;
			input.value = currentVal + 1;
		};

		// Save button
		const saveBtn = document.createElement("button");
		saveBtn.className = "btn btn-primary";
		saveBtn.type = "button";
		saveBtn.innerHTML = "<i class='ri-check-line'></i>";
		saveBtn.onclick = function () {
			saveQuantity();
		};

		// Add to group
		inputGroup.appendChild(minusBtn);
		inputGroup.appendChild(input);
		inputGroup.appendChild(plusBtn);
		inputGroup.appendChild(saveBtn);

		// Save handler
		function saveQuantity() {
			const newValue = parseInt(input.value) || 1;
			if (newValue < 1) {
				AlertResponse("Số lượng phải lớn hơn 0", "warning");
				input.value = 1;
				return;
			}

			updateProductQuantity(productId, newValue);

			$(span).text(newValue);
			$(span).show();
			$(inputGroup).remove();
		}

		// Events
		input.onkeypress = function (event) {
			if (event.key === "Enter") {
				event.preventDefault();
				saveQuantity();
			}
		};

		input.onblur = function () {
			if (!$(inputGroup).is(":hover")) {
				saveQuantity();
			}
		};

		// Replace span with input
		$(span).hide();
		$(span).after(inputGroup);
		input.focus();
		input.select();
	}

	/**
	 * Cập nhật số lượng sản phẩm
	 */
	function updateProductQuantity(productId, newQuantity) {
		try {
			const product = orderDetails.find((item) => item.productId === productId);
			if (product) {
				product.quantity = parseInt(newQuantity);
				isChangeOrderDetails = true;
				updateOrderTotals();
			}
		} catch (err) {
			console.error("Error updating product quantity:", err);
			AlertResponse("Cập nhật số lượng thất bại!", "error");
		}
	}

	/**
	 * Hiển thị danh sách sản phẩm trong đơn hàng
	 */
	function renderOrderDetail() {
		if (!orderDetails || orderDetails.length === 0) {
			$("#order-details").html(`
				<div class="alert alert-info text-center">
					<i class="ri-information-line mr-2"></i> Chưa có sản phẩm nào trong đơn hàng
				</div>
			`);
			return;
		}

		let htmlRender = "";
		orderDetails.forEach((item, index) => {
			// Display variant names if available, otherwise fallback to IDs
			let propertyValuesDisplay = "Không có";

			if (item.propertyValues && item.propertyValues.length > 0) {
				// Use stored variant names if available
				propertyValuesDisplay = item.propertyValues.join(", ");
			}

			// Cho phép xóa và sửa nếu đơn hàng chưa hoàn thành
			const isCompleteOrCancelled = $("#orderStatus").val() == "2" || $("#orderStatus").val() == "3";

			const actionButtons = !isCompleteOrCancelled
				? `<button hidden class="btn btn-sm btn-danger" onclick="OrderDetail.removeProduct('${item.productId}')">
					<i class="ri-delete-bin-line fs-6 mr-1"></i>Xóa
				  </button>`
				: "";

			htmlRender += `
				<div class="shadow-sm p-3 mb-4 bg-white rounded">
					<div class="d-flex px-4 align-items-center justify-content-between">
						<div class="col-5 d-flex align-items-center">
							<img src="${item.images && item.images.length > 0 ? item.images[0] : "/images/product/01.png"}" 
								 class="img-fluid rounded avatar-50 mr-3" alt="image">
							<div class="col-8 flex flex-column justify-content-center">
								<p class="mb-0 font-weight-bold">${item.productName}</p>
								<p class="mb-0 text-muted small">Ghi chú: ${item.note || "Không có"}</p>
								<p class="mb-0 text-muted small">Chi tiết: ${propertyValuesDisplay}</p>
							</div>
						</div>
						<div class="text-right">
							${item.isDiscounted ? `<p class="mb-0 text-muted"><del>${item.originalPrice.toLocaleString("vi-VN")}đ</del></p>` : ""}
							<p class="mb-0">
								${(item.discountPrice || item.originalPrice).toLocaleString("vi-VN")}đ x 
								<!-- <span style="cursor:pointer" class="product-qty-edit"
									onclick="OrderDetail.editQuantity(this, '${item.productId}')">
									${item.quantity}
								</span> -->
								<span style="cursor:pointer" class="product-qty-edit">
									${item.quantity}
								</span>
							</p>
							<p class="mb-0 font-weight-bold">
								${((item.discountPrice || item.originalPrice) * item.quantity).toLocaleString("vi-VN")}đ
							</p>
						</div>
						${actionButtons}
					</div>
				</div>
			`;
		});

		$("#order-details").html(htmlRender);
	}

	/**
	 * Cập nhật tổng tiền đơn hàng
	 */
	function updateOrderTotals() {
		if (!orderDetails || orderDetails.length === 0) {
			$("#order_tamtinh").text("0 đ");
			$("#order_discount").text("0 đ");
			$("#order_total").text("0 đ");
			return;
		}

		$("#order_tamtinh").html('<div class="spinner-border spinner-border-sm" role="status"></div>');
		$("#order_total").html('<div class="spinner-border spinner-border-sm" role="status"></div>');

		const data = {
			id: orderId,
			address: $("#deliveryAddress").val(),
			paymentStatus: $("#paymentStatus").val(),
			orderStatus: $("#orderStatus").val(),
			paymentMethod: $("#paymentMethod").val(),
			note: $("#notes").val(),
			products: orderDetails,
		};

		$.ajax({
			url: `/api/orders/update-products/${orderId}`, // API tính tổng tiền lại của đơn hàng đó
			type: "PUT",
			contentType: "application/json",
			data: JSON.stringify(data),
			success: function (response) {
				if (response.code === 0 && response.data) {
					const { subTotalAmount, discountAmount, totalAmount, shippingFee } = response.data.order;

					orderDetails = response.data.products;

					$("#order_tamtinh").text(`${(subTotalAmount || 0).toLocaleString("vi-VN")} đ`);
					$("#order_discount").text(`${(discountAmount || 0).toLocaleString("vi-VN")} đ`);
					$("#order_ship").text(`${(shippingFee || 0).toLocaleString("vi-VN")} đ`);
					$("#order_total").text(`${(totalAmount || 0).toLocaleString("vi-VN")} đ`);

					renderOrderDetail();
				} else {
					AlertResponse(response.message || "Không thể cập nhật thông tin sản phẩm", "error");
				}
			},
			error: function (xhr) {
				console.error("Error updating order products:", xhr.responseText);
				AlertResponse("Đã xảy ra lỗi khi cập nhật thông tin sản phẩm", "error");
				renderOrderDetail();
			},
		});
	}

	/**
	 * Xử lý thay đổi trạng thái đơn hàng
	 */
	function handleOrderStatusChange(select) {
		const orderStatus = parseInt($(select).val());

		if (orderStatus === 2) {
			// Completed
			$("#paymentStatus").val("1"); // Set payment status to Paid
		}

		const isEditingDisabled = orderStatus === 2 || orderStatus === 3; // Completed or Cancelled
		$("#order-detail-query").prop("disabled", isEditingDisabled);

		renderOrderDetail(); // Re-render để cập nhật UI theo trạng thái mới
	}

	/**
	 * Lưu hoặc cập nhật đơn hàng
	 */
	function handleSaveOrUpdate() {
		const id = orderId;

		// Format dữ liệu theo yêu cầu OrderDetailRequest
		const formattedProducts = orderDetails.map((product) => ({
			productId: product.productId,
			quantity: product.quantity,
			note: product.note || null,
			refCode: product.refCode || null,
			variantId: product.variantId || null,
			cartItemId: product.cartItemId || null,
		}));

		const ck = OrderCustomFields.validate();
		if (!ck.ok) { AlertResponse(ck.errors.join("<br>"), "warning"); return; }

		const data = {
			id: id,
			address: $("#deliveryAddress").val(),
			paymentStatus: $("#paymentStatus").val(),
			orderStatus: $("#orderStatus").val(),
			paymentMethod: $("#paymentMethod").val(),
			paymentChannel: $("#paymentChannel").val() || $("#paymentMethod").val(),
			note: $("#notes").val(),
			isChangeOrderDetails: isChangeOrderDetails,
			voucherCode: $("#voucherCode").val() || null,
			products: formattedProducts,
		};

		if (!data.products.length > 0) {
			AlertResponse("Chi tiết đơn hàng không có sản phẩm, vui lòng thêm sản phẩm!", "warning");
			return;
		}

		data.customFields = OrderCustomFields.collect();
		data.removedCustomFieldIds = OrderCustomFields.getRemoved(); 

		// Hiển thị trạng thái đang lưu
		$("#modal-order .modal-footer button").prop("disabled", true);
		$("#modal-order .modal-footer button[type='submit']").html('<i class="spinner-border spinner-border-sm mr-1"></i> Đang lưu...');

		const url = `/api/orders/${id}`;

		$.ajax({
			url: url,
			type: "PUT",
			contentType: "application/json",
			data: JSON.stringify(data),
			success: function (response) {
				if (response.code === 0) {
					AlertResponse(response.message || "Đã cập nhật đơn hàng thành công!", "success");
					$("#modal-order").modal("hide");
					if (typeof table !== "undefined") {
						table.ajax.reload(null, false);
					}
				} else {
					AlertResponse(response.message || "Lỗi khi cập nhật đơn hàng!", "error");
				}
			},
			error: function (xhr) {
				AlertResponse(xhr.responseJSON?.message || "Cập nhật đơn hàng thất bại!", "error");
			},
			complete: function () {
				$("#modal-order .modal-footer button").prop("disabled", false);
				$("#modal-order .modal-footer button[type='submit']").html("Lưu");
			},
		});
	}

	/**
	 * Thay đổi số lượng trong modal - use ProductSearchUtils
	 */
	function changeModalQuantity(change) {
		if (typeof ProductSearchUtils !== "undefined" && ProductSearchUtils.changeModalQuantity) {
			ProductSearchUtils.changeModalQuantity(change);
		}
	}

	/**
	 * Khởi tạo module
	 */
	function init(currentOrderId, initialOrderDetails) {
		orderId = currentOrderId;
		orderDetails = initialOrderDetails || [];

		// Initial render
		renderOrderDetail();

		// Initialize product search
		if (typeof ProductSearchUtils !== "undefined" && ProductSearchUtils.initProductSearch) {
			ProductSearchUtils.initProductSearch({
				inputSelector: "#order-detail-query",
				resultSelector: "#order-detail__search__result",
				onProductSelect: addProduct,
				debounceTime: 400,
			});
		}
	}

	// Public API
	return {
		init: init,
		addProductToOrder: addProduct,
		processSelectedProduct: processSelectedProduct,
		searchProducts: searchProducts,
		removeProduct: removeProduct,
		editQuantity: editQuantity,
		handleOrderStatusChange: handleOrderStatusChange,
		handleSaveOrUpdate: handleSaveOrUpdate,
		changeModalQuantity: changeModalQuantity,
		getOrderDetails: function () {
			return {
				products: orderDetails,
				isChanged: isChangeOrderDetails,
			};
		},
	};
})();
