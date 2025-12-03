// ===== OrderCustomFields (selector + render) =====
const OrderCustomFields = (function () {
    const bankSel = "#order-custom-fields-bank";
    const selectedSel = "#order-custom-fields-selected";
    const badgesSel = "#cf-selected-badges";
    let selectedIds = new Set();
    const nameCache = new Map
    const removedIds = new Set();

    function seedNameCache() {
        // từ bank
        $(`${bankSel} > [data-cf-wrapper]`).each(function () {
            nameCache.set($(this).data("cf-id"), $(this).data("cf-name") || "");
        });
        // từ modal (phòng khi thiếu)
        $("#cf-picker-list .cf-pick").each(function () {
            const id = this.value;
            const name = $(this).closest("[data-cf-pick-row]").find("strong").text().trim();
            if (name) nameCache.set(id, name);
        });
    }

    function init() {
        seedNameCache();
        // lấy từ checkbox đã tick trong modal
        selectedIds = new Set($("#cf-picker-list .cf-pick:checked").map((_, x) => x.value).get());

        // Khởi tạo theo các field đang có value
        if (selectedIds.size === 0) {
            $(bankSel).find("[data-cf-wrapper]").each(function () {
                nameCache.set($(this).data("cf-id"), $(this).data("cf-name"));
                const id = $(this).data("cf-id");
                const cur = ($(this).data("cf-current") || "").toString().trim();
                const hasVal = !!cur
                    || !!$(this).find("[data-cf-input]").val()
                    || !!$(this).find("input[type='hidden'][data-cf-hidden]").val()
                    || $(this).find("input[type='checkbox']").prop("checked");
                if (hasVal) selectedIds.add(id);
            });
        }

        // Áp dụng lựa chọn ban đầu
        renderSelected();
        renderBadges();
        initSelect2("#order-custom-fields-selected");

        // Tìm kiếm trong modal
        $("#cf-picker-search").on("input", function () {
            const q = this.value.trim().toLowerCase();
            $("#cf-picker-list [data-cf-pick-row]").each(function () {
                const name = ($(this).data("name") || "").toString();
                $(this).toggle(name.indexOf(q) !== -1);
            });
            updatePickerCounter();
        });

        // Apply trong modal
        $("#cf-picker-apply").on("click", function () {
            const prev = new Set(selectedIds);
            selectedIds = new Set($("#cf-picker-list .cf-pick:checked").map((_, x) => x.value).get());
            // thêm những cái vừa bỏ chọn vào removed
            prev.forEach(id => { if (!selectedIds.has(id)) removedIds.add(id); });
            // nếu chọn lại thì huỷ xoá
            selectedIds.forEach(id => { if (removedIds.has(id)) removedIds.delete(id); });
            renderSelected();
            renderBadges();
        });

        // Upload file/image → lấy id tạm
        $(selectedSel).on("change", "input[type='file'][data-cf-type]", async function () {
            const $inp = $(this);
            const type = $inp.data("cf-type"); // file | image
            const $wrap = $inp.closest("[data-cf-wrapper]");
            if (!this.files || this.files.length === 0) return;

            const form = new FormData();
            for (const f of this.files) form.append("files", f);

            try {
                const json = await $.ajax({
                    url: "/api/ZaloHelperApi/UploadFiles",
                    type: "POST",
                    data: form,
                    contentType: false,   // quan trọng
                    processData: false,   // quan trọng
                    cache: false
                });

                if (json.code !== 0 || !json.data?.length) throw new Error(json.message || "Upload thất bại");

                const first = json.data[0];
                $wrap.find("input[type='hidden'][data-cf-hidden]").val(first.id);

                if (type === "image") {
                    $wrap.find("[data-cf-preview]").attr("src", first.url).removeClass("d-none");
                } else {
                    $wrap.find("[data-cf-file-name]").text(first.name || "Đã chọn tệp").removeClass("d-none");
                }
            } catch (e) {
                AlertResponse(e?.message || "Upload lỗi", "error");
            }
        });

        $("#cf-picker-modal").on("shown.bs.modal", updatePickerCounter);
        $("#cf-picker-list").on("change", ".cf-pick", updatePickerCounter);

        $("#cf-pick-select-all").on("click", function () {
            $("#cf-picker-list .cf-pick:visible").prop("checked", true);
            updatePickerCounter();
        });

        $("#cf-pick-clear-all").on("click", function () {
            $("#cf-picker-list .cf-pick:visible").prop("checked", false);
            updatePickerCounter();
        });
    }

    function renderSelected() {
        $(`${selectedSel} > [data-cf-wrapper]`).each(function () {
            const id = $(this).data("cf-id");
            if (!selectedIds.has(id)) $(this).detach().appendTo(bankSel);
        });

        selectedIds.forEach(id => {
            const $node = $(
                `${selectedSel} > [data-cf-wrapper][data-cf-id='${id}'],` +
                `${bankSel} > [data-cf-wrapper][data-cf-id='${id}']`
            ).first().detach();
            if ($node.length) $(selectedSel).append($node.removeClass("d-none"));
        });

        $("#cf-picker-list .cf-pick").each(function () { this.checked = selectedIds.has(this.value); });

        initSelect2(selectedSel);
    }

    function renderBadges() {
        const $badges = $(badgesSel).empty();

        selectedIds.forEach(id => {
            const name = nameCache.get(id) || id; // không phụ thuộc DOM selected
            const $badge = $(`
                  <span class="badge bg-secondary m-1" data-cf-badge="${id}">
                    ${name}<i class="ri-close-line ms-1" style="cursor:pointer"></i>
                  </span>
                `);
            $badge.find("i").on("click", function () {
                selectedIds.delete(id);
                removedIds.add(id);
                renderSelected();
                renderBadges();
            });
            $badges.append($badge);
        });
    }

    function initSelect2(scopeSel) {
        const $parent = $("#modal-order").length ? $("#modal-order") : $(scopeSel).closest(".modal");
        $(`${scopeSel} select.cf-select2-multi[multiple]`).each(function () {
            if ($(this).data("select2")) return;
            $(this).select2({ width: "100%", placeholder: "Chọn", allowClear: true });
        });
    }

    function destroySelect2($wrap) {
        $wrap.find("select.cf-select2-multi").each(function () {
            if ($(this).data("select2")) $(this).select2("destroy");
        });
    }

    function collect() {
        const result = [];
        $(selectedSel).find("[data-cf-wrapper]").each(function () {
            const $box = $(this);
            const id = $box.data("cf-id");
            const type = ($box.data("cf-dtype") || "").toLowerCase();
            let val = "";

            if (type === "bool") {
                val = $box.find("input[type='checkbox']").is(":checked") ? "true" : "false";
            } else if (type === "multiselect") {
                val = ($box.find("select").val() || []).join(",");
            } else if (type === "file" || type === "image") {
                val = $box.find("input[type='hidden'][data-cf-hidden]").val() || "";
            } else {
                val = $box.find("[data-cf-input]").val() || "";
            }
            
            result.push({ customFieldId: id, value: val });
        });
        return result;
    }

    function validate() {
        const errors = [];
        $(selectedSel).find("[data-cf-wrapper]").each(function () {
            const $box = $(this);
            const name = $box.data("cf-name");
            const type = ($box.data("cf-dtype") || "").toLowerCase();
            const required = $box.data("cf-req") === true || $box.data("cf-req") === "true";

            const val = (() => {
                if (type === "bool") return $box.find("input[type='checkbox']").is(":checked") ? "true" : "";
                if (type === "multiselect") return ($box.find("select").val() || []).join(",");
                if (type === "file" || type === "image") return $box.find("input[type='hidden'][data-cf-hidden]").val() || "";
                return ($box.find("[data-cf-input]").val() || "").trim();
            })();

            if (required && !val) errors.push(`'${name}' là bắt buộc.`);

            if (val) {
                if (type === "int" && !/^-?\d+$/.test(val)) errors.push(`'${name}' phải là số nguyên.`);
                if (type === "decimal" && isNaN(Number(val))) errors.push(`'${name}' phải là số.`);
                if (type === "email" && !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(val)) errors.push(`'${name}' email không hợp lệ.`);
                if (type === "phone" && !/^[\+]?[0-9\-\(\)\s]+$/.test(val)) errors.push(`'${name}' số điện thoại không hợp lệ.`);
                if (type === "url") { try { new URL(val); } catch { errors.push(`'${name}' URL không hợp lệ.`); } }
            }
        });
        return { ok: errors.length === 0, errors };
    }

    function updatePickerCounter() {
        const vis = $("#cf-picker-list .cf-pick:visible");
        const checked = vis.filter(":checked").length;
        $("#cf-pick-counter").text(`${checked}/${vis.length}`);
    }

    function getRemoved() { return Array.from(removedIds); }
    function clearRemoved() { removedIds.clear(); }

    return { init, collect, validate, getRemoved, clearRemoved };
})();