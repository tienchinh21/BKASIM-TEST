import React, { useState, useEffect } from "react";
import { Drawer } from "antd";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import { toast } from "react-toastify";
import LoadingGiba from "./LoadingGiba";
import dfData from "../common/DefaultConfig.json";

interface PageData {
  id: string;
  title: string;
  contentType: "TEXT" | "FILE";
  type: string;
  content: string;
  groupId: string | null;
  sortOrder: number;
  totalPages: number;
  currentPage: number;
}

interface GroupRulesDrawerProps {
  visible: boolean;
  onClose: () => void;
  groupId: string;
}

const GroupRulesDrawer: React.FC<GroupRulesDrawerProps> = ({
  visible,
  onClose,
  groupId,
}) => {
  const userToken = useRecoilValue(token);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [pageData, setPageData] = useState<PageData | null>(null);
  const [isLoadingPage, setIsLoadingPage] = useState(false);

  useEffect(() => {
    if (visible && groupId) {
      setCurrentPage(1);
      loadPageData(1);
    }
  }, [visible, groupId]);

  useEffect(() => {
    if (visible) {
      loadPageData(currentPage);
    }
  }, [currentPage, visible]);

  const loadPageData = async (page: number) => {
    try {
      setIsLoadingPage(true);
      const response = await axios.get(
        `${dfData.domain}/api/BehaviorRuleV2/pageInfo`,
        {
          params: {
            page: page,
            type: "GROUP",
            groupid: groupId,
          },
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.success && response.data.data) {
        const data = response.data.data;
        setPageData(data);
        setTotalPages(data.totalPages);
      } else {
        toast.error("Không có nội dung để hiển thị");
      }
    } catch (error) {
      console.error("Error loading page:", error);
      toast.error("Không thể tải nội dung. Vui lòng thử lại!");
    } finally {
      setIsLoadingPage(false);
    }
  };

  const handleNext = () => {
    if (currentPage < totalPages) {
      setCurrentPage((prev) => prev + 1);
    }
  };

  const handlePrevious = () => {
    if (currentPage > 1) {
      setCurrentPage((prev) => prev - 1);
    }
  };

  const renderProgressBar = () => {
    if (totalPages === 0) return null;

    const percentage = Math.round((currentPage / totalPages) * 100);

    return (
      <div className="mb-4">
        <div className="flex justify-between items-center mb-2">
          <span className="text-gray-700 text-sm font-medium">
            Quy tắc nhóm
          </span>
          <span className="text-gray-500 text-sm">
            {currentPage}/{totalPages}
          </span>
        </div>
        <div className="w-full bg-gray-200 rounded-full h-2 overflow-hidden">
          <div
            className="bg-blue-600 h-2 rounded-full transition-all duration-300"
            style={{ width: `${percentage}%` }}
          />
        </div>
        <div className="text-center mt-2">
          <span className="text-gray-500 text-xs">
            {percentage}% Hoàn thành
          </span>
        </div>
      </div>
    );
  };

  const renderContent = () => {
    if (!pageData) {
      return (
        <div className="text-center py-12 text-gray-400">
          <p>Không có nội dung để hiển thị</p>
        </div>
      );
    }

    if (pageData.contentType === "TEXT") {
      return (
        <div className="bg-white rounded-lg p-6">
          <h2 className="text-xl font-bold mb-4 text-gray-900">
            {pageData.title}
          </h2>
          <div
            className="prose prose-sm max-w-none text-gray-800"
            dangerouslySetInnerHTML={{ __html: pageData.content }}
          />
        </div>
      );
    }

    if (pageData.contentType === "FILE") {
      const fullUrl = pageData.content.startsWith("http")
        ? pageData.content
        : `${dfData.domain}${pageData.content}`;

      const isImage = /\.(jpg|jpeg|png|gif|webp|bmp|svg)$/i.test(fullUrl);
      const isPDF = /\.pdf$/i.test(fullUrl);

      if (isImage) {
        return (
          <div className="bg-white rounded-lg overflow-hidden w-full">
            <div className="bg-gray-100 px-4 py-3 border-b border-gray-200">
              <h2 className="text-lg font-bold text-gray-900">
                {pageData.title}
              </h2>
            </div>
            <div className="relative w-full p-4">
              <img
                src={fullUrl}
                alt={pageData.title}
                className="w-full h-auto rounded"
                loading="lazy"
              />
            </div>
          </div>
        );
      }

      const viewerUrl = `https://docs.google.com/viewer?url=${encodeURIComponent(
        fullUrl
      )}&embedded=true`;

      return (
        <div className="bg-white rounded-lg overflow-hidden w-full">
          <div className="bg-gray-100 px-4 py-3 border-b border-gray-200">
            <h2 className="text-lg font-bold text-gray-900">
              {pageData.title}
            </h2>
          </div>
          <div className="relative w-full" style={{ height: "60vh" }}>
            <iframe
              src={viewerUrl}
              className="w-full h-full border-0"
              title={pageData.title}
              loading="lazy"
            />
          </div>
        </div>
      );
    }

    return null;
  };

  return (
    <Drawer
      title="Quy tắc ứng xử của nhóm"
      placement="bottom"
      height="90vh"
      open={visible}
      onClose={onClose}
      style={{ borderRadius: "15px 15px 0 0" }}
      styles={{
        body: { padding: "20px", background: "#f5f5f5" },
      }}
      maskClosable={true}
      closable={true}
      footer={
        <div className="flex gap-3 p-4 bg-white border-t">
          <button
            onClick={handlePrevious}
            disabled={currentPage === 1}
            className={`flex items-center justify-center w-14 h-12 rounded-lg transition-all duration-200 ${
              currentPage === 1
                ? "bg-gray-200 text-gray-400 cursor-not-allowed"
                : "bg-gray-800 text-white hover:bg-gray-700"
            }`}
          >
            <svg
              className="w-6 h-6"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M15 19l-7-7 7-7"
              />
            </svg>
          </button>

          {currentPage === totalPages ? (
            <button
              onClick={onClose}
              className="flex-1 py-3 rounded-lg font-bold text-base transition-all duration-200 bg-blue-600 text-white hover:bg-blue-700"
            >
              Đóng
            </button>
          ) : (
            <button
              onClick={handleNext}
              disabled={currentPage === totalPages}
              className={`flex-1 py-3 rounded-lg font-bold text-base transition-all duration-200 ${
                currentPage === totalPages
                  ? "bg-gray-200 text-gray-400 cursor-not-allowed"
                  : "bg-blue-600 text-white hover:bg-blue-700"
              }`}
            >
              Tiếp theo
            </button>
          )}
        </div>
      }
    >
      <div className="flex flex-col h-full">
        {renderProgressBar()}

        <div className="flex-1 overflow-y-auto bg-gray-50 rounded-lg p-4 mb-4">
          {isLoadingPage ? (
            <div className="flex justify-center items-center py-12">
              <LoadingGiba size="md" text="Đang tải nội dung..." />
            </div>
          ) : (
            renderContent()
          )}
        </div>
      </div>
    </Drawer>
  );
};

export default GroupRulesDrawer;
