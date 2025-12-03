// Create Order module for better organization
const Order = (function () {
	// Private variables
	let orderProducts = [];
	let discountAmount = 0;
	let subTotal = 0;
	let total = 0;
	let appliedVouchers = [];
	let voucherDiscounts = {}; // Track individual voucher discounts

	// Initialize order form when document is ready
	function init() {
		// Reset form when modal opens
		$("#new-order").on("show.bs.modal", function () {
			resetOrderForm();
		});

		// Set up product search with ProductSearchUtils
		ProductSearchUtils.initProductSearch({
			inputSelector: "#new-order-search",
			resultSelector: "#new-order__search__result",
			onProductSelect: openProductDetailModal,
			debounceTime: 500,
		});

		// Initialize phone search with debounce for autocomplete
		$("#new-order__cusPhone").on("input", ProductSearchUtils.debounce(searchMembershipAutocomplete, 500));

		// Close dropdowns when clicking outside - handled by ProductSearchUtils for products

		// Handle membership dropdown closing
		$(document).on("click", function (e) {
			if (!$(e.target).closest("#new-order__cusPhone, #membership-search-results").length) {
				$("#membership-search-results").removeClass("show");
			}
		});

		// Prevent dropdown from closing when clicking inside
		$("#membership-search-results").on("click", function (e) {
			e.stopPropagation();
		});

		// Set up product detail modal
		setupProductDetailModal();
	}

	// Function to set up the product detail modal
	function setupProductDetailModal() {
		ProductSearchUtils.setupProductModal("new", addProductFromModal);
	}

	// Open product detail modal
	function openProductDetailModal(productId) {
		// Hide search dropdown
		$("#new-order__search__result").removeClass("show");
		$("#new-order-search").val("");

		// Show modal and load product details
		$("#product-detail-modal").modal("show");

		// Use ProductSearchUtils to load product details
		ProductSearchUtils.loadProductDetail(productId, "#product-detail-modal .modal-body", function (product) {
			// Store the product data for reference
			window.currentProductDetail = product;
		});
	}

	// Add product from modal to order
	function addProductFromModal(productData) {
		// Check if product with same variant exists in order
		const existingProductIndex = orderProducts.findIndex((p) => p.productId === productData.productId && p.variantId === productData.variantId);

		if (existingProductIndex >= 0) {
			// Increment quantity if product already in order
			orderProducts[existingProductIndex].quantity += productData.quantity;
			orderProducts[existingProductIndex].note = productData.note || orderProducts[existingProductIndex].note;
		} else {
			// Add new product to order
			orderProducts.push(productData);
		}

		// Update order UI
		renderOrderProducts();
		updateOrderTotals();

		// Show success message
		AlertResponse("Sản phẩm đã được thêm vào đơn hàng", "success");
	}

	// Render product list in order
	function renderOrderProducts() {
		const productList = $("#new-order-product-list");
		productList.empty();

		if (orderProducts.length === 0) {
			productList.html(`
                <tr id="empty-product-row">
                    <td colspan="5" class="text-center py-3">Chưa có sản phẩm nào được thêm vào đơn hàng</td>
                </tr>
            `);
			return;
		}

		orderProducts.forEach((product, index) => {
			const imageUrl = product.images && product.images.length > 0 ? product.images[0] : "/images/product/01.png";
			const isDiscounted = product.productPrice > product.discountPrice;
			const totalPrice = product.discountPrice * product.quantity;

			// Format price display based on discount status
			const priceDisplay = isDiscounted
				? `<div>
                    <span class="text-primary font-weight-bold">${product.discountPrice.toLocaleString("vi-VN")} đ</span>
                    <br>
                    <small class="text-muted text-decoration-line-through">${product.productPrice.toLocaleString("vi-VN")} đ</small>
                  </div>`
				: `<span class="text-primary font-weight-bold">${product.productPrice.toLocaleString("vi-VN")} đ</span>`;

			// Add variant info if exists
			const variantInfo = product.variantNames && product.variantNames.length > 0 ? `<small class="text-muted d-block">${product.variantNames.join(", ")}</small>` : "";

			// Add note if exists
			const noteHtml = product.note ? `<small class="text-muted d-block">Ghi chú: ${product.note}</small>` : "";

			const row = `
            <tr>
                <td>
                    <div class="d-flex align-items-center">
                        <img src="${imageUrl}" class="img-fluid rounded" width="40" height="40" alt="${product.productName}">
                        <div class="ml-2">
                            <h6 class="mb-0">${product.productName}</h6>
                            ${variantInfo}
                            ${noteHtml}
                        </div>
                    </div>
                </td>
                <td class="text-center">
                    ${priceDisplay}
                </td>
                <td>
                    <div class="input-group input-group-sm">
                        <button class="btn btn-outline-secondary" type="button" onclick="Order.changeProductQuantity(${index}, -1)">-</button>
                        <input type="number" class="form-control text-center" value="${product.quantity}" 
                               min="1" onchange="Order.updateProductQuantity(${index}, this.value)">
                        <button class="btn btn-outline-secondary" type="button" onclick="Order.changeProductQuantity(${index}, 1)">+</button>
                    </div>
                </td>
                <td class="text-end font-weight-bold">${totalPrice.toLocaleString("vi-VN")} đ</td>
                <td class="text-center">
                    <button class="btn btn-sm btn-danger" onclick="Order.removeProduct(${index})">
                        <i class="ri-delete-bin-line fs-6 mx-0"></i>
                    </button>
                </td>
            </tr>
            `;

			productList.append(row);
		});
	}

	// Change product quantity
	function changeProductQuantity(index, change) {
		const newQuantity = Math.max(1, orderProducts[index].quantity + change);
		orderProducts[index].quantity = newQuantity;
		renderOrderProducts();
		updateOrderTotals();
	}

	// Update product quantity directly
	function updateProductQuantity(index, newQuantity) {
		orderProducts[index].quantity = Math.max(1, parseInt(newQuantity) || 1);
		renderOrderProducts();
		updateOrderTotals();
	}

	// Remove product from order
	function removeProduct(index) {
		orderProducts.splice(index, 1);
		renderOrderProducts();
		updateOrderTotals();
	}

	// Update order totals (subtotal, discount, total)
	function updateOrderTotals() {
		subTotal = orderProducts.reduce((sum, product) => sum + product.productPrice * product.quantity, 0);
		total = orderProducts.reduce((sum, product) => sum + product.discountPrice * product.quantity, 0);

		$("#total-temp").text(subTotal.toLocaleString("vi-VN") + " đ");
		$("#discount-temp").text(discountAmount.toLocaleString("vi-VN") + " đ");
		$("#total").text(total.toLocaleString("vi-VN") + " đ");
	}

	// Add function to add a voucher
	function addVoucher() {
		const voucherCode = $("#new-order__voucher").val().trim();

		if (!voucherCode) {
			AlertResponse("Vui lòng nhập mã giảm giá", "warning");
			return;
		}

		if (appliedVouchers.includes(voucherCode)) {
			AlertResponse("Mã giảm giá này đã được áp dụng", "warning");
			return;
		}

		if (orderProducts.length === 0) {
			AlertResponse("Vui lòng thêm sản phẩm vào đơn hàng trước khi áp dụng mã giảm giá", "warning");
			return;
		}

		// Try to apply the voucher
		applyVoucherCode(voucherCode);
	}

	// Apply a voucher code
	function applyVoucherCode(voucherCode) {
		const orderData = {
			voucherCode: voucherCode,
			products: orderProducts.map((p) => ({
				productId: p.productId,
				quantity: p.quantity,
				note: p.note,
				variantId: p.variantId,
			})),
		};

		$.ajax({
			url: "/api/orders/apply-voucher",
			type: "POST",
			contentType: "application/json",
			data: JSON.stringify(orderData),
			success: function (response) {
				if (response.code === 0 && response.data) {
					// Add to applied vouchers
					appliedVouchers.push(voucherCode);

					// Store this voucher's discount
					voucherDiscounts[voucherCode] = response.data.discountAmount || 0;

					// Update discount amount (total of all vouchers)
					recalculateDiscounts();

					// Add to the UI
					addVoucherToUI(voucherCode, voucherDiscounts[voucherCode]);

					// Clear input
					$("#new-order__voucher").val("");

					AlertResponse("Áp dụng mã giảm giá thành công", "success");
				} else {
					AlertResponse(response.message || "Mã giảm giá không hợp lệ", "error");
				}
			},
			error: function () {
				AlertResponse("Đã xảy ra lỗi khi áp dụng mã giảm giá", "error");
			},
		});
	}

	// Function to add voucher to UI
	function addVoucherToUI(code, discount) {
		const voucherElement = `
            <div class="badge badge-primary p-2 mr-2 mb-2 voucher-badge" data-code="${code}">
                ${code} (-${discount.toLocaleString("vi-VN")}đ)
                <i class="ri-close-line ml-1" style="cursor: pointer;" onclick="Order.removeVoucher('${code}')"></i>
            </div>
        `;

		$("#applied-vouchers").append(voucherElement);
	}

	// Function to remove a voucher
	function removeVoucher(code) {
		// Remove from array
		appliedVouchers = appliedVouchers.filter((v) => v !== code);

		// Remove from discounts object
		delete voucherDiscounts[code];

		// Remove from UI
		$(`.voucher-badge[data-code="${code}"]`).remove();

		// Update totals
		recalculateDiscounts();

		AlertResponse("Đã xóa mã giảm giá", "info");
	}

	// Calculate total discount from all vouchers
	function recalculateDiscounts() {
		discountAmount = Object.values(voucherDiscounts).reduce((sum, discount) => sum + discount, 0);
		updateOrderTotals();
	}

	// Reset order form
	function resetOrderForm() {
		$("#new-order__cusPhone").val("");
		$("#new-order__memebershipName").val("");
		$("#new-order__deliveryAddress").val("");
		$("#new-order__voucher").val("");
		$("#new-order__notes").val("");
		$("#new-order__paymentMethod").val("1");

		orderProducts = [];
		discountAmount = 0;
		subTotal = 0;
		total = 0;

		appliedVouchers = [];
		voucherDiscounts = {};
		$("#applied-vouchers").empty();

		renderOrderProducts();
		updateOrderTotals();
	}

	// Create new order
	function createNewOrder() {
		// Validate required fields
		const phoneNumber = $("#new-order__cusPhone").val();
		const membershipName = $("#new-order__memebershipName").val();
		const deliveryAddress = $("#new-order__deliveryAddress").val();

		if (!phoneNumber) {
			AlertResponse("Vui lòng nhập số điện thoại khách hàng", "warning");
			return;
		}

		if (!membershipName) {
			AlertResponse("Vui lòng nhập tên khách hàng", "warning");
			return;
		}

		if (!deliveryAddress) {
			AlertResponse("Vui lòng nhập địa chỉ giao hàng", "warning");
			return;
		}

		if (orderProducts.length === 0) {
			AlertResponse("Vui lòng thêm ít nhất một sản phẩm vào đơn hàng", "warning");
			return;
		}

		// Prepare order data
		const orderData = {
			bookingId: $("#new-order_bookingId").val() || null,
			membershipName: membershipName,
			phoneNumber: phoneNumber,
			address: deliveryAddress,
			paymentMethod: $("#new-order__paymentMethod").val(),
			note: $("#new-order__notes").val(),
			voucherCode: appliedVouchers.length > 0 ? appliedVouchers[0] : null, // For backward compatibility
			voucherCodes: appliedVouchers, // Send all voucher codes
			products: orderProducts.map((p) => ({
				productId: p.productId,
				quantity: p.quantity,
				note: p.note,
				variantId: p.variantId,
			})),
		};

		// Send API request to create order
		$.ajax({
			url: "/api/orders/Create",
			type: "POST",
			contentType: "application/json",
			data: JSON.stringify(orderData),
			success: function (response) {
				if (response.code === 0) {
					AlertResponse("Tạo đơn hàng thành công", "success");
					$("#new-order").modal("hide");
					// Reload table to show new order
					table.ajax.reload(null, false);
				} else {
					AlertResponse(response.message || "Đã xảy ra lỗi khi tạo đơn hàng", "error");
				}
			},
			error: function () {
				AlertResponse("Đã xảy ra lỗi khi tạo đơn hàng", "error");
			},
		});
	}

	// Search for membership by phone with autocomplete
	function searchMembershipAutocomplete() {
		const phoneNumber = $("#new-order__cusPhone").val();
		const searchResultContainer = $("#membership-search-results");

		if (!phoneNumber || phoneNumber.length < 4) {
			searchResultContainer.removeClass("show");
			return;
		}
		searchResultContainer.html('<div class="text-center py-3"><div class="spinner-border spinner-border-sm text-primary" role="status"></div> Đang tìm kiếm...</div>');
		searchResultContainer.addClass("show");
		$.ajax({
			url: "/api/memberships",
			type: "GET",
			data: {
				keyword: phoneNumber,
				pageSize: 5,
			},
			success: function (response) {
				if (response.code === 0 && response.data && response.data.length > 0) {
					displayMembershipSearchResults(response.data, searchResultContainer);
				} else {
					searchResultContainer.html('<div class="text-center py-3">Không tìm thấy khách hàng</div>');
				}
			},
			error: function () {
				searchResultContainer.html('<div class="text-center py-3 text-danger">Đã xảy ra lỗi khi tìm kiếm</div>');
			},
		});
	}

	// Display membership search results in dropdown
	function displayMembershipSearchResults(members, container) {
		container.empty();

		members.forEach((member) => {
			const memberItem = `
            <div class="dropdown-item p-2 border-bottom" onclick="Order.selectMembership('${member.phoneNumber}', '${member.displayName || member.userZaloName || ""}', '${member.address || ""}')">
                <div class="d-flex align-items-center">
                    <div class="mr-3">
                        <i class="ri-user-line ri-lg"></i>
                    </div>
                    <div class="flex-grow-1">
                        <h6 class="mb-0">${member.displayName || member.userZaloName || "Chưa có tên"}</h6>
                        <small class="text-muted">${member.phoneNumber}</small>
                    </div>
                </div>
            </div>
            `;

			container.append(memberItem);
		});
	}

	// Select membership from dropdown
	function selectMembership(phoneNumber, name, address) {
		$("#new-order__cusPhone").val(phoneNumber);
		$("#new-order__memebershipName").val(name);
		$("#new-order__deliveryAddress").val(address);

		// Hide dropdown
		$("#membership-search-results").removeClass("show");

		AlertResponse("Đã chọn thông tin khách hàng", "success");
	}

	// Manual search function (when clicking the search button)
	function searchMembership() {
		const phoneNumber = $("#new-order__cusPhone").val();

		if (!phoneNumber || phoneNumber.length < 9) {
			AlertResponse("Vui lòng nhập số điện thoại hợp lệ", "warning");
			return;
		}

		$.ajax({
			url: "/api/memberships",
			type: "GET",
			data: {
				keyword: phoneNumber,
			},
			success: function (response) {
				if (response.code === 0 && response.data && response.data.length > 0) {
					const member = response.data[0];
					$("#new-order__memebershipName").val(member.name || member.userName || "");
					$("#new-order__deliveryAddress").val(member.address || "");

					AlertResponse("Đã tìm thấy thông tin khách hàng", "success");
				} else {
					// If member not found, we still keep the phone number,
					// backend will create new membership automatically
					$("#new-order__memebershipName").val("");
					$("#new-order__deliveryAddress").val("");

					AlertResponse("Không tìm thấy khách hàng với số điện thoại này. Hệ thống sẽ tạo mới khách hàng khi đặt hàng.", "info");
				}
			},
			error: function () {
				AlertResponse("Đã xảy ra lỗi khi tìm kiếm khách hàng", "error");
			},
		});
	}

	function handleCheckTableRow() {
		const isSelectAll = $("#selectAll").is(":checked");
		if (isSelectAll) {
			$("input[data-order-id]").prop("checked", true);
			$("#quick-update-container").show();
		} else {
			$("input[data-order-id]").prop("checked", false);
			$("#quick-update-container").hide();
		}
	}

	// Show quick update UI
	function showQuickUpdate() {
		const checkedRows = $(".order-checkbox:checked");
		if (checkedRows.length > 0) {
			$("#quick-update-container").show();
		} else {
			$("#quick-update-container").hide();
		}
	}

	// Handle quick update
	function handleQuickUpdate() {
		const orderStatus = $("#update-order-status").val();
		const paymentStatus = $("#updated-order-payment").val();

		const checkedOrders = [];
		$(".order-checkbox:checked").each(function () {
			checkedOrders.push($(this).data("order-id"));
		});

		if (checkedOrders.length === 0) {
			AlertResponse("Vui lòng chọn ít nhất một đơn hàng để cập nhật", "warning");
			return;
		}

		$.ajax({
			url: "/api/orders/QuickUpdate",
			type: "PUT",
			contentType: "application/json",
			data: JSON.stringify({
				orderIds: checkedOrders,
				orderStatus: orderStatus,
				paymentStatus: paymentStatus,
			}),
			success: function (response) {
				if (response.code === 0) {
					AlertResponse("Cập nhật trạng thái đơn hàng thành công", "success");
					table.ajax.reload(null, false);
					$("#quick-update-container").hide();
				} else {
					AlertResponse(response.message || "Cập nhật trạng thái thất bại", "error");
				}
			},
			error: function () {
				AlertResponse("Đã xảy ra lỗi khi cập nhật trạng thái đơn hàng", "error");
			},
		});
	}

	// Export data
	function handleExportData() {
		var $btn = $("#exportExcelOrderBtn");
		var originalHtml = $btn.html();

		const keyword = $("#search").val();
		const startDate = $("#start").val();
		const endDate = $("#end").val();
		const orderStatus = $("#filter-order-status").val();
		const paymentStatus = $("#filter-payment-status").val();
		const branchId = $("#filter-order-branch").val();

		//window.location.href = `/api/orders/export?keyword=${keyword}&startDate=${startDate}&endDate=${endDate}&orderStatus=${orderStatus}&paymentStatus=${paymentStatus}&branchId=${branchId === "all" ? "" : branchId}`;

		$btn
			.addClass("loading")
			.html('<span class="spinner-border spinner-border-sm me-1"></span>Đang Xuất');

		$.ajax({
			url: "/api/orders/export",
			method: "POST",
			xhrFields: {
				responseType: 'blob'
			},
			data: {
				keyword: keyword,
				startDate: startDate,
				endDate: endDate,
				orderStatus: orderStatus,
				paymentStatus: paymentStatus,
				branchId: branchId === "all" ? "" : branchId
			},
			success: function (blob, status, xhr) {
				// Lấy tên file từ header
				var disposition = xhr.getResponseHeader('Content-Disposition');
				var fileName = getFileNameFromDisposition(disposition);

				var url = window.URL.createObjectURL(blob);
				var a = document.createElement('a');
				a.href = url;
				a.download = fileName;
				document.body.appendChild(a);
				a.click();
				setTimeout(function () {
					window.URL.revokeObjectURL(url);
					document.body.removeChild(a);
				}, 100);
			},
			error: function (xhr) {
				if (xhr.responseJSON) {
					AlertResponse(xhr.responseJSON.Message || "Có lỗi xảy ra!", 'error');
				} else {
					AlertResponse("Có lỗi xảy ra!", 'error');
				}
			},
			complete: function () {
				// Kết thúc loading
				$btn.removeClass("loading").html(originalHtml);
			}
		});
	}

	// Initialize on page load
	$(document).ready(init);

	// Return public methods
	return {
		openProductDetailModal: openProductDetailModal,
		addProductFromModal: addProductFromModal,
		changeProductQuantity: changeProductQuantity,
		updateProductQuantity: updateProductQuantity,
		removeProduct: removeProduct,
		addVoucher: addVoucher,
		removeVoucher: removeVoucher,
		createNewOrder: createNewOrder,
		searchMembership: searchMembership,
		selectMembership: selectMembership,
		showQuickUpdate: showQuickUpdate,
		handleCheckTableRow: handleCheckTableRow,
		handleQuickUpdate: handleQuickUpdate,
		handleExportData: handleExportData,
	};
})();
