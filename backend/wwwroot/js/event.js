class EventFormHandler {
    constructor() {
        //this.initSelect2();
        this.initGiftSelect();
        this.initSponsorSelect();
        //this.initialEditor();
        this.initSponsorSelectOnChange();
    }

    // ======== Nội dung ========
    //initialEditor(id) {
    //    window.contentEditor = new Quill(id, {
    //        theme: 'snow',
    //        modules: {
    //            imageResize: {
    //                displaySize: true,
    //            },
    //            toolbar: [
    //                [{ 'header': [1, 2, 3, false] }],
    //                ['bold', 'italic', 'underline', 'strike'],
    //                [{ 'list': 'ordered' }, { 'list': 'bullet' }],  // Thêm chỉnh list
    //                [{ 'color': [] }, { 'background': [] }],
    //                [{ 'align': [] }], // Căn lề (trái, phải, giữa, đều)
    //                ['blockquote', 'code-block'], // Thêm blockquote và code block
    //                ['image'],
    //                ['link',], // Thêm liên kết
    //                ['clean'] // Xóa định dạng
    //            ]
    //        }
    //    });
    //}

    // ======== Địa chỉ ========
    //initSelect2(cityKeyword, districtKeyword, wardKeyword) {
    //    const citySelect = $("#city");
    //    const districtSelect = $("#district");
    //    const wardSelect = $("#ward");

    //    this.loadCities(cityKeyword);

    //    citySelect.on('change', () => {
    //        const cityId = citySelect.find(":selected").data("id");
    //        this.loadDistricts(cityId, districtKeyword);
    //        wardSelect.empty().append('<option value="">Chọn Xã/Phường</option>');
    //    });

    //    districtSelect.on('change', () => {
    //        const districtId = districtSelect.find(":selected").data("id");
    //        this.loadWards(districtId, wardKeyword);
    //    });
    //}

    //loadCities(keyword, id = "#city") {
    //    const citySelect = $(id);
    //    $.getJSON("/api/ZaloHelperApi/GetCities?keyword=", res => {
    //        citySelect.empty().append('<option value="">Chọn Tỉnh/Thành phố</option>');
    //        res.data.forEach(province => {
    //            citySelect.append(`<option value="${province.name}" data-id="${province.id}" ${province.name === keyword ? 'selected' : ''}>${province.name}</option>`);
    //        });
    //        if (keyword) citySelect.val(keyword).trigger("change");
    //        citySelect.select2();
    //    });
    //}

    //loadDistricts(cityId, keyword, id = "#district") {
    //    const districtSelect = $(id).empty().append('<option value="">Chọn Quận/Huyện</option>');
    //    if (cityId) {
    //        $.getJSON(`/api/ZaloHelperApi/GetDistricts?cityId=${cityId}&keyword=`, res => {
    //            res.data.forEach(district => {
    //                districtSelect.append(`<option value="${district.name}" data-id="${district.id}" ${district.name === keyword ? 'selected' : ''}>${district.name}</option>`);
    //            });
    //            districtSelect.select2().trigger("change");
    //        });
    //    }
    //}

    //loadWards(districtId, keyword, id = "#ward") {
    //    const wardSelect = $(id).empty().append('<option value="">Chọn Xã/Phường</option>');
    //    if (districtId) {
    //        $.getJSON(`/api/ZaloHelperApi/GetWards?districtId=${districtId}&keyword=`, res => {
    //            res.data.forEach(ward => {
    //                wardSelect.append(`<option value="${ward.name}" data-id="${ward.id}" ${ward.name === keyword ? 'selected' : ''}>${ward.name}</option>`);
    //            });
    //            wardSelect.select2();
    //        });
    //    }
    //}

    // ======== Quà tặng ========
    initGiftSelect(gifts = []) {
        const $select = $('#gifts').empty();

        gifts.forEach(g => {
            const opt = new Option(g.name, g.productId, true, true);
            $(opt).attr({ 'data-image': g.images?.[0], 'data-price': g.price, 'data-quantity': g.quantity });
            $select.append(opt);
        });

        $select.select2({
            ajax: {
                url: '/api/products',
                dataType: 'json', delay: 300,
                data: params => ({ keyword: params.term, page: params.page || 1, pageSize: 20 }),
                processResults: res => ({
                    results: res.data.map(p => ({ id: p.id, text: p.name, image: p.images?.[0] || '/images/products/default.png', price: p.originalPrice || 0 })),
                    pagination: { more: res.totalPages > 1 }
                })
            },
            templateResult: this.formatProduct,
            templateSelection: (product) => {
                const opt = $(`#gifts option[value="${product.id}"]`);
                if (opt.length) {
                    if (!opt.data("quantity")) {
                        opt.attr("data-quantity", 1); // Chỉ set nếu chưa có
                    }
                    if (!opt.data("image")) {
                        opt.attr("data-image", product.image);
                    }
                    if (!opt.data("price")) {
                        opt.attr("data-price", product.price);
                    }
                }
                return this.formatProduct(product);
            },
            allowClear: true, width: '100%'
        }).on('change', this.updateGiftQuantities);

        $select.trigger('change');
    }

    updateGiftQuantities() {
        const container = $('#gift-quantity-container').empty();
        ($('#gifts').val() || []).forEach(id => {
            const opt = $(`#gifts option[value="${id}"]`);
            const name = opt.text();
            const image = opt.data('image') || '/images/products/default.png';
            const price = opt.data('price') || 0;
            const qty = opt.data('quantity') || 1;

            const html = `
                <div class="col-md-12">
                    <div class="card shadow-sm p-2 d-flex align-items-center flex-row">
                        <img src="${image}" class="rounded" style="width: 50px; height: 50px; object-fit: cover;">
                        <div class="ms-3 flex-grow-1">
                            <h6 class="mb-0">${name} - ${new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price)}</h6>
                        </div>
                        <input type="number" class="form-control gift-quantity" data-id="${id}" value="${qty}" min="1" style="width: 60px; text-align: center;">
                    </div>
                </div>`;

            container.append(html);
        });
    }

    formatProduct(product) {
        if (!product.id) return product.text;
        const img = $(product.element).data('image') || product.image || '/images/products/default.png';
        const price = $(product.element).data('price') || product.price || 0;
        return $(`<div style="display: flex; align-items: center;"><img src="${img}" class="rounded-circle" style="width: 40px; height: 40px; margin-right: 10px;" /><span>${product.text} - ${new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price)}</span></div>`);
    }

    // ======== Nhà tài trợ ========
    initSponsorSelectOnChange() {
        $('#sponsors').on('change', () => {
            const ids = $('#sponsors').val() || [];
            const selected = ids.map(id => ({ sponsorId: id }));
            this.updateSponsorTiers(selected);
        });
    }

    initSponsorSelect(sponsorSelections = []) {
        const $select = $('#sponsors').empty();

        sponsorSelections.forEach(sel => {
            const opt = new Option(sel.sponsorName, sel.sponsorId, true, true);
            $(opt).attr('data-image', sel.image);
            $select.append(opt);
        });

        $select.select2({
            ajax: {
                url: '/api/sponsors', dataType: 'json', delay: 300,
                data: params => ({ keyword: params.term, page: params.page || 1, pageSize: 20 }),
                processResults: res => ({
                    results: res.data.map(s => ({ id: s.id, text: s.sponsorName, image: s.image })),
                    pagination: { more: res.totalPages > 1 }
                })
            },
            templateResult: this.formatSponsor,
            templateSelection: this.formatSponsor,
            allowClear: true, width: '100%'
        });

        $select.val(sponsorSelections.map(s => s.sponsorId)).trigger('change');
        this.updateSponsorTiers(sponsorSelections);
    }

    formatSponsor(sponsor) {
        if (!sponsor.id) return sponsor.text;
        const img = $(sponsor.element).data('image') || sponsor.image || '/images/sponsors/default.png';
        return $(`<div style="display: flex; align-items: center;"><img src="${img}" class="rounded-circle" style="width: 40px; height: 40px; margin-right: 10px;" /><span>${sponsor.text}</span></div>`);
    }

    async updateSponsorTiers(sponsorSelections = []) {
        const sponsorIds = $('#sponsors').val() || [];
        const container = $('#sponsor-tier-container').empty();

        const tiers = await $.get('/api/sponsorshiptiers', { page: 1, pageSize: 100 }).then(res => res.data || []);

        sponsorIds.forEach(id => {
            const name = $(`#sponsors option[value="${id}"]`).text() || id;
            const selectedTierId = sponsorSelections.find(s => s.sponsorId === id)?.tierId;

            const tierOptions = tiers.map(t => `<option value="${t.id}" ${selectedTierId === t.id ? 'selected' : ''}>${t.tierName}</option>`).join('');

            container.append(`
                <div class="col-md-6 mb-3">
                    <div class="card shadow-sm p-3">
                        <h6 class="mb-2">Nhà tài trợ: <strong>${name}</strong></h6>
                        <label>Chọn hạng:</label>
                        <select class="form-control sponsor-tier-select" data-sponsor-id="${id}">
                            <option value="">-- Chọn hạng tài trợ --</option>
                            ${tierOptions}
                        </select>
                    </div>
                </div>
            `);
        });
    }

    // ======== Xử lý ảnh ========
    async validateImageAspectRatio(file, requiredWidth, requiredHeight) {
        return new Promise(resolve => {
            const reader = new FileReader();
            reader.readAsDataURL(file);
            reader.onload = e => {
                const img = new Image();
                img.src = e.target.result;
                img.onload = () => {
                    const actualRatio = img.width / img.height;
                    const expectedRatio = requiredWidth / requiredHeight;
                    resolve({ valid: Math.abs(actualRatio - expectedRatio) <= 0.01, actualWidth: img.width, actualHeight: img.height });
                };
            };
        });
    }

    async showBannerPreview(inputSelector, previewSelector) {
        const file = $(inputSelector).prop('files')[0];
        if (!file || !file.type.startsWith('image/')) return;

        const validation = await this.validateImageAspectRatio(file, 16, 9);
        if (!validation.valid) {
            AlertResponse(`Ảnh không đúng tỉ lệ 16:9 (${validation.actualWidth}x${validation.actualHeight})`, "warning");
            $(inputSelector).val("");
            return;
        }

        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = e => {
            const html = `<div class="image-preview position-relative">
                <span class="btn-preview-remove" data-url="">x</span>
                <img style="object-fit:contain; height:150px;" src="${e.target.result}" class="card-img-top" alt="Banner" />
            </div>`;
            $(previewSelector).html(html);
        };
    }

    async showImagesPreview(inputElement, carouselSelector, previewContainerSelector) {
        const files = Array.from(inputElement.files);
        const dataTransfer = new DataTransfer();

        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            if (!file || !file.type.startsWith("image/")) continue;

            const validation = await this.validateImageAspectRatio(file, 16, 9);
            if (!validation.valid) {
                AlertResponse(`Ảnh không đúng tỉ lệ 16:9 (${validation.actualWidth}x${validation.actualHeight})`, "warning");
                continue;
            }

            dataTransfer.items.add(file);
            const reader = new FileReader();
            const uniqueId = `preview-image-${Date.now()}-${i}`;

            reader.onload = e => {
                $(carouselSelector).append(`<div class="carousel-item ${$(carouselSelector).children().length === 0 ? 'active' : ''}" data-id="${uniqueId}">
                    <img src="${e.target.result}" class="d-block w-100" alt="Image" style="height:170px; object-fit:cover;" /></div>`);

                const html = `<div class="image-preview mx-2 card position-relative" style="max-width: 250px; height: 170px;" data-id="${uniqueId}">
                    <img src="${e.target.result}" class="card-img-top h-100" style="object-fit:contain" />
                    <span class="btn-preview-remove">x</span></div>`;
                $(previewContainerSelector).append(html);
            };

            reader.readAsDataURL(file);
        }

        inputElement.files = dataTransfer.files;
    }
}

window.EventFormHandler = EventFormHandler;
