/**
 * Initialize a Quill editor with custom image upload and deletion
 * @param {string} editorSelector - CSS selector for the editor container
 * @param {string} previewSelector - CSS selector for preview container (optional)
 * @returns {Quill} The Quill editor instance
 */
function createQuillEditor(editorSelector, previewSelector, maxLength = null, counterSelector = null) {
	// Custom image upload handler
	const imageHandler = function () {
		const input = document.createElement("input");
		input.setAttribute("type", "file");
		input.setAttribute("accept", "image/*");
		input.click();

		input.onchange = () => {
			const file = input.files[0];
			if (!file) return;

			handleImageUpload(file, this.quill);
		};
	};

	// Function to handle image upload (used by both file selection and paste)
	function handleImageUpload(file, quill) {
		const formData = new FormData();
		formData.append("image", file);

		const range = quill.getSelection(true);
		quill.insertText(range.index, "Đang tải ảnh lên...", "italic", true);

		$.ajax({
			url: "/api/helpers/UploadImage",
			type: "POST",
			data: formData,
			processData: false,
			contentType: false,
			success: function (response) {
				const { fileUrl, fileName } = response.data;

				// Xóa đoạn "Đang tải ảnh lên..."
				quill.deleteText(range.index, "Đang tải ảnh lên...".length);

				// Chèn ảnh vào editor
				quill.insertEmbed(range.index, "image", fileUrl);
				// Sau khi ảnh được render, gắn thêm data-filename
				setTimeout(() => {
					const imgs = quill.root.querySelectorAll("img");
					imgs.forEach((img) => {
						if (img.src === fileUrl) {
							img.setAttribute("data-filename", fileName);
						}
					});
				}, 50);
			},
			error: function (error) {
				console.error("Error uploading image:", error);
				quill.deleteText(range.index, "Đang tải ảnh lên...".length);
				quill.insertText(range.index, "Lỗi tải ảnh lên", "bold", true);
			},
		});
	}

	// Initialize Quill with options
	const quill = new Quill(editorSelector, {
		theme: "snow",
		modules: {
			imageResize: {
				displaySize: true,
			},
			clipboard: {
				matchers: [
					[
						"img",
						function (node, delta) {
							// We're handling images in the paste event handler
							// This just passes through
							return delta;
						},
					],
				],
			},
			toolbar: {
				container: [[{ header: [1, 2, 3, false] }], ["bold", "italic", "underline", "strike"], [{ list: "ordered" }, { list: "bullet" }], [{ color: [] }, { background: [] }], [{ align: [] }], ["blockquote", "code-block"], ["image"], ["link"], ["clean"]],
				handlers: {
					image: imageHandler,
				},
			},
		},
	});

	// Handle paste events for images
	quill.root.addEventListener("paste", function (e) {
		if (e.clipboardData && e.clipboardData.items) {
			const items = e.clipboardData.items;

			for (let i = 0; i < items.length; i++) {
				if (items[i].type.indexOf("image") !== -1) {
					e.preventDefault();
					const file = items[i].getAsFile();
					if (file) {
						handleImageUpload(file, quill);
					}
					break;
				}
			}
		}
	});

	// Track all images in the editor to detect deletions
	let imagesInEditor = extractImagesFromContent(quill.root.innerHTML);

	// Handle image deletion by keyboard (Backspace/Delete)
	quill.root.addEventListener("keydown", function (e) {
		const selection = window.getSelection();
		if (!selection.rangeCount) return;

		const range = selection.getRangeAt(0);
		const node = range.startContainer;

		let imgNode = null;

		if (node.nodeType === Node.ELEMENT_NODE && node.tagName === "IMG") {
			imgNode = node;
		} else if (node.nodeType === Node.TEXT_NODE && node.parentNode && node.parentNode.tagName === "IMG") {
			imgNode = node.parentNode;
		}

		if (imgNode && imgNode.tagName === "IMG") {
			const fileName = imgNode.getAttribute("data-filename");
			const imageUrl = imgNode.getAttribute("src");

			if (e.key === "Delete" || e.key === "Backspace") {
				// Xóa ảnh trong Quill
				const blot = Quill.find(imgNode);
				if (blot) {
					const index = quill.getIndex(blot);
					quill.deleteText(index, 1);
				}

				// Gửi yêu cầu xóa ảnh lên server nếu có fileName
				if (fileName) {
					deleteImage(imageUrl, fileName);
				}

				e.preventDefault();
			}
		}
	});

	// Text-change để update preview và phát hiện xóa ảnh
	quill.on("text-change", function () {
		const plainText = quill.getText(); // Lấy nội dung không có HTML

		// Giới hạn n ký tự
		if (maxLength && plainText.length > maxLength) {
			quill.deleteText(maxLength, plainText.length);
			//AlertResponse(`Nội dung không được vượt quá ${maxLength} ký tự.`, "warning");
			return;
		}

		const content = quill.root.innerHTML;

		if (previewSelector) {
			if (content === "<p><br></p>") {
				$(previewSelector).html("Nội dung");
			} else {
				$(previewSelector).html(content);
			}
		}

		const currentImages = extractImagesFromContent(content);
		const deletedImages = findDeletedImages(imagesInEditor, currentImages);

		if (deletedImages.length > 0) {
			deletedImages.forEach((imageUrl) => {
				deleteImage(imageUrl); // nếu fileName không có, vẫn truyền url
			});
		}

		imagesInEditor = currentImages;
	});

	// Store counter element (if any)
	let counterEl = null;
	if (counterSelector) {
		counterEl = document.querySelector(counterSelector);
	}

	// Optional character counter
	function updateCounter() {
		if (!counterEl || !maxLength) return;
		const plainText = quill.getText().trim();
		const currentLength = plainText.length;
		counterEl.textContent = `${currentLength}/${maxLength} kí tự`;
	}

	// Handle text-change
	quill.on("text-change", function () {
		const plainText = quill.getText().trim();

		// Enforce max length
		if (maxLength && plainText.length > maxLength) {
			quill.deleteText(maxLength, plainText.length);
			updateCounter();
			return;
		}

		updateCounter();

		const content = quill.root.innerHTML;

		if (previewSelector) {
			if (content === "<p><br></p>") {
				$(previewSelector).html("Nội dung");
			} else {
				$(previewSelector).html(content);
			}
		}

		const currentImages = extractImagesFromContent(content);
		const deletedImages = findDeletedImages(imagesInEditor, currentImages);

		if (deletedImages.length > 0) {
			deletedImages.forEach((imageUrl) => {
				deleteImage(imageUrl);
			});
		}

		imagesInEditor = currentImages;
	});

	// Initial counter
	updateCounter();

	return quill;
}

