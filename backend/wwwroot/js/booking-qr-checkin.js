/**
 * QR Check-In Scanner Module
 * Handles QR code scanning for booking check-in functionality
 */

class BookingQrCheckInScanner {
    constructor() {
        this.qrScanner = null;
        this.recentlyScanned = new Set();
        this.checkInCount = 0;
        this.currentCameraIndex = 0;
        this.availableCameras = [];
        this.isScanning = false;
        this.scanTimeout = null;
        this.lastScanTime = null;

        this.initializeModal();
    }

    initializeModal() {
        // Modal event listeners
        $('#modal-checkin-qr').on('shown.bs.modal', () => {
            this.startScanning();
        });

        $('#modal-checkin-qr').on('hidden.bs.modal', () => {
            this.stopScanning();
            this.resetResults();
        });

        // Toggle camera button
        $('#toggle-camera-btn').on('click', () => {
            this.switchCamera();
        });
    }

    async startScanning() {
        try {
            this.showCameraLoading(true);
            this.hideCameraError();

            // Initialize scanner
            if (this.qrScanner) {
                await this.qrScanner.stop();
                this.qrScanner.clear();
            }

            this.qrScanner = new Html5Qrcode("qr-reader");

            // Get available cameras
            this.availableCameras = await Html5Qrcode.getCameras();

            if (!this.availableCameras || this.availableCameras.length === 0) {
                throw new Error("Không tìm thấy camera trên thiết bị");
            }

            // Find preferred camera (back camera first)
            const preferredCamera = this.getPreferredCamera();

            // Start scanning
            await this.qrScanner.start(
                preferredCamera.id,
                {
                    fps: 30,
                    qrbox: { width: 280, height: 280 },
                    aspectRatio: 1.0,
                    disableFlip: false,
                    rememberLastUsedCamera: true
                },
                (decodedText, result) => this.onQrScanned(decodedText, result),
                (errorMessage) => {
                    // Suppress console warnings for continuous scanning
                }
            );

            this.isScanning = true;
            this.showCameraLoading(false);
            this.updateToggleCameraButton();

        } catch (error) {
            console.error("Camera initialization error:", error);
            this.showCameraError(error.message);
            this.showCameraLoading(false);
        }
    }

    async stopScanning() {
        if (this.qrScanner && this.isScanning) {
            try {
                await this.qrScanner.stop();
                this.qrScanner.clear();
                this.isScanning = false;
            } catch (error) {
                console.warn("Error stopping scanner:", error);
            }
        }

        if (this.scanTimeout) {
            clearTimeout(this.scanTimeout);
            this.scanTimeout = null;
        }

        this.qrScanner = null;
    }

    async switchCamera() {
        if (!this.availableCameras || this.availableCameras.length <= 1) {
            this.showCurrentStatus('Chỉ có một camera khả dụng', 'info');
            return;
        }

        this.currentCameraIndex = (this.currentCameraIndex + 1) % this.availableCameras.length;

        try {
            this.showCameraLoading(true);
            await this.stopScanning();
            await this.startScanning();
        } catch (error) {
            console.error("Error switching camera:", error);
            this.showCurrentStatus('Không thể chuyển camera', 'error');
        }
    }

    getPreferredCamera() {
        if (this.currentCameraIndex < this.availableCameras.length) {
            return this.availableCameras[this.currentCameraIndex];
        }

        // Find back camera
        const backCamera = this.availableCameras.find(camera =>
            camera.label.toLowerCase().includes('back') ||
            camera.label.toLowerCase().includes('rear')
        );

        return backCamera || this.availableCameras[0];
    }

    async onQrScanned(decodedText, result) {
        const bookingId = decodedText?.trim();

        if (!bookingId) {
            this.showCurrentStatus('QR code không chứa thông tin hợp lệ', 'error');
            return;
        }

        // Prevent duplicate scans
        if (this.recentlyScanned.has(bookingId)) {
            return;
        }

        this.recentlyScanned.add(bookingId);
        this.playBeepSound();

        // Clear previous timeout
        if (this.scanTimeout) {
            clearTimeout(this.scanTimeout);
        }

        // Process check-in with debounce
        this.scanTimeout = setTimeout(async () => {
            try {
                await this.performCheckIn(bookingId);
            } catch (error) {
                console.error("Check-in error:", error);
                this.showCurrentStatus('Lỗi khi thực hiện check-in', 'error');
            } finally {
                // Remove from recently scanned after delay
                setTimeout(() => {
                    this.recentlyScanned.delete(bookingId);
                }, 3000);
            }
        }, 500);
    }

    async performCheckIn(bookingId) {
        this.showCurrentStatus('Đang xử lý check-in...', 'processing');

        try {
            const response = await $.ajax({
                url: `/api/bookings/checkin/${bookingId}`,
                method: 'POST',
                contentType: 'application/json',
                timeout: 10000
            });

            console.log(response);

            if (response && response.code === 0) {
                this.handleCheckInSuccess(response.message, bookingId);
            } else {
                this.handleCheckInError(response?.message || 'Check-in thất bại', bookingId);
            }

        } catch (error) {
            console.error("API Error:", error);

            let errorMessage = 'Lỗi kết nối. Vui lòng thử lại.';

            if (error.status === 404) {
                errorMessage = 'Không tìm thấy thông tin đặt lịch';
            } else if (error.status === 400) {
                errorMessage = 'Thông tin check-in không hợp lệ';
            } else if (error.responseJSON?.message) {
                errorMessage = error.responseJSON.message;
            }

            this.handleCheckInError(errorMessage, bookingId);
        }
    }

