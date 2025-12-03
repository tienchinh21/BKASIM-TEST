import React, { useState, useEffect, useRef } from "react";
import { useNavigate, Page } from "zmp-ui";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import { toast } from "react-toastify";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import useSetHeader from "../components/hooks/useSetHeader";
import dfData from "../common/DefaultConfig.json";

interface AppBriefData {
  content: string;
  isPdf: boolean;
  pdfUrl: string | null;
  pdfFileName: string | null;
  fullUrl: string | null;
}

const AppBriefGiba: React.FC = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);

  const [loading, setLoading] = useState(true);
  const [briefData, setBriefData] = useState<AppBriefData | null>(null);
  const [hasScrolledToBottom, setHasScrolledToBottom] = useState(false);
  const contentRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    setHeader({
      title: "GIỚI THIỆU ỨNG DỤNG",
      hasLeftIcon: true,
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
    });

    loadAppBrief();
  }, []);

  const loadAppBrief = async () => {
    try {
      setLoading(true);
      const response = await axios.get(`${dfData.domain}/Setting/GetAppBrief`, {
        headers: {
          Authorization: `Bearer ${userToken}`,
        },
      });

      if (response.data.success && response.data.data) {
        setBriefData(response.data.data);
      } else {
        toast.error("Không có nội dung để hiển thị");
      }
    } catch (error) {
      console.error("Error loading app brief:", error);
      toast.error("Không thể tải nội dung. Vui lòng thử lại!");
    } finally {
      setLoading(false);
    }
  };

  // Preserve text alignment after content is rendered
  useEffect(() => {
    if (!briefData?.isPdf && contentRef.current) {
      // Use setTimeout to ensure DOM is fully rendered
      const timeoutId = setTimeout(() => {
        const contentElement = contentRef.current;
        if (!contentElement) return;

        // Function to force apply text-align with !important
        const forceTextAlign = (element: HTMLElement, alignValue: string) => {
          element.style.setProperty("text-align", alignValue, "important");
        };

        // Handle Quill editor alignment classes (ql-align-center, ql-align-left, etc.)
        const quillCenterElements =
          contentElement.querySelectorAll(".ql-align-center");
        quillCenterElements.forEach((el) => {
          const htmlEl = el as HTMLElement;
          forceTextAlign(htmlEl, "center");
        });

        const quillLeftElements =
          contentElement.querySelectorAll(".ql-align-left");
        quillLeftElements.forEach((el) => {
          const htmlEl = el as HTMLElement;
          forceTextAlign(htmlEl, "left");
        });

        const quillRightElements =
          contentElement.querySelectorAll(".ql-align-right");
        quillRightElements.forEach((el) => {
          const htmlEl = el as HTMLElement;
          forceTextAlign(htmlEl, "right");
        });

        const quillJustifyElements =
          contentElement.querySelectorAll(".ql-align-justify");
        quillJustifyElements.forEach((el) => {
          const htmlEl = el as HTMLElement;
          forceTextAlign(htmlEl, "justify");
        });

        // Find all elements with inline styles containing text-align
        const elementsWithTextAlign = contentElement.querySelectorAll(
          '[style*="text-align"]'
        );
        elementsWithTextAlign.forEach((el) => {
          const htmlEl = el as HTMLElement;
          const style = htmlEl.getAttribute("style") || "";

          // Extract text-align value from style attribute
          const textAlignMatch = style.match(/text-align\s*:\s*([^;]+)/i);
          if (textAlignMatch) {
            const alignValue = textAlignMatch[1].trim();
            forceTextAlign(htmlEl, alignValue);
          }
        });

        // Handle center tags
        const centerTags = contentElement.querySelectorAll("center");
        centerTags.forEach((el) => {
          const htmlEl = el as HTMLElement;
          forceTextAlign(htmlEl, "center");
        });

        // Handle align attribute
        const elementsWithAlign = contentElement.querySelectorAll("[align]");
        elementsWithAlign.forEach((el) => {
          const htmlEl = el as HTMLElement;
          const alignValue = htmlEl.getAttribute("align");
          if (alignValue) {
            forceTextAlign(htmlEl, alignValue);
          }
        });

        // Preserve all inline styles (font-size, font-weight, color, etc.)
        const allElements = contentElement.querySelectorAll("*");
        allElements.forEach((el) => {
          const htmlEl = el as HTMLElement;
          const styleAttr = htmlEl.getAttribute("style") || "";

          if (styleAttr) {
            // Parse and apply all inline styles
            const stylePairs = styleAttr.split(";").filter((s) => s.trim());
            stylePairs.forEach((pair) => {
              const [property, value] = pair.split(":").map((s) => s.trim());
              if (property && value) {
                // Convert kebab-case to camelCase for CSS properties
                const camelProperty = property.replace(/-([a-z])/g, (g) =>
                  g[1].toUpperCase()
                );
                htmlEl.style.setProperty(property, value, "important");
              }
            });
          }
        });
      }, 100); // Small delay to ensure DOM is ready

      return () => clearTimeout(timeoutId);
    }
    return undefined;
  }, [briefData]);

  const handleScroll = (e: React.UIEvent<HTMLDivElement>) => {
    const element = e.currentTarget;
    const scrolledToBottom =
      element.scrollHeight - element.scrollTop <= element.clientHeight + 50;

    if (scrolledToBottom && !hasScrolledToBottom) {
      setHasScrolledToBottom(true);
    }
  };

  const handleContinue = () => {
    navigate("/giba/login");
  };

  const renderContent = () => {
    if (!briefData) {
      return (
        <div className="text-center py-12 text-gray-400">
          <p>Không có nội dung để hiển thị</p>
        </div>
      );
    }

    if (briefData.isPdf && briefData.fullUrl) {
      const viewerUrl = `https://docs.google.com/viewer?url=${encodeURIComponent(
        briefData.fullUrl
      )}&embedded=true`;

      return (
        <div className="bg-white rounded-lg overflow-hidden w-full">
          <div
            className="relative w-full"
            style={{ height: "calc(100vh - 200px)" }}
          >
            <iframe
              src={viewerUrl}
              className="w-full h-full border-0"
              title="App Brief PDF"
              loading="lazy"
              onLoad={() => setHasScrolledToBottom(true)}
            />
          </div>
        </div>
      );
    }

    return (
      <div className="bg-white rounded-lg p-6">
        <style>{`
          .app-brief-content {
            word-wrap: break-word;
            overflow-wrap: break-word;
            color: #1f2937;
          }
          .app-brief-content * {
            max-width: 100%;
            box-sizing: border-box;
          }
          /* Don't reset styles - preserve all original formatting */
          .app-brief-content p,
          .app-brief-content div,
          .app-brief-content span,
          .app-brief-content h1,
          .app-brief-content h2,
          .app-brief-content h3,
          .app-brief-content h4,
          .app-brief-content h5,
          .app-brief-content h6,
          .app-brief-content center {
            /* Preserve original margins, padding, and styles from HTML */
            margin: revert;
            padding: revert;
            text-align: revert;
            font-size: revert;
            font-weight: revert;
            line-height: revert;
            color: revert;
            font-family: revert;
          }
          /* Preserve center tag alignment */
          .app-brief-content center {
            text-align: center !important;
            display: block;
          }
          /* Preserve align attribute */
          .app-brief-content [align="center"] {
            text-align: center !important;
          }
          .app-brief-content [align="left"] {
            text-align: left !important;
          }
          .app-brief-content [align="right"] {
            text-align: right !important;
          }
          .app-brief-content [align="justify"] {
            text-align: justify !important;
          }
          /* Handle Quill editor alignment classes */
          .app-brief-content .ql-align-center {
            text-align: center !important;
          }
          .app-brief-content .ql-align-left {
            text-align: left !important;
          }
          .app-brief-content .ql-align-right {
            text-align: right !important;
          }
          .app-brief-content .ql-align-justify {
            text-align: justify !important;
          }
          /* Force preserve text-align from inline styles with higher specificity */
          .app-brief-content [style*="text-align: center"],
          .app-brief-content [style*="text-align:center"],
          .app-brief-content [style*="text-align: center;"],
          .app-brief-content [style*="text-align:center;"],
          .app-brief-content [style*="text-align: center "],
          .app-brief-content [style*="text-align:center "] {
            text-align: center !important;
          }
          .app-brief-content [style*="text-align: left"],
          .app-brief-content [style*="text-align:left"],
          .app-brief-content [style*="text-align: left;"],
          .app-brief-content [style*="text-align:left;"],
          .app-brief-content [style*="text-align: left "],
          .app-brief-content [style*="text-align:left "] {
            text-align: left !important;
          }
          .app-brief-content [style*="text-align: right"],
          .app-brief-content [style*="text-align:right"],
          .app-brief-content [style*="text-align: right;"],
          .app-brief-content [style*="text-align:right;"],
          .app-brief-content [style*="text-align: right "],
          .app-brief-content [style*="text-align:right "] {
            text-align: right !important;
          }
          .app-brief-content [style*="text-align: justify"],
          .app-brief-content [style*="text-align:justify"],
          .app-brief-content [style*="text-align: justify;"],
          .app-brief-content [style*="text-align:justify;"],
          .app-brief-content [style*="text-align: justify "],
          .app-brief-content [style*="text-align:justify "] {
            text-align: justify !important;
          }
          /* Preserve all inline styles - don't override them */
          .app-brief-content [style] {
            /* Inline styles will automatically take precedence */
          }

          /* Preserve font-size from inline styles */
          .app-brief-content [style*="font-size"],
          .app-brief-content [style*="fontSize"] {
            /* Let inline styles work */
          }

          /* Preserve font-weight from inline styles */
          .app-brief-content [style*="font-weight"],
          .app-brief-content [style*="fontWeight"] {
            /* Let inline styles work */
          }

          /* Preserve color from inline styles */
          .app-brief-content [style*="color"] {
            /* Let inline styles work */
          }

          /* Preserve strong and em tags */
          .app-brief-content strong,
          .app-brief-content b {
            font-weight: bold;
          }

          .app-brief-content em,
          .app-brief-content i {
            font-style: italic;
          }
        `}</style>
        <div
          ref={contentRef}
          className="app-brief-content"
          dangerouslySetInnerHTML={{ __html: briefData.content }}
        />
      </div>
    );
  };

  return (
    <Page className="bg-black min-h-screen">
      <div className="w-full px-4 py-6 mt-[60px] pb-32">
        {loading ? (
          <div className="flex justify-center items-center min-h-screen">
            <LoadingGiba size="lg" text="Đang tải..." />
          </div>
        ) : (
          <>
            <div
              className="bg-gray-900 rounded-lg mb-6"
              onScroll={handleScroll}
              style={{
                maxHeight: briefData?.isPdf ? "auto" : "calc(100vh - 200px)",
                overflowY: briefData?.isPdf ? "visible" : "auto",
              }}
            >
              {renderContent()}
            </div>
          </>
        )}
      </div>

      {!loading && (
        <div className="fixed bottom-0 left-0 right-0 bg-black border-t border-gray-700 px-4 py-4">
          <button
            onClick={handleContinue}
            disabled={!hasScrolledToBottom}
            className={`w-full py-3 rounded-lg font-bold text-base transition-all duration-200 shadow-lg ${
              hasScrolledToBottom
                ? "bg-white text-black hover:bg-gray-200"
                : "bg-gray-700 text-gray-400 cursor-not-allowed"
            }`}
          >
            Tiếp tục
          </button>
        </div>
      )}
    </Page>
  );
};

export default AppBriefGiba;
