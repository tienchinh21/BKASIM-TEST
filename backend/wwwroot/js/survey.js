/**
 * Survey Module - Tối ưu để làm việc với Partial Views
 */
var SurveyModule = (function () {
	// Constants
	const QUESTION_TYPES = {
		paragraph: { label: "Đoạn văn", icon: "ri-text" },
		multiChoice: { label: "Nhiều lựa chọn", icon: "ri-checkbox-multiple-line" },
		singleChoice: { label: "Một lựa chọn", icon: "ri-radio-button-line" },
		likert: { label: "Thang đo Likert", icon: "ri-scales-line" },
		dropDown: { label: "Dropdown", icon: "ri-list-settings-line" },
	};

	const DELAYS = {
		EDITOR: 200,
		UI: 100,
	};

	// Private variables
	let surveyData = {
		id: "",
		sections: [],
	};

	let questionEditors = {};

	// ===== HELPER FUNCTIONS =====
	function findItem(array, key, value) {
		return array.find((item) => item[key] === value) || null;
	}

	function findQuestionById(questionId) {
		return surveyData.sections.flatMap((section) => section.questions).find((q) => q.id === questionId) || null;
	}

	function findSectionById(sectionId) {
		return findItem(surveyData.sections, "id", sectionId);
	}

	function findOptionById(questionId, optionId) {
		const question = findQuestionById(questionId);
		if (!question?.answers) return null;
		return findItem(question.answers, "id", optionId);
	}

	// ===== EDITOR MANAGEMENT =====
	function initQuillEditor(elementId, placeholder, content = "") {
		const editor = new Quill(elementId, {
			theme: "snow",
			placeholder: placeholder,
			modules: {
				toolbar: [["bold", "italic", "underline"], [{ color: [] }, { background: [] }], [{ list: "ordered" }, { list: "bullet" }], ["clean"]],
			},
		});

		if (content) {
			editor.root.innerHTML = content;
		}

		return editor;
	}

	function initSectionEditor(sectionId) {
		const editorContainer = $(`.section-item[data-section-id="${sectionId}"] .quill-question-editor`)[0];
		if (!editorContainer) return null;

		const editorId = `editor-section-${sectionId}`;

		// Clean up existing editor
		if (questionEditors[editorId]) {
			try {
				questionEditors[editorId].setText("");
			} catch (e) {
				console.log("Lỗi khi reset editor:", e);
				delete questionEditors[editorId];
			}
		}

		// Create new editor if needed
		if (!questionEditors[editorId]) {
			$(editorContainer).empty();
			questionEditors[editorId] = initQuillEditor(editorContainer, "Nhập nội dung câu hỏi...");
		}

		return questionEditors[editorId];
	}

	function initAllEditors() {
		setTimeout(() => {
			$(".section-item").each(function () {
				initSectionEditor($(this).data("section-id"));
			});
		}, DELAYS.EDITOR);
	}

	// ===== UI UPDATE FUNCTIONS =====
	function updateQuestionRequiredUI(questionId, isRequired) {
		const questionElement = $(`.question-item[data-question-id="${questionId}"]`);
		const badgesContainer = questionElement.find(".badges");

		// Cập nhật trạng thái badge
		if (isRequired) {
			if (badgesContainer.find(".badge.bg-danger").length === 0) {
				badgesContainer.prepend('<span class="badge bg-danger question-badge" title="Bắt buộc"><i class="ri-asterisk me-1"></i> Bắt buộc</span>');
			}
		} else {
			badgesContainer.find(".badge.bg-danger").remove();
		}

		// Cập nhật trạng thái checkbox
		questionElement.find(`#required-${questionId}`).prop("checked", isRequired);
	}

	function updateQuestionNumbers(sectionId) {
		const section = findSectionById(sectionId);
		if (!section) return;

		// Update data first
		section.questions.forEach((q, index) => {
			q.displayOrder = index + 1;
		});

		// Then update UI
		$(`.section-item[data-section-id="${sectionId}"] .question-item`).each(function (index) {
			$(this)
				.find(".question-number")
				.text(index + 1);
		});
	}

	// ===== MODAL MANAGEMENT =====
	function setupQuestionModal(question) {
		$("#edit-question-type").val(question.type);
		$("#edit-question-required").prop("checked", question.isRequired);

		// Set save button handler
		$("#saveQuestionEditBtn")
			.off("click")
			.on("click", () => saveQuestionEdit(question.id));

		// Show modal
		const editModal = new bootstrap.Modal(document.getElementById("editQuestionModal"));
		editModal.show();

		// Initialize editor with content
		setTimeout(() => {
			questionEditors["edit-question-editor"] = initQuillEditor("#edit-question-editor", "Nhập nội dung câu hỏi...", question.questionTitle);
		}, DELAYS.UI);
	}

	function showEditQuestionModal(questionId) {
		const question = findQuestionById(questionId);
		if (!question) return;

		// Load modal if needed
		if ($("#editQuestionModal").length === 0) {
			$.get("/Survey/EditQuestionModal", (html) => {
				$("body").append(html);
				setupQuestionModal(question);
			});
		} else {
			setupQuestionModal(question);
		}
	}

	// ===== DATA HANDLING =====
	function extractSurveyDataFromDom() {
		surveyData.sections = [];

		$(".section-item").each(function () {
			const sectionId = $(this).data("section-id");
			const sectionTitle = $(this).find(".section-title > span:nth-child(2)").text().trim();
			//const displayOrder = parseInt($(this).find(".me-2").text().replace("Phần ", "").replace(":", "")) || 1;
			const displayOrder = parseInt($(this).find(".section-number").text()) || 1;

			const sectionData = {
				id: sectionId,
				titleSection: sectionTitle,
				displayOrder: displayOrder,
				questions: [],
			};

			// Get questions
			$(this)
				.find(".question-item")
				.each(function () {
					const questionId = $(this).data("question-id");
					const questionTitle = $(this).find(".question-title").html();
					const typeText = $(this).find(".badge.bg-info").text().trim();
					const isRequired = $(this).find(".badge.bg-danger").length > 0;

					// Determine question type
					let type = "paragraph"; // default
					if (typeText.includes("Nhiều lựa chọn")) type = "multiChoice";
					else if (typeText.includes("Một lựa chọn")) type = "singleChoice";
					else if (typeText.includes("Thang đo Likert")) type = "likert";
					else if (typeText.includes("Dropdown")) type = "dropDown";

					const displayOrder = parseInt($(this).find(".question-number").text()) || 1;

					const questionData = {
						id: questionId,
						sectionId: sectionId,
						questionTitle: questionTitle,
						type: type,
						isRequired: isRequired,
						displayOrder: displayOrder,
						answers: [],
					};

					// Get options or sub-questions
					if (type === "multiChoice" || type === "singleChoice" || type === "dropDown") {
						$(this)
							.find(".option-item")
							.each(function () {
								const optionId = $(this).data("option-id");
								const value = $(this).find(".option-value").val();
								const isInput = $(this).find(".form-check-input[type=checkbox]").is(":checked");

								questionData.answers.push({
									id: optionId,
									questionID: questionId,
									value: value,
									isInput: isInput,
									displayOrder: questionData.answers.length + 1,
								});
							});
					} else if (type === "likert") {
						$(this)
							.find(".likert-question-item")
							.each(function () {
								const subQuestionId = $(this).data("sub-question-id");
								const subQuestionTitle = $(this).find("h6").text().trim();

								// Add 5 scale options for each sub-question
								for (let i = 1; i <= 5; i++) {
									questionData.answers.push({
										id: `existing_${subQuestionId}_${i}`,
										questionID: questionId,
										key: `${subQuestionId}_level_${i}`,
										value: `${i}`,
										isInput: false,
										displayOrder: i,
										subQuestionTitle: subQuestionTitle,
									});
								}
							});
					}

					sectionData.questions.push(questionData);
				});

			surveyData.sections.push(sectionData);
		});
	}

	// ===== AJAX HELPER =====
	function loadPartialView(url, data, targetContainer, emptyCallback) {
		return new Promise((resolve, reject) => {
			$.get(url, data, (html) => {
				if (emptyCallback) {
					targetContainer.find(".alert").remove();
				}
				targetContainer.append(html);
				resolve(html);
			}).fail(reject);
		});
	}

	// ===== PUBLIC METHODS =====
	function initModule(id) {
		surveyData.id = id || "";
		//console.log("Survey ID:", surveyData.id);
		// Set default dates for new survey
		if (!id) {
			const today = moment().format("YYYY-MM-DD");
			const nextWeek = moment().add(7, "days").format("YYYY-MM-DD");
			$("#startDate").val(today);
			$("#endDate").val(nextWeek);
		}

		// Handle collapse events for editors
		$(document).on("shown.bs.collapse hidden.bs.collapse", ".collapse", function () {
			setTimeout(initAllEditors, DELAYS.UI);
		});

		// Initialize data
		if (typeof initialSurveyData !== "undefined") {
			surveyData = initialSurveyData;

			// Populate form with initial data
			$("#title").val(surveyData.title || "");
			$("#description").val(surveyData.description || "");
			$("#startDate").val(surveyData.startedDate ? moment(surveyData.startedDate).format("YYYY-MM-DD") : "");
			$("#endDate").val(surveyData.endDate ? moment(surveyData.endDate).format("YYYY-MM-DD") : "");
			$("#displayOrder").val(surveyData.displayOrder || 1);

			const statusSelect = $("#status");
			if (statusSelect.length) {
				if (statusSelect.find("option").length === 0) {
					statusSelect.append('<option value="active">Hoạt động</option>');
					statusSelect.append('<option value="inactive">Không hoạt động</option>');
					statusSelect.append('<option value="draft">Bản nháp</option>');
				}
				statusSelect.val(surveyData.status || "active");
			}
		} else {
			extractSurveyDataFromDom();
		}

		// Initialize editors and bind events
		initAllEditors();
		$("#add-section-btn").on("click", addSectionDirectly);
		console.log("SectionData", surveyData.sections);
	}

	function addSectionDirectly() {
		const title = $("#new-section-title").val().trim();

		if (!title) {
			AlertResponse("Tiêu đề phần không được để trống!", "warning");
			return;
		}

		// Tự động tăng display order dựa trên sections hiện có
		const displayOrder = surveyData.sections.length + 1;

		// Create new section
		const newSection = {
			id: "temp_" + Date.now(),
			surveyID: surveyData.id,
			titleSection: title,
			displayOrder: displayOrder,
			questions: [],
		};

		surveyData.sections.push(newSection);

		// Load section partial
		loadPartialView(
			"/Survey/SectionPartial",
			{
				id: newSection.id,
				surveyId: surveyData.id,
				title: title,
				displayOrder: displayOrder,
			},
			$("#sections-container"),
			surveyData.sections.length === 1
		).then(() => {
			setTimeout(() => initSectionEditor(newSection.id), DELAYS.EDITOR);

			// Reset form
			$("#new-section-title").val("");
			// Không cần reset displayOrder vì sẽ được tính tự động
		});
	}

	function removeSection(sectionId) {
		swal({
			title: "Bạn có chắc chắn?",
			text: "Bạn có chắc chắn muốn xóa phần này?",
			icon: "warning",
			buttons: ["Hủy", "Xóa"],
			dangerMode: true,
		}).then((willDelete) => {
			if (willDelete) {
				// Remove from data
				surveyData.sections = surveyData.sections.filter((s) => s.id !== sectionId);

				// Remove from UI
				$(`.section-item[data-section-id="${sectionId}"]`).remove();

				// Show empty message if needed
				if (surveyData.sections.length === 0) {
					$("#sections-container").html('<div class="alert alert-info"><i class="ri-information-line me-1"></i> Vui lòng thêm phần cho khảo sát.</div>');
				}

				// Remove editor if exists
				delete questionEditors[`editor-section-${sectionId}`];

				swal("Đã xóa!", "Phần này đã được xóa khỏi khảo sát.", "success");
			}
		});
	}

	function addQuestionToSection(sectionId) {
		const sectionElement = $(`.section-item[data-section-id="${sectionId}"]`);
		const editorId = `editor-section-${sectionId}`;
		const quillEditor = questionEditors[editorId];

		if (!quillEditor) {
			AlertResponse("Lỗi khởi tạo editor, vui lòng thử lại!", "warning");
			return;
		}

		const content = quillEditor.root.innerHTML;
		if (!content || content === "<p><br></p>" || content === "<p></p>") {
			AlertResponse("Nội dung câu hỏi không được để trống!", "warning");
			return;
		}

		const questionType = sectionElement.find(".new-question-type").val();
		const section = findSectionById(sectionId);
		if (!section) return;

		const displayOrder = section.questions.length + 1;
		const questionId = "temp_question_" + Date.now();

		// Create new question
		const newQuestion = {
			id: questionId,
			sectionId: sectionId,
			questionTitle: content,
			type: questionType,
			isRequired: false,
			displayOrder: displayOrder,
			answers: [],
		};

		// Add to data
		section.questions.push(newQuestion);

		// Reset editor
		quillEditor.setText("");

		// Load question partial
		loadPartialView(
			"/Survey/QuestionPartial",
			{
				id: questionId,
				sectionId: sectionId,
				type: questionType,
				title: content,
				displayOrder: displayOrder,
			},
			sectionElement.find(".questions-container"),
			true
		).then(() => {
			// Cập nhật số thứ tự sau khi thêm câu hỏi mới
			updateQuestionNumbers(sectionId);
		});
	}

	function removeQuestion(questionId) {
		swal({
			title: "Bạn có chắc chắn?",
			text: "Bạn có chắc chắn muốn xóa câu hỏi này?",
			icon: "warning",
			buttons: ["Hủy", "Xóa"],
			dangerMode: true,
		}).then((willDelete) => {
			if (willDelete) {
				// Find section containing this question
				for (let i = 0; i < surveyData.sections.length; i++) {
					const questionIndex = surveyData.sections[i].questions.findIndex((q) => q.id === questionId);
					if (questionIndex !== -1) {
						// Remove from data
						surveyData.sections[i].questions.splice(questionIndex, 1);

						// Remove from UI
						const questionElement = $(`.question-item[data-question-id="${questionId}"]`);
						const questionsContainer = questionElement.closest(".questions-container");
						questionElement.remove();

						// Show empty message if needed
						if (surveyData.sections[i].questions.length === 0) {
							questionsContainer.html('<div class="alert alert-info"><i class="ri-information-line me-1"></i> Chưa có câu hỏi nào trong phần này.</div>');
						}

						// Update question numbers
						updateQuestionNumbers(surveyData.sections[i].id);

						swal("Đã xóa!", "Câu hỏi đã được xóa khỏi khảo sát.", "success");
						break;
					}
				}
			}
		});
	}

	function editQuestion(questionId) {
		showEditQuestionModal(questionId);
	}

	function saveQuestionEdit(questionId) {
		const question = findQuestionById(questionId);
		if (!question) return;

		const quill = questionEditors["edit-question-editor"];
		if (!quill) return;

		const newContent = quill.root.innerHTML;
		const newType = $("#edit-question-type").val();
		const newRequired = $("#edit-question-required").is(":checked");

		// Capture old type for comparison
		const oldType = question.type;

		// Update data
		question.questionTitle = newContent;
		question.isRequired = newRequired;

		// Handle type change
		if (oldType !== newType) {
			question.type = newType;

			// Reload question with new type
			loadPartialView(
				"/Survey/QuestionPartial",
				{
					id: question.id,
					sectionId: question.sectionId,
					type: newType,
					title: newContent,
					isRequired: newRequired,
					displayOrder: question.displayOrder,
				},
				$(),
				false
			).then((html) => {
				$(`.question-item[data-question-id="${questionId}"]`).replaceWith(html);
			});
		} else {
			// Just update UI
			updateQuestionRequiredUI(questionId, newRequired);
			$(`.question-item[data-question-id="${questionId}"] .question-title`).html(newContent);
		}

		// Close modal
		const modal = bootstrap.Modal.getInstance(document.getElementById("editQuestionModal"));
		if (modal) modal.hide();

		AlertResponse("Đã cập nhật câu hỏi thành công!", "success");
	}

	function toggleRequired(questionId, isRequired) {
		const question = findQuestionById(questionId);
		if (question) {
			question.isRequired = isRequired;

			// Cập nhật UI
			updateQuestionRequiredUI(questionId, isRequired);

			// Hiệu ứng visual feedback khi thay đổi
			const questionElement = $(`.question-item[data-question-id="${questionId}"]`);
			questionElement.addClass("border-danger");
			setTimeout(() => {
				questionElement.removeClass("border-danger");
			}, 300);
		}
	}

	function addOption(questionId) {
		const question = findQuestionById(questionId);
		if (!question) return;

		const questionElement = $(`.question-item[data-question-id="${questionId}"]`);
		const optionValue = questionElement.find(".new-option-value").val().trim();

		if (!optionValue) {
			AlertResponse("Vui lòng nhập nội dung cho lựa chọn", "warning");
			return;
		}

		// Initialize answers array if needed
		if (!question.answers) {
			question.answers = [];
		}

		const optionId = `temp_option_${Date.now()}`;
		const displayOrder = question.answers.length + 1;

		// Create new option
		const newOption = {
			id: optionId,
			questionID: questionId,
			key: `option_${displayOrder}`,
			value: optionValue,
			isInput: false,
			displayOrder: displayOrder,
		};

		// Add to data
		question.answers.push(newOption);

		// Load option partial
		loadPartialView(
			"/Survey/OptionPartial",
			{
				questionId: questionId,
				id: optionId,
				value: optionValue,
				displayOrder: displayOrder,
			},
			questionElement.find(".options-list"),
			true
		).then(() => {
			questionElement.find(".new-option-value").val("");
		});
	}

	function updateOption(questionId, optionId, value) {
		const option = findOptionById(questionId, optionId);
		if (option) {
			option.value = value;
		}
	}

	function removeOption(questionId, optionId) {
		const question = findQuestionById(questionId);
		if (!question || !question.answers) return;

		// Remove from data
		question.answers = question.answers.filter((o) => o.id !== optionId);

		// Remove from UI
		$(`.option-item[data-option-id="${optionId}"]`).remove();

		// Show alert if no options left
		const optionsList = $(`.question-item[data-question-id="${questionId}"] .options-list`);
		if (question.answers.length === 0) {
			optionsList.html('<div class="alert alert-info">Chưa có lựa chọn nào</div>');
		}
	}

	function addSubQuestion(questionId) {
		const question = findQuestionById(questionId);
		if (!question) return;

		const questionElement = $(`.question-item[data-question-id="${questionId}"]`);
		const subQuestionTitle = questionElement.find(".new-sub-question").val().trim();

		if (!subQuestionTitle) {
			AlertResponse("Vui lòng nhập nội dung cho câu hỏi con", "warning");
			return;
		}

		// Create sub-question ID
		const subQuestionId = `temp_sub_${Date.now()}`;

		// Create HTML for the Likert scale
		const subQuestionHtml = `
            <div class="likert-question-item" data-sub-question-id="${subQuestionId}">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <h6 class="mb-0">${subQuestionTitle}</h6>
                    <button class="btn btn-sm btn-outline-danger" 
                        onclick="SurveyModule.removeSubQuestion('${questionId}', '${subQuestionId}')">
                        <i class="ri-delete-bin-line fs-6"></i>
                    </button>
                </div>
                <div class="likert-scale-container">
                    <div class="likert-scale d-flex justify-content-between align-items-center">
                        ${Array.from(
							{ length: 5 },
							(_, i) => `
                            <div class="likert-option text-center">
                                <div class="form-check">
                                    <input class="form-check-input" type="radio" disabled data-likertAnswer-id="" data-likertAnswer-key="${i + 1}">
                                    <label class="form-check-label">${i + 1}</label>
                                </div>
                            </div>
                        `
						).join("")}
                    </div>
                </div>
            </div>
        `;

		// Add to UI
		const subQuestionsContainer = questionElement.find(".sub-questions-container");
		subQuestionsContainer.find(".alert").remove();
		subQuestionsContainer.append(subQuestionHtml);
		questionElement.find(".new-sub-question").val("");

		// Initialize answers array if needed
		if (!question.answers) {
			question.answers = [];
		}

		// Add options to data (1-5 scale)
		for (let i = 1; i <= 5; i++) {
			const timestamp = Date.now();
			question.answers.push({
				id: `temp_answer_${timestamp}_${i}`,
				questionID: questionId,
				key: `${subQuestionId}_level_${i}`,
				value: `${i}`,
				isInput: false,
				displayOrder: i,
				subQuestionTitle: subQuestionTitle,
			});
		}
	}

	function removeSubQuestion(questionId, subQuestionId) {
		const question = findQuestionById(questionId);
		if (!question || !question.answers) return;

		// Remove from data
		question.answers = question.answers.filter((a) => !a.key || !a.key.startsWith(subQuestionId));

		// Remove from UI
		$(`.likert-question-item[data-sub-question-id="${subQuestionId}"]`).remove();

		// Show alert if no sub-questions left
		const subQuestions = $(`.question-item[data-question-id="${questionId}"] .likert-question-item`);
		if (subQuestions.length === 0) {
			const container = $(`.question-item[data-question-id="${questionId}"] .sub-questions-container`);
			container.html('<div class="alert alert-info">Vui lòng thêm câu hỏi con cho thang đo Likert</div>');
		}
	}

	function toggleAllowInput(questionId, optionId, isAllowed) {
		const option = findOptionById(questionId, optionId);
		if (option) {
			option.isInput = isAllowed;
		}
	}

	function handleSave() {
		// Trước khi gửi dữ liệu, kiểm tra các section không có tiêu đề
		extractSurveyDataFromDom();

		// Kiểm tra và cập nhật tiêu đề nếu trống
		surveyData.sections.forEach((section) => {
			if (!section.titleSection || section.titleSection.trim() === "") {
				// Nếu không có tiêu đề, đặt giá trị mặc định
				section.titleSection = "Phần " + section.displayOrder;
			}
		});

		const title = $("#title").val().trim();
		if (!title) {
			AlertResponse("Tiêu đề khảo sát không được để trống!", "warning");
			return;
		}

		const startDate = $("#startDate").val();
		const endDate = $("#endDate").val();

		if (startDate && endDate && moment(startDate).isAfter(moment(endDate))) {
			AlertResponse("Ngày bắt đầu không thể sau ngày kết thúc!", "warning");
			return;
		}

		if (surveyData.sections.length === 0) {
			AlertResponse("Khảo sát cần có ít nhất một phần!", "warning");
			return;
		}

		// Prepare request data
		const surveyRequest = {
			id: surveyData.id || null,
			title: title,
			description: $("#description").val(),
			status: $("#status").val(),
			displayOrder: parseInt($("#displayOrder").val()) || 1,
			isDisplay: $("#isDisplay").is(":checked"),
			startedDate: startDate ? moment(startDate).format() : null,
			endDate: endDate ? moment(endDate).format() : null,
			sections: surveyData.sections.map((section) => ({
				id: section.id.startsWith("temp_") ? null : section.id,
				surveyID: surveyData.id,
				titleSection: section.titleSection,
				displayOrder: section.displayOrder,
				questions: section.questions.map((question) => {
					// Create basic question object
					const questionObj = {
						id: question.id.startsWith("temp_") ? null : question.id,
						sectionId: section.id.startsWith("temp_") ? null : section.id,
						type: question.type,
						questionTitle: question.questionTitle,
						isRequired: question.isRequired,
						displayOrder: question.displayOrder,
						answers: (question.answers || []).map((answer) => ({
							id: answer.id.startsWith("temp_") ? null : answer.id,
							questionId: question.id.startsWith("temp_") ? null : question.id,
							key: answer.value,
							value: answer.value,
							isInput: answer.isInput,
							displayOrder: answer.displayOrder,
						})),
					};

					// For Likert questions, extract and add the likertQuestions property
					if (question.type === "likert" && question.answers) {
						// Extract unique sub-questions
						const subQuestionMap = new Map();
						question.answers.forEach((answer) => {
							if (answer.subQuestionTitle) {
								const subQuestionId = answer.key.split("_level_")[0];
								if (!subQuestionMap.has(subQuestionId)) {
									subQuestionMap.set(subQuestionId, {
										questionId: question.id.startsWith("temp_") ? null : question.id,
										questionLikertId: subQuestionId.startsWith("temp_") ? null : subQuestionId,
										questionLikertTitle: answer.subQuestionTitle,
										optionCount: 5,
									});
								}
							}
						});

						// Only one set of 5-point scale options, no matter how many sub-questions
						
						//for (let i = 1; i <= 5; i++) {
						//	simplifiedAnswers.push({
						//		id: null,
						//		questionId: question.id.startsWith("temp_") ? null : question.id,
						//		key: `${i}`,
						//		value: `${i}`,
						//		isInput: false,
						//		displayOrder: i,
						//	});
						//}
						const likertOption = {}
						const likertInputs = document.querySelectorAll('.likert-scale-container .form-check-input');
						likertInputs.forEach((input, index) => {
							const questionId = question.id.startsWith("temp_") ? null : question.id;
							const key = input.getAttribute('data-likertAnswer-key');
							//simplifiedAnswers.push({
							//	id: input.getAttribute('data-likertAnswer-id'),
							//	questionId: question.id.startsWith("temp_") ? null : question.id,
							//	key: input.getAttribute('data-likertAnswer-key'),
							//	value: input.getAttribute('data-likertAnswer-key'), // hoặc nếu value khác thì lấy riêng
							//	isInput: false,
							//	displayOrder: parseInt(input.getAttribute('data-likertAnswer-key'), 10),
							//});
							likertOption[`${questionId}:${key}`] = {
								id: input.getAttribute('data-likertAnswer-id'),
								questionId: questionId,
								key: input.getAttribute('data-likertAnswer-key'),
								value: input.getAttribute('data-likertAnswer-key'), // hoặc nếu value khác thì lấy riêng
								isInput: false,
								displayOrder: index,
							};
						});
						const simplifiedAnswers = Object.values(likertOption);

						questionObj.likertQuestions = Array.from(subQuestionMap.values());
						questionObj.answers = simplifiedAnswers;
					}

					return questionObj;
				}),
			})),
		};

		// Show loading
		$("#loadingOverlay").removeClass("d-none");
		console.log(surveyRequest);
		// Send request
		$.ajax({
			url: surveyData.id ? `/api/Surveys/${surveyData.id}` : "/api/Surveys",
			type: surveyData.id ? "PUT" : "POST",
			contentType: "application/json",
			data: JSON.stringify(surveyRequest),
			success: function (response) {
				if (response.code === 0) {
					AlertResponse(response.message || "Lưu khảo sát thành công!", "success");
					setTimeout(() => (window.location.href = "/Survey"), 1000);
				} else {
					AlertResponse(response.message || "Đã có lỗi xảy ra!", "error");
				}
			},
			error: function (err) {
				console.error(err);
				AlertResponse("Đã xảy ra lỗi khi gửi yêu cầu, vui lòng thử lại sau!", "error");
			},
			complete: function () {
				$("#loadingOverlay").addClass("d-none");
			},
		});
	}

	// Return public API
	return {
		init: initModule,
		addSectionDirectly: addSectionDirectly,
		addQuestionToSection: addQuestionToSection,
		removeSection: removeSection,
		removeQuestion: removeQuestion,
		updateQuestionNumbers: updateQuestionNumbers,
		editQuestion: editQuestion,
		saveQuestionEdit: saveQuestionEdit,
		toggleRequired: toggleRequired,
		addOption: addOption,
		updateOption: updateOption,
		removeOption: removeOption,
		addSubQuestion: addSubQuestion,
		removeSubQuestion: removeSubQuestion,
		toggleAllowInput: toggleAllowInput,
		handleSave: handleSave,
	};
})();
