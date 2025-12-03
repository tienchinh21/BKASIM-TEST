import { useState } from "react";

export const CollapsibleText = ({ text, lines = 2, style = {} }) => {
    const [expanded, setExpanded] = useState(true);
    const showToggle = (text?.length || 0) > 80; // ngưỡng tuỳ chỉnh

    return (
        <div onClick={(e) => e.stopPropagation()}>
            <div
                style={{
                    // chống tràn chuỗi kiểu "aaaaaaaaaaaa..."
                    overflowWrap: "anywhere",
                    wordBreak: "break-word",
                    whiteSpace: "normal",

                    // kẹp dòng khi chưa mở rộng
                    display: expanded ? "block" : "-webkit-box",
                    WebkitBoxOrient: "vertical",
                    WebkitLineClamp: expanded ? undefined : lines,
                    overflow: "hidden",

                    ...style,
                }}
                title={text} // desktop có tooltip full
            >
                {text}
            </div>

            {showToggle && (
                <button
                    onClick={() => setExpanded((v) => !v)}
                    style={{
                        marginTop: 4,
                        border: "none",
                        background: "transparent",
                        padding: 0,
                        color: "var(--primary-color)",
                        fontSize: 12,
                        textDecoration: "underline",
                        cursor: "pointer",
                        display: "none",
                    }}
                >
                    {expanded ? "Thu gọn" : "Xem thêm"}
                </button>
            )}
        </div>
    );
};