/**
 * Extract all image URLs from HTML content
 * @param {string} htmlContent - HTML content to parse
 * @returns {Array} Array of image URLs
 */
function extractImagesFromContent(htmlContent) {
	const images = [];
	const tempDiv = document.createElement("div");
	tempDiv.innerHTML = htmlContent;

	const imgElements = tempDiv.querySelectorAll("img");
	imgElements.forEach((img) => {
		if (img.src) {
			images.push(img.src);
		}
	});

	return images;
}

/**
 * Find images that were deleted
 * @param {Array} previousImages - Previously tracked images
 * @param {Array} currentImages - Current images in editor
 * @returns {Array} Array of deleted image URLs
 */
function findDeletedImages(previousImages, currentImages) {
	return previousImages.filter((url) => !currentImages.includes(url));
}

/**
 * Request server to delete an image
 * @param {string} imageUrl - URL of image to delete
 * @param {string} [fileName] - Optional: filename to use for deletion
 */
function deleteImage(imageUrl, fileName = null) {
	const payload = {
		fileUrl: imageUrl,
		filename: fileName || imageUrl.split("/").pop(),
	};
	swal({
		title: "Cảnh báo",
		text: `Ảnh sẽ không thể phục hồi nếu như bạn xóa!`,
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
				url: "/api/helpers/DeleteImage",
				type: "DELETE",
				data: JSON.stringify(payload),
				contentType: "application/json",
				success: function (result) {
					console.log("Image deleted successfully:", payload.filename);
				},
				error: function (error) {
					console.error("Error deleting image:", error);
				},
			});
		}
	});
}
