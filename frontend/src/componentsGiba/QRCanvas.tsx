import React, { useRef, useImperativeHandle, forwardRef } from "react";
import { QRCode } from "zmp-qrcode";

interface QRCodeInstance {
  canvas: HTMLCanvasElement;
  getBase64: (type?: "png" | "jpg") => string;
}

export interface QRCanvasProps {
  value: string;
  size?: number;
  className?: string;
  style?: React.CSSProperties;
  showBorder?: boolean;
  borderColor?: string;
}

export interface QRCanvasRef {
  /** Get QR code as base64 image */
  getBase64: (type?: "png" | "jpg") => string;
  /** Get canvas element */
  getCanvas: () => HTMLCanvasElement | null;
}

const QRCanvas = forwardRef<QRCanvasRef, QRCanvasProps>(
  (
    {
      value,
      size = 128,
      className = "",
      style = {},
      showBorder = false,
      borderColor = "#e5e7eb",
    },
    ref
  ) => {
    const qrRef = useRef<QRCodeInstance>(null);

    useImperativeHandle(ref, () => ({
      getBase64: (type: "png" | "jpg" = "png") => {
        if (qrRef.current) {
          return qrRef.current.getBase64(type);
        }
        return "";
      },
      getCanvas: () => {
        if (qrRef.current) {
          return qrRef.current.canvas;
        }
        return null;
      },
    }));

    if (!value) {
      return (
        <div
          className={`flex items-center justify-center bg-gray-100 rounded ${className}`}
          style={{ width: size, height: size, ...style }}
        >
          <p className="text-gray-500 text-xs">No QR data</p>
        </div>
      );
    }

    return (
      <div
        className={`inline-block ${className}`}
        style={{
          padding: showBorder ? "8px" : "0",
          border: showBorder ? `1px solid ${borderColor}` : "none",
          borderRadius: showBorder ? "8px" : "0",
          ...style,
        }}
      >
        <QRCode ref={qrRef} value={value} size={size} />
      </div>
    );
  }
);

QRCanvas.displayName = "QRCanvas";

export default QRCanvas;