    handleCheckInSuccess(message, bookingId) {
        this.checkInCount++;
        this.lastScanTime = new Date();

        // Update counter
        $('#checkin-counter').text(this.checkInCount);

        // Show success status
        this.showCurrentStatus(message, 'success');

        // Add to history
        this.addToHistory(message, 'success', bookingId);

        // Update last scan time
        this.updateLastScanTime();

        // Reload main table if available
        if (typeof table !== 'undefined' && table.ajax) {
            table.ajax.reload(null, false);
        }
    }

    handleCheckInError(message, bookingId) {
        this.showCurrentStatus(message, 'error');

        // Add error to history
        this.addToHistory(message, 'error', bookingId);

        // Update last scan time
        this.lastScanTime = new Date();
        this.updateLastScanTime();
    }

    showCurrentStatus(message, type = 'info') {
        const statusElement = $('#current-status');

        const iconMap = {
            success: { icon: 'ri-check-circle-fill', color: 'text-white', bg: 'bg-success' },
            error: { icon: 'ri-close-circle-fill', color: 'text-white', bg: 'bg-danger' },
            processing: { icon: 'ri-loader-4-line', color: 'text-white', bg: 'bg-primary' },
            info: { icon: 'ri-qr-scan-2-line', color: 'text-black', bg: 'bg-light' }
        };

        const config = iconMap[type] || iconMap.info;
        const isProcessing = type === 'processing';

        statusElement.html(`
            <div class="p-3 rounded ${config.bg} bg-opacity-10 border border-${config.bg.replace('bg-', '')} border-opacity-25">
                <i class="${config.icon} ${config.color} ${isProcessing ? 'spinner-icon' : ''}" style="font-size: 2.5rem;"></i>
                <div class="mt-2 fw-medium ${config.color}">${message}</div>
            </div>
        `);

        // Add spinning animation for processing
        if (isProcessing) {
            statusElement.find('.spinner-icon').css('animation', 'spin 1s linear infinite');
        }
    }

    addToHistory(message, type, bookingId = null) {
        const historyContainer = $('#messages-history');
        const currentTime = new Date().toLocaleTimeString('vi-VN', {
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });

        const iconMap = {
            success: { icon: 'ri-check-circle-fill', color: 'text-success' },
            error: { icon: 'ri-close-circle-fill', color: 'text-danger' },
            info: { icon: 'ri-information-fill', color: 'text-info' }
        };

        const config = iconMap[type] || iconMap.info;

        // Remove "no actions" message if exists
        historyContainer.find('.text-center.text-muted.py-3').parent().remove();

        const historyItem = `
            <div class="border-bottom pb-2 mb-2 history-item">
                <div class="d-flex align-items-start">
                    <i class="${config.icon} ${config.color} me-2 mt-1"></i>
                    <div class="flex-grow-1">
                        <div class="fw-medium">${message}</div>
                        ${bookingId ? `<small class="text-muted">ID: ${bookingId}</small><br>` : ''}
                        <small class="text-muted">${currentTime}</small>
                    </div>
                </div>
            </div>
        `;

        // Prepend new item (latest first)
        historyContainer.prepend(historyItem);

        // Limit history items (keep only last 10)
        const items = historyContainer.find('.history-item');
        if (items.length > 10) {
            items.slice(10).remove();
        }

        // Scroll to top to show latest item
        historyContainer.scrollTop(0);
    }

    updateLastScanTime() {
        if (this.lastScanTime) {
            const timeString = this.lastScanTime.toLocaleTimeString('vi-VN');
            $('#last-scan-time').text(timeString);
        }
    }

    updateToggleCameraButton() {
        const btn = $('#toggle-camera-btn');
        if (this.availableCameras && this.availableCameras.length > 1) {
            btn.removeClass('d-none');
            const currentCamera = this.availableCameras[this.currentCameraIndex];
            const cameraName = currentCamera?.label || `Camera ${this.currentCameraIndex + 1}`;
            btn.attr('title', `Hiện tại: ${cameraName}`);
        } else {
            btn.addClass('d-none');
        }
    }

    showCameraLoading(show) {
        $('#camera-loading').toggle(show);
    }

    showCameraError(message = 'Không thể truy cập camera') {
        $('#camera-error').removeClass('d-none');
        $('#camera-error .text-danger div').text(message);
    }

    hideCameraError() {
        $('#camera-error').addClass('d-none');
    }

    playBeepSound() {
        try {
            const audio = document.getElementById('qr-beep-sound');
            if (audio) {
                audio.currentTime = 0;
                audio.play().catch(e => console.warn('Cannot play beep sound:', e));
            }
        } catch (error) {
            console.warn('Audio play error:', error);
        }
    }

    resetResults() {
        this.checkInCount = 0;
        this.recentlyScanned.clear();
        this.lastScanTime = null;

        $('#checkin-counter').text('0');
        $('#last-scan-time').text('--');
        $('#current-status').html(`
            <i class="ri-qr-scan-2-line text-muted" style="font-size: 3rem; opacity: 0.3;"></i>
            <div class="mt-2 text-muted">Sẵn sàng quét QR code</div>
        `);

        // Reset history
        $('#messages-history').html(`
            <div class="text-center text-muted py-3">
                <small>Chưa có thao tác nào</small>
            </div>
        `);
    }
}

// Global function to clear results
window.clearCheckInResults = function () {
    if (window.qrCheckInScanner) {
        window.qrCheckInScanner.resetResults();
    }
};

// Initialize scanner when document is ready
$(document).ready(function () {
    window.qrCheckInScanner = new BookingQrCheckInScanner();
});