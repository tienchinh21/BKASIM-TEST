/**
 * Shared Product Search Functions
 * Common functions for searching and displaying products across the application
 */
const ProductSearchUtils = (function () {
	// Internal cache to store recent search results
	const searchCache = new Map();
	const CACHE_TTL = 60000; // 60 seconds

	// Debounce function with configurable timeout
	function debounce(func, timeout = 300) {
		let timer;
		return (...args) => {
			clearTimeout(timer);
			timer = setTimeout(() => {
				func.apply(this, args);
			}, timeout);
		};
	}

	/**
	 * Search products with API, using cache when possible
	 * @param {string} query - Search query
	 * @param {string} resultSelector - CSS selector for results container
	 * @param {Function} onResultsDisplay - Optional callback after results are displayed
	 */
	function searchProducts(query, resultSelector, onResultsDisplay) {
		const $resultContainer = $(resultSelector);

		if (!query || query.length < 2) {
			$resultContainer.removeClass("show");
			return;
		}

		$resultContainer.html('<div class="text-center py-2"><div class="spinner-border spinner-border-sm" role="status"></div> Đang tìm kiếm...</div>');
		$resultContainer.addClass("show");

		// Check cache first
		const cacheKey = `search:${query.toLowerCase()}`;
		const cachedResult = searchCache.get(cacheKey);

		if (cachedResult && Date.now() - cachedResult.timestamp < CACHE_TTL) {
			displaySearchResults(cachedResult.data, resultSelector, onResultsDisplay);
			return;
		}

		$.ajax({
			url: "/api/products?stockStatus=1&stockStatus=2",
			type: "GET",
			data: {
				keyword: query,
				pageSize: 10,
			},
			success: function (res) {
				if (res.data && res.data.length > 0) {
					// Cache the successful results
					searchCache.set(cacheKey, {
						data: res.data,
						timestamp: Date.now(),
					});

					displaySearchResults(res.data, resultSelector, onResultsDisplay);
				} else {
					$resultContainer.html('<div class="text-center py-3">Không tìm thấy sản phẩm phù hợp</div>');
				}
			},
			error: function (xhr) {
				console.error("Product search error:", xhr.responseText);
				$resultContainer.html('<div class="text-center py-2 text-danger">Lỗi khi tìm kiếm</div>');
			},
		});
	}

	/**
	 * Display product search results with optimized DOM manipulations
	 * @param {Array} products - Array of product objects from API
	 * @param {string} selector - CSS selector for results container
	 * @param {Function} onResultsDisplay - Optional callback after results are displayed
	 */
	function displaySearchResults(products, selector, onResultsDisplay) {
		const $dropdown = $(selector);
		const fragment = document.createDocumentFragment();

		// Clear results once for better performance
		$dropdown.empty();

		products.forEach((product) => {
			const image = product.images && product.images.length > 0 ? product.images[0] : "/images/product/01.png";
			const price = product.price || product.discountPrice || 0;
			const originalPrice = product.originalPrice || price;
			const isDiscounted = originalPrice > price;
			const productTypeText = product.isGift ? "Quà tặng" : "Sản phẩm";

			// Escape product name for security
			const escapedName = $("<div>").text(product.name).html();

			// Create a dropdown item container
			const itemDiv = document.createElement("div");
			itemDiv.className = "dropdown-item p-3 border-bottom";
			itemDiv.setAttribute("data-product-id", product.id);

			// Set inner HTML with template (safer than creating DOM nodes for complex structures)
			itemDiv.innerHTML = `
                <div class="d-flex align-items-center">
                    <img src="${image}" class="img-fluid rounded" width="60" height="60" alt="${escapedName}" loading="lazy">
                    <div class="ml-3 flex-grow-1">
                        <h6 class="mb-0">${escapedName}</h6>
                        <div>
                            <small class="text-muted">${productTypeText}</small>
                            ${
								isDiscounted
									? `<span class="text-primary font-weight-bold">${price.toLocaleString("vi-VN")}đ</span>
                                    <small class="text-muted"><del>${originalPrice.toLocaleString("vi-VN")}đ</del></small>`
									: `<span class="text-primary font-weight-bold">${price.toLocaleString("vi-VN")}đ</span>`
							}
                        </div>
                    </div>
                    <button type="button" class="btn btn-sm btn-primary product-action-btn" data-product-id="${product.id}">
                        <i class="ri-add-line"></i> Thêm
                    </button>
                </div>`;

			fragment.appendChild(itemDiv);
		});

		// Use document fragment for single DOM update
		$dropdown.append(fragment);
		$dropdown.addClass("show");

		// Call the callback if provided
		if (typeof onResultsDisplay === "function") {
			onResultsDisplay(products, $dropdown);
		}
	}

	/**
	 * Initialize product search on an input field
	 * @param {Object} config - Configuration object
	 * @param {string} config.inputSelector - CSS selector for search input
	 * @param {string} config.resultSelector - CSS selector for results container
	 * @param {Function} config.onProductSelect - Callback when product is selected
	 * @param {Function} config.onResultsDisplay - Optional callback after results are displayed
	 * @param {number} config.debounceTime - Debounce time in milliseconds (default: 300)
	 */
	function initProductSearch(config) {
		const { inputSelector, resultSelector, onProductSelect, onResultsDisplay = null, debounceTime = 300 } = config;

		// Set up debounced search on input
		const debouncedSearch = debounce(function () {
			const query = $(inputSelector).val();
			searchProducts(query, resultSelector, onResultsDisplay);
		}, debounceTime);

		// Remove existing event handlers to prevent duplication
		$(inputSelector).off("input.productSearch").on("input.productSearch", debouncedSearch);

		// Unified event delegation for product selection
		$(document)
			.off("click.productAction", `${resultSelector} .product-action-btn`)
			.on("click.productAction", `${resultSelector} .product-action-btn`, function (e) {
				e.preventDefault();
				const productId = $(this).data("product-id");
				if (onProductSelect && typeof onProductSelect === "function") {
					onProductSelect(productId);
					$(resultSelector).removeClass("show");
				}
			});

		// Handle click outside to close dropdown
		$(document)
			.off("click.outsideSearch")
			.on("click.outsideSearch", function (e) {
				if (!$(e.target).closest(`${inputSelector}, ${resultSelector}`).length) {
					$(resultSelector).removeClass("show");
				}
			});

		// Optional keyboard navigation
		$(inputSelector)
			.off("keydown.productSearch")
			.on("keydown.productSearch", function (e) {
				const $dropdown = $(resultSelector);
				if (!$dropdown.hasClass("show")) return;

				if (e.key === "Escape") {
					$dropdown.removeClass("show");
					e.preventDefault();
				}
			});
	}

	/**
	 * Clear the search cache
	 */
	function clearCache() {
		searchCache.clear();
	}

	/**
	 * Render product detail in a target container
	 * @param {Object} product - Product object
	 * @param {string} targetSelector - CSS selector for target container
	 */
	function renderProductDetail(product, targetSelector) {
		const hasVariants = product.variants && product.variants.length > 0;
		const hasOptions = product.options && product.options.length > 0;

		let optionsHtml = "";

		if (hasOptions) {
			product.options.forEach((option) => {
				const variantsHtml = option.variants
					.map((variant) => {
						return `
                    <div class="form-check form-check-inline">
                        <input class="form-check-input variant-selector" type="radio" name="${option.propertyId}" 
                            id="${variant.propertyValueId}" value="${variant.propertyValueId}">
                        <label class="form-check-label" for="${variant.propertyValueId}">
                            ${variant.propertyValueName}
                        </label>
                    </div>`;
					})
					.join("");

				optionsHtml += `
            <div class="mb-3">
                <label class="font-weight-bold">${option.optionName}:</label>
                <div>${variantsHtml}</div>
            </div>`;
			});
		}

		// Hiển thị thông tin giá
		const priceDisplay = product.isDiscounted
			? `<p class="mb-2" id="product-price-display">
            <span class="text-muted"><del>${product.originalPrice.toLocaleString("vi-VN")}đ</del></span>
            <span class="text-danger font-weight-bold">${product.discountPrice.toLocaleString("vi-VN")}đ</span>
            </p>`
			: `<p class="mb-2" id="product-price-display">
            <span class="text-primary font-weight-bold">${product.originalPrice.toLocaleString("vi-VN")}đ</span>
            </p>`;

		const modalContent = `
        <div>
            <h5 class="mb-3">${product.name}</h5>
            <div class="row mb-3">
                <div class="col-md-5">
                    <img src="${product.images && product.images.length > 0 ? product.images[0] : "/images/product/01.png"}" 
                        class="img-fluid rounded" alt="${product.name}">
                </div>
                <div class="col-md-7">
                    ${priceDisplay}
                    <div class="mb-3">
                       <!-- ${product.description ? `<small>${product.description}</small>` : ""} -->
                    </div>
                    ${optionsHtml}
                    <div class="mb-3">
                        <label class="font-weight-bold">Số lượng:</label>
                        <div class="input-group" style="width: 150px">
                            <div class="input-group-prepend">
                                <button class="btn btn-outline-secondary" type="button" 
                                    onclick="${window.currentOrderContext === "update" ? "OrderDetail" : "ProductSearchUtils"}.changeModalQuantity(-1)">
                                    <i class="ri-subtract-line"></i>
                                </button>
                            </div>
                            <input type="number" id="product-quantity" class="form-control text-center" value="1" min="1">
                            <div class="input-group-append">
                                <button class="btn btn-outline-secondary" type="button" 
                                    onclick="${window.currentOrderContext === "update" ? "OrderDetail" : "ProductSearchUtils"}.changeModalQuantity(1)">
                                    <i class="ri-add-line"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                    <div class="mb-3">
                        <label class="font-weight-bold">Ghi chú:</label>
                        <textarea id="product-note" class="form-control" rows="2" placeholder="Nhập ghi chú đặc biệt cho sản phẩm này..."></textarea>
                    </div>
                </div>
            </div>
        </div>
    `;

		// Add content to the target
		$(targetSelector).html(modalContent);

		// Set up event handlers for variant selection
		if (hasVariants && hasOptions) {
			$(targetSelector)
				.find('input[type="radio"]')
				.on("change", function () {
					updateVariantPrice(product);
				});
		}
	}

	/**
	 * Update price display when variants are selected
	 * @param {Object} product - Product object with variants
	 */
	function updateVariantPrice(product) {
		// Get all selected variant IDs
		let selectedVariants = [];
		$("input[type='radio']:checked").each(function () {
			selectedVariants.push($(this).val());
		});

		// Only update price if we have all required selections
		if (product.options && selectedVariants.length === product.options.length) {
			// Find the matching variant
			let matchedVariant = null;
			if (product.variants) {
				matchedVariant = product.variants.find((v) => {
					const vIds = v.propertyValueIds || [];
					return vIds.length === selectedVariants.length && vIds.every((id) => selectedVariants.includes(id));
				});
			}

			// Update price if we found a matching variant
			if (matchedVariant) {
				const originalPrice = matchedVariant.originalPrice;
				const discountPrice = matchedVariant.discountPrice || originalPrice;
				const isDiscounted = discountPrice < originalPrice;

				let priceHtml = "";
				if (isDiscounted) {
					priceHtml = `
                    <span class="text-muted"><del>${originalPrice?.toLocaleString("vi-VN")}đ</del></span>
                    <span class="text-danger font-weight-bold">${discountPrice?.toLocaleString("vi-VN")}đ</span>
                `;
				} else {
					priceHtml = `<span class="text-primary font-weight-bold">${originalPrice.toLocaleString("vi-VN")}đ</span>`;
				}

				// Replace the price display
				$("#product-price-display").html(priceHtml);

				// Update global product price references for subsequent operations
				window.currentProductDetail.selectedVariantPrice = discountPrice;
				window.currentProductDetail.selectedVariantOriginalPrice = originalPrice;
			}
		}
	}

	/**
	 * Load product detail from API and render it
	 * @param {string} productId - Product ID to load
	 * @param {string} targetSelector - Where to render the product
	 * @param {Function} callback - Optional callback with product data
	 */
	function loadProductDetail(productId, targetSelector, callback) {
		$(targetSelector).html('<div class="text-center py-4"><div class="spinner-border text-primary" role="status"></div><p class="mt-2">Đang tải thông tin sản phẩm...</p></div>');

		$.ajax({
			url: `/api/products/${productId}`,
			type: "GET",
			success: function (response) {
				if (response.code === 0 && response.data) {
					renderProductDetail(response.data, targetSelector);
					if (typeof callback === "function") {
						callback(response.data);
					}
				} else {
					$(targetSelector).html('<div class="text-center py-4 text-danger"><i class="ri-error-warning-line ri-3x"></i><p class="mt-2">Không thể tải thông tin sản phẩm</p></div>');
				}
			},
			error: function () {
				$(targetSelector).html('<div class="text-center py-4 text-danger"><i class="ri-error-warning-line ri-3x"></i><p class="mt-2">Đã xảy ra lỗi khi tải thông tin sản phẩm</p></div>');
			},
		});
	}

	/**
	 * Change quantity in product modal
	 * @param {number} change - Amount to change (positive or negative)
	 */
	function changeModalQuantity(change) {
		const input = document.getElementById("product-quantity");
		if (!input) return;

		let currentValue = parseInt(input.value) || 1;
		if (change > 0 || currentValue > 1) {
			currentValue += change;
			if (currentValue < 1) currentValue = 1;
			input.value = currentValue;
		}
	}

	/**
	 * Handle adding product from modal
	 * @param {Function} callback - Function to call with the selected product data
	 */
	function handleAddProductFromModal(callback) {
		try {
			const product = window.currentProductDetail;
			if (!product) {
				AlertResponse("Không tìm thấy thông tin sản phẩm", "error");
				return;
			}

			// Get selected variants
			let variantNames = [];
			let propertyValues = [];
			let selectedVariants = [];

			$("input[type='radio']:checked").each(function () {
				const valueId = $(this).val();
				selectedVariants.push(valueId);

				// Store variant name for display
				const optionName = $(this).closest(".mb-3").find("label.font-weight-bold").text().replace(":", "").trim();
				const valueName = $(this).siblings("label").text();
				variantNames.push(`${optionName}: ${valueName}`);
				propertyValues.push(valueName);
			});

			const quantity = parseInt($("#product-quantity").val()) || 1;
			const note = $("#product-note").val();

			// Check if variants should be selected
			const hasOptions = $("input[type='radio']").length > 0;
			if (hasOptions && selectedVariants.length === 0) {
				AlertResponse("Vui lòng chọn biến thể sản phẩm", "warning");
				return;
			}

			// Use selectedVariantPrice if available
			let productPrice = product.originalPrice;
			let discountPrice = product.discountPrice || productPrice;

			if (product.selectedVariantOriginalPrice) {
				productPrice = product.selectedVariantOriginalPrice;
			}

			if (product.selectedVariantPrice) {
				discountPrice = product.selectedVariantPrice;
			}

			// Find variant ID if variants exist
			let variantId = null;
			if (selectedVariants.length > 0 && product.variants) {
				const matchingVariant = product.variants.find((v) => JSON.stringify(v.propertyValueIds.sort()) === JSON.stringify(selectedVariants.sort()));

				if (matchingVariant) {
					variantId = matchingVariant.variantId;
				}
			}

			// Create product data object to pass back to the caller
			const productData = {
				productId: product.id,
				productName: product.name,
				productPrice: productPrice,
				discountPrice: discountPrice,
				isDiscounted: productPrice !== discountPrice,
				quantity: quantity,
				note: note,
				images: product.images,
				propertyValueIds: selectedVariants,
				variantNames: variantNames,
				propertyValues: propertyValues,
				variantId: variantId,
			};

			// Close modal
			$("#product-detail-modal").modal("hide");

			// Call the provided callback with the product data
			if (callback && typeof callback === "function") {
				callback(productData);
			}
		} catch (error) {
			console.error("Error processing product selection:", error);
			AlertResponse("Đã xảy ra lỗi khi xử lý thông tin sản phẩm", "error");
		}
	}

	/**
	 * Set up product modal for a specific context
	 * @param {string} context - 'new' for new orders, 'update' for updating orders
	 * @param {Function} addProductCallback - Callback to handle adding the product
	 */
	function setupProductModal(context, addProductCallback) {
		// Store the callback for later use
		window.currentAddProductCallback = addProductCallback;
		window.currentOrderContext = context;

		// Set up the add button
		const $addButton = $("#product-detail-modal").find("#add-to-order-btn");
		$addButton.off("click").on("click", function () {
			handleAddProductFromModal(window.currentAddProductCallback);
		});

		if (context === "update") {
			$addButton.html('<i class="ri-shopping-cart-line mr-1"></i> Thêm vào đơn hàng');
		} else {
			$addButton.html('<i class="ri-shopping-cart-line mr-1"></i> Thêm vào đơn hàng mới');
		}
	}

	// Return public API
	return {
		searchProducts: searchProducts,
		displaySearchResults: displaySearchResults,
		initProductSearch: initProductSearch,
		debounce: debounce,
		clearCache: clearCache,
		renderProductDetail: renderProductDetail,
		updateVariantPrice: updateVariantPrice,
		loadProductDetail: loadProductDetail,
		changeModalQuantity: changeModalQuantity,
		handleAddProductFromModal: handleAddProductFromModal,
		setupProductModal: setupProductModal,
	};
})();
